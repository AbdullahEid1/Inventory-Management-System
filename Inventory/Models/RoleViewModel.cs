using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class RoleViewModel
    {
        [Display(Name = "Role Name")]
        public required string RoleName { get; set; }
    }
}
