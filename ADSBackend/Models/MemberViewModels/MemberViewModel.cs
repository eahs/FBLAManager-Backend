using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ADSBackend.Models.MemberViewModels
{
    public class MemberViewModel
    {
        [Key]
        public int MemberId { get; set; }

        public string Username { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public string Password { get; set; }

        public List<ClubMember> ClubMembers { get; set; }

        public List<MeetingAttendees> MeetingAttendees { get; set; } //attendees

        public List<int> ClubIds { get; set; }

        public MemberViewModel()
        {

        }

        public MemberViewModel(Member member)
        {
            this.MemberId = member.MemberId;
            this.Username = member.Username;
            this.Email = member.Email;
            this.Password = member.Password;
            this.Phone = member.Phone;
            this.ClubMembers = member.ClubMembers;
            this.MeetingAttendees = member.MeetingAttendees;
            this.ClubIds = member.ClubMembers?.Select(cm => cm.ClubId).ToList();
        }
    }
}
