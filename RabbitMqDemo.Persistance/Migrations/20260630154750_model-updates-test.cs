using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RabbitMqDemo.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class modelupdatestest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_OrderSagaState",
                table: "OrderSagaState");

            migrationBuilder.DropColumn(
                name: "CurrentStep",
                table: "OrderSagaState");

            migrationBuilder.RenameColumn(
                name: "SagaId",
                table: "OrderSagaState",
                newName: "OrderId");

            migrationBuilder.AddColumn<Guid>(
                name: "CorrelationId",
                table: "OrderSagaState",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<decimal>(
                name: "Amount",
                table: "OrderSagaState",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "CurrentState",
                table: "OrderSagaState",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CustomerEmail",
                table: "OrderSagaState",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FailureReason",
                table: "OrderSagaState",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CancelledAt",
                table: "Orders",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CompletedAt",
                table: "Orders",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Orders",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "FailureReason",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrderSagaState",
                table: "OrderSagaState",
                column: "CorrelationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_OrderSagaState",
                table: "OrderSagaState");

            migrationBuilder.DropColumn(
                name: "CorrelationId",
                table: "OrderSagaState");

            migrationBuilder.DropColumn(
                name: "Amount",
                table: "OrderSagaState");

            migrationBuilder.DropColumn(
                name: "CurrentState",
                table: "OrderSagaState");

            migrationBuilder.DropColumn(
                name: "CustomerEmail",
                table: "OrderSagaState");

            migrationBuilder.DropColumn(
                name: "FailureReason",
                table: "OrderSagaState");

            migrationBuilder.DropColumn(
                name: "CancelledAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "FailureReason",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "OrderId",
                table: "OrderSagaState",
                newName: "SagaId");

            migrationBuilder.AddColumn<string>(
                name: "CurrentStep",
                table: "OrderSagaState",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrderSagaState",
                table: "OrderSagaState",
                column: "SagaId");
        }
    }
}
