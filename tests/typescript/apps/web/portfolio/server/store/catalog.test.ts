import { reset } from 'cloudflare:test';
import { env } from 'cloudflare:workers';
import { D1Client } from '@effect/sql-d1';
import { assert, beforeEach, it } from '@effect/vitest';
import { store } from '@rasm/database';
import { catalog, ownerUserId, readDocument, registerOwner } from '@rasm/portfolio/server';
import { Effect, Layer, Option } from 'effect';
import { SqlClient } from 'effect/sql';

// --- [COMPOSITION] ---------------------------------------------------------------------

const hosted = Effect.gen(function* () {
    const sql = yield* SqlClient.SqlClient;
    yield* Effect.forEach(['CREATE TABLE documents (state TEXT PRIMARY KEY NOT NULL, body TEXT NOT NULL)', 'CREATE TABLE assets (id TEXT PRIMARY KEY NOT NULL, body TEXT NOT NULL)', 'CREATE TABLE owner (email TEXT PRIMARY KEY NOT NULL, user_id TEXT NOT NULL)'], (text) => sql.unsafe(text), { discard: true });
    yield* sql`INSERT INTO documents ${sql.insert({ state: 'draft', body: '{"name":"Studio","introduction":"","email":"studio@example.com","entries":[]}' })}`;
    yield* sql`INSERT INTO assets ${sql.insert({ id: 'asset', body: '{"kind":"image","uploads":[]}' })}`;
    yield* sql`INSERT INTO owner ${sql.insert({ email: 'studio@example.com', userId: 'user' })}`;
});

beforeEach(reset);
it.effect('rebuilds hosted documents, assets, and owner tables with rows intact', () =>
    Effect.gen(function* () {
        const sql = yield* SqlClient.SqlClient;
        assert.deepStrictEqual(yield* ownerUserId('studio@example.com'), Option.some('user'));
        assert.deepStrictEqual(yield* registerOwner({ email: 'studio@example.com', userId: 'other' }), 'user');
        assert.deepStrictEqual((yield* readDocument('draft')).data.portfolio.name, 'Studio');
        assert.deepStrictEqual(yield* sql`SELECT id, body, uploads, removing, ${sql('objectKey')} FROM assets`, [{ id: 'asset', body: '{"kind":"image","uploads":[]}', uploads: '[]', removing: null, objectKey: null }]);
        assert.deepStrictEqual(
            yield* sql`SELECT name, strict, wr FROM pragma_table_list WHERE schema = 'main' AND type = 'table' AND name IN ('documents', 'assets', 'owner') ORDER BY name`,
            ['assets', 'documents', 'owner'].map((name) => ({ name, strict: 1, wr: Number(name === 'owner') })),
        );
    }).pipe(Effect.provide(store(catalog, (options) => Layer.tap(D1Client.layer({ db: env.DB, ...options }), (context) => Effect.provide(hosted, context))))),
);
