using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using PracticalApp.NorthwindContextLib;
using PracticalApp.NorthwindEntitiesLib;
using System.Linq;

namespace PracticalApp.NorthwindAPI.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private static ConcurrentDictionary<string, Customer> _customerCache;
        private Customer UpdateCache(string customerId, Customer customer)
        {
            Customer c;
            if (_customerCache.TryGetValue(customerId, out c))
            {
                if (_customerCache.TryUpdate(customerId, customer, c))
                {
                    return customer;
                }
                return c;
            }
            return null;
        }
        private Northwind _db;

        public CustomerRepository(Northwind db)
        {
            _db = db;
            if (_customerCache == null)
            {
                _customerCache =
                new ConcurrentDictionary<string, Customer>(
                    _db.Customers
                    .ToDictionary(c => c.CustomerID));
            }
        }

        public async Task<Customer> CreateAsync(Customer customer)
        {
            customer.CustomerID = customer.CustomerID.ToUpper();
            _db.Customers.Add(customer);
            var affected = await _db.SaveChangesAsync();
            if (affected == 1)
            {
                return _customerCache.AddOrUpdate(customer.CustomerID, customer, UpdateCache);
            }
            else
            {
                return null;
            }
        }

        public async Task<bool?> DeleteAsync(string id)
        {
            id = id.ToUpper();
            var customer = await _db.Customers.FindAsync(id);
            var removed = _db.Customers.Remove(customer);
            var affected = await _db.SaveChangesAsync();

            if (affected == 1)
            {
                return _customerCache.TryRemove(id, out Customer c);
            }
            return null;
        }

        public Task<IEnumerable<Customer>> RetrieveAllAsync()
        {
            // For Performance Get From Cache
            return Task.Run<IEnumerable<Customer>>(() => _customerCache.Values);
        }

        public Task<Customer> RetrieveAsync(string id)
        {
            // Read From Cache
            return Task.Run<Customer>(() =>
            {
                var upperId = id.ToUpper();
                Customer customer;
                _customerCache.TryGetValue(upperId, out customer);
                return customer;
            });
        }

        public async Task<Customer> UpdateAsync(string id, Customer customer)
        {
            customer.CustomerID = customer.CustomerID.ToUpper();
            id = id.ToUpper();
            var update = _db.Customers.Update(customer);
            var affected = await _db.SaveChangesAsync();
            if (affected == 1)
            {
                return UpdateCache(id, customer);
            }
            return null;
        }
    }
}