using System;
using Microsoft.EntityFrameworkCore.Migrations;
using System.IO;

#nullable disable

namespace DotNetMvcWeb.Migrations.MssqlDb
{
    public partial class AddMssqlUpdateDescriptionProcedure : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            string sqlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Database/Procedures/Mssql/SP_UPDATE_ITEM_DESCRIPTION.sql");
            string sql;
            if (File.Exists(sqlPath)) 
            {
                sql = File.ReadAllText(sqlPath);
            }
            else 
            {
                // Fallback for execution if path is different in deployed environment
                sql = @"CREATE OR ALTER PROCEDURE SP_UPDATE_ITEM_DESCRIPTION
                            @Id INT,
                            @NewDescription NVARCHAR(MAX)
                        AS
                        BEGIN
                            SET NOCOUNT ON;

                            UPDATE [MssqlDemoItems]
                            SET [Description] = @NewDescription
                            WHERE [Id] = @Id;
                        END";
            }
            
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE SP_UPDATE_ITEM_DESCRIPTION;");
        }
    }
}
