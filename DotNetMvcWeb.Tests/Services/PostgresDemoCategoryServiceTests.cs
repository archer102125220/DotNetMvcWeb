using DotNetMvcWeb.Data;
using DotNetMvcWeb.Models;
using DotNetMvcWeb.Services.Implements;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DotNetMvcWeb.Tests.Services
{
    public class PostgresDemoCategoryServiceTests
    {
        private PostgresDbContext CreateDbContext(string dbName)
        {
            DbContextOptions<PostgresDbContext> options = new DbContextOptionsBuilder<PostgresDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new PostgresDbContext(options);
        }

        [Fact]
        public async Task GetCategoriesAsync_ReturnsCategoriesOrderedByCreatedAtDesc()
        {
            using PostgresDbContext context = CreateDbContext(nameof(GetCategoriesAsync_ReturnsCategoriesOrderedByCreatedAtDesc));
            DateTime now = DateTime.UtcNow;
            context.PostgresDemoCategories.AddRange(
                new PostgresDemoCategory { Id = 1, Name = "Cat1", CreatedAt = now.AddDays(-1) },
                new PostgresDemoCategory { Id = 2, Name = "Cat2", CreatedAt = now }
            );
            await context.SaveChangesAsync();

            PostgresDemoCategoryService service = new(context);

            List<PostgresDemoCategory> result = await service.GetCategoriesAsync();

            Assert.Equal(2, result.Count);
            Assert.Equal(2, result[0].Id);
        }

        [Fact]
        public async Task GetCategoryByIdAsync_WhenExists_ReturnsCategory()
        {
            using PostgresDbContext context = CreateDbContext(nameof(GetCategoryByIdAsync_WhenExists_ReturnsCategory));
            context.PostgresDemoCategories.Add(new PostgresDemoCategory { Id = 10, Name = "Cat10" });
            await context.SaveChangesAsync();

            PostgresDemoCategoryService service = new(context);

            PostgresDemoCategory? result = await service.GetCategoryByIdAsync(10);

            Assert.NotNull(result);
            Assert.Equal("Cat10", result.Name);
        }

        [Fact]
        public async Task GetCategoryByIdAsync_WhenNotExists_ReturnsNull()
        {
            using PostgresDbContext context = CreateDbContext(nameof(GetCategoryByIdAsync_WhenNotExists_ReturnsNull));
            PostgresDemoCategoryService service = new(context);

            PostgresDemoCategory? result = await service.GetCategoryByIdAsync(999);

            Assert.Null(result);
        }

        [Fact]
        public async Task CreateCategoryAsync_SetsCreatedAtAndAddsEntity()
        {
            using PostgresDbContext context = CreateDbContext(nameof(CreateCategoryAsync_SetsCreatedAtAndAddsEntity));
            PostgresDemoCategoryService service = new(context);
            PostgresDemoCategory newCat = new() { Name = "NewCat" };

            await service.CreateCategoryAsync(newCat);

            PostgresDemoCategory? saved = await context.PostgresDemoCategories.FirstOrDefaultAsync(c => c.Name == "NewCat");
            Assert.NotNull(saved);
            Assert.NotEqual(default, saved.CreatedAt);
        }

        [Fact]
        public async Task UpdateCategoryAsync_UpdatesEntityInDatabase()
        {
            using PostgresDbContext context = CreateDbContext(nameof(UpdateCategoryAsync_UpdatesEntityInDatabase));
            PostgresDemoCategory cat = new() { Id = 20, Name = "Original" };
            context.PostgresDemoCategories.Add(cat);
            await context.SaveChangesAsync();

            context.Entry(cat).State = EntityState.Detached;

            PostgresDemoCategoryService service = new(context);
            cat.Name = "Modified";

            await service.UpdateCategoryAsync(cat);

            PostgresDemoCategory? updated = await context.PostgresDemoCategories.FindAsync(20);
            Assert.NotNull(updated);
            Assert.Equal("Modified", updated.Name);
        }

        [Fact]
        public async Task DeleteCategoryAsync_WhenExists_RemovesCategory()
        {
            using PostgresDbContext context = CreateDbContext(nameof(DeleteCategoryAsync_WhenExists_RemovesCategory));
            context.PostgresDemoCategories.Add(new PostgresDemoCategory { Id = 30, Name = "ToDelete" });
            await context.SaveChangesAsync();

            PostgresDemoCategoryService service = new(context);

            await service.DeleteCategoryAsync(30);

            Assert.Null(await context.PostgresDemoCategories.FindAsync(30));
        }

        [Fact]
        public async Task DeleteCategoryAsync_WhenNotExists_DoesNothing()
        {
            using PostgresDbContext context = CreateDbContext(nameof(DeleteCategoryAsync_WhenNotExists_DoesNothing));
            PostgresDemoCategoryService service = new(context);

            await service.DeleteCategoryAsync(999);
        }

        [Fact]
        public async Task CategoryExists_ReturnsTrueForExisting_FalseForNonExisting()
        {
            using PostgresDbContext context = CreateDbContext(nameof(CategoryExists_ReturnsTrueForExisting_FalseForNonExisting));
            context.PostgresDemoCategories.Add(new PostgresDemoCategory { Id = 40, Name = "ExistCat" });
            await context.SaveChangesAsync();

            PostgresDemoCategoryService service = new(context);

            Assert.True(service.CategoryExists(40));
            Assert.False(service.CategoryExists(999));
        }
    }
}
