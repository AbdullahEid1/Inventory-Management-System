using Microsoft.EntityFrameworkCore;
using Inventory.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Inventory.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Supplier> Supplier { get; set; }
        public DbSet<Product> Product { get; set; }
        public DbSet<Category> Category { get; set; }

        // Constructor that accepts DbContextOptions
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) // Passing options to the base DbContext class
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Supplier Configuration
            modelBuilder.Entity<Supplier>()
                .Property(s => s.SupplierName)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Supplier>()
                .Property(s => s.Phone)
                .HasMaxLength(15)
                .IsUnicode(false); // Assuming phone is alphanumeric

            modelBuilder.Entity<Supplier>()
                .Property(s => s.Email)
                .HasMaxLength(100)
                .IsUnicode(false); // Emails don't need Unicode

            modelBuilder.Entity<Supplier>()
                .Property(s => s.Address)
                .HasMaxLength(200);

            // Product Configuration
            modelBuilder.Entity<Product>()
                .Property(p => p.ProductName)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Product>()
                .Property(p => p.StockQuantity)
                .HasDefaultValue(0);

            modelBuilder.Entity<Product>()
                .Property(p => p.LowStockThreshold)
                .HasDefaultValue(10);

            // Category Configuration
            modelBuilder.Entity<Category>()
                .Property(c => c.CategoryName)
                .IsRequired()
                .HasMaxLength(100);

            // One-to-Many Relationships
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Supplier)
                .WithMany(s => s.Products)
                .HasForeignKey(p => p.SupplierID);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryID);

            // On Delete Behavior: SetNull
            modelBuilder.Entity<Product>()
                .HasOne(c => c.Supplier)
                .WithMany(p => p.Products)
                .HasForeignKey(c => c.SupplierID)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Product>()
                .HasOne(c => c.Category)
                .WithMany(p => p.Products)
                .HasForeignKey(c => c.CategoryID)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}


