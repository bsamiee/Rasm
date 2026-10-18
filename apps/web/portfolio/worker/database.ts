import { table } from '@rasm/database/catalog.ts';
import { atomic, select } from '@rasm/database/store.ts';
import { unavailable } from '@rasm/hosting/http.ts';
import { Array, Effect, flow, Match, Option, Schema, Struct } from 'effect';
import { HttpApiError } from 'effect/http-api';
import { SqlClient, SqlSchema } from 'effect/sql';
import { PreconditionFailed } from '../model/api.ts';
import { ImageAsset, PdfAsset, VideoAsset } from '../model/asset.ts';
import { Portfolio, PortfolioData, StoredPortfolio } from '../model/document.ts';

// --- [MODELS] --------------------------------------------------------------------------

const documents = table('documents', { state: Schema.Literals(['draft', 'published']), body: Schema.fromJsonString(StoredPortfolio), revision: Schema.Int.check(Schema.isGreaterThanOrEqualTo(0)) }, { key: ['state'], defaults: { revision: 0 } });
const assets = table(
    'assets',
    { id: Schema.String, body: Schema.fromJsonString(Schema.Unknown), objectKey: Schema.NullOr(Schema.String), removing: Schema.NullOr(Schema.BooleanFromBit), uploads: Schema.NullOr(Schema.fromJsonString(Schema.Unknown)) },
    { key: ['id'], projections: { objectKey: { from: 'body', path: ['objectKey'] }, removing: { from: 'body', path: ['removing'] }, uploads: { from: 'body', path: ['uploads'] } } },
);
const owner = table('owner', { email: Schema.String, userId: Schema.String }, { key: ['email'], withoutRowid: true });
const catalog = { tables: [documents, assets, owner], retired: [] } as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [DOCUMENTS]
const portfolioReferences = (sql: SqlClient.SqlClient) => sql`SELECT state, j.value ->> '$.assetId' AS asset_id
    FROM documents d, json_tree(d.body) j
    WHERE CASE WHEN j.type = 'object' THEN json_type(j.value,'$.assetId') END = 'text'`;
