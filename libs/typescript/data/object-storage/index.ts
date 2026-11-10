import { ByteSize, Cause, Clock, Context, Effect, Formatter, Match, Option, Schema, Stream } from 'effect';
import { HttpServerRequest, HttpServerResponse } from 'effect/http';
import type { ObjectStore } from 'partial-content';
import { createUploadHandler, isUploadDestinationConflictError, isUploadOffsetConflictError, isUploadOverrunError, type PrefixedUploadStore, type UploadHandlerOptions } from 'partial-content/upload';
import { type ServeContext, type ServeObjectOptions, serveObject } from 'partial-content/web';

// --- [ERRORS] --------------------------------------------------------------------------

class ObjectStorageError extends Schema.TaggedError<ObjectStorageError, { readonly brand: unique symbol }>()('ObjectStorageError', {
    reason: Schema.TaggedUnion({ Refused: {}, Conflict: {}, Unavailable: {} }),
    subject: Schema.String,
    cause: Schema.optionalKey(Schema.Defect()),
}) {
    static readonly of = (reason: ObjectStorageError['reason']['_tag'], subject: string) => (cause?: unknown) => ObjectStorageError.make({ reason: { _tag: reason }, subject, ...(cause === undefined ? {} : { cause }) });
    static readonly written = (subject: string) => (cause: unknown) =>
        Match.value(cause).pipe(Match.whenOr(isUploadOverrunError, isUploadOffsetConflictError, ObjectStorageError.of('Refused', subject)), Match.when(isUploadDestinationConflictError, ObjectStorageError.of('Conflict', subject)), Match.orElse(ObjectStorageError.of('Unavailable', subject)));

    override get message() {
        return [`${this.reason._tag}: ${this.subject}`, ...(this.cause === undefined ? [] : [Formatter.format(this.cause)])].join(': ');
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

class ObjectStorage extends Context.Service<ObjectStorage, { readonly objects: ObjectStore; readonly uploads: PrefixedUploadStore; readonly delete: (key: string) => Effect.Effect<void, ObjectStorageError> }>()('@rasm/object-storage/ObjectStorage') {
    static readonly upload = Effect.fn('ObjectStorage.upload')(function* (object: { readonly key: string; readonly size: ByteSize.ByteSize }) {
        const [{ uploads }, created] = yield* Effect.all([ObjectStorage, Clock.currentTimeMillis]);
        const length = ByteSize.toNumberUnsafe(object.size);
        const { uploadToken } = yield* Effect.acquireRelease(Effect.tryPromise({ try: (signal) => uploads.createUpload({ key: object.key, length, now: created, signal }), catch: ObjectStorageError.of('Unavailable', object.key) }), (upload) =>
            Effect.tryPromise({ try: () => uploads.abortUpload(upload.uploadToken), catch: ObjectStorageError.of('Unavailable', object.key) }).pipe(Effect.ignore({ log: 'Error', message: 'upload abort failed' })),
        );
        return {
            uploadToken,
            write: Effect.fn('ObjectStorage.write')(function* (content: Stream.Stream<Uint8Array, unknown>) {
                const [body, now] = yield* Effect.all([Stream.toReadableStreamEffect(content), Clock.currentTimeMillis]);
                yield* Effect.tryPromise({ try: (signal) => uploads.appendChunk(uploadToken, 0, body, { now, maxBytes: length, maxBytesFatal: true, signal }), catch: ObjectStorageError.written(object.key) }).pipe(
                    Effect.filterOrFail(
                        ({ interrupted }) => !interrupted,
                        () => ObjectStorageError.of('Refused', object.key)(),
                    ),
                );
                yield* Effect.tryPromise({ try: (signal) => uploads.completeUpload(uploadToken, { now, expectedLength: length, signal }), catch: ObjectStorageError.written(object.key) });
            }),
        };
    });
    static readonly remove = Effect.fn('ObjectStorage.remove')(function* (key: string, uploadToken: Option.Option<string>) {
        const storage = yield* ObjectStorage;
        yield* Option.match(uploadToken, { onNone: () => Effect.void, onSome: (token) => Effect.tryPromise({ try: (signal) => storage.uploads.abortUpload(token, { signal }), catch: ObjectStorageError.of('Unavailable', key) }) });
        yield* storage.delete(key);
    });
    static readonly serve = Effect.fn('ObjectStorage.serve')(function* (object: ServeContext, options: Omit<ServeObjectOptions, 'onError'> = {}) {
        const [{ objects }, request, context] = yield* Effect.all([ObjectStorage, Effect.flatMap(HttpServerRequest.HttpServerRequest, HttpServerRequest.toWeb).pipe(Effect.orDie), Effect.context<never>()]);
        const served = serveObject(objects, { ...options, onError: (error, { operation, key }) => Effect.logError(Cause.fail(ObjectStorageError.of('Unavailable', `${operation} ${key}`)(error))).pipe(Effect.runSyncWith(context)) });
        return HttpServerResponse.fromWeb(yield* Effect.promise(() => served(request, object)));
    });
    static readonly receive = Effect.fn('ObjectStorage.receive')(function* (options: Omit<UploadHandlerOptions, 'onError'>) {
        const [{ uploads }, request, context] = yield* Effect.all([ObjectStorage, Effect.flatMap(HttpServerRequest.HttpServerRequest, HttpServerRequest.toWeb).pipe(Effect.orDie), Effect.context<never>()]);
        const received = createUploadHandler(uploads, { ...options, onError: (error, { operation }) => Effect.logError(Cause.fail(ObjectStorageError.of('Unavailable', operation)(error))).pipe(Effect.runSyncWith(context)) });
        return HttpServerResponse.fromWeb(yield* Effect.promise(() => received(request)));
    });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { ObjectStorage, ObjectStorageError };
