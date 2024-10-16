using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Inventory.Models
{
        public class Category
        {
            public int CategoryID { get; set; } // Primary Key

            public string CategoryName { get; set; } // Will be configured with Fluent API

            // Navigation Property (One-to-Many relationship with Product)
            public ICollection<Product>? Products { get; set; }
        }
}
