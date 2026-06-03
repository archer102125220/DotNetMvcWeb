using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DotNetMvcWeb.Migrations
{
    /// <summary>
    /// 這是針對 Seed Data 產生的 Migration 檔案。
    /// 當我們在 AppDbContext 使用 `HasData` 寫入資料後，產生出的 Migration 會自動呼叫 `InsertData` 來將資料寫入。
    /// 透過 Migration 來塞入預設資料 (Seed Data) 是 EF Core 官方建議的做法，可以確保所有開發/正式環境的初始資料一致。
    /// </summary>
    public partial class SeedOracleDemoData : Migration
    {
        /// <summary>
        /// 執行資料庫更新時，EF Core 會幫我們把下面這些預設的示範資料寫入資料表中。
        /// InsertData 會根據明確指定的欄位與對應的 values 來建立資料。
        /// 如果資料庫中已經存在相同 Id 的資料，且內容有變動，EF Core 的其他 Migration 機制也會透過 UpdateData 來處理。
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

        /// <summary>
        /// Down 方法是用於復原此 Migration 的操作。
        /// 因為 Up 是插入資料，所以 Down 就是根據對應的 Id 把這些 Seed Data 從資料表中刪除 (DeleteData)。
        /// 這樣做可以確保在復原這份 Migration 後，資料庫能回到沒有這些 Seed Data 的狀態。
        /// </summary>
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
