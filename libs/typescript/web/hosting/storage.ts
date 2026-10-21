import { DeleteObjectCommand, S3Client } from '@aws-sdk/client-s3';
import { Cause, Clock, Context, Effect, Layer, Option, Stream } from 'effect';
import { HttpServerRequest, HttpServerResponse } from 'effect/http';
import { HttpApiError } from 'effect/http-api';
import type { ObjectStore } from 'partial-content';
import { r2Store, r2UploadStore } from 'partial-content/r2';
import { type SweepingWriteStore, s3Store, s3UploadStore } from 'partial-content/s3';
import { isUploadOverrunError } from 'partial-content/upload';
import { serveObject as serve } from 'partial-content/web';

// --- [SERVICES] ------------------------------------------------------------------------

class Storage extends Context.Service<Storage, { readonly objects: ObjectStore; readonly uploads: SweepingWriteStore; readonly remove: (key: string) => Effect.Effect<unknown, Cause.UnknownError> }>()('@rasm/hosting/storage') {}

// --- [OPERATIONS] ----------------------------------------------------------------------

const createUpload = Effect.fn('createUpload')(function* (object: { readonly key: string; readonly size: number }) {
    const [{ uploads }, created] = yield* Effect.all([Storage, Clock.currentTimeMillis]);
    const { uploadToken } = yield* Effect.acquireRelease(
        Effect.tryPromise(() => uploads.createUpload({ key: object.key, length: object.size, now: created })),
        (upload) => Effect.tryPromise(() => uploads.abortUpload(upload.uploadToken)).pipe(Effect.ignore({ log: 'Error', message: 'upload abort failed' })),
    );
    return {
        uploadToken,
        write: Effect.fn('write')(function* (content: Stream.Stream<Uint8Array, unknown>) {
            const [body, now] = yield* Effect.all([Stream.toReadableStreamEffect(content), Clock.currentTimeMillis]);
            yield* Effect.tryPromise({
                try: (signal) => uploads.appendChunk(uploadToken, 0, body, { now, maxBytes: object.size, maxBytesFatal: true, signal }),
                catch: (error) => (isUploadOverrunError(error) ? HttpApiError.BadRequest.make() : new Cause.UnknownError(error)),
            }).pipe(
                Effect.filterOrFail(
                    ({ bytesWritten }) => bytesWritten === object.size,
                    () => HttpApiError.BadRequest.make(),
                ),
            );
            yield* Effect.tryPromise(() => uploads.completeUpload(uploadToken, { now }));
        }),
    };
});
const releaseObject = Effect.fn('releaseObject')(function* (key: string, uploadToken: Option.Option<string>) {
    const { uploads, remove } = yield* Storage;
    yield* Option.match(uploadToken, { onNone: () => Effect.void, onSome: (token) => Effect.tryPromise(() => uploads.abortUpload(token)) });
    yield* remove(key);
});
const serveObject = Effect.fn('serveObject')(function* (object: { readonly key: string; readonly mime: string }) {
    const [{ objects }, request, context] = yield* Effect.all([Storage, Effect.flatMap(HttpServerRequest.HttpServerRequest, HttpServerRequest.toWeb), Effect.context<never>()]);
    const served = serve(objects, {
        disposition: 'inline',
        securityHeaders: () => ({ 'X-Content-Type-Options': 'nosniff' }),
        onError: (error, { operation, key }) => Effect.logError('object serve failed', Cause.die(error)).pipe(Effect.annotateLogs({ operation, key }), Effect.runSyncWith(context)),
    });
    return HttpServerResponse.fromWeb(yield* Effect.promise(() => served(request, object)));
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const r2 = (bucket: R2Bucket) => Layer.succeed(Storage, { objects: r2Store({ bucket }), uploads: r2UploadStore({ bucket }), remove: (key) => Effect.tryPromise(() => bucket.delete(key)) });
const s3 = (bucket: string) =>
    Layer.effect(
        Storage,
        Effect.map(
            Effect.acquireRelease(
                Effect.sync(() => new S3Client({ forcePathStyle: true })),
                (client) => Effect.sync(() => client.destroy()),
            ),
            (client) => ({ objects: s3Store({ client, bucket }), uploads: s3UploadStore({ client, bucket }), remove: (key) => Effect.tryPromise(() => client.send(new DeleteObjectCommand({ Bucket: bucket, Key: key }))) }),
        ),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { createUpload, r2, releaseObject, Storage, s3, serveObject };
