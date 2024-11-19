using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Movies_System.Data;
using Movies_System.Models;

namespace Movies_System.Services
{
    public class ReservationService : IReservationService
    {
        private readonly ApplicationDbContext _context;

        public ReservationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<SeatStatus>> GetSeatAvailabilityAsync(int showTimeId)
        {
            var showTime = await _context.ShowTimes
                .Include(st => st.Hall)
                .FirstOrDefaultAsync(st => st.ShowTimeId == showTimeId);
            if (showTime == null)
            {
                throw new Exception("Showtime not found.");
            }

            var reservedSeatIds = await _context.ReservationSeats
                .Where(rs => rs.ShowTimeId == showTimeId)
                .Select(rs => rs.SeatId)
                .ToListAsync();

            return await _context.Seats
                .Where(s => s.HallId == showTime.HallId)
                .Select(s => new SeatStatus     
                {
                    SeatId = s.SeatId,
                    SeatNumber = s.SeatNumber.ToString(),
                    Status = reservedSeatIds.Contains(s.SeatId) ? "Reserved" : "Free"
                })
                .ToListAsync();

        }

        public async Task<Reservation> CreateReservationAsync(int userId, int showTimeId, string seatNumbersInput)
        {
            var showTime = await _context.ShowTimes
                .Include(st => st.Hall)
                .FirstOrDefaultAsync(st => st.ShowTimeId == showTimeId);
            if (showTime == null || showTime.StartTime <= DateTime.Now)
                throw new Exception("Cannot reserve seats for a showtime that has already started.");

            var seatNumbers = seatNumbersInput.Split(',')
                .Select(sn => sn.Trim())
                .Where(sn => int.TryParse(sn, out _)) 
                .Select(int.Parse)
                .ToList();

            if (!seatNumbers.Any())
            {
                throw new Exception("Invalid input. Please enter seat numbers separated by commas.");
            }

            var availableSeats = await _context.Seats
                .Where(s => s.HallId == showTime.HallId)
                .ToListAsync();

            var unavailableSeats = seatNumbers.Where(seatNumber =>
                !availableSeats.Any(s => s.SeatNumber == seatNumber) ||
                _context.ReservationSeats.Any(rs => rs.Seat.SeatNumber == seatNumber && rs.ShowTimeId == showTimeId)
            ).ToList();

            if (unavailableSeats.Any())
            {
                throw new Exception($"Seats not available or invalid: {string.Join(", ", unavailableSeats)}");
            }

            var reservation = new Reservation
            {
                UserId = userId,
                ShowTimeId = showTimeId,
                ReservationDate = DateTime.Now
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            foreach (var seatNumber in seatNumbers)
            {
                var seat = availableSeats.FirstOrDefault(s => s.SeatNumber == seatNumber);
                if (seat != null)
                {
                    _context.ReservationSeats.Add(new ReservationSeat
                    {
                        ReservationId = reservation.ReservationId,
                        SeatId = seat.SeatId,
                        ShowTimeId = showTimeId
                    });
                }
            }

            await _context.SaveChangesAsync();
            return reservation;
        }

        public async Task CancelReservationAsync(int reservationId)
        {
            var reservation = await _context.Reservations
                .Include(r => r.ShowTime)
                .Include(r => r.ReservationSeats)
                .FirstOrDefaultAsync(r => r.ReservationId == reservationId);

            if (reservation == null)
                throw new Exception("Reservation not found.");
            if (reservation.ShowTime.StartTime <= DateTime.Now)
                throw new Exception("Cannot cancel a reservation after the showtime has started.");

            _context.ReservationSeats.RemoveRange(reservation.ReservationSeats);
            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Reservation>> GetUserReservationsAsync(int userId)
        {
            return await _context.Reservations
                .Include(r => r.ShowTime)
                    .ThenInclude(st => st.Movie)
                .Include(r => r.ShowTime)
                    .ThenInclude(st => st.Hall)
                .Include(r => r.ReservationSeats)
                    .ThenInclude(rs => rs.Seat)
                .Where(r => r.UserId == userId && r.ShowTime.StartTime > DateTime.Now)
                .ToListAsync();
        }

        public async Task<List<UserReservations>> GetAllReservationsAsync()
        {
            var reservations = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.ShowTime)
                    .ThenInclude(st => st.Movie)
                .Include(r => r.ShowTime)
                    .ThenInclude(st => st.Hall)
                .Include(r => r.ReservationSeats)
                    .ThenInclude(rs => rs.Seat)
                .ToListAsync();

            return reservations
                .GroupBy(r => r.User)
                .Select(g => new UserReservations
                {
                    UserId = g.Key.Id,
                    Name = g.Key.Name, 
                    Reservations = g.Select(res => new ReservationDetails
                    {
                        ReservationId = res.ReservationId,
                        MovieTitle = res.ShowTime.Movie.Title,
                        HallName = res.ShowTime.Hall.Name,
                        ShowTime = res.ShowTime.StartTime,
                        ReservedSeats = res.ReservationSeats.Select(rs => rs.Seat.SeatNumber).ToList()
                    }).ToList()
                })
                .ToList();
        }

    }

    public class SeatStatus
    {
        public int SeatId { get; set; }
        public string SeatNumber { get; set; }
        public string Status { get; set; }
    }
    public class UserReservations
    {
        public int UserId { get; set; }
        public string Name { get; set; } 
        public List<ReservationDetails> Reservations { get; set; }
    }
    public class ReservationDetails
    {
        public int ReservationId { get; set; }
        public string MovieTitle { get; set; }
        public string HallName { get; set; }
        public DateTime ShowTime { get; set; }
        public List<int> ReservedSeats { get; set; }
    }

}
