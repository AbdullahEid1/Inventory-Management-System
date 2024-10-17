using Inventory.Data;
using Inventory.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Inventory.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger ,ApplicationDbContext context)
        {
            _logger = logger;
            _context=context;
        }

        public IActionResult Index()
        {
            int products = _context.Product.Count();
            int categories = _context.Category.Count();
            int suppliers = _context.Supplier.Count();
            int users = _context.Users.Count();

            var lowstock = _context.Product.Include(p => p.Supplier).Include(p => p.Category).Where(p => p.StockQuantity < p.LowStockThreshold).ToList();

            HomeViewModel model = new HomeViewModel
            {
                TotalProducts = products,
                TotalUsers = users,
                TotalCategories = categories,
                TotalSupplires = suppliers,
                LowStockProducts = lowstock
            };

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
