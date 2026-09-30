using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetMvcWeb.Data;
using DotNetMvcWeb.Models;
using DotNetMvcWeb.Repositories.Implements;
using DotNetMvcWeb.Repositories.Interfaces;
using DotNetMvcWeb.Services.Interfaces;

namespace DotNetMvcWeb.Services.Implements
{
    /// <summary>
    /// [教學註解] 服務層實作 (Service Implementation)
    /// 負責處理商業邏輯與規則，不再直接存取 DbContext，而是透過注入 IOracleDemoCategoryRepository 進行資料存取。
    /// 這種架構分層為：Controller -> Service (業務邏輯) -> Repository (資料存取) -> DbContext。
    /// </summary>
    public class OracleDemoCategoryService : IOracleDemoCategoryService
    {
        private readonly IOracleDemoCategoryRepository _categoryRepository;

        // DI 容器使用的唯一 public 建構子
        public OracleDemoCategoryService(IOracleDemoCategoryRepository categoryRepository)
        {
            ArgumentNullException.ThrowIfNull(categoryRepository);
            _categoryRepository = categoryRepository;
        }

        // 提供給既有單元測試直接傳入 DbContext 的便利建構子 (設為 internal 避免 DI 歧義)
        internal OracleDemoCategoryService(AppDbContext context)
            : this(new OracleDemoCategoryRepository(context))
        {
        }

        public async Task<List<OracleDemoCategory>> GetCategoriesAsync()
        {
            return await _categoryRepository.GetCategoriesAsync();
        }

        public async Task<OracleDemoCategory?> GetCategoryByIdAsync(int id)
        {
            return await _categoryRepository.GetCategoryByIdAsync(id);
        }

        public async Task CreateCategoryAsync(OracleDemoCategory category)
        {
            ArgumentNullException.ThrowIfNull(category);
            // 商業邏輯：建立時自動指派 UTC 建立時間
            category.CreatedAt = DateTime.UtcNow;
            await _categoryRepository.AddCategoryAsync(category);
        }

        public async Task UpdateCategoryAsync(OracleDemoCategory category)
        {
            ArgumentNullException.ThrowIfNull(category);
            await _categoryRepository.UpdateCategoryAsync(category);
        }

        public async Task DeleteCategoryAsync(int id)
        {
            await _categoryRepository.DeleteCategoryAsync(id);
        }

        public bool CategoryExists(int id)
        {
            return _categoryRepository.CategoryExists(id);
        }
    }
}
