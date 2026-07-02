using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetMvcWeb.Models;

namespace DotNetMvcWeb.Services.Interfaces
{
    public interface IMssqlDemoCategoryService
    {
        Task<List<MssqlDemoCategory>> GetCategoriesAsync();
        Task<MssqlDemoCategory?> GetCategoryByIdAsync(int id);
        Task CreateCategoryAsync(MssqlDemoCategory category);
        Task UpdateCategoryAsync(MssqlDemoCategory category);
        Task DeleteCategoryAsync(int id);
        bool CategoryExists(int id);
    }
}
