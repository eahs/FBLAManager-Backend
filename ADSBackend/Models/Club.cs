using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ADSBackend.Models
{
    public class Club
    {
        [Key]
        public int ClubId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public List<MemberClubs> MemberClubs { get; set; }
    }
}
