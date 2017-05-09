using System.ComponentModel.DataAnnotations;

namespace Common
{
    [MetadataType(typeof(UserRequestMetaData))]
    public partial class User
    {
    }

    public class UserRequestMetaData
    {
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(100, ErrorMessage = "Must be between 5 to 255 characters", MinimumLength = 5)]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
