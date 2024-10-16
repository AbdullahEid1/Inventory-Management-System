using System.ComponentModel.DataAnnotations;

namespace FirstMVC.ViewModel
{
    public class LoginViewModel
    {
        [Display(Name = "Username")]
        public required string UserName { get; set; }

        [DataType(DataType.Password)]
        public required string Password { get; set; }

        [Display(Name ="Remember me")]
        public bool RememberMe { get; set; }
    }
}
