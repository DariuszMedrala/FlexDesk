using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlexDesk.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkspaceLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "building_number",
                table: "workspaces",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "city",
                table: "workspaces",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "floor",
                table: "workspaces",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "street",
                table: "workspaces",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "workspaces",
                keyColumn: "id",
                keyValue: new Guid("a1a1a1a1-0000-0000-0000-000000000001"),
                columns: new[] { "building_number", "city", "floor", "street" },
                values: new object[] { "1", "Kraków", 1, "Rynek Główny" });

            migrationBuilder.UpdateData(
                table: "workspaces",
                keyColumn: "id",
                keyValue: new Guid("a1a1a1a1-0000-0000-0000-000000000002"),
                columns: new[] { "building_number", "city", "floor", "street" },
                values: new object[] { "1", "Kraków", 1, "Rynek Główny" });

            migrationBuilder.UpdateData(
                table: "workspaces",
                keyColumn: "id",
                keyValue: new Guid("b2b2b2b2-0000-0000-0000-000000000001"),
                columns: new[] { "building_number", "city", "floor", "street" },
                values: new object[] { "10", "Kraków", 2, "Długa" });

            migrationBuilder.UpdateData(
                table: "workspaces",
                keyColumn: "id",
                keyValue: new Guid("b2b2b2b2-0000-0000-0000-000000000002"),
                columns: new[] { "building_number", "city", "floor", "street" },
                values: new object[] { "100", "Warszawa", 3, "Marszałkowska" });

            migrationBuilder.UpdateData(
                table: "workspaces",
                keyColumn: "id",
                keyValue: new Guid("c3c3c3c3-0000-0000-0000-000000000001"),
                columns: new[] { "building_number", "city", "floor", "street" },
                values: new object[] { "5", "Kraków", 2, "Grodzka" });

            migrationBuilder.UpdateData(
                table: "workspaces",
                keyColumn: "id",
                keyValue: new Guid("c3c3c3c3-0000-0000-0000-000000000002"),
                columns: new[] { "building_number", "city", "floor", "street" },
                values: new object[] { "5", "Kraków", 3, "Grodzka" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "building_number",
                table: "workspaces");

            migrationBuilder.DropColumn(
                name: "city",
                table: "workspaces");

            migrationBuilder.DropColumn(
                name: "floor",
                table: "workspaces");

            migrationBuilder.DropColumn(
                name: "street",
                table: "workspaces");
        }
    }
}
