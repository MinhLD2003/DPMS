using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DPMS_WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class DPIADocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DPIADocument_DPIAs_DPIAId",
                table: "DPIADocument");

            migrationBuilder.DropForeignKey(
                name: "FK_DPIADocument_Users_CreatedById",
                table: "DPIADocument");

            migrationBuilder.DropForeignKey(
                name: "FK_DPIADocument_Users_LastModifiedById",
                table: "DPIADocument");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DPIADocument",
                table: "DPIADocument");

            migrationBuilder.RenameTable(
                name: "DPIADocument",
                newName: "DPIADocuments");

            migrationBuilder.RenameIndex(
                name: "IX_DPIADocument_LastModifiedById",
                table: "DPIADocuments",
                newName: "IX_DPIADocuments_LastModifiedById");

            migrationBuilder.RenameIndex(
                name: "IX_DPIADocument_DPIAId",
                table: "DPIADocuments",
                newName: "IX_DPIADocuments_DPIAId");

            migrationBuilder.RenameIndex(
                name: "IX_DPIADocument_CreatedById",
                table: "DPIADocuments",
                newName: "IX_DPIADocuments_CreatedById");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DPIADocuments",
                table: "DPIADocuments",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DPIADocuments_DPIAs_DPIAId",
                table: "DPIADocuments",
                column: "DPIAId",
                principalTable: "DPIAs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DPIADocuments_Users_CreatedById",
                table: "DPIADocuments",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DPIADocuments_Users_LastModifiedById",
                table: "DPIADocuments",
                column: "LastModifiedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DPIADocuments_DPIAs_DPIAId",
                table: "DPIADocuments");

            migrationBuilder.DropForeignKey(
                name: "FK_DPIADocuments_Users_CreatedById",
                table: "DPIADocuments");

            migrationBuilder.DropForeignKey(
                name: "FK_DPIADocuments_Users_LastModifiedById",
                table: "DPIADocuments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DPIADocuments",
                table: "DPIADocuments");

            migrationBuilder.RenameTable(
                name: "DPIADocuments",
                newName: "DPIADocument");

            migrationBuilder.RenameIndex(
                name: "IX_DPIADocuments_LastModifiedById",
                table: "DPIADocument",
                newName: "IX_DPIADocument_LastModifiedById");

            migrationBuilder.RenameIndex(
                name: "IX_DPIADocuments_DPIAId",
                table: "DPIADocument",
                newName: "IX_DPIADocument_DPIAId");

            migrationBuilder.RenameIndex(
                name: "IX_DPIADocuments_CreatedById",
                table: "DPIADocument",
                newName: "IX_DPIADocument_CreatedById");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DPIADocument",
                table: "DPIADocument",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DPIADocument_DPIAs_DPIAId",
                table: "DPIADocument",
                column: "DPIAId",
                principalTable: "DPIAs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DPIADocument_Users_CreatedById",
                table: "DPIADocument",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DPIADocument_Users_LastModifiedById",
                table: "DPIADocument",
                column: "LastModifiedById",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
