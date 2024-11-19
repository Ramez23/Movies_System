using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Movies_System.Data;
using Movies_System.Models;

namespace Movies_System.Services
{
    public class HallService : IHallService
    {
        private readonly ApplicationDbContext _context;

        public HallService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddHallAsync(Hall hall)
        {
            _context.Halls.Add(hall);
            await _context.SaveChangesAsync();

            var seats = new List<Seat>();
            for (int i = 1; i <= hall.Capacity; i++)
            {
                seats.Add(new Seat
                {
                    SeatNumber = i,
                    HallId = hall.HallId,
                    Status = "Free"
                });
            }
            _context.Seats.AddRange(seats);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateHallAsync(Hall hall)
        {
            _context.Halls.Update(hall);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteHallAsync(int hallId)
        {
            var hall = await _context.Halls.FindAsync(hallId);
            if (hall != null)
            {
                _context.Halls.Remove(hall);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Hall>> GetAllHallsAsync()
        {
            return await _context.Halls.ToListAsync();
        }
    }
}
