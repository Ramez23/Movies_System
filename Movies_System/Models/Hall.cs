using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Movies_System.Models
{
    public class Hall
    {
        [Key]
        public int HallId { get; set; }

        public string Name { get; set; }

        public int Capacity { get; set; }

        public virtual ICollection<Seat> Seats { get; set; }

        public virtual ICollection<ShowTime> ShowTimes { get; set; }
    }


}
