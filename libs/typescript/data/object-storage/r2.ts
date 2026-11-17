import { Effect, Layer } from 'effect';
import { type R2Bucket, r2Store } from 'partial-content/r2';
import { s3Clients } from '#/s3';
import { ObjectStorage } from '#/storage';

// --- [COMPOSITION] ---------------------------------------------------------------------

const r2Storage = ({ bucket, name }: { readonly bucket: R2Bucket; readonly name: string }) =>
    Layer.effect(
        ObjectStorage,
        Effect.map(s3Clients({ forcePathStyle: true }), (clients) => ObjectStorage.make(clients, name, r2Store({ bucket, reservedPrefix: '' }))),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { r2Storage };
