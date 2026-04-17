using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ColafHotel.Migrations
{
    /// <inheritdoc />
    public partial class AddAvailabilityCalendarAndPaymentHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PaymentReference",
                table: "Reservations",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentStatus",
                table: "Reservations",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "Due on Stay");

            migrationBuilder.AddColumn<DateTime>(
                name: "PaymentUpdatedAt",
                table: "Reservations",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ReservationPaymentLogs",
                columns: table => new
                {
                    ReservationPaymentLogId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ReservationId = table.Column<int>(type: "INTEGER", nullable: false),
                    PaymentStatus = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Message = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReservationPaymentLogs", x => x.ReservationPaymentLogId);
                    table.ForeignKey(
                        name: "FK_ReservationPaymentLogs_Reservations_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "Reservations",
                        principalColumn: "ReservationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReservationPaymentLogs_ReservationId",
                table: "ReservationPaymentLogs",
                column: "ReservationId");

            migrationBuilder.Sql("""
                UPDATE Reservations
                SET PaymentStatus = CASE
                    WHEN PaymentOption = 'Pay in Advance' THEN 'Pending Payment'
                    ELSE 'Due on Stay'
                END,
                PaymentUpdatedAt = CreatedAt;
                """);

            migrationBuilder.Sql("""
                INSERT INTO ReservationPaymentLogs (ReservationId, PaymentStatus, Message, CreatedAt)
                SELECT
                    ReservationId,
                    PaymentStatus,
                    CASE
                        WHEN PaymentOption = 'Pay in Advance' THEN 'Payment history initialized. Guest selected advance payment.'
                        ELSE 'Payment history initialized. Guest will pay on stay.'
                    END,
                    COALESCE(PaymentUpdatedAt, CreatedAt)
                FROM Reservations;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReservationPaymentLogs");

            migrationBuilder.DropColumn(
                name: "PaymentReference",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "PaymentUpdatedAt",
                table: "Reservations");
        }
    }
}
