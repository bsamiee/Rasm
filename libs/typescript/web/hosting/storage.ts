import { DeleteObjectCommand, S3Client } from '@aws-sdk/client-s3';
import { Cause, Clock, Context, Effect, flow, Layer, Option, Stream } from 'effect';
import { HttpServerRequest, HttpServerResponse } from 'effect/http';
import { HttpApiError } from 'effect/http-api';
import type { ObjectStore } from 'partial-content';
import { r2Store, r2UploadStore } from 'partial-content/r2';
import { type SweepingWriteStore, s3Store, s3UploadStore } from 'partial-content/s3';
import { isUploadOverrunError } from 'partial-content/upload';
import { serveObject as serve } from 'partial-content/web';

// --- [SERVICES] ------------------------------------------------------------------------

class Storage extends Context.Service<Storage, { readonly objects: ObjectStore; readonly uploads: SweepingWriteStore; readonly remove: (key: string) => Promise<unknown> }>()('@rasm/hosting/storage') {}

// --- [OPERATIONS] ----------------------------------------------------------------------

const putObject = Effect.fn('putObject')(function* <S, R>(object: { readonly key: string; readonly size: number }, content: Stream.Stream<Uint8Array, unknown>, started: (uploadToken: string) => Effect.Effect<void, S, R>) {
    const [{ uploads }, now] = yield* Effect.all([Storage, Clock.currentTimeMillis]);
    const { uploadToken } = yield* Effect.acquireRelease(
        Effect.tryPromise(() => uploads.createUpload({ key: object.key, length: object.size, now })),
        (created) => Effect.tryPromise(() => uploads.abortUpload(created.uploadToken)).pipe(Effect.catch(Effect.logError)),
    );
    yield* started(uploadToken);
    const body = yield* Stream.toReadableStreamEffect(content);
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
});
const releaseObject = Effect.fn('releaseObject')(function* (key: string, uploadToken: Option.Option<string>) {
    const { uploads, remove } = yield* Storage;
    yield* Option.match(uploadToken, { onNone: () => Effect.void, onSome: (token) => Effect.tryPromise(() => uploads.abortUpload(token)) });
    yield* Effect.tryPromise(() => remove(key));
});
const serveObject = Effect.fn('serveObject')(function* (request: HttpServerRequest.HttpServerRequest, object: { readonly key: string; readonly mime: string }) {
    const [{ objects }, requested] = yield* Effect.all([Storage, HttpServerRequest.toWeb(request)]);
    const run = Effect.runSyncWith(yield* Effect.context());
    return HttpServerResponse.fromWeb(yield* Effect.promise(() => serve(objects, { disposition: 'inline', securityHeaders: () => ({ 'X-Content-Type-Options': 'nosniff' }), onError: flow(Effect.logError, run) })(requested, object)));
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const r2 = (bucket: R2Bucket) => Layer.succeed(Storage, { objects: r2Store({ bucket }), uploads: r2UploadStore({ bucket }), remove: (key) => bucket.delete(key) });
const s3 = (bucket: string) =>
    Layer.effect(
        Storage,
        Effect.map(
            Effect.acquireRelease(
                Effect.sync(() => new S3Client({ forcePathStyle: true })),
                (client) => Effect.sync(() => client.destroy()),
            ),
            (client) => ({ objects: s3Store({ client, bucket }), uploads: s3UploadStore({ client, bucket }), remove: (key) => client.send(new DeleteObjectCommand({ Bucket: bucket, Key: key })) }),
        ),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { putObject, r2, releaseObject, Storage, s3, serveObject };
