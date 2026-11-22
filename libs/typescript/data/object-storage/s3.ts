import { AbortMultipartUploadCommand, DeleteObjectCommand, HeadObjectCommand, NoSuchUpload, NotFound, S3Client, S3ServiceException } from '@aws-sdk/client-s3';
import { ByteSize, Effect, Layer, Option, Redacted } from 'effect';
import { s3Store } from 'partial-content/s3';
import { access, ObjectStorage, sent } from '#/storage.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const s3Storage = (bucket: string) =>
    Layer.effect(
        ObjectStorage,
        Effect.gen(function* () {
            const { endpoint, region, accessKeyId, secretAccessKey } = yield* access;
            const client = yield* Effect.acquireRelease(
                Effect.sync(() => new S3Client({ forcePathStyle: true, requestChecksumCalculation: 'WHEN_REQUIRED', endpoint, region, credentials: { accessKeyId: Redacted.value(accessKeyId), secretAccessKey: Redacted.value(secretAccessKey) } })),
                (opened) => Effect.sync(() => opened.destroy()),
            );
            return yield* ObjectStorage.make(bucket, {
                objects: s3Store({ client, bucket, reservedPrefix: '' }),
                stat: (key) =>
                    sent(key, (abortSignal) => client.send(new HeadObjectCommand({ Bucket: bucket, Key: key }), { abortSignal })).pipe(
                        Effect.map(({ ContentLength = 0, ContentType = '' }) => Option.some({ size: ByteSize.bytes(ContentLength), mime: ContentType })),
                        Effect.catchIf(
                            ({ cause }) => cause instanceof NotFound || (cause instanceof S3ServiceException && cause.$metadata.httpStatusCode === 404),
                            () => Effect.succeedNone,
                        ),
                    ),
                remove: (key, uploadId) =>
                    Option.match(uploadId, {
                        onNone: () => Effect.void,
                        onSome: (UploadId) =>
                            sent(key, (abortSignal) => client.send(new AbortMultipartUploadCommand({ Bucket: bucket, Key: key, UploadId }), { abortSignal })).pipe(
                                Effect.catchIf(
                                    ({ cause }) => cause instanceof NotFound || cause instanceof NoSuchUpload,
                                    () => Effect.void,
                                ),
                            ),
                    }).pipe(Effect.andThen(sent(key, (abortSignal) => client.send(new DeleteObjectCommand({ Bucket: bucket, Key: key }), { abortSignal }))), Effect.asVoid),
            });
        }),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { s3Storage };
