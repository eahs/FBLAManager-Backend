using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ADSBackend.Models
{
    public class Session
    {
        [Key]
        public int SessionId { get; set; }

        public int MemberId { get; set; }

        public string Email { get; set; }

        public string Key { get; set; }

        public string GoogleToken { get; set; }

        public string AppleToken { get; set; }

        public DateTime LastAccessTime { get; set; }
    }
}
