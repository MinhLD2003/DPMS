using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DPMS_WebAPI.Migrations.DPMSLogging
{
    /// <inheritdoc />
    public partial class LogResponses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Url",
                table: "HttpLogs",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "HttpType",
                table: "HttpLogs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ResponseStatus",
                table: "HttpLogs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TraceId",
                table: "HttpLogs",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HttpType",
                table: "HttpLogs");

            migrationBuilder.DropColumn(
                name: "ResponseStatus",
                table: "HttpLogs");

            migrationBuilder.DropColumn(
                name: "TraceId",
                table: "HttpLogs");

            migrationBuilder.AlterColumn<string>(
                name: "Url",
                table: "HttpLogs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
