using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShoeShop.Migrations
{
    /// <inheritdoc />
    public partial class ChangeOrderIdToString : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Удаляем все существующие заказы для упрощения миграции
            migrationBuilder.Sql("DELETE FROM OrderDetails");
            migrationBuilder.Sql("DELETE FROM Orders");

            // Удаляем внешний ключ
            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetails_Orders_OrderId",
                table: "OrderDetails");

            // Удаляем первичный ключ
            migrationBuilder.DropPrimaryKey(
                name: "PK_Orders",
                table: "Orders");

            // Изменяем тип столбца Id в Orders
            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "Orders",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            // Изменяем тип столбца OrderId в OrderDetails
            migrationBuilder.AlterColumn<string>(
                name: "OrderId",
                table: "OrderDetails",
                type: "nvarchar(10)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            // Добавляем первичный ключ обратно
            migrationBuilder.AddPrimaryKey(
                name: "PK_Orders",
                table: "Orders",
                column: "Id");

            // Добавляем внешний ключ обратно
            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetails_Orders_OrderId",
                table: "OrderDetails",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Удаляем все заказы
            migrationBuilder.Sql("DELETE FROM OrderDetails");
            migrationBuilder.Sql("DELETE FROM Orders");

            // Удаляем внешний ключ
            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetails_Orders_OrderId",
                table: "OrderDetails");

            // Удаляем первичный ключ
            migrationBuilder.DropPrimaryKey(
                name: "PK_Orders",
                table: "Orders");

            // Возвращаем тип столбца Id в Orders
            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Orders",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            // Возвращаем тип столбца OrderId в OrderDetails
            migrationBuilder.AlterColumn<Guid>(
                name: "OrderId",
                table: "OrderDetails",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)");

            // Добавляем первичный ключ обратно
            migrationBuilder.AddPrimaryKey(
                name: "PK_Orders",
                table: "Orders",
                column: "Id");

            // Добавляем внешний ключ обратно
            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetails_Orders_OrderId",
                table: "OrderDetails",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}