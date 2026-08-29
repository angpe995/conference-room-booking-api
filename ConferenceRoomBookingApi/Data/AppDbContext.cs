using Microsoft.EntityFrameworkCore;
using ConferenceRoomBookingApi.Models;

namespace ConferenceRoomBookingApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Room> Rooms => Set<Room>();
        public DbSet<Service> Services => Set<Service>();
        public DbSet<Booking> Bookings => Set<Booking>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Room>()
                .HasMany(room => room.Services)
                .WithOne()
                .HasForeignKey("RoomId")
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Room>()
                .HasMany<Booking>()
                .WithOne()
                .HasForeignKey(booking => booking.RoomId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

}