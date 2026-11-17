import { AbortMultipartUploadCommand, CompleteMultipartUploadCommand, CreateMultipartUploadCommand, DeleteObjectCommand, HeadObjectCommand, ListPartsCommand, NoSuchUpload, NotFound, PutObjectCommand, type S3Client, S3ServiceException, UploadPartCommand } from '@aws-sdk/client-s3';
import { getSignedUrl } from '@aws-sdk/s3-request-presigner';
import { ByteSize, Cause, Context, Effect, Formatter, Match, Option, Schema } from 'effect';
import { HttpServerRequest, HttpServerResponse } from 'effect/http';
import type { ObjectStore } from 'partial-content';
import { type ServeContext, type ServeObjectOptions, serveObject } from 'partial-content/web';

// --- [MODELS] --------------------------------------------------------------------------

const Presignable = Schema.Union([
    Schema.Struct({ method: Schema.Literal('PUT'), key: Schema.String, uploadId: Schema.String, partNumber: Schema.Int.check(Schema.isBetween({ minimum: 1, maximum: 10_000 })) }),
    Schema.Struct({ method: Schema.Literal('PUT'), key: Schema.String }),
    Schema.Struct({ method: Schema.Literal('POST'), key: Schema.String, uploadId: Schema.String }),
    Schema.Struct({ method: Schema.Literal('POST'), key: Schema.String }),
    Schema.Struct({ method: Schema.Literal('GET'), key: Schema.String, uploadId: Schema.String }),
    Schema.Struct({ method: Schema.Literal('DELETE'), key: Schema.String, uploadId: Schema.String }),
]);
const Presigned = Schema.Struct({ url: Schema.String, headers: Schema.Record(Schema.String, Schema.String) });

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
        readonly origin: string;
        readonly sign: (request: typeof Presignable.Type, stated: { readonly size: ByteSize.ByteSize; readonly mime: string }) => Effect.Effect<typeof Presigned.Type, ObjectStorageError>;
        readonly stat: (key: string) => Effect.Effect<Option.Option<{ readonly size: ByteSize.ByteSize; readonly mime: string }>, ObjectStorageError>;
        readonly remove: (key: string, uploadId: Option.Option<string>) => Effect.Effect<void, ObjectStorageError>;
    }
>()('@rasm/object-storage/storage/ObjectStorage') {
    static readonly make = ({ client, signer, origin }: { readonly client: S3Client; readonly signer: S3Client; readonly origin: string }, bucket: string, objects: ObjectStore): ObjectStorage['Service'] => {
        const created = { 'If-None-Match': '*' };
        return {
            objects,
            origin,
            sign: (request, { size, mime }) =>
                Match.value(request).pipe(
                    Match.when({ method: 'PUT', uploadId: Schema.is(Schema.String) }, ({ key, uploadId, partNumber }) => presigned(key, () => getSignedUrl(signer, new UploadPartCommand({ Bucket: bucket, Key: key, UploadId: uploadId, PartNumber: partNumber })))),
                    Match.when({ method: 'PUT' }, ({ key }) => presigned(key, () => getSignedUrl(signer, new PutObjectCommand({ Bucket: bucket, Key: key, ContentType: mime, ContentLength: ByteSize.toNumberUnsafe(size), IfNoneMatch: '*' })), { 'Content-Type': mime, ...created })),
                    Match.when({ method: 'POST', uploadId: Schema.is(Schema.String) }, ({ key, uploadId }) => presigned(key, () => getSignedUrl(signer, new CompleteMultipartUploadCommand({ Bucket: bucket, Key: key, UploadId: uploadId, IfNoneMatch: '*' })), created)),
                    Match.when({ method: 'POST' }, ({ key }) => presigned(key, () => getSignedUrl(signer, new CreateMultipartUploadCommand({ Bucket: bucket, Key: key, ContentType: mime })), { 'Content-Type': mime })),
                    Match.when({ method: 'GET' }, ({ key, uploadId }) => presigned(key, () => getSignedUrl(signer, new ListPartsCommand({ Bucket: bucket, Key: key, UploadId: uploadId })))),
                    Match.when({ method: 'DELETE' }, ({ key, uploadId }) => presigned(key, () => getSignedUrl(signer, new AbortMultipartUploadCommand({ Bucket: bucket, Key: key, UploadId: uploadId })))),
                    Match.exhaustive,
                ),
            stat: (key) =>
                sent(key, (abortSignal) => client.send(new HeadObjectCommand({ Bucket: bucket, Key: key }), { abortSignal })).pipe(
                    Effect.map(({ ContentLength = 0, ContentType = '' }) => Option.some({ size: ByteSize.bytes(ContentLength), mime: ContentType })),
                    Effect.catchIf(
                        ({ cause }) => cause instanceof NotFound || (cause instanceof S3ServiceException && cause.$metadata.httpStatusCode === 404),
                        () => Effect.succeedNone,
                    ),
                ),
            remove: (key, uploadId) =>
                Option.match(uploadId, { onNone: () => Effect.void, onSome: (UploadId) => sent(key, (abortSignal) => client.send(new AbortMultipartUploadCommand({ Bucket: bucket, Key: key, UploadId }), { abortSignal })).pipe(Effect.catch(absent)) }).pipe(
                    Effect.andThen(sent(key, (abortSignal) => client.send(new DeleteObjectCommand({ Bucket: bucket, Key: key }), { abortSignal }))),
                    Effect.asVoid,
                ),
        };
    };
    static readonly remove = (key: string, uploadId: Option.Option<string>) => ObjectStorage.use((storage) => storage.remove(key, uploadId));
    static readonly serve = Effect.fn('ObjectStorage.serve')(function* (object: ServeContext, options: Omit<ServeObjectOptions, 'onError'> = {}) {
        const [{ objects }, request, context] = yield* Effect.all([ObjectStorage, Effect.flatMap(HttpServerRequest.HttpServerRequest, HttpServerRequest.toWeb).pipe(Effect.orDie), Effect.context<never>()]);
        const served = serveObject(objects, { ...options, onError: (error, { operation, key }) => Effect.runSyncWith(context)(Effect.logError(Cause.fail(ObjectStorageError.of('Unavailable', `${operation} ${key}`)(error)))) });
        return HttpServerResponse.fromWeb(yield* Effect.promise(() => served(request, object)));
    });
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const sent = <A>(key: string, send: (signal: AbortSignal) => Promise<A>) => Effect.tryPromise({ try: send, catch: ObjectStorageError.of('Unavailable', key) });
const presigned = (key: string, sign: () => Promise<string>, headers: Readonly<Record<string, string>> = {}) => Effect.map(sent(key, sign), (url) => ({ url, headers }));
const absent = (error: ObjectStorageError) => (error.cause instanceof NotFound || error.cause instanceof NoSuchUpload ? Effect.void : Effect.fail(error));

// --- [EXPORTS] -------------------------------------------------------------------------

export { ObjectStorage, ObjectStorageError, Presignable, Presigned };
