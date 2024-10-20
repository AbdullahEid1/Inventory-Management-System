using Inventory.Data;
using Inventory.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ILogger<HomeController> logger ,ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _context=context;
            _userManager = userManager;
        }

        public IActionResult Index(int id)
        {
            int products = _context.Product.Count();
            Product showedproduct = _context.Product.FirstOrDefault(p => p.ProductID == id);
            int categories = _context.Category.Count();
            int suppliers = _context.Supplier.Count();
            int users = _context.Users.Count();

            var lowstock = _context.Product.Include(p => p.Supplier).Include(p => p.Category).Where(p => p.StockQuantity < p.LowStockThreshold).ToList();

            HomeViewModel model = new HomeViewModel
            {
                TotalStockValue = _context.Product.Sum(p => p.StockQuantity * p.Price),
                TotalProducts = products,
                TotalUsers = users,
                ShowedProduct = showedproduct,
                TotalCategories = categories,
                TotalSupplires = suppliers,
                LowStockProducts = lowstock
            };

            return View(model);
        }


        /*public async Task<IActionResult> ShowUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            var userViewModels = new List<UserViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user); // Fetch user roles

                userViewModels.Add(new UserViewModel
                {
                    FirstName = user.LName,
                    LastName = user.LName,
                    Email = user.Email,
                    userid = user.Id,
                    Username = user.UserName,
                    Role = roles[0],// Add the roles to the ViewModel
                });
            }

            return View(userViewModels);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }*/
    }
}
