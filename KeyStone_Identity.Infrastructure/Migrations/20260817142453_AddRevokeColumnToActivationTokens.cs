using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KeyStone_Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRevokeColumnToActivationTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "RevokedAt",
                table: "ActivationTokens",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RevokedAt",
                table: "ActivationTokens");
        }
    }
}
