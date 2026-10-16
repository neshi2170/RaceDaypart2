using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;
namespace RaceDay.Api.Controllers;
[ApiController, Authorize(Roles = "Organiser")]
[Route("api/categories/{categoryId:int}/route")]
public class RoutesController(RaceDayDbContext db) : ControllerBase
{
    [HttpGet, AllowAnonymous]
    public async Task<IActionResult> Get(int categoryId)
    {
        var route = await db.Routes.SingleOrDefaultAsync(r => r.CategoryId == categoryId);
        return route is null ? NotFound() : Ok(route);
    }
    [HttpPut]
    public async Task<IActionResult> Upsert(int categoryId, RouteRequest r)
    {
        if (!await db.Categories.AnyAsync(c => c.CategoryId == categoryId)) return NotFound("Category not found.");
        if (r.ElevationGainMeters is < 0) return BadRequest("Elevation gain cannot be negative.");
        var route = await db.Routes.SingleOrDefaultAsync(x => x.CategoryId == categoryId);
        if (route is null) { route = new RouteInfo { CategoryId = categoryId }; db.Routes.Add(route); }
        route.GpxDataUrl = r.GpxDataUrl; route.ElevationGainMeters = r.ElevationGainMeters;
        route.StartCoordinates = r.StartCoordinates; route.WeatherLookupReference = r.WeatherLookupReference;
        await db.SaveChangesAsync(); return Ok(route);
    }
}
