using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ADSBackend.Models
{
    public class ADSBackendContext : DbContext
    {
        public ADSBackendContext (DbContextOptions<ADSBackendContext> options)
            : base(options)
        {
        }

        public DbSet<ADSBackend.Models.Member> Member { get; set; }
    }
}
