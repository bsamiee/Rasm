import { atomic } from '@rasm/database/store.ts';
import { unavailable } from '@rasm/hosting/http.ts';
import { Effect, flow, Schema } from 'effect';
import { HttpApiError } from 'effect/http-api';
import { SqlClient, SqlSchema } from 'effect/sql';
import { PendingUpload } from '../model/api.ts';
import type { Asset } from '../model/asset.ts';
import { portfolioReferences } from './database.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [UPLOADS]
const beginUpload = Effect.fn('beginUpload')(
    function* (asset: typeof Asset.Type, attemptId: string, keys: readonly string[]) {
        const sql = yield* SqlClient.SqlClient;
        const { id, ...metadata } = asset;
        const json = Schema.encodeEffect(Schema.fromJsonString(Schema.Unknown));
        const attempt = yield* json({ id: attemptId, discard: false, objects: keys.map((key) => ({ key })) });
        const body = yield* json(metadata);
        return yield* SqlSchema.findOne({
            Request: Schema.Void,
            Result: Schema.Struct({ objectKey: Schema.OptionFromNullOr(Schema.String) }),
            execute: () => sql`INSERT INTO assets(id,body) VALUES (${id},json_set(${body}, '$.uploads', json_array(json(${attempt}))))
                ON CONFLICT(id) DO UPDATE SET body = CASE WHEN assets.${sql('objectKey')} IS NULL
                    THEN json_insert(assets.body, '$.uploads[#]', json(${attempt})) ELSE assets.body END
                WHERE json_remove(assets.body,'$.uploads','$.objectKey','$.removing') = json_remove(excluded.body,'$.uploads')
                    AND assets.removing IS NOT TRUE
                RETURNING ${sql('objectKey')}`,
        })(undefined).pipe(Effect.map(({ objectKey }) => objectKey));
    },
    Effect.catchTag('NoSuchElementError', () => Effect.fail(HttpApiError.Conflict.make())),
    Effect.catchTag(['SqlError', 'SchemaError'], unavailable),
);
const attachUpload = flow(
    SqlSchema.findOne({
        Request: Schema.Struct({ key: Schema.String, uploadId: Schema.String }),
        Result: Schema.Struct({ id: Schema.String }),
        execute: (object) =>
            SqlClient.SqlClient.use(
                (sql) => sql`UPDATE assets SET body = json_set(body, (
            SELECT u.fullkey || '.objects[' || o.key || '].uploadId'
            FROM json_each(body,'$.uploads') u, json_each(u.value,'$.objects') o
            WHERE o.value ->> '$.key' = ${object.key}
        ), ${object.uploadId}) WHERE EXISTS (
            SELECT 1 FROM json_each(uploads) u, json_each(u.value,'$.objects') o
            WHERE u.value ->> '$.discard' = 0 AND o.value ->> '$.key' = ${object.key}
        ) RETURNING id`,
            ),
    }),
    Effect.asVoid,
    Effect.catchTag('NoSuchElementError', () => Effect.fail(HttpApiError.Conflict.make())),
    Effect.catchTag(['SqlError', 'SchemaError'], unavailable),
);
const completeUpload = flow(
    SqlSchema.findOne({
        Request: Schema.String,
        Result: Schema.Struct({ id: Schema.String }),
        execute: (attempt) =>
            SqlClient.SqlClient.use(
                (sql) => sql`UPDATE assets
            SET body = json_set(body,'$.objectKey',COALESCE(${sql('objectKey')},${attempt}))
            WHERE removing IS NOT TRUE AND EXISTS (
                SELECT 1 FROM json_each(uploads) u WHERE u.value ->> '$.id' = ${attempt}
                    AND (u.value ->> '$.discard' = 0 OR ${sql('objectKey')} IS NOT NULL)
            ) RETURNING id`,
            ),
    }),
    Effect.asVoid,
    Effect.catchTag('NoSuchElementError', () => Effect.fail(HttpApiError.Conflict.make())),
    Effect.catchTag(['SqlError', 'SchemaError'], unavailable),
);

