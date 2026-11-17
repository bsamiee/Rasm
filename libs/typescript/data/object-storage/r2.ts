import { ByteSize, Effect, Layer, Option } from 'effect';
import { r2Store } from 'partial-content/r2';
import { ObjectStorage, sent } from '#/storage';

// --- [COMPOSITION] ---------------------------------------------------------------------

const r2Storage = ({ bucket, name }: { readonly bucket: R2Bucket; readonly name: string }) =>
    Layer.effect(
        ObjectStorage,
        ObjectStorage.make(name, {
            objects: r2Store({ bucket, reservedPrefix: '' }),
            stat: (key) =>
                Effect.map(
                    sent(key, () => bucket.head(key)),
                    (object) => Option.map(Option.fromNullOr(object), ({ size, httpMetadata }) => ({ size: ByteSize.bytes(size), mime: httpMetadata?.contentType ?? '' })),
                ),
            remove: (key, uploadId) => Option.match(uploadId, { onNone: () => Effect.void, onSome: (id) => sent(key, () => bucket.resumeMultipartUpload(key, id).abort()) }).pipe(Effect.andThen(sent(key, () => bucket.delete(key)))),
        }),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { r2Storage };
