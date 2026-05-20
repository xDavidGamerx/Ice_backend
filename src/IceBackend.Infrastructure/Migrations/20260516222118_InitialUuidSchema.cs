using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IceBackend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialUuidSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "cosmetic_assets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CosmeticType = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    DisplayName = table.Column<string>(type: "text", nullable: false),
                    AssetKey = table.Column<string>(type: "text", nullable: false),
                    ModelUrl = table.Column<string>(type: "text", nullable: false),
                    TextureUrl = table.Column<string>(type: "text", nullable: false),
                    Sha256 = table.Column<string>(type: "text", nullable: false),
                    AssetVersion = table.Column<int>(type: "integer", nullable: false),
                    Metadata = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cosmetic_assets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "players",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: false),
                    UuidType = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SessionHash = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_players", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "bootstrap_tokens",
                columns: table => new
                {
                    TokenHash = table.Column<string>(type: "text", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false),
                    Context = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bootstrap_tokens", x => x.TokenHash);
                    table.ForeignKey(
                        name: "FK_bootstrap_tokens_players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "payment_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Provider = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    ProviderEventId = table.Column<string>(type: "text", nullable: false),
                    PaymentIntentId = table.Column<string>(type: "text", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    RawEvent = table.Column<string>(type: "jsonb", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_payment_events_players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "player_cosmetic_ownership",
                columns: table => new
                {
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    CosmeticId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderPaymentId = table.Column<string>(type: "text", nullable: false),
                    AcquiredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_player_cosmetic_ownership", x => new { x.PlayerId, x.CosmeticId });
                    table.ForeignKey(
                        name: "FK_player_cosmetic_ownership_cosmetic_assets_CosmeticId",
                        column: x => x.CosmeticId,
                        principalTable: "cosmetic_assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_player_cosmetic_ownership_players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "player_cosmetics",
                columns: table => new
                {
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Slot = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    CosmeticId = table.Column<Guid>(type: "uuid", nullable: false),
                    EquippedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_player_cosmetics", x => new { x.PlayerId, x.Slot });
                    table.ForeignKey(
                        name: "FK_player_cosmetics_cosmetic_assets_CosmeticId",
                        column: x => x.CosmeticId,
                        principalTable: "cosmetic_assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_player_cosmetics_players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "player_external_auth",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Provider = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ExternalId = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: true),
                    LinkedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_player_external_auth", x => x.Id);
                    table.ForeignKey(
                        name: "FK_player_external_auth_players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_bootstrap_tokens_PlayerId",
                table: "bootstrap_tokens",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_cosmetic_assets_AssetKey",
                table: "cosmetic_assets",
                column: "AssetKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cosmetic_assets_Metadata",
                table: "cosmetic_assets",
                column: "Metadata")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "IX_payment_events_PlayerId",
                table: "payment_events",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_payment_events_ProviderEventId",
                table: "payment_events",
                column: "ProviderEventId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_player_cosmetic_ownership_CosmeticId",
                table: "player_cosmetic_ownership",
                column: "CosmeticId");

            migrationBuilder.CreateIndex(
                name: "IX_player_cosmetics_CosmeticId",
                table: "player_cosmetics",
                column: "CosmeticId");

            migrationBuilder.CreateIndex(
                name: "IX_player_external_auth_PlayerId",
                table: "player_external_auth",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_player_external_auth_Provider_ExternalId",
                table: "player_external_auth",
                columns: new[] { "Provider", "ExternalId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_players_Username",
                table: "players",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bootstrap_tokens");

            migrationBuilder.DropTable(
                name: "payment_events");

            migrationBuilder.DropTable(
                name: "player_cosmetic_ownership");

            migrationBuilder.DropTable(
                name: "player_cosmetics");

            migrationBuilder.DropTable(
                name: "player_external_auth");

            migrationBuilder.DropTable(
                name: "cosmetic_assets");

            migrationBuilder.DropTable(
                name: "players");
        }
    }
}
