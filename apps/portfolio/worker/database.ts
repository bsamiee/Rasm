import { D1Client } from '@effect/sql-d1';
import { type Cause, Effect, flow, Layer, Match, Option, Record, Schema } from 'effect';
import { HttpApiError } from 'effect/http-api';
import { SqlSchema } from 'effect/sql';
import { isPreconditionFailure } from 'partial-content';
import { contentEtag } from '../db/digest.ts';
import { schema } from '../db/schema.ts';
import { PreconditionFailed } from '../model/api.ts';
import { ImageAsset, PdfAsset, VideoAsset } from '../model/asset.ts';
import { emptyPortfolio, PortfolioData, type StoredPortfolio } from '../model/document.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [SERVICE_UNAVAILABLE]
const unavailable = (error: Cause.YieldableError): Effect.Effect<never, HttpApiError.ServiceUnavailable> => Effect.andThen(Effect.logError(error), Effect.fail(new HttpApiError.ServiceUnavailable()));

// --- [DOCUMENTS]
const readDocument = flow(
    SqlSchema.findOne({
        Request: Schema.Literals(['draft', 'published']),
        Result: Schema.Struct({
            data: Schema.Struct({ portfolio: Schema.fromJsonString(Schema.Unknown), assets: Schema.Record(Schema.String, Schema.fromJsonString(Schema.Unknown)) }).pipe(Schema.decodeTo(PortfolioData)),
            etag: Schema.String,
        }),
        execute: (selected) =>
            D1Client.D1Client.use((sql) =>
                sql.batch([
                    sql<{ body: string; digest: string }>`SELECT body,digest FROM portfolios JOIN documents USING (digest) WHERE state = ${selected}`,
                    sql<{ id: string; body: string }>`SELECT a.id,a.body FROM assets a
                        WHERE a.object_key IS NOT NULL AND a.removing = 0 AND (
                            ${selected} = 'draft' OR a.id IN (
                                SELECT asset_id FROM portfolio_references JOIN documents USING (digest) WHERE state = ${selected}
                            )
                        )`,
                ]),
            ).pipe(
                Effect.map(([documents, assets]) => {
                    const available = Record.fromIterableWith(assets, (asset) => [asset.id, asset.body]);
                    return documents.map(({ body, digest }) => ({ data: { portfolio: body, assets: available }, etag: digest }));
                }),
            ),
    }),
    Effect.catchTag(['SqlError', 'SchemaError', 'NoSuchElementError'], unavailable),
);
const writeDocument = Effect.fn('writeDocument')(
    function* (portfolio: typeof StoredPortfolio.Type, publish: boolean, etag: string) {
        const sql = yield* D1Client.D1Client;
        const previous = yield* SqlSchema.findOne({
            Request: Schema.Void,
            Result: Schema.Struct({ digest: Schema.String }),
            execute: () => sql`SELECT digest FROM documents WHERE state = 'draft'`,
        })(undefined);
        if (isPreconditionFailure(new Headers({ 'if-match': etag }), previous.digest, undefined)) {
            return yield* new PreconditionFailed();
        }
        const body = JSON.stringify(portfolio);
        const digest = yield* contentEtag(body);
        const unchanged = sql`(SELECT digest FROM documents WHERE state = 'draft') = ${previous.digest}`;
        const references = sql`NOT EXISTS (
            SELECT 1 FROM json_tree(${body}) j LEFT JOIN assets a
                ON a.id = CASE WHEN j.type = 'object' THEN j.value ->> '$.assetId' END
            WHERE CASE WHEN j.type = 'object' THEN json_type(j.value, '$.assetId') = 'text' AND (
                a.id IS NULL OR a.object_key IS NULL OR a.removing <> 0 OR
                CASE
                    WHEN j.key = 'poster' OR j.value ->> '$.kind' = 'image' THEN a.mime NOT IN ${sql.in(ImageAsset.fields.mime.literals)}
                    WHEN j.value ->> '$.kind' = 'video' THEN a.mime NOT IN ${sql.in(VideoAsset.fields.mime.literals)}
                    WHEN j.value ->> '$.kind' = 'pdf' THEN a.mime NOT IN ${sql.in(PdfAsset.fields.mime.literals)} OR NOT EXISTS (
                        SELECT 1 FROM json_each(a.body, '$.pages') WHERE key = (j.value ->> '$.page') - 1
                    )
                END
            ) END
        )`;
        const result = yield* SqlSchema.findOne({
            Request: Schema.Void,
            Result: Schema.Struct({ status: Schema.Literals(['saved', 'stale', 'missing']) }),
            execute: () =>
                sql
                    .batch([
                        sql`INSERT INTO portfolios(digest,body) SELECT ${digest},${body} WHERE ${unchanged} AND ${references}
                    ON CONFLICT(digest) DO UPDATE SET body = excluded.body`,
                        sql`SELECT CASE WHEN changes() > 0 THEN 'saved' WHEN NOT (${unchanged}) THEN 'stale' ELSE 'missing' END AS status`,
                        sql`UPDATE documents SET digest = ${digest} WHERE (state = 'draft' OR ${publish}) AND changes() > 0`,
                        sql`DELETE FROM portfolios WHERE changes() > 0 AND digest NOT IN (SELECT digest FROM documents)`,
                    ])
                    .pipe(Effect.map(([, status]) => status)),
        })(undefined);
        return yield* Match.value(result.status).pipe(
            Match.when('saved', () => Effect.succeed(digest)),
            Match.when('stale', () => Effect.fail(new PreconditionFailed())),
            Match.when('missing', () => Effect.fail(new HttpApiError.Conflict())),
            Match.exhaustive,
        );
    },
    Effect.catchTag(['SqlError', 'SchemaError', 'NoSuchElementError', 'PlatformError'], unavailable),
);

