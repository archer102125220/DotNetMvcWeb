using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DotNetMvcWeb.Migrations
{
    /// <summary>
    /// 這是針對 Seed Data 產生的 Migration 檔案。
    /// 當我們在 AppDbContext 使用 `HasData` 寫入資料後，產生出的 Migration 會自動呼叫 `InsertData` 來將資料寫入。
    /// </summary>
    public partial class SeedOracleDemoData : Migration
    {
        /// <summary>
        /// 執行資料庫更新時，EF Core 會幫我們把下面這些預設的示範資料寫入資料表中。
        /// </summary>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "OracleDemoItems",
                columns: new[] { "Id", "CreatedAt", "Description", "Name" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 6, 1, 10, 0, 0, 0, DateTimeKind.Utc), "這是第一筆透過 EF Core Seed 建立的測試資料。", "測試示範項目 1" },
                    { 2, new DateTime(2026, 6, 2, 10, 0, 0, 0, DateTimeKind.Utc), "示範如何在 Oracle 資料庫中儲存繁體中文內容。", "測試示範項目 2" },
                    { 3, new DateTime(2026, 6, 2, 8, 25, 25, 881, DateTimeKind.Utc).AddTicks(6280), "可以嘗試在畫面上點擊編輯或刪除這筆資料，測試 HTMX 的互動效果！", "教學用項目" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "OracleDemoItems",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "OracleDemoItems",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "OracleDemoItems",
                keyColumn: "Id",
                keyValue: 3);
        }
    }
}
