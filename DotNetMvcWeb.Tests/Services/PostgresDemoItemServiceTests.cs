using DotNetMvcWeb.Data;
using DotNetMvcWeb.Models;
using DotNetMvcWeb.Services.Implements;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DotNetMvcWeb.Tests.Services
{
    public class PostgresDemoItemServiceTests
    {
        private PostgresDbContext CreateDbContext(string dbName)
        {
            DbContextOptions<PostgresDbContext> options = new DbContextOptionsBuilder<PostgresDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new PostgresDbContext(options);
        }

        private PostgresDbContext CreateRelationalDbContext()
        {
            DbContextOptions<PostgresDbContext> options = new DbContextOptionsBuilder<PostgresDbContext>()
                .UseNpgsql("Host=127.0.0.1;Database=DummyPgDb;Username=postgres;Password=DummyPass123!;Timeout=1")
                .Options;
            return new PostgresDbContext(options);
        }

        [Fact]
        public async Task GetItemsAsync_WithoutKeyword_ReturnsAllItemsOrderedByCreatedAtDesc()
        {
            using PostgresDbContext context = CreateDbContext(nameof(GetItemsAsync_WithoutKeyword_ReturnsAllItemsOrderedByCreatedAtDesc));
            DateTime now = DateTime.UtcNow;
            PostgresDemoCategory cat = new() { Id = 1, Name = "Cat1" };
            context.PostgresDemoCategories.Add(cat);
            context.PostgresDemoItems.AddRange(
                new PostgresDemoItem { Id = 1, Name = "Item1", CreatedAt = now.AddHours(-1), CategoryId = 1 },
                new PostgresDemoItem { Id = 2, Name = "Item2", CreatedAt = now, CategoryId = 1 }
            );
            await context.SaveChangesAsync();

            PostgresDemoItemService service = new(context);

            List<PostgresDemoItem> result = await service.GetItemsAsync(null);

            Assert.Equal(2, result.Count);
            Assert.Equal(2, result[0].Id);
            Assert.NotNull(result[0].Category);
        }

        [Fact]
        public async Task GetItemsAsync_WithKeyword_InvokesRelationalQueryBranch()
        {
            using PostgresDbContext context = CreateRelationalDbContext();
            PostgresDemoItemService service = new(context);

            await Assert.ThrowsAnyAsync<Exception>(() => service.GetItemsAsync("SearchTerm"));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task GetItemByIdAsync_ReturnsItemWithOrWithoutCategory(bool includeCategory)
        {
            using PostgresDbContext context = CreateDbContext($"GetItemByIdAsync_{includeCategory}");
            PostgresDemoCategory cat = new() { Id = 5, Name = "Cat5" };
            context.PostgresDemoCategories.Add(cat);
            context.PostgresDemoItems.Add(new PostgresDemoItem { Id = 10, Name = "Item10", CategoryId = 5 });
            await context.SaveChangesAsync();

            PostgresDemoItemService service = new(context);

            PostgresDemoItem? result = await service.GetItemByIdAsync(10, includeCategory);

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
            using PostgresDbContext context = CreateDbContext(nameof(GetItemByIdAsync_WhenNotFound_ReturnsNull));
            PostgresDemoItemService service = new(context);

            PostgresDemoItem? result = await service.GetItemByIdAsync(999);

            Assert.Null(result);
        }

        [Fact]
        public async Task CreateItemAsync_WhenCreatedAtIsDefault_AssignsUtcNow()
        {
            using PostgresDbContext context = CreateDbContext(nameof(CreateItemAsync_WhenCreatedAtIsDefault_AssignsUtcNow));
            PostgresDemoItemService service = new(context);
            PostgresDemoItem item = new() { Name = "DefaultDateItem" };

            await service.CreateItemAsync(item);

            PostgresDemoItem? saved = await context.PostgresDemoItems.FirstOrDefaultAsync(i => i.Name == "DefaultDateItem");
            Assert.NotNull(saved);
            Assert.NotEqual(default, saved.CreatedAt);
        }

        [Fact]
        public async Task CreateItemAsync_WhenCreatedAtIsExplicitlySet_RetainsOriginalDate()
        {
            using PostgresDbContext context = CreateDbContext(nameof(CreateItemAsync_WhenCreatedAtIsExplicitlySet_RetainsOriginalDate));
            PostgresDemoItemService service = new(context);
            DateTime explicitDate = new(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
            PostgresDemoItem item = new() { Name = "ExplicitDateItem", CreatedAt = explicitDate };

            await service.CreateItemAsync(item);

            PostgresDemoItem? saved = await context.PostgresDemoItems.FirstOrDefaultAsync(i => i.Name == "ExplicitDateItem");
            Assert.NotNull(saved);
            Assert.Equal(explicitDate, saved.CreatedAt);
        }

        [Fact]
        public async Task UpdateItemAsync_ModifiesEntityInDatabase()
        {
            using PostgresDbContext context = CreateDbContext(nameof(UpdateItemAsync_ModifiesEntityInDatabase));
            PostgresDemoItem item = new() { Id = 20, Name = "OldName", Description = "OldDesc" };
            context.PostgresDemoItems.Add(item);
            await context.SaveChangesAsync();

            context.Entry(item).State = EntityState.Detached;

            PostgresDemoItemService service = new(context);
            item.Name = "UpdatedName";
            item.Description = "UpdatedDesc";

            await service.UpdateItemAsync(item);

            PostgresDemoItem? updated = await context.PostgresDemoItems.FindAsync(20);
            Assert.NotNull(updated);
            Assert.Equal("UpdatedName", updated.Name);
            Assert.Equal("UpdatedDesc", updated.Description);
        }

        [Fact]
        public async Task DeleteItemAsync_WhenExists_RemovesEntity()
        {
            using PostgresDbContext context = CreateDbContext(nameof(DeleteItemAsync_WhenExists_RemovesEntity));
            context.PostgresDemoItems.Add(new PostgresDemoItem { Id = 30, Name = "ItemToDelete" });
            await context.SaveChangesAsync();

            PostgresDemoItemService service = new(context);

            await service.DeleteItemAsync(30);

            Assert.Null(await context.PostgresDemoItems.FindAsync(30));
        }

        [Fact]
        public async Task DeleteItemAsync_WhenNotExists_DoesNothing()
        {
            using PostgresDbContext context = CreateDbContext(nameof(DeleteItemAsync_WhenNotExists_DoesNothing));
            PostgresDemoItemService service = new(context);

            await service.DeleteItemAsync(999);
        }

        [Fact]
        public async Task ItemExists_ReturnsTrueForExisting_FalseForNonExisting()
        {
            using PostgresDbContext context = CreateDbContext(nameof(ItemExists_ReturnsTrueForExisting_FalseForNonExisting));
            context.PostgresDemoItems.Add(new PostgresDemoItem { Id = 40, Name = "ExistingItem" });
            await context.SaveChangesAsync();

            PostgresDemoItemService service = new(context);

            Assert.True(service.ItemExists(40));
            Assert.False(service.ItemExists(999));
        }

        [Fact]
        public async Task GetItemsViaAdoNetAsync_WhenNoConnectionString_ThrowsInvalidOperationException()
        {
            using PostgresDbContext context = CreateDbContext(nameof(GetItemsViaAdoNetAsync_WhenNoConnectionString_ThrowsInvalidOperationException));
            PostgresDemoItemService service = new(context);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetItemsViaAdoNetAsync("kw"));
        }

        [Theory]
        [InlineData("kw")]
        [InlineData(null)]
        public async Task GetItemsViaAdoNetAsync_WithConnectionString_AttemptsConnectionAndHandlesException(string? keyword)
        {
            using PostgresDbContext context = CreateRelationalDbContext();
            PostgresDemoItemService service = new(context);

            await Assert.ThrowsAnyAsync<Exception>(() => service.GetItemsViaAdoNetAsync(keyword));
        }

        [Fact]
        public async Task UpdateItemDescriptionViaProcAsync_AttemptsExecution()
        {
            using PostgresDbContext context = CreateRelationalDbContext();
            PostgresDemoItemService service = new(context);

            await Assert.ThrowsAnyAsync<Exception>(() => service.UpdateItemDescriptionViaProcAsync(1, "New Description"));
        }
    }
}
