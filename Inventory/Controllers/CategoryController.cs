using Inventory.Data;
using Inventory.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Controllers
{
    [Authorize]

    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }


        // GET: Category/Index
        public IActionResult Index(int page = 1, int pageSize = 6)
        {
            var categories = _context.Category.AsQueryable();

            var totalcategories = categories.Count();

            categories = categories
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            var totalPages = (int)Math.Ceiling(totalcategories / (double)pageSize);

            var model = new CategoryListViewModel
            {
                Categories = categories.ToList(),
                CurrentPage = page,
                TotalPages = totalPages
            };

            return View(model);
        }


        // GET: Category/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Category/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Category.Add(category);
                _context.SaveChanges();
                TempData["success"] = "Category added successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }


        // GET: Category/Edit/{id}
        public IActionResult Edit(int id)
        {
            var category = _context.Category.Find(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: Category/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Category updatecategory)
        {
            var category = _context.Category.Find(id);

            if (id != category.CategoryID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                category.CategoryName = updatecategory.CategoryName;
                _context.SaveChanges();
                TempData["success"] = "Category updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }


        // GET: Category/Delete/{id}
        public IActionResult Delete(int id)
        {
            var category = _context.Category.Find(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: Category/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var category = _context.Category.Find(id);
            if (category != null)
            {
                _context.Category.Remove(category);
                TempData["success"] = "Category deleted successfully!";
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }


        // This is the endpoint for AJAX calls to add new category
        [HttpPost]
        public IActionResult AddCategory(string categoryName)
        {
            var category = new Category
            {
                CategoryName = categoryName
            };

            _context.Category.Add(category);
            TempData["success"] = "Category added successfully!";
            _context.SaveChanges();

            return Json(new { categoryID = category.CategoryID, categoryName = category.CategoryName });
        }


        //Get: Category/Search
        public IActionResult Search(string keyword, int page = 1, int pageSize = 6)
        {
            var categories = string.IsNullOrEmpty(keyword)
                ? _context.Category.AsQueryable()
                : _context.Category
                    .Where(c => c.CategoryName.StartsWith(keyword))
                    .AsQueryable();

            ViewData["Keyword"] = keyword;

            // Pagination

            var totalcategories = categories.Count();

            categories = categories
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            var totalPages = (int)Math.Ceiling(totalcategories / (double)pageSize);

            var model = new CategoryListViewModel
            {
                Categories = categories.ToList(),
                CurrentPage = page,
                TotalPages = totalPages
            };

            return View("Index", model);
        }


        // GET: Category/PrintProducts/5
        public async Task<IActionResult> PrintProducts(int id)
        {
            // Retrieve the Category with its associated products


            var category = await _context.Category
                .Include(s => s.Products)
                .FirstOrDefaultAsync(s => s.CategoryID == id);


            if (category == null)
            {
                return NotFound("Category not found.");
            }

            return View(category);
        }
    }
}
