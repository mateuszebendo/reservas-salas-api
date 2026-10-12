using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reservas.Data.Migrations
{
    /// <inheritdoc />
    public partial class AtributoAtivadaSala : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Desativada",
                table: "Salas",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Desativada",
                table: "Salas");
        }
    }
}
