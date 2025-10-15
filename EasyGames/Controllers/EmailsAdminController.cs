using EasyGames.Data;
using EasyGames.Helpers;                    // TierHelper
using EasyGames.Models.ViewModels;         // EmailBlastVM
using EasyGames.Services;                  // IEmailSender
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EasyGames.Controllers
{
    [Authorize(Roles = "Owner")]
    public class EmailsAdminController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _users;
        private readonly IEmailSender _email;

        public EmailsAdminController(
            ApplicationDbContext db,
            UserManager<IdentityUser> users,
            IEmailSender email)
        {
            _db = db;
            _users = users;
            _email = email;
        }

        [HttpGet]
        public IActionResult Compose() => View(new EmailBlastVM());

        // Build recipients and show preview
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Preview(EmailBlastVM vm)
        {
            if (!ModelState.IsValid) return View("Compose", vm);

            var allUsers = _users.Users.ToList(); // Identity users

            // ✅ FIXED: compute profit per user via OrderItems (no nested aggregates)
            var profits = await _db.OrderItems
                .Where(oi => oi.Order != null)
                .GroupBy(oi => oi.Order!.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    Profit = g.Sum(oi => (oi.UnitPrice - oi.UnitBuyPrice) * oi.Quantity)
                })
                .ToListAsync();

            string PickTier(decimal p) => TierHelper.FromProfit(p);

            // Left-join profits to all users, compute tier
            var joined = from u in allUsers
                         join p in profits on u.Id equals p.UserId into gp
                         from p in gp.DefaultIfEmpty()
                         let profit = p?.Profit ?? 0m
                         let tier = PickTier(profit)
                         select new { u.Email, u.Id, Profit = profit, Tier = tier };

            IEnumerable<string> recipients = vm.Target switch
            {
                "Bronze" => joined.Where(x => x.Tier == "Bronze").Select(x => x.Email!),
                "Silver" => joined.Where(x => x.Tier == "Silver").Select(x => x.Email!),
                "Gold" => joined.Where(x => x.Tier == "Gold").Select(x => x.Email!),
                "Platinum" => joined.Where(x => x.Tier == "Platinum").Select(x => x.Email!),
                _ => joined.Select(x => x.Email!)
            };

            vm.Recipients = recipients.Where(e => !string.IsNullOrWhiteSpace(e))
                                      .Distinct()
                                      .ToList();
            vm.RecipientCount = vm.Recipients.Count;

            TempData["Info"] = $"Preview built: {vm.RecipientCount} recipient(s).";
            return View("Compose", vm);
        }

        // Send to recipients
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(EmailBlastVM vm)
        {
            if (!ModelState.IsValid) return View("Compose", vm);

            // Rebuild recipients using the same logic as Preview
            var allUsers = _users.Users.ToList();

            var profits = await _db.OrderItems
                .Where(oi => oi.Order != null)
                .GroupBy(oi => oi.Order!.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    Profit = g.Sum(oi => (oi.UnitPrice - oi.UnitBuyPrice) * oi.Quantity)
                })
                .ToListAsync();

            string PickTier(decimal p) => TierHelper.FromProfit(p);

            var joined = from u in allUsers
                         join p in profits on u.Id equals p.UserId into gp
                         from p in gp.DefaultIfEmpty()
                         let profit = p?.Profit ?? 0m
                         let tier = PickTier(profit)
                         select new { u.Email, Tier = tier };

            IEnumerable<string> recipients = vm.Target switch
            {
                "Bronze" => joined.Where(x => x.Tier == "Bronze").Select(x => x.Email!),
                "Silver" => joined.Where(x => x.Tier == "Silver").Select(x => x.Email!),
                "Gold" => joined.Where(x => x.Tier == "Gold").Select(x => x.Email!),
                "Platinum" => joined.Where(x => x.Tier == "Platinum").Select(x => x.Email!),
                _ => joined.Select(x => x.Email!)
            };

            var list = recipients.Where(e => !string.IsNullOrWhiteSpace(e))
                                 .Distinct()
                                 .ToList();

            if (list.Count == 0)
            {
                TempData["Error"] = "No recipients match the selected audience.";
                return View("Compose", vm);
            }

            foreach (var to in list)
            {
                await _email.SendAsync(to, vm.Subject, vm.Body);
            }

            TempData["Ok"] = $"Sent to {list.Count} recipient(s).";
            return View("Compose", new EmailBlastVM { Target = vm.Target });
        }
    }
}
