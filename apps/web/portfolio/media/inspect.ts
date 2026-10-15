import { imageRenditions, videoDimensions } from '@rasm/media/inspect.ts';
import { PrepareError } from '@rasm/media/uploads.ts';
import { Array, Effect, Match, Schema, Struct } from 'effect';
import { Asset, ImageAsset, PdfAsset } from '../model/asset.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

const inspectFile = Effect.fnUntraced(function* (file: Blob, id: string, name: string, mime: string) {
    const measured = yield* Match.value(mime).pipe(
        Match.when(Match.is(...PdfAsset.fields.mime.literals), () =>
            Effect.tryPromise({ try: () => import('@rasm/media/pdf.ts'), catch: (cause) => PrepareError.make({ reason: { _tag: 'Unloaded', cause } }) }).pipe(
                Effect.flatMap(({ inspectPdf }) => inspectPdf(file)),
                Effect.map((metadata) => ({ metadata, renditions: [] })),
            ),
        ),
        Match.when(Match.is(...ImageAsset.fields.mime.literals), () =>
            imageRenditions(file, { minimumWidth: 256, type: mime === 'image/jpeg' ? mime : 'image/png' }).pipe(
                Effect.map(({ width, height, renditions }) => ({
                    metadata: { width, height, ...(Array.isReadonlyArrayNonEmpty(renditions) && { renditions: renditions.map(({ width: rendition, file: preview }) => ({ width: rendition, size: preview.size, mime: preview.type })) }) },
                    renditions: renditions.map(Struct.get('file')),
                })),
            ),
        ),
        Match.orElse(() => videoDimensions(file).pipe(Effect.map((metadata) => ({ metadata, renditions: [] })))),
    );
    const asset = yield* Schema.decodeUnknownEffect(Asset)({ id, name, size: file.size, mime, ...measured.metadata });
    return { asset, renditions: measured.renditions };
}, Effect.scoped);

// --- [EXPORTS] -------------------------------------------------------------------------

export { inspectFile };
