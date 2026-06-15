using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RabbitMqDemo.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class OrderSaga : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrderSagaState",
                columns: table => new
                {
                    SagaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrentStep = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderSagaState", x => x.SagaId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderSagaState");
        }
    }
}
