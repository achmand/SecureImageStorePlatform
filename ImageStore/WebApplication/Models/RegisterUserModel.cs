using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace WebApplication.Models
{
    public sealed class RegisterUserModel
    {
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(255, ErrorMessage = "Must be between 5 to 255 characters", MinimumLength = 5)]
        //[Remote("UsernameExist", "Users", HttpMethod = "POST", ErrorMessage = "Username already exist.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, ErrorMessage = "Must be between 8 and 100 characters", MinimumLength = 8)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[$@$!%*?&])[A-Za-z\d$@$!%*?&]{8,}",
            ErrorMessage = "Minimum 8 characters at least 1 Uppercase Alphabet, 1 Lowercase Alphabet, 1 Number and 1 Special Character")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DisplayName("Confirm Password")]
        [Required(ErrorMessage = "Must confirm Password.")]
        [StringLength(100, ErrorMessage = "Must be between 8 to 100 characters", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [System.ComponentModel.DataAnnotations.Compare("Password")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Role is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Not a valid role.")]
        public int? RoleId { get; set; }

        public IEnumerable<SelectListItem> RoleList { get; set; }
    }
}