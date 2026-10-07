import { env } from 'cloudflare:workers';
import { BrowserCrypto } from '@effect/platform-browser';
import { D1Client } from '@effect/sql-d1';
import { Array, type Cause, Crypto, Effect, Layer, Match, Option, Record, Schema, String, Struct } from 'effect';
import { Hex } from 'effect/encoding';
import { HttpApiError } from 'effect/http-api';
import { isPreconditionFailure } from 'partial-content';
import { PreconditionFailed } from '../model/api.ts';
import { Asset } from '../model/asset.ts';
import { emptyPortfolio, Portfolio, type StoredPortfolio } from '../model/document.ts';
import { Assets } from '../model/placement.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type State = 'draft' | 'published';

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [SERVICE_UNAVAILABLE]
const unavailable = (error: Cause.YieldableError): Effect.Effect<never, HttpApiError.ServiceUnavailable> => Effect.andThen(Effect.logError(error), Effect.fail(new HttpApiError.ServiceUnavailable()));

// --- [DOCUMENTS]
const contentEtag = (body: string): Effect.Effect<string, HttpApiError.ServiceUnavailable> =>
    Crypto.Crypto.use((crypto) => crypto.digest('SHA-256', new TextEncoder().encode(body))).pipe(
        Effect.map((digest) => `"${Hex.encode(digest)}"`),
        Effect.provide(BrowserCrypto.layer),
        Effect.catchTag('PlatformError', unavailable),
    );
