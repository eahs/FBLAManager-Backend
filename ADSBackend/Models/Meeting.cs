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
        public int MeetingId { get; set; } //backend

        public int OrganizerId { get; set; } //meeting creator, backend

        public string Organizer { get; set; }

        public string ContactId { get; set; }

        public string EventName { get; set; }

        public int Capacity { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime Start { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime End { get; set; }

        public string Password { get; set; }

        public string Color { get; set; }

        public bool AllDay { get; set; }

        public List<Member> Members { get; set; } //attendees
    }
}
