using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.IO;

#nullable disable

namespace DotNetMvcWeb.Migrations
{
    [DbContext(typeof(DotNetMvcWeb.Data.AppDbContext))]
    [Migration("20260713132330_AddOracleUpdateDescriptionProcedure")]
    /// <inheritdoc />
    public partial class AddOracleUpdateDescriptionProcedure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            string sqlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Database/Procedures/Oracle/SP_UPDATE_ITEM_DESCRIPTION.sql");
            string sql;
            if (File.Exists(sqlPath)) 
            {
                sql = File.ReadAllText(sqlPath);
            }
            else 
            {
                // Fallback for execution if path is different in deployed environment
                sql = @"CREATE OR REPLACE PROCEDURE SP_UPDATE_ITEM_DESCRIPTION (
                            p_Id IN NUMBER,
                            p_NewDescription IN VARCHAR2
                        ) AS
                        BEGIN
                            UPDATE ""OracleDemoItems""
                            SET ""Description"" = p_NewDescription
                            WHERE ""Id"" = p_Id;
                        END;";
            }
            
            migrationBuilder.Sql(sql);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE SP_UPDATE_ITEM_DESCRIPTION;");
        }
    }
}
