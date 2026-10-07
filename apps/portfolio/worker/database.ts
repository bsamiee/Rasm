import { env } from 'cloudflare:workers';
import { D1Client } from '@effect/sql-d1';
import { Array, type Cause, Effect, Layer, Schema, String as Strings } from 'effect';
import { HttpApiError } from 'effect/http-api';
import type { Asset } from '../model/asset.ts';
import { emptyPortfolio, PortfolioData, type StoredPortfolio } from '../model/document.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type State = 'draft' | 'published';

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [SERVICE_UNAVAILABLE]
const unavailable = (error: Cause.YieldableError): Effect.Effect<never, HttpApiError.ServiceUnavailable> => Effect.andThen(Effect.logError(error), Effect.fail(new HttpApiError.ServiceUnavailable()));

// --- [DOCUMENTS]
const decodePortfolioData = Schema.decodeUnknownEffect(Schema.Tuple([Schema.Struct({ body: Schema.fromJsonString(PortfolioData) })]));
const readDocument = Effect.fn('readDocument')(
    function* (state: State) {
        const sql = yield* D1Client.D1Client;
        return yield* sql`
            WITH document AS (SELECT COALESCE((SELECT body FROM documents WHERE state = ${state}), ${JSON.stringify(emptyPortfolio)}) AS body)
            SELECT json_object('portfolio', json(document.body), 'assets', json((
                SELECT json_group_object(assets.id, json(assets.body)) FROM assets
                WHERE ${state} = 'draft' OR assets.id IN (SELECT value FROM json_tree(document.body) WHERE key = 'assetId')
            ))) AS body FROM document
        `.pipe(
            Effect.flatMap(decodePortfolioData),
            Effect.map(([row]) => row.body),
        );
    },
    Effect.catchTag(['SqlError', 'SchemaError'], unavailable),
);
const writeDocument = Effect.fn('writeDocument')(
    function* (portfolio: typeof StoredPortfolio.Type, states: readonly State[]) {
        const sql = yield* D1Client.D1Client;
        const body = JSON.stringify(portfolio);
        yield* sql`
            SELECT json_object('portfolio', json(${body}), 'assets', json((
                SELECT json_group_object(id, json(assets.body)) FROM assets WHERE id IN (SELECT value FROM json_tree(${body}) WHERE key = 'assetId')
            ))) AS body
        `.pipe(
            Effect.flatMap(decodePortfolioData),
            Effect.catchTag('SchemaError', () => Effect.fail(new HttpApiError.Conflict())),
        );
        yield* sql`
            INSERT INTO documents(state,body)
            SELECT value, ${body} FROM json_each(${JSON.stringify(states)})
            WHERE NOT EXISTS (SELECT 1 FROM json_tree(${body}) WHERE key = 'assetId' AND value NOT IN (SELECT id FROM assets))
            ON CONFLICT(state) DO UPDATE SET body = excluded.body RETURNING state
        `.pipe(Effect.filterOrFail(Array.isReadonlyArrayNonEmpty, () => new HttpApiError.Conflict()));
    },
    Effect.catchTag('SqlError', unavailable),
);

// --- [ASSETS]
const storeAsset = Effect.fn('storeAsset')(
    function* (asset: typeof Asset.Type) {
        const sql = yield* D1Client.D1Client;
        yield* sql`
            INSERT INTO assets(id,body) VALUES (${asset.id},${JSON.stringify(asset)})
            ON CONFLICT(id) DO UPDATE SET body = assets.body WHERE assets.body = excluded.body RETURNING id
        `.pipe(Effect.filterOrFail(Array.isReadonlyArrayNonEmpty, () => new HttpApiError.Conflict()));
    },
    Effect.catchTag('SqlError', unavailable),
);
const deleteAsset = Effect.fn('deleteAsset')(
    function* (id: string) {
        const sql = yield* D1Client.D1Client;
        yield* sql`
            DELETE FROM assets WHERE id = ${id} AND NOT EXISTS (
                SELECT 1 FROM documents, json_tree(documents.body) WHERE json_tree.key = 'assetId' AND json_tree.value = ${id}
            ) RETURNING id
        `.pipe(Effect.filterOrElse(Array.isReadonlyArrayNonEmpty, () => sql`SELECT id FROM assets WHERE id = ${id}`.pipe(Effect.filterOrFail(Array.isReadonlyArrayEmpty, () => new HttpApiError.Conflict()))));
    },
    Effect.catchTag('SqlError', unavailable),
);
const isPublished = Effect.fn('isPublished')(
    function* (id: string) {
        const sql = yield* D1Client.D1Client;
        return yield* sql`SELECT 1 FROM documents, json_tree(documents.body) WHERE state = 'published' AND json_tree.key = 'assetId' AND json_tree.value = ${id} LIMIT 1`.pipe(Effect.map(Array.isReadonlyArrayNonEmpty));
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
        D1Client.D1Client.use((sql) => sql.batch([sql`CREATE TABLE IF NOT EXISTS owner (email TEXT PRIMARY KEY, user_id TEXT NOT NULL)`, sql`CREATE TABLE IF NOT EXISTS documents (state TEXT PRIMARY KEY, body TEXT NOT NULL)`, sql`CREATE TABLE IF NOT EXISTS assets (id TEXT PRIMARY KEY, body TEXT NOT NULL)`])),
    ),
    D1Client.layer({ db: env.DB, transformResultNames: Strings.snakeToCamel }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { database, deleteAsset, isPublished, ownerUserId, readDocument, registerOwner, storeAsset, unavailable, writeDocument };
