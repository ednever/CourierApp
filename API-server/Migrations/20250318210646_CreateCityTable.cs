using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API_server.Migrations
{
    /// <inheritdoc />
    public partial class CreateCityTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "City",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    PriceForCar = table.Column<decimal>(type: "TEXT", nullable: false),
                    PriceForScooter = table.Column<decimal>(type: "TEXT", nullable: false),
                    PriceForBicycle = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_City", x => x.ID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "City");
        }
    }
}
