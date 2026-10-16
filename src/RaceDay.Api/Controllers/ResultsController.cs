using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;
namespace RaceDay.Api.Controllers;
[ApiController, Authorize]
[Route("api/results")]
public class ResultsController(RaceDayDbContext db) : ControllerBase
{
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    [HttpPost, Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Create(ResultRequest r)
    {
        if (r.FinishTimeMs <= 0 || r.OverallPlacement is <= 0) return BadRequest("Finish time must be positive and placement, when supplied, must be positive.");
        var enrolment = await db.Enrolments.Include(x => x.Category).ThenInclude(c => c!.Event).SingleOrDefaultAsync(x => x.EnrolmentId == r.EnrolmentId);
        if (enrolment is null) return NotFound("Enrolment not found.");
        if (enrolment.Category!.Event!.OrganiserId != UserId) return Forbid();
        if (await db.Results.AnyAsync(x => x.EnrolmentId == r.EnrolmentId)) return Conflict("A result already exists for this enrolment.");
        var result = new Result { EnrolmentId = r.EnrolmentId, FinishTimeMs = r.FinishTimeMs, OverallPlacement = r.OverallPlacement };
        db.Results.Add(result); await db.SaveChangesAsync();
        return Created("/api/results/me", result);
    }
    [HttpGet("me"), Authorize(Roles = "Participant")]
    public async Task<IActionResult> Mine() => Ok(await db.Results.Include(r => r.Enrolment).ThenInclude(e => e!.Category).ThenInclude(c => c!.Event)
        .Where(r => r.Enrolment!.ParticipantId == UserId)
        .Select(r => new { r.ResultId, r.FinishTimeMs, r.OverallPlacement, r.RecordedAt,
            Event = r.Enrolment!.Category!.Event!.Title, Category = r.Enrolment.Category.CategoryName }).ToListAsync());
}
