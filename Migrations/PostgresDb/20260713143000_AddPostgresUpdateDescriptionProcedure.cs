using System;
using Microsoft.EntityFrameworkCore.Migrations;
using System.IO;

#nullable disable

namespace DotNetMvcWeb.Migrations.PostgresDb
{
    public partial class AddPostgresUpdateDescriptionProcedure : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            string sqlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Database/Procedures/Postgres/SP_UPDATE_ITEM_DESCRIPTION.sql");
            string sql;
            if (File.Exists(sqlPath)) 
            {
                sql = File.ReadAllText(sqlPath);
            }
            else 
            {
                // Fallback for execution if path is different in deployed environment
                sql = @"CREATE OR REPLACE PROCEDURE SP_UPDATE_ITEM_DESCRIPTION (
                            p_Id integer,
                            p_NewDescription character varying
                        )
                        LANGUAGE plpgsql
                        AS $$
                        BEGIN
                            UPDATE ""PostgresDemoItems""
                            SET ""Description"" = p_NewDescription
                            WHERE ""Id"" = p_Id;
                        END;
                        $$;";
            }
            
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE SP_UPDATE_ITEM_DESCRIPTION;");
        }
    }
}
