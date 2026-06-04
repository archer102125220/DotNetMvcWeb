using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DotNetMvcWeb.Migrations.MysqlMigrations
{
    /// <inheritdoc />
    public partial class InitialMysqlDemo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MysqlDemoCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MysqlDemoCategories", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MysqlDemoItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "longtext", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MysqlDemoItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MysqlDemoItems_MysqlDemoCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "MysqlDemoCategories",
                        principalColumn: "Id");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.InsertData(
                table: "MysqlDemoCategories",
                columns: new[] { "Id", "CreatedAt", "Name" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 6, 1, 10, 0, 0, 0, DateTimeKind.Utc), "一般" },
                    { 2, new DateTime(2026, 6, 1, 10, 0, 0, 0, DateTimeKind.Utc), "重要" }
                });

            migrationBuilder.InsertData(
                table: "MysqlDemoItems",
                columns: new[] { "Id", "CategoryId", "CreatedAt", "Description", "Name" },
                values: new object[,]
                {
                    { 3, null, new DateTime(2026, 6, 2, 12, 0, 0, 0, DateTimeKind.Utc), "可以嘗試在畫面上點擊編輯或刪除這筆資料，測試 HTMX 的互動效果！", "教學用項目" },
                    { 1, 1, new DateTime(2026, 6, 1, 10, 0, 0, 0, DateTimeKind.Utc), "這是第一筆透過 EF Core Seed 建立的測試資料。", "測試示範項目 1" },
                    { 2, 2, new DateTime(2026, 6, 2, 10, 0, 0, 0, DateTimeKind.Utc), "示範如何在 MySQL 資料庫中儲存繁體中文內容。", "測試示範項目 2" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_MysqlDemoItems_CategoryId",
                table: "MysqlDemoItems",
                column: "CategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MysqlDemoItems");

            migrationBuilder.DropTable(
                name: "MysqlDemoCategories");
        }
    }
}
