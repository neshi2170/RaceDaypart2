namespace RaceDay.Api.Models;
public class Event
{
    public int EventId { get; set; }
    public int OrganiserId { get; set; }
    public User? Organiser { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Location { get; set; } = "";
    public DateTime EventDate { get; set; }
    public string EventType { get; set; } = "Run";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<Category> Categories { get; set; } = new List<Category>();
}
