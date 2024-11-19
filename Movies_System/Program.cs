using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Movies_System.Data;
using Movies_System.Models;
using Movies_System.Services;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Movies_System
{
    class Program
    {
        private readonly IUserService _userService;
        private readonly IShowtimeService _showtimeService;
        private readonly IHallService _hallService;
        private readonly IMovieService _movieService;
        private readonly IReservationService _reservationService;
        private readonly ApplicationDbContext _context;

        private User _loggedInUser;

        public Program(IUserService userService, IShowtimeService showtimeService, IMovieService movieService, IHallService hallService , IReservationService reservationService, ApplicationDbContext context)
        {
            _userService = userService;
            _showtimeService = showtimeService;
            _movieService = movieService;
            _hallService = hallService;
            _reservationService = reservationService;
            _context = context;
        }


        static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var host = Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Warning);
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<IConfiguration>(configuration);
                    services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

                    services.AddScoped<IUserService, UserService>();
                    services.AddScoped<IShowtimeService, ShowtimeService>();
                    services.AddScoped<IMovieService, MovieService>();
                    services.AddScoped<IHallService, HallService>();
                    services.AddScoped<IReservationService, ReservationService>();
                    services.AddTransient<Program>();
                })
                .Build();

            using var scope = host.Services.CreateScope();
            var program = scope.ServiceProvider.GetRequiredService<Program>();
            //await program.AdminDataAsync();
            await program.RunAsync();
        }

        private async Task AdminDataAsync()
        {
            var adminEmail = "admin@example.com";

            if (!await _context.Users.AnyAsync(u => u.Email == adminEmail))
            {
                var adminUser = new User
                {
                    Name = "Admin User",
                    Email = adminEmail,
                    Password = HashPassword("Admin@123"),
                    ConfirmPassword = HashPassword("Admin@123"),
                    PhoneNumber = "1234567890",
                    Role = UserRole.Admin
                };
                _context.Users.Add(adminUser);
                await _context.SaveChangesAsync();
            }

            Console.WriteLine("Data seeded successfully.");

        }

        private string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }

        private async Task RunAsync()
        {
            bool running = true;
            while (running)
            {
                Console.WriteLine("\nSelect operation:");
                Console.WriteLine("1 - Register");
                Console.WriteLine("2 - Login");
                Console.WriteLine("3 - Exit");
                Console.Write("Your choice: ");
                var option = Console.ReadLine();

                switch (option)
                {
                    case "1":
                        await RegisterAsync();
                        break;
                    case "2":
                        await LoginAsync();
                        break;
                    case "3":
                        running = false;
                        break;
                    default:
                        Console.WriteLine("Invalid selection.");
                        break;
                }
            }
        }

        private async Task RegisterAsync()
        {
            string name;
            string email;
            string password;
            string confirmPassword;
            string phoneNumber;

            while (true)
            {
                try
                {
                    while (true)
                    {
                        Console.Write("Enter Name: ");
                        name = Console.ReadLine();
                        if (string.IsNullOrWhiteSpace(name))
                        {
                            Console.WriteLine("Name cannot be empty. Please try again.");
                            continue;
                        }
                        break;
                    }

                    while (true)
                    {
                        Console.Write("Enter Email: ");
                        email = Console.ReadLine();
                        if (string.IsNullOrWhiteSpace(email))
                        {
                            Console.WriteLine("Email cannot be empty. Please try again.");
                            continue;
                        }
                        if (!await _userService.IsValidEmailAsync(email))
                        {
                            Console.WriteLine("Invalid email format. Please try again.");
                            continue;
                        }
                        if (await _userService.EmailExistsAsync(email))
                        {
                            Console.WriteLine("Email already registered. Please try another email.");
                            continue;
                        }
                        break;
                    }

                    while (true)
                    {
                        Console.Write("Enter Password: ");
                        password = Console.ReadLine();
                        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
                        {
                            Console.WriteLine("Password must be at least 6 characters long. Please try again.");
                            continue;
                        }
                        break;
                    }

                    while (true)
                    {
                        Console.Write("Confirm Password: ");
                        confirmPassword = Console.ReadLine();
                        if (password != confirmPassword)
                        {
                            Console.WriteLine("Passwords do not match. Please try again.");
                            continue;
                        }
                        break;
                    }

                    Console.Write("Enter Phone Number: ");
                    phoneNumber = Console.ReadLine();

                    await _userService.RegisterUserAsync(name, email, password, confirmPassword, phoneNumber);
                    Console.WriteLine("User registered successfully.");
                    break;
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine($"Registration error: {ex.Message}");
                }
                catch (InvalidOperationException ex)
                {
                    Console.WriteLine($"Registration error: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unexpected error occurred during registration: {ex.Message}");
                }
            }
        }

        private async Task LoginAsync()
        {
            while (true)
            {
                try
                {
                    Console.Write("Enter Email: ");
                    var email = Console.ReadLine();

                    Console.Write("Enter Password: ");
                    var password = Console.ReadLine();

                    _loggedInUser = await _userService.LoginAsync(email, password);
                    if (_loggedInUser != null)
                    {
                        Console.WriteLine($"Welcome, {_loggedInUser.Name}!");
                        await ShowMenuAsync();
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Invalid email or password.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Login error: {ex.Message}");
                }
            }
        }

        private async Task ShowMenuAsync()
        {
            bool running = true;
            while (running)
            {
                if (_loggedInUser.Role == UserRole.Admin)
                {
                    Console.WriteLine("\nAdmin Menu:");
                    Console.WriteLine("1 - View All Users");
                    Console.WriteLine("2 - View Upcoming Showtimes");
                    Console.WriteLine("3 - Browse Movies by Genre");
                    Console.WriteLine("4 - Manage Movies");
                    Console.WriteLine("5 - Manage Halls");
                    Console.WriteLine("6 - Manage Showtimes");
                    Console.WriteLine("7 - View All Reservations");
                    Console.WriteLine("8 - Update User Information");
                    Console.WriteLine("9 - Delete User");
                    Console.WriteLine("10 - Logout");
                }
                else
                {
                    Console.WriteLine("\nUser Menu:");
                    Console.WriteLine("1 - View Upcoming Showtimes");
                    Console.WriteLine("2 - Browse Movies by Genre");
                    Console.WriteLine("3 - Make a Reservation");
                    Console.WriteLine("4 - View My Reservations");
                    Console.WriteLine("5 - Cancel a Reservation");
                    Console.WriteLine("6 - Update Your Information");
                    Console.WriteLine("7 - Delete Your Account");
                    Console.WriteLine("8 - Logout");
                }

                Console.Write("Your choice: ");
                var option = Console.ReadLine();

                switch (option)
                {
                    case "1":
                        if (_loggedInUser.Role == UserRole.Admin)
                            await ViewAllUsersAsync();
                        else
                            await ViewUpcomingShowtimesAsync();
                        break;
                    case "2":
                        if (_loggedInUser.Role == UserRole.Admin)
                            await ViewUpcomingShowtimesAsync();
                        else
                            await BrowseByGenreAsync();
                        break;
                    case "3":
                        if (_loggedInUser.Role == UserRole.Admin)
                            await BrowseByGenreAsync();
                        else
                            await MakeReservationAsync();
                        break;
                    case "4":
                        if (_loggedInUser.Role == UserRole.Admin)
                            await ManageMoviesAsync();
                        else
                        {
                            await ViewMyReservationsAsync();
                        }
                        break;
                    case "5":
                        if (_loggedInUser.Role == UserRole.Admin)
                            await ManageHallsAsync();
                        else
                        {
                            await CancelReservationAsync();
                        }
                        break;
                    case "6":
                        if (_loggedInUser.Role == UserRole.Admin)
                            await ManageShowtimesAsync();
                        else
                        {
                            await UpdateUserAsync(_loggedInUser);
                        }
                        break;
                    case "7":
                        if (_loggedInUser.Role == UserRole.Admin)
                            await ViewAllReservationsAsync();
                        else
                        {
                            if (await DeleteUserAsync(_loggedInUser))
                                running = false;
                        }
                        break;
                    case "8":
                        if (_loggedInUser.Role == UserRole.Admin)
                        {
                            await UpdateSpecificUserAsync();
                        }
                        else
                        {
                            Console.WriteLine("Logging out...");
                            running = false;
                        }
                        break;
                    case "9":
                        if (_loggedInUser.Role == UserRole.Admin)
                        {
                            if (await DeleteSpecificUserAsync())
                                running = false;
                        }
                        else
                        {
                            Console.WriteLine("Invalid selection.");
                        }
                        break;
                    case "10":
                        if (_loggedInUser.Role == UserRole.Admin)
                        {
                            Console.WriteLine("Logging out...");
                            running = false;
                        }
                        else
                        {
                            Console.WriteLine("Invalid selection.");
                        }
                        break;
                    default:
                        Console.WriteLine("Invalid selection.");
                        break;
                }
            }
        }

        private async Task MakeReservationAsync()
        {
            await ViewUpcomingShowtimesAsync();
            Console.WriteLine("\nEnter the showtime ID for your reservation:");
            if (!int.TryParse(Console.ReadLine(), out int showTimeId))
            {
                Console.WriteLine("Invalid showtime ID.");
                return;
            }

            try
            {
                var seats = await _reservationService.GetSeatAvailabilityAsync(showTimeId);
                if (!seats.Any())
                {
                    Console.WriteLine("No seats available for this showtime.");
                    return;
                }

                Console.WriteLine("\nAvailable Seats:");
                foreach (var seat in seats)
                {
                    Console.WriteLine($"Seat Number: {seat.SeatNumber}, Status: {seat.Status}");
                }
                Console.WriteLine("\nEnter seat numbers (comma-separated) to reserve:");
                var seatInput = Console.ReadLine();
                var seatIds = seatInput.Split(',')
                    .Select(s => int.TryParse(s.Trim(), out int seatId) ? seatId : -1)
                    .Where(id => id != -1)
                    .ToList();

                var seatIdsString = string.Join(",", seatIds);
                var reservation = await _reservationService.CreateReservationAsync(_loggedInUser.Id, showTimeId, seatIdsString);
                Console.WriteLine("Reservation created successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Reservation failed: {ex.Message}");
            }
        }

        private async Task ViewMyReservationsAsync()
        {
            var reservations = await _reservationService.GetUserReservationsAsync(_loggedInUser.Id);
            if (!reservations.Any())
            {
                Console.WriteLine("No upcoming reservations.");
                return;
            }

            Console.WriteLine("\nMy Reservations:");
            foreach (var reservation in reservations)
            {
                Console.WriteLine($"Reservation ID: {reservation.ReservationId}, Movie: {reservation.ShowTime.Movie.Title}, Hall: {reservation.ShowTime.Hall.Name}, Time: {reservation.ShowTime.StartTime}");
                Console.WriteLine("Reserved Seats:");

                if (reservation.ReservationSeats != null)
                {
                    foreach (var seat in reservation.ReservationSeats)
                    {
                        Console.WriteLine($"Seat Number: {seat.Seat.SeatNumber}");
                    }
                }
                else
                {
                    Console.WriteLine("No seats reserved.");
                }
            }
        }

        private async Task CancelReservationAsync()
        {
            await ViewMyReservationsAsync();
            Console.Write("Enter the reservation ID to cancel: ");
            if (!int.TryParse(Console.ReadLine(), out int reservationId))
            {
                Console.WriteLine("Invalid reservation ID.");
                return;
            }

            try
            {
                await _reservationService.CancelReservationAsync(reservationId);
                Console.WriteLine("Reservation canceled successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cancellation failed: {ex.Message}");
            }
        }

        private async Task ViewAllReservationsAsync()
        {
            var allReservations = await _reservationService.GetAllReservationsAsync();

            if (!allReservations.Any())
            {
                Console.WriteLine("No reservations found.");
                return;
            }

            Console.WriteLine("\nAll Reservations:");
            foreach (var userReservations in allReservations)
            {
                Console.WriteLine($"User ID: {userReservations.UserId}, User Name: {userReservations.Name}");
                foreach (var reservation in userReservations.Reservations)
                {
                    Console.WriteLine($"\tReservation ID: {reservation.ReservationId}, Movie: {reservation.MovieTitle}, Hall: {reservation.HallName}, Time: {reservation.ShowTime}");
                    Console.WriteLine($"\tReserved Seats: {string.Join(", ", reservation.ReservedSeats)}");
                }
            }

            Console.Write("\nEnter the Reservation ID to cancel or press Enter to return: ");
            var input = Console.ReadLine();
            if (int.TryParse(input, out int reservationId))
            {
                try
                {
                    await _reservationService.CancelReservationAsync (reservationId);
                    Console.WriteLine("Reservation canceled successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
            else if (!string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("Invalid Reservation ID.");
            }
        }

        private async Task ViewAllUsersAsync()
        {
            var ordinaryUsers = await _context.Users
                .Where(u => u.Role != UserRole.Admin)
                .ToListAsync();

            Console.WriteLine("\nOrdinary Users:");
            foreach (var user in ordinaryUsers)
            {
                Console.WriteLine($"ID: {user.Id}, Name: {user.Name}, Email: {user.Email}, Role: {user.Role}");
            }
        }

        private async Task UpdateSpecificUserAsync()
        {
            await ViewAllUsersAsync();
            Console.Write("Enter the ID of the user to update: ");
            if (int.TryParse(Console.ReadLine(), out int userId))
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user != null)
                {
                    Console.Write("Enter new Name (leave blank to keep current): ");
                    var name = Console.ReadLine();
                    Console.Write("Enter new Email (leave blank to keep current): ");
                    var email = Console.ReadLine();
                    Console.Write("Enter new Phone Number (leave blank to keep current): ");
                    var phoneNumber = Console.ReadLine();

                    if (!string.IsNullOrWhiteSpace(name)) user.Name = name;
                    if (!string.IsNullOrWhiteSpace(email)) user.Email = email;
                    if (!string.IsNullOrWhiteSpace(phoneNumber)) user.PhoneNumber = phoneNumber;

                    _context.Users.Update(user);
                    await _context.SaveChangesAsync();
                    Console.WriteLine("User information updated successfully.");
                }
                else
                {
                    Console.WriteLine("User not found.");
                }
            }
            else
            {
                Console.WriteLine("Invalid user ID.");
            }
        }

        private async Task<bool> DeleteSpecificUserAsync()
        {
            await ViewAllUsersAsync();
            Console.Write("Enter the ID of the user to delete: ");
            if (int.TryParse(Console.ReadLine(), out int userId))
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user != null)
                {
                    _context.Users.Remove(user);
                    await _context.SaveChangesAsync();
                    Console.WriteLine("User deleted successfully.");
                    return true;
                }
                else
                {
                    Console.WriteLine("User not found.");
                    return false;
                }
            }
            else
            {
                Console.WriteLine("Invalid user ID.");
                return false;
            }
        }

        private async Task UpdateUserAsync(User user)
        {
            Console.Write("Enter new Name (leave blank to keep current): ");
            var name = Console.ReadLine();
            Console.Write("Enter new Email (leave blank to keep current): ");
            var email = Console.ReadLine();
            Console.Write("Enter new Phone Number (leave blank to keep current): ");
            var phoneNumber = Console.ReadLine();

            try
            {
                await _userService.UpdateUserAsync(user.Id, name, email, phoneNumber);
                Console.WriteLine("User information updated successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Update failed: {ex.Message}");
            }
        }

        private async Task<bool> DeleteUserAsync(User user)
        {
            Console.Write("Are you sure you want to delete your account? (yes/no): ");
            var confirmation = Console.ReadLine();
            if (confirmation?.ToLower() == "yes")
            {
                try
                {
                    await _userService.DeleteUserAsync(user.Id);
                    Console.WriteLine("User account deleted successfully.");
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Delete failed: {ex.Message}");
                    return false;
                }
            }
            Console.WriteLine("Account deletion cancelled.");
            return false;
        }

        private async Task ViewUpcomingShowtimesAsync()
        {
            var showtimes = await _showtimeService.GetUpcomingShowtimesAsync();
            if (!showtimes.Any())
            {
                Console.WriteLine("No upcoming showtimes available.");
                return;
            }

            Console.WriteLine("\nUpcoming Showtimes:");
            foreach (var showtime in showtimes)
            {
                Console.WriteLine($"ID: {showtime.ShowTimeId}, Movie: {showtime.Movie.Title}, Genre: {showtime.Movie.Genre}, Hall: {showtime.Hall.Name}, Time: {showtime.StartTime}");
            }
        }   

        private async Task BrowseByGenreAsync()
        {
            while (true)
            {
                try
                {
                    var genres = await _showtimeService.GetAllGenresAsync();
                    Console.WriteLine("\nAvailable Genres:");
                    foreach (var genre in genres)
                    {
                        Console.WriteLine($"- {genre}");
                    }

                    Console.Write("\nEnter genre to browse: ");
                    var selectedGenre = Console.ReadLine();

                    if (!Enum.TryParse(typeof(MovieGenre), selectedGenre, true, out var genreEnum))
                    {
                        throw new ArgumentException("Invalid genre selected.");
                    }

                    var genreShowtimes = await _showtimeService.GetShowtimesByGenreAsync(selectedGenre);

                    if (!genreShowtimes.Any())
                    {
                        Console.WriteLine("No movies found for the selected genre.");
                        return;
                    }

                    Console.WriteLine($"\nShowtimes for Genre '{selectedGenre}':");
                    foreach (var showtime in genreShowtimes)
                    {
                        Console.WriteLine($"Movie: {showtime.Movie.Title}, Hall: {showtime.Hall.Name}, Time: {showtime.StartTime}");
                    }
                    break; // Exit loop if successful
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine(ex.Message); // Display error message
                    Console.WriteLine("Please enter a valid genre from the list above.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                    return; // Exit if an unexpected error occurs
                }
            }
        }

        private async Task ManageMoviesAsync()
        {
            await GetMoviesAsync(); 

            Console.WriteLine("\nMovie Management:");
            Console.WriteLine("1 - Add Movie");
            Console.WriteLine("2 - Update Movie");
            Console.WriteLine("3 - Delete Movie");
            Console.Write("Your choice: ");
            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    await AddMovieAsync();
                    break;
                case "2":
                    await UpdateMovieAsync();
                    break;
                case "3":
                    await DeleteMovieAsync();
                    break;
                default:
                    Console.WriteLine("Invalid selection.");
                    break;
            }
        }

        private async Task GetMoviesAsync()
        {
            var movies = await _movieService.GetAllMoviesAsync();
            if (!movies.Any())
            {
                Console.WriteLine("No movies available.");
                return;
            }

            Console.WriteLine("\nExisting Movies:");
            foreach (var movie in movies)
            {
                Console.WriteLine($"ID: {movie.MovieId}, Title: {movie.Title}, Genre: {movie.Genre}, Duration: {movie.Duration} mins, Release Date: {movie.ReleaseDate}, Rating: {movie.Rating}");
            }
        }

        private async Task AddMovieAsync()
        {
            Console.Write("Enter Movie Title: ");
            var title = Console.ReadLine();

            Console.WriteLine("Select Genre:");
            foreach (var genreOption in Enum.GetValues(typeof(MovieGenre)))
            {
                Console.WriteLine($"{(int)genreOption} - {genreOption}");
            }
            Console.Write("Your choice: ");
            if (!Enum.TryParse(Console.ReadLine(), out MovieGenre genre))
            {
                Console.WriteLine("Invalid genre selected.");
                return;
            }

            Console.Write("Enter Duration (in minutes): ");
            if (!int.TryParse(Console.ReadLine(), out int duration))
            {
                Console.WriteLine("Invalid duration.");
                return;
            }

            Console.Write("Enter Release Date (yyyy-mm-dd): ");
            if (!DateTime.TryParse(Console.ReadLine(), out DateTime releaseDate))
            {
                Console.WriteLine("Invalid date format.");
                return;
            }

            Console.WriteLine("Select Rating:");
            foreach (var ratingOption in Enum.GetValues(typeof(MovieRating)))
            {
                Console.WriteLine($"{(int)ratingOption} - {ratingOption}");
            }
            Console.Write("Your choice: ");
            if (!Enum.TryParse(Console.ReadLine(), out MovieRating rating))
            {
                Console.WriteLine("Invalid rating selected.");
                return;
            }

            var movie = new Movie
            {
                Title = title,
                Genre = genre,
                Duration = duration,
                ReleaseDate = releaseDate,
                Rating = rating
            };

            await _movieService.AddMovieAsync(movie);
            Console.WriteLine("Movie added successfully.");
        }

        private async Task UpdateMovieAsync()
        {
            await GetMoviesAsync();

            Console.Write("Enter the ID of the movie to update: ");
            if (!int.TryParse(Console.ReadLine(), out int movieId))
            {
                Console.WriteLine("Invalid movie ID.");
                return;
            }

            var movieToUpdate = await _movieService.GetAllMoviesAsync().ContinueWith(t => t.Result.FirstOrDefault(m => m.MovieId == movieId));
            if (movieToUpdate == null)
            {
                Console.WriteLine("Movie not found.");
                return;
            }

            Console.Write("Enter new Title (leave blank to keep current): ");
            var title = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(title))
                movieToUpdate.Title = title;

            Console.WriteLine("Select new Genre (leave blank to keep current):");
            foreach (var genreOption in Enum.GetValues(typeof(MovieGenre)))
            {
                Console.WriteLine($"{(int)genreOption} - {genreOption}");
            }
            Console.Write("Your choice: ");
            var genreInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(genreInput) && Enum.TryParse(genreInput, out MovieGenre genre))
                movieToUpdate.Genre = genre;

            Console.Write("Enter new Duration (leave blank to keep current): ");
            var durationInput = Console.ReadLine();
            if (int.TryParse(durationInput, out int duration))
                movieToUpdate.Duration = duration;

            Console.Write("Enter new Release Date (leave blank to keep current): ");
            var releaseDateInput = Console.ReadLine();
            if (DateTime.TryParse(releaseDateInput, out DateTime releaseDate))
                movieToUpdate.ReleaseDate = releaseDate;

            Console.WriteLine("Select new Rating (leave blank to keep current):");
            foreach (var ratingOption in Enum.GetValues(typeof(MovieRating)))
            {
                Console.WriteLine($"{(int)ratingOption} - {ratingOption}");
            }
            Console.Write("Your choice: ");
            var ratingInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(ratingInput) && Enum.TryParse(ratingInput, out MovieRating rating))
                movieToUpdate.Rating = rating;

            await _movieService.UpdateMovieAsync(movieToUpdate);
            Console.WriteLine("Movie updated successfully.");
        }

        private async Task DeleteMovieAsync()
        {
            var movies = await _movieService.GetAllMoviesAsync();
            if (!movies.Any())
            {
                Console.WriteLine("No movies available.");
                return;
            }

            foreach (var movie in movies)
            {
                Console.WriteLine($"ID: {movie.MovieId}, Title: {movie.Title}");
            }

            Console.Write("Enter the ID of the movie to delete: ");
            if (!int.TryParse(Console.ReadLine(), out int movieId))
            {
                Console.WriteLine("Invalid movie ID.");
                return;
            }

            await _movieService.DeleteMovieAsync(movieId);
            Console.WriteLine("Movie deleted successfully.");
        }

        private async Task ManageHallsAsync()
        {
            await GetHallsAsync(); // Show all existing halls first

            Console.WriteLine("\nHall Management:");
            Console.WriteLine("1 - Add Hall");
            Console.WriteLine("2 - Update Hall");
            Console.WriteLine("3 - Delete Hall");
            Console.Write("Your choice: ");
            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    await AddHallAsync();
                    break;
                case "2":
                    await UpdateHallAsync();
                    break;
                case "3":
                    await DeleteHallAsync();
                    break;
                default:
                    Console.WriteLine("Invalid selection.");
                    break;
            }
        }

        private async Task GetHallsAsync()
        {
            var halls = await _hallService.GetAllHallsAsync();
            if (!halls.Any())
            {
                Console.WriteLine("No halls available.");
                return;
            }

            Console.WriteLine("\nExisting Halls:");
            foreach (var hall in halls)
            {
                Console.WriteLine($"ID: {hall.HallId}, Name: {hall.Name}, Capacity: {hall.Capacity}");
            }
        }

        private async Task AddHallAsync()
        {
            Console.Write("Enter Hall Name: ");
            var name = Console.ReadLine();
            Console.Write("Enter Capacity: ");
            if (!int.TryParse(Console.ReadLine(), out int capacity))
            {
                Console.WriteLine("Invalid capacity.");
                return;
            }

            var hall = new Hall
            {
                Name = name,
                Capacity = capacity
            };

            await _hallService.AddHallAsync(hall);
            Console.WriteLine("Hall added successfully.");
        }

        private async Task UpdateHallAsync()
        {
            var halls = await _hallService.GetAllHallsAsync();
            if (!halls.Any())
            {
                Console.WriteLine("No halls available.");
                return;
            }

            foreach (var hall in halls)
            {
                Console.WriteLine($"ID: {hall.HallId}, Name: {hall.Name}");
            }

            Console.Write("Enter the ID of the hall to update: ");
            if (!int.TryParse(Console.ReadLine(), out int hallId))
            {
                Console.WriteLine("Invalid hall ID.");
                return;
            }

            var hallToUpdate = halls.FirstOrDefault(h => h.HallId == hallId);
            if (hallToUpdate == null)
            {
                Console.WriteLine("Hall not found.");
                return;
            }

            Console.Write("Enter new Name (leave blank to keep current): ");
            var name = Console.ReadLine();
            Console.Write("Enter new Capacity (leave blank to keep current): ");
            var capacityInput = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(name)) hallToUpdate.Name = name;
            if (int.TryParse(capacityInput, out int capacity)) hallToUpdate.Capacity = capacity;

            await _hallService.UpdateHallAsync(hallToUpdate);
            Console.WriteLine("Hall updated successfully.");
        }

        private async Task DeleteHallAsync()
        {
            var halls = await _hallService.GetAllHallsAsync();
            if (!halls.Any())
            {
                Console.WriteLine("No halls available.");
                return;
            }

            foreach (var hall in halls)
            {
                Console.WriteLine($"ID: {hall.HallId}, Name: {hall.Name}");
            }

            Console.Write("Enter the ID of the hall to delete: ");
            if (!int.TryParse(Console.ReadLine(), out int hallId))
            {
                Console.WriteLine("Invalid hall ID.");
                return;
            }

            await _hallService.DeleteHallAsync(hallId);
            Console.WriteLine("Hall deleted successfully.");
        }

        private async Task ManageShowtimesAsync()
        {
            await GetShowtimesAsync(); // Show all existing showtimes first

            Console.WriteLine("\nShowtime Management:");
            Console.WriteLine("1 - Add Showtime");
            Console.WriteLine("2 - Update Showtime");
            Console.WriteLine("3 - Delete Showtime");
            Console.Write("Your choice: ");
            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    await AddShowtimeAsync();
                    break;
                case "2":
                    await UpdateShowtimeAsync();
                    break;
                case "3":
                    await DeleteShowtimeAsync();
                    break;
                default:
                    Console.WriteLine("Invalid selection.");
                    break;
            }
        }

        private async Task GetShowtimesAsync()
        {
            var showtimes = await _showtimeService.GetUpcomingShowtimesAsync();
            if (!showtimes.Any())
            {
                Console.WriteLine("No upcoming showtimes available.");
                return;
            }

            Console.WriteLine("\nExisting Showtimes:");
            foreach (var showtime in showtimes)
            {
                Console.WriteLine($"ID: {showtime.ShowTimeId}, Movie: {showtime.Movie.Title}, Hall: {showtime.Hall.Name}, Start Time: {showtime.StartTime}");
            }
        }

        private async Task AddShowtimeAsync()
        {
            var movies = await _movieService.GetAllMoviesAsync();
            var halls = await _hallService.GetAllHallsAsync();

            Console.WriteLine("Select Movie:");
            foreach (var movie in movies)
            {
                Console.WriteLine($"ID: {movie.MovieId}, Title: {movie.Title}");
            }
            if (!int.TryParse(Console.ReadLine(), out int movieId))
            {
                Console.WriteLine("Invalid movie ID.");
                return;
            }

            Console.WriteLine("Select Hall:");
            foreach (var hall in halls)
            {
                Console.WriteLine($"ID: {hall.HallId}, Name: {hall.Name}");
            }
            if (!int.TryParse(Console.ReadLine(), out int hallId))
            {
                Console.WriteLine("Invalid hall ID.");
                return;
            }

            Console.Write("Enter Showtime Start Time (yyyy-mm-dd HH:mm): ");
            if (!DateTime.TryParse(Console.ReadLine(), out DateTime startTime))
            {
                Console.WriteLine("Invalid date format.");
                return;
            }

            // Retrieve the selected movie to calculate its duration
            var selectedMovie = movies.FirstOrDefault(m => m.MovieId == movieId);
            if (selectedMovie == null)
            {
                Console.WriteLine("Movie not found.");
                return;
            }

            // Calculate the proposed end time for the showtime
            var endTime = startTime.AddMinutes(selectedMovie.Duration);

            // Check if there's a conflicting showtime in the same hall
            var conflictingShowtime = await _showtimeService.GetShowtimesByHallAndTimeAsync(hallId, startTime, endTime);
            if (conflictingShowtime != null)
            {
                Console.WriteLine($"The hall '{conflictingShowtime.Hall.Name}' is already booked for '{conflictingShowtime.Movie.Title}' from {conflictingShowtime.StartTime} to {conflictingShowtime.EndTime}.");
                Console.WriteLine($"The hall will be available after {conflictingShowtime.EndTime}.");
                return;
            }

            var showtime = new ShowTime
            {
                MovieId = movieId,
                HallId = hallId,
                StartTime = startTime
            };

            await _showtimeService.AddShowtimeAsync(showtime);
            Console.WriteLine("Showtime added successfully.");
        }

        private async Task UpdateShowtimeAsync()
        {
            var showtimes = await _showtimeService.GetUpcomingShowtimesAsync();
            if (!showtimes.Any())
            {
                Console.WriteLine("No showtimes available.");
                return;
            }

            foreach (var showtime in showtimes)
            {
                Console.WriteLine($"ID: {showtime.ShowTimeId}, Movie: {showtime.Movie.Title}, Hall: {showtime.Hall.Name}, Time: {showtime.StartTime}");
            }

            Console.Write("Enter the ID of the showtime to update: ");
            if (!int.TryParse(Console.ReadLine(), out int showtimeId))
            {
                Console.WriteLine("Invalid showtime ID.");
                return;
            }

            var showtimeToUpdate = showtimes.FirstOrDefault(s => s.ShowTimeId == showtimeId);
            if (showtimeToUpdate == null)
            {
                Console.WriteLine("Showtime not found.");
                return;
            }

            Console.Write("Enter new Start Time (leave blank to keep current): ");
            var startTimeInput = Console.ReadLine();

            if (DateTime.TryParse(startTimeInput, out DateTime startTime))
                showtimeToUpdate.StartTime = startTime;

            await _showtimeService.UpdateShowtimeAsync(showtimeToUpdate);
            Console.WriteLine("Showtime updated successfully.");
        }
        
        private async Task DeleteShowtimeAsync()
        {
            var showtimes = await _showtimeService.GetUpcomingShowtimesAsync();
            if (!showtimes.Any())
            {
                Console.WriteLine("No showtimes available.");
                return;
            }

            foreach (var showtime in showtimes)
            {
                Console.WriteLine($"ID: {showtime.ShowTimeId}, Movie: {showtime.Movie.Title}, Hall: {showtime.Hall.Name}, Time: {showtime.StartTime}");
            }

            Console.Write("Enter the ID of the showtime to delete: ");
            if (!int.TryParse(Console.ReadLine(), out int showtimeId))
            {
                Console.WriteLine("Invalid showtime ID.");
                return;
            }

            await _showtimeService.DeleteShowtimeAsync(showtimeId);
            Console.WriteLine("Showtime deleted successfully.");
        }



    }
}




