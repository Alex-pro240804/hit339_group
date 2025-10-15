using EasyGames.Data;
using EasyGames.Helpers;
using EasyGames.Models;
using EasyGames.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EasyGames.Controllers
{
    [Authorize(Roles = "Owner")]
    public class UsersAdminController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersAdminController(
            ApplicationDbContext db,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // Owner: list users
        public IActionResult Index()
        {
            ViewBag.AllRoles = new[] { "User", "Owner" };
            return View(_userManager.Users.ToList());
        }

        // Owner: per-user sales history + tier (Step 3)
        public async Task<IActionResult> History(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var orders = await _db.Orders
                .Where(o => o.UserId == id)
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            var revenue = orders.Sum(o => o.Items.Sum(i => i.UnitPrice * i.Quantity));
            var profit = orders.Sum(o => o.Items.Sum(i => (i.UnitPrice - i.UnitBuyPrice) * i.Quantity));
            var tier = TierHelper.FromProfit(profit);

            var vm = new UserSalesHistoryVM
            {
                UserId = id,
                Email = user.Email,
                Orders = orders,
                Revenue = revenue,
                Profit = profit,
                Tier = tier
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetRole(string id, string role)
        {
            // 1) Ensure role exists
            if (!await _roleManager.RoleExistsAsync(role))
            {
                var createRole = await _roleManager.CreateAsync(new IdentityRole(role));
                if (!createRole.Succeeded)
                {
                    TempData["Error"] = $"Failed to create role '{role}': " +
                        string.Join("; ", createRole.Errors.Select(e => e.Description));
                    return RedirectToAction(nameof(Index));
                }
            }

            // 2) Find user
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) { TempData["Error"] = "User not found."; return RedirectToAction(nameof(Index)); }

            // 3) Remove old roles, add new
            var currentRoles = await _userManager.GetRolesAsync(user);
            var removeRes = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeRes.Succeeded)
            {
                TempData["Error"] = "Failed removing current roles: " +
                    string.Join("; ", removeRes.Errors.Select(e => e.Description));
                return RedirectToAction(nameof(Index));
            }

            var addRes = await _userManager.AddToRoleAsync(user, role);
            if (!addRes.Succeeded)
            {
                TempData["Error"] = "Failed adding role: " +
                    string.Join("; ", addRes.Errors.Select(e => e.Description));
                return RedirectToAction(nameof(Index));
            }

            TempData["Ok"] = $"Role for {user.Email} set to {role}. " +
                             (User.Identity?.Name == user.UserName
                                ? "Sign out and sign in to refresh your access."
                                : "");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                var res = await _userManager.DeleteAsync(user);
                TempData[res.Succeeded ? "Ok" : "Error"] = res.Succeeded
                    ? $"Deleted {user.Email}."
                    : string.Join("; ", res.Errors.Select(e => e.Description));
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
