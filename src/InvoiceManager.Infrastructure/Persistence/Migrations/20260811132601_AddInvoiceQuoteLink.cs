using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InvoiceManager.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoiceQuoteLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SourceQuoteId",
                table: "Invoices",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_SourceQuoteId",
                table: "Invoices",
                column: "SourceQuoteId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Quotes_SourceQuoteId",
                table: "Invoices",
                column: "SourceQuoteId",
                principalTable: "Quotes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Quotes_SourceQuoteId",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_SourceQuoteId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "SourceQuoteId",
                table: "Invoices");
        }
    }
}
