using Microsoft.EntityFrameworkCore;
using Movies_System.Models;

namespace Movies_System.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Movie> Movies { get; set; }
        public DbSet<Hall> Halls { get; set; }
        public DbSet<Seat> Seats { get; set; }
        public DbSet<ShowTime> ShowTimes { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<ReservationSeat> ReservationSeats { get; set; } 

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // User entity
            modelBuilder.Entity<User>()
                .HasKey(u => u.Id);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Reservations)
                .WithOne(r => r.User)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Movie entity
            modelBuilder.Entity<Movie>()
                .HasKey(m => m.MovieId);

            modelBuilder.Entity<Movie>()
                .HasMany(m => m.ShowTimes)
                .WithOne(st => st.Movie)
                .HasForeignKey(st => st.MovieId)
                .OnDelete(DeleteBehavior.Cascade);

            // Hall entity
            modelBuilder.Entity<Hall>()
                .HasKey(h => h.HallId);

            modelBuilder.Entity<Hall>()
                .HasMany(h => h.Seats)
                .WithOne(s => s.Hall)
                .HasForeignKey(s => s.HallId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Hall>()
                .HasMany(h => h.ShowTimes)
                .WithOne(st => st.Hall)
                .HasForeignKey(st => st.HallId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seat entity
            modelBuilder.Entity<Seat>()
                .HasKey(s => s.SeatId);

            modelBuilder.Entity<Seat>()
                .HasMany(s => s.ReservationSeats)
                .WithOne(rs => rs.Seat)
                .HasForeignKey(rs => rs.SeatId)
                .OnDelete(DeleteBehavior.Restrict); 

            // ShowTime entity
            modelBuilder.Entity<ShowTime>()
                .HasKey(st => st.ShowTimeId);

            modelBuilder.Entity<ShowTime>()
                .HasMany(st => st.ReservationSeats)
                .WithOne(rs => rs.ShowTime)
                .HasForeignKey(rs => rs.ShowTimeId)
                .OnDelete(DeleteBehavior.Restrict); 

            // Reservation entity
            modelBuilder.Entity<Reservation>()
                .HasKey(r => r.ReservationId);

            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reservations)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Reservation>()
                .HasMany(r => r.ReservationSeats)
                .WithOne(rs => rs.Reservation)
                .HasForeignKey(rs => rs.ReservationId)
                .OnDelete(DeleteBehavior.Restrict);

            // ReservationSeat entity
            modelBuilder.Entity<ReservationSeat>()
                .HasKey(rs => rs.Id);

            modelBuilder.Entity<ReservationSeat>()
                .HasIndex(rs => new { rs.SeatId, rs.ShowTimeId })
                .IsUnique();

            modelBuilder.Entity<ReservationSeat>()
                .HasOne(rs => rs.Reservation)
                .WithMany(r => r.ReservationSeats)
                .HasForeignKey(rs => rs.ReservationId)
                .OnDelete(DeleteBehavior.Restrict); 

            modelBuilder.Entity<ReservationSeat>()
                .HasOne(rs => rs.Seat)
                .WithMany(s => s.ReservationSeats)
                .HasForeignKey(rs => rs.SeatId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ReservationSeat>()
                .HasOne(rs => rs.ShowTime)
                .WithMany(st => st.ReservationSeats)
                .HasForeignKey(rs => rs.ShowTimeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
