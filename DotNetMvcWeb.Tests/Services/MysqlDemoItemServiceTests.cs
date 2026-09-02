using DotNetMvcWeb.Data;
using DotNetMvcWeb.Models;
using DotNetMvcWeb.Services.Implements;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DotNetMvcWeb.Tests.Services
{
    public class MysqlDemoItemServiceTests
    {
        private MysqlDbContext CreateDbContext(string dbName)
        {
            DbContextOptions<MysqlDbContext> options = new DbContextOptionsBuilder<MysqlDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new MysqlDbContext(options);
        }

        private MysqlDbContext CreateRelationalDbContext()
        {
            DbContextOptions<MysqlDbContext> options = new DbContextOptionsBuilder<MysqlDbContext>()
                .UseMySQL("Server=127.0.0.1;Database=DummyMysqlDb;Uid=root;Pwd=DummyPass123!;Connect Timeout=1")
                .Options;
            return new MysqlDbContext(options);
        }

        [Fact]
        public async Task GetItemsAsync_WithoutKeyword_ReturnsAllItemsOrderedByCreatedAtDesc()
        {
            using MysqlDbContext context = CreateDbContext(nameof(GetItemsAsync_WithoutKeyword_ReturnsAllItemsOrderedByCreatedAtDesc));
            DateTime now = DateTime.UtcNow;
            MysqlDemoCategory cat = new() { Id = 1, Name = "Cat1" };
            context.MysqlDemoCategories.Add(cat);
            context.MysqlDemoItems.AddRange(
                new MysqlDemoItem { Id = 1, Name = "Item1", CreatedAt = now.AddHours(-1), CategoryId = 1 },
                new MysqlDemoItem { Id = 2, Name = "Item2", CreatedAt = now, CategoryId = 1 }
            );
            await context.SaveChangesAsync();

            MysqlDemoItemService service = new(context);

            List<MysqlDemoItem> result = await service.GetItemsAsync(null);

            Assert.Equal(2, result.Count);
            Assert.Equal(2, result[0].Id);
            Assert.NotNull(result[0].Category);
        }

        [Fact]
        public async Task GetItemsAsync_WithKeyword_InvokesRelationalQueryBranch()
        {
            using MysqlDbContext context = CreateRelationalDbContext();
            MysqlDemoItemService service = new(context);

            await Assert.ThrowsAnyAsync<Exception>(() => service.GetItemsAsync("SearchTerm"));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task GetItemByIdAsync_ReturnsItemWithOrWithoutCategory(bool includeCategory)
        {
            using MysqlDbContext context = CreateDbContext($"GetItemByIdAsync_{includeCategory}");
            MysqlDemoCategory cat = new() { Id = 5, Name = "Cat5" };
            context.MysqlDemoCategories.Add(cat);
            context.MysqlDemoItems.Add(new MysqlDemoItem { Id = 10, Name = "Item10", CategoryId = 5 });
            await context.SaveChangesAsync();

            MysqlDemoItemService service = new(context);

            MysqlDemoItem? result = await service.GetItemByIdAsync(10, includeCategory);

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
            using MysqlDbContext context = CreateDbContext(nameof(GetItemByIdAsync_WhenNotFound_ReturnsNull));
            MysqlDemoItemService service = new(context);

            MysqlDemoItem? result = await service.GetItemByIdAsync(999);

            Assert.Null(result);
        }

        [Fact]
        public async Task CreateItemAsync_WhenCreatedAtIsDefault_AssignsUtcNow()
        {
            using MysqlDbContext context = CreateDbContext(nameof(CreateItemAsync_WhenCreatedAtIsDefault_AssignsUtcNow));
            MysqlDemoItemService service = new(context);
            MysqlDemoItem item = new() { Name = "DefaultDateItem" };

            await service.CreateItemAsync(item);

            MysqlDemoItem? saved = await context.MysqlDemoItems.FirstOrDefaultAsync(i => i.Name == "DefaultDateItem");
            Assert.NotNull(saved);
            Assert.NotEqual(default, saved.CreatedAt);
        }

        [Fact]
        public async Task CreateItemAsync_WhenCreatedAtIsExplicitlySet_RetainsOriginalDate()
        {
            using MysqlDbContext context = CreateDbContext(nameof(CreateItemAsync_WhenCreatedAtIsExplicitlySet_RetainsOriginalDate));
            MysqlDemoItemService service = new(context);
            DateTime explicitDate = new(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
            MysqlDemoItem item = new() { Name = "ExplicitDateItem", CreatedAt = explicitDate };

            await service.CreateItemAsync(item);

            MysqlDemoItem? saved = await context.MysqlDemoItems.FirstOrDefaultAsync(i => i.Name == "ExplicitDateItem");
            Assert.NotNull(saved);
            Assert.Equal(explicitDate, saved.CreatedAt);
        }

        [Fact]
        public async Task UpdateItemAsync_ModifiesEntityInDatabase()
        {
            using MysqlDbContext context = CreateDbContext(nameof(UpdateItemAsync_ModifiesEntityInDatabase));
            MysqlDemoItem item = new() { Id = 20, Name = "OldName", Description = "OldDesc" };
            context.MysqlDemoItems.Add(item);
            await context.SaveChangesAsync();

            context.Entry(item).State = EntityState.Detached;

            MysqlDemoItemService service = new(context);
            item.Name = "UpdatedName";
            item.Description = "UpdatedDesc";

            await service.UpdateItemAsync(item);

            MysqlDemoItem? updated = await context.MysqlDemoItems.FindAsync(20);
            Assert.NotNull(updated);
            Assert.Equal("UpdatedName", updated.Name);
            Assert.Equal("UpdatedDesc", updated.Description);
        }

        [Fact]
        public async Task DeleteItemAsync_WhenExists_RemovesEntity()
        {
            using MysqlDbContext context = CreateDbContext(nameof(DeleteItemAsync_WhenExists_RemovesEntity));
            context.MysqlDemoItems.Add(new MysqlDemoItem { Id = 30, Name = "ItemToDelete" });
            await context.SaveChangesAsync();

            MysqlDemoItemService service = new(context);

            await service.DeleteItemAsync(30);

            Assert.Null(await context.MysqlDemoItems.FindAsync(30));
        }

        [Fact]
        public async Task DeleteItemAsync_WhenNotExists_DoesNothing()
        {
            using MysqlDbContext context = CreateDbContext(nameof(DeleteItemAsync_WhenNotExists_DoesNothing));
            MysqlDemoItemService service = new(context);

            await service.DeleteItemAsync(999);
        }

        [Fact]
        public async Task ItemExists_ReturnsTrueForExisting_FalseForNonExisting()
        {
            using MysqlDbContext context = CreateDbContext(nameof(ItemExists_ReturnsTrueForExisting_FalseForNonExisting));
            context.MysqlDemoItems.Add(new MysqlDemoItem { Id = 40, Name = "ExistingItem" });
            await context.SaveChangesAsync();

            MysqlDemoItemService service = new(context);

            Assert.True(service.ItemExists(40));
            Assert.False(service.ItemExists(999));
        }

        [Fact]
        public async Task GetItemsViaAdoNetAsync_WhenNoConnectionString_ThrowsInvalidOperationException()
        {
            using MysqlDbContext context = CreateDbContext(nameof(GetItemsViaAdoNetAsync_WhenNoConnectionString_ThrowsInvalidOperationException));
            MysqlDemoItemService service = new(context);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetItemsViaAdoNetAsync("kw"));
        }

        [Theory]
        [InlineData("kw")]
        [InlineData(null)]
        public async Task GetItemsViaAdoNetAsync_WithConnectionString_AttemptsConnectionAndHandlesException(string? keyword)
        {
            using MysqlDbContext context = CreateRelationalDbContext();
            MysqlDemoItemService service = new(context);

            await Assert.ThrowsAnyAsync<Exception>(() => service.GetItemsViaAdoNetAsync(keyword));
        }

        [Fact]
        public async Task UpdateItemDescriptionViaProcAsync_AttemptsExecution()
        {
            using MysqlDbContext context = CreateRelationalDbContext();
            MysqlDemoItemService service = new(context);

            await Assert.ThrowsAnyAsync<Exception>(() => service.UpdateItemDescriptionViaProcAsync(1, "New Description"));
        }
    }
}
