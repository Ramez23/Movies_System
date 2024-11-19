using Movies_System.Data;
using Movies_System.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Movies_System.Services
{
    public class ShowtimeService : IShowtimeService
    {
        private readonly ApplicationDbContext _context;

        public ShowtimeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddShowtimeAsync(ShowTime showtime)
        {
            _context.ShowTimes.Add(showtime);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateShowtimeAsync(ShowTime showtime)
        {
            _context.ShowTimes.Update(showtime);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteShowtimeAsync(int showtimeId)
        {
            var showtime = await _context.ShowTimes.FindAsync(showtimeId);
            if (showtime != null)
            {
                _context.ShowTimes.Remove(showtime);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<ShowTime>> GetUpcomingShowtimesAsync()
        {
            return await _context.ShowTimes
                .Include(st => st.Movie)
                .Include(st => st.Hall)
                .Where(st => st.StartTime > DateTime.Now)
                .ToListAsync();
        }

        public async Task<List<ShowTime>> GetShowtimesByGenreAsync(string genre)
        {
            if (Enum.TryParse(typeof(MovieGenre), genre, true, out var genreEnum))
            {
                return await _context.ShowTimes
                    .Include(st => st.Movie)
                    .Include(st => st.Hall)
                    .Where(st => st.Movie.Genre == (MovieGenre)genreEnum && st.StartTime > DateTime.Now)
                    .ToListAsync();
            }
            else
            {
                throw new ArgumentException("Invalid genre selected.");
            }
        }

        public async Task<List<string>> GetAllGenresAsync()
        {
            return Enum.GetNames(typeof(MovieGenre)).ToList();
        }
        public async Task<ShowTime> GetShowtimesByHallAndTimeAsync(int hallId, DateTime startTime, DateTime endTime)
        {
            var showtimes = await _context.ShowTimes
                .Include(st => st.Movie)
                .Include(st => st.Hall)
                .Where(st => st.HallId == hallId && st.StartTime < endTime)
                .ToListAsync();

            return showtimes.FirstOrDefault(st =>
                st.StartTime < endTime &&
                st.StartTime.AddMinutes(st.Movie.Duration) > startTime);
        }


    }
}
