import { Effect, Exit, Formatter, flow, Option, Schema, type Scope, Sink, Stream } from 'effect';
import { HttpServerRequest, HttpServerResponse } from 'effect/http';
import { r2Store } from 'partial-content/r2';
import { serveObjectRaw } from 'partial-content/web';

// --- [TYPES] ---------------------------------------------------------------------------

interface StoredObject {
    readonly key: string;
    readonly mime: string;
}

// --- [ERRORS] --------------------------------------------------------------------------

const StorageReason = Schema.TaggedUnion({ Failed: { step: Schema.Literals(['create', 'upload', 'complete', 'abort', 'delete']), key: Schema.String, cause: Schema.Defect() } });
class StorageError extends Schema.TaggedError<StorageError, { readonly brand: unique symbol }>()('StorageError', { reason: StorageReason }) {
    override get message(): string {
        return StorageReason.match(this.reason, { Failed: ({ step, key, cause }) => `R2 ${step} of ${key} failed: ${Formatter.format(cause)}` });
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const call = <A>(step: (typeof StorageReason.Type)['step'], key: string, run: () => Promise<A>): Effect.Effect<A, StorageError> => Effect.tryPromise({ try: run, catch: (cause) => StorageError.make({ reason: { _tag: 'Failed', step, key, cause } }) });
const putObject = Effect.fnUntraced(function* <E, W, S, R>(
    bucket: R2Bucket,
    object: StoredObject & { readonly size: number },
    content: Stream.Stream<Uint8Array, E>,
    { started, onError }: { readonly started: (uploadId: string) => Effect.Effect<void, S, R>; readonly onError: (error: unknown) => W },
): Effect.fn.Return<void, E | W | S | StorageError, R | Scope.Scope> {
    const upload = yield* Effect.acquireRelease(
        call('create', object.key, () => bucket.createMultipartUpload(object.key, { httpMetadata: { contentType: object.mime } })),
        (handle, exit) => (Exit.isSuccess(exit) ? Effect.void : call('abort', object.key, () => handle.abort()).pipe(Effect.catch(Effect.logError))),
    );
    yield* started(upload.uploadId);
    const body = new FixedLengthStream(object.size);
    const [, part] = yield* Effect.all([Stream.run(content, Sink.fromWritableStream({ evaluate: () => body.writable, onError })), call('upload', object.key, () => upload.uploadPart(1, body.readable))], { concurrency: 'unbounded' });
    yield* call('complete', object.key, () => upload.complete([part]));
});
const releaseObject = (bucket: R2Bucket, key: string, uploadId: Option.Option<string>): Effect.Effect<void, StorageError> =>
    Effect.andThen(
        Option.match(uploadId, { onNone: () => Effect.void, onSome: (id) => call('abort', key, () => bucket.resumeMultipartUpload(key, id).abort()) }),
        call('delete', key, () => bucket.delete(key)),
    );
const serveObject = Effect.fnUntraced(function* (bucket: R2Bucket, request: HttpServerRequest.HttpServerRequest, object: StoredObject) {
    const requested = yield* HttpServerRequest.toWeb(request);
    const run = Effect.runSyncWith(yield* Effect.context());
    const response = yield* Effect.promise(() => serveObjectRaw(r2Store({ bucket, reservedPrefix: '' }), { disposition: 'inline', securityHeaders: () => ({ 'X-Content-Type-Options': 'nosniff' }), onError: flow(Effect.logError, run) })(requested, object));
    return HttpServerResponse.raw(response.body instanceof ReadableStream ? response.body.pipeThrough(new FixedLengthStream(Number(response.headers['Content-Length']))) : response.body, response);
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { putObject, releaseObject, serveObject };
