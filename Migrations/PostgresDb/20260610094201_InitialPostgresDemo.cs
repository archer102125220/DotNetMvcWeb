using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DotNetMvcWeb.Migrations.PostgresDb
{
    /// <inheritdoc />
    public partial class InitialPostgresDemo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PostgresDemoCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostgresDemoCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PostgresDemoItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CategoryId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostgresDemoItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PostgresDemoItems_PostgresDemoCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "PostgresDemoCategories",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "PostgresDemoCategories",
                columns: new[] { "Id", "CreatedAt", "Name" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 6, 1, 10, 0, 0, 0, DateTimeKind.Utc), "一般 (PG)" },
                    { 2, new DateTime(2026, 6, 1, 10, 0, 0, 0, DateTimeKind.Utc), "重要 (PG)" }
                });

            migrationBuilder.InsertData(
                table: "PostgresDemoItems",
                columns: new[] { "Id", "CategoryId", "CreatedAt", "Description", "Name" },
                values: new object[,]
                {
                    { 3, null, new DateTime(2026, 6, 2, 12, 0, 0, 0, DateTimeKind.Utc), "測試 HTMX 互動效果的 Postgres 範例資料！", "教學用項目 (PG)" },
                    { 1, 1, new DateTime(2026, 6, 1, 10, 0, 0, 0, DateTimeKind.Utc), "這是第一筆透過 EF Core Seed 建立的 Postgres 測試資料。", "測試示範項目 1 (PG)" },
                    { 2, 2, new DateTime(2026, 6, 2, 10, 0, 0, 0, DateTimeKind.Utc), "示範如何在 Postgres 資料庫中儲存內容。", "測試示範項目 2 (PG)" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_PostgresDemoItems_CategoryId",
                table: "PostgresDemoItems",
                column: "CategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PostgresDemoItems");

            migrationBuilder.DropTable(
                name: "PostgresDemoCategories");
        }
    }
}
