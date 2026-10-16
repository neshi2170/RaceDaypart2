using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;
namespace RaceDay.Api.Controllers;
[ApiController]
[Route("api/events")]
public class EventsController(RaceDayDbContext db) : ControllerBase
{
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> List([FromQuery] string? eventType, [FromQuery] DateTime? from)
    {
        var q = db.Events.Include(e => e.Categories).AsQueryable();
        if (!string.IsNullOrWhiteSpace(eventType)) q = q.Where(e => e.EventType == eventType);
        if (from.HasValue) q = q.Where(e => e.EventDate >= from.Value);
        return Ok(await q.OrderBy(e => e.EventDate).ToListAsync());
    }
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> Get(int id)
    {
        var item = await db.Events.Include(e => e.Categories).ThenInclude(c => c.Route).SingleOrDefaultAsync(e => e.EventId == id);
        return item is null ? NotFound() : Ok(item);
    }
    [HttpPost, Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Create(EventRequest r)
    {
        if (!ValidEvent(r)) return BadRequest(new { message = "Title, location, future date and Run/Walk/Cycle event type are required." });
        var e = new Event { OrganiserId = UserId, Title = r.Title.Trim(), Description = r.Description, Location = r.Location.Trim(), EventDate = r.EventDate, EventType = r.EventType };
        db.Events.Add(e); await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = e.EventId }, e);
    }
    [HttpPut("{id:int}"), Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Update(int id, EventRequest r)
    {
        if (!ValidEvent(r)) return BadRequest("Invalid event details.");
        var e = await db.Events.FindAsync(id);
        if (e is null) return NotFound();
        if (e.OrganiserId != UserId) return Forbid();
        e.Title = r.Title.Trim(); e.Description = r.Description; e.Location = r.Location.Trim(); e.EventDate = r.EventDate; e.EventType = r.EventType;
        await db.SaveChangesAsync(); return Ok(e);
    }
    [HttpDelete("{id:int}"), Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Delete(int id)
    {
        var e = await db.Events.FindAsync(id);
        if (e is null) return NotFound();
        if (e.OrganiserId != UserId) return Forbid();
        db.Events.Remove(e); await db.SaveChangesAsync(); return NoContent();
    }
    [HttpPost("{id:int}/categories"), Authorize(Roles = "Organiser")]
    public async Task<IActionResult> AddCategory(int id, CategoryRequest r)
    {
        var e = await db.Events.FindAsync(id);
        if (e is null) return NotFound();
        if (e.OrganiserId != UserId) return Forbid();
        if (string.IsNullOrWhiteSpace(r.CategoryName) || r.DistanceKm <= 0 || r.EntryFee < 0) return BadRequest("Category name, positive distance and non-negative fee are required.");
        var c = new Category { EventId = id, CategoryName = r.CategoryName.Trim(), DistanceKm = r.DistanceKm, EntryFee = r.EntryFee, AgeGroup = r.AgeGroup };
        db.Categories.Add(c); await db.SaveChangesAsync();
        return Created($"/api/categories/{c.CategoryId}", c);
    }
    [HttpGet("{id:int}/enrolments"), Authorize(Roles = "Organiser")]
    public async Task<IActionResult> EventEnrolments(int id)
    {
        var e = await db.Events.FindAsync(id);
        if (e is null) return NotFound();
        if (e.OrganiserId != UserId) return Forbid();
        var rows = await db.Enrolments.Include(x => x.Participant).Include(x => x.Category)
            .Where(x => x.Category!.EventId == id).Select(x => new { x.EnrolmentId, x.EnrolmentDate, x.EmergencyContact,
                Participant = x.Participant!.FirstName + " " + x.Participant.LastName, x.Participant.Email, Category = x.Category!.CategoryName }).ToListAsync();
        return Ok(rows);
    }
    private static bool ValidEvent(EventRequest r) =>
        !string.IsNullOrWhiteSpace(r.Title) && !string.IsNullOrWhiteSpace(r.Location) &&
        r.EventDate > DateTime.UtcNow.AddMinutes(-5) && new[] { "Run", "Walk", "Cycle" }.Contains(r.EventType, StringComparer.OrdinalIgnoreCase);
}
