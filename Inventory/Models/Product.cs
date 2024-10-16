using System.ComponentModel.DataAnnotations;

namespace Inventory.Models
{
    public class Product
    {
        public int ProductID { get; set; } // Primary Key

        public string ProductName { get; set; } // Product name will be configured in Fluent API

        public int? CategoryID { get; set; } // Foreign Key to Category
        public int? SupplierID { get; set; } // Foreign Key to Supplier

        public decimal Price { get; set; } // Price with validation through Fluent API

        public int StockQuantity { get; set; } // Will be configured in Fluent API

        public int LowStockThreshold { get; set; } // Configured with default value in Fluent API

        public bool IsAvailable { get; set; } = true; // Default value to true

        // Navigation Properties
        public Category? Category { get; set; }
        public Supplier? Supplier { get; set; }
    }
}
