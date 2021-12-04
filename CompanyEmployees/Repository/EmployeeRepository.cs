using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts;
using Entities;
using Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Repository
{
    public class EmployeeRepository : RepositoryBase<Employee>, IEmployeeRepository
    {
        public EmployeeRepository(RepositoryContext db) : base(db)
        {
        }

        public void CreateEmployeeFor(Guid companyId, Employee employee)
        {
            employee.CompanyId = companyId;// we could not trust to Presentation layer for this setting, altough it is possible
            // Of course we can double check company existence here too but I think it is a business matter and should do in business/presentation layer
            Create(employee);
        }

        public void DeleteEmployee(Employee employee)
        {
            Delete(employee);
        }

        public Employee GetEmployee(Guid companyId, Guid employeeId, bool trackChanges)
        {
            return FindByCondition(e => e.CompanyId == companyId && e.Id == employeeId, trackChanges).FirstOrDefault();
        }

        public async Task<Employee> GetEmployeeAsync(Guid companyId, Guid employeeId, bool trackChanges)
        {
            return await FindByCondition(e => e.CompanyId == companyId && e.Id == employeeId, trackChanges).FirstOrDefaultAsync();
        }

        public IQueryable<Employee> GetEmployees(Guid companyId, bool trackChanges)
        {
            return FindByCondition(e => e.CompanyId == companyId, trackChanges).OrderBy(e => e.Name);
        }
    }
}