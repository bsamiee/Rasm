import { AwsClient } from 'aws4fetch';
import { ByteSize, Cause, Config, Context, Effect, Formatter, Match, Option, Redacted, Schema } from 'effect';
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
    Schema.Struct({ method: Schema.Literal('PUT'), key: Schema.String, uploadId: Schema.String, partNumber: Schema.Int.check(Schema.isBetween({ minimum: 1, maximum: 10_000 })) }),
    Schema.Struct({ method: Schema.Literal('PUT'), key: Schema.String }),
    Schema.Struct({ method: Schema.Literal('POST'), key: Schema.String, uploadId: Schema.String }),
    Schema.Struct({ method: Schema.Literal('POST'), key: Schema.String }),
    Schema.Struct({ method: Schema.Literal('GET'), key: Schema.String, uploadId: Schema.String }),
    Schema.Struct({ method: Schema.Literal('DELETE'), key: Schema.String, uploadId: Schema.String }),
]);
class Presigned extends Schema.Opaque<Presigned, unknown>()(Schema.Struct({ url: Schema.String, headers: Schema.Record(Schema.String, Schema.String) })) {}

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

class ObjectStorage extends Context.Service<
    ObjectStorage,
    {
        readonly objects: ObjectStore;
        readonly endpoint: Effect.Effect<URL, ObjectStorageError>;
        readonly sign: (request: typeof Presignable.Type, stated: { readonly size: ByteSize.ByteSize; readonly mime: string }) => Effect.Effect<Presigned, ObjectStorageError>;
        readonly stat: (key: string) => Effect.Effect<Option.Option<{ readonly size: ByteSize.ByteSize; readonly mime: string }>, ObjectStorageError>;
        readonly remove: (key: string, uploadId: Option.Option<string>) => Effect.Effect<void, ObjectStorageError>;
    }
>()('@rasm/object-storage/storage/ObjectStorage') {
    static readonly make = Effect.fnUntraced(function* (bucket: string, held: Pick<ObjectStorage['Service'], 'objects' | 'stat' | 'remove'>) {
        const { endpoint, published, region, accessKeyId, secretAccessKey } = yield* access;
        const signer = new AwsClient({ accessKeyId: Redacted.value(accessKeyId), secretAccessKey: Redacted.value(secretAccessKey), service: 's3', region });
        const stated = Option.getOrElse(published, () => endpoint);
        const resolved = Effect.flatMap(Effect.serviceOption(HttpServerRequest.HttpServerRequest), (request) => Effect.fromNullishOr(URL.parse(stated, Option.getOrUndefined(Option.flatMap(request, HttpServerRequest.toURL))))).pipe(Effect.mapError(ObjectStorageError.of('Unavailable', `endpoint ${stated}`)));
        return {
            ...held,
            endpoint: resolved,
            sign: (request, { size, mime }) => {
                const created = { 'If-None-Match': '*' };
                const { query, headers, signed } = Match.value(request).pipe(
                    Match.when({ method: 'PUT', uploadId: Schema.is(Schema.String) }, ({ uploadId, partNumber }) => ({ query: { partNumber: `${partNumber}`, uploadId }, headers: {}, signed: {} })),
                    Match.when({ method: 'PUT' }, () => ({ query: {}, headers: { 'Content-Type': mime, ...created }, signed: { 'Content-Length': `${ByteSize.toNumberUnsafe(size)}` } })),
                    Match.when({ method: 'POST', uploadId: Schema.is(Schema.String) }, ({ uploadId }) => ({ query: { uploadId }, headers: created, signed: {} })),
                    Match.when({ method: 'POST' }, () => ({ query: { uploads: '' }, headers: { 'Content-Type': mime }, signed: {} })),
                    Match.when({ method: 'GET' }, ({ uploadId }) => ({ query: { uploadId }, headers: {}, signed: {} })),
                    Match.when({ method: 'DELETE' }, ({ uploadId }) => ({ query: { uploadId }, headers: {}, signed: {} })),
                    Match.exhaustive,
                );
                return Effect.flatMap(resolved, (at) =>
                    sent(request.key, () => signer.sign(Object.assign(new URL(`${bucket}/${request.key}`, at.href.replace(/\/?$/u, '/')), { search: new URLSearchParams(query).toString() }), { method: request.method, headers: { ...headers, ...signed }, aws: { signQuery: true, allHeaders: true } })),
                ).pipe(Effect.map(({ url }) => ({ url, headers })));
            },
        } satisfies ObjectStorage['Service'];
    });
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
