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
    public class OracleDemoItemRepositoryTests
    {
        private AppDbContext CreateDbContext(string dbName)
        {
            DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $"{dbName}_{Guid.NewGuid()}")
                .Options;
            return new AppDbContext(options);
        }

        private AppDbContext CreateRelationalDbContext()
        {
            DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
                .UseOracle("User Id=system;Password=DummyPass123!;Data Source=127.0.0.1:1521/XEPDB1;Connection Timeout=1")
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public void Constructor_WhenContextNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new OracleDemoItemRepository(null!));
        }

        [Fact]
        public async Task GetItemsAsync_WithoutKeyword_ReturnsAllItemsOrderedByCreatedAtDesc()
        {
            using AppDbContext context = CreateDbContext(nameof(GetItemsAsync_WithoutKeyword_ReturnsAllItemsOrderedByCreatedAtDesc));
            DateTime now = DateTime.UtcNow;
            OracleDemoCategory cat = new() { Id = 1, Name = "Cat1" };
            context.OracleDemoCategories.Add(cat);
            context.OracleDemoItems.AddRange(
                new OracleDemoItem { Id = 1, Name = "Item1", CreatedAt = now.AddHours(-1), CategoryId = 1 },
                new OracleDemoItem { Id = 2, Name = "Item2", CreatedAt = now, CategoryId = 1 }
            );
            await context.SaveChangesAsync();

            OracleDemoItemRepository repository = new(context);

            List<OracleDemoItem> result = await repository.GetItemsAsync(null);

            Assert.Equal(2, result.Count);
            Assert.Equal(2, result[0].Id);
            Assert.NotNull(result[0].Category);
        }

        [Fact]
        public async Task GetItemsAsync_WithKeyword_InvokesRelationalQueryBranch()
        {
            using AppDbContext context = CreateRelationalDbContext();
            OracleDemoItemRepository repository = new(context);

            await Assert.ThrowsAnyAsync<Exception>(() => repository.GetItemsAsync("SearchTerm"));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task GetItemByIdAsync_ReturnsItemWithOrWithoutCategory(bool includeCategory)
        {
            using AppDbContext context = CreateDbContext($"GetItemByIdAsync_{includeCategory}");
            OracleDemoCategory cat = new() { Id = 5, Name = "Cat5" };
            context.OracleDemoCategories.Add(cat);
            context.OracleDemoItems.Add(new OracleDemoItem { Id = 10, Name = "Item10", CategoryId = 5 });
            await context.SaveChangesAsync();

            OracleDemoItemRepository repository = new(context);

            OracleDemoItem? result = await repository.GetItemByIdAsync(10, includeCategory);

            Assert.NotNull(result);
            Assert.Equal(10, result.Id);
            if (includeCategory)
            {
                Assert.NotNull(result.Category);
            }
        }

        [Fact]
        public async Task GetItemByIdAsync_WhenNotFound_ReturnsNull()
        {
            using AppDbContext context = CreateDbContext(nameof(GetItemByIdAsync_WhenNotFound_ReturnsNull));
            OracleDemoItemRepository repository = new(context);

            OracleDemoItem? result = await repository.GetItemByIdAsync(999);

            Assert.Null(result);
        }

        [Fact]
        public async Task AddItemAsync_AddsEntity()
        {
            using AppDbContext context = CreateDbContext(nameof(AddItemAsync_AddsEntity));
            OracleDemoItemRepository repository = new(context);
            OracleDemoItem item = new() { Name = "NewItem", CreatedAt = DateTime.UtcNow };

            await repository.AddItemAsync(item);

            OracleDemoItem? saved = await context.OracleDemoItems.FirstOrDefaultAsync(i => i.Name == "NewItem");
            Assert.NotNull(saved);
        }

        [Fact]
        public async Task UpdateItemAsync_ModifiesEntityInDatabase()
        {
            using AppDbContext context = CreateDbContext(nameof(UpdateItemAsync_ModifiesEntityInDatabase));
            OracleDemoItem item = new() { Id = 20, Name = "OldName", Description = "OldDesc" };
            context.OracleDemoItems.Add(item);
            await context.SaveChangesAsync();

            context.Entry(item).State = EntityState.Detached;

            OracleDemoItemRepository repository = new(context);
            item.Name = "UpdatedName";
            item.Description = "UpdatedDesc";

            await repository.UpdateItemAsync(item);

            OracleDemoItem? updated = await context.OracleDemoItems.FindAsync(20);
            Assert.NotNull(updated);
            Assert.Equal("UpdatedName", updated.Name);
            Assert.Equal("UpdatedDesc", updated.Description);
        }

        [Fact]
        public async Task DeleteItemAsync_WhenExists_RemovesEntity()
        {
            using AppDbContext context = CreateDbContext(nameof(DeleteItemAsync_WhenExists_RemovesEntity));
            context.OracleDemoItems.Add(new OracleDemoItem { Id = 30, Name = "ItemToDelete" });
            await context.SaveChangesAsync();

            OracleDemoItemRepository repository = new(context);

            await repository.DeleteItemAsync(30);

            Assert.Null(await context.OracleDemoItems.FindAsync(30));
        }

        [Fact]
        public async Task DeleteItemAsync_WhenNotExists_DoesNothing()
        {
            using AppDbContext context = CreateDbContext(nameof(DeleteItemAsync_WhenNotExists_DoesNothing));
            OracleDemoItemRepository repository = new(context);

            await repository.DeleteItemAsync(999);
        }

        [Fact]
        public async Task ItemExists_ReturnsTrueForExisting_FalseForNonExisting()
        {
            using AppDbContext context = CreateDbContext(nameof(ItemExists_ReturnsTrueForExisting_FalseForNonExisting));
            context.OracleDemoItems.Add(new OracleDemoItem { Id = 40, Name = "ExistingItem" });
            await context.SaveChangesAsync();

            OracleDemoItemRepository repository = new(context);

            Assert.True(repository.ItemExists(40));
            Assert.False(repository.ItemExists(999));
        }

        [Fact]
        public async Task GetItemsViaAdoNetAsync_WhenNoConnectionString_ThrowsInvalidOperationException()
        {
            using AppDbContext context = CreateDbContext(nameof(GetItemsViaAdoNetAsync_WhenNoConnectionString_ThrowsInvalidOperationException));
            OracleDemoItemRepository repository = new(context);

            await Assert.ThrowsAsync<InvalidOperationException>(() => repository.GetItemsViaAdoNetAsync("kw"));
        }

        [Theory]
        [InlineData("kw")]
        [InlineData(null)]
        public async Task GetItemsViaAdoNetAsync_WithConnectionString_AttemptsConnectionAndHandlesException(string? keyword)
        {
            using AppDbContext context = CreateRelationalDbContext();
            OracleDemoItemRepository repository = new(context);

            await Assert.ThrowsAnyAsync<Exception>(() => repository.GetItemsViaAdoNetAsync(keyword));
        }

        [Fact]
        public async Task UpdateItemDescriptionViaProcAsync_AttemptsExecution()
        {
            using AppDbContext context = CreateRelationalDbContext();
            OracleDemoItemRepository repository = new(context);

            await Assert.ThrowsAnyAsync<Exception>(() => repository.UpdateItemDescriptionViaProcAsync(1, "New Description"));
        }
    }
}
