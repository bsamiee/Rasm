-- --- [DOCUMENTS] ----------------------------------------------------------------------

CREATE TABLE IF NOT EXISTS documents (state TEXT PRIMARY KEY NOT NULL, body TEXT NOT NULL);

-- --- [ASSETS] -------------------------------------------------------------------------

CREATE TABLE IF NOT EXISTS assets (id TEXT PRIMARY KEY NOT NULL, body TEXT NOT NULL);

-- --- [OWNER] --------------------------------------------------------------------------

CREATE TABLE IF NOT EXISTS owner (email TEXT PRIMARY KEY NOT NULL, user_id TEXT NOT NULL);
