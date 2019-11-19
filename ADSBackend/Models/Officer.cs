using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ADSBackend.Models
{
    public class Officer
    {
        [Key]
        public int OfficerId { get; set; }

        public string Name { get; set; }

        public string Position { get; set; }

        public string Image { get; set; }

        [Display(Name = "Website")]
        public string WebsiteLink { get; set; }

        public int Level { get; set; }

        public int Order { get; set; }
    }
}
