import { atomic, type Catalog, literal, table } from '@rasm/database/table.ts';
import { unavailable } from '@rasm/hosting/http.ts';
import { Array, Effect, flow, Match, Option, Record, Schema, Struct } from 'effect';
import { HttpApiError } from 'effect/http-api';
import { SqlClient, SqlSchema, type Statement } from 'effect/sql';
import { PreconditionFailed } from '../model/api.ts';
import { Portfolio, PortfolioData, StoredPortfolio } from '../model/document.ts';
import { Placement } from '../model/placement.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const mimes = JSON.stringify(Record.fromIterableWith(Placement.to.members, ({ fields }) => [fields.kind.literal, fields.asset.schema.fields.mime.literals]));

// --- [MODELS] --------------------------------------------------------------------------

const documents = table('documents', { state: Schema.Literals(['draft', 'published']), body: Schema.fromJsonString(StoredPortfolio), revision: Schema.Natural }, { key: ['state'], defaults: { revision: 0 } });
const Upload = Schema.Struct({ id: Schema.String, discard: Schema.Boolean, objects: Schema.Array(Schema.Struct({ key: Schema.String, uploadId: Schema.OptionFromOptionalKey(Schema.String) })) });
const assets = table(
    'assets',
    { id: Schema.String, body: Schema.fromJsonString(Schema.Unknown), objectKey: Schema.OptionFromNullOr(Schema.String), removing: Schema.NullOr(Schema.BooleanFromBit), uploads: Schema.NullOr(Schema.fromJsonString(Schema.Array(Upload))), available: Schema.BooleanFromBit },
    {
        projections: {
            objectKey: { from: 'body', path: ['objectKey'] },
            removing: { from: 'body', path: ['removing'] },
            uploads: { from: 'body', path: ['uploads'] },
            available: { where: { objectKey: { not: null }, removing: { not: [1] } } },
        },
    },
);
const bookkeeping = Object.values(assets.table.columns)
    .flatMap(({ projection }) => Option.toArray(projection))
    .flatMap((projected) => (projected._tag === 'Path' ? [literal(['$', ...projected.path].join('.'))] : []))
    .join(', ');
const AssetRead = Schema.Struct({ id: assets.fields.id, width: Schema.OptionFromNullOr(Schema.Finite) });
const owner = table('owner', { email: Schema.String, userId: Schema.String }, { key: ['email'], withoutRowid: true });
const catalog = { tables: [documents, assets, owner], retired: [] } as const satisfies Catalog;

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [DOCUMENTS]
const references = (sql: SqlClient.SqlClient, source: Statement.Fragment) => sql`SELECT d.state, j.value AS asset_id, j.json -> j.path AS placement
    FROM ${source} d, json_tree(d.body) j WHERE j.key = 'assetId' AND j.type = 'text'`;
