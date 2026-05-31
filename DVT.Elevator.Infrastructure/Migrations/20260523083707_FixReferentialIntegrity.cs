using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DVT.Elevator.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixReferentialIntegrity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PassengerRequests_Floors_FloorId",
                table: "PassengerRequests");

            migrationBuilder.DropIndex(
                name: "IX_PassengerRequests_FloorId",
                table: "PassengerRequests");

            migrationBuilder.DropColumn(
                name: "FloorId",
                table: "PassengerRequests");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "PassengerRequests",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Direction",
                table: "PassengerRequests",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "PassengerRequests",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Direction",
                table: "PassengerRequests",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<int>(
                name: "FloorId",
                table: "PassengerRequests",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PassengerRequests_FloorId",
                table: "PassengerRequests",
                column: "FloorId");

            migrationBuilder.AddForeignKey(
                name: "FK_PassengerRequests_Floors_FloorId",
                table: "PassengerRequests",
                column: "FloorId",
                principalTable: "Floors",
                principalColumn: "Id");
        }
    }
}
