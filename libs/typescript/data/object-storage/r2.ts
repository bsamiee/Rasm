import { ByteSize, Effect, Layer, Option } from 'effect';
import { r2Store } from 'partial-content/r2';
import { ObjectStorage, sent } from '#/storage';

// --- [COMPOSITION] ---------------------------------------------------------------------

const r2Storage = ({ bucket, name }: { readonly bucket: R2Bucket; readonly name: string }) =>
    ObjectStorage.make(name, {
        objects: r2Store({ bucket, reservedPrefix: '' }),
        stat: (key) => sent(key, () => bucket.head(key)).pipe(Effect.map((object) => Option.map(Option.fromNullOr(object), ({ size, httpMetadata }) => ({ size: ByteSize.bytes(size), mime: httpMetadata?.contentType ?? '' })))),
        remove: (key, uploadId) => Effect.transposeOption(Option.map(uploadId, (id) => sent(key, () => bucket.resumeMultipartUpload(key, id).abort()))).pipe(Effect.andThen(sent(key, () => bucket.delete(key)))),
    }).pipe(Layer.effect(ObjectStorage));

// --- [EXPORTS] -------------------------------------------------------------------------

export { r2Storage };
