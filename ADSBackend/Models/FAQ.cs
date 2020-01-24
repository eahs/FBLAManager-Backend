using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ADSBackend.Models
{
    public class FAQ
    {
        [Key]
        public int FAQId { get; set; }

        public string Question { get; set; }

        public string Answer { get; set; }

        public DateTime Created { get; set; }

        public DateTime Edited { get; set; }
    }
}
