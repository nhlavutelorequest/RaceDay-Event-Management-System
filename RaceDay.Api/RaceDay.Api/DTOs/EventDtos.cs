using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.DTOs
{
    public class CreateEventDto
    {
        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        [Required, MaxLength(200)]
        public string Location { get; set; } = string.Empty;

        [Required]
        public decimal DistanceKm { get; set; }

        // Must be "Run", "Walk", or "Cycle"
        [Required]
        public string EventType { get; set; } = string.Empty;

        public string? BannerImageUrl { get; set; }
    }

    public class UpdateEventDto : CreateEventDto { }
}