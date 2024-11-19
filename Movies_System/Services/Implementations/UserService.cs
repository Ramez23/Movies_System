using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Movies_System.Data;
using Movies_System.Models;

namespace Movies_System.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task RegisterUserAsync(string name, string email, string password, string confirmPassword, string phoneNumber, UserRole role = UserRole.Ordinary)
        {
            if (!await IsValidEmailAsync(email))
            {
                throw new ArgumentException("Invalid email format.");
            }

            if (await EmailExistsAsync(email))
            {
                throw new ArgumentException("Email already registered.");
            }

            if (password != confirmPassword)
            {
                throw new ArgumentException("Passwords do not match.");
            }

            var hashedPassword = HashPassword(password);
            var user = new User
            {
                Name = name,
                Email = email,
                Password = hashedPassword,
                ConfirmPassword = hashedPassword,
                PhoneNumber = phoneNumber,
                Role = role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task<User> LoginAsync(string email, string password)
        {
            var hashedPassword = HashPassword(password);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email && u.Password == hashedPassword);
            if (user == null)
            {
                throw new ArgumentException("Invalid email or password.");
            }

            return user;
        }

        public async Task UpdateUserAsync(int userId, string name, string email, string phoneNumber)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("User not found.");
            }

            if (!string.IsNullOrWhiteSpace(email) && !await IsValidEmailAsync(email))
            {
                throw new ArgumentException("Invalid email format.");
            }

            if (!string.IsNullOrWhiteSpace(name)) user.Name = name;
            if (!string.IsNullOrWhiteSpace(email)) user.Email = email;
            if (!string.IsNullOrWhiteSpace(phoneNumber)) user.PhoneNumber = phoneNumber;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteUserAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("User not found.");
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsValidEmailAsync(string email)
        {
            try
            {
                await Task.Delay(10); // Simulate asynchronous operation
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }

        private string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }
    }
}
