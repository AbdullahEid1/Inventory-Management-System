using Inventory.Data;
using Inventory.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DinkToPdf;
using DinkToPdf.Contracts;
using OfficeOpenXml;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using Models;
using System.Drawing.Printing;
using System.Linq;
using System.Threading.Tasks;

namespace Inventory.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConverter _pdfConverter;

        public ProductController(ApplicationDbContext context, IConverter pdfConverter)
        {
            _context = context;
            _pdfConverter = pdfConverter;
        }

        // Utility function to load categories and suppliers
        private void LoadCategoriesAndSuppliers()
        {
            ViewBag.Categories = _context.Category.ToList();
            ViewBag.Suppliers = _context.Supplier.ToList();
        }

       
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

        
        //Get: Product/Search
        public IActionResult Search(string keyword, int page = 1, int pageSize = 6)
        {
            var products = string.IsNullOrEmpty(keyword)
                ? _context.Product
                    .Include(p => p.Category)
                    .Include(p => p.Supplier)
                    .AsQueryable()
                : _context.Product
                    .Where(p => p.ProductName.StartsWith(keyword))
                    .Include(p => p.Category)
                    .Include(p => p.Supplier)
                    .AsQueryable();

            ViewData["Keyword"] = keyword;

            // Load all categories to display in dropdown for filteration
            ViewBag.Categories = new SelectList(_context.Category.ToList(), "CategoryID", "CategoryName");

            // Pagination
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

            return View("Index", model);
        }


        //Get: Product/AddQuantity
        public IActionResult AddQuantity(int id, int Quantity)
        {

           var product = _context.Product.Find(id);
            if (product != null)
            {
                product.StockQuantity += Quantity;
                _context.SaveChanges();
                TempData["success"] = "Stock quantity updated successfully!";
            }
            return RedirectToAction("Index", "Home");
        }


        //Post: Product/GetProduct
        [HttpPost]
        public IActionResult GetProduct(int id)
        {
            var product = _context.Product.Include(p => p.Category).FirstOrDefault(p => p.ProductID == id);

            if (product != null)
            {
                return PartialView("_ProductDetailsPartial", product);
            }
            else
            {
                return Json(new { success = false, message = "Product not found" });
            }
        }


        //Get: Product/ExportProduct
        public IActionResult ExportProduct(int id, int quantity)
        {

            var product = _context.Product.Find(id);
            if (product != null)
            {
                product.StockQuantity -= quantity;
                _context.SaveChanges();
                TempData["success"] = $"Product Exported With {quantity}";
            }
            else
            {
                TempData["fail"] = "Product not found";
            }
            return RedirectToAction("Index", "Home");
        }


        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult GeneratePDFReport()
        {
            var products = _context.Product
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .ToList();

            string htmlContent = "<h1>Inventory Report</h1><table border='1'><thead><tr><th>Product Name</th><th>Stock Quantity</th><th>Price</th><th>Category</th><th>Supplier</th></tr></thead><tbody>";

            foreach (var product in products)
            {
                htmlContent += $"<tr><td>{product.ProductName}</td><td>{product.StockQuantity}</td><td>{product.Price}</td><td>{product.Category.CategoryName}</td><td>{product.Supplier.SupplierName}</td></tr>";
            }

            htmlContent += "</tbody></table>";

            var pdfDoc = new HtmlToPdfDocument()
            {
                GlobalSettings = {
                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Portrait,
                    PaperSize = DinkToPdf.PaperKind.A4,
                    Margins = new MarginSettings { Top = 10 },
                    DocumentTitle = "Inventory Report"
                },
                Objects = {
                    new ObjectSettings()
                    {
                        PagesCount = true,
                        HtmlContent = htmlContent,
                        WebSettings = { DefaultEncoding = "utf-8" },
                        FooterSettings = { FontSize = 8, Right = "Page [page] of [toPage]" }
                    }
                }
            };

            var pdf = _pdfConverter.Convert(pdfDoc);
            return File(pdf, "application/pdf", "InventoryReport.pdf");
        }

    }
}
