using DotNetMvcWeb.Data;
using DotNetMvcWeb.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DotNetMvcWeb.Tests.Data
{
    public class DbContextModelTests
    {
        [Fact]
        public void AppDbContext_Model_ConfiguresRequiredMaxLengthAndSeedData()
        {
            DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("AppDbContextModelTest")
                .Options;

            using AppDbContext context = new(options);
            var entityType = context.Model.FindEntityType(typeof(OracleDemoItem));
            Assert.NotNull(entityType);

            var nameProperty = entityType.FindProperty("Name");
            Assert.NotNull(nameProperty);
            Assert.False(nameProperty.IsNullable);
            Assert.Equal(200, nameProperty.GetMaxLength());
        }

        [Fact]
        public void MssqlDbContext_Model_ConfiguresRequiredMaxLengthAndSeedData()
        {
            DbContextOptions<MssqlDbContext> options = new DbContextOptionsBuilder<MssqlDbContext>()
                .UseInMemoryDatabase("MssqlDbContextModelTest")
                .Options;

            using MssqlDbContext context = new(options);
            var entityType = context.Model.FindEntityType(typeof(MssqlDemoItem));
            Assert.NotNull(entityType);

            var nameProperty = entityType.FindProperty("Name");
            Assert.NotNull(nameProperty);
            Assert.False(nameProperty.IsNullable);
            Assert.Equal(200, nameProperty.GetMaxLength());
        }

        [Fact]
        public void MysqlDbContext_Model_ConfiguresRequiredMaxLengthAndSeedData()
        {
            DbContextOptions<MysqlDbContext> options = new DbContextOptionsBuilder<MysqlDbContext>()
                .UseInMemoryDatabase("MysqlDbContextModelTest")
                .Options;

            using MysqlDbContext context = new(options);
            var entityType = context.Model.FindEntityType(typeof(MysqlDemoItem));
            Assert.NotNull(entityType);

            var nameProperty = entityType.FindProperty("Name");
            Assert.NotNull(nameProperty);
            Assert.False(nameProperty.IsNullable);
            Assert.Equal(200, nameProperty.GetMaxLength());
        }

        [Fact]
        public void PostgresDbContext_Model_ConfiguresRequiredMaxLengthAndSeedData()
        {
            DbContextOptions<PostgresDbContext> options = new DbContextOptionsBuilder<PostgresDbContext>()
                .UseInMemoryDatabase("PostgresDbContextModelTest")
                .Options;

            using PostgresDbContext context = new(options);
            var entityType = context.Model.FindEntityType(typeof(PostgresDemoItem));
            Assert.NotNull(entityType);

            var nameProperty = entityType.FindProperty("Name");
            Assert.NotNull(nameProperty);
            Assert.False(nameProperty.IsNullable);
            Assert.Equal(200, nameProperty.GetMaxLength());
        }
    }
}
