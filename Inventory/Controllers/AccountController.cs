using Inventory.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models;
using System.Runtime.InteropServices;
using System.Security.Claims;

namespace Inventory.Controllers
{

    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _UserManager;
        private readonly RoleManager<IdentityRole> _RoleManager;
        private readonly SignInManager<ApplicationUser> _SigninManager;

        public AccountController(RoleManager<IdentityRole> RoleManager ,UserManager<ApplicationUser> UserManager, SignInManager<ApplicationUser> SigninManager)
        {
            _UserManager = UserManager;
            _SigninManager = SigninManager; 
            _RoleManager = RoleManager;

        }

        public async Task<IActionResult> SignOut()
        {
            await _SigninManager.SignOutAsync();
            TempData["success"] = "You have been logged out";
            return RedirectToAction("Index", "Home");
        }
        public IActionResult Login()
        {
            return View("Login");
        }

        public IActionResult Register()
        {
            return View("Register");
        }

        [HttpPost]
        public async Task<IActionResult> SaveLogin(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser applicationUser =
                await _UserManager.FindByNameAsync(model.UserName);
                if (applicationUser != null)
                {
                    bool found =
                   await _UserManager.CheckPasswordAsync(applicationUser, model.Password);
                    if (found)
                    {

                        var Claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name,applicationUser.UserName),
                        };

                        var identity = new ClaimsIdentity(Claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        await _SigninManager.SignInAsync(applicationUser, model.RememberMe, identity.ToString());
                        TempData["success"] = "You have been logged in";
                        return RedirectToAction("Index", "Home");
                    }

                }
                ModelState.AddModelError("", " Wrong Username Or Password");

            }
            TempData["fail"] = "Wrong Username Or Password";
            return View("Login", model);
        }

        [HttpPost]
        public async Task<IActionResult> SaveRegister(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser applicationUser = new ApplicationUser();
                applicationUser.FName = model.FName;
                applicationUser.LName = model.LName;
                applicationUser.UserName = model.UserName;
                applicationUser.Email = model.Email;
                applicationUser.PhoneNumber = model.PhoneNumber;
                applicationUser.PasswordHash = model.Password;

                IdentityResult result = await _UserManager.CreateAsync(applicationUser, model.Password);
                if (result.Succeeded)
                {
                    await _UserManager.AddToRoleAsync(applicationUser, "Staff");
                    TempData["success"] = "Account Created Successfully";

                    await _SigninManager.SignInAsync(applicationUser, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }
                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError("", item.Description);
                }

            }
            else
            {
                TempData["fail"] = "Invalid Input";
                return View("Register", model);
            }
            TempData["fail"] = "Invalid Input";
            return View("Register", model);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Role()
        {
            return View("Role");
        }

        [HttpPost]
        public async Task<IActionResult> SaveRole(RoleViewModel model)
        {
            if (ModelState.IsValid)
            {
                IdentityRole role = new IdentityRole();
                role.Name = model.RoleName;

                IdentityResult result = await _RoleManager.CreateAsync(role);
                if (result.Succeeded)
                {
                    ViewBag.Success = true;
                    TempData["success"] = "Role Created Successfully";
                    return RedirectToAction("Index", "Home");
                }
                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError("", item.Description);
                }
            }

            return View("Role");
        }
    }
}
