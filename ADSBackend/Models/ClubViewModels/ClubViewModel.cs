using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ADSBackend.Models.ClubViewModels
{
    public class ClubViewModel
    {
        [Key]
        public int ClubId { get; set; }

        public int CreatorId { get; set; }

        public string Creator { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Password { get; set; }

        public List<ClubMember> ClubMembers { get; set; }

        public List<int> MemberIds { get; set; }

        public ClubViewModel()
        {

        }

        public ClubViewModel(Club club)
        {
            this.ClubId = club.ClubId;
            this.CreatorId = club.CreatorId;
            this.Creator = club.Creator;
            this.Name = club.Name;
            this.Description = club.Description;
            this.Password = club.Password;
            this.ClubMembers = club.ClubMembers;
            this.MemberIds = club.ClubMembers?.Select(cm => cm.MemberId).ToList();
        }
    }
}
