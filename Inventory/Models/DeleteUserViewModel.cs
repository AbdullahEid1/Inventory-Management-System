namespace Inventory.Models
{
    public class DeleteUserViewModel
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Roles { get; set; } // Display roles as comma-separated values
    }
}
