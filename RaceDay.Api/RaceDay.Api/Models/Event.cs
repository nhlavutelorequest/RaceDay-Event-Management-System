using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models
{
    public class Event
    {
        [Key]
        public int EventId { get; set; }

        [Required]
        public int OrganiserId { get; set; }

        [ForeignKey(nameof(OrganiserId))]
        public User? Organiser { get; set; }

        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        [Required, MaxLength(200)]
        public string Location { get; set; } = string.Empty;

        [Required, Column(TypeName = "decimal(6,2)")]
        public decimal DistanceKm { get; set; }

        [Required, MaxLength(10)]
        public string EventType { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? BannerImageUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Category> Categories { get; set; } = new List<Category>();
        public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
    }

    public static class EventTypes
    {
        public static readonly string[] Allowed = { "Run", "Walk", "Cycle" };
        public static bool IsValid(string value) => Allowed.Contains(value);
    }
}