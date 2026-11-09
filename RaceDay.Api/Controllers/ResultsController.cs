
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Contracts;
using RaceDay.Api.Data;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api")]
public class ResultsController(RaceDayContext db) : ControllerBase
{
    private int UserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // Returns results belonging to the signed-in Participant.
    [HttpGet("results/me")]
    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> Mine()
    {
        var results = await db.Results
            .Where(r => r.Enrolment!.ParticipantId == UserId)
            .Include(r => r.Enrolment)
                .ThenInclude(e => e!.EventCategory)
                    .ThenInclude(ec => ec!.Event)
            .Include(r => r.Enrolment)
                .ThenInclude(e => e!.EventCategory)
                    .ThenInclude(ec => ec!.Category)
            .OrderByDescending(r => r.Enrolment!.EventCategory!.Event!.EventDate)
            .Select(r => new
            {
                r.ResultId,
                r.EnrolmentId,
                EventName = r.Enrolment!.EventCategory!.Event!.EventName,
                EventDate = r.Enrolment.EventCategory.Event.EventDate,
                EventId = r.Enrolment.EventCategory.Event.EventId,
                CategoryName = r.Enrolment.EventCategory.Category!.CategoryName,
                r.FinishPosition,
                r.FinishTime,
                r.ResultStatus,
                r.RecordedAt
            })
            .ToListAsync();

        var counts = new Dictionary<int, int>();

        foreach (var eventId in results.Select(r => r.EventId).Distinct())
        {
            counts[eventId] = await db.Results.CountAsync(r =>
                r.ResultStatus == "Finished" &&
                r.Enrolment!.EventId == eventId);
        }

        return Ok(results.Select(r => new
        {
            r.ResultId,
            r.EnrolmentId,
            r.EventName,
            r.EventDate,
            r.CategoryName,
            r.FinishPosition,
            r.FinishTime,
            r.ResultStatus,
            r.RecordedAt,
            TotalFinishers = counts.GetValueOrDefault(r.EventId),

            PositionLabel = r.FinishPosition.HasValue
                ? $"{r.FinishPosition}{Ordinal(r.FinishPosition.Value)} out of {counts.GetValueOrDefault(r.EventId)} finishers"
                : "—"
        }));
    }

    // Converts positions into ordinal numbers: 1st, 2nd, 3rd, 4th.
    private static string Ordinal(int n)
    {
        if (n % 100 >= 11 && n % 100 <= 13)
            return "th";

        return (n % 10) switch
        {
            1 => "st",
            2 => "nd",
            3 => "rd",
            _ => "th"
        };
    }

    // Gets a result for an enrolment.
    [HttpGet("enrolments/{enrolmentId:int}/result")]
    [Authorize]
    public async Task<IActionResult> Get(int enrolmentId)
    {
        var e = await db.Enrolments
            .Include(x => x.EventCategory)
                .ThenInclude(x => x!.Event)
            .Include(x => x.Result)
            .FirstOrDefaultAsync(x => x.EnrolmentId == enrolmentId);

        if (e is null)
            return NotFound();

        if (User.IsInRole("Participant") && e.ParticipantId != UserId)
            return Forbid();

        if (User.IsInRole("Organizer") &&
            e.EventCategory!.Event!.OrganizerId != UserId)
            return Forbid();

        return e.Result is null
            ? NotFound(new { message = "No result recorded yet." })
            : Ok(e.Result);
    }

    // Creates a result for an enrolled Participant.
    [HttpPost("enrolments/{enrolmentId:int}/result")]
    [Authorize(Roles = "Organizer")]
    public async Task<IActionResult> Create(
        int enrolmentId,
        ResultRequest request)
    {
        var e = await db.Enrolments
            .Include(x => x.EventCategory)
                .ThenInclude(x => x!.Event)
            .FirstOrDefaultAsync(x => x.EnrolmentId == enrolmentId);

        if (e is null)
            return NotFound();

        if (e.EventCategory!.Event!.OrganizerId != UserId)
            return Forbid();

        if (e.EventCategory.Event.EventDate.Date > DateTime.UtcNow.Date &&
            e.EventCategory.Event.Status != "Completed")
        {
            return Conflict(new
            {
                message = "Results can only be entered after the event has concluded."
            });
        }

        if (await db.Results.AnyAsync(r => r.EnrolmentId == enrolmentId))
        {
            return Conflict(new
            {
                message = "A result already exists for this enrolment."
            });
        }

        if (!Valid(request))
        {
            return BadRequest(new
            {
                message = "Invalid result status or finishing position."
            });
        }

        var result = new RaceResult
        {
            EnrolmentId = enrolmentId,
            FinishPosition = request.FinishPosition,
            FinishTime = request.FinishTime,
            ResultStatus = request.ResultStatus
        };

        db.Results.Add(result);
        await db.SaveChangesAsync();

        return CreatedAtAction(
            nameof(Get),
            new { enrolmentId },
            result);
    }

    // Updates an existing result.
    [HttpPut("enrolments/{enrolmentId:int}/result")]
    [Authorize(Roles = "Organizer")]
    public async Task<IActionResult> Update(
        int enrolmentId,
        ResultRequest request)
    {
        var e = await db.Enrolments
            .Include(x => x.EventCategory)
                .ThenInclude(x => x!.Event)
            .FirstOrDefaultAsync(x => x.EnrolmentId == enrolmentId);

        if (e is null)
            return NotFound();

        if (e.EventCategory!.Event!.OrganizerId != UserId)
            return Forbid();

        if (e.EventCategory.Event.EventDate.Date > DateTime.UtcNow.Date &&
            e.EventCategory.Event.Status != "Completed")
        {
            return Conflict(new
            {
                message = "Results can only be entered after the event has concluded."
            });
        }

        var r = await db.Results
            .SingleOrDefaultAsync(x => x.EnrolmentId == enrolmentId);

        if (r is null)
            return NotFound();

        if (!Valid(request))
        {
            return BadRequest(new
            {
                message = "Invalid result status or finishing position."
            });
        }

        r.FinishPosition = request.FinishPosition;
        r.FinishTime = request.FinishTime;
        r.ResultStatus = request.ResultStatus;
        r.RecordedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        return Ok(r);
    }

    // Validates the submitted result.
    private static bool Valid(ResultRequest r)
    {
        return
            (r.FinishPosition is null or > 0) &&
            (r.FinishTime is null ||
             (r.FinishTime.Value >= TimeSpan.Zero &&
              r.FinishTime.Value < TimeSpan.FromDays(1))) &&
            (r.ResultStatus is "Pending" or "Finished" or "DNF" or "DNS");
    }
}
