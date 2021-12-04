using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace APIClient.DataTransferObjects
{
    public class CompanyDto
    {
        public Guid Id { get; set; }
        // [JsonPropertyName("name")]
        public string Name { get; set; }
        public string FullAddress { get; set; }
        // public IEnumerable<EmployeeDto> Employees { get; set; }
    }
}