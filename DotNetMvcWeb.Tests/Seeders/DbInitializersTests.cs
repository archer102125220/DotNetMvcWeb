using DotNetMvcWeb.Data;
using DotNetMvcWeb.Seeders;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DotNetMvcWeb.Tests.Seeders
{
    public class DbInitializersTests
    {
        [Fact]
        public void DbInitializer_SeedsOracleItemsAndHandlesSecondCallEarlyReturn()
        {
            // Arrange
            DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "OracleSeederTest")
                .Options;

            using (AppDbContext context = new(options))
            {
                // Act - First call (Seeds data)
                DbInitializer.Initialize(context);

                // Assert
                Assert.True(context.OracleDemoItems.Any(i => i.Name == "動態種子資料 1"));
                Assert.True(context.OracleDemoItems.Any(i => i.Name == "動態種子資料 2"));
            }

            using (AppDbContext context = new(options))
            {
                int countBefore = context.OracleDemoItems.Count();

                // Act - Second call (Triggers early return)
                DbInitializer.Initialize(context);

                int countAfter = context.OracleDemoItems.Count();

                // Assert - Count should not increase
                Assert.Equal(countBefore, countAfter);
            }
        }

        [Fact]
        public void MssqlDbInitializer_SeedsItemsAndHandlesSecondCallEarlyReturn()
        {
            // Arrange
            DbContextOptions<MssqlDbContext> options = new DbContextOptionsBuilder<MssqlDbContext>()
                .UseInMemoryDatabase(databaseName: "MssqlSeederTest")
                .Options;

            using (MssqlDbContext context = new(options))
            {
                // Act - First call
                MssqlDbInitializer.Initialize(context);

                // Assert
                Assert.True(context.MssqlDemoItems.Any(i => i.Name == "動態種子資料 1"));
            }

            using (MssqlDbContext context = new(options))
            {
                int countBefore = context.MssqlDemoItems.Count();

                // Act - Second call
                MssqlDbInitializer.Initialize(context);

                // Assert
                Assert.Equal(countBefore, context.MssqlDemoItems.Count());
            }
        }

        [Fact]
        public void MysqlDbInitializer_SeedsItemsAndHandlesSecondCallEarlyReturn()
        {
            // Arrange
            DbContextOptions<MysqlDbContext> options = new DbContextOptionsBuilder<MysqlDbContext>()
                .UseInMemoryDatabase(databaseName: "MysqlSeederTest")
                .Options;

            using (MysqlDbContext context = new(options))
            {
                // Act - First call
                MysqlDbInitializer.Initialize(context);

                // Assert
                Assert.True(context.MysqlDemoItems.Any(i => i.Name == "動態種子資料 1"));
            }

            using (MysqlDbContext context = new(options))
            {
                int countBefore = context.MysqlDemoItems.Count();

                // Act - Second call
                MysqlDbInitializer.Initialize(context);

                // Assert
                Assert.Equal(countBefore, context.MysqlDemoItems.Count());
            }
        }

        [Fact]
        public void PostgresDbInitializer_SeedsItemsAndHandlesSecondCallEarlyReturn()
        {
            // Arrange
            DbContextOptions<PostgresDbContext> options = new DbContextOptionsBuilder<PostgresDbContext>()
                .UseInMemoryDatabase(databaseName: "PostgresSeederTest")
                .Options;

            using (PostgresDbContext context = new(options))
            {
                // Act - First call
                PostgresDbInitializer.Initialize(context);

                // Assert
                Assert.True(context.PostgresDemoItems.Any(i => i.Name == "動態種子資料 1 (PG)"));
            }

            using (PostgresDbContext context = new(options))
            {
                int countBefore = context.PostgresDemoItems.Count();

                // Act - Second call
                PostgresDbInitializer.Initialize(context);

                // Assert
                Assert.Equal(countBefore, context.PostgresDemoItems.Count());
            }
        }
    }
}
