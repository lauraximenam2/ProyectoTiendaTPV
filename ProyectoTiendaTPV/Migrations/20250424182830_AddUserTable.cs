using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoTiendaTPV.Migrations
{
    /// <inheritdoc />
    public partial class AddUserTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            var adminHash = "$2a$11$XvH4uMqNtL2sFqC6G3WzU.5p5j.q4s.9m8f.K3Z7z.E5i.I3q.o3y";
            var sellerHash = "$2a$11$jZ8kE6pY7tW9nL4gH1xTq.uV3i.W5x.R7f.D9k.N1o.Q3p.z7o.x6";

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Username", "PasswordHash", "Role" },
                values: new object[,]
                {
            { "admin", adminHash, 0 }, // 0 corresponde a Administrator en el enum
            { "vendedor", sellerHash, 1 }  // 1 corresponde a Seller
                });

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
