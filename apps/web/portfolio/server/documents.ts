import { atomic } from '@rasm/database';
import { unavailable } from '@rasm/hosting';
import { Array, Effect, Function, Match, Schema, SchemaGetter } from 'effect';
import { HttpApiBuilder, HttpApiSchema } from 'effect/http-api';
import { SqlClient, SqlSchema } from 'effect/sql';
import { Api, PlacementConflict, PreconditionFailed } from '#/model/api.ts';
import { AssetId, type ImageAsset } from '#/model/asset.ts';
import { Portfolio, PortfolioData, type StoredPortfolio } from '#/model/portfolio.ts';
import { documents, projectedPaths, references } from '#/server/catalog.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const treeSegment = /\.(?<key>[^.[]+)|\[(?<index>\d+)\]/gu;

// --- [MODELS] --------------------------------------------------------------------------

const TreePath = Schema.String.pipe(
    Schema.decodeTo(Schema.Array(Schema.Union([Schema.String, Schema.Int])), {
        decode: SchemaGetter.transform((fullkey) => Array.fromIterable(fullkey.matchAll(treeSegment)).map(([, key, index]) => (index === undefined ? `${key}` : Number(index)))),
        encode: SchemaGetter.transform((path) => ['$', ...path.map((segment) => (typeof segment === 'number' ? `[${segment}]` : `.${segment}`))].join('')),
    }),
);
const Stored = Schema.Struct({ revision: documents.schema.fields.revision, data: Schema.fromJsonString(PortfolioData) });
const Draft = Schema.Struct({ revision: documents.schema.fields.revision, stored: Schema.BooleanFromBit, unplaced: Schema.fromJsonString(Schema.Array(Schema.Struct({ path: TreePath, assetId: AssetId }))) });

// --- [OPERATIONS] ----------------------------------------------------------------------

const empty = Schema.encodeEffect(Schema.fromJsonString(Portfolio))(Portfolio.make({})).pipe(Effect.orDie);
const readDocument = Effect.fn('readDocument')(
    Function.flow(
        SqlSchema.findOne({
            Request: documents.schema.fields.state,
            Result: Stored,
            execute: Effect.fnUntraced(function* (requested: typeof documents.schema.fields.state.Encoded) {
                const [sql, body] = yield* Effect.all([SqlClient.SqlClient, empty]);
                return yield* sql`SELECT COALESCE(d.revision, 0) AS revision, json_object('portfolio', json(COALESCE(d.body, ${body})), 'assets', (
                SELECT json_group_object(a.id, json_set(json_remove(a.body, ${projectedPaths}), '$.id', a.id)) FROM assets a
                WHERE a.available AND (s.state = 'draft' OR a.id IN (SELECT asset_id FROM (${references(sql)}) WHERE state = s.state))
            )) AS data
            FROM (SELECT ${requested} AS state) s LEFT JOIN documents d ON d.state = s.state`;
            }),
        }),
        Effect.catchTag('NoSuchElementError', Effect.die),
        Effect.catchTag(['SqlError', 'SchemaError'], unavailable),
    ),
);
const storeDraft = SqlSchema.findOne({
    Request: Schema.Struct({ body: documents.schema.fields.body, publish: Schema.Boolean, revision: documents.schema.fields.revision }),
    Result: Draft,
    execute: ({ body, publish, revision }) =>
        SqlClient.SqlClient.use((sql) => {
            const unplaced = sql`SELECT r.fullkey, r.asset_id FROM (${references(sql, sql`(SELECT 'draft' AS state, ${body} AS body)`)}) r
            LEFT JOIN assets a ON a.id = r.asset_id AND a.available AND a.body ->> '$.kind' = coalesce(r.placement ->> '$.kind', ${'image' satisfies ImageAsset['kind']})
                AND (r.placement ->> '$.page' IS NULL OR r.placement ->> '$.page' <= json_array_length(a.body, '$.pages'))
            WHERE a.id IS NULL`;
            const save = sql`INSERT INTO documents (state, body, revision) SELECT 'draft', ${body}, ${revision + 1} FROM (SELECT 1) n LEFT JOIN documents d ON d.state = 'draft'
            WHERE COALESCE(d.revision, 0) = ${revision} AND NOT EXISTS (SELECT 1 FROM (${unplaced}) u)
            ON CONFLICT (state) DO UPDATE SET body = excluded.body, revision = excluded.revision WHERE documents.body IS DISTINCT FROM excluded.body`;
            const published = sql`INSERT INTO documents (state, body, revision) SELECT 'published', body, revision FROM documents WHERE state = 'draft' AND body = ${body}
            ON CONFLICT (state) DO UPDATE SET body = excluded.body, revision = excluded.revision
            WHERE (documents.body, documents.revision) IS DISTINCT FROM (excluded.body, excluded.revision)`;
            const current = sql`SELECT COALESCE(MAX(revision), 0) AS revision, COALESCE(MAX(body = ${body}), 0) AS stored,
            (SELECT json_group_array(json_object('path', u.fullkey, 'assetId', u.asset_id)) FROM (${unplaced}) u) AS unplaced
            FROM documents WHERE state = 'draft'`;
            return Effect.map(atomic([save, ...(publish ? [published] : []), current]), Array.lastNonEmpty);
        }),
});
const writeDocument = Effect.fn('writeDocument')(
    function* (portfolio: typeof StoredPortfolio.Type, publish: boolean, revision: number) {
        const draft = yield* storeDraft({ body: portfolio, publish, revision });
        return yield* Match.value(draft).pipe(
            Match.when({ stored: true }, ({ revision: stored }) => Effect.succeed(HttpApiSchema.withHeaders({ body: undefined, headers: { 'draft-etag': stored } }))),
            Match.when({ revision, unplaced: Schema.is(PlacementConflict.fields.placements) }, ({ unplaced }) => PlacementConflict.make({ placements: unplaced })),
            Match.orElse(({ revision: stored }) => PreconditionFailed.make({ revision: stored })),
        );
    },
    Effect.catchTag('NoSuchElementError', Effect.die),
    Effect.catchTag(['SqlError', 'SchemaError'], unavailable),
);

// --- [COMPOSITION] ---------------------------------------------------------------------

const documentGroup = HttpApiBuilder.group(Api, 'documents', (handlers) =>
    handlers.handleAll({
        draft: () => Effect.map(readDocument('draft'), ({ data, revision }) => HttpApiSchema.withHeaders({ body: data, headers: { 'draft-etag': revision } })),
        save: ({ payload, headers }) => writeDocument(payload, false, headers['if-match']),
        publish: ({ payload, headers }) => writeDocument(payload, true, headers['if-match']),
    }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { documentGroup, readDocument };
