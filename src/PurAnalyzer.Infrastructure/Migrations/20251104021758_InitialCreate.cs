using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurAnalyzer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "documents",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ba_code = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    type = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    document_number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    operation_date = table.Column<DateTime>(type: "date", nullable: true),
                    document_day_number = table.Column<int>(type: "integer", nullable: true),
                    contractor_code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    contractor_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    external_document_number = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    external_document_date = table.Column<DateTime>(type: "date", nullable: true),
                    net_total = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    vat_total = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    gross_total = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    flag1 = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    flag2 = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    flag3 = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    comment = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_documents", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "document_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    product_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    quantity = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    unit_price_net = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    net_value = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    vat_value = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    quantity_before = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    average_before = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    quantity_after = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    average_after = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    product_group = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    document_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_document_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_document_items_documents_document_id",
                        column: x => x.document_id,
                        principalTable: "documents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_document_items_document_id",
                table: "document_items",
                column: "document_id");

            migrationBuilder.CreateIndex(
                name: "ix_documents_ba_code_type_document_number_operation_date",
                table: "documents",
                columns: new[] { "ba_code", "type", "document_number", "operation_date" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "document_items");

            migrationBuilder.DropTable(
                name: "documents");
        }
    }
}
