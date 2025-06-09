using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DPMS_WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class ReAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConsentTokens",
                columns: table => new
                {
                    TokenString = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsValid = table.Column<bool>(type: "bit", nullable: false),
                    ExpireTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExternalSystemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsentTokens", x => x.TokenString);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Dob = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Salt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsEmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    IsPasswordConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    LastTimeLogin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModifiedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Users_Users_LastModifiedById",
                        column: x => x.LastModifiedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReferenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModifiedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comments_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Comments_Users_LastModifiedById",
                        column: x => x.LastModifiedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Comments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExternalSystems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ApiKeyHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModifiedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalSystems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExternalSystems_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExternalSystems_Users_LastModifiedById",
                        column: x => x.LastModifiedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Features",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FeatureName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    State = table.Column<int>(type: "int", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HttpMethod = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModifiedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Features", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Features_Features_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Features",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Features_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Features_Users_LastModifiedById",
                        column: x => x.LastModifiedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Forms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Version = table.Column<int>(type: "int", nullable: false),
                    FormType = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModifiedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Forms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Forms_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Forms_Users_LastModifiedById",
                        column: x => x.LastModifiedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PrivacyPolicies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PolicyCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModifiedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrivacyPolicies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrivacyPolicies_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PrivacyPolicies_Users_LastModifiedById",
                        column: x => x.LastModifiedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Purposes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModifiedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Purposes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Purposes_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Purposes_Users_LastModifiedById",
                        column: x => x.LastModifiedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Responsibilities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModifiedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Responsibilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Responsibilities_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Responsibilities_Users_LastModifiedById",
                        column: x => x.LastModifiedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DPIAs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExternalSystemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModifiedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DPIAs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DPIAs_ExternalSystems_ExternalSystemId",
                        column: x => x.ExternalSystemId,
                        principalTable: "ExternalSystems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DPIAs_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DPIAs_Users_LastModifiedById",
                        column: x => x.LastModifiedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DSARs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequesterName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequesterEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RequiredResponse = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExternalSystemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModifiedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DSARs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DSARs_ExternalSystems_ExternalSystemId",
                        column: x => x.ExternalSystemId,
                        principalTable: "ExternalSystems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DSARs_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DSARs_Users_LastModifiedById",
                        column: x => x.LastModifiedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Groups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsGlobal = table.Column<bool>(type: "bit", nullable: false),
                    SystemId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModifiedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Groups_ExternalSystems_SystemId",
                        column: x => x.SystemId,
                        principalTable: "ExternalSystems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Groups_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Groups_Users_LastModifiedById",
                        column: x => x.LastModifiedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IssueTickets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExternalSystemId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TicketType = table.Column<int>(type: "int", nullable: false),
                    IssueTicketStatus = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModifiedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssueTickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IssueTickets_ExternalSystems_ExternalSystemId",
                        column: x => x.ExternalSystemId,
                        principalTable: "ExternalSystems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_IssueTickets_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IssueTickets_Users_LastModifiedById",
                        column: x => x.LastModifiedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FormElements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FormId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataType = table.Column<int>(type: "int", nullable: true),
                    OrderIndex = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModifiedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormElements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormElements_FormElements_ParentId",
                        column: x => x.ParentId,
                        principalTable: "FormElements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FormElements_Forms_FormId",
                        column: x => x.FormId,
                        principalTable: "Forms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FormElements_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FormElements_Users_LastModifiedById",
                        column: x => x.LastModifiedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Submissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FormId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SystemId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModifiedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Submissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Submissions_ExternalSystems_SystemId",
                        column: x => x.SystemId,
                        principalTable: "ExternalSystems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Submissions_Forms_FormId",
                        column: x => x.FormId,
                        principalTable: "Forms",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Submissions_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Submissions_Users_LastModifiedById",
                        column: x => x.LastModifiedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Consents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DataSubjectId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ConsentMethod = table.Column<int>(type: "int", nullable: false),
                    ConsentIp = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    ConsentUserAgent = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    ConsentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PrivacyPolicyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExternalSystemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsWithdrawn = table.Column<bool>(type: "bit", nullable: false),
                    WithdrawnDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModifiedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Consents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Consents_ExternalSystems_ExternalSystemId",
                        column: x => x.ExternalSystemId,
                        principalTable: "ExternalSystems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Consents_PrivacyPolicies_PrivacyPolicyId",
                        column: x => x.PrivacyPolicyId,
                        principalTable: "PrivacyPolicies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Consents_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Consents_Users_LastModifiedById",
                        column: x => x.LastModifiedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExternalSystemPurposes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExternalSystemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurposeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModifiedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalSystemPurposes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExternalSystemPurposes_ExternalSystems_ExternalSystemId",
                        column: x => x.ExternalSystemId,
                        principalTable: "ExternalSystems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExternalSystemPurposes_Purposes_PurposeId",
                        column: x => x.PurposeId,
                        principalTable: "Purposes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExternalSystemPurposes_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExternalSystemPurposes_Users_LastModifiedById",
                        column: x => x.LastModifiedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DPIADocument",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DPIAId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResponsibleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModifiedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FileUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    FileFormat = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DPIADocument", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DPIADocument_DPIAs_DPIAId",
                        column: x => x.DPIAId,
                        principalTable: "DPIAs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DPIADocument_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DPIADocument_Users_LastModifiedById",
                        column: x => x.LastModifiedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DPIAEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DPIAId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Event = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EventType = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModifiedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DPIAEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DPIAEvents_DPIAs_DPIAId",
                        column: x => x.DPIAId,
                        principalTable: "DPIAs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DPIAEvents_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DPIAEvents_Users_LastModifiedById",
                        column: x => x.LastModifiedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DPIAEvents_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DPIAMembers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DPIAId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModifiedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DPIAMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DPIAMembers_DPIAs_DPIAId",
                        column: x => x.DPIAId,
                        principalTable: "DPIAs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DPIAMembers_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DPIAMembers_Users_LastModifiedById",
                        column: x => x.LastModifiedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DPIAMembers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DPIAResponsibilities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DPIAId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResponsibilityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModifiedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DPIAResponsibilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DPIAResponsibilities_DPIAs_DPIAId",
                        column: x => x.DPIAId,
                        principalTable: "DPIAs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DPIAResponsibilities_Responsibilities_ResponsibilityId",
                        column: x => x.ResponsibilityId,
                        principalTable: "Responsibilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DPIAResponsibilities_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DPIAResponsibilities_Users_LastModifiedById",
                        column: x => x.LastModifiedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "GroupFeatures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FeatureId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModifiedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupFeatures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupFeatures_Features_FeatureId",
                        column: x => x.FeatureId,
                        principalTable: "Features",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupFeatures_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupFeatures_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GroupFeatures_Users_LastModifiedById",
                        column: x => x.LastModifiedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsPic = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModifiedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserGroups_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserGroups_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserGroups_Users_LastModifiedById",
                        column: x => x.LastModifiedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserGroups_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IssueTicketDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IssueTicketId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModifiedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FileUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    FileFormat = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssueTicketDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IssueTicketDocuments_IssueTickets_IssueTicketId",
                        column: x => x.IssueTicketId,
                        principalTable: "IssueTickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IssueTicketDocuments_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IssueTicketDocuments_Users_LastModifiedById",
                        column: x => x.LastModifiedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FormResponses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubmissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FormElementId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModifiedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormResponses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormResponses_FormElements_FormElementId",
                        column: x => x.FormElementId,
                        principalTable: "FormElements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FormResponses_Submissions_SubmissionId",
                        column: x => x.SubmissionId,
                        principalTable: "Submissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FormResponses_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FormResponses_Users_LastModifiedById",
                        column: x => x.LastModifiedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ConsentPurposes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConsentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurposeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModifiedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsentPurposes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsentPurposes_Consents_ConsentId",
                        column: x => x.ConsentId,
                        principalTable: "Consents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConsentPurposes_Purposes_PurposeId",
                        column: x => x.PurposeId,
                        principalTable: "Purposes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConsentPurposes_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConsentPurposes_Users_LastModifiedById",
                        column: x => x.LastModifiedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MemberResponsibilities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DPIAResponsibilityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompletionStatus = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModifiedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberResponsibilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MemberResponsibilities_DPIAMembers_MemberId",
                        column: x => x.MemberId,
                        principalTable: "DPIAMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MemberResponsibilities_DPIAResponsibilities_DPIAResponsibilityId",
                        column: x => x.DPIAResponsibilityId,
                        principalTable: "DPIAResponsibilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MemberResponsibilities_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MemberResponsibilities_Users_LastModifiedById",
                        column: x => x.LastModifiedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Comments_CreatedById",
                table: "Comments",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_LastModifiedById",
                table: "Comments",
                column: "LastModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_UserId",
                table: "Comments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsentPurposes_ConsentId",
                table: "ConsentPurposes",
                column: "ConsentId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsentPurposes_CreatedById",
                table: "ConsentPurposes",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_ConsentPurposes_LastModifiedById",
                table: "ConsentPurposes",
                column: "LastModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_ConsentPurposes_PurposeId",
                table: "ConsentPurposes",
                column: "PurposeId");

            migrationBuilder.CreateIndex(
                name: "IX_Consents_CreatedById",
                table: "Consents",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Consents_ExternalSystemId",
                table: "Consents",
                column: "ExternalSystemId");

            migrationBuilder.CreateIndex(
                name: "IX_Consents_LastModifiedById",
                table: "Consents",
                column: "LastModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_Consents_PrivacyPolicyId",
                table: "Consents",
                column: "PrivacyPolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_DPIADocument_CreatedById",
                table: "DPIADocument",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_DPIADocument_DPIAId",
                table: "DPIADocument",
                column: "DPIAId");

            migrationBuilder.CreateIndex(
                name: "IX_DPIADocument_LastModifiedById",
                table: "DPIADocument",
                column: "LastModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_DPIAEvents_CreatedById",
                table: "DPIAEvents",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_DPIAEvents_DPIAId",
                table: "DPIAEvents",
                column: "DPIAId");

            migrationBuilder.CreateIndex(
                name: "IX_DPIAEvents_LastModifiedById",
                table: "DPIAEvents",
                column: "LastModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_DPIAEvents_UserId",
                table: "DPIAEvents",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DPIAMembers_CreatedById",
                table: "DPIAMembers",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_DPIAMembers_DPIAId_UserId",
                table: "DPIAMembers",
                columns: new[] { "DPIAId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DPIAMembers_LastModifiedById",
                table: "DPIAMembers",
                column: "LastModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_DPIAMembers_UserId",
                table: "DPIAMembers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DPIAResponsibilities_CreatedById",
                table: "DPIAResponsibilities",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_DPIAResponsibilities_DPIAId_ResponsibilityId",
                table: "DPIAResponsibilities",
                columns: new[] { "DPIAId", "ResponsibilityId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DPIAResponsibilities_LastModifiedById",
                table: "DPIAResponsibilities",
                column: "LastModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_DPIAResponsibilities_ResponsibilityId",
                table: "DPIAResponsibilities",
                column: "ResponsibilityId");

            migrationBuilder.CreateIndex(
                name: "IX_DPIAs_CreatedById",
                table: "DPIAs",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_DPIAs_ExternalSystemId",
                table: "DPIAs",
                column: "ExternalSystemId");

            migrationBuilder.CreateIndex(
                name: "IX_DPIAs_LastModifiedById",
                table: "DPIAs",
                column: "LastModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_DSARs_CreatedById",
                table: "DSARs",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_DSARs_ExternalSystemId",
                table: "DSARs",
                column: "ExternalSystemId");

            migrationBuilder.CreateIndex(
                name: "IX_DSARs_LastModifiedById",
                table: "DSARs",
                column: "LastModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalSystemPurposes_CreatedById",
                table: "ExternalSystemPurposes",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalSystemPurposes_ExternalSystemId",
                table: "ExternalSystemPurposes",
                column: "ExternalSystemId");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalSystemPurposes_LastModifiedById",
                table: "ExternalSystemPurposes",
                column: "LastModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalSystemPurposes_PurposeId",
                table: "ExternalSystemPurposes",
                column: "PurposeId");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalSystems_CreatedById",
                table: "ExternalSystems",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalSystems_LastModifiedById",
                table: "ExternalSystems",
                column: "LastModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_Features_CreatedById",
                table: "Features",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Features_LastModifiedById",
                table: "Features",
                column: "LastModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_Features_ParentId",
                table: "Features",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_FormElements_CreatedById",
                table: "FormElements",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_FormElements_FormId",
                table: "FormElements",
                column: "FormId");

            migrationBuilder.CreateIndex(
                name: "IX_FormElements_LastModifiedById",
                table: "FormElements",
                column: "LastModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_FormElements_ParentId",
                table: "FormElements",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_FormResponses_CreatedById",
                table: "FormResponses",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_FormResponses_FormElementId",
                table: "FormResponses",
                column: "FormElementId");

            migrationBuilder.CreateIndex(
                name: "IX_FormResponses_LastModifiedById",
                table: "FormResponses",
                column: "LastModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_FormResponses_SubmissionId",
                table: "FormResponses",
                column: "SubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Forms_CreatedById",
                table: "Forms",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Forms_LastModifiedById",
                table: "Forms",
                column: "LastModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_GroupFeatures_CreatedById",
                table: "GroupFeatures",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_GroupFeatures_FeatureId",
                table: "GroupFeatures",
                column: "FeatureId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupFeatures_GroupId_FeatureId",
                table: "GroupFeatures",
                columns: new[] { "GroupId", "FeatureId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GroupFeatures_LastModifiedById",
                table: "GroupFeatures",
                column: "LastModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_Groups_CreatedById",
                table: "Groups",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Groups_LastModifiedById",
                table: "Groups",
                column: "LastModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_Groups_SystemId",
                table: "Groups",
                column: "SystemId");

            migrationBuilder.CreateIndex(
                name: "IX_IssueTicketDocuments_CreatedById",
                table: "IssueTicketDocuments",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_IssueTicketDocuments_IssueTicketId",
                table: "IssueTicketDocuments",
                column: "IssueTicketId");

            migrationBuilder.CreateIndex(
                name: "IX_IssueTicketDocuments_LastModifiedById",
                table: "IssueTicketDocuments",
                column: "LastModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_IssueTickets_CreatedById",
                table: "IssueTickets",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_IssueTickets_ExternalSystemId",
                table: "IssueTickets",
                column: "ExternalSystemId");

            migrationBuilder.CreateIndex(
                name: "IX_IssueTickets_LastModifiedById",
                table: "IssueTickets",
                column: "LastModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_MemberResponsibilities_CreatedById",
                table: "MemberResponsibilities",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_MemberResponsibilities_DPIAResponsibilityId",
                table: "MemberResponsibilities",
                column: "DPIAResponsibilityId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberResponsibilities_LastModifiedById",
                table: "MemberResponsibilities",
                column: "LastModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_MemberResponsibilities_MemberId",
                table: "MemberResponsibilities",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_PrivacyPolicies_CreatedById",
                table: "PrivacyPolicies",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_PrivacyPolicies_LastModifiedById",
                table: "PrivacyPolicies",
                column: "LastModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_Purposes_CreatedById",
                table: "Purposes",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Purposes_LastModifiedById",
                table: "Purposes",
                column: "LastModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_Responsibilities_CreatedById",
                table: "Responsibilities",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Responsibilities_LastModifiedById",
                table: "Responsibilities",
                column: "LastModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_CreatedById",
                table: "Submissions",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_FormId",
                table: "Submissions",
                column: "FormId");

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_LastModifiedById",
                table: "Submissions",
                column: "LastModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_SystemId",
                table: "Submissions",
                column: "SystemId");

            migrationBuilder.CreateIndex(
                name: "IX_UserGroups_CreatedById",
                table: "UserGroups",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_UserGroups_GroupId",
                table: "UserGroups",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_UserGroups_LastModifiedById",
                table: "UserGroups",
                column: "LastModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_UserGroups_UserId_GroupId",
                table: "UserGroups",
                columns: new[] { "UserId", "GroupId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_CreatedById",
                table: "Users",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Users_LastModifiedById",
                table: "Users",
                column: "LastModifiedById");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.DropTable(
                name: "ConsentPurposes");

            migrationBuilder.DropTable(
                name: "ConsentTokens");

            migrationBuilder.DropTable(
                name: "DPIADocument");

            migrationBuilder.DropTable(
                name: "DPIAEvents");

            migrationBuilder.DropTable(
                name: "DSARs");

            migrationBuilder.DropTable(
                name: "ExternalSystemPurposes");

            migrationBuilder.DropTable(
                name: "FormResponses");

            migrationBuilder.DropTable(
                name: "GroupFeatures");

            migrationBuilder.DropTable(
                name: "IssueTicketDocuments");

            migrationBuilder.DropTable(
                name: "MemberResponsibilities");

            migrationBuilder.DropTable(
                name: "UserGroups");

            migrationBuilder.DropTable(
                name: "Consents");

            migrationBuilder.DropTable(
                name: "Purposes");

            migrationBuilder.DropTable(
                name: "FormElements");

            migrationBuilder.DropTable(
                name: "Submissions");

            migrationBuilder.DropTable(
                name: "Features");

            migrationBuilder.DropTable(
                name: "IssueTickets");

            migrationBuilder.DropTable(
                name: "DPIAMembers");

            migrationBuilder.DropTable(
                name: "DPIAResponsibilities");

            migrationBuilder.DropTable(
                name: "Groups");

            migrationBuilder.DropTable(
                name: "PrivacyPolicies");

            migrationBuilder.DropTable(
                name: "Forms");

            migrationBuilder.DropTable(
                name: "DPIAs");

            migrationBuilder.DropTable(
                name: "Responsibilities");

            migrationBuilder.DropTable(
                name: "ExternalSystems");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
