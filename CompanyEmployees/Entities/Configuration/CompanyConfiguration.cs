using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Entities.Configuration
{
    // this can be used for seeeding for example but it is primarily for factoring all Company Configuration here and the add them using the
    // ModelBuilder.ApplyConfiguration<Company>(new CompanyConfiguration()) to DbContext OnModelCreating method.
    // Alternatively we can apply this configs directly in OnModelCreating too
    public class CompanyConfiguration : IEntityTypeConfiguration<Company>
    {
        public void Configure(EntityTypeBuilder<Company> builder)
        {
            // we use the guid strings here since we want to have them always and do not generate them randomly every time!
            builder.HasData(new Company
            {
                Id = new Guid("c9d4c053-49b6-410c-bc78-2d54a9991870"),
                Name = "IT_Solutions Ltd",
                Address = "583 Wall Dr. Gwynn Oak, MD 21207",
                Country = "USA"
            }, new Company
            {
                Id = new Guid("3d490a70-94ce-4d15-9494-5248280c2ce3"),
                Name = "Admin_Solutions Ltd",
                Address = "312 Forest Avenue, BF 923",
                Country = "USA"
            });
        }
    }
}