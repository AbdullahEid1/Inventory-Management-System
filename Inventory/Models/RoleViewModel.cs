using System.ComponentModel.DataAnnotations;

namespace FirstMVC.ViewModel
{
    public class RoleViewModel
    {
        [Display(Name = "Role Name")]
        public required string RoleName { get; set; }
    }
}
