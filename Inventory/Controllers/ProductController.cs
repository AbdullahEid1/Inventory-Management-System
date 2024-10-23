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
using OfficeOpenXml.Style;
using System.Drawing;
using System.Composition;

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


        //Get: Product/ProductExists
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


        //Get: Product/GeneratePDFReport
        [Authorize(Roles = "Admin")]
        public IActionResult GeneratePDFReport()
        {
            var products = _context.Product
                .Include(p => p.Category)
                .Include(p => p.Supplier)
            .ToList();


            // ---------------------- Enhanced PDF report ----------------------------- //

            var tableRows = "";
            var totalStockValue = products.Sum(p => p.Price * p.StockQuantity).ToString("F2");

            foreach (var product in products)
            {
                string rowColor = "";
                var lowStockThreshold = product.LowStockThreshold;
                string stockDisplay = product.StockQuantity.ToString();

                // Apply conditions for low stock and out of stock
                if (product.StockQuantity == 0)
                {
                    stockDisplay += " (Out of Stock)";
                    rowColor = "style='background-color: red; color: white;'";
                }
                else if (product.StockQuantity < lowStockThreshold)
                {
                    stockDisplay += " (Low Stock)";
                    rowColor = "style='background-color: orange;'";
                }

                // Append the product row to the tableRows
                tableRows += $@"
                <tr {rowColor}>
                    <td>{product.ProductName}</td>
                    <td>{product.Category.CategoryName}</td>
                    <td>{product.Supplier.SupplierName}</td>
                    <td>{product.Price}</td>
                    <td>{stockDisplay}</td>
                </tr>";
            }

            // After generating the rows, you can create the rest of the HTML as previously shown
            var htmlContent = $@"
            <!DOCTYPE html>
            <html lang='en'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>Inventory Report</title>
                <style>
                    body {{
                        font-family: Arial, sans-serif;
                        margin: 20px;
                    }}
                    h1 {{
                        text-align: center;
                        color: #333;
                    }}
                    table {{
                        width: 100%;
                        border-collapse: collapse;
                        margin-top: 20px;
                        box-shadow: 0 2px 3px rgba(0,0,0,0.1);
                    }}
                    table, th, td {{
                        border: 1px solid #ddd;
                    }}
                    th {{
                        background-color: #4CAF50;
                        color: white;
                        padding: 10px;
                    }}
                    td {{
                        padding: 10px;
                        text-align: center;
                    }}
                    .report-date {{
                        text-align: left;
                        margin-top: 20px;
                        color: #555;
                    }}
                    .footer {{
                        text-align: center;
                        margin-top: 40px;
                        font-size: 12px;
                        color: #999;
                    }}
                    .total-row {{
                        background-color: #f2f2f2;
                        font-weight: bold;
                    }}
                </style>
            </head>
            <body>
                <h1>Inventory Report</h1>
                <div class='report-date'>
                    Date: {DateTime.Now.ToString("yyyy-MM-dd")} <br>
                    Time: {DateTime.Now.ToString("HH:mm:ss")}
                </div>
                <table>
                    <thead>
                        <tr>
                            <th>Product Name</th>
                            <th>Category</th>
                            <th>Supplier</th>
                            <th>Price (EGP)</th>
                            <th>Stock Quantity</th>
                        </tr>
                    </thead>
                    <tbody>
                        {tableRows}
                        <tr class='total-row'>
                            <td colspan='3'>Total Stock Value</td> <!-- Spans the first three columns -->
                            <td colspan='2'>{totalStockValue} (EGP)</td> <!-- Spans the last two columns -->
                        </tr>
                    </tbody>
                </table>
                <div class='footer'>
                    Inventory Management System - Report
                </div>
            </body>
            </html>";

            var converter = new SynchronizedConverter(new PdfTools());
            var doc = new HtmlToPdfDocument()
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
                        FooterSettings = { FontSize = 9, Center = "Page [page] of [toPage]", Line = true }
                    }
                }
            };

            byte[] pdf = _pdfConverter.Convert(doc);
            return File(pdf, "application/pdf", "Inventory Report.pdf");
        }


        //Get: Product/GenerateExcelReport
        public async Task<IActionResult> GenerateExcelReport()
        {
            var products = await _context.Product
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .ToListAsync();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Inventory Report");

                // Set the title
                var titleCells = worksheet.Cells[1, 1, 1, 5];
                titleCells.Merge = true;
                titleCells.Value = "Inventory Report";
                titleCells.Style.Font.Bold = true;
                titleCells.Style.Font.Size = 22;
                titleCells.Style.Fill.PatternType = ExcelFillStyle.Solid;
                titleCells.Style.Fill.BackgroundColor.SetColor(Color.AliceBlue);
                titleCells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                // Include the current date and time
                worksheet.Cells[2, 1].Value = $"Date: {DateTime.Now.ToString("yyyy-MM-dd")}";
                worksheet.Cells[2, 1].Style.Font.Bold = true;
                worksheet.Cells[2, 2].Value = $"Time: {DateTime.Now.ToString("HH:mm:ss")}";
                worksheet.Cells[2, 2].Style.Font.Bold = true;

                // Set headers
                worksheet.Cells[4, 1].Value = "Product Name";
                worksheet.Cells[4, 2].Value = "Category";
                worksheet.Cells[4, 3].Value = "Supplier";
                worksheet.Cells[4, 4].Value = "Price (EGP)";
                worksheet.Cells[4, 5].Value = "Stock Quantity";

                // Set the header style
                using (var range = worksheet.Cells[4, 1, 4, 5])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }

                // Fill data
                for (int i = 0; i < products.Count; i++)
                {
                    var product = products[i];
                    worksheet.Cells[i + 5, 1].Value = product.ProductName;
                    worksheet.Cells[i + 5, 2].Value = product.Category.CategoryName;
                    worksheet.Cells[i + 5, 3].Value = product.Supplier.SupplierName;
                    worksheet.Cells[i + 5, 4].Value = product.Price;
                    if (product.StockQuantity == 0)
                    {
                        worksheet.Cells[i + 5, 5].Value = product.StockQuantity + " (out of Stock!)";
                        worksheet.Cells[i + 5, 5].Style.Font.Bold = true;
                        worksheet.Cells[i + 5, 1, i + 5, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[i + 5, 1, i + 5, 5].Style.Font.Color.SetColor(Color.White);
                        worksheet.Cells[i + 5, 1, i + 5, 5].Style.Fill.BackgroundColor.SetColor(Color.Red);
                    }
                    else if(product.StockQuantity < product.LowStockThreshold && product.StockQuantity > 0)
                    {
                        worksheet.Cells[i + 5, 5].Value = product.StockQuantity + " (Low Stock!)";
                        worksheet.Cells[i + 5, 5].Style.Font.Bold = true;
                        worksheet.Cells[i + 5, 1, i + 5, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[i + 5, 1, i + 5, 5].Style.Font.Color.SetColor(Color.White);
                        worksheet.Cells[i + 5, 1, i + 5, 5].Style.Fill.BackgroundColor.SetColor(Color.Orange);
                    }
                    else
                    {
                        worksheet.Cells[i + 5, 5].Value = product.StockQuantity;
                    }
                    worksheet.Cells[i + 5, 1, i + 5, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }

                // Total Stock Value
                worksheet.Cells[products.Count + 6, 1, products.Count + 6, 3].Merge = true;
                worksheet.Cells[products.Count + 6, 1, products.Count + 6, 3].Value = "Total Stock Value =";
                worksheet.Cells[products.Count + 6, 4, products.Count + 6, 5].Merge = true;
                worksheet.Cells[products.Count + 6, 4, products.Count + 6, 5].Value = _context.Product.Sum(p => p.StockQuantity * p.Price).ToString("F2") + " (EGP)";

                var TotalStockValueCells = worksheet.Cells[products.Count + 6, 1, products.Count + 6, 5];
                TotalStockValueCells.Style.Fill.PatternType = ExcelFillStyle.Solid;
                TotalStockValueCells.Style.Font.Size = 12;
                TotalStockValueCells.Style.Font.Bold = true;
                TotalStockValueCells.Style.Fill.BackgroundColor.SetColor(Color.DarkBlue);
                TotalStockValueCells.Style.Font.Color.SetColor(Color.White);
                TotalStockValueCells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                // Auto-fit columns
                worksheet.Cells.AutoFitColumns();

                // Return the Excel file as a downloadable file
                var excelFile = package.GetAsByteArray();
                return File(excelFile, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Inventory Report.xlsx");
            }
        }
    }
}
