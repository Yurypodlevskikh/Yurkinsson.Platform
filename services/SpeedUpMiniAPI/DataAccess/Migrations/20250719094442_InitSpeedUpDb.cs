using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class InitSpeedUpDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MetronomeCollectionNames",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetronomeCollectionNames", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserInfos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    GuidId = table.Column<string>(type: "varchar(225)", maxLength: 225, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NickName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    JwtToken = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RefreshToken = table.Column<string>(type: "varchar(1024)", maxLength: 1024, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RefreshTokenExpiry = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    HueDegrees = table.Column<int>(type: "int", nullable: false),
                    MaxSettingsCount = table.Column<int>(type: "int", nullable: false),
                    IsSupporter = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AccountTier = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastSupportDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    SupportType = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserInfos", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MetronomeSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserInfoId = table.Column<int>(type: "int", nullable: false),
                    MetronomeCollectionId = table.Column<int>(type: "int", nullable: true),
                    Title = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SourceBpm = table.Column<int>(type: "int", nullable: false),
                    TargetBpm = table.Column<int>(type: "int", nullable: false),
                    BarsPerPattern = table.Column<int>(type: "int", nullable: false),
                    TempoPeriods = table.Column<int>(type: "int", nullable: true),
                    IsReversed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsRepeatPattern = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    BeatsPerBar = table.Column<int>(type: "int", nullable: false),
                    IsTempoIncreace = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Description = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetronomeSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MetronomeSettings_MetronomeCollectionNames_MetronomeCollecti~",
                        column: x => x.MetronomeCollectionId,
                        principalTable: "MetronomeCollectionNames",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MetronomeSettings_UserInfos_UserInfoId",
                        column: x => x.UserInfoId,
                        principalTable: "UserInfos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_MetronomeSettings_MetronomeCollectionId",
                table: "MetronomeSettings",
                column: "MetronomeCollectionId");

            migrationBuilder.CreateIndex(
                name: "IX_MetronomeSettings_UserInfoId",
                table: "MetronomeSettings",
                column: "UserInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_UserInfos_GuidId",
                table: "UserInfos",
                column: "GuidId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MetronomeSettings");

            migrationBuilder.DropTable(
                name: "MetronomeCollectionNames");

            migrationBuilder.DropTable(
                name: "UserInfos");
        }
    }
}
