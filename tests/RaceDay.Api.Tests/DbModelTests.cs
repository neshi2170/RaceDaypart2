using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using Xunit;
namespace RaceDay.Api.Tests;
public class DbModelTests
{
    [Fact]
    public async Task SeededRolesAndEventRelationshipsCanBeStored()
    {
        var options = new DbContextOptionsBuilder<RaceDayDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        await using var db = new RaceDayDbContext(options);
        await db.Database.EnsureCreatedAsync();
        Assert.Equal(2, await db.Roles.CountAsync());
        var organiserRole = await db.Roles.SingleAsync(r => r.RoleName == "Organiser");
        var user = new User { FirstName = "Test", LastName = "Organiser", Email = "organiser@example.test", PasswordHash = "hashed", RoleId = organiserRole.RoleId };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        var ev = new RaceDay.Api.Models.Event { OrganiserId = user.UserId, Title = "Test Run", Location = "Johannesburg", EventDate = DateTime.UtcNow.AddDays(10), EventType = "Run" };
        db.Events.Add(ev);
        await db.SaveChangesAsync();
        Assert.Equal(user.UserId, (await db.Events.SingleAsync()).OrganiserId);
    }
}
