using DPMS_WebAPI.Enums;
using System.ComponentModel.DataAnnotations;

namespace DPMS_WebAPI.ViewModels.Auth
{

    public class CreateAccountVM
    {
        [Required(ErrorMessage = "Please enter full name")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Full name must be between 3 and 100 characters")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Please enter email address")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please confirm password")]
        [Display(Name = "Confirm Password")]
        public bool IsPasswordConfirmed { get; set; }

        [Required(ErrorMessage = "Please enter username")]
        [StringLength(50, MinimumLength = 5, ErrorMessage = "Username must be between 5 and 50 characters")]
        [RegularExpression(@"^[a-zA-Z0-9._-]+$", ErrorMessage = "Username can only contain letters, numbers, and ._- characters")]
        [Display(Name = "Username")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Please select account status")]
        [Display(Name = "Status")]
        public UserStatus Status { get; set; }

        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$",
            ErrorMessage = "Password must contain at least one lowercase letter, one uppercase letter, one number, and one special character")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string? Password { get; set; }
    }
}
