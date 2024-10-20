using System.ComponentModel.DataAnnotations;

namespace Inventory.Models
{
    public class EditUserViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        
        [Display(Name = "Phone")]
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }

        public string SelectedRole { get; set; } // To hold the selected role
        public List<string> AvailableRoles { get; set; } = new List<string>(); // List of available roles
    }
}
