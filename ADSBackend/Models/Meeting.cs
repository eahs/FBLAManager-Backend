using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ADSBackend.Models
{
    public class Meeting
    {
        [Key]
        public int MeetingId { get; set; }

        public int PlannerId { get; set; } //meeting creator

        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [DataType(DataType.Time)]
        public DateTime StartTime { get; set; }

        [DataType(DataType.Time)]
        public DateTime EndTime { get; set; }

        public string Password { get; set; }

        public List<Member> Members { get; set; } //attendees
    }
}
