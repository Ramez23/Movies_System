using System.Collections.Generic;
using System.Threading.Tasks;
using Movies_System.Models;

namespace Movies_System.Services
{
    public interface IMovieService
    {
        Task AddMovieAsync(Movie movie);
        Task UpdateMovieAsync(Movie movie);
        Task DeleteMovieAsync(int movieId);
        Task<List<Movie>> GetAllMoviesAsync();

    }
}
