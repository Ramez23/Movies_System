using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Movies_System.Models
{
    public class Seat
    {
        [Key]
        public int SeatId { get; set; }

        public int SeatNumber { get; set; }

        public string Status { get; set; }

        public int HallId { get; set; }

        [ForeignKey("HallId")]
        public virtual Hall Hall { get; set; }

        public virtual ICollection<ReservationSeat> ReservationSeats { get; set; }
    }


}
