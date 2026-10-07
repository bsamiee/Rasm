import { D1Client } from '@effect/sql-d1';
import { Array, Effect, Match, Option } from 'effect';
import { HttpApiError } from 'effect/http-api';
import type { PendingUpload } from '../model/api.ts';
import { type Asset, ImageAsset } from '../model/asset.ts';
import { contentEtag, unavailable } from './database.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

const beginUpload = Effect.fn('beginUpload')(
    function* (asset: typeof Asset.Type, attemptId: string, keys: readonly string[]) {
        const sql = yield* D1Client.D1Client;
        const digest = yield* contentEtag(JSON.stringify(asset));
        const stored = Match.value(asset).pipe(
            Match.when({ mime: 'application/pdf' }, ({ id, pages, ...body }) => ({ id, body, pages, renditions: [] })),
            Match.when({ mime: Match.is(...ImageAsset.fields.mime.literals) }, ({ id, renditions, ...body }) => ({ id, body, pages: [], renditions: renditions ?? [] })),
            Match.orElse(({ id, ...body }) => ({ id, body, pages: [], renditions: [] })),
        );
        const [sources] = yield* sql.batch([
            sql<{ objectKey: string | null }>`
                INSERT INTO assets(id,digest,body) VALUES (${asset.id},${digest},${JSON.stringify(stored.body)})
                ON CONFLICT(id) DO UPDATE SET digest = assets.digest
                WHERE assets.digest = excluded.digest AND assets.removing = 0 RETURNING object_key
            `,
            ...stored.pages.map(
                (page, position) => sql`
                INSERT INTO pages(asset_id,position,body)
                SELECT ${asset.id},${position},${JSON.stringify(page)} FROM assets WHERE id = ${asset.id} AND digest = ${digest} AND removing = 0
                ON CONFLICT(asset_id,position) DO NOTHING
            `,
            ),
            ...stored.renditions.map(
                (rendition, position) => sql`
                INSERT INTO renditions(asset_id,position,body)
                SELECT ${asset.id},${position},${JSON.stringify(rendition)} FROM assets WHERE id = ${asset.id} AND digest = ${digest} AND removing = 0
                ON CONFLICT(asset_id,position) DO NOTHING
            `,
            ),
            sql`
                INSERT INTO upload_attempts(id,asset_id)
                SELECT ${attemptId},id FROM assets WHERE id = ${asset.id} AND digest = ${digest} AND object_key IS NULL AND removing = 0
            `,
            ...keys.map((key) => sql`INSERT INTO upload_objects(key,attempt_id) SELECT ${key},id FROM upload_attempts WHERE id = ${attemptId}`),
        ]);
        return yield* Effect.succeed(sources).pipe(
            Effect.filterOrFail(Array.isReadonlyArrayNonEmpty, () => new HttpApiError.Conflict()),
            Effect.map((rows) => Option.fromNullishOr(Array.headNonEmpty(rows).objectKey)),
        );
    },
    Effect.catchTag('SqlError', unavailable),
);
const attachUpload = Effect.fn('attachUpload')(
    function* (key: string, uploadId: string) {
        const sql = yield* D1Client.D1Client;
        yield* sql`
            UPDATE upload_objects SET upload_id = ${uploadId} WHERE key = ${key}
            AND attempt_id IN (SELECT id FROM upload_attempts WHERE discard = 0)
            RETURNING key
        `.pipe(Effect.filterOrFail(Array.isReadonlyArrayNonEmpty, () => new HttpApiError.Conflict()));
    },
    Effect.catchTag('SqlError', unavailable),
);
const completeUpload = Effect.fn('completeUpload')(
    function* (attemptId: string) {
        const sql = yield* D1Client.D1Client;
        const [, sources] = yield* sql.batch([
            sql`
                UPDATE assets SET object_key = ${attemptId}
                WHERE object_key IS NULL AND removing = 0
                AND id IN (SELECT asset_id FROM upload_attempts WHERE id = ${attemptId} AND discard = 0)
            `,
            sql`
                SELECT assets.id FROM assets JOIN upload_attempts ON upload_attempts.asset_id = assets.id
                WHERE upload_attempts.id = ${attemptId} AND assets.object_key IS NOT NULL AND assets.removing = 0
            `,
        ]);
        yield* Effect.succeed(sources).pipe(Effect.filterOrFail(Array.isReadonlyArrayNonEmpty, () => new HttpApiError.Conflict()));
    },
    Effect.catchTag('SqlError', unavailable),
);
const releaseUploads = Effect.fn('releaseUploads')(
    function* (target: { readonly assetId: string; readonly remove: boolean } | { readonly attemptId: string }) {
        const sql = yield* D1Client.D1Client;
        const source = 'assetId' in target ? sql`id = ${target.assetId}` : sql`id IN (SELECT asset_id FROM upload_attempts WHERE id = ${target.attemptId})`;
        const remove = 'assetId' in target && target.remove;
        const [conflicts, , , objects] = yield* sql.batch([
            sql`
                SELECT id FROM assets WHERE ${source} AND ${remove}
                AND EXISTS (SELECT 1 FROM portfolio_references JOIN documents USING(digest) WHERE asset_id = assets.id)
            `,
            sql`
                UPDATE assets SET removing = 1 WHERE ${source} AND ${remove}
                AND NOT EXISTS (SELECT 1 FROM portfolio_references JOIN documents USING(digest) WHERE asset_id = assets.id)
            `,
            sql`
                UPDATE upload_attempts SET discard = 1
                WHERE ${'attemptId' in target ? sql`id = ${target.attemptId}` : sql`asset_id = ${target.assetId}`}
                AND asset_id IN (SELECT id FROM assets WHERE ${remove ? sql`removing = 1` : sql`removing = 1 OR object_key IS NULL OR object_key <> upload_attempts.id`})
            `,
            sql<{ key: string; uploadId: string | null }>`
                SELECT upload_objects.key,upload_objects.upload_id FROM upload_objects JOIN upload_attempts ON upload_attempts.id = upload_objects.attempt_id
                WHERE upload_attempts.discard = 1 AND ${'attemptId' in target ? sql`upload_attempts.id = ${target.attemptId}` : sql`upload_attempts.asset_id = ${target.assetId}`}
            `,
        ]);
        yield* Effect.succeed(conflicts).pipe(Effect.filterOrFail(Array.isReadonlyArrayEmpty, () => new HttpApiError.Conflict()));
        return objects;
    },
    Effect.catchTag('SqlError', unavailable),
);
const finishRelease = Effect.fn('finishRelease')(
    function* (key: string) {
        const sql = yield* D1Client.D1Client;
        yield* sql.batch([
            sql`DELETE FROM upload_objects WHERE key = ${key} AND attempt_id IN (SELECT id FROM upload_attempts WHERE discard = 1)`,
            sql`DELETE FROM upload_attempts WHERE discard = 1 AND NOT EXISTS (SELECT 1 FROM upload_objects WHERE attempt_id = upload_attempts.id)`,
            sql`DELETE FROM assets WHERE (object_key IS NULL OR removing = 1) AND NOT EXISTS (SELECT 1 FROM upload_attempts WHERE asset_id = assets.id)`,
        ]);
    },
    Effect.catchTag('SqlError', unavailable),
);
const readPendingUploads = Effect.fn('readPendingUploads')(
    function* () {
        const sql = yield* D1Client.D1Client;
        return yield* sql<typeof PendingUpload.Type>`
            SELECT id,body ->> '$.name' AS name,
                CASE WHEN removing = 1 THEN 'removing' WHEN object_key IS NULL THEN 'incomplete' ELSE 'cleanup' END AS status
            FROM assets WHERE removing = 1 OR object_key IS NULL
                OR EXISTS (SELECT 1 FROM upload_attempts WHERE asset_id = assets.id AND id <> assets.object_key)
        `;
    },
    Effect.catchTag('SqlError', unavailable),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { attachUpload, beginUpload, completeUpload, finishRelease, readPendingUploads, releaseUploads };
