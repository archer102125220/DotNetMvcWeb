using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetMvcWeb.Data;
using DotNetMvcWeb.Models;
using DotNetMvcWeb.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DotNetMvcWeb.Repositories.Implements
{
    /// <summary>
    /// [教學註解] 倉儲層實作 (Repository Implementation)
    /// 這裡封裝所有與 AppDbContext (Oracle) 的資料存取邏輯。
    /// 遵循 EF Core 深度檢查規範：使用非同步方法 (ToListAsync, SaveChangesAsync)、唯讀查詢套用 AsNoTracking。
    /// </summary>
    public class OracleDemoCategoryRepository : IOracleDemoCategoryRepository
    {
        private readonly AppDbContext _context;

        public OracleDemoCategoryRepository(AppDbContext context)
        {
            ArgumentNullException.ThrowIfNull(context);
            _context = context;
        }

        public async Task<List<OracleDemoCategory>> GetCategoriesAsync()
        {
            return await _context.OracleDemoCategories
                .AsNoTracking()
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<OracleDemoCategory?> GetCategoryByIdAsync(int id)
        {
            return await _context.OracleDemoCategories.FindAsync(id);
        }

        public async Task AddCategoryAsync(OracleDemoCategory category)
        {
            ArgumentNullException.ThrowIfNull(category);
            _context.OracleDemoCategories.Add(category);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCategoryAsync(OracleDemoCategory category)
        {
            ArgumentNullException.ThrowIfNull(category);
            _context.OracleDemoCategories.Update(category);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCategoryAsync(int id)
        {
            OracleDemoCategory? item = await _context.OracleDemoCategories.FindAsync(id);
            if (item is not null)
            {
                _context.OracleDemoCategories.Remove(item);
                await _context.SaveChangesAsync();
            }
        }

        public bool CategoryExists(int id)
        {
            return _context.OracleDemoCategories.Any(e => e.Id == id);
        }
    }
}
