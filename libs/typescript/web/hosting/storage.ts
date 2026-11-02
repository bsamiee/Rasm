import { DeleteObjectCommand, S3Client, type S3ClientConfig } from '@aws-sdk/client-s3';
import { Cause, Clock, Context, Effect, Layer, Option, Stream } from 'effect';
import { HttpServerRequest, HttpServerResponse } from 'effect/http';
import { HttpApiError } from 'effect/http-api';
import type { ObjectStore } from 'partial-content';
import { r2Store, r2UploadStore } from 'partial-content/r2';
import { s3Store, s3UploadStore } from 'partial-content/s3';
import { isUploadDestinationConflictError, isUploadOverrunError, type PrefixedUploadStore } from 'partial-content/upload';
import { type ServeContext, type ServeObjectOptions, serveObject } from 'partial-content/web';

// --- [SERVICES] ------------------------------------------------------------------------

class ObjectStorage extends Context.Service<ObjectStorage, { readonly objects: ObjectStore; readonly uploads: PrefixedUploadStore; readonly delete: (key: string) => Effect.Effect<unknown, Cause.UnknownError> }>()('@rasm/hosting/storage/ObjectStorage') {
    static readonly r2 = (bucket: R2Bucket) => {
        const uploads = r2UploadStore({ bucket });
        return Layer.succeed(ObjectStorage, { objects: r2Store({ bucket, reservedPrefix: uploads.uploadPrefix }), uploads, delete: (key) => Effect.tryPromise({ try: () => bucket.delete(key), catch: failure('delete', key) }) });
    };
    static readonly s3 = (bucket: string, client: S3ClientConfig = {}) =>
        Layer.effect(
            ObjectStorage,
            Effect.map(
                Effect.acquireRelease(
                    Effect.sync(() => new S3Client(client)),
                    (opened) => Effect.sync(() => opened.destroy()),
                ),
                (opened) => {
                    const uploads = s3UploadStore({ client: opened, bucket });
                    return { objects: s3Store({ client: opened, bucket, reservedPrefix: uploads.uploadPrefix }), uploads, delete: (key: string) => Effect.tryPromise({ try: (signal) => opened.send(new DeleteObjectCommand({ Bucket: bucket, Key: key }), { abortSignal: signal }), catch: failure('delete', key) }) };
                },
            ),
        );
    static readonly upload = Effect.fn('ObjectStorage.upload')(function* (object: { readonly key: string; readonly size: number }) {
        const [{ uploads }, created] = yield* Effect.all([ObjectStorage, Clock.currentTimeMillis]);
        const { uploadToken } = yield* Effect.acquireRelease(Effect.tryPromise({ try: (signal) => uploads.createUpload({ key: object.key, length: object.size, now: created, signal }), catch: failure('upload', object.key) }), (upload) =>
            Effect.tryPromise({ try: () => uploads.abortUpload(upload.uploadToken), catch: failure('upload abort', object.key) }).pipe(Effect.ignore({ log: 'Error', message: 'upload abort failed' })),
        );
        return {
            uploadToken,
            write: Effect.fn('ObjectStorage.write')(function* (content: Stream.Stream<Uint8Array, unknown>) {
                const [body, now] = yield* Effect.all([Stream.toReadableStreamEffect(content), Clock.currentTimeMillis]);
                yield* Effect.tryPromise({
                    try: (signal) => uploads.appendChunk(uploadToken, 0, body, { now, maxBytes: object.size, maxBytesFatal: true, signal }),
                    catch: (error) => (isUploadOverrunError(error) ? HttpApiError.BadRequest.make() : failure('upload write', object.key)(error)),
                }).pipe(
                    Effect.filterOrFail(
                        ({ bytesWritten }) => bytesWritten === object.size,
                        () => HttpApiError.BadRequest.make(),
                    ),
                );
                yield* Effect.tryPromise({
                    try: (signal) => uploads.completeUpload(uploadToken, { now, expectedLength: object.size, signal }),
                    catch: (error) => (isUploadDestinationConflictError(error) ? HttpApiError.Conflict.make() : failure('upload completion', object.key)(error)),
                });
            }),
        };
    });
    static readonly remove = Effect.fn('ObjectStorage.remove')(function* (key: string, upload: Option.Option<string>) {
        const storage = yield* ObjectStorage;
        yield* Option.match(upload, { onNone: () => Effect.void, onSome: (token) => Effect.tryPromise({ try: (signal) => storage.uploads.abortUpload(token, { signal }), catch: failure('upload abort', key) }) });
        yield* storage.delete(key);
    });
    static readonly serve = Effect.fn('ObjectStorage.serve')(function* (object: ServeContext, options: Omit<ServeObjectOptions, 'onError'> = {}) {
        const [{ objects }, request, context] = yield* Effect.all([ObjectStorage, Effect.flatMap(HttpServerRequest.HttpServerRequest, HttpServerRequest.toWeb), Effect.context<never>()]);
        const served = serveObject(objects, { ...options, onError: (error, { operation, key }) => Effect.logError('object serve failed', Cause.die(error)).pipe(Effect.annotateLogs({ operation, key }), Effect.runSyncWith(context)) });
        return HttpServerResponse.raw(yield* Effect.promise(() => served(request, object)));
    });
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const failure = (operation: string, key: string) => (cause: unknown) => new Cause.UnknownError(cause, `object ${key} ${operation} failed`);

// --- [EXPORTS] -------------------------------------------------------------------------

export { ObjectStorage };
