using System.Collections.Generic;
using System.Threading.Tasks;
using PracticalApp.NorthwindEntitiesLib;

namespace PracticalApp.NorthwindAPI.Repositories
{
    public interface ICustomerRepository
    {
        Task<Customer> RetrieveAsync(string id);
        Task<IEnumerable<Customer>> RetrieveAllAsync();
        Task<Customer> CreateAsync(Customer customer);
        Task<Customer> UpdateAsync(string id, Customer customer);
        Task<bool?> DeleteAsync(string id);
    }
}