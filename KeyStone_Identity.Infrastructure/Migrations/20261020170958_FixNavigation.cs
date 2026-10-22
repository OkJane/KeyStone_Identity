using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KeyStone_Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixNavigation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Permissions_RolePermissions_RolePermissionsID",
                table: "Permissions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRoles_RolePermissions_RolePermissionsID",
                table: "UserRoles");

            migrationBuilder.DropIndex(
                name: "IX_UserRoles_RolePermissionsID",
                table: "UserRoles");

            migrationBuilder.DropIndex(
                name: "IX_UserRoles_UserId",
                table: "UserRoles");

            migrationBuilder.DropIndex(
                name: "IX_Permissions_RolePermissionsID",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "RolePermissionsID",
                table: "UserRoles");

            migrationBuilder.DropColumn(
                name: "RolePermissionsID",
                table: "Permissions");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "UserRoles",
                newName: "ID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Roles",
                newName: "ID");

            migrationBuilder.AlterColumn<long>(
                name: "RoleID",
                table: "RolePermissions",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "ID",
                keyValue: 1,
                column: "RoleID",
                value: 1L);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "ID",
                keyValue: 2,
                column: "RoleID",
                value: 1L);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "ID",
                keyValue: 3,
                column: "RoleID",
                value: 1L);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "ID",
                keyValue: 4,
                column: "RoleID",
                value: 1L);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "ID",
                keyValue: 5,
                column: "RoleID",
                value: 1L);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "ID",
                keyValue: 6,
                column: "RoleID",
                value: 1L);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "ID",
                keyValue: 7,
                column: "RoleID",
                value: 1L);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "ID",
                keyValue: 8,
                column: "RoleID",
                value: 1L);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "ID",
                keyValue: 9,
                column: "RoleID",
                value: 1L);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "ID",
                keyValue: 10,
                column: "RoleID",
                value: 1L);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "ID",
                keyValue: 11,
                column: "RoleID",
                value: 1L);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "ID",
                keyValue: 12,
                column: "RoleID",
                value: 1L);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "ID",
                keyValue: 13,
                column: "RoleID",
                value: 2L);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "ID",
                keyValue: 14,
                column: "RoleID",
                value: 2L);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "ID",
                keyValue: 15,
                column: "RoleID",
                value: 3L);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "ID",
                keyValue: 16,
                column: "RoleID",
                value: 3L);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "ID",
                keyValue: 17,
                column: "RoleID",
                value: 3L);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "ID",
                keyValue: 18,
                column: "RoleID",
                value: 3L);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "ID",
                keyValue: 19,
                column: "RoleID",
                value: 3L);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "ID",
                keyValue: 20,
                column: "RoleID",
                value: 3L);

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId",
                table: "UserRoles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionID",
                table: "RolePermissions",
                column: "PermissionID");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_RoleID",
                table: "RolePermissions",
                column: "RoleID");

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermissions_Permissions_PermissionID",
                table: "RolePermissions",
                column: "PermissionID",
                principalTable: "Permissions",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermissions_Roles_RoleID",
                table: "RolePermissions",
                column: "RoleID",
                principalTable: "Roles",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RolePermissions_Permissions_PermissionID",
                table: "RolePermissions");

            migrationBuilder.DropForeignKey(
                name: "FK_RolePermissions_Roles_RoleID",
                table: "RolePermissions");

            migrationBuilder.DropIndex(
                name: "IX_UserRoles_UserId",
                table: "UserRoles");

            migrationBuilder.DropIndex(
                name: "IX_RolePermissions_PermissionID",
                table: "RolePermissions");

            migrationBuilder.DropIndex(
                name: "IX_RolePermissions_RoleID",
                table: "RolePermissions");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "UserRoles",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "Roles",
                newName: "Id");

            migrationBuilder.AddColumn<int>(
                name: "RolePermissionsID",
                table: "UserRoles",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "RoleID",
                table: "RolePermissions",
                type: "integer",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<int>(
                name: "RolePermissionsID",
                table: "Permissions",
                type: "integer",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "ID",
                keyValue: 1,
                column: "RolePermissionsID",
                value: null);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "ID",
                keyValue: 2,
                column: "RolePermissionsID",
                value: null);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "ID",
                keyValue: 3,
                column: "RolePermissionsID",
                value: null);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "ID",
                keyValue: 4,
                column: "RolePermissionsID",
                value: null);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "ID",
                keyValue: 5,
                column: "RolePermissionsID",
                value: null);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "ID",
                keyValue: 6,
                column: "RolePermissionsID",
                value: null);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "ID",
                keyValue: 7,
                column: "RolePermissionsID",
                value: null);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "ID",
                keyValue: 8,
                column: "RolePermissionsID",
                value: null);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "ID",
                keyValue: 9,
                column: "RolePermissionsID",
                value: null);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "ID",
                keyValue: 10,
                column: "RolePermissionsID",
                value: null);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "ID",
                keyValue: 11,
                column: "RolePermissionsID",
                value: null);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "ID",
                keyValue: 12,
                column: "RolePermissionsID",
                value: null);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "ID",
                keyValue: 1,
                column: "RoleID",
                value: 1);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "ID",
                keyValue: 2,
                column: "RoleID",
                value: 1);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "ID",
                keyValue: 3,
                column: "RoleID",
                value: 1);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "ID",
                keyValue: 4,
                column: "RoleID",
                value: 1);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "ID",
                keyValue: 5,
                column: "RoleID",
                value: 1);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "ID",
                keyValue: 6,
                column: "RoleID",
                value: 1);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "ID",
                keyValue: 7,
                column: "RoleID",
                value: 1);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "ID",
                keyValue: 8,
                column: "RoleID",
                value: 1);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "ID",
                keyValue: 9,
                column: "RoleID",
                value: 1);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "ID",
                keyValue: 10,
                column: "RoleID",
                value: 1);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "ID",
                keyValue: 11,
                column: "RoleID",
                value: 1);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "ID",
                keyValue: 12,
                column: "RoleID",
                value: 1);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "ID",
                keyValue: 13,
                column: "RoleID",
                value: 2);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "ID",
                keyValue: 14,
                column: "RoleID",
                value: 2);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "ID",
                keyValue: 15,
                column: "RoleID",
                value: 3);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "ID",
                keyValue: 16,
                column: "RoleID",
                value: 3);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "ID",
                keyValue: 17,
                column: "RoleID",
                value: 3);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "ID",
                keyValue: 18,
                column: "RoleID",
                value: 3);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "ID",
                keyValue: 19,
                column: "RoleID",
                value: 3);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "ID",
                keyValue: 20,
                column: "RoleID",
                value: 3);

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RolePermissionsID",
                table: "UserRoles",
                column: "RolePermissionsID");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId",
                table: "UserRoles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_RolePermissionsID",
                table: "Permissions",
                column: "RolePermissionsID");

            migrationBuilder.AddForeignKey(
                name: "FK_Permissions_RolePermissions_RolePermissionsID",
                table: "Permissions",
                column: "RolePermissionsID",
                principalTable: "RolePermissions",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoles_RolePermissions_RolePermissionsID",
                table: "UserRoles",
                column: "RolePermissionsID",
                principalTable: "RolePermissions",
                principalColumn: "ID");
        }
    }
}
