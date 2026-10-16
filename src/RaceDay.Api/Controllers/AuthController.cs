using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers;
[ApiController]
[Route("api/auth")]
public class AuthController(RaceDayDbContext db, PasswordService passwords, TokenService tokens) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<UserResponse>> Register(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName) ||
            string.IsNullOrWhiteSpace(request.Email) || request.Password.Length < 8)
            return BadRequest(new { message = "Names/email are required and password must be at least 8 characters." });
        var email = request.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(u => u.Email == email)) return Conflict(new { message = "Email is already registered." });
        var roleName = request.Role.Trim();
        if (roleName is not ("Organiser" or "Participant")) return BadRequest(new { message = "Role must be Organiser or Participant." });
        var role = await db.Roles.SingleOrDefaultAsync(r => r.RoleName == roleName);
        if (role is null) return StatusCode(500, new { message = "Required roles are not seeded. Apply migrations first." });
        var user = new User { FirstName = request.FirstName.Trim(), LastName = request.LastName.Trim(), Email = email,
            PasswordHash = passwords.Hash(request.Password), RoleId = role.RoleId };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return Created("/api/users/profile", new UserResponse(user.UserId, user.FirstName, user.LastName, user.Email, role.RoleName, user.Phone));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users.Include(u => u.Role).SingleOrDefaultAsync(u => u.Email == email);
        if (user is null || !passwords.Verify(request.Password, user.PasswordHash))
            return Unauthorized(new { message = "Invalid email or password." });
        var (token, expires) = tokens.Create(user);
        return Ok(new AuthResponse(token, expires, new UserResponse(user.UserId, user.FirstName, user.LastName, user.Email, user.Role!.RoleName, user.Phone)));
    }
}
