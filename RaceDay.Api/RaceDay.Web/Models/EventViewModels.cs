namespace RaceDay.Web.Models
{
    public class EventListItem
    {
        public int EventId { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = "";
        public decimal DistanceKm { get; set; }
        public string EventType { get; set; } = "";
        public string? BannerImageUrl { get; set; }
        public string OrganiserName { get; set; } = "";
    }

    public class CategoryItem
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = "";
        public string? Description { get; set; }
    }

    public class EventDetails : EventListItem
    {
        public List<CategoryItem> Categories { get; set; } = new();
    }

    public class EventsIndexViewModel
    {
        public List<EventListItem> Events { get; set; } = new();
        public string? Search { get; set; }
        public string? Type { get; set; }
    }
}