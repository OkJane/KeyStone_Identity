using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KeyStone_Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailVerification_ActivationTokensMap : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActivationToken_Users_UserId",
                table: "ActivationToken");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ActivationToken",
                table: "ActivationToken");

            migrationBuilder.RenameTable(
                name: "ActivationToken",
                newName: "ActivationTokens");

            migrationBuilder.RenameIndex(
                name: "IX_ActivationToken_UserId",
                table: "ActivationTokens",
                newName: "IX_ActivationTokens_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ActivationTokens",
                table: "ActivationTokens",
                column: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_ActivationTokens_Users_UserId",
                table: "ActivationTokens",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActivationTokens_Users_UserId",
                table: "ActivationTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ActivationTokens",
                table: "ActivationTokens");

            migrationBuilder.RenameTable(
                name: "ActivationTokens",
                newName: "ActivationToken");

            migrationBuilder.RenameIndex(
                name: "IX_ActivationTokens_UserId",
                table: "ActivationToken",
                newName: "IX_ActivationToken_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ActivationToken",
                table: "ActivationToken",
                column: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_ActivationToken_Users_UserId",
                table: "ActivationToken",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
