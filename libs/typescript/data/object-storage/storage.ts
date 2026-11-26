import { AwsClient } from 'aws4fetch';
import { ByteSize, Cause, Config, Context, Effect, Formatter, Option, Redacted, Schema } from 'effect';
import { HttpServerRequest, HttpServerResponse } from 'effect/http';
import type { ObjectStore } from 'partial-content';
import { type ServeContext, type ServeObjectOptions, serveObject } from 'partial-content/web';

// --- [CONSTANTS] -----------------------------------------------------------------------

const access = Config.all({
    endpoint: Config.String('AWS_ENDPOINT_URL_S3'),
    published: Config.option(Config.String('S3_PUBLIC_ENDPOINT_URL')),
    region: Config.String('AWS_REGION'),
    accessKeyId: Config.Redacted('AWS_ACCESS_KEY_ID'),
    secretAccessKey: Config.Redacted('AWS_SECRET_ACCESS_KEY'),
});

// --- [MODELS] --------------------------------------------------------------------------

const Presignable = Schema.Union([
    Schema.Struct({ _tag: Schema.tagDefaultOmit('UploadPart'), method: Schema.tag('PUT'), key: Schema.String, uploadId: Schema.String, partNumber: Schema.Int.check(Schema.isBetween({ minimum: 1, maximum: 10_000 })) }),
    Schema.Struct({ _tag: Schema.tagDefaultOmit('PutObject'), method: Schema.tag('PUT'), key: Schema.String }),
    Schema.Struct({ _tag: Schema.tagDefaultOmit('CompleteMultipartUpload'), method: Schema.tag('POST'), key: Schema.String, uploadId: Schema.String }),
    Schema.Struct({ _tag: Schema.tagDefaultOmit('CreateMultipartUpload'), method: Schema.tag('POST'), key: Schema.String }),
    Schema.Struct({ _tag: Schema.tagDefaultOmit('ListParts'), method: Schema.tag('GET'), key: Schema.String, uploadId: Schema.String }),
    Schema.Struct({ _tag: Schema.tagDefaultOmit('AbortMultipartUpload'), method: Schema.tag('DELETE'), key: Schema.String, uploadId: Schema.String }),
]).pipe(Schema.toTaggedUnion('_tag'));
class Presigned extends Schema.Opaque<Presigned>()(Schema.Struct({ url: Schema.String, headers: Schema.Record(Schema.String, Schema.String) })) {}

// --- [ERRORS] --------------------------------------------------------------------------

class ObjectStorageError extends Schema.TaggedError<ObjectStorageError, { readonly brand: unique symbol }>()('ObjectStorageError', {
    reason: Schema.TaggedUnion({ Unavailable: {} }),
    subject: Schema.String,
    cause: Schema.optionalKey(Schema.Defect()),
    message: Schema.String,
}) {
    static readonly of = (reason: ObjectStorageError['reason']['_tag'], subject: string) => (cause?: unknown) =>
        ObjectStorageError.make({ reason: { _tag: reason }, subject, ...(cause === undefined ? {} : { cause }), message: [`${reason}: ${subject}`, ...(cause === undefined ? [] : [Formatter.format(cause)])].join(': ') });
}

// --- [SERVICES] ------------------------------------------------------------------------

class ObjectStorage extends Context.Service<ObjectStorage>()('@rasm/object-storage/storage/ObjectStorage', {
    make: Effect.fnUntraced(function* (
        bucket: string,
        held: {
            readonly objects: ObjectStore;
            readonly stat: (key: string) => Effect.Effect<Option.Option<{ readonly size: ByteSize.ByteSize; readonly mime: string }>, ObjectStorageError>;
            readonly remove: (key: string, uploadId: Option.Option<string>) => Effect.Effect<void, ObjectStorageError>;
        },
    ) {
        const { endpoint, published, region, accessKeyId, secretAccessKey } = yield* access;
        const signer = new AwsClient({ accessKeyId: Redacted.value(accessKeyId), secretAccessKey: Redacted.value(secretAccessKey), service: 's3', region });
        const stated = Option.getOrElse(published, () => endpoint);
        const resolved = Effect.flatMap(Effect.serviceOption(HttpServerRequest.HttpServerRequest), (request) => Effect.fromNullishOr(URL.parse(stated, Option.getOrUndefined(Option.flatMap(request, HttpServerRequest.toURL))))).pipe(Effect.mapError(ObjectStorageError.of('Unavailable', `endpoint ${stated}`)));
        return {
            ...held,
            endpoint: resolved,
            sign: ({ size, mime }: { readonly size: ByteSize.ByteSize; readonly mime: string }) =>
                Effect.fnUntraced(function* (target: typeof Presignable.Type) {
                    const created = { 'If-None-Match': '*' };
                    const added = Presignable.matchOrElse(
                        target,
                        {
                            UploadPart: ({ uploadId, partNumber }) => ({ query: { partNumber: `${partNumber}`, uploadId } }),
                            PutObject: () => ({ headers: { 'Content-Type': mime, ...created }, signed: { 'Content-Length': `${ByteSize.toNumberUnsafe(size)}` } }),
                            CompleteMultipartUpload: ({ uploadId }) => ({ query: { uploadId }, headers: created }),
                            CreateMultipartUpload: () => ({ query: { uploads: '' }, headers: { 'Content-Type': mime } }),
                        },
                        ({ uploadId }) => ({ query: { uploadId } }),
                    );
                    const { query, headers, signed } = { query: {}, headers: {}, signed: {}, ...added };
                    const at = yield* resolved;
                    const { url } = yield* sent(target.key, () => signer.sign(Object.assign(new URL(`${bucket}/${target.key}`, at.href.replace(/\/?$/u, '/')), { search: new URLSearchParams(query).toString() }), { method: target.method, headers: { ...headers, ...signed }, aws: { signQuery: true, allHeaders: true } }));
                    return { url, headers };
                }),
        };
    }),
}) {
    static readonly remove = (key: string, uploadId: Option.Option<string>) => ObjectStorage.use((storage) => storage.remove(key, uploadId));
    static readonly serve = Effect.fn('ObjectStorage.serve')(function* (object: ServeContext, options: Omit<ServeObjectOptions, 'onError'> = {}) {
        const [{ objects }, request, context] = yield* Effect.all([ObjectStorage, Effect.flatMap(HttpServerRequest.HttpServerRequest, HttpServerRequest.toWeb).pipe(Effect.orDie), Effect.context<never>()]);
        const served = serveObject(objects, { ...options, onError: (error, { operation, key }) => Effect.runSyncWith(context)(Effect.logError(Cause.fail(ObjectStorageError.of('Unavailable', `${operation} ${key}`)(error)))) });
        return HttpServerResponse.fromWeb(yield* Effect.promise(() => served(request, object)));
    });
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const sent = <A>(key: string, send: (signal: AbortSignal) => Promise<A>) => Effect.tryPromise({ try: send, catch: ObjectStorageError.of('Unavailable', key) });

// --- [EXPORTS] -------------------------------------------------------------------------

export { access, ObjectStorage, ObjectStorageError, Presignable, Presigned, sent };