const readDocument = Effect.fn('readDocument')(
    (state: typeof documents.fields.state.Type) =>
        Effect.flatten(
            Effect.zipWith(
                SqlClient.SqlClient,
                Schema.encodeEffect(Schema.fromJsonString(Portfolio))(Portfolio.make({})),
                (sql, empty) => sql`SELECT COALESCE(d.revision, 0) AS revision, json_object('portfolio', json(COALESCE(d.body, ${empty})), 'assets', (
                    SELECT json_group_object(a.id, json_set(json_remove(a.body, ${sql.literal(bookkeeping)}), '$.id', a.id)) FROM assets a
                    WHERE a.available AND (s.state = 'draft' OR a.id IN (SELECT asset_id FROM (${references(sql, sql.literal('documents'))}) WHERE state = s.state))
                )) AS data
                FROM (SELECT ${state} AS state) s LEFT JOIN documents d ON d.state = s.state`,
            ),
        ),
    Effect.flatMap(Schema.decodeUnknownEffect(Schema.Tuple([Schema.Struct({ revision: documents.fields.revision, data: Schema.fromJsonString(PortfolioData) })]))),
    Effect.map(([row]) => row),
    Effect.catchTag(['SqlError', 'SchemaError'], unavailable),
);
const writeDocument = Effect.fn('writeDocument')(
    function* (portfolio: typeof StoredPortfolio.Type, publish: boolean, revision: number) {
        const [sql, body] = yield* Effect.all([SqlClient.SqlClient, Schema.encodeEffect(documents.fields.body)(portfolio)]);
        const save = sql`INSERT INTO documents (state, body, revision) SELECT 'draft', n.body, ${revision + 1} FROM (SELECT ${body} AS body) n WHERE NOT EXISTS (
                SELECT 1 FROM (${references(sql, sql`(SELECT 'draft' AS state, n.body AS body)`)}) r
                LEFT JOIN assets a ON a.id = r.asset_id AND a.available AND (a.body ->> '$.mime') IN (SELECT value FROM json_each(${mimes}, '$.' || coalesce(r.placement ->> '$.kind', 'image')))
                    AND (r.placement ->> '$.page' IS NULL OR r.placement ->> '$.page' <= json_array_length(a.body, '$.pages'))
                WHERE a.id IS NULL
            )
            ON CONFLICT (state) DO UPDATE SET body = excluded.body, revision = excluded.revision
            WHERE documents.revision = ${revision} AND documents.body IS DISTINCT FROM excluded.body
            RETURNING revision`;
        const state = sql`SELECT COALESCE(MAX(revision), 0) AS revision, COALESCE(MAX(body = ${body}), 0) AS unchanged FROM documents WHERE state = 'draft'`;
        const published = sql`INSERT INTO documents (state, body, revision) SELECT 'published', body, revision FROM documents WHERE state = 'draft' AND body = ${body} AND revision IN (${revision}, ${revision + 1})
            ON CONFLICT (state) DO UPDATE SET body = excluded.body, revision = excluded.revision
            WHERE (documents.body, documents.revision) IS DISTINCT FROM (excluded.body, excluded.revision)`;
        const [saved, [current]] = yield* Effect.flatMap(
            atomic([save, state, ...(publish ? [published] : [])]),
            Schema.decodeUnknownEffect(Schema.TupleWithRest(Schema.Tuple([Schema.Array(Schema.Unknown), Schema.Tuple([Schema.Struct({ revision: documents.fields.revision, unchanged: Schema.BooleanFromBit })])]), [Schema.Unknown])),
        );
        return yield* Match.value({ written: Array.isReadonlyArrayNonEmpty(saved), ...current }).pipe(
            Match.whenOr({ written: true }, { revision, unchanged: true }, ({ revision: stored }) => Effect.succeed(stored)),
            Match.when({ revision }, () => Effect.fail(HttpApiError.Conflict.make())),
            Match.orElse(() => Effect.fail(PreconditionFailed.make())),
        );
    },
    Effect.catchTag(['SqlError', 'SchemaError'], unavailable),
);

// --- [ASSETS]
const readAsset = Effect.fn('readAsset')(
    (request: typeof AssetRead.Type) =>
        SqlSchema.findOne({
            Request: AssetRead,
            Result: Schema.Struct({ mime: Schema.String, published: Schema.BooleanFromBit, objectKey: Schema.String }),
            execute: ({ id, width }) =>
                SqlClient.SqlClient.use(
                    (sql) => sql`SELECT COALESCE(r.value ->> '$.mime', a.body ->> '$.mime') AS mime, concat_ws('/', a.${sql('objectKey')}, r.value ->> '$.width') AS ${sql('objectKey')},
                        EXISTS (SELECT 1 FROM (${references(sql, sql.literal('documents'))}) WHERE state = 'published' AND asset_id = a.id) AS published
                    FROM (SELECT ${id} AS id, ${width} AS width) q JOIN assets a ON a.id = q.id LEFT JOIN json_each(a.body, '$.renditions') r ON r.value ->> '$.width' = q.width
                    WHERE a.available AND q.width IS r.value ->> '$.width'`,
                ),
        })(request),
    Effect.catchTag(['SqlError', 'SchemaError'], unavailable, () => Effect.fail(HttpApiError.NotFound.make())),
);

// --- [OWNER]
const ownerUserId = Effect.fn('ownerUserId')((email: string) => Effect.flatMap(Effect.flatten(owner.select('email', [email])), Schema.decodeUnknownEffect(Schema.Array(owner))), Effect.map(flow(Array.head, Option.map(Struct.get('userId')))), Effect.catchTag(['SqlError', 'SchemaError'], unavailable));
const registerOwner = Effect.fn('registerOwner')(
    (row: typeof owner.Type) => Effect.flatMap(owner.ensure([row]), Schema.decodeUnknownEffect(Schema.Tuple([owner]))),
    Effect.map(([{ userId }]) => userId),
    Effect.catchTag(['SqlError', 'SchemaError'], unavailable),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { assets, bookkeeping, catalog, ownerUserId, readAsset, readDocument, references, registerOwner, Upload, writeDocument };
