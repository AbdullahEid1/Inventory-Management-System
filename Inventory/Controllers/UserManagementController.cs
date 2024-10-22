using Inventory.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Inventory.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserManagementController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserManagementController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: UserManagementController/ShowUsers/5
        public async Task<IActionResult> ShowUsers()
        {
            var users = await _userManager.Users.ToListAsync();

            var model = new List<UserViewModel>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                model.Add(new UserViewModel
                {
                    userid = user.Id,
                    Username = user.UserName,
                    Email = user.Email,
                    FirstName = user.FName,
                    LastName = user.LName,
                    PhoneNumber = user.PhoneNumber,
                    Role = string.Join(", ", roles) // Joining roles into a comma-separated string
                });
            }

            return View(model);
        }


        // GET: UserManagementController/EditUser/5
        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var allRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();

            var model = new EditUserViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                FirstName = user.FName,
                LastName = user.LName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                AvailableRoles = allRoles,
                SelectedRole = roles.FirstOrDefault()
            };

            return View(model);
        }


        // Post: UserManagementController/Edit/5
        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.Id);
                if (user == null)
                {
                    return NotFound();
                }

                // Get the current roles of the user
                var currentRoles = await _userManager.GetRolesAsync(user);

                // Check if the user is currently an admin
                if (currentRoles.Contains("Admin"))
                {
                    // Get the number of admins
                    var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");

                    // If there's only one admin and the new selected role is not Admin
                    if (adminUsers.Count == 1 && model.SelectedRole != "Admin")
                    {
                        // Add an error message and return to the view
                        ModelState.AddModelError(string.Empty, "Sorry! There must be at least one Admin in the system. Please assign another user as Admin before changing the role of this user.");
                        model.AvailableRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync(); // Reload available roles
                        return View(model);
                    }
                }

                user.FName = model.FirstName;
                user.LName = model.LastName;
                user.Email = model.Email;
                user.PhoneNumber = model.PhoneNumber;

                // Remove the user from their current roles
                var removeRolesResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeRolesResult.Succeeded)
                {
                    ModelState.AddModelError(string.Empty, "Error occurred while removing user roles.");
                    model.AvailableRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync(); // Reload available roles
                    return View(model);
                }

                // Assign the user to the newly selected role
                var addRoleResult = await _userManager.AddToRoleAsync(user, model.SelectedRole);
                if (!addRoleResult.Succeeded)
                {
                    ModelState.AddModelError(string.Empty, "Error occurred while assigning user roles.");
                    model.AvailableRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync(); // Reload available roles
                    return View(model);
                }

                // Update the user's other details if necessary
                var updateResult = await _userManager.UpdateAsync(user);
                if (updateResult.Succeeded)
                {
                    TempData["success"] = "User Updated Successfully";
                    return RedirectToAction("ShowUsers");
                }

                // Add any errors to the model state
                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed; redisplay the form.
            model.AvailableRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync(); // Reload available roles
            TempData["fail"] = "Invalid Input";
            return View(model);
        }



        // GET: UserManagementController/DeleteUser/{id}
        [HttpGet]
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user); // Get user roles

            // Prepare the user details for the confirmation page
            var model = new DeleteUserViewModel
            {
                Id = user.Id,
                Username = user.UserName,
                Email = user.Email,
                FirstName = user.FName,
                LastName = user.LName,
                PhoneNumber = user.PhoneNumber,
                Roles = string.Join(", ", roles) // Display roles as a comma-separated string
            };

            return View(model); // Passing the user object to the view
        }


        // POST: UserManagementController/DeleteUserConfirmed/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUserConfirmed(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Check if the user has an Admin role
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("Admin"))
            {
                // Get the number of users with the Admin role
                var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");

                // If there is only one Admin, prevent deletion
                if (adminUsers.Count == 1)
                {
                    ModelState.AddModelError(string.Empty, "Sorry! There must be at least one Admin in the system. Please assign another user as Admin before deleting.");

                    // Return the user details to the DeleteUser confirmation view with an error message
                    var model = new DeleteUserViewModel
                    {
                        Id = user.Id,
                        Username = user.UserName,
                        Email = user.Email,
                        FirstName = user.FName,
                        LastName = user.LName,
                        PhoneNumber = user.PhoneNumber,
                        Roles = string.Join(", ", roles)
                    };
                    return View("DeleteUser", model); // Re-display the confirmation view with error
                }
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["success"] = "User Deleted Successfully";
                return RedirectToAction("ShowUsers"); // Redirect back to the users list
            }

            // Handle errors if deletion fails
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var returnModel = new DeleteUserViewModel
            {
                Id = user.Id,
                Username = user.UserName,
                Email = user.Email,
                FirstName = user.FName,
                LastName = user.LName,
                PhoneNumber = user.PhoneNumber,
                Roles = string.Join(", ", userRoles)
            };

            return View("DeleteUser", returnModel);
        }
    }
}
