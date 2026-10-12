import { D1Client } from '@effect/sql-d1';
import { type Cause, Crypto, Effect, flow, Match, Option, type PlatformError, Record, Schema } from 'effect';
import { Hex } from 'effect/encoding';
import { Etag } from 'effect/http';
import { HttpApiError } from 'effect/http-api';
import { findOne, findOneOption } from 'effect/sql/SqlSchema';
import type { Statement } from 'effect/sql/Statement';
import { isPreconditionFailure } from 'partial-content';
import { PreconditionFailed } from '../model/api.ts';
import { ImageAsset, PdfAsset, VideoAsset } from '../model/asset.ts';
import { PortfolioData, type StoredPortfolio } from '../model/document.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const emptyBody = JSON.stringify({ name: '', introduction: '', email: '', entries: [] } satisfies typeof StoredPortfolio.Type);

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [SERVICE_UNAVAILABLE]
const unavailable = (error: Cause.YieldableError): Effect.Effect<never, HttpApiError.ServiceUnavailable> => Effect.andThen(Effect.logError(error), Effect.fail(new HttpApiError.ServiceUnavailable()));

// --- [DOCUMENTS]
const portfolioReferences = (sql: D1Client.D1Client): Statement<{ state: string; assetId: string }> => sql`SELECT state, j.value ->> '$.assetId' AS asset_id
    FROM documents d, json_tree(d.body) j
    WHERE CASE WHEN j.type = 'object' THEN json_type(j.value,'$.assetId') END = 'text'`;
