using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IceBackend.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerWearablesColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_cosmetic_asset_versions_cosmetic_assets_CosmeticAssetId",
                table: "cosmetic_asset_versions");

            migrationBuilder.DropForeignKey(
                name: "FK_player_cosmetic_ownership_cosmetic_assets_CosmeticId",
                table: "player_cosmetic_ownership");

            migrationBuilder.DropTable(
                name: "player_cosmetics");

            migrationBuilder.DropPrimaryKey(
                name: "PK_player_cosmetic_ownership",
                table: "player_cosmetic_ownership");

            migrationBuilder.DropIndex(
                name: "IX_player_cosmetic_ownership_CosmeticId",
                table: "player_cosmetic_ownership");

            migrationBuilder.DropPrimaryKey(
                name: "PK_cosmetic_assets",
                table: "cosmetic_assets");

            migrationBuilder.DropIndex(
                name: "IX_cosmetic_asset_versions_CosmeticAssetId_Architecture",
                table: "cosmetic_asset_versions");

            migrationBuilder.DropColumn(
                name: "SessionHash",
                table: "players");

            migrationBuilder.DropColumn(
                name: "CosmeticId",
                table: "player_cosmetic_ownership");

            migrationBuilder.DropColumn(
                name: "CosmeticAssetId",
                table: "cosmetic_asset_versions");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "cosmetic_assets",
                newName: "id");

            migrationBuilder.AddColumn<int>(
                name: "equipped_cape_id",
                table: "players",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "equipped_cape_uuid",
                table: "players",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "equipped_hat_id",
                table: "players",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "equipped_hat_uuid",
                table: "players",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "equipped_pants_id",
                table: "players",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "equipped_pants_uuid",
                table: "players",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "equipped_shirt_id",
                table: "players",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "equipped_shirt_uuid",
                table: "players",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "equipped_shoes_id",
                table: "players",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "equipped_shoes_uuid",
                table: "players",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "equipped_wing_id",
                table: "players",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "equipped_wing_uuid",
                table: "players",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "requires_session_sync",
                table: "players",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "cosmetic_asset_internal_id",
                table: "player_cosmetic_ownership",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "internal_id",
                table: "cosmetic_assets",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<int>(
                name: "cosmetic_asset_internal_id",
                table: "cosmetic_asset_versions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_player_cosmetic_ownership",
                table: "player_cosmetic_ownership",
                columns: new[] { "PlayerId", "cosmetic_asset_internal_id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_cosmetic_assets",
                table: "cosmetic_assets",
                column: "internal_id");

            migrationBuilder.CreateTable(
                name: "player_subscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StripeSubscriptionId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    AutoRenew = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    AccumulatedMonths = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_player_subscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_player_subscriptions_players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "unresolved_payment_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Provider = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    ProviderEventId = table.Column<string>(type: "text", nullable: false),
                    PaymentIntentId = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    RawEvent = table.Column<string>(type: "jsonb", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_unresolved_payment_events", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_players_equipped_cape_id",
                table: "players",
                column: "equipped_cape_id");

            migrationBuilder.CreateIndex(
                name: "IX_players_equipped_hat_id",
                table: "players",
                column: "equipped_hat_id");

            migrationBuilder.CreateIndex(
                name: "IX_players_equipped_pants_id",
                table: "players",
                column: "equipped_pants_id");

            migrationBuilder.CreateIndex(
                name: "IX_players_equipped_shirt_id",
                table: "players",
                column: "equipped_shirt_id");

            migrationBuilder.CreateIndex(
                name: "IX_players_equipped_shoes_id",
                table: "players",
                column: "equipped_shoes_id");

            migrationBuilder.CreateIndex(
                name: "IX_players_equipped_wing_id",
                table: "players",
                column: "equipped_wing_id");

            migrationBuilder.CreateIndex(
                name: "IX_player_cosmetic_ownership_cosmetic_asset_internal_id",
                table: "player_cosmetic_ownership",
                column: "cosmetic_asset_internal_id");

            migrationBuilder.CreateIndex(
                name: "IX_cosmetic_assets_id",
                table: "cosmetic_assets",
                column: "id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cosmetic_asset_versions_cosmetic_asset_internal_id_Architec~",
                table: "cosmetic_asset_versions",
                columns: new[] { "cosmetic_asset_internal_id", "Architecture" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_player_subscriptions_PlayerId",
                table: "player_subscriptions",
                column: "PlayerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_unresolved_payment_events_ProviderEventId",
                table: "unresolved_payment_events",
                column: "ProviderEventId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_cosmetic_asset_versions_cosmetic_assets_cosmetic_asset_inte~",
                table: "cosmetic_asset_versions",
                column: "cosmetic_asset_internal_id",
                principalTable: "cosmetic_assets",
                principalColumn: "internal_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_player_cosmetic_ownership_cosmetic_assets_cosmetic_asset_in~",
                table: "player_cosmetic_ownership",
                column: "cosmetic_asset_internal_id",
                principalTable: "cosmetic_assets",
                principalColumn: "internal_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_players_cosmetic_assets_equipped_cape_id",
                table: "players",
                column: "equipped_cape_id",
                principalTable: "cosmetic_assets",
                principalColumn: "internal_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_players_cosmetic_assets_equipped_hat_id",
                table: "players",
                column: "equipped_hat_id",
                principalTable: "cosmetic_assets",
                principalColumn: "internal_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_players_cosmetic_assets_equipped_pants_id",
                table: "players",
                column: "equipped_pants_id",
                principalTable: "cosmetic_assets",
                principalColumn: "internal_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_players_cosmetic_assets_equipped_shirt_id",
                table: "players",
                column: "equipped_shirt_id",
                principalTable: "cosmetic_assets",
                principalColumn: "internal_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_players_cosmetic_assets_equipped_shoes_id",
                table: "players",
                column: "equipped_shoes_id",
                principalTable: "cosmetic_assets",
                principalColumn: "internal_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_players_cosmetic_assets_equipped_wing_id",
                table: "players",
                column: "equipped_wing_id",
                principalTable: "cosmetic_assets",
                principalColumn: "internal_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_cosmetic_asset_versions_cosmetic_assets_cosmetic_asset_inte~",
                table: "cosmetic_asset_versions");

            migrationBuilder.DropForeignKey(
                name: "FK_player_cosmetic_ownership_cosmetic_assets_cosmetic_asset_in~",
                table: "player_cosmetic_ownership");

            migrationBuilder.DropForeignKey(
                name: "FK_players_cosmetic_assets_equipped_cape_id",
                table: "players");

            migrationBuilder.DropForeignKey(
                name: "FK_players_cosmetic_assets_equipped_hat_id",
                table: "players");

            migrationBuilder.DropForeignKey(
                name: "FK_players_cosmetic_assets_equipped_pants_id",
                table: "players");

            migrationBuilder.DropForeignKey(
                name: "FK_players_cosmetic_assets_equipped_shirt_id",
                table: "players");

            migrationBuilder.DropForeignKey(
                name: "FK_players_cosmetic_assets_equipped_shoes_id",
                table: "players");

            migrationBuilder.DropForeignKey(
                name: "FK_players_cosmetic_assets_equipped_wing_id",
                table: "players");

            migrationBuilder.DropTable(
                name: "player_subscriptions");

            migrationBuilder.DropTable(
                name: "unresolved_payment_events");

            migrationBuilder.DropIndex(
                name: "IX_players_equipped_cape_id",
                table: "players");

            migrationBuilder.DropIndex(
                name: "IX_players_equipped_hat_id",
                table: "players");

            migrationBuilder.DropIndex(
                name: "IX_players_equipped_pants_id",
                table: "players");

            migrationBuilder.DropIndex(
                name: "IX_players_equipped_shirt_id",
                table: "players");

            migrationBuilder.DropIndex(
                name: "IX_players_equipped_shoes_id",
                table: "players");

            migrationBuilder.DropIndex(
                name: "IX_players_equipped_wing_id",
                table: "players");

            migrationBuilder.DropPrimaryKey(
                name: "PK_player_cosmetic_ownership",
                table: "player_cosmetic_ownership");

            migrationBuilder.DropIndex(
                name: "IX_player_cosmetic_ownership_cosmetic_asset_internal_id",
                table: "player_cosmetic_ownership");

            migrationBuilder.DropPrimaryKey(
                name: "PK_cosmetic_assets",
                table: "cosmetic_assets");

            migrationBuilder.DropIndex(
                name: "IX_cosmetic_assets_id",
                table: "cosmetic_assets");

            migrationBuilder.DropIndex(
                name: "IX_cosmetic_asset_versions_cosmetic_asset_internal_id_Architec~",
                table: "cosmetic_asset_versions");

            migrationBuilder.DropColumn(
                name: "equipped_cape_id",
                table: "players");

            migrationBuilder.DropColumn(
                name: "equipped_cape_uuid",
                table: "players");

            migrationBuilder.DropColumn(
                name: "equipped_hat_id",
                table: "players");

            migrationBuilder.DropColumn(
                name: "equipped_hat_uuid",
                table: "players");

            migrationBuilder.DropColumn(
                name: "equipped_pants_id",
                table: "players");

            migrationBuilder.DropColumn(
                name: "equipped_pants_uuid",
                table: "players");

            migrationBuilder.DropColumn(
                name: "equipped_shirt_id",
                table: "players");

            migrationBuilder.DropColumn(
                name: "equipped_shirt_uuid",
                table: "players");

            migrationBuilder.DropColumn(
                name: "equipped_shoes_id",
                table: "players");

            migrationBuilder.DropColumn(
                name: "equipped_shoes_uuid",
                table: "players");

            migrationBuilder.DropColumn(
                name: "equipped_wing_id",
                table: "players");

            migrationBuilder.DropColumn(
                name: "equipped_wing_uuid",
                table: "players");

            migrationBuilder.DropColumn(
                name: "requires_session_sync",
                table: "players");

            migrationBuilder.DropColumn(
                name: "cosmetic_asset_internal_id",
                table: "player_cosmetic_ownership");

            migrationBuilder.DropColumn(
                name: "internal_id",
                table: "cosmetic_assets");

            migrationBuilder.DropColumn(
                name: "cosmetic_asset_internal_id",
                table: "cosmetic_asset_versions");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "cosmetic_assets",
                newName: "Id");

            migrationBuilder.AddColumn<string>(
                name: "SessionHash",
                table: "players",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CosmeticId",
                table: "player_cosmetic_ownership",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CosmeticAssetId",
                table: "cosmetic_asset_versions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_player_cosmetic_ownership",
                table: "player_cosmetic_ownership",
                columns: new[] { "PlayerId", "CosmeticId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_cosmetic_assets",
                table: "cosmetic_assets",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "player_cosmetics",
                columns: table => new
                {
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Slot = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    CosmeticId = table.Column<Guid>(type: "uuid", nullable: true),
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

            migrationBuilder.CreateIndex(
                name: "IX_player_cosmetic_ownership_CosmeticId",
                table: "player_cosmetic_ownership",
                column: "CosmeticId");

            migrationBuilder.CreateIndex(
                name: "IX_cosmetic_asset_versions_CosmeticAssetId_Architecture",
                table: "cosmetic_asset_versions",
                columns: new[] { "CosmeticAssetId", "Architecture" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_player_cosmetics_CosmeticId",
                table: "player_cosmetics",
                column: "CosmeticId");

            migrationBuilder.AddForeignKey(
                name: "FK_cosmetic_asset_versions_cosmetic_assets_CosmeticAssetId",
                table: "cosmetic_asset_versions",
                column: "CosmeticAssetId",
                principalTable: "cosmetic_assets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_player_cosmetic_ownership_cosmetic_assets_CosmeticId",
                table: "player_cosmetic_ownership",
                column: "CosmeticId",
                principalTable: "cosmetic_assets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