const readDocument = flow(
    SqlSchema.findOne({
        Request: documents.fields.state,
        Result: Schema.Struct({
            revision: documents.fields.revision,
            data: Schema.fromJsonString(PortfolioData),
        }),
        execute: Effect.fnUntraced(function* (selected) {
            const sql = yield* SqlClient.SqlClient;
            const empty = yield* Schema.encodeEffect(Schema.fromJsonString(Portfolio))(Portfolio.make({}));
            return yield* sql`SELECT COALESCE(d.revision, 0) AS revision, json_object('portfolio', json(COALESCE(d.body, ${empty})), 'assets', (
                SELECT json_group_object(a.id, json(json_set(json_remove(a.body,'$.uploads','$.objectKey','$.removing'),'$.id',a.id))) FROM assets a
                WHERE a.${sql('objectKey')} IS NOT NULL AND a.removing IS NOT TRUE AND (
                    s.state = 'draft' OR a.id IN (SELECT asset_id FROM (${portfolioReferences(sql)}) WHERE state = s.state)
                )
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
        const references = sql`NOT EXISTS (
            SELECT 1 FROM json_tree(${body}) j LEFT JOIN assets a
                ON a.id = CASE WHEN j.type = 'object' THEN j.value ->> '$.assetId' END
            WHERE CASE WHEN j.type = 'object' THEN json_type(j.value, '$.assetId') = 'text' AND (
                a.id IS NULL OR a.${sql('objectKey')} IS NULL OR a.removing IS TRUE OR
                CASE
                    WHEN j.key = 'poster' OR j.value ->> '$.kind' = 'image' THEN (a.body ->> '$.mime') NOT IN ${sql.in(ImageAsset.fields.mime.literals)}
                    WHEN j.value ->> '$.kind' = 'video' THEN (a.body ->> '$.mime') NOT IN ${sql.in(VideoAsset.fields.mime.literals)}
                    WHEN j.value ->> '$.kind' = 'pdf' THEN (a.body ->> '$.mime') NOT IN ${sql.in(PdfAsset.fields.mime.literals)} OR NOT EXISTS (
                        SELECT 1 FROM json_each(a.body, '$.pages') WHERE key = (j.value ->> '$.page') - 1
                    )
                END
            ) END
        )`;
        const save = sql`INSERT INTO documents (state, body, revision) SELECT 'draft', ${body}, ${revision + 1} WHERE ${references}
            ON CONFLICT (state) DO UPDATE SET body = excluded.body, revision = excluded.revision
            WHERE documents.revision = ${revision} AND documents.body IS DISTINCT FROM excluded.body
            RETURNING revision`;
        const state = sql`SELECT COALESCE(MAX(revision), 0) AS revision, COALESCE(MAX(body = ${body}), 0) AS unchanged FROM documents WHERE state = 'draft'`;
        const published = sql`INSERT INTO documents (state, body, revision) SELECT state, body, revision FROM (SELECT 'published' AS state, body, revision FROM documents WHERE state = 'draft' AND body = ${body} AND revision IN (${revision}, ${revision + 1})) WHERE true
            ON CONFLICT (state) DO UPDATE SET body = excluded.body, revision = excluded.revision
            WHERE (documents.body, documents.revision) IS DISTINCT FROM (excluded.body, excluded.revision)`;
        const [saved, [current]] = yield* Effect.flatMap(
            atomic([save, state, ...(publish ? [published] : [])]),
            Schema.decodeUnknownEffect(Schema.TupleWithRest(Schema.Tuple([Schema.Array(documents.mapFields(Struct.pick(['revision']))), Schema.Tuple([Schema.Struct({ revision: documents.fields.revision, unchanged: Schema.BooleanFromBit })])]), [Schema.Unknown])),
        );
        return yield* Match.value({ saved: Array.head(saved), ...current }).pipe(
            Match.when({ saved: Option.isSome }, ({ saved: row }) => Effect.succeed(row.value.revision)),
            Match.when({ revision: (stored: number) => stored !== revision }, () => Effect.fail(PreconditionFailed.make())),
            Match.when({ unchanged: true }, (row) => Effect.succeed(row.revision)),
            Match.orElse(() => Effect.fail(HttpApiError.Conflict.make())),
        );
    },
    Effect.catchTag(['SqlError', 'SchemaError'], unavailable),
);

// --- [ASSETS]
const readAsset = flow(
    SqlSchema.findOne({
        Request: Schema.Struct({ id: Schema.String, width: Schema.OptionFromNullOr(Schema.Finite) }),
        Result: Schema.Struct({ mime: Schema.String, published: Schema.BooleanFromBit, objectKey: Schema.String }),
        execute: (request) =>
            SqlClient.SqlClient.use(
                (sql) => sql`
                SELECT CASE WHEN ${request.width} IS NULL THEN (a.body ->> '$.mime') ELSE r.value ->> '$.mime' END AS mime,
                    CASE WHEN ${request.width} IS NULL THEN a.${sql('objectKey')} ELSE a.${sql('objectKey')} || '/' || (r.value ->> '$.width') END AS ${sql('objectKey')},
                    EXISTS (
                        SELECT 1 FROM (${portfolioReferences(sql)})
                        WHERE state = 'published' AND asset_id = ${request.id}
                    ) AS published
                FROM assets a LEFT JOIN json_each(a.body, '$.renditions') r ON r.value ->> '$.width' = ${request.width}
                WHERE a.id = ${request.id} AND a.${sql('objectKey')} IS NOT NULL AND a.removing IS NOT TRUE
                    AND (${request.width} IS NULL OR r.value IS NOT NULL)
            `,
            ),
    }),
    Effect.catchTag('NoSuchElementError', () => Effect.fail(HttpApiError.NotFound.make())),
    Effect.catchTag(['SqlError', 'SchemaError'], unavailable),
);

// --- [OWNER]
const ownerUserId = flow(SqlSchema.findOneOption({ Request: owner.fields.email, Result: owner, execute: (email) => Effect.flatten(select(owner, 'email', [email])) }), Effect.map(Option.map(({ userId }) => userId)), Effect.catchTag(['SqlError', 'SchemaError'], unavailable));
const registerOwner = Effect.fn('registerOwner')(
    function* (row: typeof owner.Type) {
        const sql = yield* SqlClient.SqlClient;
        const [, [stored]] = yield* Effect.flatMap(atomic([sql`INSERT INTO owner ${sql.insert(row)} ON CONFLICT (email) DO NOTHING`, yield* select(owner, 'email', [row.email])]), Schema.decodeUnknownEffect(Schema.Tuple([Schema.Tuple([]), Schema.Tuple([owner])])));
        return stored.userId;
    },
    Effect.catchTag(['SqlError', 'SchemaError'], unavailable),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { catalog, ownerUserId, portfolioReferences, readAsset, readDocument, registerOwner, writeDocument };
