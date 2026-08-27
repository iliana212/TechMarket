using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TechMarket_Pedidos.Migrations
{
    /// <inheritdoc />
    public partial class Tablas_Pedidos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Pedidos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClienteNombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Estado = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Total = table.Column<decimal>(type: "numeric(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pedidos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ItemsPedido",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PedidoId = table.Column<int>(type: "integer", nullable: false),
                    ProductoId = table.Column<int>(type: "integer", nullable: false),
                    ProductoNombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Cantidad = table.Column<int>(type: "integer", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "numeric(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemsPedido", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemsPedido_Pedidos_PedidoId",
                        column: x => x.PedidoId,
                        principalTable: "Pedidos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ItemsPedido_PedidoId",
                table: "ItemsPedido",
                column: "PedidoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItemsPedido");

            migrationBuilder.DropTable(
                name: "Pedidos");
        }
    }
}
