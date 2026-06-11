using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyChainCore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedDataInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Almacenes",
                columns: new[] { "Id", "Nombre", "Ubicacion" },
                values: new object[] { 1, "Almacén Central Lima", "Sede Principal" });

            migrationBuilder.InsertData(
                table: "Categorias",
                columns: new[] { "Id", "Activo", "Nombre" },
                values: new object[] { 1, true, "Tecnología" });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Descripcion", "Nombre" },
                values: new object[] { 1, "Administrador del Sistema", "Admin" });

            migrationBuilder.InsertData(
                table: "Productos",
                columns: new[] { "Id", "CategoriaId", "CodigoSku", "Descripcion", "Nombre", "PrecioUnitario", "StockMinimo" },
                values: new object[] { 1, 1, "LAP-LOQ-01", "Core i7 - 16GB RAM", "Lenovo LOQ Laptop", 3500.00m, 5 });

            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "Id", "Email", "FechaCreacion", "NombreCompleto", "PasswordHash", "RolId" },
                values: new object[] { 1, "jose@supplychain.com", new DateTime(2026, 6, 11, 0, 0, 0, 0, DateTimeKind.Utc), "Jose Luis Monteza", "hashed_password", 1 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Almacenes",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
