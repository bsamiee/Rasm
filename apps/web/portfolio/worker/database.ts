import { atomic, table } from '@rasm/database/table.ts';
import { unavailable } from '@rasm/hosting/http.ts';
import { Array, Effect, flow, Match, Option, Schema, Struct } from 'effect';
import { HttpApiError } from 'effect/http-api';
import { SqlClient, SqlSchema, type Statement } from 'effect/sql';
import { PreconditionFailed } from '../model/api.ts';
import { ImageAsset, PdfAsset, VideoAsset } from '../model/asset.ts';
import { Portfolio, PortfolioData, StoredPortfolio } from '../model/document.ts';

// --- [MODELS] --------------------------------------------------------------------------

const documents = table('documents', { state: Schema.Literals(['draft', 'published']), body: Schema.fromJsonString(StoredPortfolio), revision: Schema.Int.check(Schema.isGreaterThanOrEqualTo(0)) }, { key: ['state'], defaults: { revision: 0 } });
const assets = table(
    'assets',
    { id: Schema.String, body: Schema.fromJsonString(Schema.Unknown), objectKey: Schema.OptionFromNullOr(Schema.String), removing: Schema.NullOr(Schema.BooleanFromBit), uploads: Schema.NullOr(Schema.fromJsonString(Schema.Unknown)) },
    { projections: { objectKey: { from: 'body', path: ['objectKey'] }, removing: { from: 'body', path: ['removing'] }, uploads: { from: 'body', path: ['uploads'] } } },
);
const owner = table('owner', { email: Schema.String, userId: Schema.String }, { key: ['email'], withoutRowid: true });
const catalog = { tables: [documents, assets, owner], retired: [] } as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [DOCUMENTS]
const references = (sql: SqlClient.SqlClient, source: Statement.Fragment) => sql`SELECT d.state, j.value AS asset_id, j.json -> j.path AS placement
    FROM ${source} d, json_tree(d.body) j WHERE j.key = 'assetId' AND j.type = 'text'`;
const readDocument = flow(
    SqlSchema.findOne({
        Request: documents.fields.state,
        Result: Schema.Struct({ revision: documents.fields.revision, data: Schema.fromJsonString(PortfolioData) }),
        execute: Effect.fnUntraced(function* (selected) {
            const sql = yield* SqlClient.SqlClient;
            const empty = yield* Schema.encodeEffect(Schema.fromJsonString(Portfolio))(Portfolio.make({}));
            return yield* sql`SELECT COALESCE(d.revision, 0) AS revision, json_object('portfolio', json(COALESCE(d.body, ${empty})), 'assets', (
                SELECT json_group_object(a.id, json_set(json_remove(a.body, '$.uploads', '$.objectKey', '$.removing'), '$.id', a.id)) FROM assets a
                WHERE a.${sql('objectKey')} IS NOT NULL AND a.removing IS NOT TRUE
                    AND (s.state = 'draft' OR a.id IN (SELECT asset_id FROM (${references(sql, sql.literal('documents'))}) WHERE state = s.state))
            )) AS data
            FROM (SELECT ${selected} AS state) s LEFT JOIN documents d ON d.state = s.state`;
        }),
    }),
    Effect.catchTag(['SqlError', 'SchemaError', 'NoSuchElementError'], unavailable),
);
const writeDocument = Effect.fn('writeDocument')(
    function* (portfolio: typeof StoredPortfolio.Type, publish: boolean, revision: number) {
        const sql = yield* SqlClient.SqlClient;
        const body = yield* Schema.encodeEffect(documents.fields.body)(portfolio);
        const save = sql`INSERT INTO documents (state, body, revision) SELECT 'draft', ${body}, ${revision + 1} WHERE NOT EXISTS (
                SELECT 1 FROM (${references(sql, sql`(SELECT 'draft' AS state, ${body} AS body)`)}) r
                LEFT JOIN assets a ON a.id = r.asset_id AND a.${sql('objectKey')} IS NOT NULL AND a.removing IS NOT TRUE AND CASE r.placement ->> '$.kind'
                    WHEN 'video' THEN (a.body ->> '$.mime') IN ${sql.in(VideoAsset.fields.mime.literals)}
                    WHEN 'pdf' THEN (a.body ->> '$.mime') IN ${sql.in(PdfAsset.fields.mime.literals)} AND r.placement ->> '$.page' <= json_array_length(a.body, '$.pages')
                    ELSE (a.body ->> '$.mime') IN ${sql.in(ImageAsset.fields.mime.literals)}
                END
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
const readAsset = flow(
    SqlSchema.findOne({
        Request: Schema.Struct({ id: Schema.String, width: Schema.OptionFromNullOr(Schema.Finite) }),
        Result: Schema.Struct({ mime: Schema.String, published: Schema.BooleanFromBit, objectKey: Schema.String }),
        execute: ({ id, width }) =>
            SqlClient.SqlClient.use(
                (sql) => sql`SELECT COALESCE(r.value ->> '$.mime', a.body ->> '$.mime') AS mime, concat_ws('/', a.${sql('objectKey')}, r.value ->> '$.width') AS ${sql('objectKey')},
                    EXISTS (SELECT 1 FROM (${references(sql, sql.literal('documents'))}) WHERE state = 'published' AND asset_id = a.id) AS published
                FROM assets a LEFT JOIN json_each(a.body, '$.renditions') r ON r.value ->> '$.width' = ${width}
                WHERE a.id = ${id} AND a.${sql('objectKey')} IS NOT NULL AND a.removing IS NOT TRUE AND ${width} IS r.value ->> '$.width'`,
            ),
    }),
    Effect.catchTag(['SqlError', 'SchemaError'], unavailable, () => Effect.fail(HttpApiError.NotFound.make())),
);

// --- [OWNER]
const ownerUserId = flow(SqlSchema.findOneOption({ Request: owner.fields.email, Result: owner, execute: (email) => Effect.flatten(owner.select('email', [email])) }), Effect.map(Option.map(Struct.get('userId'))), Effect.catchTag(['SqlError', 'SchemaError'], unavailable));
const registerOwner = Effect.fn('registerOwner')(
    (row: typeof owner.Type) => Effect.flatMap(owner.ensure([row]), Schema.decodeUnknownEffect(Schema.Tuple([owner]))),
    Effect.map(([{ userId }]) => userId),
    Effect.catchTag(['SqlError', 'SchemaError'], unavailable),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { assets, catalog, ownerUserId, readAsset, readDocument, references, registerOwner, writeDocument };
