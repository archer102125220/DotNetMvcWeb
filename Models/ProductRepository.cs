using DotNetMvcWeb.Models;
using System.Collections.Generic;
using System.Linq;

namespace DotNetMvcWeb.Models
{
    /// <summary>
    /// 產品資料存取庫 (模擬資料庫)
    /// </summary>
    public static class ProductRepository
    {
        private static readonly List<Product> _products = new()
        {
            new Product { Id = 1, Name = "筆記型電腦", Price = 35000, Description = "高效能開發用筆電" },
            new Product { Id = 2, Name = "無線滑鼠", Price = 1200, Description = "人體工學設計滑鼠" }
        };

        public static List<Product> GetAll() => _products;

        public static Product? GetById(int id) => _products.FirstOrDefault(p => p.Id == id);

        public static void Add(Product product)
        {
            product.Id = _products.Any() ? _products.Max(p => p.Id) + 1 : 1;
            _products.Add(product);
        }

        public static void Update(Product product)
        {
            var existing = GetById(product.Id);
            if (existing != null)
            {
                existing.Name = product.Name;
                existing.Price = product.Price;
                existing.Description = product.Description;
            }
        }

        public static void Delete(int id)
        {
            var product = GetById(id);
            if (product != null)
            {
                _products.Remove(product);
            }
        }
    }
}
