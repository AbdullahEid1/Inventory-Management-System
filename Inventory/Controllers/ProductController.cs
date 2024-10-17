using ECommerce.Model.ViewModels;
using Inventory.Data;
using Inventory.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Drawing.Printing;
using System.Linq;
using System.Threading.Tasks;

namespace Inventory.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Utility function to load categories and suppliers
        private void LoadCategoriesAndSuppliers()
        {
            ViewBag.Categories = _context.Category.ToList();
            ViewBag.Suppliers = _context.Supplier.ToList();
        }

        // GET: Product
        /*public async Task<IActionResult> Index()
        {
            var products = await _context.Product
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .ToListAsync();
            return View(products);
        }*/
       
        public async Task<IActionResult> Index(int? categoryId, int page = 1, int pageSize = 6)
        {
            // Load all categories to display in dropdown
            ViewBag.Categories = new SelectList(await _context.Category.ToListAsync(), "CategoryID", "CategoryName");

            // Fetch products, optionally filter by categoryId if provided
            var products = _context.Product
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .AsQueryable();

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                products = products.Where(p => p.CategoryID == categoryId); ;
            }

            var totalProducts = products.Count();

            products = products
                .Skip((page - 1) * pageSize)
                .Take(pageSize);
                


            var totalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);

            var model = new ProductListViewModel
            {
                Products = products.ToList(),
                CurrentPage = page,
                TotalPages = totalPages
            };


            return View(model);
        }


        // GET: Product/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .FirstOrDefaultAsync(p => p.ProductID == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Product/Create
        public IActionResult Create()
        {
            LoadCategoriesAndSuppliers();
            return View();
        }

        // POST: Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductID,ProductName,CategoryID,SupplierID,Price,StockQuantity,LowStockThreshold")] Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                TempData["success"] = "Product added successfully!";
                return RedirectToAction(nameof(Index));
            }
            LoadCategoriesAndSuppliers();
            return View(product);
        }

        // GET: Product/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound("Product ID is missing.");
            }

            var product = await _context.Product
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .FirstOrDefaultAsync(p => p.ProductID == id);

            if (product == null)
            {
                return NotFound("Product not found.");
            }

            LoadCategoriesAndSuppliers();
            return View(product);
        }

        // POST: Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product updateProduct)
        {
            if (id != updateProduct.ProductID)
            {
                return NotFound("Mismatching Product ID.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var product = await _context.Product.FindAsync(id);

                    if (product == null)
                    {
                        return NotFound("Product not found.");
                    }

                    product.ProductName = updateProduct.ProductName;
                    product.Price = updateProduct.Price;
                    product.CategoryID = updateProduct.CategoryID;
                    product.SupplierID = updateProduct.SupplierID;
                    product.StockQuantity = updateProduct.StockQuantity;
                    product.LowStockThreshold = updateProduct.LowStockThreshold;

                    await _context.SaveChangesAsync();
                    TempData["success"] = "Product updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(updateProduct.ProductID))
                    {
                        return NotFound("Concurrency issue: Product no longer exists.");
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            LoadCategoriesAndSuppliers();
            return View(updateProduct);
        }

        // GET: Product/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .FirstOrDefaultAsync(p => p.ProductID == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Product.FindAsync(id);
            if (product != null)
            {
                _context.Product.Remove(product);
                TempData["success"] = "Product deleted successfully!";
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Product.Any(e => e.ProductID == id);
        }

        public IActionResult Search(string keyword, int page = 1, int pageSize = 6)
        {
            ViewBag.Categories = new SelectList( _context.Category.ToList(), "CategoryID", "CategoryName");

            var products = _context.Product
                .Where(p => p.ProductName.StartsWith(keyword))
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .AsQueryable();

            products = products
               .Skip((page - 1) * pageSize)
               .Take(pageSize);


            var totalProducts = products.Count();

            var totalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);

            var model = new ProductListViewModel
            {
                Products = products.ToList(),
                CurrentPage = page,
                TotalPages = totalPages
            };

            return View("Index", model);
        }
    }
}