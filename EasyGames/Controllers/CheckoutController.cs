using EasyGames.Data;
using EasyGames.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EasyGames.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public CheckoutController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        private string GetUserId() => _userManager.GetUserId(User)!;

        // GET: /Checkout
        public async Task<IActionResult> Index()
        {
            var uid = GetUserId();
            var items = await _db.CartItems
                                 .Include(c => c.Product)
                                 .Where(c => c.UserId == uid)
                                 .ToListAsync();
            return View(items);
        }

        // POST: /Checkout/Confirm
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm()
        {
            var uid = GetUserId();
            var cart = await _db.CartItems
                                .Include(c => c.Product)
                                .Where(c => c.UserId == uid)
                                .ToListAsync();

            if (!cart.Any())
                return RedirectToAction("Index"); // nothing to buy

            // Ensure enough stock
            foreach (var line in cart)
            {
                if (line.Product == null || line.Quantity > line.Product.StockQty)
                {
                    TempData["Error"] = $"Not enough stock for {line.Product?.Name}.";
                    return RedirectToAction("Index");
                }
            }

            using var tx = await _db.Database.BeginTransactionAsync();

            var order = new Order
            {
                UserId = uid,
                CreatedAt = DateTime.UtcNow,
                Status = OrderStatus.Paid
            };

            decimal total = 0m;

            foreach (var line in cart)
            {
                var sell = line.Product!.Price;      // selling price at checkout
                var buy = line.Product!.BuyPrice;   //  snapshot cost at checkout

                order.Items.Add(new OrderItem
                {
                    ProductId = line.ProductId,
                    UnitPrice = sell,
                    UnitBuyPrice = buy,               //  store cost
                    Quantity = line.Quantity
                });

                total += sell * line.Quantity;

                // reduce web inventory
                line.Product.StockQty -= line.Quantity;
            }

            order.Total = total;

            _db.Orders.Add(order);
            _db.CartItems.RemoveRange(cart);

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            return RedirectToAction("Success", new { id = order.Id });
        }

        // GET: /Checkout/Success/{id}
        public async Task<IActionResult> Success(int id)
        {
            var uid = GetUserId();
            var order = await _db.Orders
                                 .Include(o => o.Items)
                                 .ThenInclude(i => i.Product)
                                 .FirstOrDefaultAsync(o => o.Id == id && o.UserId == uid);
            if (order == null) return NotFound();
            return View(order);
        }
    }
}
