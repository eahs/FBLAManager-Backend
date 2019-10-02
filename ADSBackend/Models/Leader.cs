using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ADSBackend.Models
{
    public class Leader : Member
    {
        public List<int> ClubsLeading { get; set; }
    }
}
