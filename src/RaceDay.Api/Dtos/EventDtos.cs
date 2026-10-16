namespace RaceDay.Api.Dtos;
public record EventRequest(string Title, string Description, string Location, DateTime EventDate, string EventType);
public record CategoryRequest(string CategoryName, decimal DistanceKm, decimal EntryFee, string? AgeGroup);
public record EnrolmentRequest(int CategoryId, string EmergencyContact);
public record ResultRequest(int EnrolmentId, long FinishTimeMs, int? OverallPlacement);
public record RouteRequest(string? GpxDataUrl, int? ElevationGainMeters, string? StartCoordinates, string? WeatherLookupReference);
