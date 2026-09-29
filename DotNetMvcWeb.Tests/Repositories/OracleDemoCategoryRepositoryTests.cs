using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetMvcWeb.Data;
using DotNetMvcWeb.Models;
using DotNetMvcWeb.Repositories.Implements;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DotNetMvcWeb.Tests.Repositories
{
    public class OracleDemoCategoryRepositoryTests
    {
        private AppDbContext CreateDbContext(string dbName)
        {
            DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $"{dbName}_{Guid.NewGuid()}")
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public void Constructor_WhenContextNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new OracleDemoCategoryRepository(null!));
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

            OracleDemoCategoryRepository repository = new(context);

            List<OracleDemoCategory> result = await repository.GetCategoriesAsync();

            Assert.Equal(2, result.Count);
            Assert.Equal(2, result[0].Id);
        }

        [Fact]
        public async Task GetCategoryByIdAsync_WhenExists_ReturnsCategory()
        {
            using AppDbContext context = CreateDbContext(nameof(GetCategoryByIdAsync_WhenExists_ReturnsCategory));
            context.OracleDemoCategories.Add(new OracleDemoCategory { Id = 10, Name = "Cat10" });
            await context.SaveChangesAsync();

            OracleDemoCategoryRepository repository = new(context);

            OracleDemoCategory? result = await repository.GetCategoryByIdAsync(10);

            Assert.NotNull(result);
            Assert.Equal("Cat10", result.Name);
        }

        [Fact]
        public async Task GetCategoryByIdAsync_WhenNotExists_ReturnsNull()
        {
            using AppDbContext context = CreateDbContext(nameof(GetCategoryByIdAsync_WhenNotExists_ReturnsNull));
            OracleDemoCategoryRepository repository = new(context);

            OracleDemoCategory? result = await repository.GetCategoryByIdAsync(999);

            Assert.Null(result);
        }

        [Fact]
        public async Task AddCategoryAsync_AddsEntity()
        {
            using AppDbContext context = CreateDbContext(nameof(AddCategoryAsync_AddsEntity));
            OracleDemoCategoryRepository repository = new(context);
            OracleDemoCategory newCat = new() { Name = "NewCat", CreatedAt = DateTime.UtcNow };

            await repository.AddCategoryAsync(newCat);

            OracleDemoCategory? saved = await context.OracleDemoCategories.FirstOrDefaultAsync(c => c.Name == "NewCat");
            Assert.NotNull(saved);
        }

        [Fact]
        public async Task UpdateCategoryAsync_UpdatesEntityInDatabase()
        {
            using AppDbContext context = CreateDbContext(nameof(UpdateCategoryAsync_UpdatesEntityInDatabase));
            OracleDemoCategory cat = new() { Id = 20, Name = "Original" };
            context.OracleDemoCategories.Add(cat);
            await context.SaveChangesAsync();

            context.Entry(cat).State = EntityState.Detached;

            OracleDemoCategoryRepository repository = new(context);
            cat.Name = "Modified";

            await repository.UpdateCategoryAsync(cat);

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

            OracleDemoCategoryRepository repository = new(context);

            await repository.DeleteCategoryAsync(30);

            Assert.Null(await context.OracleDemoCategories.FindAsync(30));
        }

        [Fact]
        public async Task DeleteCategoryAsync_WhenNotExists_DoesNothing()
        {
            using AppDbContext context = CreateDbContext(nameof(DeleteCategoryAsync_WhenNotExists_DoesNothing));
            OracleDemoCategoryRepository repository = new(context);

            await repository.DeleteCategoryAsync(999);
        }

        [Fact]
        public async Task CategoryExists_ReturnsTrueForExisting_FalseForNonExisting()
        {
            using AppDbContext context = CreateDbContext(nameof(CategoryExists_ReturnsTrueForExisting_FalseForNonExisting));
            context.OracleDemoCategories.Add(new OracleDemoCategory { Id = 40, Name = "ExistCat" });
            await context.SaveChangesAsync();

            OracleDemoCategoryRepository repository = new(context);

            Assert.True(repository.CategoryExists(40));
            Assert.False(repository.CategoryExists(999));
        }
    }
}