const readDocument = Effect.fn('readDocument')(
    function* (state: State) {
        const sql = yield* D1Client.D1Client;
        const visible = sql`a.object_key IS NOT NULL AND a.removing = 0 AND (
            ${state} = 'draft' OR a.id IN (
                SELECT asset_id FROM portfolio_references JOIN documents USING (digest) WHERE state = ${state}
            )
        )`;
        const [documents, entries, compositions, rows, pages, renditions] = yield* sql.batch([
            sql<{ body: string; digest: string }>`SELECT body, digest FROM portfolios JOIN documents USING (digest) WHERE state = ${state}`,
            sql<{ id: string; body: string }>`SELECT id, body FROM entries JOIN documents USING (digest) WHERE state = ${state} ORDER BY position`,
            sql<{ entryId: string; body: string }>`SELECT entry_id, body FROM compositions JOIN documents USING (digest) WHERE state = ${state} ORDER BY entry_id, position`,
            sql<{ id: string; body: string }>`SELECT a.id, a.body FROM assets a WHERE ${visible}`,
            sql<{ assetId: string; body: string }>`SELECT p.asset_id, p.body FROM pages p JOIN assets a ON a.id = p.asset_id WHERE ${visible} ORDER BY p.asset_id, p.position`,
            sql<{ assetId: string; body: string }>`SELECT r.asset_id, r.body FROM renditions r JOIN assets a ON a.id = r.asset_id WHERE ${visible} ORDER BY r.asset_id, r.position`,
        ]);
        const decode = Schema.decodeUnknownEffect(Schema.fromJsonString(Schema.ObjectKeyword));
        const sheets = yield* Effect.forEach(pages, ({ assetId, body }) => decode(body).pipe(Effect.map((page) => ({ assetId, page }))));
        const pageGroups = Record.map(Array.groupBy(sheets, Struct.get('assetId')), (group) => ({ pages: group.map(Struct.get('page')) }));
        const images = yield* Effect.forEach(renditions, ({ assetId, body }) => decode(body).pipe(Effect.map((rendition) => ({ assetId, rendition }))));
        const renditionGroups = Record.map(Array.groupBy(images, Struct.get('assetId')), (group) => ({ renditions: group.map(Struct.get('rendition')) }));
        const assets = yield* Effect.forEach(rows, ({ id, body }) => decode(body).pipe(Effect.flatMap((asset) => Schema.decodeUnknownEffect(Asset)({ ...asset, id, ...pageGroups[id], ...renditionGroups[id] })))).pipe(Effect.map(Record.fromIterableBy(Struct.get('id'))));
        const selected = Array.head(documents);
        const metadata = yield* Option.match(selected, { onNone: () => Effect.succeed(emptyPortfolio), onSome: ({ body }) => decode(body) });
        const ordered = yield* Effect.forEach(compositions, ({ entryId, body }) => decode(body).pipe(Effect.map((composition) => ({ entryId, composition }))));
        const grouped = Array.groupBy(ordered, Struct.get('entryId'));
        const chapters = yield* Effect.forEach(entries, ({ id, body }) => decode(body).pipe(Effect.map((entry) => ({ ...entry, id, compositions: grouped[id]?.map(Struct.get('composition')) ?? [] }))));
        const portfolio = yield* Schema.decodeUnknownEffect(Portfolio)({ ...metadata, entries: chapters }).pipe(Effect.provideService(Assets, assets));
        const etag = yield* Option.match(selected, { onNone: () => contentEtag(JSON.stringify(emptyPortfolio)), onSome: ({ digest }) => Effect.succeed(digest) });
        return { data: { portfolio, assets }, etag };
    },
    Effect.catchTag(['SqlError', 'SchemaError'], unavailable),
);
const writeDocument = Effect.fn('writeDocument')(
    function* (portfolio: typeof StoredPortfolio.Type, states: readonly State[], etag: string) {
        const sql = yield* D1Client.D1Client;
        const emptyEtag = yield* contentEtag(JSON.stringify(emptyPortfolio));
        const previous = yield* sql<{ digest: string }>`SELECT digest FROM documents WHERE state = 'draft'`.pipe(Effect.map((rows) => Option.getOrElse(Option.map(Array.head(rows), Struct.get('digest')), () => emptyEtag)));
        if (isPreconditionFailure(new Headers({ 'if-match': etag }), previous, undefined)) {
            return yield* new PreconditionFailed();
        }
        const digest = yield* contentEtag(JSON.stringify(portfolio));
        const { entries, ...metadata } = portfolio;
        const unchanged = sql`COALESCE((SELECT digest FROM documents WHERE state = 'draft'), ${emptyEtag}) = ${previous}`;
        const references = sql`NOT EXISTS (
            SELECT 1 FROM portfolio_references r LEFT JOIN assets a ON a.id = r.asset_id
            WHERE r.digest = ${digest} AND (
                a.id IS NULL OR a.object_key IS NULL OR a.removing <> 0 OR
                CASE r.kind
                    WHEN 'pdf' THEN a.mime <> 'application/pdf' OR NOT EXISTS (
                        SELECT 1 FROM pages WHERE asset_id = r.asset_id AND position = r.page - 1
                    )
                    ELSE a.mime NOT LIKE r.kind || '/%'
                END
            )
        )`;
        const results = yield* sql.batch([
            sql<never>`INSERT INTO portfolios(digest,body) VALUES (${digest},${JSON.stringify(metadata)}) ON CONFLICT(digest) DO NOTHING`,
            ...entries.flatMap(({ id, compositions, ...entry }, position) => [
                sql<never>`INSERT INTO entries(digest,id,position,body) VALUES (${digest},${id},${position},${JSON.stringify(entry)}) ON CONFLICT(digest,id) DO NOTHING`,
                ...compositions.map((composition, index) => sql<never>`INSERT INTO compositions(digest,entry_id,position,body) VALUES (${digest},${id},${index},${JSON.stringify(composition)}) ON CONFLICT(digest,entry_id,position) DO NOTHING`),
            ]),
            sql<{ status: 'saved' | 'stale' | 'missing' }>`SELECT CASE WHEN NOT (${unchanged}) THEN 'stale' WHEN NOT (${references}) THEN 'missing' ELSE 'saved' END AS status`,
            sql<never>`
                WITH current AS MATERIALIZED (SELECT ${unchanged} AND ${references} AS accepted)
                INSERT INTO documents(state,digest)
                SELECT value, ${digest} FROM json_each(${JSON.stringify(states)}), current WHERE accepted
                ON CONFLICT(state) DO UPDATE SET digest = excluded.digest
            `,
            sql<never>`DELETE FROM portfolios WHERE digest NOT IN (SELECT digest FROM documents)`,
        ]);
        return yield* Match.value(Array.getUnsafe(Array.flatten(results), 0)).pipe(
            Match.when({ status: 'saved' }, () => Effect.succeed(digest)),
            Match.when({ status: 'stale' }, () => Effect.fail(new PreconditionFailed())),
            Match.when({ status: 'missing' }, () => Effect.fail(new HttpApiError.Conflict())),
            Match.exhaustive,
        );
    },
    Effect.catchTag('SqlError', unavailable),
);

// --- [ASSETS]
const readAsset = Effect.fn('readAsset')(
    function* (id: string, width: Option.Option<number>) {
        const sql = yield* D1Client.D1Client;
        return yield* sql<{ mime: string; published: number; objectKey: string }>`
            WITH request(width) AS (VALUES (${Option.getOrNull(width)}))
            SELECT CASE WHEN request.width IS NULL THEN a.mime ELSE r.body ->> '$.mime' END AS mime,
                CASE WHEN request.width IS NULL THEN a.object_key ELSE a.object_key || '/' || (r.body ->> '$.width') END AS object_key,
                EXISTS (
                SELECT 1 FROM portfolio_references JOIN documents USING (digest)
                WHERE state = 'published' AND asset_id = ${id}
                ) AS published
            FROM assets a CROSS JOIN request LEFT JOIN renditions r
                ON r.asset_id = a.id AND r.body ->> '$.width' = request.width
            WHERE a.id = ${id} AND a.object_key IS NOT NULL AND a.removing = 0
                AND (request.width IS NULL OR r.asset_id IS NOT NULL)
        `.pipe(
            Effect.filterOrFail(Array.isReadonlyArrayNonEmpty, () => new HttpApiError.NotFound()),
            Effect.map(Array.headNonEmpty),
        );
    },
    Effect.catchTag('SqlError', unavailable),
);

