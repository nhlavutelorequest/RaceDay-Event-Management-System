using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.DTOs
{
    public class UpdateProfileDto
    {
        [Required(ErrorMessage = "Full name is required."), MaxLength(100, ErrorMessage = "Full name is too long.")]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(20, ErrorMessage = "Phone number is too long.")]
        public string? PhoneNumber { get; set; }
    }
}