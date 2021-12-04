using System;
using System.ComponentModel.DataAnnotations;

namespace Entities.DataTransferObjects
{
    public class EmployeeForCreationUpdateDto : EmployeeManipulationDto
    {
        public Guid Id { get; set; }
    }
}