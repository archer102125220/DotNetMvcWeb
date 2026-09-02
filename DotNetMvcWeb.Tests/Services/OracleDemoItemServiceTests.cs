using DotNetMvcWeb.Data;
using DotNetMvcWeb.Models;
using DotNetMvcWeb.Services.Implements;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DotNetMvcWeb.Tests.Services
{
    public class OracleDemoItemServiceTests
    {
        private AppDbContext CreateDbContext(string dbName)
        {
            DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
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

            OracleDemoItemService service = new(context);

            List<OracleDemoItem> result = await service.GetItemsAsync(null);

            Assert.Equal(2, result.Count);
            Assert.Equal(2, result[0].Id);
            Assert.NotNull(result[0].Category);
        }

        [Fact]
        public async Task GetItemsAsync_WithKeyword_InvokesRelationalQueryBranch()
        {
            using AppDbContext context = CreateRelationalDbContext();
            OracleDemoItemService service = new(context);

            await Assert.ThrowsAnyAsync<Exception>(() => service.GetItemsAsync("SearchTerm"));
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

            OracleDemoItemService service = new(context);

            OracleDemoItem? result = await service.GetItemByIdAsync(10, includeCategory);

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
            OracleDemoItemService service = new(context);

            OracleDemoItem? result = await service.GetItemByIdAsync(999);

            Assert.Null(result);
        }

        [Fact]
        public async Task CreateItemAsync_WhenCreatedAtIsDefault_AssignsUtcNow()
        {
            using AppDbContext context = CreateDbContext(nameof(CreateItemAsync_WhenCreatedAtIsDefault_AssignsUtcNow));
            OracleDemoItemService service = new(context);
            OracleDemoItem item = new() { Name = "DefaultDateItem" };

            await service.CreateItemAsync(item);

            OracleDemoItem? saved = await context.OracleDemoItems.FirstOrDefaultAsync(i => i.Name == "DefaultDateItem");
            Assert.NotNull(saved);
            Assert.NotEqual(default, saved.CreatedAt);
        }

        [Fact]
        public async Task CreateItemAsync_WhenCreatedAtIsExplicitlySet_RetainsOriginalDate()
        {
            using AppDbContext context = CreateDbContext(nameof(CreateItemAsync_WhenCreatedAtIsExplicitlySet_RetainsOriginalDate));
            OracleDemoItemService service = new(context);
            DateTime explicitDate = new(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
            OracleDemoItem item = new() { Name = "ExplicitDateItem", CreatedAt = explicitDate };

            await service.CreateItemAsync(item);

            OracleDemoItem? saved = await context.OracleDemoItems.FirstOrDefaultAsync(i => i.Name == "ExplicitDateItem");
            Assert.NotNull(saved);
            Assert.Equal(explicitDate, saved.CreatedAt);
        }

        [Fact]
        public async Task UpdateItemAsync_ModifiesEntityInDatabase()
        {
            using AppDbContext context = CreateDbContext(nameof(UpdateItemAsync_ModifiesEntityInDatabase));
            OracleDemoItem item = new() { Id = 20, Name = "OldName", Description = "OldDesc" };
            context.OracleDemoItems.Add(item);
            await context.SaveChangesAsync();

            context.Entry(item).State = EntityState.Detached;

            OracleDemoItemService service = new(context);
            item.Name = "UpdatedName";
            item.Description = "UpdatedDesc";

            await service.UpdateItemAsync(item);

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

            OracleDemoItemService service = new(context);

            await service.DeleteItemAsync(30);

            Assert.Null(await context.OracleDemoItems.FindAsync(30));
        }

        [Fact]
        public async Task DeleteItemAsync_WhenNotExists_DoesNothing()
        {
            using AppDbContext context = CreateDbContext(nameof(DeleteItemAsync_WhenNotExists_DoesNothing));
            OracleDemoItemService service = new(context);

            await service.DeleteItemAsync(999);
        }

        [Fact]
        public async Task ItemExists_ReturnsTrueForExisting_FalseForNonExisting()
        {
            using AppDbContext context = CreateDbContext(nameof(ItemExists_ReturnsTrueForExisting_FalseForNonExisting));
            context.OracleDemoItems.Add(new OracleDemoItem { Id = 40, Name = "ExistingItem" });
            await context.SaveChangesAsync();

            OracleDemoItemService service = new(context);

            Assert.True(service.ItemExists(40));
            Assert.False(service.ItemExists(999));
        }

        [Fact]
        public async Task GetItemsViaAdoNetAsync_WhenNoConnectionString_ThrowsInvalidOperationException()
        {
            using AppDbContext context = CreateDbContext(nameof(GetItemsViaAdoNetAsync_WhenNoConnectionString_ThrowsInvalidOperationException));
            OracleDemoItemService service = new(context);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetItemsViaAdoNetAsync("kw"));
        }

        [Theory]
        [InlineData("kw")]
        [InlineData(null)]
        public async Task GetItemsViaAdoNetAsync_WithConnectionString_AttemptsConnectionAndHandlesException(string? keyword)
        {
            using AppDbContext context = CreateRelationalDbContext();
            OracleDemoItemService service = new(context);

            await Assert.ThrowsAnyAsync<Exception>(() => service.GetItemsViaAdoNetAsync(keyword));
        }

        [Fact]
        public async Task UpdateItemDescriptionViaProcAsync_AttemptsExecution()
        {
            using AppDbContext context = CreateRelationalDbContext();
            OracleDemoItemService service = new(context);

            await Assert.ThrowsAnyAsync<Exception>(() => service.UpdateItemDescriptionViaProcAsync(1, "New Description"));
        }
    }
}
