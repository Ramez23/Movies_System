using System.Collections.Generic;
using System.Threading.Tasks;
using Movies_System.Models;

namespace Movies_System.Services
{
    public interface IHallService
    {
        Task AddHallAsync(Hall hall);
        Task UpdateHallAsync(Hall hall);
        Task DeleteHallAsync(int hallId);
        Task<List<Hall>> GetAllHallsAsync();
    }
}
