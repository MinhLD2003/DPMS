using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DPMS_WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class addIndexToMemberResponsibility : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MemberResponsibilities_MemberId",
                table: "MemberResponsibilities");

            migrationBuilder.CreateIndex(
                name: "IX_MemberResponsibilities_MemberId_DPIAResponsibilityId",
                table: "MemberResponsibilities",
                columns: new[] { "MemberId", "DPIAResponsibilityId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MemberResponsibilities_MemberId_DPIAResponsibilityId",
                table: "MemberResponsibilities");

            migrationBuilder.CreateIndex(
                name: "IX_MemberResponsibilities_MemberId",
                table: "MemberResponsibilities",
                column: "MemberId");
        }
    }
}
