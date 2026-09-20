using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models
{
    public class Enrolment
    {
        [Key]
        public int EnrolmentId { get; set; }

        [Required]
        public int ParticipantId { get; set; }

        [ForeignKey(nameof(ParticipantId))]
        public User? Participant { get; set; }

        [Required]
        public int EventId { get; set; }

        [ForeignKey(nameof(EventId))]
        public Event? Event { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public Category? Category { get; set; }

        [Required, MaxLength(20)]
        public string Status { get; set; } = "Pending";

        public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

        public Result? Result { get; set; }
    }

    public static class EnrolmentStatuses
    {
        public static readonly string[] Allowed = { "Pending", "Confirmed", "Cancelled" };
        public static bool IsValid(string value) => Allowed.Contains(value);
    }
}