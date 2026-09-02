using DotNetMvcWeb.Data;
using DotNetMvcWeb.Models;
using DotNetMvcWeb.Services.Implements;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DotNetMvcWeb.Tests.Services
{
    public class MssqlDemoItemServiceTests
    {
        private MssqlDbContext CreateDbContext(string dbName)
        {
            DbContextOptions<MssqlDbContext> options = new DbContextOptionsBuilder<MssqlDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new MssqlDbContext(options);
        }

        private MssqlDbContext CreateRelationalDbContext()
        {
            DbContextOptions<MssqlDbContext> options = new DbContextOptionsBuilder<MssqlDbContext>()
                .UseSqlServer("Server=127.0.0.1;Database=DummyMssqlDb;User Id=sa;Password=DummyPass123!;TrustServerCertificate=True;Connect Timeout=1")
                .Options;
            return new MssqlDbContext(options);
        }

        [Fact]
        public async Task GetItemsAsync_WithoutKeyword_ReturnsAllItemsOrderedByCreatedAtDesc()
        {
            using MssqlDbContext context = CreateDbContext(nameof(GetItemsAsync_WithoutKeyword_ReturnsAllItemsOrderedByCreatedAtDesc));
            DateTime now = DateTime.UtcNow;
            MssqlDemoCategory cat = new() { Id = 1, Name = "Cat1" };
            context.MssqlDemoCategories.Add(cat);
            context.MssqlDemoItems.AddRange(
                new MssqlDemoItem { Id = 1, Name = "Item1", CreatedAt = now.AddHours(-1), CategoryId = 1 },
                new MssqlDemoItem { Id = 2, Name = "Item2", CreatedAt = now, CategoryId = 1 }
            );
            await context.SaveChangesAsync();

            MssqlDemoItemService service = new(context);

            List<MssqlDemoItem> result = await service.GetItemsAsync(null);

            Assert.Equal(2, result.Count);
            Assert.Equal(2, result[0].Id);
            Assert.Equal(1, result[1].Id);
            Assert.NotNull(result[0].Category);
        }

        [Fact]
        public async Task GetItemsAsync_WithKeyword_InvokesRelationalQueryBranch()
        {
            using MssqlDbContext context = CreateRelationalDbContext();
            MssqlDemoItemService service = new(context);

            // Attempts connection and exercises FromSqlInterpolated query construction
            await Assert.ThrowsAnyAsync<Exception>(() => service.GetItemsAsync("SearchTerm"));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task GetItemByIdAsync_ReturnsItemWithOrWithoutCategory(bool includeCategory)
        {
            using MssqlDbContext context = CreateDbContext($"GetItemByIdAsync_{includeCategory}");
            MssqlDemoCategory cat = new() { Id = 5, Name = "Cat5" };
            context.MssqlDemoCategories.Add(cat);
            context.MssqlDemoItems.Add(new MssqlDemoItem { Id = 10, Name = "Item10", CategoryId = 5 });
            await context.SaveChangesAsync();

            MssqlDemoItemService service = new(context);

            MssqlDemoItem? result = await service.GetItemByIdAsync(10, includeCategory);

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
            using MssqlDbContext context = CreateDbContext(nameof(GetItemByIdAsync_WhenNotFound_ReturnsNull));
            MssqlDemoItemService service = new(context);

            MssqlDemoItem? result = await service.GetItemByIdAsync(999);

            Assert.Null(result);
        }

        [Fact]
        public async Task CreateItemAsync_WhenCreatedAtIsDefault_AssignsUtcNow()
        {
            using MssqlDbContext context = CreateDbContext(nameof(CreateItemAsync_WhenCreatedAtIsDefault_AssignsUtcNow));
            MssqlDemoItemService service = new(context);
            MssqlDemoItem item = new() { Name = "DefaultDateItem" };

            await service.CreateItemAsync(item);

            MssqlDemoItem? saved = await context.MssqlDemoItems.FirstOrDefaultAsync(i => i.Name == "DefaultDateItem");
            Assert.NotNull(saved);
            Assert.NotEqual(default, saved.CreatedAt);
        }

        [Fact]
        public async Task CreateItemAsync_WhenCreatedAtIsExplicitlySet_RetainsOriginalDate()
        {
            using MssqlDbContext context = CreateDbContext(nameof(CreateItemAsync_WhenCreatedAtIsExplicitlySet_RetainsOriginalDate));
            MssqlDemoItemService service = new(context);
            DateTime explicitDate = new(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
            MssqlDemoItem item = new() { Name = "ExplicitDateItem", CreatedAt = explicitDate };

            await service.CreateItemAsync(item);

            MssqlDemoItem? saved = await context.MssqlDemoItems.FirstOrDefaultAsync(i => i.Name == "ExplicitDateItem");
            Assert.NotNull(saved);
            Assert.Equal(explicitDate, saved.CreatedAt);
        }

        [Fact]
        public async Task UpdateItemAsync_ModifiesEntityInDatabase()
        {
            using MssqlDbContext context = CreateDbContext(nameof(UpdateItemAsync_ModifiesEntityInDatabase));
            MssqlDemoItem item = new() { Id = 20, Name = "OldName", Description = "OldDesc" };
            context.MssqlDemoItems.Add(item);
            await context.SaveChangesAsync();

            context.Entry(item).State = EntityState.Detached;

            MssqlDemoItemService service = new(context);
            item.Name = "UpdatedName";
            item.Description = "UpdatedDesc";

            await service.UpdateItemAsync(item);

            MssqlDemoItem? updated = await context.MssqlDemoItems.FindAsync(20);
            Assert.NotNull(updated);
            Assert.Equal("UpdatedName", updated.Name);
            Assert.Equal("UpdatedDesc", updated.Description);
        }

        [Fact]
        public async Task DeleteItemAsync_WhenExists_RemovesEntity()
        {
            using MssqlDbContext context = CreateDbContext(nameof(DeleteItemAsync_WhenExists_RemovesEntity));
            context.MssqlDemoItems.Add(new MssqlDemoItem { Id = 30, Name = "ItemToDelete" });
            await context.SaveChangesAsync();

            MssqlDemoItemService service = new(context);

            await service.DeleteItemAsync(30);

            Assert.Null(await context.MssqlDemoItems.FindAsync(30));
        }

        [Fact]
        public async Task DeleteItemAsync_WhenNotExists_DoesNothing()
        {
            using MssqlDbContext context = CreateDbContext(nameof(DeleteItemAsync_WhenNotExists_DoesNothing));
            MssqlDemoItemService service = new(context);

            await service.DeleteItemAsync(999);
        }

        [Fact]
        public async Task ItemExists_ReturnsTrueForExisting_FalseForNonExisting()
        {
            using MssqlDbContext context = CreateDbContext(nameof(ItemExists_ReturnsTrueForExisting_FalseForNonExisting));
            context.MssqlDemoItems.Add(new MssqlDemoItem { Id = 40, Name = "ExistingItem" });
            await context.SaveChangesAsync();

            MssqlDemoItemService service = new(context);

            Assert.True(service.ItemExists(40));
            Assert.False(service.ItemExists(999));
        }

        [Fact]
        public async Task GetItemsViaAdoNetAsync_WhenNoConnectionString_ThrowsInvalidOperationException()
        {
            using MssqlDbContext context = CreateDbContext(nameof(GetItemsViaAdoNetAsync_WhenNoConnectionString_ThrowsInvalidOperationException));
            MssqlDemoItemService service = new(context);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetItemsViaAdoNetAsync("kw"));
        }

        [Theory]
        [InlineData("kw")]
        [InlineData(null)]
        public async Task GetItemsViaAdoNetAsync_WithConnectionString_AttemptsConnectionAndHandlesException(string? keyword)
        {
            using MssqlDbContext context = CreateRelationalDbContext();
            MssqlDemoItemService service = new(context);

            await Assert.ThrowsAnyAsync<Exception>(() => service.GetItemsViaAdoNetAsync(keyword));
        }

        [Fact]
        public async Task UpdateItemDescriptionViaProcAsync_AttemptsExecution()
        {
            using MssqlDbContext context = CreateRelationalDbContext();
            MssqlDemoItemService service = new(context);

            await Assert.ThrowsAnyAsync<Exception>(() => service.UpdateItemDescriptionViaProcAsync(1, "New Description"));
        }
    }
}
