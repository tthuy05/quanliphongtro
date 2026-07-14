using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TroiSinhVien.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddContractServiceQuantity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Quantity",
                table: "ContractServices",
                type: "numeric(12,2)",
                precision: 12,
                scale: 2,
                nullable: false,
                defaultValue: 1m);

            migrationBuilder.AddCheckConstraint(
                name: "CK_ContractService_Quantity",
                table: "ContractServices",
                sql: "\"Quantity\" > 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_ContractService_Quantity",
                table: "ContractServices");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "ContractServices");
        }
    }
}
