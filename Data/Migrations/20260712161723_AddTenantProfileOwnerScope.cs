using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TroiSinhVien.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantProfileOwnerScope : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TenantProfiles_PhoneNumber",
                table: "TenantProfiles");

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerId",
                table: "TenantProfiles",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_TenantProfiles_OwnerId_PhoneNumber",
                table: "TenantProfiles",
                columns: new[] { "OwnerId", "PhoneNumber" });

            migrationBuilder.AddForeignKey(
                name: "FK_TenantProfiles_AspNetUsers_OwnerId",
                table: "TenantProfiles",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TenantProfiles_AspNetUsers_OwnerId",
                table: "TenantProfiles");

            migrationBuilder.DropIndex(
                name: "IX_TenantProfiles_OwnerId_PhoneNumber",
                table: "TenantProfiles");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "TenantProfiles");

            migrationBuilder.CreateIndex(
                name: "IX_TenantProfiles_PhoneNumber",
                table: "TenantProfiles",
                column: "PhoneNumber");
        }
    }
}
