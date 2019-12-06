using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;

namespace ADSBackend.Models
{
    public enum MeetingType
    {
        Meeting = 1,
        [Display(Name = "Club Event")]
        ClubEvent = 2,
        [Display(Name = "Competitive Event")]
        CompetitiveEvent = 3,
        [Display(Name = "Community Service")]
        CommunityService = 4
    }
    public class Meeting
    {
        [Key]
        public int MeetingId { get; set; } //backend

        public int OrganizerId { get; set; } //meeting creator, backend

        public string Organizer { get; set; }

        [Display(Name = "Contact ID")]
        public string ContactId { get; set; }

        [Display(Name = "Name")]
        public string EventName { get; set; }

        public string Description { get; set; }

        public int Capacity { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime Start { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime End { get; set; }

        public string Password { get; set; }

        public string Color { get; set; }

        [Display(Name = "All Day")]
        public bool AllDay { get; set; }

        public MeetingType Type { get; set; } = MeetingType.Meeting;

        public List<MeetingAttendees> MeetingAttendees { get; set; } //attendees
    }
}
