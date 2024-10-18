using Microsoft.AspNetCore.Mvc;
using Inventory.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Inventory.Data;
using Microsoft.AspNetCore.Authorization;

namespace Inventory.Controllers
{
    [Authorize]
    public class SupplierController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SupplierController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Supplier
        public IActionResult Index(string searchString, int page = 1, int pageSize = 6)
        {
            // Retrieve suppliers and apply search if necessary
            var suppliers = string.IsNullOrEmpty(searchString)
                ? _context.Supplier.AsQueryable()
                : _context.Supplier
                    .Where(s => s.SupplierName.StartsWith(searchString))
                    .AsQueryable();

            ViewData["SearchString"] = searchString;

            // Pagination

            var totalsuppliers = suppliers.Count();

            suppliers = suppliers
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            var totalPages = (int)Math.Ceiling(totalsuppliers / (double)pageSize);

            var model = new SupplierListViewModel
            {
                Suppliers = suppliers.ToList(),
                CurrentPage = page,
                TotalPages = totalPages
            };

            return View(model);
        }


        // GET: Supplier/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Supplier/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Supplier supplier)
        {
            if (ModelState.IsValid)
            {
                _context.Add(supplier);
                await _context.SaveChangesAsync();
                TempData["success"] = "Supplier added successfully!";
                return RedirectToAction(nameof(Index));
                
            }
            return View(supplier);
        }

        
        // GET: Supplier/Edit/5    
        public async Task<IActionResult> Edit(int? id)
        {
            // Check if the supplier ID is null
            if (id == null)
            {
                return NotFound("Supplier ID is missing.");
            }

            // Retrieve the supplier from the database
            var supplier = await _context.Supplier.FindAsync(id);

            // Check if the supplier exists
            if (supplier == null)
            {
                return NotFound("Supplier not found.");
            }

            return View(supplier);
        }

        // POST: Supplier/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Supplier supplier)
        {
            // Ensure the posted ID matches the supplier ID
            if (id != supplier.SupplierID)
            {
                return BadRequest("The submitted Supplier ID does not match the original.");
            }

            // Validate the model state
            if (!ModelState.IsValid)
            {
                return View(supplier);
            }

            try
            {
                // Attempt to update the supplier in the database
                _context.Update(supplier);
                await _context.SaveChangesAsync();

                // Success message and redirection
                TempData["success"] = "Supplier updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                // Handle concurrency exception (if two users are updating the same record)
                if (!SupplierExists(supplier.SupplierID))
                {
                    return NotFound("Supplier no longer exists.");
                }
                else
                {
                    // Log the error if necessary and rethrow it
                    throw;
                }
            }
            catch (Exception ex)
            {
                // Handle any unexpected errors
                ModelState.AddModelError("", "An error occurred while updating the supplier. Please try again.");
                // Optionally log the error
                // _logger.LogError(ex, "Error while updating supplier {SupplierID}", supplier.SupplierID);
                return View(supplier);
            }
        }


        // GET: Supplier/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var supplier = await _context.Supplier
                .FirstOrDefaultAsync(m => m.SupplierID == id);
            if (supplier == null)
            {
                return NotFound();
            }

            return View(supplier);
        }

        // POST: Supplier/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var supplier = await _context.Supplier.FindAsync(id);
            if (supplier != null)
            {
                _context.Supplier.Remove(supplier);
                TempData["success"] = "Supplier deleted successfully!";
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }


        // Check if supplier exists
        private bool SupplierExists(int id)
        {
            return _context.Supplier.Any(e => e.SupplierID == id);
        }


        // GET: Supplier/PrintProducts/5
        public async Task<IActionResult> PrintProducts(int id)
        {
            // Retrieve the supplier with their associated products


            var supplier = await _context.Supplier
                .Include(s => s.Products)
                .FirstOrDefaultAsync(s => s.SupplierID == id);


            if (supplier == null)
            {
                return NotFound("Supplier not found.");
            }

            return View(supplier);
        }

    }
}
