namespace RaceDay.Api.Models;
public class RouteInfo
{
    public int RouteId { get; set; }
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
    public string? GpxDataUrl { get; set; }
    public int? ElevationGainMeters { get; set; }
    public string? StartCoordinates { get; set; }
    public string? WeatherLookupReference { get; set; }
}
