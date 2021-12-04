using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Entities.DataTransferObjects
{
    public class CompanyDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string FullAddress { get; set; }
        // public IEnumerable<EmployeeDto> Employees { get; set; } // since we use this as xml we should convert interfaces to concrete class
        public List<EmployeeDto> Employees { get; set; }
    }
}