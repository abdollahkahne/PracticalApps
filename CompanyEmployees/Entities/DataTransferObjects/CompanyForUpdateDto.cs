using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Entities.DataTransferObjects
{
    public class CompanyForUpdateDto : CompanyManipulationDto
    {
        public IEnumerable<EmployeeForCreationUpdateDto> Employees { get; set; }
    }
}