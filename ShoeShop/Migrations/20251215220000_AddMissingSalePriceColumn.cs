using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShoeShop.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingSalePriceColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Проверяем существование колонки и добавляем только если её нет
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                              WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'SalePrice')
                BEGIN
                    ALTER TABLE Products ADD SalePrice float NULL
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                          WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'SalePrice')
                BEGIN
                    ALTER TABLE Products DROP COLUMN SalePrice
                END
            ");
        }
    }
}