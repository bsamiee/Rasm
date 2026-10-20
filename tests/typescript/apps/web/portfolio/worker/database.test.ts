import { reset } from 'cloudflare:test';
import { env } from 'cloudflare:workers';
import { BrowserCrypto } from '@effect/platform-browser';
import { D1Client } from '@effect/sql-d1';
import { assert, beforeEach, it } from '@effect/vitest';
import { reconcile } from '@rasm/database/reconcile.ts';
import { names } from '@rasm/database/table.ts';
import { catalog, ownerUserId, readDocument, registerOwner, writeDocument } from 'architectural-portfolio/worker/database.ts';
import { Effect, Layer, Option } from 'effect';
import { SqlClient } from 'effect/sql';

// --- [COMPOSITION] ---------------------------------------------------------------------

const store = Layer.mergeAll(D1Client.layer({ db: env.DB, ...names }), BrowserCrypto.layer);

beforeEach(reset);
it.effect('rebuilds the hosted documents, assets, and owner tables and keeps their rows', () =>
    Effect.gen(function* () {
        const sql = yield* SqlClient.SqlClient;
        yield* Effect.forEach(
            ['CREATE TABLE IF NOT EXISTS documents (state TEXT PRIMARY KEY NOT NULL, body TEXT NOT NULL)', 'CREATE TABLE IF NOT EXISTS assets (id TEXT PRIMARY KEY NOT NULL, body TEXT NOT NULL)', 'CREATE TABLE IF NOT EXISTS owner (email TEXT PRIMARY KEY NOT NULL, user_id TEXT NOT NULL)'],
            (text) => sql.unsafe(text),
            { discard: true },
        );
        yield* sql`INSERT INTO documents ${sql.insert({ state: 'draft', body: '{"name":"Studio","introduction":"","email":"studio@example.com","entries":[]}' })}`;
        yield* sql`INSERT INTO assets ${sql.insert({ id: 'asset', body: '{"kind":"image","uploads":[]}' })}`;
        yield* sql`INSERT INTO owner ${sql.insert({ email: 'studio@example.com', userId: 'user' })}`;
        yield* reconcile(catalog, Option.none());
        assert.deepStrictEqual(yield* ownerUserId('studio@example.com'), Option.some('user'));
        assert.deepStrictEqual(yield* registerOwner({ email: 'studio@example.com', userId: 'other' }), 'user');
        assert.deepStrictEqual((yield* readDocument('draft')).data.portfolio.name, 'Studio');
        assert.deepStrictEqual(yield* sql`SELECT id, body, uploads, removing, ${sql('objectKey')} FROM assets`, [{ id: 'asset', body: '{"kind":"image","uploads":[]}', uploads: '[]', removing: null, objectKey: null }]);
        assert.deepStrictEqual(
            yield* sql`SELECT name, strict, wr FROM pragma_table_list WHERE schema = 'main' AND type = 'table' AND name IN ('documents', 'assets', 'owner') ORDER BY name`,
            ['assets', 'documents', 'owner'].map((name) => ({ name, strict: 1, wr: Number(name === 'owner') })),
        );
    }).pipe(Effect.provide(store)),
);
it.effect('saves and publishes a draft in one atomic batch, keeps an unchanged save at its revision, and rejects a stale revision', () =>
    Effect.gen(function* () {
        const draft = { name: 'Studio', introduction: '', email: 'studio@example.com', entries: [] };
        yield* reconcile(catalog, Option.none());
        const empty = yield* readDocument('draft');
        const saved = yield* writeDocument(draft, true, empty.revision);
        const published = yield* readDocument('published');
        assert.deepStrictEqual([empty.revision, saved, published.data.portfolio.name, published.revision], [0, 1, 'Studio', saved]);
        assert.deepStrictEqual(yield* writeDocument(draft, false, saved), saved);
        assert.deepStrictEqual(yield* writeDocument({ ...draft, name: 'Atelier' }, false, saved), saved + 1);
        assert.deepStrictEqual((yield* Effect.flip(writeDocument(draft, false, empty.revision)))._tag, 'PreconditionFailed');
    }).pipe(Effect.provide(store)),
);
