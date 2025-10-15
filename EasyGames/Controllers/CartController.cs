using EasyGames.Data;
using EasyGames.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EasyGames.Controllers
{
    [Authorize] // only logged-in users can access the cart
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public CartController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        private string GetUserId() => _userManager.GetUserId(User)!;

        // GET: /Cart
        public async Task<IActionResult> Index()
        {
            var uid = GetUserId();
            var items = await _db.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserId == uid)
                .ToListAsync();

            return View(items);
        }

        // POST: /Cart/Add/5?qty=1
        [HttpPost]
        public async Task<IActionResult> Add(int id, int qty = 1)
        {
            var uid = GetUserId();
            var item = await _db.CartItems.FirstOrDefaultAsync(c => c.UserId == uid && c.ProductId == id);
            if (item == null)
            {
                item = new CartItem { UserId = uid, ProductId = id, Quantity = qty };
                _db.CartItems.Add(item);
            }
            else
            {
                item.Quantity += qty;
            }
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // POST: /Cart/Update/5
        [HttpPost]
        public async Task<IActionResult> Update(int id, int qty)
        {
            var uid = GetUserId();
            var item = await _db.CartItems.FirstOrDefaultAsync(c => c.Id == id && c.UserId == uid);
            if (item != null)
            {
                item.Quantity = qty;
                await _db.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        // POST: /Cart/Remove/5
        [HttpPost]
        public async Task<IActionResult> Remove(int id)
        {
            var uid = GetUserId();
            var item = await _db.CartItems.FirstOrDefaultAsync(c => c.Id == id && c.UserId == uid);
            if (item != null)
            {
                _db.CartItems.Remove(item);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}
