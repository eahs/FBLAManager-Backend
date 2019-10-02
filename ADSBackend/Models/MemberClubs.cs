using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ADSBackend.Models
{
    public class MemberClubs
    {
        public int Id { get; set; }
        public Member Member { get; set; }
        public int ClubId { get; set; }
        public Club Club { get; set; }
    }
}
