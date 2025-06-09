using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DPMS_WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class RiskAssessment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DPIAEvents_Users_CreatedById",
                table: "DPIAEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_DPIAEvents_Users_LastModifiedById",
                table: "DPIAEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_DPIAResponsibilities_Responsibilities_ResponsibilityId",
                table: "DPIAResponsibilities");

            migrationBuilder.AddColumn<int>(
                name: "PriorityAfterMitigation",
                table: "Risks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RiskImpactAfterMitigation",
                table: "Risks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RiskProbabilityAfterMitigation",
                table: "Risks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_DPIAEvents_Users_CreatedById",
                table: "DPIAEvents",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DPIAEvents_Users_LastModifiedById",
                table: "DPIAEvents",
                column: "LastModifiedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DPIAResponsibilities_Responsibilities_ResponsibilityId",
                table: "DPIAResponsibilities",
                column: "ResponsibilityId",
                principalTable: "Responsibilities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DPIAEvents_Users_CreatedById",
                table: "DPIAEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_DPIAEvents_Users_LastModifiedById",
                table: "DPIAEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_DPIAResponsibilities_Responsibilities_ResponsibilityId",
                table: "DPIAResponsibilities");

            migrationBuilder.DropColumn(
                name: "PriorityAfterMitigation",
                table: "Risks");

            migrationBuilder.DropColumn(
                name: "RiskImpactAfterMitigation",
                table: "Risks");

            migrationBuilder.DropColumn(
                name: "RiskProbabilityAfterMitigation",
                table: "Risks");

            migrationBuilder.AddForeignKey(
                name: "FK_DPIAEvents_Users_CreatedById",
                table: "DPIAEvents",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DPIAEvents_Users_LastModifiedById",
                table: "DPIAEvents",
                column: "LastModifiedById",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DPIAResponsibilities_Responsibilities_ResponsibilityId",
                table: "DPIAResponsibilities",
                column: "ResponsibilityId",
                principalTable: "Responsibilities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
