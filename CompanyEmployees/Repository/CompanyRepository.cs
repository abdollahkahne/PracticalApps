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
    public class CompanyRepository : RepositoryBase<Company>, ICompanyRepository
    {
        public CompanyRepository(RepositoryContext db) : base(db) { } // This should explicitly existed

        public void CreateCompany(Company company)
        {
            Create(company);
        }

        public void DeleteCompany(Company company)
        {
            Delete(company); // This deletes all child elements since we selected relation ship such that cascade on deleting Parent
        }

        public Company GetById(Guid id, bool trackChanges)
        {
            return FindByCondition(c => c.Id == id, trackChanges).SingleOrDefault();
        }

        public async Task<Company> GetByIdAsync(Guid id, bool trackChanges)
        {
            return await FindByCondition(c => c.Id == id, trackChanges).SingleOrDefaultAsync();
        }

        public IQueryable<Company> GetCompaniesCollection(IEnumerable<Guid> ids, bool trackChanges)
        {
            return FindByCondition(c => ids.Contains(c.Id), trackChanges);
        }

        IQueryable<Company> ICompanyRepository.GetAll(bool trackChanges)
        {
            return FindAll(trackChanges).OrderBy(c => c.Name);
        }
    }
}