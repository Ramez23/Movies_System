using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Movies_System.Models;

namespace Movies_System.Services
{
    public interface IShowtimeService
    {
        Task<List<ShowTime>> GetUpcomingShowtimesAsync();
        Task<List<ShowTime>> GetShowtimesByGenreAsync(string genre);
        Task<List<string>> GetAllGenresAsync();
        Task AddShowtimeAsync(ShowTime showtime);
        Task UpdateShowtimeAsync(ShowTime showtime);
        Task DeleteShowtimeAsync(int showtimeId);
        Task<ShowTime> GetShowtimesByHallAndTimeAsync(int hallId, DateTime startTime, DateTime endTime);

    }
}
