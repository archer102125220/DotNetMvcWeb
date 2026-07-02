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
    public class MssqlDemoCategoryService : IMssqlDemoCategoryService
    {
        private readonly MssqlDbContext _context;

        public MssqlDemoCategoryService(MssqlDbContext context)
        {
            _context = context;
        }

        public async Task<List<MssqlDemoCategory>> GetCategoriesAsync()
        {
            return await _context.MssqlDemoCategories
                .AsNoTracking()
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<MssqlDemoCategory?> GetCategoryByIdAsync(int id)
        {
            return await _context.MssqlDemoCategories.FindAsync(id);
        }

        public async Task CreateCategoryAsync(MssqlDemoCategory category)
        {
            category.CreatedAt = DateTime.UtcNow;
            _context.Add(category);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCategoryAsync(MssqlDemoCategory category)
        {
            _context.Update(category);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCategoryAsync(int id)
        {
            MssqlDemoCategory? item = await _context.MssqlDemoCategories.FindAsync(id);
            if (item != null)
            {
                _context.MssqlDemoCategories.Remove(item);
                await _context.SaveChangesAsync();
            }
        }

        public bool CategoryExists(int id)
        {
            return _context.MssqlDemoCategories.Any(e => e.Id == id);
        }
    }
}
