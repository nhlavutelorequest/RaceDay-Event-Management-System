using System.ComponentModel.DataAnnotations;

namespace RaceDay.Web.Models
{
    public class RegisterViewModel
    {
        [Required, Display(Name = "Full Name")]
        public string FullName { get; set; } = "";

        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Phone, Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [Required]
        public string Role { get; set; } = "Participant";

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).+$",
            ErrorMessage = "Password needs an uppercase letter, a lowercase letter, a number and a special character.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [Required, Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
        [DataType(DataType.Password), Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = "";
    }

    public class LoginViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";
    }
}