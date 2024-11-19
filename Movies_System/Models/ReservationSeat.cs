using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Movies_System.Models
{
    public class ReservationSeat
    {
        [Key]
        public int Id { get; set; }

        public int ReservationId { get; set; }

        public int SeatId { get; set; }

        public int ShowTimeId { get; set; }

        [ForeignKey("ReservationId")]
        public virtual Reservation Reservation { get; set; }

        [ForeignKey("SeatId")]
        public virtual Seat Seat { get; set; }

        [ForeignKey("ShowTimeId")]
        public virtual ShowTime ShowTime { get; set; }
    }
}
