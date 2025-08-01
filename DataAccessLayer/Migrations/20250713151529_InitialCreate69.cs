using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate69 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Entities",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Adress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Patent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    identifiantfiscal = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RC = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ICE = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CNSS = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Nom = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entities", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "UserSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TvaRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RetenueRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Devis = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Annefiscal = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "clients",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    identifiantFiscal = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StatusTVA = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Contact = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DelayDePayment = table.Column<int>(type: "int", nullable: false),
                    ExnLimite = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ExnUtiliser = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    entityId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clients", x => x.id);
                    table.ForeignKey(
                        name: "FK_clients_Entities_entityId",
                        column: x => x.entityId,
                        principalTable: "Entities",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "compteBancaires",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Intitule = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Banque = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Agence = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RIB = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IBAN = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SwiftCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Devise = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SoldeInitial = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DateOuverture = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EntiteId = table.Column<int>(type: "int", nullable: false),
                    EstActif = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compteBancaires", x => x.Id);
                    table.ForeignKey(
                        name: "FK_compteBancaires_Entities_EntiteId",
                        column: x => x.EntiteId,
                        principalTable: "Entities",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "fournisseurs",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    identifiantFiscal = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StatusTVA = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Contact = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Rib = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TauxDeReturn = table.Column<double>(type: "float", nullable: false),
                    delay = table.Column<int>(type: "int", nullable: false),
                    entityId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fournisseurs", x => x.id);
                    table.ForeignKey(
                        name: "FK_fournisseurs_Entities_entityId",
                        column: x => x.entityId,
                        principalTable: "Entities",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "Utilisateurs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Utilisateurs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Utilisateurs_Entities_EntityId",
                        column: x => x.EntityId,
                        principalTable: "Entities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "factureClients",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DateEmission = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    clientId = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateEcheance = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MontantTH = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TVa = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ModeDePayment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    rate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    devis = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    entiteId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    payed = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_factureClients", x => x.id);
                    table.ForeignKey(
                        name: "FK_factureClients_Entities_entiteId",
                        column: x => x.entiteId,
                        principalTable: "Entities",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_factureClients_clients_clientId",
                        column: x => x.clientId,
                        principalTable: "clients",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "factureFournisseurs",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DateReception = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Retenue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    fournisseurId = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateEcheance = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MontantTH = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TVa = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ModeDePayment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    rate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    devis = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    entiteId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    payed = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_factureFournisseurs", x => x.id);
                    table.ForeignKey(
                        name: "FK_factureFournisseurs_Entities_entiteId",
                        column: x => x.entiteId,
                        principalTable: "Entities",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_factureFournisseurs_fournisseurs_fournisseurId",
                        column: x => x.fournisseurId,
                        principalTable: "fournisseurs",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "payments",
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
                    comptebancaireId = table.Column<int>(type: "int", nullable: false),
                    entityname = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    registername = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    entityid = table.Column<int>(type: "int", nullable: false),
                    Discriminator = table.Column<string>(type: "nvarchar(21)", maxLength: 21, nullable: false),
                    fournisseurFacture = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    fournisseurname = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FactureClient = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    clientname = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    debit = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UsedById = table.Column<int>(type: "int", nullable: true),
                    datedefacture = table.Column<DateTime>(type: "datetime2", nullable: true),
                    months = table.Column<int>(type: "int", nullable: true),
                    compte = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ville = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_payments_Entities_entityid",
                        column: x => x.entityid,
                        principalTable: "Entities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_payments_Utilisateurs_RegisteredById",
                        column: x => x.RegisteredById,
                        principalTable: "Utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_payments_Utilisateurs_UsedById",
                        column: x => x.UsedById,
                        principalTable: "Utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_payments_compteBancaires_comptebancaireId",
                        column: x => x.comptebancaireId,
                        principalTable: "compteBancaires",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentDocuments",
                columns: table => new
                {
                    DocumentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaymentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentDocuments", x => x.DocumentId);
                    table.ForeignKey(
                        name: "FK_PaymentDocuments_payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_clients_entityId",
                table: "clients",
                column: "entityId");

            migrationBuilder.CreateIndex(
                name: "IX_compteBancaires_EntiteId",
                table: "compteBancaires",
                column: "EntiteId");

            migrationBuilder.CreateIndex(
                name: "IX_factureClients_clientId",
                table: "factureClients",
                column: "clientId");

            migrationBuilder.CreateIndex(
                name: "IX_factureClients_entiteId",
                table: "factureClients",
                column: "entiteId");

            migrationBuilder.CreateIndex(
                name: "IX_factureFournisseurs_entiteId",
                table: "factureFournisseurs",
                column: "entiteId");

            migrationBuilder.CreateIndex(
                name: "IX_factureFournisseurs_fournisseurId",
                table: "factureFournisseurs",
                column: "fournisseurId");

            migrationBuilder.CreateIndex(
                name: "IX_fournisseurs_entityId",
                table: "fournisseurs",
                column: "entityId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentDocuments_PaymentId",
                table: "PaymentDocuments",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_payments_comptebancaireId",
                table: "payments",
                column: "comptebancaireId");

            migrationBuilder.CreateIndex(
                name: "IX_payments_entityid",
                table: "payments",
                column: "entityid");

            migrationBuilder.CreateIndex(
                name: "IX_payments_RegisteredById",
                table: "payments",
                column: "RegisteredById");

            migrationBuilder.CreateIndex(
                name: "IX_payments_UsedById",
                table: "payments",
                column: "UsedById");

            migrationBuilder.CreateIndex(
                name: "IX_Utilisateurs_EntityId",
                table: "Utilisateurs",
                column: "EntityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "factureClients");

            migrationBuilder.DropTable(
                name: "factureFournisseurs");

            migrationBuilder.DropTable(
                name: "PaymentDocuments");

            migrationBuilder.DropTable(
                name: "UserSettings");

            migrationBuilder.DropTable(
                name: "clients");

            migrationBuilder.DropTable(
                name: "fournisseurs");

            migrationBuilder.DropTable(
                name: "payments");

            migrationBuilder.DropTable(
                name: "Utilisateurs");

            migrationBuilder.DropTable(
                name: "compteBancaires");

            migrationBuilder.DropTable(
                name: "Entities");
        }
    }
}
