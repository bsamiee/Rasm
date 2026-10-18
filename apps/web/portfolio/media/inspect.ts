import { imageRenditions, videoDimensions } from '@rasm/media/inspect.ts';
import { Cause, Effect, Match, Schema, Struct } from 'effect';
import { Asset, ImageAsset, PdfAsset, VideoAsset } from '../model/asset.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

const inspectFile = Effect.fn('inspectFile')(
    function* ({ data, id, name, mime }: { readonly data: Blob; readonly id: string; readonly name: string; readonly mime: string }) {
        const { renditions, ...metadata } = yield* Match.value(mime).pipe(
            Match.when(Match.is(...PdfAsset.fields.mime.literals), () =>
                Effect.tryPromise(() => import('@rasm/media/pdf.ts')).pipe(
                    Effect.flatMap(({ inspectPdf }) => inspectPdf(data)),
                    Effect.map((measured) => ({ ...measured, renditions: [] })),
                ),
            ),
            Match.when(Match.is(...ImageAsset.fields.mime.literals), () => imageRenditions(data, { minimumWidth: 256, type: mime === 'image/jpeg' ? mime : 'image/png' })),
            Match.orElse(() => videoDimensions(data).pipe(Effect.map((measured) => ({ ...measured, renditions: [] })))),
        );
        return {
            asset: yield* Schema.decodeUnknownEffect(Asset)({ id, name, size: data.size, mime, ...metadata, ...(renditions.length > 0 && { renditions: renditions.map((rendition) => ({ width: rendition.width, size: rendition.file.size, mime: rendition.file.type })) }) }),
            renditions: renditions.map(Struct.get('file')),
        };
    },
    (effect, { mime }) =>
        Effect.mapError(
            effect,
            (error) =>
                new Cause.UnknownError(
                    error,
                    Match.value({ error, mime }).pipe(
                        Match.when({ error: { _tag: 'InspectError', reason: { _tag: 'Protected' } } }, () => 'This PDF is password protected and was not added. Remove it and add an unlocked export.'),
                        Match.when({ error: { _tag: 'InspectError' }, mime: Match.is(...PdfAsset.fields.mime.literals) }, () => 'This PDF could not be read and was not added. Remove it and add a fresh PDF export.'),
                        Match.when({ error: { _tag: 'InspectError' }, mime: Match.is(...VideoAsset.fields.mime.literals) }, () => 'This browser cannot play the video, so it was not added. Remove it, export it as H.264 MP4 or VP9 WebM, and add the export.'),
                        Match.when({ error: { _tag: 'InspectError' } }, () => 'This image could not be read and was not added. Remove it and add a fresh export.'),
                        Match.orElse(() => 'This file could not be read and was not added. Retry it, or remove it and add it again.'),
                    ),
                ),
        ),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { inspectFile };
