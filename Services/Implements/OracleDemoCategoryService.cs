using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetMvcWeb.Data;
using DotNetMvcWeb.Models;
using DotNetMvcWeb.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DotNetMvcWeb.Services.Implements
{
    /// <summary>
    /// [教學註解] 服務層實作 (Service Implementation)
    /// 這裡是實際處理商業邏輯與資料庫互動的地方。將原本寫在 Controller 裡的 DbContext 操作全部集中於此。
    /// 這種做法稱為「Service Layer Pattern」或是簡化版的 Repository Pattern。
    /// </summary>
    public class OracleDemoCategoryService : IOracleDemoCategoryService
    {
        private readonly AppDbContext _context;

        public OracleDemoCategoryService(AppDbContext context)
        {
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

        public async Task CreateCategoryAsync(OracleDemoCategory category)
        {
            category.CreatedAt = DateTime.UtcNow;
            _context.Add(category);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCategoryAsync(OracleDemoCategory category)
        {
            _context.Update(category);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCategoryAsync(int id)
        {
            OracleDemoCategory? item = await _context.OracleDemoCategories.FindAsync(id);
            if (item != null)
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
