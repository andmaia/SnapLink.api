using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SnapLink.api.Migrations
{
    /// <inheritdoc />
    public partial class inital : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Pages",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FinishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsPrivate = table.Column<bool>(type: "boolean", nullable: false),
                    AccessCode = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PageFiles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Data = table.Column<byte[]>(type: "bytea", nullable: true),
                    Size = table.Column<int>(type: "integer", nullable: false),
                    ContentType = table.Column<string>(type: "text", nullable: true),
                    PageId = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TimeToExpire = table.Column<int>(type: "integer", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    FinishedAT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PageFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PageFiles_Pages_PageId",
                        column: x => x.PageId,
                        principalTable: "Pages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PageFiles_PageId",
                table: "PageFiles",
                column: "PageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PageFiles");

            migrationBuilder.DropTable(
                name: "Pages");
        }
    }
}
