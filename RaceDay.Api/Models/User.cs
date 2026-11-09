// RaceDay: Entity representing an account, its role, profile information, and related events and enrolments.
// Comments explain the purpose of this file and important logic; the code behavior is unchanged.

namespace RaceDay.Api.Models;

public class User
{
    public int UserId { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Role { get; set; } = "Participant";
    public string? Phone { get; set; }
    public string? ProfileImageUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<RaceEvent> OrganizedEvents { get; set; } = new List<RaceEvent>();
    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
}