// --- [REMOVALS]
const releaseUploads = Effect.fn('releaseUploads')(
    function* (target: { readonly assetId: string; readonly remove: boolean } | { readonly attemptId: string }) {
        const sql = yield* SqlClient.SqlClient;
        const source = 'assetId' in target ? sql`assets.id = ${target.assetId}` : sql`EXISTS (SELECT 1 FROM json_each(uploads) WHERE value ->> '$.id' = ${target.attemptId})`;
        const objectKey = sql('objectKey');
        const attempts = 'attemptId' in target ? sql`u.value ->> '$.id' = ${target.attemptId}` : sql`TRUE`;
        const remove = 'assetId' in target && target.remove ? sql`TRUE` : sql`FALSE`;
        const [, , [{ conflict, objects }]] = yield* Effect.flatMap(
            atomic([
                sql`UPDATE assets SET body = json_set(body,'$.removing',json('true')) WHERE ${source} AND ${remove}
                AND NOT EXISTS (SELECT 1 FROM (${portfolioReferences(sql)}) WHERE asset_id = assets.id)`,
                sql`UPDATE assets SET body = json_set(body,'$.uploads',json((
                SELECT json_group_array(json(CASE WHEN ${attempts} AND (
                    removing IS TRUE OR (NOT ${remove} AND (${objectKey} IS NULL OR ${objectKey} <> u.value ->> '$.id'))
                ) THEN json_set(u.value,'$.discard',json('true')) ELSE u.value END)) FROM json_each(uploads) u
            ))) WHERE ${source}`,
                sql`SELECT EXISTS (SELECT 1 FROM assets WHERE ${source} AND ${remove} AND removing IS NOT TRUE) AS conflict,
                (SELECT json_group_array(json_object('key', o.value ->> '$.key', 'uploadId', o.value ->> '$.uploadId'))
                    FROM assets, json_each(uploads) u, json_each(u.value,'$.objects') o
                    WHERE ${source} AND ${attempts} AND u.value ->> '$.discard' = 1) AS objects`,
            ]),
            Schema.decodeUnknownEffect(Schema.Tuple([Schema.Unknown, Schema.Unknown, Schema.Tuple([Schema.Struct({ conflict: Schema.BooleanFromBit, objects: Schema.fromJsonString(Schema.Array(Schema.Struct({ key: Schema.String, uploadId: Schema.OptionFromNullOr(Schema.String) }))) })])])),
        );
        return yield* conflict ? Effect.fail(HttpApiError.Conflict.make()) : Effect.succeed(objects);
    },
    Effect.catchTag(['SqlError', 'SchemaError'], unavailable),
);
const finishRelease = Effect.fn('finishRelease')(
    (key: string) =>
        SqlClient.SqlClient.use((sql) =>
            atomic([
                sql`UPDATE assets SET body = json_set(body,'$.uploads',json((
            SELECT json_group_array(json(CASE WHEN u.value ->> '$.discard' = 1
                THEN json_set(u.value,'$.objects',json((SELECT json_group_array(json(value)) FROM json_each(u.value,'$.objects') WHERE value ->> '$.key' <> ${key})))
                ELSE u.value END)) FROM json_each(uploads) u
        ))) WHERE EXISTS (SELECT 1 FROM json_each(uploads) u, json_each(u.value,'$.objects') o WHERE o.value ->> '$.key' = ${key})`,
                sql`UPDATE assets SET body = json_set(body,'$.uploads',json((
            SELECT json_group_array(json(value)) FROM json_each(uploads)
            WHERE value ->> '$.discard' = 0 OR json_array_length(value,'$.objects') > 0
        ))) WHERE EXISTS (SELECT 1 FROM json_each(uploads) WHERE value ->> '$.discard' = 1 AND json_array_length(value,'$.objects') = 0)`,
                sql`DELETE FROM assets WHERE (${sql('objectKey')} IS NULL OR removing IS TRUE) AND json_array_length(uploads) = 0`,
            ]),
        ),
    (effect) => effect.pipe(Effect.asVoid, Effect.catchTag('SqlError', unavailable)),
);
const readPendingUploads = flow(
    SqlSchema.findAll({
        Request: Schema.Void,
        Result: PendingUpload,
        execute: () =>
            SqlClient.SqlClient.use(
                (sql) => sql`SELECT id,body ->> '$.name' AS name,
            CASE WHEN removing IS TRUE THEN 'removing' WHEN ${sql('objectKey')} IS NULL THEN 'incomplete' ELSE 'cleanup' END AS status
            FROM assets WHERE removing IS TRUE OR ${sql('objectKey')} IS NULL
                OR EXISTS (SELECT 1 FROM json_each(uploads) WHERE value ->> '$.id' <> ${sql('objectKey')})`,
            ),
    }),
    Effect.catchTag(['SqlError', 'SchemaError'], unavailable),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { attachUpload, beginUpload, completeUpload, finishRelease, readPendingUploads, releaseUploads };