// --- [OWNER]
const ownerUserId = Effect.fn('ownerUserId')(
    function* (email: string) {
        const sql = yield* D1Client.D1Client;
        return yield* sql<{ userId: string }>`SELECT user_id FROM owner WHERE email = ${email}`.pipe(Effect.map(Array.head));
    },
    Effect.catchTag('SqlError', unavailable),
);
const registerOwner = Effect.fn('registerOwner')(
    function* (email: string, userId: string) {
        const sql = yield* D1Client.D1Client;
        return yield* sql<{ userId: string }>`INSERT INTO owner(email,user_id) VALUES (${email},${userId}) ON CONFLICT(email) DO UPDATE SET user_id = owner.user_id RETURNING user_id`.pipe(Effect.map(Array.head));
    },
    Effect.catchTag('SqlError', unavailable),
);

// --- [COMPOSITION] ---------------------------------------------------------------------

const database = Layer.provideMerge(
    Layer.effectDiscard(
        D1Client.D1Client.use((sql) =>
            sql.batch([
                sql`CREATE TABLE IF NOT EXISTS owner (email TEXT PRIMARY KEY, user_id TEXT NOT NULL)`,
                sql`CREATE TABLE IF NOT EXISTS portfolios (digest TEXT PRIMARY KEY, body TEXT NOT NULL)`,
                sql`CREATE TABLE IF NOT EXISTS documents (state TEXT PRIMARY KEY, digest TEXT NOT NULL REFERENCES portfolios(digest))`,
                sql`CREATE TABLE IF NOT EXISTS entries (digest TEXT NOT NULL REFERENCES portfolios(digest) ON DELETE CASCADE, id TEXT NOT NULL, position INTEGER NOT NULL, body TEXT NOT NULL, PRIMARY KEY(digest,id))`,
                sql`CREATE TABLE IF NOT EXISTS compositions (digest TEXT NOT NULL, entry_id TEXT NOT NULL, position INTEGER NOT NULL, body TEXT NOT NULL, PRIMARY KEY(digest,entry_id,position), FOREIGN KEY(digest,entry_id) REFERENCES entries(digest,id) ON DELETE CASCADE)`,
                sql`CREATE TABLE IF NOT EXISTS assets (id TEXT PRIMARY KEY, digest TEXT NOT NULL, body TEXT NOT NULL, mime TEXT GENERATED ALWAYS AS (body ->> '$.mime') VIRTUAL, object_key TEXT, removing INTEGER NOT NULL DEFAULT 0)`,
                sql`CREATE TABLE IF NOT EXISTS pages (asset_id TEXT NOT NULL REFERENCES assets(id) ON DELETE CASCADE, position INTEGER NOT NULL, body TEXT NOT NULL, PRIMARY KEY(asset_id,position))`,
                sql`CREATE TABLE IF NOT EXISTS renditions (asset_id TEXT NOT NULL REFERENCES assets(id) ON DELETE CASCADE, position INTEGER NOT NULL, body TEXT NOT NULL, PRIMARY KEY(asset_id,position))`,
                sql`CREATE TABLE IF NOT EXISTS upload_attempts (id TEXT PRIMARY KEY, asset_id TEXT NOT NULL REFERENCES assets(id) ON DELETE CASCADE, discard INTEGER NOT NULL DEFAULT 0)`,
                sql`CREATE TABLE IF NOT EXISTS upload_objects (key TEXT PRIMARY KEY, attempt_id TEXT NOT NULL REFERENCES upload_attempts(id) ON DELETE CASCADE, upload_id TEXT)`,
                sql`CREATE INDEX IF NOT EXISTS upload_attempts_asset ON upload_attempts(asset_id)`,
                sql`CREATE INDEX IF NOT EXISTS upload_objects_attempt ON upload_objects(attempt_id)`,
                sql`CREATE VIEW IF NOT EXISTS portfolio_references AS
                SELECT digest, j.value ->> '$.assetId' AS asset_id,
                    CASE WHEN j.key = 'poster' THEN 'image' ELSE j.value ->> '$.kind' END AS kind,
                    j.value ->> '$.page' AS page
                FROM (
                    SELECT digest, body FROM portfolios UNION ALL
                    SELECT digest, body FROM entries UNION ALL
                    SELECT digest, body FROM compositions
                ) p, json_tree(p.body) j
                WHERE CASE WHEN j.type = 'object' THEN json_type(j.value,'$.assetId') END = 'text'
            `,
            ]),
        ),
    ),
    D1Client.layer({ db: env.DB, transformResultNames: String.snakeToCamel }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { contentEtag, database, ownerUserId, readAsset, readDocument, registerOwner, unavailable, writeDocument };
