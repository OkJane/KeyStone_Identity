using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace KeyStone_Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditReadPermission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "ID", "CreatedAt", "Description", "Name" },
                values: new object[] { 13, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "View all audit logs", "audit.read_all" });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "ID", "PermissionID", "RoleID" },
                values: new object[,]
                {
                    { 21, 13, 1L },
                    { 22, 13, 3L }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "ID",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "ID",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "ID",
                keyValue: 13);
        }
    }
}
