using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CourierApp.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Phenomenon",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Phenomenon", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Weather",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StationName = table.Column<string>(type: "TEXT", nullable: false),
                    WMOCode = table.Column<int>(type: "INTEGER", nullable: false),
                    AirTemperature = table.Column<decimal>(type: "TEXT", nullable: false),
                    WindSpeed = table.Column<decimal>(type: "TEXT", nullable: false),
                    PhenomenonID = table.Column<int>(type: "INTEGER", nullable: false),
                    Timestamp = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Weather", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Weather_Phenomenon_PhenomenonID",
                        column: x => x.PhenomenonID,
                        principalTable: "Phenomenon",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Weather_PhenomenonID",
                table: "Weather",
                column: "PhenomenonID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Weather");

            migrationBuilder.DropTable(
                name: "Phenomenon");
        }
    }
}
