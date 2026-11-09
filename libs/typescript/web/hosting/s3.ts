import { DeleteObjectCommand, S3Client, type S3ClientConfig } from '@aws-sdk/client-s3';
import { Effect, Layer } from 'effect';
import { s3Store, s3UploadStore } from 'partial-content/s3';
import { HostingError } from '#/error';
import { ObjectStorage } from '#/object-storage';

// --- [OPERATIONS] ----------------------------------------------------------------------

const s3Storage = Effect.fnUntraced(function* (bucket: string, options: S3ClientConfig) {
    const client = yield* Effect.acquireRelease(
        Effect.sync(() => new S3Client(options)),
        (opened) => Effect.sync(() => opened.destroy()),
    );
    const uploads = s3UploadStore({ client, bucket });
    return {
        objects: s3Store({ client, bucket, reservedPrefix: uploads.uploadPrefix }),
        uploads,
        delete: (key: string) => Effect.tryPromise({ try: (signal) => client.send(new DeleteObjectCommand({ Bucket: bucket, Key: key }), { abortSignal: signal }), catch: HostingError.storage('delete', key) }).pipe(Effect.asVoid),
    };
}, Layer.effect(ObjectStorage));

// --- [EXPORTS] -------------------------------------------------------------------------

export { s3Storage };
