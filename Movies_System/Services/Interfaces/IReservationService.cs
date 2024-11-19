using Movies_System.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Movies_System.Services
{
    public interface IReservationService
    {
        Task<List<SeatStatus>> GetSeatAvailabilityAsync(int showTimeId);
        Task<Reservation> CreateReservationAsync(int userId, int showTimeId, string seatNumbersInput);
        Task CancelReservationAsync(int reservationId);
        Task<List<Reservation>> GetUserReservationsAsync(int userId);
        Task<List<UserReservations>> GetAllReservationsAsync();
    }
}
