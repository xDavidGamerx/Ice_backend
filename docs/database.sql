-- ==============================================================================
-- 1. IDENTITY SYSTEM
-- ==============================================================================

CREATE TABLE IF NOT EXISTS players (
    id SERIAL PRIMARY KEY,
    username VARCHAR(16) NOT NULL UNIQUE,
    uuid_type VARCHAR(10) NOT NULL CHECK (uuid_type IN ('PREMIUM', 'ICE')), 
    player_uuid UUID NOT NULL UNIQUE,
    password_hash TEXT, -- NULLABLE: Las cuentas exclusivas de OAuth no tienen password local
    session_hash VARCHAR(64),
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS player_external_auth (
    id SERIAL PRIMARY KEY,
    player_id INTEGER NOT NULL REFERENCES players(id) ON DELETE CASCADE,
    provider VARCHAR(20) NOT NULL CHECK (provider IN ('MICROSOFT', 'GOOGLE')),
    external_id VARCHAR(255) NOT NULL,
    email VARCHAR(255),
    linked_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(provider, external_id)
);

CREATE TABLE IF NOT EXISTS bootstrap_tokens (
    token_hash VARCHAR(64) PRIMARY KEY,
    player_id INTEGER NOT NULL REFERENCES players(id) ON DELETE CASCADE,
    expires_at TIMESTAMP NOT NULL,
    is_used BOOLEAN NOT NULL DEFAULT FALSE,
    context VARCHAR(32) NOT NULL -- Ej: 'LAUNCHER_LOGIN'
);

-- ==============================================================================
-- 2. COSMETICS SYSTEM
-- ==============================================================================

CREATE TABLE IF NOT EXISTS cosmetic_assets (
    id SERIAL PRIMARY KEY,
    cosmetic_type VARCHAR(24) NOT NULL CHECK (cosmetic_type IN ('HAT', 'WING', 'CAPE', 'SHIRT', 'PANTS', 'SHOES')),
    display_name VARCHAR(80) NOT NULL,
    asset_key VARCHAR(120) NOT NULL UNIQUE,
    model_url TEXT NOT NULL,
    texture_url TEXT NOT NULL,
    sha256 CHAR(64) NOT NULL,
    asset_version INTEGER NOT NULL DEFAULT 1,
    metadata JSONB NOT NULL DEFAULT '{}', -- Metadatos (Offsets, escalas, bone mappings de Blockbench)
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS player_cosmetics (
    player_id INTEGER PRIMARY KEY REFERENCES players(id) ON DELETE CASCADE,
    hat_id INTEGER REFERENCES cosmetic_assets(id) ON DELETE SET NULL,
    wing_id INTEGER REFERENCES cosmetic_assets(id) ON DELETE SET NULL,
    cape_id INTEGER REFERENCES cosmetic_assets(id) ON DELETE SET NULL,
    shirt_id INTEGER REFERENCES cosmetic_assets(id) ON DELETE SET NULL,
    pants_id INTEGER REFERENCES cosmetic_assets(id) ON DELETE SET NULL,
    shoes_id INTEGER REFERENCES cosmetic_assets(id) ON DELETE SET NULL,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS player_cosmetic_ownership (
    player_id INTEGER NOT NULL REFERENCES players(id) ON DELETE CASCADE,
    cosmetic_id INTEGER NOT NULL REFERENCES cosmetic_assets(id) ON DELETE RESTRICT,
    provider_payment_id TEXT NOT NULL,
    acquired_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (player_id, cosmetic_id)
);

-- ==============================================================================
-- 3. BILLING & AUDIT
-- ==============================================================================

CREATE TABLE IF NOT EXISTS payment_events (
    id SERIAL PRIMARY KEY,
    provider VARCHAR(24) NOT NULL CHECK (provider IN ('STRIPE', 'PAYPAL')),
    provider_event_id TEXT NOT NULL UNIQUE, -- Mandato de Idempotencia
    payment_intent_id TEXT NOT NULL,
    player_uuid UUID NOT NULL REFERENCES players(player_uuid) ON DELETE RESTRICT,
    status VARCHAR(32) NOT NULL,
    raw_event JSONB NOT NULL,
    processed_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- ==============================================================================
-- 4. INDICES CRÍTICOS (PERFORMANCE)
-- ==============================================================================

CREATE INDEX IF NOT EXISTS idx_players_player_uuid ON players(player_uuid);
CREATE INDEX IF NOT EXISTS idx_external_auth_lookup ON player_external_auth(external_id, provider);
CREATE INDEX IF NOT EXISTS idx_bootstrap_player ON bootstrap_tokens(player_id);
CREATE INDEX IF NOT EXISTS idx_ownership_player_id ON player_cosmetic_ownership(player_id);
CREATE INDEX IF NOT EXISTS idx_payment_events_intent ON payment_events(payment_intent_id);