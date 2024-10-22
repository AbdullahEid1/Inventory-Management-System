using Inventory.Data;
using Inventory.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.Extensions.Options;
namespace Inventory
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Register DinkToPdf as a service
            builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));


            // Identity
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(
                option =>
                {
                    option.Password.RequireDigit = false;
                    option.Password.RequireNonAlphanumeric = false;
                    option.Password.RequiredLength = 8;
                }
            )
                .AddEntityFrameworkStores<ApplicationDbContext>();

            builder.Services.AddDbContext<ApplicationDbContext>(
                (options) => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
            );

           
            // Add services to the container.
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                SeedRolesAndAdminUser(roleManager, userManager).Wait();
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }

        public static async Task SeedRolesAndAdminUser(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            // Check if "admin" role exists, if not, create it
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            // Check if "staff" role exists, if not, create it
            if (!await roleManager.RoleExistsAsync("Staff"))
            {
                await roleManager.CreateAsync(new IdentityRole("Staff"));
            }

            // Seed an admin user if it doesn't already exist
            var adminUserName = "Admin";
            var adminEmail = "admin@gmail.com";
            var adminUser = await userManager.FindByNameAsync(adminUserName);

            if (adminUser == null)
            {
                var user = new ApplicationUser
                {
                    UserName = adminUserName,
                    FName= "admin",
                    LName= "admin",
                    Email = adminEmail,
                };

                // Create the admin user with a predefined password
                var result = await userManager.CreateAsync(user, "Admin@123456"); // Ensure this password meets your security needs

                // Assign the "admin" role to the user
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Admin");
                }
            }
        }
    }


}


