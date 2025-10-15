using EasyGames.Data;
using EasyGames.Helpers;                 
using EasyGames.Models;                  
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EasyGames.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public OrdersController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        private string GetUserId() => _userManager.GetUserId(User)!;

        // GET: /Orders/My
        // Shows the signed-in user's orders + totals + computed tier
        public async Task<IActionResult> My()
        {
            var uid = GetUserId();

            var orders = await _db.Orders
                .Where(o => o.UserId == uid)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            // compute totals from order items (sell - buy) for profit
            var revenue = orders.Sum(o => o.Items.Sum(i => i.UnitPrice * i.Quantity));
            var profit = orders.Sum(o => o.Items.Sum(i => (i.UnitPrice - i.UnitBuyPrice) * i.Quantity));
            var tier = TierHelper.FromProfit(profit);

            ViewBag.Revenue = revenue;
            ViewBag.Profit = profit;
            ViewBag.Tier = tier;

            return View(orders);
        }

        // GET: /Orders/Details/{id}
        // Shows a single order (only if it belongs to the current user)
        public async Task<IActionResult> Details(int id)
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
