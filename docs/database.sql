-- 1. Tabla de Jugadores (players)
CREATE TABLE players (
    "Id" uuid NOT NULL,
    "Username" text NOT NULL,
    "UuidType" character varying(10) NOT NULL,
    "PasswordHash" text NULL,
    "SessionHash" text NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    
    -- Suscripción (ICE+)
    "requires_session_sync" boolean NOT NULL DEFAULT false,
    
    -- Wearables Internos (IDs secuenciales de base de datos)
    "equipped_hat_id" integer NULL,
    "equipped_wing_id" integer NULL,
    "equipped_cape_id" integer NULL,
    "equipped_shirt_id" integer NULL,
    "equipped_pants_id" integer NULL,
    "equipped_shoes_id" integer NULL,
    
    -- Wearables Públicos (UUIDs expuestos a la API/Launcher)
    "equipped_hat_uuid" uuid NULL,
    "equipped_wing_uuid" uuid NULL,
    "equipped_cape_uuid" uuid NULL,
    "equipped_shirt_uuid" uuid NULL,
    "equipped_pants_uuid" uuid NULL,
    "equipped_shoes_uuid" uuid NULL,
    
    CONSTRAINT "PK_players" PRIMARY KEY ("Id")
);
CREATE UNIQUE INDEX "IX_players_Username" ON players ("Username");

-- 2. Tabla de Cosméticos (cosmetic_assets)
CREATE TABLE cosmetic_assets (
    "internal_id" serial NOT NULL,
    "id" uuid NOT NULL,
    "CosmeticType" character varying(24) NOT NULL,
    "DisplayName" character varying(128) NOT NULL,
    "AssetVersion" integer NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    
    CONSTRAINT "PK_cosmetic_assets" PRIMARY KEY ("internal_id")
);
CREATE UNIQUE INDEX "IX_cosmetic_assets_id" ON cosmetic_assets ("id");

-- 3. Llaves Foráneas de Wearables en players apuntando a cosmetic_assets(internal_id)
ALTER TABLE players ADD CONSTRAINT "FK_players_cosmetic_assets_equipped_hat_id" FOREIGN KEY ("equipped_hat_id") REFERENCES cosmetic_assets ("internal_id") ON DELETE RESTRICT;
ALTER TABLE players ADD CONSTRAINT "FK_players_cosmetic_assets_equipped_wing_id" FOREIGN KEY ("equipped_wing_id") REFERENCES cosmetic_assets ("internal_id") ON DELETE RESTRICT;
ALTER TABLE players ADD CONSTRAINT "FK_players_cosmetic_assets_equipped_cape_id" FOREIGN KEY ("equipped_cape_id") REFERENCES cosmetic_assets ("internal_id") ON DELETE RESTRICT;
ALTER TABLE players ADD CONSTRAINT "FK_players_cosmetic_assets_equipped_shirt_id" FOREIGN KEY ("equipped_shirt_id") REFERENCES cosmetic_assets ("internal_id") ON DELETE RESTRICT;
ALTER TABLE players ADD CONSTRAINT "FK_players_cosmetic_assets_equipped_pants_id" FOREIGN KEY ("equipped_pants_id") REFERENCES cosmetic_assets ("internal_id") ON DELETE RESTRICT;
ALTER TABLE players ADD CONSTRAINT "FK_players_cosmetic_assets_equipped_shoes_id" FOREIGN KEY ("equipped_shoes_id") REFERENCES cosmetic_assets ("internal_id") ON DELETE RESTRICT;

-- 4. Versiones de Cosméticos (cosmetic_asset_versions)
CREATE TABLE cosmetic_asset_versions (
    "Id" uuid NOT NULL,
    "cosmetic_asset_internal_id" integer NOT NULL,
    "Architecture" character varying(16) NOT NULL,
    "Sha256Hash" character varying(64) NOT NULL,
    "SizeBytes" bigint NOT NULL,
    "MetadataJson" jsonb NOT NULL,
    "UploadedAt" timestamp with time zone NOT NULL,
    
    CONSTRAINT "PK_cosmetic_asset_versions" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_cosmetic_asset_versions_cosmetic_assets_cosmetic_asset_internal_id" FOREIGN KEY ("cosmetic_asset_internal_id") REFERENCES cosmetic_assets ("internal_id") ON DELETE CASCADE
);
CREATE UNIQUE INDEX "IX_cosmetic_asset_versions_Asset_Architecture" ON cosmetic_asset_versions ("cosmetic_asset_internal_id", "Architecture");
CREATE INDEX "IX_cosmetic_asset_versions_MetadataJson" ON cosmetic_asset_versions USING gin ("MetadataJson");
CREATE INDEX "IX_cosmetic_asset_versions_Sha256Hash" ON cosmetic_asset_versions ("Sha256Hash");

-- 5. Propiedad de Cosméticos (player_cosmetic_ownership)
CREATE TABLE player_cosmetic_ownership (
    "PlayerId" uuid NOT NULL,
    "CosmeticId" uuid NOT NULL,
    "ProviderPaymentId" text NOT NULL,
    "AcquiredAt" timestamp with time zone NOT NULL,
    
    CONSTRAINT "PK_player_cosmetic_ownership" PRIMARY KEY ("PlayerId", "CosmeticId"),
    CONSTRAINT "FK_player_cosmetic_ownership_players_PlayerId" FOREIGN KEY ("PlayerId") REFERENCES players ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_player_cosmetic_ownership_cosmetic_assets_CosmeticId" FOREIGN KEY ("CosmeticId") REFERENCES cosmetic_assets ("id") ON DELETE RESTRICT
);
CREATE INDEX "IX_player_cosmetic_ownership_CosmeticId" ON player_cosmetic_ownership ("CosmeticId");

-- 6. Historial de Autenticación Externa (player_external_auth)
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

-- 7. Tokens de Inicio Rápido (bootstrap_tokens)
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

-- 8. Eventos de Pago (payment_events)
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

-- 9. Eventos de Pago no Resueltos (unresolved_payment_events)
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

-- 10. Suscripción Premium del Jugador (player_subscriptions)
CREATE TABLE player_subscriptions (
    "Id" uuid NOT NULL,
    "PlayerId" uuid NOT NULL,
    "IsActive" boolean NOT NULL DEFAULT false,
    "ExpiresAt" timestamp with time zone NULL,
    "StripeSubscriptionId" character varying(255) NULL,
    "AutoRenew" boolean NOT NULL DEFAULT false,
    "AccumulatedMonths" integer NOT NULL DEFAULT 0,
    "UpdatedAt" timestamp with time zone NOT NULL,
    
    CONSTRAINT "PK_player_subscriptions" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_player_subscriptions_players_PlayerId" FOREIGN KEY ("PlayerId") REFERENCES players ("Id") ON DELETE CASCADE
);
CREATE UNIQUE INDEX "IX_player_subscriptions_PlayerId" ON player_subscriptions ("PlayerId");
CREATE INDEX "IX_player_subscriptions_StripeSubscriptionId" ON player_subscriptions ("StripeSubscriptionId");