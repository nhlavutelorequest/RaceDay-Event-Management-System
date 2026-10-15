using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.DTOs
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Full name is required."), MaxLength(100, ErrorMessage = "Full name is too long.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required."), EmailAddress(ErrorMessage = "Please enter a valid email address."), MaxLength(150, ErrorMessage = "Email is too long.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required."), MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        public string Password { get; set; } = string.Empty;

        [MaxLength(20, ErrorMessage = "Phone number is too long.")]
        public string? PhoneNumber { get; set; }

        // Must be "Organiser" or "Participant"
        [Required(ErrorMessage = "Please choose a role.")]
        public string Role { get; set; } = string.Empty;
    }

    public class LoginDto
    {
        [Required(ErrorMessage = "Email is required."), EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; } = string.Empty;
    }
}