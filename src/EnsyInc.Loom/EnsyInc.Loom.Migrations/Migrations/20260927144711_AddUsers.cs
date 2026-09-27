using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnsyInc.Loom.Migrations.Migrations;

/// <inheritdoc />
public partial class AddUsers : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Users",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                EntraObjectId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                FirstName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                LastName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                Email = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Users", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Users_EntraObjectId",
            table: "Users",
            column: "EntraObjectId",
            unique: true,
            filter: "[DeletedAt] IS NULL");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Users");
    }
}
