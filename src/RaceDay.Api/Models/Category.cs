namespace RaceDay.Api.Models;
public class Category
{
    public int CategoryId { get; set; }
    public int EventId { get; set; }
    public Event? Event { get; set; }
    public string CategoryName { get; set; } = "";
    public decimal DistanceKm { get; set; }
    public decimal EntryFee { get; set; }
    public string? AgeGroup { get; set; }
    public RouteInfo? Route { get; set; }
    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
}
