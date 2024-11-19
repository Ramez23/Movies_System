using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Movies_System.Models
{
    public class Reservation
    {
        [Key]
        public int ReservationId { get; set; }

        public int UserId { get; set; }

        public int ShowTimeId { get; set; }

        public DateTime ReservationDate { get; set; }

        public int? PaymentId { get; set; }  

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("ShowTimeId")]
        public virtual ShowTime ShowTime { get; set; }

        public virtual ICollection<ReservationSeat> ReservationSeats { get; set; }
    }
}
