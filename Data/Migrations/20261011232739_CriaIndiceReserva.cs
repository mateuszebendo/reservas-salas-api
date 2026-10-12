using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reservas.Data.Migrations
{
    /// <inheritdoc />
    public partial class CriaIndiceReserva : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reservas_SalaId",
                table: "Reservas");

            migrationBuilder.AddColumn<bool>(
                name: "Cancelada",
                table: "Reservas",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Reserva_IntervaloPorSala",
                table: "Reservas",
                columns: new[] { "SalaId", "Inicio", "Fim" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reserva_IntervaloPorSala",
                table: "Reservas");

            migrationBuilder.DropColumn(
                name: "Cancelada",
                table: "Reservas");

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_SalaId",
                table: "Reservas",
                column: "SalaId");
        }
    }
}
