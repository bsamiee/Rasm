import { atomic } from '@rasm/database/table.ts';
import { unavailable } from '@rasm/hosting/http.ts';
import { Array, Data, Effect, Schema, Struct } from 'effect';
import { HttpApiError } from 'effect/http-api';
import { SqlClient, SqlSchema, type Statement } from 'effect/sql';
import { PendingUpload } from '../model/api.ts';
import type { Asset } from '../model/asset.ts';
import { assets, bookkeeping, references, Upload } from './database.ts';

// --- [MODELS] --------------------------------------------------------------------------

type Release = Data.TaggedEnum<{ Abandon: { readonly assetId: string; readonly attemptId: string }; Discard: { readonly assetId: string }; Delete: { readonly assetId: string } }>;
const Release = Data.taggedEnum<Release>();

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
                    WHERE json_remove(assets.body, ${sql.literal(bookkeeping)}) = json_remove(excluded.body, ${sql.literal(bookkeeping)}) AND assets.removing IS NOT TRUE
                    RETURNING ${sql('objectKey')}`,
                ),
        })({ id, body: { ...metadata, uploads: [{ id: attemptId, discard: false, objects: keys.map((key) => ({ key })) } satisfies typeof Upload.Encoded] } }),
    Effect.map(Struct.get('objectKey')),
    Effect.catchTag(['SqlError', 'SchemaError'], unavailable, () => Effect.fail(HttpApiError.Conflict.make())),
);
const attachUpload = Effect.fn('attachUpload')(
    (assetId: string, key: string, uploadId: string) =>
        SqlClient.SqlClient.use(
            (sql) => sql`UPDATE assets SET body = json_set(body, (
                SELECT o.fullkey || '.uploadId' FROM json_each(body, '$.uploads') u, json_each(body, u.fullkey || '.objects') o WHERE o.value ->> '$.key' = ${key}
            ), ${uploadId}) WHERE id = ${assetId} AND EXISTS (
                SELECT 1 FROM json_each(uploads) u, json_each(u.value, '$.objects') o WHERE u.value ->> '$.discard' = 0 AND o.value ->> '$.key' = ${key}
            ) RETURNING id`,
        ),
    Effect.filterOrFail(Array.isReadonlyArrayNonEmpty, () => HttpApiError.Conflict.make()),
    Effect.asVoid,
    Effect.catchTag('SqlError', unavailable),
);
const completeUpload = Effect.fn('completeUpload')(
    (assetId: string, attempt: string) =>
        SqlClient.SqlClient.use(
            (sql) => sql`UPDATE assets SET body = json_insert(body, '$.objectKey', ${attempt}) WHERE id = ${assetId} AND removing IS NOT TRUE AND EXISTS (
                SELECT 1 FROM json_each(uploads) u WHERE u.value ->> '$.id' = ${attempt} AND (u.value ->> '$.discard' = 0 OR ${sql('objectKey')} IS NOT NULL)
            ) RETURNING id`,
        ),
    Effect.filterOrFail(Array.isReadonlyArrayNonEmpty, () => HttpApiError.Conflict.make()),
    Effect.asVoid,
    Effect.catchTag('SqlError', unavailable),
);

// --- [REMOVALS]
const releaseUploads = Effect.fn('releaseUploads')(
    (release: Release) =>
        SqlClient.SqlClient.use((sql) => {
            const pending = sql`(removing IS TRUE OR ${sql('objectKey')} IS NOT u.value ->> '$.id')`;
            const discard = (...discarded: readonly Statement.Fragment[]) => sql`UPDATE assets SET body = json_set(body, '$.uploads', json((
                SELECT json_group_array(json(CASE WHEN ${sql.and(discarded)} THEN json_set(u.value, '$.discard', json('true')) ELSE u.value END)) FROM json_each(uploads) u
            ))) WHERE id = ${release.assetId}`;
            const objects = (...released: readonly Statement.Fragment[]) => sql`SELECT json_group_array(json(o.value)) FROM assets, json_each(uploads) u, json_each(u.value, '$.objects') o
                WHERE ${sql.and([sql`assets.id = ${release.assetId}`, sql`u.value ->> '$.discard' = 1`, ...released])}`;
            return Release.$match(release, {
                Abandon: ({ attemptId }) => {
                    const attempt = sql`u.value ->> '$.id' = ${attemptId}`;
                    return atomic([discard(attempt, pending), sql`SELECT (${objects(attempt)}) AS objects`]);
                },
                Discard: () => atomic([discard(pending), sql`SELECT (${objects()}) AS objects`]),
                Delete: ({ assetId }) =>
                    atomic([
                        sql`UPDATE assets SET body = json_set(body, '$.removing', json('true')) WHERE id = ${assetId} AND NOT EXISTS (SELECT 1 FROM (${references(sql, sql.literal('documents'))}) WHERE asset_id = assets.id)`,
                        discard(sql`removing IS TRUE`),
                        sql`SELECT (${objects()}) AS objects WHERE NOT EXISTS (SELECT 1 FROM assets WHERE id = ${assetId} AND removing IS NOT TRUE)`,
                    ]),
            });
        }),
    Effect.map(Array.lastNonEmpty),
    Effect.flatMap(Schema.decodeUnknownEffect(Schema.Array(Schema.Struct({ objects: Schema.fromJsonString(Upload.fields.objects) })))),
    Effect.filterOrFail(Array.isReadonlyArrayNonEmpty, () => HttpApiError.Conflict.make()),
    Effect.map(([{ objects }]) => objects),
    Effect.catchTag(['SqlError', 'SchemaError'], unavailable),
);
const finishRelease = Effect.fn('finishRelease')(
    (assetId: string, keys: Array.NonEmptyReadonlyArray<string>) =>
        SqlClient.SqlClient.use((sql) =>
            atomic([
                sql`UPDATE assets SET body = json_set(body, '$.uploads', json((
                    SELECT json_group_array(json(value)) FROM (
                        SELECT CASE WHEN u.value ->> '$.discard' = 1
                            THEN json_set(u.value, '$.objects', json((SELECT json_group_array(json(o.value)) FROM json_each(u.value, '$.objects') o LEFT JOIN json_each(${JSON.stringify(keys)}) k ON k.value = o.value ->> '$.key' WHERE k.value IS NULL)))
                            ELSE u.value END AS value FROM json_each(uploads) u
                    ) WHERE value ->> '$.discard' = 0 OR json_array_length(value, '$.objects') > 0
                ))) WHERE id = ${assetId}`,
                sql`DELETE FROM assets WHERE id = ${assetId} AND NOT available AND json_array_length(uploads) = 0`,
            ]),
        ),
    Effect.asVoid,
    Effect.catchTag('SqlError', unavailable),
);
const readPendingUploads = SqlClient.SqlClient.use(
    (sql) => sql`SELECT id, body ->> '$.name' AS name,
        CASE WHEN removing IS TRUE THEN 'removing' WHEN ${sql('objectKey')} IS NULL THEN 'incomplete' ELSE 'cleanup' END AS status
        FROM assets WHERE NOT available OR EXISTS (SELECT 1 FROM json_each(uploads) WHERE value ->> '$.id' <> ${sql('objectKey')})`,
).pipe(Effect.flatMap(Schema.decodeUnknownEffect(Schema.Array(PendingUpload))), Effect.catchTag(['SqlError', 'SchemaError'], unavailable), Effect.withSpan('readPendingUploads'));

// --- [EXPORTS] -------------------------------------------------------------------------

export { attachUpload, beginUpload, completeUpload, finishRelease, Release, readPendingUploads, releaseUploads };
