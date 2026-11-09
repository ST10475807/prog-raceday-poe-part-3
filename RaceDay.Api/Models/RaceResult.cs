// RaceDay: Entity storing the finishing position, time, and status for an enrolment.
// Comments explain the purpose of this file and important logic; the code behavior is unchanged.

namespace RaceDay.Api.Models;

public class RaceResult
{
    public int ResultId { get; set; }
    public int EnrolmentId { get; set; }
    public Enrolment? Enrolment { get; set; }
    public int? FinishPosition { get; set; }
    public TimeSpan? FinishTime { get; set; }
    public string ResultStatus { get; set; } = "Pending";
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
}
