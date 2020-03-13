using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ADSBackend.Models
{
    public class BoardPost
    {
        [Key]
        public int PostId { get; set; }

        public string Title { get; set; }

        public string Director { get; set; }

        [DisplayFormat(DataFormatString = "{0:MMMM d, yyyy @ h:mm tt}")]
        [DataType(DataType.DateTime)]
        public DateTime WriteTime { get; set; }

        [DisplayFormat(DataFormatString = "{0:MMMM d, yyyy @ h:mm tt}")]
        [DataType(DataType.DateTime)]
        public DateTime EditedTime { get; set; }

        [DisplayFormat(DataFormatString = "{0:MMMM d, yyyy @ h:mm tt}")]
        [Display(Name = "Scheduled Time")]
        [DataType(DataType.DateTime)]
        public DateTime PostTime { get; set; }

        public string Message { get; set; }

        public string Status { get; set; }

        [Display(Name = "Image URL")]
        public string ImageURL { get; set; }

        public int ClubId { get; set; }
        public Club Club { get; set; }
    }
}
