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

        [Required, MinLength(6)]
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