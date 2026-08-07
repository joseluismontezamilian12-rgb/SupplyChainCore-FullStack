using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyChainCore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AutenticacionRolesYSemillaSegura : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Usuarios",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "Descripcion",
                value: "Administrador del Sistema: puede registrar movimientos en el ledger");

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Descripcion", "Nombre" },
                values: new object[] { 2, "Operador: acceso de solo lectura a stock, historial y KPIs", "Operador" });

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "100000.P3ocnlstSKbA4fS3jSNZrg==.tFxScx1pDVYzd6/qJok/Te3DERUxXxr5c4EGiHOby0U=");

            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "Id", "Email", "FechaCreacion", "NombreCompleto", "PasswordHash", "RolId" },
                values: new object[] { 2, "operador@supplychain.com", new DateTime(2026, 6, 11, 0, 0, 0, 0, DateTimeKind.Utc), "Operador de Almacén", "100000.obLD1OX2BxgpOktcbX6PkA==.GL3wW8VDG1K7qiEzAaCmF0xTdJ60eJshIbcUEvygKG0=", 2 });

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios");

            migrationBuilder.DeleteData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Usuarios",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "Descripcion",
                value: "Administrador del Sistema");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "hashed_password");
        }
    }
}
