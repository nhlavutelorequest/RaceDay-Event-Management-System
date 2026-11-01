using System.ComponentModel.DataAnnotations;

namespace RaceDay.Web.Models
{
    public class MyEventItem : EventListItem
    {
        public int EnrolmentCount { get; set; }
    }

    public class EventFormVm
    {
        public int EventId { get; set; }

        [Required(ErrorMessage = "Event name is required."), MaxLength(150, ErrorMessage = "Event name is too long.")]
        public string Name { get; set; } = "";

        [MaxLength(1000, ErrorMessage = "Description is too long.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Please choose the event date and time.")]
        public DateTime? EventDate { get; set; }

        [Required(ErrorMessage = "Location is required."), MaxLength(200, ErrorMessage = "Location is too long.")]
        public string Location { get; set; } = "";

        [Required(ErrorMessage = "Distance is required."), Range(0.1, 1000, ErrorMessage = "Distance must be between 0.1 and 1000 km.")]
        public decimal? DistanceKm { get; set; }

        [Required(ErrorMessage = "Please choose an event type.")]
        public string EventType { get; set; } = "";

        // Kept so editing an event never wipes its banner
        public string? BannerImageUrl { get; set; }
    }
}