namespace RaceDay.Api.Models;
public class Result
{
    public int ResultId { get; set; }
    public int EnrolmentId { get; set; }
    public Enrolment? Enrolment { get; set; }
    public long FinishTimeMs { get; set; }
    public int? OverallPlacement { get; set; }
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
}
