import { atomic } from '@rasm/database/table.ts';
import { unavailable } from '@rasm/hosting/http.ts';
import { Array, Effect, Schema, Struct } from 'effect';
import { HttpApiError } from 'effect/http-api';
import { SqlClient, SqlSchema } from 'effect/sql';
import { PendingUpload } from '../model/api.ts';
import type { Asset } from '../model/asset.ts';
import { assets, references } from './database.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [UPLOADS]
const beginUpload = Effect.fn('beginUpload')(
    ({ id, ...metadata }: typeof Asset.Type, attemptId: string, keys: readonly string[]) =>
        SqlSchema.findOne({
            Request: assets.mapFields(Struct.pick(['id', 'body'])),
            Result: assets.mapFields(Struct.pick(['objectKey'])),
            execute: (row) =>
                SqlClient.SqlClient.use(
                    (sql) => sql`INSERT INTO assets (id, body) VALUES (${row.id}, ${row.body})
                    ON CONFLICT (id) DO UPDATE SET body = CASE WHEN assets.${sql('objectKey')} IS NULL THEN json_insert(assets.body, '$.uploads[#]', excluded.body -> '$.uploads[0]') ELSE assets.body END
                    WHERE json_remove(assets.body, '$.uploads', '$.objectKey', '$.removing') = json_remove(excluded.body, '$.uploads') AND assets.removing IS NOT TRUE
                    RETURNING ${sql('objectKey')}`,
                ),
        })({ id, body: { ...metadata, uploads: [{ id: attemptId, discard: false, objects: keys.map((key) => ({ key })) }] } }),
    Effect.map(Struct.get('objectKey')),
    Effect.catchTag(['SqlError', 'SchemaError'], unavailable, () => Effect.fail(HttpApiError.Conflict.make())),
);
const attachUpload = Effect.fn('attachUpload')(
    (key: string, uploadId: string) =>
        SqlClient.SqlClient.use(
            (sql) => sql`UPDATE assets SET body = json_set(body, (
                SELECT o.fullkey || '.uploadId' FROM json_each(body, '$.uploads') u, json_each(body, u.fullkey || '.objects') o WHERE o.value ->> '$.key' = ${key}
            ), ${uploadId}) WHERE EXISTS (
                SELECT 1 FROM json_each(uploads) u, json_each(u.value, '$.objects') o WHERE u.value ->> '$.discard' = 0 AND o.value ->> '$.key' = ${key}
            ) RETURNING id`,
        ),
    Effect.filterOrFail(Array.isReadonlyArrayNonEmpty, () => HttpApiError.Conflict.make()),
    Effect.asVoid,
    Effect.catchTag('SqlError', unavailable),
);
const completeUpload = Effect.fn('completeUpload')(
    (attempt: string) =>
        SqlClient.SqlClient.use(
            (sql) => sql`UPDATE assets SET body = json_insert(body, '$.objectKey', ${attempt}) WHERE removing IS NOT TRUE AND EXISTS (
                SELECT 1 FROM json_each(uploads) u WHERE u.value ->> '$.id' = ${attempt} AND (u.value ->> '$.discard' = 0 OR ${sql('objectKey')} IS NOT NULL)
            ) RETURNING id`,
        ),
    Effect.filterOrFail(Array.isReadonlyArrayNonEmpty, () => HttpApiError.Conflict.make()),
    Effect.asVoid,
    Effect.catchTag('SqlError', unavailable),
);