// --- [ASSETS]
const readAsset = flow(
    SqlSchema.findOne({
        Request: Schema.Struct({ id: Schema.String, width: Schema.OptionFromNullOr(Schema.Number) }),
        Result: Schema.Struct({ mime: Schema.String, published: Schema.BooleanFromBit, objectKey: Schema.String }),
        execute: (request) =>
            D1Client.D1Client.use(
                (sql) => sql`
                SELECT CASE WHEN ${request.width} IS NULL THEN a.mime ELSE r.value ->> '$.mime' END AS mime,
                    CASE WHEN ${request.width} IS NULL THEN a.object_key ELSE a.object_key || '/' || (r.value ->> '$.width') END AS object_key,
                    EXISTS (
                        SELECT 1 FROM portfolio_references JOIN documents USING (digest)
                        WHERE state = 'published' AND asset_id = ${request.id}
                    ) AS published
                FROM assets a LEFT JOIN json_each(a.body, '$.renditions') r ON r.value ->> '$.width' = ${request.width}
                WHERE a.id = ${request.id} AND a.object_key IS NOT NULL AND a.removing = 0
                    AND (${request.width} IS NULL OR r.value IS NOT NULL)
            `,
            ),
    }),
    Effect.catchTag('NoSuchElementError', () => Effect.fail(new HttpApiError.NotFound())),
    Effect.catchTag(['SqlError', 'SchemaError'], unavailable),
);

// --- [OWNER]
const ownerUserId = flow(
    SqlSchema.findOneOption({ Request: Schema.String, Result: Schema.Struct({ userId: Schema.String }), execute: (email) => D1Client.D1Client.use((sql) => sql`SELECT user_id FROM owner WHERE email = ${email}`) }),
    Effect.map(Option.map(({ userId }) => userId)),
    Effect.catchTag(['SqlError', 'SchemaError'], unavailable),
);
const registerOwner = flow(
    SqlSchema.findOne({
        Request: Schema.Struct({ email: Schema.String, userId: Schema.String }),
        Result: Schema.Struct({ userId: Schema.String }),
        execute: (owner) => D1Client.D1Client.use((sql) => sql`INSERT INTO owner(email,user_id) VALUES (${owner.email},${owner.userId}) ON CONFLICT(email) DO UPDATE SET user_id = owner.user_id RETURNING user_id`),
    }),
    Effect.map(({ userId }) => userId),
    Effect.catchTag(['SqlError', 'SchemaError', 'NoSuchElementError'], unavailable),
);

// --- [COMPOSITION] ---------------------------------------------------------------------

const database = Layer.effectDiscard(
    Effect.gen(function* () {
        const sql = yield* D1Client.D1Client;
        const body = JSON.stringify(emptyPortfolio);
        const digest = yield* contentEtag(body);
        yield* sql.batch([
            ...schema(sql),
            sql`INSERT INTO portfolios(digest,body) SELECT ${digest},${body}
                WHERE NOT EXISTS (SELECT 1 FROM documents WHERE state = 'draft')
                    OR NOT EXISTS (SELECT 1 FROM documents WHERE state = 'published')
                ON CONFLICT(digest) DO NOTHING`,
            sql`INSERT INTO documents(state,digest) VALUES ('draft',${digest}),('published',${digest})
                ON CONFLICT(state) DO NOTHING`,
        ]);
    }).pipe(Effect.catchTag('PlatformError', unavailable)),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { database, ownerUserId, readAsset, readDocument, registerOwner, unavailable, writeDocument };
