using DotNetMvcWeb.Data;
using DotNetMvcWeb.Models;
using DotNetMvcWeb.Services.Implements;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DotNetMvcWeb.Tests.Services
{
    public class MysqlDemoCategoryServiceTests
    {
        private MysqlDbContext CreateDbContext(string dbName)
        {
            DbContextOptions<MysqlDbContext> options = new DbContextOptionsBuilder<MysqlDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new MysqlDbContext(options);
        }

        [Fact]
        public async Task GetCategoriesAsync_ReturnsCategoriesOrderedByCreatedAtDesc()
        {
            using MysqlDbContext context = CreateDbContext(nameof(GetCategoriesAsync_ReturnsCategoriesOrderedByCreatedAtDesc));
            DateTime now = DateTime.UtcNow;
            context.MysqlDemoCategories.AddRange(
                new MysqlDemoCategory { Id = 1, Name = "Cat1", CreatedAt = now.AddDays(-1) },
                new MysqlDemoCategory { Id = 2, Name = "Cat2", CreatedAt = now }
            );
            await context.SaveChangesAsync();

            MysqlDemoCategoryService service = new(context);

            List<MysqlDemoCategory> result = await service.GetCategoriesAsync();

            Assert.Equal(2, result.Count);
            Assert.Equal(2, result[0].Id);
        }

        [Fact]
        public async Task GetCategoryByIdAsync_WhenExists_ReturnsCategory()
        {
            using MysqlDbContext context = CreateDbContext(nameof(GetCategoryByIdAsync_WhenExists_ReturnsCategory));
            context.MysqlDemoCategories.Add(new MysqlDemoCategory { Id = 10, Name = "Cat10" });
            await context.SaveChangesAsync();

            MysqlDemoCategoryService service = new(context);

            MysqlDemoCategory? result = await service.GetCategoryByIdAsync(10);

            Assert.NotNull(result);
            Assert.Equal("Cat10", result.Name);
        }

        [Fact]
        public async Task GetCategoryByIdAsync_WhenNotExists_ReturnsNull()
        {
            using MysqlDbContext context = CreateDbContext(nameof(GetCategoryByIdAsync_WhenNotExists_ReturnsNull));
            MysqlDemoCategoryService service = new(context);

            MysqlDemoCategory? result = await service.GetCategoryByIdAsync(999);

            Assert.Null(result);
        }

        [Fact]
        public async Task CreateCategoryAsync_SetsCreatedAtAndAddsEntity()
        {
            using MysqlDbContext context = CreateDbContext(nameof(CreateCategoryAsync_SetsCreatedAtAndAddsEntity));
            MysqlDemoCategoryService service = new(context);
            MysqlDemoCategory newCat = new() { Name = "NewCat" };

            await service.CreateCategoryAsync(newCat);

            MysqlDemoCategory? saved = await context.MysqlDemoCategories.FirstOrDefaultAsync(c => c.Name == "NewCat");
            Assert.NotNull(saved);
            Assert.NotEqual(default, saved.CreatedAt);
        }

        [Fact]
        public async Task UpdateCategoryAsync_UpdatesEntityInDatabase()
        {
            using MysqlDbContext context = CreateDbContext(nameof(UpdateCategoryAsync_UpdatesEntityInDatabase));
            MysqlDemoCategory cat = new() { Id = 20, Name = "Original" };
            context.MysqlDemoCategories.Add(cat);
            await context.SaveChangesAsync();

            context.Entry(cat).State = EntityState.Detached;

            MysqlDemoCategoryService service = new(context);
            cat.Name = "Modified";

            await service.UpdateCategoryAsync(cat);

            MysqlDemoCategory? updated = await context.MysqlDemoCategories.FindAsync(20);
            Assert.NotNull(updated);
            Assert.Equal("Modified", updated.Name);
        }

        [Fact]
        public async Task DeleteCategoryAsync_WhenExists_RemovesCategory()
        {
            using MysqlDbContext context = CreateDbContext(nameof(DeleteCategoryAsync_WhenExists_RemovesCategory));
            context.MysqlDemoCategories.Add(new MysqlDemoCategory { Id = 30, Name = "ToDelete" });
            await context.SaveChangesAsync();

            MysqlDemoCategoryService service = new(context);

            await service.DeleteCategoryAsync(30);

            Assert.Null(await context.MysqlDemoCategories.FindAsync(30));
        }

        [Fact]
        public async Task DeleteCategoryAsync_WhenNotExists_DoesNothing()
        {
            using MysqlDbContext context = CreateDbContext(nameof(DeleteCategoryAsync_WhenNotExists_DoesNothing));
            MysqlDemoCategoryService service = new(context);

            await service.DeleteCategoryAsync(999);
        }

        [Fact]
        public async Task CategoryExists_ReturnsTrueForExisting_FalseForNonExisting()
        {
            using MysqlDbContext context = CreateDbContext(nameof(CategoryExists_ReturnsTrueForExisting_FalseForNonExisting));
            context.MysqlDemoCategories.Add(new MysqlDemoCategory { Id = 40, Name = "ExistCat" });
            await context.SaveChangesAsync();

            MysqlDemoCategoryService service = new(context);

            Assert.True(service.CategoryExists(40));
            Assert.False(service.CategoryExists(999));
        }
    }
}
