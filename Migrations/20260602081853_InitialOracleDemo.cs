using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotNetMvcWeb.Migrations
{
    /// <summary>
    /// 這是一份 EF Core 自動產生的 Migration (資料庫遷移) 檔案。
    /// 當我們執行 `dotnet ef migrations add` 時，EF Core 會比對目前的 DB 結構與程式碼 (DbContext/Model)，
    /// 發現有新的變更 (例如新增資料表) 時，就會產生這份檔案。
    /// </summary>
    public partial class InitialOracleDemo : Migration
    {
        /// <summary>
        /// Up 方法定義了「升級」資料庫結構時要執行的操作。
        /// 當執行 `dotnet ef database update` 時，就會執行這裡面的邏輯。
        /// </summary>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 建立名為 OracleDemoItems 的資料表
            migrationBuilder.CreateTable(
                name: "OracleDemoItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Name = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OracleDemoItems", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OracleDemoItems");
        }
    }
}
