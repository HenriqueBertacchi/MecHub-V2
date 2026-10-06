using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MecHub.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "usuario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nome = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Senha = table.Column<string>(type: "varchar(355)", maxLength: 355, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    tipo_login = table.Column<int>(type: "int", nullable: false),
                    id_google = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    data_criacao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Email = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    reset_senha_token = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    reset_senha_expira_em = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuario", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "mecanico",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    usuario_id = table.Column<int>(type: "int", nullable: false),
                    telefone = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mecanico", x => x.id);
                    table.ForeignKey(
                        name: "FK_mecanico_usuario_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "cliente",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    nome = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    telefone = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    cpf = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    mecanico_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cliente", x => x.id);
                    table.ForeignKey(
                        name: "FK_cliente_mecanico_mecanico_id",
                        column: x => x.mecanico_id,
                        principalTable: "mecanico",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "servico",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    descricao = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    valor = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    tipo = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    mecanico_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_servico", x => x.id);
                    table.ForeignKey(
                        name: "FK_servico_mecanico_mecanico_id",
                        column: x => x.mecanico_id,
                        principalTable: "mecanico",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "veiculo",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    marca = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    modelo = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    placa = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    cor = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ano_fabricacao = table.Column<int>(type: "int", nullable: false),
                    data_criacao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    cliente_id = table.Column<int>(type: "int", nullable: false),
                    status_veiculo = table.Column<int>(type: "int", nullable: false),
                    observacao_status = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    data_atualizacao_status = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    mecanico_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_veiculo", x => x.id);
                    table.ForeignKey(
                        name: "FK_veiculo_cliente_cliente_id",
                        column: x => x.cliente_id,
                        principalTable: "cliente",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_veiculo_mecanico_mecanico_id",
                        column: x => x.mecanico_id,
                        principalTable: "mecanico",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ordem_servico",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    mecanico_id = table.Column<int>(type: "int", nullable: false),
                    cliente_id = table.Column<int>(type: "int", nullable: false),
                    veiculo_id = table.Column<int>(type: "int", nullable: false),
                    status_ordem = table.Column<int>(type: "int", nullable: false),
                    data_criacao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    data_fechamento = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ordem_servico", x => x.id);
                    table.ForeignKey(
                        name: "FK_ordem_servico_cliente_cliente_id",
                        column: x => x.cliente_id,
                        principalTable: "cliente",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ordem_servico_mecanico_mecanico_id",
                        column: x => x.mecanico_id,
                        principalTable: "mecanico",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ordem_servico_veiculo_veiculo_id",
                        column: x => x.veiculo_id,
                        principalTable: "veiculo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "item_ordem_servico",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ordem_servico_id = table.Column<int>(type: "int", nullable: false),
                    servico_id = table.Column<int>(type: "int", nullable: false),
                    quantidade = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_item_ordem_servico", x => x.id);
                    table.ForeignKey(
                        name: "FK_item_ordem_servico_ordem_servico_ordem_servico_id",
                        column: x => x.ordem_servico_id,
                        principalTable: "ordem_servico",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_item_ordem_servico_servico_servico_id",
                        column: x => x.servico_id,
                        principalTable: "servico",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_cliente_cpf",
                table: "cliente",
                column: "cpf",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cliente_email",
                table: "cliente",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cliente_mecanico_id",
                table: "cliente",
                column: "mecanico_id");

            migrationBuilder.CreateIndex(
                name: "IX_item_ordem_servico_ordem_servico_id",
                table: "item_ordem_servico",
                column: "ordem_servico_id");

            migrationBuilder.CreateIndex(
                name: "IX_item_ordem_servico_servico_id",
                table: "item_ordem_servico",
                column: "servico_id");

            migrationBuilder.CreateIndex(
                name: "IX_mecanico_usuario_id",
                table: "mecanico",
                column: "usuario_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ordem_servico_cliente_id",
                table: "ordem_servico",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "IX_ordem_servico_mecanico_id",
                table: "ordem_servico",
                column: "mecanico_id");

            migrationBuilder.CreateIndex(
                name: "IX_ordem_servico_veiculo_id",
                table: "ordem_servico",
                column: "veiculo_id");

            migrationBuilder.CreateIndex(
                name: "IX_servico_mecanico_id",
                table: "servico",
                column: "mecanico_id");

            migrationBuilder.CreateIndex(
                name: "IX_usuario_Email",
                table: "usuario",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_veiculo_cliente_id",
                table: "veiculo",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "IX_veiculo_mecanico_id",
                table: "veiculo",
                column: "mecanico_id");

            migrationBuilder.CreateIndex(
                name: "IX_veiculo_placa",
                table: "veiculo",
                column: "placa",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "item_ordem_servico");

            migrationBuilder.DropTable(
                name: "ordem_servico");

            migrationBuilder.DropTable(
                name: "servico");

            migrationBuilder.DropTable(
                name: "veiculo");

            migrationBuilder.DropTable(
                name: "cliente");

            migrationBuilder.DropTable(
                name: "mecanico");

            migrationBuilder.DropTable(
                name: "usuario");
        }
    }
}