// --- [REMOVALS]
const releaseUploads = Effect.fn('releaseUploads')(
    function* (target: { readonly assetId: string; readonly remove: boolean } | { readonly attemptId: string }) {
        const sql = yield* SqlClient.SqlClient;
        const source = 'assetId' in target ? sql`assets.id = ${target.assetId}` : sql`EXISTS (SELECT 1 FROM json_each(uploads) WHERE value ->> '$.id' = ${target.attemptId})`;
        const attempts = 'attemptId' in target ? sql`u.value ->> '$.id' = ${target.attemptId}` : sql`TRUE`;
        const remove = 'assetId' in target && target.remove ? sql`TRUE` : sql`FALSE`;
        const [{ conflict, objects }] = yield* atomic([
            sql`UPDATE assets SET body = json_set(body, '$.removing', json('true')) WHERE ${source} AND ${remove}
                AND NOT EXISTS (SELECT 1 FROM (${references(sql, sql.literal('documents'))}) WHERE asset_id = assets.id)`,
            sql`UPDATE assets SET body = json_set(body, '$.uploads', json((
                SELECT json_group_array(json(CASE WHEN ${attempts} AND (removing IS TRUE OR (NOT ${remove} AND ${sql('objectKey')} IS NOT u.value ->> '$.id'))
                    THEN json_set(u.value, '$.discard', json('true')) ELSE u.value END)) FROM json_each(uploads) u
            ))) WHERE ${source}`,
            sql`SELECT EXISTS (SELECT 1 FROM assets WHERE ${source} AND ${remove} AND removing IS NOT TRUE) AS conflict,
                (SELECT json_group_array(json(o.value)) FROM assets, json_each(uploads) u, json_each(u.value, '$.objects') o WHERE ${source} AND ${attempts} AND u.value ->> '$.discard' = 1) AS objects`,
        ]).pipe(Effect.map(Array.lastNonEmpty), Effect.flatMap(Schema.decodeUnknownEffect(Schema.Tuple([Schema.Struct({ conflict: Schema.BooleanFromBit, objects: Schema.fromJsonString(Schema.Array(Schema.Struct({ key: Schema.String, uploadId: Schema.OptionFromOptionalKey(Schema.String) }))) })]))));
        return yield* conflict ? Effect.fail(HttpApiError.Conflict.make()) : Effect.succeed(objects);
    },
    Effect.catchTag(['SqlError', 'SchemaError'], unavailable),
);
const finishRelease = Effect.fn('finishRelease')(
    (key: string) =>
        SqlClient.SqlClient.use((sql) =>
            atomic([
                sql`UPDATE assets SET body = json_set(body, '$.uploads', json((
                    SELECT json_group_array(json(value)) FROM (
                        SELECT CASE WHEN u.value ->> '$.discard' = 1
                            THEN json_set(u.value, '$.objects', json((SELECT json_group_array(json(o.value)) FROM json_each(u.value, '$.objects') o WHERE o.value ->> '$.key' <> ${key})))
                            ELSE u.value END AS value FROM json_each(uploads) u
                    ) WHERE value ->> '$.discard' = 0 OR json_array_length(value, '$.objects') > 0
                ))) WHERE EXISTS (SELECT 1 FROM json_each(uploads) u, json_each(u.value, '$.objects') o WHERE o.value ->> '$.key' = ${key})`,
                sql`DELETE FROM assets WHERE (${sql('objectKey')} IS NULL OR removing IS TRUE) AND json_array_length(uploads) = 0`,
            ]),
        ),
    Effect.asVoid,
    Effect.catchTag('SqlError', unavailable),
);
const readPendingUploads = SqlClient.SqlClient.use(
    (sql) => sql`SELECT id, body ->> '$.name' AS name,
        CASE WHEN removing IS TRUE THEN 'removing' WHEN ${sql('objectKey')} IS NULL THEN 'incomplete' ELSE 'cleanup' END AS status
        FROM assets WHERE removing IS TRUE OR ${sql('objectKey')} IS NULL OR EXISTS (SELECT 1 FROM json_each(uploads) WHERE value ->> '$.id' <> ${sql('objectKey')})`,
).pipe(Effect.flatMap(Schema.decodeUnknownEffect(Schema.Array(PendingUpload))), Effect.catchTag(['SqlError', 'SchemaError'], unavailable));

// --- [EXPORTS] -------------------------------------------------------------------------

export { attachUpload, beginUpload, completeUpload, finishRelease, readPendingUploads, releaseUploads };
