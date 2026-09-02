using DotNetMvcWeb.Data;
using DotNetMvcWeb.Models;
using DotNetMvcWeb.Services.Implements;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DotNetMvcWeb.Tests.Services
{
    public class MssqlDemoCategoryServiceTests
    {
        private MssqlDbContext CreateDbContext(string dbName)
        {
            DbContextOptions<MssqlDbContext> options = new DbContextOptionsBuilder<MssqlDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new MssqlDbContext(options);
        }

        [Fact]
        public async Task GetCategoriesAsync_ReturnsCategoriesOrderedByCreatedAtDesc()
        {
            using MssqlDbContext context = CreateDbContext(nameof(GetCategoriesAsync_ReturnsCategoriesOrderedByCreatedAtDesc));
            DateTime now = DateTime.UtcNow;
            context.MssqlDemoCategories.AddRange(
                new MssqlDemoCategory { Id = 1, Name = "Cat1", CreatedAt = now.AddDays(-1) },
                new MssqlDemoCategory { Id = 2, Name = "Cat2", CreatedAt = now }
            );
            await context.SaveChangesAsync();

            MssqlDemoCategoryService service = new(context);

            List<MssqlDemoCategory> result = await service.GetCategoriesAsync();

            Assert.Equal(2, result.Count);
            Assert.Equal(2, result[0].Id);
            Assert.Equal(1, result[1].Id);
        }

        [Fact]
        public async Task GetCategoryByIdAsync_WhenExists_ReturnsCategory()
        {
            using MssqlDbContext context = CreateDbContext(nameof(GetCategoryByIdAsync_WhenExists_ReturnsCategory));
            context.MssqlDemoCategories.Add(new MssqlDemoCategory { Id = 10, Name = "Cat10" });
            await context.SaveChangesAsync();

            MssqlDemoCategoryService service = new(context);

            MssqlDemoCategory? result = await service.GetCategoryByIdAsync(10);

            Assert.NotNull(result);
            Assert.Equal("Cat10", result.Name);
        }

        [Fact]
        public async Task GetCategoryByIdAsync_WhenNotExists_ReturnsNull()
        {
            using MssqlDbContext context = CreateDbContext(nameof(GetCategoryByIdAsync_WhenNotExists_ReturnsNull));
            MssqlDemoCategoryService service = new(context);

            MssqlDemoCategory? result = await service.GetCategoryByIdAsync(999);

            Assert.Null(result);
        }

        [Fact]
        public async Task CreateCategoryAsync_SetsCreatedAtAndAddsEntity()
        {
            using MssqlDbContext context = CreateDbContext(nameof(CreateCategoryAsync_SetsCreatedAtAndAddsEntity));
            MssqlDemoCategoryService service = new(context);
            MssqlDemoCategory newCat = new() { Name = "NewCat" };

            await service.CreateCategoryAsync(newCat);

            MssqlDemoCategory? saved = await context.MssqlDemoCategories.FirstOrDefaultAsync(c => c.Name == "NewCat");
            Assert.NotNull(saved);
            Assert.NotEqual(default, saved.CreatedAt);
        }

        [Fact]
        public async Task UpdateCategoryAsync_UpdatesEntityInDatabase()
        {
            using MssqlDbContext context = CreateDbContext(nameof(UpdateCategoryAsync_UpdatesEntityInDatabase));
            MssqlDemoCategory cat = new() { Id = 20, Name = "Original" };
            context.MssqlDemoCategories.Add(cat);
            await context.SaveChangesAsync();

            // Detach to test update
            context.Entry(cat).State = EntityState.Detached;

            MssqlDemoCategoryService service = new(context);
            cat.Name = "Modified";

            await service.UpdateCategoryAsync(cat);

            MssqlDemoCategory? updated = await context.MssqlDemoCategories.FindAsync(20);
            Assert.NotNull(updated);
            Assert.Equal("Modified", updated.Name);
        }

        [Fact]
        public async Task DeleteCategoryAsync_WhenExists_RemovesCategory()
        {
            using MssqlDbContext context = CreateDbContext(nameof(DeleteCategoryAsync_WhenExists_RemovesCategory));
            context.MssqlDemoCategories.Add(new MssqlDemoCategory { Id = 30, Name = "ToDelete" });
            await context.SaveChangesAsync();

            MssqlDemoCategoryService service = new(context);

            await service.DeleteCategoryAsync(30);

            Assert.Null(await context.MssqlDemoCategories.FindAsync(30));
        }

        [Fact]
        public async Task DeleteCategoryAsync_WhenNotExists_DoesNothing()
        {
            using MssqlDbContext context = CreateDbContext(nameof(DeleteCategoryAsync_WhenNotExists_DoesNothing));
            MssqlDemoCategoryService service = new(context);

            await service.DeleteCategoryAsync(999);
            // Verify no exception thrown
        }

        [Fact]
        public async Task CategoryExists_ReturnsTrueForExisting_FalseForNonExisting()
        {
            using MssqlDbContext context = CreateDbContext(nameof(CategoryExists_ReturnsTrueForExisting_FalseForNonExisting));
            context.MssqlDemoCategories.Add(new MssqlDemoCategory { Id = 40, Name = "ExistCat" });
            await context.SaveChangesAsync();

            MssqlDemoCategoryService service = new(context);

            Assert.True(service.CategoryExists(40));
            Assert.False(service.CategoryExists(999));
        }
    }
}
