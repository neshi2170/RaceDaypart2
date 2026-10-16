using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;
namespace RaceDay.Api.Controllers;
[ApiController, Authorize(Roles = "Participant")]
[Route("api/enrolments")]
public class EnrolmentsController(RaceDayDbContext db) : ControllerBase
{
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    [HttpPost]
    public async Task<IActionResult> Create(EnrolmentRequest r)
    {
        if (string.IsNullOrWhiteSpace(r.EmergencyContact)) return BadRequest("Emergency contact is required.");
        var category = await db.Categories.Include(c => c.Event).SingleOrDefaultAsync(c => c.CategoryId == r.CategoryId);
        if (category is null) return NotFound("Category not found.");
        if (category.Event!.EventDate <= DateTime.UtcNow) return BadRequest("Cannot enrol in a past event.");
        if (await db.Enrolments.AnyAsync(x => x.ParticipantId == UserId && x.CategoryId == r.CategoryId))
            return Conflict("You are already enrolled in this category.");
        var e = new Enrolment { ParticipantId = UserId, CategoryId = r.CategoryId, EmergencyContact = r.EmergencyContact.Trim() };
        db.Enrolments.Add(e);
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException) { return Conflict("Enrolment conflicts with an existing record."); }
        return Created($"/api/enrolments/me", new { e.EnrolmentId, e.ParticipantId, e.CategoryId, e.EnrolmentDate });
    }
    [HttpGet("me")]
    public async Task<IActionResult> Mine() => Ok(await db.Enrolments.Include(x => x.Category).ThenInclude(c => c!.Event)
        .Include(x => x.Result).Where(x => x.ParticipantId == UserId)
        .Select(x => new { x.EnrolmentId, x.EnrolmentDate, Category = x.Category!.CategoryName, Event = x.Category.Event!.Title,
            EventDate = x.Category.Event.EventDate, x.Result }).ToListAsync());
}
