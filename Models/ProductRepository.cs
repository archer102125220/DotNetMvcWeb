using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetMvcWeb.Models;

namespace DotNetMvcWeb.Models
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllAsync();
        Task<Product?> GetByIdAsync(int id);
        Task AddAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(int id);
    }

    /// <summary>
    /// 產品資料存取庫 (模擬資料庫)
    /// </summary>
    public class ProductRepository : IProductRepository
    {
        private readonly List<Product> _products = new()
        {
            new Product { Id = 1, Name = "筆記型電腦", Price = 35000, Description = "高效能開發用筆電" },
            new Product { Id = 2, Name = "無線滑鼠", Price = 1200, Description = "人體工學設計滑鼠" }
        };

        public Task<IEnumerable<Product>> GetAllAsync() => Task.FromResult<IEnumerable<Product>>(_products);

        public Task<Product?> GetByIdAsync(int id) => Task.FromResult(_products.FirstOrDefault(p => p.Id == id));

        public Task AddAsync(Product product)
        {
            ArgumentNullException.ThrowIfNull(product);
            product.Id = _products.Any() ? _products.Max(p => p.Id) + 1 : 1;
            _products.Add(product);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Product product)
        {
            ArgumentNullException.ThrowIfNull(product);
            Product? existing = _products.FirstOrDefault(p => p.Id == product.Id);
            if (existing is not null)
            {
                existing.Name = product.Name;
                existing.Price = product.Price;
                existing.Description = product.Description;
            }
            return Task.CompletedTask;
        }

        public Task DeleteAsync(int id)
        {
            Product? product = _products.FirstOrDefault(p => p.Id == id);
            if (product is not null)
            {
                _products.Remove(product);
            }
            return Task.CompletedTask;
        }
    }
}
