using DotNetMvcWeb.Data;
using DotNetMvcWeb.Models;
using DotNetMvcWeb.Services.Implements;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DotNetMvcWeb.Tests.Services
{
    public class OracleDemoCategoryServiceTests
    {
        private AppDbContext CreateDbContext(string dbName)
        {
            DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task GetCategoriesAsync_ReturnsCategoriesOrderedByCreatedAtDesc()
        {
            using AppDbContext context = CreateDbContext(nameof(GetCategoriesAsync_ReturnsCategoriesOrderedByCreatedAtDesc));
            DateTime now = DateTime.UtcNow;
            context.OracleDemoCategories.AddRange(
                new OracleDemoCategory { Id = 1, Name = "Cat1", CreatedAt = now.AddDays(-1) },
                new OracleDemoCategory { Id = 2, Name = "Cat2", CreatedAt = now }
            );
            await context.SaveChangesAsync();

            OracleDemoCategoryService service = new(context);

            List<OracleDemoCategory> result = await service.GetCategoriesAsync();

            Assert.Equal(2, result.Count);
            Assert.Equal(2, result[0].Id);
        }

        [Fact]
        public async Task GetCategoryByIdAsync_WhenExists_ReturnsCategory()
        {
            using AppDbContext context = CreateDbContext(nameof(GetCategoryByIdAsync_WhenExists_ReturnsCategory));
            context.OracleDemoCategories.Add(new OracleDemoCategory { Id = 10, Name = "Cat10" });
            await context.SaveChangesAsync();

            OracleDemoCategoryService service = new(context);

            OracleDemoCategory? result = await service.GetCategoryByIdAsync(10);

            Assert.NotNull(result);
            Assert.Equal("Cat10", result.Name);
        }

        [Fact]
        public async Task GetCategoryByIdAsync_WhenNotExists_ReturnsNull()
        {
            using AppDbContext context = CreateDbContext(nameof(GetCategoryByIdAsync_WhenNotExists_ReturnsNull));
            OracleDemoCategoryService service = new(context);

            OracleDemoCategory? result = await service.GetCategoryByIdAsync(999);

            Assert.Null(result);
        }

        [Fact]
        public async Task CreateCategoryAsync_SetsCreatedAtAndAddsEntity()
        {
            using AppDbContext context = CreateDbContext(nameof(CreateCategoryAsync_SetsCreatedAtAndAddsEntity));
            OracleDemoCategoryService service = new(context);
            OracleDemoCategory newCat = new() { Name = "NewCat" };

            await service.CreateCategoryAsync(newCat);

            OracleDemoCategory? saved = await context.OracleDemoCategories.FirstOrDefaultAsync(c => c.Name == "NewCat");
            Assert.NotNull(saved);
            Assert.NotEqual(default, saved.CreatedAt);
        }

        [Fact]
        public async Task UpdateCategoryAsync_UpdatesEntityInDatabase()
        {
            using AppDbContext context = CreateDbContext(nameof(UpdateCategoryAsync_UpdatesEntityInDatabase));
            OracleDemoCategory cat = new() { Id = 20, Name = "Original" };
            context.OracleDemoCategories.Add(cat);
            await context.SaveChangesAsync();

            context.Entry(cat).State = EntityState.Detached;

            OracleDemoCategoryService service = new(context);
            cat.Name = "Modified";

            await service.UpdateCategoryAsync(cat);

            OracleDemoCategory? updated = await context.OracleDemoCategories.FindAsync(20);
            Assert.NotNull(updated);
            Assert.Equal("Modified", updated.Name);
        }

        [Fact]
        public async Task DeleteCategoryAsync_WhenExists_RemovesCategory()
        {
            using AppDbContext context = CreateDbContext(nameof(DeleteCategoryAsync_WhenExists_RemovesCategory));
            context.OracleDemoCategories.Add(new OracleDemoCategory { Id = 30, Name = "ToDelete" });
            await context.SaveChangesAsync();

            OracleDemoCategoryService service = new(context);

            await service.DeleteCategoryAsync(30);

            Assert.Null(await context.OracleDemoCategories.FindAsync(30));
        }

        [Fact]
        public async Task DeleteCategoryAsync_WhenNotExists_DoesNothing()
        {
            using AppDbContext context = CreateDbContext(nameof(DeleteCategoryAsync_WhenNotExists_DoesNothing));
            OracleDemoCategoryService service = new(context);

            await service.DeleteCategoryAsync(999);
        }

        [Fact]
        public async Task CategoryExists_ReturnsTrueForExisting_FalseForNonExisting()
        {
            using AppDbContext context = CreateDbContext(nameof(CategoryExists_ReturnsTrueForExisting_FalseForNonExisting));
            context.OracleDemoCategories.Add(new OracleDemoCategory { Id = 40, Name = "ExistCat" });
            await context.SaveChangesAsync();

            OracleDemoCategoryService service = new(context);

            Assert.True(service.CategoryExists(40));
            Assert.False(service.CategoryExists(999));
        }
    }
}
