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
        public int Id { get; set; }

        [DataType(DataType.Time)]
        public DateTime Start { get; set; }

        [DataType(DataType.Time)]
        public DateTime End { get; set; }
        public string Password { get; set; }
        public List<int> MemberIds { get; set; } 
    }
}
