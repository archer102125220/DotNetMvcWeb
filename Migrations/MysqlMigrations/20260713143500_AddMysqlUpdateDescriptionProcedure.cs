using System;
using Microsoft.EntityFrameworkCore.Migrations;
using System.IO;

#nullable disable

namespace DotNetMvcWeb.Migrations.MysqlMigrations
{
    public partial class AddMysqlUpdateDescriptionProcedure : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            string sqlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Database/Procedures/Mysql/SP_UPDATE_ITEM_DESCRIPTION.sql");
            string sql;
            if (File.Exists(sqlPath)) 
            {
                sql = File.ReadAllText(sqlPath);
            }
            else 
            {
                // Fallback for execution if path is different in deployed environment
                sql = @"CREATE PROCEDURE SP_UPDATE_ITEM_DESCRIPTION (
                            IN p_Id INT,
                            IN p_NewDescription TEXT
                        )
                        BEGIN
                            UPDATE `MysqlDemoItems`
                            SET `Description` = p_NewDescription
                            WHERE `Id` = p_Id;
                        END";
            }
            
            // MySQL does not support CREATE OR REPLACE PROCEDURE, so we DROP IF EXISTS first
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_UPDATE_ITEM_DESCRIPTION;");
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_UPDATE_ITEM_DESCRIPTION;");
        }
    }
}
