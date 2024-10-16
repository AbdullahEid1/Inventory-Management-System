using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Inventory.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, ErrorMessage = "First name cannot be longer than 50 characters.")]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "First name can only contain letters")]
        public string FName { get; set; }


        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, ErrorMessage = "Last name cannot be longer than 50 characters.")]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Last name can only contain letters")]
        public string LName { get; set; }
    }
}
