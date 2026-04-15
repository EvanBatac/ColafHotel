using ColafHotel.Helpers;
using ColafHotel.Models;
using Microsoft.EntityFrameworkCore;

namespace ColafHotel.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Reservation> Reservations => Set<Reservation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Reservation>()
            .HasOne(r => r.User)
            .WithMany(u => u.Reservations)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Reservation>()
            .HasOne(r => r.Room)
            .WithMany(rm => rm.Reservations)
            .HasForeignKey(r => r.RoomId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>().HasData(new User
        {
            UserId = 1,
            FullName = "ColafHotel Admin",
            Email = "admin@colafhotel.com",
            PasswordHash = PasswordHasher.HashPasswordForSeed("Admin@123", "colafhotel-admin-seed"),
            Role = Roles.Admin,
            ProfilePicturePath = null,
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });

        modelBuilder.Entity<Room>().HasData(
            new Room
            {
                RoomId = 1,
                RoomName = "Garden Haven",
                RoomType = RoomTypes.Single,
                Description = "A cozy single room with a calming garden-inspired interior and work desk.",
                PricePerNight = 3200m,
                ImagePath = null,
                IsAvailable = true
            },
            new Room
            {
                RoomId = 2,
                RoomName = "City Lights",
                RoomType = RoomTypes.Double,
                Description = "A spacious double room with soft lighting, lounge seating, and city views.",
                PricePerNight = 4800m,
                ImagePath = null,
                IsAvailable = true
            },
            new Room
            {
                RoomId = 3,
                RoomName = "Skyline Suite",
                RoomType = RoomTypes.Suite,
                Description = "A premium suite with a separate sitting area, deluxe bath, and skyline ambiance.",
                PricePerNight = 8500m,
                ImagePath = null,
                IsAvailable = true
            });
    }
}
