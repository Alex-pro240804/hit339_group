using EasyGames.Data;
using EasyGames.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EasyGames.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _db;
        public ProductsController(ApplicationDbContext db) => _db = db;

        // /Products?category=Game&search=chess
        public async Task<IActionResult> Index(ProductCategory? category, string? search)
        {
            var q = _db.Products.AsQueryable();

            if (category.HasValue)
                q = q.Where(p => p.Category == category);

            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(p => p.Name.Contains(search) || (p.Description != null && p.Description.Contains(search)));

            var items = await q
                .OrderBy(p => p.Name)
                .ToListAsync();

            ViewBag.SelectedCategory = category;
            ViewBag.Search = search;
            return View(items);
        }

        // /Products/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var item = await _db.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (item == null) return NotFound();
            return View(item);
        }
    }
}
