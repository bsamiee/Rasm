import { DeleteObjectCommand, S3Client, type S3ClientConfig } from '@aws-sdk/client-s3';
import { ByteSize, Cause, Clock, Context, Effect, Layer, Match, Option, Stream } from 'effect';
import { HttpServerRequest, HttpServerResponse } from 'effect/http';
import { HttpApiError } from 'effect/http-api';
import type { ObjectStore } from 'partial-content';
import { r2Store, r2UploadStore } from 'partial-content/r2';
import { s3Store, s3UploadStore } from 'partial-content/s3';
import { isUploadDestinationConflictError, isUploadOffsetConflictError, isUploadOverrunError, type PrefixedUploadStore } from 'partial-content/upload';
import { type ServeContext, type ServeObjectOptions, serveObject } from 'partial-content/web';

// --- [SERVICES] ------------------------------------------------------------------------

class ObjectStorage extends Context.Service<ObjectStorage, { readonly objects: ObjectStore; readonly uploads: PrefixedUploadStore; readonly delete: (key: string) => Effect.Effect<void, Cause.UnknownError> }>()('@rasm/hosting/server/object-storage/ObjectStorage') {
    static readonly r2 = (bucket: R2Bucket) => {
        const uploads = r2UploadStore({ bucket });
        return Layer.succeed(ObjectStorage, { objects: r2Store({ bucket, reservedPrefix: uploads.uploadPrefix }), uploads, delete: (key) => Effect.tryPromise({ try: () => bucket.delete(key), catch: failure('delete', key) }) });
    };
    static readonly s3 = Effect.fnUntraced(function* (bucket: string, client: S3ClientConfig = {}) {
        const opened = yield* Effect.acquireRelease(
            Effect.sync(() => new S3Client(client)),
            (s3) => Effect.sync(s3.destroy.bind(s3)),
        );
        const uploads = s3UploadStore({ client: opened, bucket });
        return {
            objects: s3Store({ client: opened, bucket, reservedPrefix: uploads.uploadPrefix }),
            uploads,
            delete: (key: string) => Effect.asVoid(Effect.tryPromise({ try: (signal) => opened.send(new DeleteObjectCommand({ Bucket: bucket, Key: key }), { abortSignal: signal }), catch: failure('delete', key) })),
        };
    }, Layer.effect(ObjectStorage));
    static readonly upload = Effect.fn('ObjectStorage.upload')(function* (object: { readonly key: string; readonly size: ByteSize.ByteSize }) {
        const [{ uploads }, created] = yield* Effect.all([ObjectStorage, Clock.currentTimeMillis]);
        const length = ByteSize.toNumberUnsafe(object.size);
        const { uploadToken } = yield* Effect.acquireRelease(Effect.tryPromise({ try: (signal) => uploads.createUpload({ key: object.key, length, now: created, signal }), catch: failure('upload', object.key) }), (upload) =>
            Effect.tryPromise({ try: () => uploads.abortUpload(upload.uploadToken), catch: failure('upload abort', object.key) }).pipe(Effect.ignore({ log: 'Error' })),
        );
        return {
            uploadToken,
            write: Effect.fn('ObjectStorage.write')(function* (content: Stream.Stream<Uint8Array, unknown>) {
                const [body, now] = yield* Effect.all([Stream.toReadableStreamEffect(content), Clock.currentTimeMillis]);
                yield* Effect.tryPromise({
                    try: (signal) => uploads.appendChunk(uploadToken, 0, body, { now, maxBytes: length, maxBytesFatal: true, signal }),
                    catch: (error) => (isUploadOverrunError(error) ? HttpApiError.BadRequest.make() : failure('upload write', object.key)(error)),
                });
                yield* Effect.tryPromise({
                    try: (signal) => uploads.completeUpload(uploadToken, { now, expectedLength: length, signal }),
                    catch: (error) =>
                        Match.value(error).pipe(
                            Match.when(isUploadOffsetConflictError, HttpApiError.BadRequest.make.bind(HttpApiError.BadRequest, {}, undefined)),
                            Match.when(isUploadDestinationConflictError, HttpApiError.Conflict.make.bind(HttpApiError.Conflict, {}, undefined)),
                            Match.orElse(failure('upload completion', object.key)),
                        ),
                });
            }),
        };
    });
    static readonly remove = Effect.fn('ObjectStorage.remove')(function* (key: string, uploadToken: Option.Option<string>) {
        const storage = yield* ObjectStorage;
        yield* Option.match(uploadToken, { onNone: () => Effect.void, onSome: (token) => Effect.tryPromise({ try: (signal) => storage.uploads.abortUpload(token, { signal }), catch: failure('upload abort', key) }) });
        yield* storage.delete(key);
    });
    static readonly serve = Effect.fn('ObjectStorage.serve')(function* (object: ServeContext, options: Omit<ServeObjectOptions, 'onError'> = {}) {
        const [{ objects }, request, context] = yield* Effect.all([ObjectStorage, Effect.flatMap(HttpServerRequest.HttpServerRequest, HttpServerRequest.toWeb).pipe(Effect.orDie), Effect.context<never>()]);
        const served = serveObject(objects, { ...options, onError: (error, { operation, key }) => Effect.logError(Cause.fail(failure(operation, key)(error))).pipe(Effect.runSyncWith(context)) });
        return HttpServerResponse.raw(yield* Effect.promise(() => served(request, object)));
    });
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const failure = (operation: string, key: string) => (cause: unknown) => new Cause.UnknownError(cause, `object ${key} ${operation} failed`);

// --- [EXPORTS] -------------------------------------------------------------------------

export { ObjectStorage };
