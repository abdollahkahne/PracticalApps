using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Configuration;
using Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Entities
{
    // This work as an abstraction layer between data layer and business logic layer
    public class RepositoryContext : DbContext
    {
        public RepositoryContext(DbContextOptions<RepositoryContext> options) : base(options)
        {

        }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Company> Companies { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfiguration<Company>(new CompanyConfiguration());
            builder.ApplyConfiguration<Employee>(new EmployeeConfiguration());
        }

        //     protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        // => optionsBuilder.LogTo(Console.WriteLine);

    }
}