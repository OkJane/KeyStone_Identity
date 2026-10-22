using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace KeyStone_Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedPermissionAndRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RolePermissionsID",
                table: "UserRoles",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleID = table.Column<int>(type: "integer", nullable: false),
                    PermissionID = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RolePermissionsID = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Permissions_RolePermissions_RolePermissionsID",
                        column: x => x.RolePermissionsID,
                        principalTable: "RolePermissions",
                        principalColumn: "ID");
                });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "ID", "CreatedAt", "Description", "Name", "RolePermissionsID" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 10, 20, 16, 39, 42, 685, DateTimeKind.Utc).AddTicks(234), "View user records", "users.read", null },
                    { 2, new DateTime(2026, 10, 20, 16, 39, 42, 685, DateTimeKind.Utc).AddTicks(888), "Update user details or account status", "users.update", null },
                    { 3, new DateTime(2026, 10, 20, 16, 39, 42, 685, DateTimeKind.Utc).AddTicks(891), "Lock or disable a user account", "users.lock", null },
                    { 4, new DateTime(2026, 10, 20, 16, 39, 42, 685, DateTimeKind.Utc).AddTicks(892), "Assign roles to users", "users.assign_role", null },
                    { 5, new DateTime(2026, 10, 20, 16, 39, 42, 685, DateTimeKind.Utc).AddTicks(894), "View roles", "roles.read", null },
                    { 6, new DateTime(2026, 10, 20, 16, 39, 42, 685, DateTimeKind.Utc).AddTicks(895), "Create roles", "roles.create", null },
                    { 7, new DateTime(2026, 10, 20, 16, 39, 42, 685, DateTimeKind.Utc).AddTicks(896), "Update role details", "roles.update", null },
                    { 8, new DateTime(2026, 10, 20, 16, 39, 42, 685, DateTimeKind.Utc).AddTicks(898), "Delete roles", "roles.delete", null },
                    { 9, new DateTime(2026, 10, 20, 16, 39, 42, 685, DateTimeKind.Utc).AddTicks(900), "View available permissions", "permissions.read", null },
                    { 10, new DateTime(2026, 10, 20, 16, 39, 42, 685, DateTimeKind.Utc).AddTicks(901), "Grant or revoke permissions for roles", "roles.assign_permission", null },
                    { 11, new DateTime(2026, 10, 20, 16, 39, 42, 685, DateTimeKind.Utc).AddTicks(902), "View your own profile", "profile.read", null },
                    { 12, new DateTime(2026, 10, 20, 16, 39, 42, 685, DateTimeKind.Utc).AddTicks(904), "Update your own profile", "profile.update", null }
                });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "ID", "PermissionID", "RoleID" },
                values: new object[,]
                {
                    { 1, 1, 1 },
                    { 2, 2, 1 },
                    { 3, 3, 1 },
                    { 4, 4, 1 },
                    { 5, 5, 1 },
                    { 6, 6, 1 },
                    { 7, 7, 1 },
                    { 8, 8, 1 },
                    { 9, 9, 1 },
                    { 10, 10, 1 },
                    { 11, 11, 1 },
                    { 12, 12, 1 },
                    { 13, 11, 2 },
                    { 14, 12, 2 },
                    { 15, 1, 3 },
                    { 16, 3, 3 },
                    { 17, 5, 3 },
                    { 18, 9, 3 },
                    { 19, 11, 3 },
                    { 20, 12, 3 }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1L, "Admin" },
                    { 2L, "User" },
                    { 3L, "Support" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RolePermissionsID",
                table: "UserRoles",
                column: "RolePermissionsID");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_RolePermissionsID",
                table: "Permissions",
                column: "RolePermissionsID");

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoles_RolePermissions_RolePermissionsID",
                table: "UserRoles",
                column: "RolePermissionsID",
                principalTable: "RolePermissions",
                principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserRoles_RolePermissions_RolePermissionsID",
                table: "UserRoles");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropIndex(
                name: "IX_UserRoles_RolePermissionsID",
                table: "UserRoles");

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1L);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2L);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3L);

            migrationBuilder.DropColumn(
                name: "RolePermissionsID",
                table: "UserRoles");
        }
    }
}
