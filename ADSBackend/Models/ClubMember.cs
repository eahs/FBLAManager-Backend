using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ADSBackend.Models
{
    public class ClubMember
    {
        public int MemberId { get; set; }
        public Member Member { get; set; }
        public int ClubId { get; set; }
        public Club Club { get; set; }
    }
}
