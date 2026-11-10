import { Effect, Layer } from 'effect';
import { type R2Bucket, type R2UploadStoreOptions, r2Store, r2UploadStore } from 'partial-content/r2';
import { ObjectStorage, ObjectStorageError } from '#/index';

// --- [COMPOSITION] ---------------------------------------------------------------------

const r2Storage = ({ bucket, ...options }: Omit<R2UploadStoreOptions, 'bucket'> & { readonly bucket: R2Bucket & R2UploadStoreOptions['bucket'] }) => {
    const uploads = r2UploadStore({ ...options, bucket });
    return Layer.succeed(ObjectStorage, { objects: r2Store({ bucket, reservedPrefix: uploads.uploadPrefix }), uploads, delete: (key) => Effect.tryPromise({ try: () => bucket.delete(key), catch: ObjectStorageError.of('Unavailable', key) }) });
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { r2Storage };
