using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IceBackend.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCosmeticAssetVersions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_player_cosmetics_cosmetic_assets_CosmeticId",
                table: "player_cosmetics");

            migrationBuilder.DropIndex(
                name: "IX_cosmetic_assets_AssetKey",
                table: "cosmetic_assets");

            migrationBuilder.DropIndex(
                name: "IX_cosmetic_assets_Metadata",
                table: "cosmetic_assets");

            migrationBuilder.DropColumn(
                name: "AssetKey",
                table: "cosmetic_assets");

            migrationBuilder.DropColumn(
                name: "Metadata",
                table: "cosmetic_assets");

            migrationBuilder.DropColumn(
                name: "ModelUrl",
                table: "cosmetic_assets");

            migrationBuilder.DropColumn(
                name: "Sha256",
                table: "cosmetic_assets");

            migrationBuilder.DropColumn(
                name: "TextureUrl",
                table: "cosmetic_assets");

            migrationBuilder.AlterColumn<Guid>(
                name: "CosmeticId",
                table: "player_cosmetics",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "DisplayName",
                table: "cosmetic_assets",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateTable(
                name: "cosmetic_asset_versions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CosmeticAssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    Architecture = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Sha256Hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    MetadataJson = table.Column<string>(type: "jsonb", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cosmetic_asset_versions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_cosmetic_asset_versions_cosmetic_assets_CosmeticAssetId",
                        column: x => x.CosmeticAssetId,
                        principalTable: "cosmetic_assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_cosmetic_asset_versions_CosmeticAssetId_Architecture",
                table: "cosmetic_asset_versions",
                columns: new[] { "CosmeticAssetId", "Architecture" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cosmetic_asset_versions_MetadataJson",
                table: "cosmetic_asset_versions",
                column: "MetadataJson")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "IX_cosmetic_asset_versions_Sha256Hash",
                table: "cosmetic_asset_versions",
                column: "Sha256Hash");

            migrationBuilder.AddForeignKey(
                name: "FK_player_cosmetics_cosmetic_assets_CosmeticId",
                table: "player_cosmetics",
                column: "CosmeticId",
                principalTable: "cosmetic_assets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_player_cosmetics_cosmetic_assets_CosmeticId",
                table: "player_cosmetics");

            migrationBuilder.DropTable(
                name: "cosmetic_asset_versions");

            migrationBuilder.AlterColumn<Guid>(
                name: "CosmeticId",
                table: "player_cosmetics",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DisplayName",
                table: "cosmetic_assets",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128);

            migrationBuilder.AddColumn<string>(
                name: "AssetKey",
                table: "cosmetic_assets",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Metadata",
                table: "cosmetic_assets",
                type: "jsonb",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ModelUrl",
                table: "cosmetic_assets",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Sha256",
                table: "cosmetic_assets",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TextureUrl",
                table: "cosmetic_assets",
                type: "text",
                nullable: false,
                defaultValue: "");

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

            migrationBuilder.AddForeignKey(
                name: "FK_player_cosmetics_cosmetic_assets_CosmeticId",
                table: "player_cosmetics",
                column: "CosmeticId",
                principalTable: "cosmetic_assets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
