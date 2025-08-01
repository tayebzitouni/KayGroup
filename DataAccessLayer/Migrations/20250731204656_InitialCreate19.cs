using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate19 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_payments_Utilisateurs_UsedById",
                table: "payments");

            migrationBuilder.DropIndex(
                name: "IX_payments_UsedById",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "PaymentUtilisatuer_devis",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "PaymentUtilisatuer_rate",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "UsedById",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "compte",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "datedefacture",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "debit",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "months",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "ville",
                table: "payments");

            migrationBuilder.AddColumn<int>(
                name: "PaymentUtilisatuerId",
                table: "PaymentDocuments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "paymentUtilisatuers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MethodeDePayment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    reference = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsAutrePayment = table.Column<bool>(type: "bit", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RegisteredById = table.Column<int>(type: "int", nullable: false),
                    entityname = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    registername = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    entityid = table.Column<int>(type: "int", nullable: false),
                    debit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UsedById = table.Column<int>(type: "int", nullable: false),
                    datedefacture = table.Column<DateTime>(type: "datetime2", nullable: false),
                    months = table.Column<int>(type: "int", nullable: false),
                    compte = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ville = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    devis = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    rate = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_paymentUtilisatuers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_paymentUtilisatuers_Entities_entityid",
                        column: x => x.entityid,
                        principalTable: "Entities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_paymentUtilisatuers_Utilisateurs_RegisteredById",
                        column: x => x.RegisteredById,
                        principalTable: "Utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_paymentUtilisatuers_Utilisateurs_UsedById",
                        column: x => x.UsedById,
                        principalTable: "Utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentDocuments_PaymentUtilisatuerId",
                table: "PaymentDocuments",
                column: "PaymentUtilisatuerId");

            migrationBuilder.CreateIndex(
                name: "IX_paymentUtilisatuers_entityid",
                table: "paymentUtilisatuers",
                column: "entityid");

            migrationBuilder.CreateIndex(
                name: "IX_paymentUtilisatuers_RegisteredById",
                table: "paymentUtilisatuers",
                column: "RegisteredById");

            migrationBuilder.CreateIndex(
                name: "IX_paymentUtilisatuers_UsedById",
                table: "paymentUtilisatuers",
                column: "UsedById");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentDocuments_paymentUtilisatuers_PaymentUtilisatuerId",
                table: "PaymentDocuments",
                column: "PaymentUtilisatuerId",
                principalTable: "paymentUtilisatuers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentDocuments_paymentUtilisatuers_PaymentUtilisatuerId",
                table: "PaymentDocuments");

            migrationBuilder.DropTable(
                name: "paymentUtilisatuers");

            migrationBuilder.DropIndex(
                name: "IX_PaymentDocuments_PaymentUtilisatuerId",
                table: "PaymentDocuments");

            migrationBuilder.DropColumn(
                name: "PaymentUtilisatuerId",
                table: "PaymentDocuments");

            migrationBuilder.AddColumn<string>(
                name: "PaymentUtilisatuer_devis",
                table: "payments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PaymentUtilisatuer_rate",
                table: "payments",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "payments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UsedById",
                table: "payments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "compte",
                table: "payments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "datedefacture",
                table: "payments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "debit",
                table: "payments",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "months",
                table: "payments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ville",
                table: "payments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_payments_UsedById",
                table: "payments",
                column: "UsedById");

            migrationBuilder.AddForeignKey(
                name: "FK_payments_Utilisateurs_UsedById",
                table: "payments",
                column: "UsedById",
                principalTable: "Utilisateurs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
