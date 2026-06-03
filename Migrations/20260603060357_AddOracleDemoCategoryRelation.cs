using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DotNetMvcWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddOracleDemoCategoryRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "OracleDemoItems",
                type: "NUMBER(10)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "OracleDemoCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Name = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OracleDemoCategories", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "OracleDemoCategories",
                columns: new[] { "Id", "CreatedAt", "Name" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 6, 1, 10, 0, 0, 0, DateTimeKind.Utc), "一般" },
                    { 2, new DateTime(2026, 6, 1, 10, 0, 0, 0, DateTimeKind.Utc), "重要" }
                });

            migrationBuilder.UpdateData(
                table: "OracleDemoItems",
                keyColumn: "Id",
                keyValue: 1,
                column: "CategoryId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "OracleDemoItems",
                keyColumn: "Id",
                keyValue: 2,
                column: "CategoryId",
                value: 2);

            migrationBuilder.UpdateData(
                table: "OracleDemoItems",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CategoryId", "CreatedAt" },
                values: new object[] { null, new DateTime(2026, 6, 2, 12, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.CreateIndex(
                name: "IX_OracleDemoItems_CategoryId",
                table: "OracleDemoItems",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_OracleDemoItems_OracleDemoCategories_CategoryId",
                table: "OracleDemoItems",
                column: "CategoryId",
                principalTable: "OracleDemoCategories",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OracleDemoItems_OracleDemoCategories_CategoryId",
                table: "OracleDemoItems");

            migrationBuilder.DropTable(
                name: "OracleDemoCategories");

            migrationBuilder.DropIndex(
                name: "IX_OracleDemoItems_CategoryId",
                table: "OracleDemoItems");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "OracleDemoItems");

            migrationBuilder.UpdateData(
                table: "OracleDemoItems",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 2, 8, 25, 25, 881, DateTimeKind.Utc).AddTicks(6280));
        }
    }
}
