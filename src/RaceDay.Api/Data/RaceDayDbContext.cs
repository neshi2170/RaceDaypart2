using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;

namespace RaceDay.Api.Data;
public class RaceDayDbContext(DbContextOptions<RaceDayDbContext> options) : DbContext(options)
{
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<RouteInfo> Routes => Set<RouteInfo>();
    public DbSet<Enrolment> Enrolments => Set<Enrolment>();
    public DbSet<Result> Results => Set<Result>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Role>().HasIndex(x => x.RoleName).IsUnique();
        b.Entity<Role>().Property(x => x.RoleName).HasMaxLength(50).IsRequired();
        b.Entity<User>().HasIndex(x => x.Email).IsUnique();
        b.Entity<User>().Property(x => x.Email).HasMaxLength(255).IsRequired();
        b.Entity<User>().Property(x => x.PasswordHash).IsRequired();
        b.Entity<User>().HasOne(x => x.Role).WithMany(x => x.Users).HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Event>().HasOne(x => x.Organiser).WithMany(x => x.Events).HasForeignKey(x => x.OrganiserId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Event>().Property(x => x.Title).HasMaxLength(200).IsRequired();
        b.Entity<Event>().Property(x => x.EventType).HasMaxLength(20).IsRequired();
        b.Entity<Category>().Property(x => x.DistanceKm).HasPrecision(5, 2);
        b.Entity<Category>().Property(x => x.EntryFee).HasPrecision(10, 2);
        b.Entity<Category>().HasOne(x => x.Event).WithMany(x => x.Categories).HasForeignKey(x => x.EventId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<RouteInfo>().HasKey(x => x.RouteId);
        b.Entity<RouteInfo>().HasIndex(x => x.CategoryId).IsUnique();
        b.Entity<RouteInfo>().HasOne(x => x.Category).WithOne(x => x.Route).HasForeignKey<RouteInfo>(x => x.CategoryId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<Enrolment>().HasIndex(x => new { x.ParticipantId, x.CategoryId }).IsUnique();
        b.Entity<Enrolment>().HasOne(x => x.Participant).WithMany(x => x.Enrolments).HasForeignKey(x => x.ParticipantId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Enrolment>().HasOne(x => x.Category).WithMany(x => x.Enrolments).HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Result>().HasIndex(x => x.EnrolmentId).IsUnique();
        b.Entity<Result>().HasOne(x => x.Enrolment).WithOne(x => x.Result).HasForeignKey<Result>(x => x.EnrolmentId).OnDelete(DeleteBehavior.Cascade);

        b.Entity<Role>().HasData(
            new Role { RoleId = 1, RoleName = "Organiser" },
            new Role { RoleId = 2, RoleName = "Participant" });
    }
}
