using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaymentTypeMicroservice.Migrations
{
    /// <inheritdoc />
    public partial class second : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Method",
                table: "payment_type",
                newName: "Type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Type",
                table: "payment_type",
                newName: "Method");
        }
    }
}
