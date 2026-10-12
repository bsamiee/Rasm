import { D1Client } from '@effect/sql-d1';
import { Effect, flow, Schema } from 'effect';
import { HttpApiError } from 'effect/http-api';
import { findAll, findOne } from 'effect/sql/SqlSchema';
import { PendingUpload } from '../model/api.ts';
import type { Asset } from '../model/asset.ts';
import { portfolioReferences, unavailable } from './database.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [UPLOADS]
const beginUpload = Effect.fn('beginUpload')(
    function* (asset: typeof Asset.Type, attemptId: string, keys: readonly string[]) {
        const sql = yield* D1Client.D1Client;
        const { id, ...metadata } = asset;
        const attempt = JSON.stringify({ id: attemptId, discard: false, objects: keys.map((key) => ({ key })) });
        return yield* findOne({
            Request: Schema.Void,
            Result: Schema.Struct({ objectKey: Schema.OptionFromNullOr(Schema.String) }),
            execute: () => sql`INSERT INTO assets(id,body) VALUES (${id},json_set(${JSON.stringify(metadata)}, '$.uploads', json_array(json(${attempt}))))
                ON CONFLICT(id) DO UPDATE SET body = CASE WHEN assets.body ->> '$.objectKey' IS NULL
                    THEN json_insert(assets.body, '$.uploads[#]', json(${attempt})) ELSE assets.body END
                WHERE json_remove(assets.body,'$.uploads','$.objectKey','$.removing') = json_remove(excluded.body,'$.uploads')
                    AND COALESCE(assets.body ->> '$.removing',0) = 0
                RETURNING body ->> '$.objectKey' AS object_key`,
        })(undefined).pipe(Effect.map(({ objectKey }) => objectKey));
    },
    Effect.catchTag('NoSuchElementError', () => Effect.fail(new HttpApiError.Conflict())),
    Effect.catchTag(['SqlError', 'SchemaError'], unavailable),
);
const attachUpload = flow(
    findOne({
        Request: Schema.Struct({ key: Schema.String, uploadId: Schema.String }),
        Result: Schema.Struct({ id: Schema.String }),
        execute: (object) =>
            D1Client.D1Client.use(
                (sql) => sql`UPDATE assets SET body = json_set(body, (
            SELECT u.fullkey || '.objects[' || o.key || '].uploadId'
            FROM json_each(body,'$.uploads') u, json_each(u.value,'$.objects') o
            WHERE o.value ->> '$.key' = ${object.key}
        ), ${object.uploadId}) WHERE EXISTS (
            SELECT 1 FROM json_each(body,'$.uploads') u, json_each(u.value,'$.objects') o
            WHERE u.value ->> '$.discard' = 0 AND o.value ->> '$.key' = ${object.key}
        ) RETURNING id`,
            ),
    }),
    Effect.asVoid,
    Effect.catchTag('NoSuchElementError', () => Effect.fail(new HttpApiError.Conflict())),
    Effect.catchTag(['SqlError', 'SchemaError'], unavailable),
);
const completeUpload = flow(
    findOne({
        Request: Schema.String,
        Result: Schema.Struct({ id: Schema.String }),
        execute: (attempt) =>
            D1Client.D1Client.use(
                (sql) => sql`UPDATE assets
            SET body = json_set(body,'$.objectKey',COALESCE(body ->> '$.objectKey',${attempt}))
            WHERE COALESCE(body ->> '$.removing',0) = 0 AND EXISTS (
                SELECT 1 FROM json_each(body,'$.uploads') u WHERE u.value ->> '$.id' = ${attempt}
                    AND (u.value ->> '$.discard' = 0 OR body ->> '$.objectKey' IS NOT NULL)
            ) RETURNING id`,
            ),
    }),
    Effect.asVoid,
    Effect.catchTag('NoSuchElementError', () => Effect.fail(new HttpApiError.Conflict())),
    Effect.catchTag(['SqlError', 'SchemaError'], unavailable),
);

