Movies System Console Application
This project is a .NET Console Application designed to manage a movie reservation system. It includes functionalities for managing movies, halls, seats, showtimes, and reservations. The application provides a structured, service-based architecture, ensuring modular and maintainable code. Administrative privileges are required to manage halls, movies, and showtimes.

Features
User Roles
Admin:
Can add, update, and delete halls, movies, and showtimes.
User:
Can browse available movies and showtimes.
Can reserve seats for specific showtimes.
Core Functionalities
Movies Management (Admin Only): Manage movie information, including title, duration, and other attributes.
Halls Management (Admin Only): Manage movie halls and their seating capacities.
Showtimes Management (Admin Only): Schedule and manage showtimes for movies in specific halls.
Reservations Management: Users can reserve specific seats for chosen showtimes, with validations for seat availability.
Project Structure
The project is organized as follows:

Data:

ApplicationDBContext.cs: Contains the Entity Framework Core database context for the application.
Migrations:

Stores migration files for creating and managing the database schema.
Models:

Hall.cs: Represents movie halls.
Movie.cs: Represents movies and their details.
ShowTime.cs: Represents scheduled showtimes for movies.
Seat.cs: Represents individual seats in a hall.
Reservation.cs: Represents reservations made by users.
ReservationSeat.cs: Maps reserved seats to reservations.
User.cs: Represents user details for managing roles and reservations.
Services:

Implementations:
HallService.cs: Business logic for managing halls (Admin Only).
MovieService.cs: Business logic for managing movies (Admin Only).
ShowtimeService.cs: Business logic for managing showtimes (Admin Only).
ReservationService.cs: Business logic for handling reservations.
UserService.cs: Handles user-related operations like authentication and role management.
Interfaces:
Define contracts for services, ensuring adherence to clean architecture principles.
Technologies Used
.NET 8.0: Framework for building the console application.
Entity Framework Core: ORM for database operations.
SQL Server: Relational database for data persistence.
