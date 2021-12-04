using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Src.Data.TagHelperModel
{
    public interface IProductRepository
    {
        IEnumerable<Product> Products { get; }
        void AddProduct(Product product);
    }

    // The work of the ProductRepository is to provide with a repository (in-memory storage) of product to the application.
    public class ProductRepository : IProductRepository
    {
        private List<Product> _products = new List<Product> {
            new Product { Name = "Men Shoes", Price = 99.99F, Quantity= 100},
            new Product { Name = "Women Shoes", Price = 199.99F, Quantity= 200},
            new Product { Name = "Children Games", Price = 299.99F, Quantity= 300},
            new Product { Name = "Coats", Price = 399.99F, Quantity= 400},
        };

        public IEnumerable<Product> Products => _products;

        public void AddProduct(Product product)
        {
            // _products.Append(product); // This generate a new sequence and should be set in _product again
            _products.Add(product); // To use add method we should convert _products to List not IEnumerable
        }
    }
}