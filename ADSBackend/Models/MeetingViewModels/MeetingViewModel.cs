using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;

namespace ADSBackend.Models.MeetingViewModels
{

    public class MeetingViewModel
    {
        [Key]
        public int MeetingId { get; set; } //backend

        public int OrganizerId { get; set; } //meeting creator, backend

        public string Organizer { get; set; }

        public string ContactId { get; set; }

        public string EventName { get; set; }

        public string Description { get; set; }

        public int Capacity { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime Start { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime End { get; set; }

        public string Password { get; set; }

        public string Color { get; set; }

        public bool AllDay { get; set; }

        public MeetingType Type { get; set; } = MeetingType.Meeting;

        public List<int> MemberIds { get; set; }

        public MeetingViewModel()
        {

        }

        public MeetingViewModel(Meeting meeting)
        {
            this.MeetingId = meeting.MeetingId;
            this.OrganizerId = meeting.OrganizerId;
            this.Organizer = meeting.Organizer;
            this.ContactId = meeting.ContactId;
            this.EventName = meeting.EventName;
            this.Description = meeting.Description;
            this.Capacity = meeting.Capacity;
            this.Start = meeting.Start;
            this.End = meeting.End;
            this.Password = meeting.Password;
            this.Color = meeting.Color;
            this.AllDay = meeting.AllDay;
            this.Type = meeting.Type;
            this.MemberIds = meeting.MeetingAttendees?.Select(ma => ma.MemberId).ToList();
        }
    }
}
