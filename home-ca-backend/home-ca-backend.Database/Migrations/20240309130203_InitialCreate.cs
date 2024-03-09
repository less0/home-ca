using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace home_ca_backend.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CertificateAuthority",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PemCertificate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EncryptedCertificate = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    PemPrivateKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CertificateAuthorityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CertificateAuthority", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CertificateAuthority_CertificateAuthority_CertificateAuthorityId",
                        column: x => x.CertificateAuthorityId,
                        principalTable: "CertificateAuthority",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Leaf",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EncryptedCertificate = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    PemCertificate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PemPrivateKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CertificateAuthorityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leaf", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Leaf_CertificateAuthority_CertificateAuthorityId",
                        column: x => x.CertificateAuthorityId,
                        principalTable: "CertificateAuthority",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CertificateAuthority_CertificateAuthorityId",
                table: "CertificateAuthority",
                column: "CertificateAuthorityId");

            migrationBuilder.CreateIndex(
                name: "IX_Leaf_CertificateAuthorityId",
                table: "Leaf",
                column: "CertificateAuthorityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Leaf");

            migrationBuilder.DropTable(
                name: "CertificateAuthority");
        }
    }
}
