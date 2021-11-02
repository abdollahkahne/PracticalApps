using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Src.Core.Models;

namespace Src.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public virtual DbSet<BrainstormSession> BrainstormSessions
        {
            get; set;

        }
        // public virtual DbSet<Idea> Ideas { get; set; }
    }
}