using System.ComponentModel.DataAnnotations;

using System.Collections.Generic;

namespace Inventory.Models
{
    public class Supplier
    {
        public int SupplierID { get; set; } // Primary Key

        public string SupplierName { get; set; } // Supplier name will be configured in Fluent API

        public string? Phone { get; set; } // Will be validated with Fluent API

        public string? Email { get; set; } // Will be validated with Fluent API

        public string? Address { get; set; } // Optional, maximum length set in Fluent API

        // Navigation Property (One-to-Many relationship with Product)
        public ICollection<Product>? Products { get; set; }
    }
}
