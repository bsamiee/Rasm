import { reset } from 'cloudflare:test';
import { env } from 'cloudflare:workers';
import { D1Client } from '@effect/sql-d1';
import { assert, beforeEach, it } from '@effect/vitest';
import { store } from '@rasm/database/store.ts';
import type { names } from '@rasm/database/table.ts';
import { catalog, ownerUserId, readAsset, readDocument, registerOwner, writeDocument } from 'architectural-portfolio/worker/database.ts';
import { Effect, Layer, Option } from 'effect';
import { SqlClient } from 'effect/sql';

// --- [COMPOSITION] ---------------------------------------------------------------------

const client = (options: typeof names) => D1Client.layer({ db: env.DB, ...options });
const hosted = Effect.gen(function* () {
    const sql = yield* SqlClient.SqlClient;
    yield* Effect.forEach(
        ['CREATE TABLE IF NOT EXISTS documents (state TEXT PRIMARY KEY NOT NULL, body TEXT NOT NULL)', 'CREATE TABLE IF NOT EXISTS assets (id TEXT PRIMARY KEY NOT NULL, body TEXT NOT NULL)', 'CREATE TABLE IF NOT EXISTS owner (email TEXT PRIMARY KEY NOT NULL, user_id TEXT NOT NULL)'],
        (text) => sql.unsafe(text),
        { discard: true },
    );
    yield* sql`INSERT INTO documents ${sql.insert({ state: 'draft', body: '{"name":"Studio","introduction":"","email":"studio@example.com","entries":[]}' })}`;
    yield* sql`INSERT INTO assets ${sql.insert({ id: 'asset', body: '{"kind":"image","uploads":[]}' })}`;
    yield* sql`INSERT INTO owner ${sql.insert({ email: 'studio@example.com', userId: 'user' })}`;
});

beforeEach(reset);
it.effect('rebuilds the hosted documents, assets, and owner tables and keeps their rows', () =>
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
    }).pipe(Effect.provide(store(catalog, (options) => Layer.tap(client(options), (context) => Effect.provide(hosted, context))))),
);
it.effect('saves and publishes a draft in one atomic batch, keeps an unchanged save at its revision, and rejects a stale revision', () =>
    Effect.gen(function* () {
        const draft = { name: 'Studio', introduction: '', email: 'studio@example.com', entries: [] };
        const empty = yield* readDocument('draft');
        const saved = yield* writeDocument(draft, true, empty.revision);
        const published = yield* readDocument('published');
        assert.deepStrictEqual([empty.revision, saved, published.data.portfolio.name, published.revision], [0, 1, 'Studio', saved]);
        assert.deepStrictEqual(yield* writeDocument(draft, false, saved), saved);
        assert.deepStrictEqual(yield* writeDocument({ ...draft, name: 'Atelier' }, false, saved), saved + 1);
        assert.deepStrictEqual((yield* Effect.flip(writeDocument(draft, false, empty.revision)))._tag, 'PreconditionFailed');
    }).pipe(Effect.provide(store(catalog, client))),
);
it.effect('refuses a draft placing a missing, removed, or mistyped asset or an absent sheet, and saves one placing a present sheet', () =>
    Effect.gen(function* () {
        const sql = yield* SqlClient.SqlClient;
        const [sheet, removed, missing] = ['0b3f6c1e-2d4a-4e8b-9c7d-1a2b3c4d5e6f', '1c4a7d2f-3e5b-4f9c-8d6e-2b3c4d5e6f7a', '2d5b8e3a-4f6c-4a0d-9e7f-3c4d5e6f7a8b'];
        const draft = { name: 'Studio', introduction: '', email: '', entries: [] };
        const sheets = (page: number) => ({ caption: '', assetId: sheet, kind: 'pdf', page }) as const;
        yield* sql`INSERT INTO assets ${sql.insert([
            { id: sheet, body: '{"name":"set.pdf","size":1,"mime":"application/pdf","pages":[{"width":1,"height":1}],"objectKey":"sheet","uploads":[]}' },
            { id: removed, body: '{"name":"plan.png","size":1,"mime":"image/png","width":1,"height":1,"objectKey":"plan","removing":true,"uploads":[]}' },
        ])}`;
        const refused = yield* Effect.forEach([...[missing, removed, sheet].map((assetId) => ({ caption: '', assetId, kind: 'image', role: 'drawing', framing: { mode: 'contain' } }) as const), sheets(2)], (hero) => Effect.flip(writeDocument({ ...draft, hero }, false, 0)));
        assert.deepStrictEqual([...refused.map(({ _tag }) => _tag), yield* writeDocument({ ...draft, hero: sheets(1) }, false, 0)], ['Conflict', 'Conflict', 'Conflict', 'Conflict', 1]);
    }).pipe(Effect.provide(store(catalog, client))),
);
it.effect('reads an asset or one of its renditions and refuses an absent rendition', () =>
    Effect.gen(function* () {
        const sql = yield* SqlClient.SqlClient;
        const read = (width: Option.Option<number>) => readAsset({ id: 'photo', width });
        yield* sql`INSERT INTO assets ${sql.insert({ id: 'photo', body: '{"name":"photo.png","size":1,"mime":"image/png","width":2,"height":1,"renditions":[{"width":1,"size":1,"mime":"image/jpeg"}],"objectKey":"photo","uploads":[]}' })}`;
        assert.deepStrictEqual([yield* read(Option.none()), yield* read(Option.some(1)), (yield* Effect.flip(read(Option.some(2))))._tag], [{ mime: 'image/png', objectKey: 'photo', published: false }, { mime: 'image/jpeg', objectKey: 'photo/1', published: false }, 'NotFound']);
    }).pipe(Effect.provide(store(catalog, client))),
);
