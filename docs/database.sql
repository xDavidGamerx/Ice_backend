CREATE TABLE players (
    "Id" uuid NOT NULL,
    "Username" text NOT NULL,
    "UuidType" character varying(10) NOT NULL,
    "PasswordHash" text NULL,
    "SessionHash" text NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_players" PRIMARY KEY ("Id")
);
CREATE UNIQUE INDEX "IX_players_Username" ON players ("Username");

CREATE TABLE player_external_auth (
    "Id" uuid NOT NULL,
    "PlayerId" uuid NOT NULL,
    "Provider" character varying(20) NOT NULL,
    "ExternalId" text NOT NULL,
    "Email" text NULL,
    "LinkedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_player_external_auth" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_player_external_auth_players_PlayerId" FOREIGN KEY ("PlayerId") REFERENCES players ("Id") ON DELETE CASCADE
);
CREATE INDEX "IX_player_external_auth_PlayerId" ON player_external_auth ("PlayerId");
CREATE UNIQUE INDEX "IX_player_external_auth_Provider_ExternalId" ON player_external_auth ("Provider", "ExternalId");

CREATE TABLE bootstrap_tokens (
    "TokenHash" text NOT NULL,
    "PlayerId" uuid NOT NULL,
    "ExpiresAt" timestamp with time zone NOT NULL,
    "IsUsed" boolean NOT NULL,
    "Context" text NOT NULL,
    CONSTRAINT "PK_bootstrap_tokens" PRIMARY KEY ("TokenHash"),
    CONSTRAINT "FK_bootstrap_tokens_players_PlayerId" FOREIGN KEY ("PlayerId") REFERENCES players ("Id") ON DELETE CASCADE
);
CREATE INDEX "IX_bootstrap_tokens_PlayerId" ON bootstrap_tokens ("PlayerId");

CREATE TABLE cosmetic_assets (
    "Id" uuid NOT NULL,
    "CosmeticType" character varying(24) NOT NULL,
    "DisplayName" character varying(128) NOT NULL,
    "AssetVersion" integer NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_cosmetic_assets" PRIMARY KEY ("Id")
);

CREATE TABLE cosmetic_asset_versions (
    "Id" uuid NOT NULL,
    "CosmeticAssetId" uuid NOT NULL,
    "Architecture" character varying(16) NOT NULL,
    "Sha256Hash" character varying(64) NOT NULL,
    "SizeBytes" bigint NOT NULL,
    "MetadataJson" jsonb NOT NULL,
    "UploadedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_cosmetic_asset_versions" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_cosmetic_asset_versions_cosmetic_assets_CosmeticAssetId" FOREIGN KEY ("CosmeticAssetId") REFERENCES cosmetic_assets ("Id") ON DELETE CASCADE
);
CREATE UNIQUE INDEX "IX_cosmetic_asset_versions_CosmeticAssetId_Architecture" ON cosmetic_asset_versions ("CosmeticAssetId", "Architecture");
CREATE INDEX "IX_cosmetic_asset_versions_MetadataJson" ON cosmetic_asset_versions USING gin ("MetadataJson");
CREATE INDEX "IX_cosmetic_asset_versions_Sha256Hash" ON cosmetic_asset_versions ("Sha256Hash");

CREATE TABLE player_cosmetics (
    "PlayerId" uuid NOT NULL,
    "Slot" character varying(24) NOT NULL,
    "CosmeticId" uuid NULL,
    "EquippedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_player_cosmetics" PRIMARY KEY ("PlayerId", "Slot"),
    CONSTRAINT "FK_player_cosmetics_cosmetic_assets_CosmeticId" FOREIGN KEY ("CosmeticId") REFERENCES cosmetic_assets ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_player_cosmetics_players_PlayerId" FOREIGN KEY ("PlayerId") REFERENCES players ("Id") ON DELETE CASCADE
);
CREATE INDEX "IX_player_cosmetics_CosmeticId" ON player_cosmetics ("CosmeticId");

CREATE TABLE player_cosmetic_ownership (
    "PlayerId" uuid NOT NULL,
    "CosmeticId" uuid NOT NULL,
    "ProviderPaymentId" text NOT NULL,
    "AcquiredAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_player_cosmetic_ownership" PRIMARY KEY ("PlayerId", "CosmeticId"),
    CONSTRAINT "FK_player_cosmetic_ownership_cosmetic_assets_CosmeticId" FOREIGN KEY ("CosmeticId") REFERENCES cosmetic_assets ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_player_cosmetic_ownership_players_PlayerId" FOREIGN KEY ("PlayerId") REFERENCES players ("Id") ON DELETE CASCADE
);
CREATE INDEX "IX_player_cosmetic_ownership_CosmeticId" ON player_cosmetic_ownership ("CosmeticId");

CREATE TABLE payment_events (
    "Id" uuid NOT NULL,
    "Provider" character varying(24) NOT NULL,
    "ProviderEventId" text NOT NULL,
    "PaymentIntentId" text NOT NULL,
    "PlayerId" uuid NOT NULL,
    "Status" text NOT NULL,
    "RawEvent" jsonb NOT NULL,
    "ProcessedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_payment_events" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_payment_events_players_PlayerId" FOREIGN KEY ("PlayerId") REFERENCES players ("Id") ON DELETE RESTRICT
);
CREATE INDEX "IX_payment_events_PlayerId" ON payment_events ("PlayerId");
CREATE UNIQUE INDEX "IX_payment_events_ProviderEventId" ON payment_events ("ProviderEventId");

CREATE TABLE unresolved_payment_events (
    "Id" uuid NOT NULL,
    "Provider" character varying(24) NOT NULL,
    "ProviderEventId" text NOT NULL,
    "PaymentIntentId" text NOT NULL,
    "Status" text NOT NULL,
    "RawEvent" jsonb NOT NULL,
    "ProcessedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_unresolved_payment_events" PRIMARY KEY ("Id")
);
CREATE UNIQUE INDEX "IX_unresolved_payment_events_ProviderEventId" ON unresolved_payment_events ("ProviderEventId");