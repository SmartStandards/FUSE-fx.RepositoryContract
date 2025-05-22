using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechDemo.WebApi.Migrations {
  /// <inheritdoc />
  public partial class Initial : Migration {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder) {
      migrationBuilder.CreateTable(
          name: "Nations",
          columns: table => new {
            Id = table.Column<int>(type: "int", nullable: false)
                  .Annotation("SqlServer:Identity", "1, 1"),
            Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
            Code = table.Column<int>(type: "int", nullable: false)
          },
          constraints: table => {
            table.PrimaryKey("PK_Nations", x => x.Id);
          });

      migrationBuilder.CreateTable(
          name: "People",
          columns: table => new {
            Id = table.Column<int>(type: "int", nullable: false)
                  .Annotation("SqlServer:Identity", "1, 1"),
            Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
            NationId = table.Column<int>(type: "int", nullable: false)
          },
          constraints: table => {
            table.PrimaryKey("PK_People", x => x.Id);
            table.ForeignKey(
                      name: "FK_People_Nations_NationId",
                      column: x => x.NationId,
                      principalTable: "Nations",
                      principalColumn: "Id",
                      onDelete: ReferentialAction.Cascade);
          });

      migrationBuilder.CreateTable(
          name: "Addresses",
          columns: table => new {
            Id = table.Column<int>(type: "int", nullable: false)
                  .Annotation("SqlServer:Identity", "1, 1"),
            Street = table.Column<string>(type: "nvarchar(max)", nullable: false),
            City = table.Column<string>(type: "nvarchar(max)", nullable: false),
            PersonId = table.Column<int>(type: "int", nullable: false)
          },
          constraints: table => {
            table.PrimaryKey("PK_Addresses", x => x.Id);
            table.ForeignKey(
                      name: "FK_Addresses_People_PersonId",
                      column: x => x.PersonId,
                      principalTable: "People",
                      principalColumn: "Id",
                      onDelete: ReferentialAction.Cascade);
          });

      migrationBuilder.CreateIndex(
          name: "IX_Addresses_PersonId",
          table: "Addresses",
          column: "PersonId");

      migrationBuilder.CreateIndex(
          name: "IX_People_NationId",
          table: "People",
          column: "NationId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder) {
      migrationBuilder.DropTable(
          name: "Addresses");

      migrationBuilder.DropTable(
          name: "People");

      migrationBuilder.DropTable(
          name: "Nations");
    }
  }
}
