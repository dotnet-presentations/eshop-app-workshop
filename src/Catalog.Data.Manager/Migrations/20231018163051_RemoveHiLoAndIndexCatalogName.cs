using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace eShop.Catalog.Data.Manager.Migrations
{
    /// <inheritdoc />
    public partial class RemoveHiLoAndIndexCatalogName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropSequence(
                name: "catalog_brand_hilo",
                schema: "catalog");

            migrationBuilder.DropSequence(
                name: "catalog_hilo",
                schema: "catalog");

            migrationBuilder.DropSequence(
                name: "catalog_type_hilo",
                schema: "catalog");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "CatalogType",
                schema: "catalog",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "CatalogBrand",
                schema: "catalog",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Catalog",
                schema: "catalog",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.CreateIndex(
                name: "IX_Catalog_Name",
                table: "Catalog",
                schema: "catalog",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Catalog_Name",
                table: "Catalog",
                schema: "catalog");

            migrationBuilder.CreateSequence(
                name: "catalog_brand_hilo",
                schema: "catalog",
                incrementBy: 10);

            migrationBuilder.CreateSequence(
                name: "catalog_hilo",
                schema: "catalog",
                incrementBy: 10);

            migrationBuilder.CreateSequence(
                name: "catalog_type_hilo",
                schema: "catalog",
                incrementBy: 10);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "CatalogType",
                schema: "catalog",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "CatalogBrand",
                schema: "catalog",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Catalog",
                schema: "catalog",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
        }
    }
}
