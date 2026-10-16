using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
namespace RaceDay.Api.Controllers;
[ApiController, Authorize]
[Route("api/users")]
public class UsersController(RaceDayDbContext db) : ControllerBase
{
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    [HttpGet("profile")]
    public async Task<ActionResult<UserResponse>> GetProfile()
    {
        var u = await db.Users.Include(x => x.Role).SingleOrDefaultAsync(x => x.UserId == UserId);
        if (u is null) return NotFound();
        return Ok(new UserResponse(u.UserId, u.FirstName, u.LastName, u.Email, u.Role!.RoleName, u.Phone));
    }
    [HttpPut("profile")]
    public async Task<ActionResult<UserResponse>> UpdateProfile(UpdateProfileRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName)) return BadRequest("First and last name are required.");
        var u = await db.Users.Include(x => x.Role).SingleOrDefaultAsync(x => x.UserId == UserId);
        if (u is null) return NotFound();
        u.FirstName = request.FirstName.Trim(); u.LastName = request.LastName.Trim(); u.Phone = request.Phone;
        await db.SaveChangesAsync();
        return Ok(new UserResponse(u.UserId, u.FirstName, u.LastName, u.Email, u.Role!.RoleName, u.Phone));
    }
}
