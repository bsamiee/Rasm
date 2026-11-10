import { DeleteObjectCommand, S3Client, type S3ClientConfig } from '@aws-sdk/client-s3';
import { Effect, Layer } from 'effect';
import { type S3UploadStoreOptions, s3Store, s3UploadStore } from 'partial-content/s3';
import { ObjectStorage, ObjectStorageError } from '#/index';

// --- [COMPOSITION] ---------------------------------------------------------------------

const s3Storage = Effect.fnUntraced(function* ({ bucket, ...options }: Omit<S3UploadStoreOptions, 'client'>, config: S3ClientConfig) {
    const client = yield* Effect.acquireRelease(
        Effect.sync(() => new S3Client(config)),
        (opened) => Effect.sync(() => opened.destroy()),
    );
    const uploads = s3UploadStore({ ...options, client, bucket });
    return {
        objects: s3Store({ client, bucket, reservedPrefix: uploads.uploadPrefix }),
        uploads,
        delete: (key: string) => Effect.tryPromise({ try: (signal) => client.send(new DeleteObjectCommand({ Bucket: bucket, Key: key }), { abortSignal: signal }), catch: ObjectStorageError.of('Unavailable', key) }).pipe(Effect.asVoid),
    };
}, Layer.effect(ObjectStorage));

// --- [EXPORTS] -------------------------------------------------------------------------

export { s3Storage };
