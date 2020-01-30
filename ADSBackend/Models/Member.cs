using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ADSBackend.Models
{
    public class Member
    {
        [Key]
        public int MemberId { get; set; }

        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        public string FullName { get; set; }

        public string Gender { get; set; }

        public string Address { get; set; }

        public string City { get; set; }

        public string State { get; set; } = "PA";

        [Display(Name = "Zip Code")]
        public string ZipCode { get; set; }

        public int Grade { get; set; }

        [Display(Name = "Recruited By")]
        public string RecruitedBy { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        [Display(Name = "Profile Picture")]
        public string profileImageSource { get; set; }

        [Display(Name = "Bio")]
        public string Description { get; set; }

        [Required]
        public string Password { get; set; }

        public string Salt { get; set; }

        public List<ClubMember> ClubMembers { get; set; }

        public List<MeetingAttendees> MeetingAttendees { get; set; } //attendees
    }
}
