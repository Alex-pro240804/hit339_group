using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EasyGames.Data;
using EasyGames.Models;
using Microsoft.AspNetCore.Authorization;

namespace EasyGames.Controllers
{
    [Authorize(Roles = "Owner")]
    public class ProductsAdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsAdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ProductsAdmin
        public async Task<IActionResult> Index()
        {
            // keep it tidy by ordering by Name
            var products = await _context.Products
                .OrderBy(p => p.Name)
                .ToListAsync();

            return View(products);
        }

        // GET: ProductsAdmin/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null) return NotFound();

            return View(product);
        }

        // GET: ProductsAdmin/Create
        public IActionResult Create() => View();

        // POST: ProductsAdmin/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            // Overposting-safe: bind only allowed fields (timestamps set server-side)
            [Bind("Name,Category,Description,Source,BuyPrice,Price,StockQty,ImageUrl")]
            Product product)
        {
            // Server-side validation for finance
            if (product.BuyPrice < 0 || product.Price < 0)
                ModelState.AddModelError(string.Empty, "Prices must be non-negative.");
            if (product.BuyPrice > product.Price)
                ModelState.AddModelError(nameof(product.Price), "Sell price should be ≥ buy price.");

            if (!ModelState.IsValid) return View(product);

            product.CreatedAt = DateTime.UtcNow;
            product.UpdatedAt = null;

            _context.Add(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: ProductsAdmin/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            return View(product);
        }

        // POST: ProductsAdmin/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            // Overposting-safe: do NOT bind CreatedAt/UpdatedAt/Id from client except id route
            [Bind("Name,Category,Description,Source,BuyPrice,Price,StockQty,ImageUrl")]
            Product incoming)
        {
            // Basic route/body consistency (we only use id from route)
            var existing = await _context.Products.FindAsync(id);
            if (existing == null) return NotFound();

            // Finance validation
            if (incoming.BuyPrice < 0 || incoming.Price < 0)
                ModelState.AddModelError(string.Empty, "Prices must be non-negative.");
            if (incoming.BuyPrice > incoming.Price)
                ModelState.AddModelError(nameof(incoming.Price), "Sell price should be ≥ buy price.");

            if (!ModelState.IsValid) return View(existing);

            try
            {
                // Update allowed fields only
                existing.Name = incoming.Name;
                existing.Category = incoming.Category;
                existing.Description = incoming.Description;
                existing.Source = incoming.Source;
                existing.BuyPrice = incoming.BuyPrice;
                existing.Price = incoming.Price;
                existing.StockQty = incoming.StockQty;
                existing.ImageUrl = incoming.ImageUrl;

                existing.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id)) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: ProductsAdmin/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null) return NotFound();

            return View(product);
        }

        // POST: ProductsAdmin/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id) =>
            _context.Products.Any(e => e.Id == id);
    }
}
