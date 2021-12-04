using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Models;

namespace Contracts
{
    public interface ICompanyRepository
    {
        IQueryable<Company> GetAll(bool trackChanges); // since we use dto for transfering data better to convert the data to enumrable at that time to improve performance
        Company GetById(Guid id, bool trackChanges);
        void CreateCompany(Company company);
        IQueryable<Company> GetCompaniesCollection(IEnumerable<Guid> ids, bool trackChanges);
        void DeleteCompany(Company company);

        // IQueryable<Company> GetAllAsync(bool trackChanges); // sine we return IQueryable, we does not send the query to database yet so it is sync in nature!
        Task<Company> GetByIdAsync(Guid id, bool trackChanges);
        // void CreateCompany(Company company); // this does not do async task and only modifies EntityState so making it async does not do anything different!
        // IQueryable<Company> GetCompaniesCollectionAsync(IEnumerable<Guid> ids, bool trackChanges);
        // void DeleteCompany(Company company);
    }
}