// --- [REMOVALS]
const releaseUploads = Effect.fn('releaseUploads')(
    function* (target: { readonly assetId: string; readonly remove: boolean } | { readonly attemptId: string }) {
        const sql = yield* D1Client.D1Client;
        const source = 'assetId' in target ? sql`assets.id = ${target.assetId}` : sql`EXISTS (SELECT 1 FROM json_each(body,'$.uploads') WHERE value ->> '$.id' = ${target.attemptId})`;
        const attempts = 'attemptId' in target ? sql`u.value ->> '$.id' = ${target.attemptId}` : sql`TRUE`;
        const remove = 'assetId' in target && target.remove;
        return yield* findOne({
            Request: Schema.Void,
            Result: Schema.Struct({
                conflict: Schema.BooleanFromBit,
                objects: Schema.Array(Schema.Struct({ key: Schema.String, uploadId: Schema.OptionFromNullOr(Schema.String) })),
            }),
            execute: () =>
                sql
                    .batch([
                        sql`UPDATE assets SET body = json_set(body,'$.removing',json('true')) WHERE ${source} AND ${remove}
                    AND NOT EXISTS (SELECT 1 FROM (${portfolioReferences(sql)}) WHERE asset_id = assets.id)`,
                        sql`UPDATE assets SET body = json_set(body,'$.uploads',json((
                    SELECT json_group_array(json(CASE WHEN ${attempts} AND (
                        body ->> '$.removing' = 1 OR (${!remove} AND (body ->> '$.objectKey' IS NULL OR body ->> '$.objectKey' <> u.value ->> '$.id'))
                    ) THEN json_set(u.value,'$.discard',json('true')) ELSE u.value END)) FROM json_each(body,'$.uploads') u
                ))) WHERE ${source}`,
                        sql`SELECT EXISTS (SELECT 1 FROM assets WHERE ${source} AND ${remove} AND COALESCE(body ->> '$.removing',0) = 0) AS conflict`,
                        sql`SELECT o.value ->> '$.key' AS key, o.value ->> '$.uploadId' AS upload_id
                    FROM assets, json_each(body,'$.uploads') u, json_each(u.value,'$.objects') o
                    WHERE ${source} AND ${attempts} AND u.value ->> '$.discard' = 1`,
                    ])
                    .pipe(Effect.map(([, , statuses, objects]) => statuses.map((status) => ({ ...status, objects })))),
        })(undefined).pipe(
            Effect.filterOrFail(
                ({ conflict }) => !conflict,
                () => new HttpApiError.Conflict(),
            ),
            Effect.map(({ objects }) => objects),
        );
    },
    Effect.catchTag(['SqlError', 'SchemaError', 'NoSuchElementError'], unavailable),
);
const finishRelease = Effect.fn('finishRelease')(
    (key: string) =>
        D1Client.D1Client.use((sql) =>
            sql.batch([
                sql`UPDATE assets SET body = json_set(body,'$.uploads',json((
            SELECT json_group_array(json(CASE WHEN u.value ->> '$.discard' = 1
                THEN json_set(u.value,'$.objects',json((SELECT json_group_array(json(value)) FROM json_each(u.value,'$.objects') WHERE value ->> '$.key' <> ${key})))
                ELSE u.value END)) FROM json_each(body,'$.uploads') u
        ))) WHERE EXISTS (SELECT 1 FROM json_each(body,'$.uploads') u, json_each(u.value,'$.objects') o WHERE o.value ->> '$.key' = ${key})`,
                sql`UPDATE assets SET body = json_set(body,'$.uploads',json((
            SELECT json_group_array(json(value)) FROM json_each(body,'$.uploads')
            WHERE value ->> '$.discard' = 0 OR json_array_length(value,'$.objects') > 0
        ))) WHERE EXISTS (SELECT 1 FROM json_each(body,'$.uploads') WHERE value ->> '$.discard' = 1 AND json_array_length(value,'$.objects') = 0)`,
                sql`DELETE FROM assets WHERE (body ->> '$.objectKey' IS NULL OR body ->> '$.removing' = 1) AND json_array_length(body,'$.uploads') = 0`,
            ]),
        ),
    (effect) => effect.pipe(Effect.asVoid, Effect.catchTag('SqlError', unavailable)),
);
const readPendingUploads = flow(
    findAll({
        Request: Schema.Void,
        Result: PendingUpload,
        execute: () =>
            D1Client.D1Client.use(
                (sql) => sql`SELECT id,body ->> '$.name' AS name,
            CASE WHEN body ->> '$.removing' = 1 THEN 'removing' WHEN body ->> '$.objectKey' IS NULL THEN 'incomplete' ELSE 'cleanup' END AS status
            FROM assets WHERE body ->> '$.removing' = 1 OR body ->> '$.objectKey' IS NULL
                OR EXISTS (SELECT 1 FROM json_each(body,'$.uploads') WHERE value ->> '$.id' <> body ->> '$.objectKey')`,
            ),
    }),
    Effect.catchTag(['SqlError', 'SchemaError'], unavailable),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { attachUpload, beginUpload, completeUpload, finishRelease, readPendingUploads, releaseUploads };
