using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Movies_System.Models
{
    public class ShowTime
    {
        [Key]
        public int ShowTimeId { get; set; }

        public int MovieId { get; set; }

        public int HallId { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime
        {
            get { return StartTime.AddMinutes(Movie.Duration); }
        }

        [ForeignKey("MovieId")]
        public virtual Movie Movie { get; set; }

        [ForeignKey("HallId")]
        public virtual Hall Hall { get; set; }

        public virtual ICollection<Reservation> Reservations { get; set; }
        public virtual ICollection<ReservationSeat> ReservationSeats { get; set; }
    }

}
