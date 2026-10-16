namespace RaceDay.Api.Dtos;
public record RegisterRequest(string FirstName, string LastName, string Email, string Password, string Role);
public record LoginRequest(string Email, string Password);
public record UserResponse(int UserId, string FirstName, string LastName, string Email, string Role, string? Phone);
public record AuthResponse(string Token, DateTime ExpiresAtUtc, UserResponse User);
public record UpdateProfileRequest(string FirstName, string LastName, string? Phone);
