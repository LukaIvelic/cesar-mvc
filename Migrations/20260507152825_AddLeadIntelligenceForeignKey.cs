using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cesar.Migrations
{
    /// <inheritdoc />
    public partial class AddLeadIntelligenceForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_LeadIntelligences_LeadId",
                table: "LeadIntelligences",
                column: "LeadId");

            migrationBuilder.AddForeignKey(
                name: "FK_LeadIntelligences_RawLeads_LeadId",
                table: "LeadIntelligences",
                column: "LeadId",
                principalTable: "RawLeads",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LeadIntelligences_RawLeads_LeadId",
                table: "LeadIntelligences");

            migrationBuilder.DropIndex(
                name: "IX_LeadIntelligences_LeadId",
                table: "LeadIntelligences");
        }
    }
}
