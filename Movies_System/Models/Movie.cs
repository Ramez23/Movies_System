using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Movies_System.Models
{
        public enum MovieGenre
        {
            Action,
            Comedy,
            Drama,
            Horror,
            SciFi,
            Romance,
            Documentary,
            Animation
        }

        public enum MovieRating
        {
            G,
            PG,
            PG13,
            R,
            NC17
        }

    public class Movie
    {
        [Key]
        public int MovieId { get; set; }

        public string Title { get; set; }

        public MovieGenre Genre { get; set; } 

        public int Duration { get; set; }

        public DateTime ReleaseDate { get; set; }

        public MovieRating Rating { get; set; } 
        public virtual ICollection<ShowTime> ShowTimes { get; set; }
    }


}
