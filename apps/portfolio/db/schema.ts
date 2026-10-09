import type { D1Client } from '@effect/sql-d1';
import type { Statement } from 'effect/sql';

// --- [OPERATIONS] ----------------------------------------------------------------------

const schema = (sql: D1Client.D1Client): readonly Statement.Statement<unknown>[] => [
    sql`CREATE TABLE IF NOT EXISTS owner (email TEXT PRIMARY KEY, user_id TEXT NOT NULL)`,
    sql`CREATE TABLE IF NOT EXISTS portfolios (digest TEXT PRIMARY KEY, body TEXT NOT NULL)`,
    sql`CREATE TABLE IF NOT EXISTS documents (state TEXT PRIMARY KEY, digest TEXT NOT NULL REFERENCES portfolios(digest))`,
    sql`CREATE TABLE IF NOT EXISTS assets (
        digest TEXT NOT NULL, body TEXT NOT NULL,
        id TEXT GENERATED ALWAYS AS (body ->> '$.id') VIRTUAL NOT NULL UNIQUE,
        mime TEXT GENERATED ALWAYS AS (body ->> '$.mime') VIRTUAL,
        object_key TEXT, removing INTEGER NOT NULL DEFAULT 0
    )`,
    sql`CREATE TABLE IF NOT EXISTS upload_attempts (id TEXT PRIMARY KEY, asset_id TEXT NOT NULL REFERENCES assets(id) ON DELETE CASCADE, discard INTEGER NOT NULL DEFAULT 0)`,
    sql`CREATE TABLE IF NOT EXISTS upload_objects (key TEXT PRIMARY KEY, attempt_id TEXT NOT NULL REFERENCES upload_attempts(id) ON DELETE CASCADE, upload_id TEXT)`,
    sql`CREATE INDEX IF NOT EXISTS upload_attempts_asset ON upload_attempts(asset_id)`,
    sql`CREATE INDEX IF NOT EXISTS upload_objects_attempt ON upload_objects(attempt_id)`,
    sql`CREATE VIEW IF NOT EXISTS portfolio_references AS
        SELECT digest, j.value ->> '$.assetId' AS asset_id,
            CASE WHEN j.key = 'poster' THEN 'image' ELSE j.value ->> '$.kind' END AS kind,
            j.value ->> '$.page' AS page
        FROM portfolios p, json_tree(p.body) j
        WHERE CASE WHEN j.type = 'object' THEN json_type(j.value,'$.assetId') END = 'text'`,
];

// --- [EXPORTS] -------------------------------------------------------------------------

export { schema };
