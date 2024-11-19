using System.Threading.Tasks;
using Movies_System.Models;

namespace Movies_System.Services
{
    public interface IUserService
    {
        Task RegisterUserAsync(string name, string email, string password, string confirmPassword, string phoneNumber, UserRole role = UserRole.Ordinary);
        Task<User> LoginAsync(string email, string password);
        Task UpdateUserAsync(int userId, string name, string email, string phoneNumber);
        Task DeleteUserAsync(int userId);
        Task<bool> EmailExistsAsync(string email);
        Task<bool> IsValidEmailAsync(string email);
    }
}
