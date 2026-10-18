import { imageRenditions, videoDimensions } from '@rasm/media/inspect.ts';
import { Effect, Match, Schema, Struct } from 'effect';
import { Asset, ImageAsset, PdfAsset } from '../model/asset.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

const inspectFile = Effect.fn('inspectFile')(function* (file: Blob, id: string, name: string, mime: string) {
    const measured = yield* Match.value(mime).pipe(
        Match.when(Match.is(...PdfAsset.fields.mime.literals), () =>
            Effect.tryPromise(() => import('@rasm/media/pdf.ts')).pipe(
                Effect.flatMap(({ inspectPdf }) => inspectPdf(file)),
                Effect.map((metadata) => ({ metadata, renditions: [] })),
            ),
        ),
        Match.when(Match.is(...ImageAsset.fields.mime.literals), () =>
            imageRenditions(file, { minimumWidth: 256, type: mime === 'image/jpeg' ? mime : 'image/png' }).pipe(
                Effect.map(({ width, height, renditions }) => ({ metadata: { width, height, ...(renditions.length > 0 && { renditions: renditions.map(Struct.omit(['file'])) }) }, renditions: renditions.map(Struct.get('file')) })),
            ),
        ),
        Match.orElse(() => videoDimensions(file).pipe(Effect.map((metadata) => ({ metadata, renditions: [] })))),
    );
    return { asset: yield* Schema.decodeUnknownEffect(Asset)({ id, name, size: file.size, mime, ...measured.metadata }), renditions: measured.renditions };
}, Effect.scoped);

// --- [EXPORTS] -------------------------------------------------------------------------

export { inspectFile };
