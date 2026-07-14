using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TroiSinhVien.Data.Migrations
{
    /// <inheritdoc />
    public partial class EnforceActiveLeaseInvariants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_RoomTenants_TenantProfileId",
                table: "RoomTenants",
                column: "TenantProfileId",
                unique: true,
                filter: "\"IsDeleted\" = FALSE AND \"MoveOutDate\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_RoomId",
                table: "Contracts",
                column: "RoomId",
                unique: true,
                filter: "\"IsDeleted\" = FALSE AND \"Status\" = 'Active'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RoomTenants_TenantProfileId",
                table: "RoomTenants");

            migrationBuilder.DropIndex(
                name: "IX_Contracts_RoomId",
                table: "Contracts");
        }
    }
}
