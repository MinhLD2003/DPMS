using System.ComponentModel.DataAnnotations;

namespace DemoExternalSystem.Models
{
    public class LoginVM
    {
        [DataType(DataType.EmailAddress)]
        [Required(ErrorMessage = "Username không được để trống")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password không được để trống")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}
