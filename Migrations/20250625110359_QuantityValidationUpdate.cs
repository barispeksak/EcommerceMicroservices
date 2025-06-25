using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductService.Migrations
{
    /// <inheritdoc />
    public partial class QuantityValidationUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        // 1) Sayısal olmayan tüm değerleri önce “0” yap (hala TEXT iken)
        migrationBuilder.Sql(@"
            UPDATE ""ProductItems""
            SET    ""QuantityInStock"" = '0'
            WHERE  ""QuantityInStock"" IS NULL
            OR  ""QuantityInStock"" !~ '^[0-9]+$';
        ");
        migrationBuilder.Sql(@"
            ALTER TABLE ""ProductItems""
            ALTER COLUMN ""QuantityInStock""
            TYPE integer
            USING trim(""QuantityInStock"")::integer;
        ");

        }

    }
}
