using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ColafHotel.Migrations
{
    /// <inheritdoc />
    public partial class AddReservationPaymentOption : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PaymentOption",
                table: "Reservations",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "Pay on Stay");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentOption",
                table: "Reservations");
        }
    }
}
