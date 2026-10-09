import { D1Client } from '@effect/sql-d1';
import { Effect, flow, Schema } from 'effect';
import { HttpApiError } from 'effect/http-api';
import { SqlSchema } from 'effect/sql';
import { contentEtag } from '../db/digest.ts';
import { PendingUpload } from '../model/api.ts';
import type { Asset } from '../model/asset.ts';
import { unavailable } from './database.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [UPLOADS]
const beginUpload = Effect.fn('beginUpload')(
    function* (asset: typeof Asset.Type, attemptId: string, keys: readonly string[]) {
        const sql = yield* D1Client.D1Client;
        const body = JSON.stringify(asset);
        const digest = yield* contentEtag(body);
        const source = yield* SqlSchema.findOne({
            Request: Schema.Void,
            Result: Schema.Struct({ objectKey: Schema.OptionFromNullOr(Schema.String) }),
            execute: () =>
                sql
                    .batch([
                        sql`INSERT INTO assets(digest,body) VALUES (${digest},${body})
                    ON CONFLICT(id) DO UPDATE SET digest = assets.digest
                    WHERE assets.digest = excluded.digest AND assets.removing = 0 RETURNING object_key`,
                        sql`INSERT INTO upload_attempts(id,asset_id)
                    SELECT ${attemptId},id FROM assets WHERE id = ${asset.id} AND object_key IS NULL AND changes() > 0`,
                        sql`INSERT INTO upload_objects(key,attempt_id)
                    SELECT j.value,upload_attempts.id FROM upload_attempts,json_each(${JSON.stringify(keys)}) j WHERE upload_attempts.id = ${attemptId}`,
                    ])
                    .pipe(Effect.map(([sources]) => sources)),
        })(undefined);
        return source.objectKey;
    },
    Effect.catchTag('NoSuchElementError', () => Effect.fail(new HttpApiError.Conflict())),
    Effect.catchTag(['SqlError', 'SchemaError', 'PlatformError'], unavailable),
);
const attachUpload = flow(
    SqlSchema.findOne({
        Request: Schema.Struct({ key: Schema.String, uploadId: Schema.String }),
        Result: Schema.Struct({ key: Schema.String }),
        execute: (object) =>
            D1Client.D1Client.use(
                (sql) => sql`UPDATE upload_objects SET upload_id = ${object.uploadId} WHERE key = ${object.key}
                AND attempt_id IN (SELECT id FROM upload_attempts WHERE discard = 0) RETURNING key`,
            ),
    }),
    Effect.asVoid,
    Effect.catchTag('NoSuchElementError', () => Effect.fail(new HttpApiError.Conflict())),
    Effect.catchTag(['SqlError', 'SchemaError'], unavailable),
);
const completeUpload = flow(
    SqlSchema.findOne({
        Request: Schema.String,
        Result: Schema.Struct({ id: Schema.String }),
        execute: (attempt) =>
            D1Client.D1Client.use(
                (sql) => sql`UPDATE assets SET object_key = COALESCE(object_key,${attempt})
                WHERE removing = 0 AND id IN (
                    SELECT asset_id FROM upload_attempts WHERE id = ${attempt} AND (discard = 0 OR assets.object_key IS NOT NULL)
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
        const source = 'assetId' in target ? sql`id = ${target.assetId}` : sql`id IN (SELECT asset_id FROM upload_attempts WHERE id = ${target.attemptId})`;
        const attempts = 'attemptId' in target ? sql`id = ${target.attemptId}` : sql`asset_id = ${target.assetId}`;
        const remove = 'assetId' in target && target.remove;
        return yield* SqlSchema.findOne({
            Request: Schema.Void,
            Result: Schema.Struct({
                conflict: Schema.BooleanFromBit,
                objects: Schema.Array(Schema.Struct({ key: Schema.String, uploadId: Schema.OptionFromNullOr(Schema.String) })),
            }),
            execute: () =>
                sql
                    .batch([
                        sql`UPDATE assets SET removing = 1 WHERE ${source} AND ${remove}
                    AND NOT EXISTS (SELECT 1 FROM portfolio_references JOIN documents USING(digest) WHERE asset_id = assets.id)`,
                        sql`UPDATE upload_attempts SET discard = 1 WHERE ${attempts}
                    AND asset_id IN (SELECT id FROM assets WHERE ${remove ? sql`removing = 1` : sql`removing = 1 OR object_key IS NULL OR object_key <> upload_attempts.id`})`,
                        sql`SELECT EXISTS (
                        SELECT 1 FROM assets WHERE ${source} AND ${remove}
                        AND EXISTS (SELECT 1 FROM portfolio_references JOIN documents USING(digest) WHERE asset_id = assets.id)
                    ) AS conflict`,
                        sql`SELECT key,upload_id FROM upload_objects
                        WHERE attempt_id IN (SELECT id FROM upload_attempts WHERE discard = 1 AND ${attempts})`,
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
                sql`DELETE FROM upload_objects WHERE key = ${key} AND attempt_id IN (SELECT id FROM upload_attempts WHERE discard = 1)`,
                sql`DELETE FROM upload_attempts WHERE discard = 1 AND NOT EXISTS (SELECT 1 FROM upload_objects WHERE attempt_id = upload_attempts.id)`,
                sql`DELETE FROM assets WHERE (object_key IS NULL OR removing = 1) AND NOT EXISTS (SELECT 1 FROM upload_attempts WHERE asset_id = assets.id)`,
            ]),
        ),
    (effect) => effect.pipe(Effect.asVoid, Effect.catchTag('SqlError', unavailable)),
);
const readPendingUploads = flow(
    SqlSchema.findAll({
        Request: Schema.Void,
        Result: PendingUpload,
        execute: () =>
            D1Client.D1Client.use(
                (sql) => sql`SELECT id,body ->> '$.name' AS name,
                CASE WHEN removing = 1 THEN 'removing' WHEN object_key IS NULL THEN 'incomplete' ELSE 'cleanup' END AS status
                FROM assets WHERE removing = 1 OR object_key IS NULL
                    OR EXISTS (SELECT 1 FROM upload_attempts WHERE asset_id = assets.id AND id <> assets.object_key)`,
            ),
    }),
    Effect.catchTag(['SqlError', 'SchemaError'], unavailable),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { attachUpload, beginUpload, completeUpload, finishRelease, readPendingUploads, releaseUploads };