const contentEtag = (body: string): Effect.Effect<string, PlatformError.PlatformError, Crypto.Crypto> => Crypto.Crypto.use((crypto) => crypto.digest('SHA-256', new TextEncoder().encode(body))).pipe(Effect.map((digest) => Etag.toString({ _tag: 'Strong', value: Hex.encode(digest) })));
const readDocument = flow(
    findOne({
        Request: Schema.Literals(['draft', 'published']),
        Result: Schema.Struct({
            data: Schema.Struct({ portfolio: Schema.fromJsonString(Schema.Unknown), assets: Schema.Record(Schema.String, Schema.fromJsonString(Schema.Unknown)) }).pipe(Schema.decodeTo(PortfolioData)),
            body: Schema.String,
        }),
        execute: (selected) =>
            D1Client.D1Client.use((sql) =>
                sql.batch([
                    sql<{ body: string }>`SELECT COALESCE((SELECT body FROM documents WHERE state = ${selected}),${emptyBody}) AS body`,
                    sql<{ id: string; body: string }>`SELECT a.id,json_set(json_remove(a.body,'$.uploads','$.objectKey','$.removing'),'$.id',a.id) AS body FROM assets a
                        WHERE a.body ->> '$.objectKey' IS NOT NULL AND COALESCE(a.body ->> '$.removing',0) = 0 AND (
                            ${selected} = 'draft' OR a.id IN (
                                SELECT asset_id FROM (${portfolioReferences(sql)}) WHERE state = ${selected}
                            )
                        )`,
                ]),
            ).pipe(
                Effect.map(([documents, assets]) => {
                    const available = Record.fromIterableWith(assets, (asset) => [asset.id, asset.body]);
                    return documents.map(({ body }) => ({ data: { portfolio: body, assets: available }, body }));
                }),
            ),
    }),
    Effect.flatMap(({ data, body }) => contentEtag(body).pipe(Effect.map((etag) => ({ data, etag })))),
    Effect.catchTag(['SqlError', 'SchemaError', 'NoSuchElementError', 'PlatformError'], unavailable),
);
const writeDocument = Effect.fn('writeDocument')(
    function* (portfolio: typeof StoredPortfolio.Type, publish: boolean, etag: string) {
        const sql = yield* D1Client.D1Client;
        const previous = yield* findOne({
            Request: Schema.Void,
            Result: Schema.Struct({ body: Schema.String }),
            execute: () => sql`SELECT COALESCE((SELECT body FROM documents WHERE state = 'draft'),${emptyBody}) AS body`,
        })(undefined);
        if (isPreconditionFailure(new Headers({ 'if-match': etag }), yield* contentEtag(previous.body), undefined)) {
            return yield* new PreconditionFailed();
        }
        const body = JSON.stringify(portfolio);
        const digest = yield* contentEtag(body);
        const unchanged = sql`COALESCE((SELECT body FROM documents WHERE state = 'draft'),${emptyBody}) = ${previous.body}`;
        const references = sql`NOT EXISTS (
            SELECT 1 FROM json_tree(${body}) j LEFT JOIN assets a
                ON a.id = CASE WHEN j.type = 'object' THEN j.value ->> '$.assetId' END
            WHERE CASE WHEN j.type = 'object' THEN json_type(j.value, '$.assetId') = 'text' AND (
                a.id IS NULL OR a.body ->> '$.objectKey' IS NULL OR a.body ->> '$.removing' = 1 OR
                CASE
                    WHEN j.key = 'poster' OR j.value ->> '$.kind' = 'image' THEN (a.body ->> '$.mime') NOT IN ${sql.in(ImageAsset.fields.mime.literals)}
                    WHEN j.value ->> '$.kind' = 'video' THEN (a.body ->> '$.mime') NOT IN ${sql.in(VideoAsset.fields.mime.literals)}
                    WHEN j.value ->> '$.kind' = 'pdf' THEN (a.body ->> '$.mime') NOT IN ${sql.in(PdfAsset.fields.mime.literals)} OR NOT EXISTS (
                        SELECT 1 FROM json_each(a.body, '$.pages') WHERE key = (j.value ->> '$.page') - 1
                    )
                END
            ) END
        )`;
        const result = yield* findOne({
            Request: Schema.Void,
            Result: Schema.Struct({ status: Schema.Literals(['saved', 'stale', 'missing']) }),
            execute: () =>
                sql
                    .batch([
                        sql`INSERT INTO documents(state,body) SELECT 'draft',${body} WHERE ${unchanged} AND ${references}
                            ON CONFLICT(state) DO UPDATE SET body = excluded.body`,
                        sql`SELECT CASE WHEN changes() > 0 THEN 'saved' WHEN NOT (${unchanged}) THEN 'stale' ELSE 'missing' END AS status`,
                        sql`INSERT INTO documents(state,body) SELECT 'published',${body} WHERE ${publish} AND changes() > 0
                            ON CONFLICT(state) DO UPDATE SET body = excluded.body`,
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
    findOne({
        Request: Schema.Struct({ id: Schema.String, width: Schema.OptionFromNullOr(Schema.Number) }),
        Result: Schema.Struct({ mime: Schema.String, published: Schema.BooleanFromBit, objectKey: Schema.String }),
        execute: (request) =>
            D1Client.D1Client.use(
                (sql) => sql`
                SELECT CASE WHEN ${request.width} IS NULL THEN (a.body ->> '$.mime') ELSE r.value ->> '$.mime' END AS mime,
                    CASE WHEN ${request.width} IS NULL THEN a.body ->> '$.objectKey' ELSE (a.body ->> '$.objectKey') || '/' || (r.value ->> '$.width') END AS object_key,
                    EXISTS (
                        SELECT 1 FROM (${portfolioReferences(sql)})
                        WHERE state = 'published' AND asset_id = ${request.id}
                    ) AS published
                FROM assets a LEFT JOIN json_each(a.body, '$.renditions') r ON r.value ->> '$.width' = ${request.width}
                WHERE a.id = ${request.id} AND a.body ->> '$.objectKey' IS NOT NULL AND COALESCE(a.body ->> '$.removing',0) = 0
                    AND (${request.width} IS NULL OR r.value IS NOT NULL)
            `,
            ),
    }),
    Effect.catchTag('NoSuchElementError', () => Effect.fail(new HttpApiError.NotFound())),
    Effect.catchTag(['SqlError', 'SchemaError'], unavailable),
);

// --- [OWNER]
const ownerUserId = flow(
    findOneOption({ Request: Schema.String, Result: Schema.Struct({ userId: Schema.String }), execute: (email) => D1Client.D1Client.use((sql) => sql`SELECT user_id FROM owner WHERE email = ${email}`) }),
    Effect.map(Option.map(({ userId }) => userId)),
    Effect.catchTag(['SqlError', 'SchemaError'], unavailable),
);
const registerOwner = flow(
    findOne({
        Request: Schema.Struct({ email: Schema.String, userId: Schema.String }),
        Result: Schema.Struct({ userId: Schema.String }),
        execute: (owner) => D1Client.D1Client.use((sql) => sql`INSERT INTO owner(email,user_id) VALUES (${owner.email},${owner.userId}) ON CONFLICT(email) DO UPDATE SET user_id = owner.user_id RETURNING user_id`),
    }),
    Effect.map(({ userId }) => userId),
    Effect.catchTag(['SqlError', 'SchemaError', 'NoSuchElementError'], unavailable),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { ownerUserId, portfolioReferences, readAsset, readDocument, registerOwner, unavailable, writeDocument };
