using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnalysisTagger.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Teams",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ShieldImagePath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ShirtNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    Position = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    PhotoPath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CustomAttributes = table.Column<string>(type: "TEXT", nullable: false),
                    TeamId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Players_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Competition = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Season = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    MatchDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Sport = table.Column<int>(type: "INTEGER", nullable: false),
                    TaggingMode = table.Column<int>(type: "INTEGER", nullable: false),
                    HomeTeamId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AwayTeamId = table.Column<Guid>(type: "TEXT", nullable: false),
                    VideoFilePath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Projects_Teams_AwayTeamId",
                        column: x => x.AwayTeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Projects_Teams_HomeTeamId",
                        column: x => x.HomeTeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Playlists",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Playlists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Playlists_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TagTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Sport = table.Column<int>(type: "INTEGER", nullable: false),
                    IsBuiltIn = table.Column<bool>(type: "INTEGER", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TagTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TagTemplates_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Color = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    DefaultLeadTime = table.Column<long>(type: "INTEGER", nullable: false),
                    DefaultLagTime = table.Column<long>(type: "INTEGER", nullable: false),
                    SubCategories = table.Column<string>(type: "TEXT", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    TagTemplateId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Categories_TagTemplates_TagTemplateId",
                        column: x => x.TagTemplateId,
                        principalTable: "TagTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EventTags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SegmentStartTicks = table.Column<long>(type: "INTEGER", nullable: false),
                    SegmentEndTicks = table.Column<long>(type: "INTEGER", nullable: false),
                    CategoryId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SubCategory = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    CustomData = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventTags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventTags_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EventTags_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DrawingAnnotations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FrameTimecode = table.Column<long>(type: "INTEGER", nullable: false),
                    SerializedDrawingData = table.Column<string>(type: "TEXT", nullable: false),
                    ThumbnailPath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    EventTagId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrawingAnnotations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DrawingAnnotations_EventTags_EventTagId",
                        column: x => x.EventTagId,
                        principalTable: "EventTags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EventTagPlayers",
                columns: table => new
                {
                    EventTagId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TaggedPlayersId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventTagPlayers", x => new { x.EventTagId, x.TaggedPlayersId });
                    table.ForeignKey(
                        name: "FK_EventTagPlayers_EventTags_EventTagId",
                        column: x => x.EventTagId,
                        principalTable: "EventTags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventTagPlayers_Players_TaggedPlayersId",
                        column: x => x.TaggedPlayersId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlaylistEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TagId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    SegmentOverrideStartTicks = table.Column<long>(type: "INTEGER", nullable: true),
                    SegmentOverrideEndTicks = table.Column<long>(type: "INTEGER", nullable: true),
                    PlaylistId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaylistEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlaylistEntries_EventTags_TagId",
                        column: x => x.TagId,
                        principalTable: "EventTags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlaylistEntries_Playlists_PlaylistId",
                        column: x => x.PlaylistId,
                        principalTable: "Playlists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_TagTemplateId",
                table: "Categories",
                column: "TagTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_DrawingAnnotations_EventTagId",
                table: "DrawingAnnotations",
                column: "EventTagId");

            migrationBuilder.CreateIndex(
                name: "IX_EventTagPlayers_TaggedPlayersId",
                table: "EventTagPlayers",
                column: "TaggedPlayersId");

            migrationBuilder.CreateIndex(
                name: "IX_EventTags_CategoryId",
                table: "EventTags",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_EventTags_ProjectId",
                table: "EventTags",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Players_TeamId",
                table: "Players",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_PlaylistEntries_PlaylistId",
                table: "PlaylistEntries",
                column: "PlaylistId");

            migrationBuilder.CreateIndex(
                name: "IX_PlaylistEntries_TagId",
                table: "PlaylistEntries",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_Playlists_ProjectId",
                table: "Playlists",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_AwayTeamId",
                table: "Projects",
                column: "AwayTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_HomeTeamId",
                table: "Projects",
                column: "HomeTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_TagTemplates_ProjectId",
                table: "TagTemplates",
                column: "ProjectId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DrawingAnnotations");

            migrationBuilder.DropTable(
                name: "EventTagPlayers");

            migrationBuilder.DropTable(
                name: "PlaylistEntries");

            migrationBuilder.DropTable(
                name: "Players");

            migrationBuilder.DropTable(
                name: "EventTags");

            migrationBuilder.DropTable(
                name: "Playlists");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "TagTemplates");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropTable(
                name: "Teams");
        }
    }
}
