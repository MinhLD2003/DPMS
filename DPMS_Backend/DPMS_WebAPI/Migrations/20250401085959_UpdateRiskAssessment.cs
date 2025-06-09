using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DPMS_WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRiskAssessment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "RaisedAt",
                table: "Risks",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "RiskOwner",
                table: "Risks",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RaisedAt",
                table: "Risks");

            migrationBuilder.DropColumn(
                name: "RiskOwner",
                table: "Risks");
        }
    }
}
