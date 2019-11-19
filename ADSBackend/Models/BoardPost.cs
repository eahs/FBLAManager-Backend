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

        [DataType(DataType.DateTime)]
        public DateTime PostTime { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime EditedTime { get; set; }

        public string Message { get; set; }

        public int ClubId { get; set; }
        public Club Club { get; set; }
    }
}
