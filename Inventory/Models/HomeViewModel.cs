namespace Inventory.Models
{
    public class HomeViewModel
    {
        public int TotalProducts { get; set; }
        public int TotalCategories { get; set; }
        public int TotalUsers { get; set; }

        public Product ShowedProduct { get; set; }
        public int TotalSupplires { get; set; }

        public IEnumerable<Product> LowStockProducts { get; set; }
        public decimal TotalStockValue { get; set; }
    }
}
