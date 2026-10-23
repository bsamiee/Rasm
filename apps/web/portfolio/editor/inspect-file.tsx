import { imageRenditions, videoDimensions } from '@rasm/media';
import { Cause, Effect, Match, Schema, Struct } from 'effect';
import { Asset, ImageAsset, PdfAsset, VideoAsset } from '#/model/asset';

// --- [OPERATIONS] ----------------------------------------------------------------------

const inspectFile = Effect.fn('inspectFile')(
    function* ({ data, id, name, mime }: { readonly data: Blob; readonly id: string; readonly name: string; readonly mime: string }) {
        const { renditions, ...metadata } = yield* Match.value(mime).pipe(
            Match.when(Match.is(...PdfAsset.fields.mime.literals), () =>
                Effect.tryPromise(() => import('@rasm/media/pdf')).pipe(
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
                        Match.when({ error: { _tag: 'InspectError', reason: { _tag: 'Protected' } } }, () => 'PDF is password protected. Remove it and add unlocked export'),
                        Match.when({ error: { _tag: 'InspectError' }, mime: Match.is(...PdfAsset.fields.mime.literals) }, () => 'Browser cannot read PDF. Remove it and add new export'),
                        Match.when({ error: { _tag: 'InspectError' }, mime: Match.is(...VideoAsset.fields.mime.literals) }, () => 'Browser cannot play video. Remove it and add H.264 MP4 or VP9 WebM export'),
                        Match.when({ error: { _tag: 'InspectError' } }, () => 'Browser cannot read image. Remove it and add new export'),
                        Match.orElse(() => 'Browser cannot read file. Retry or add it again'),
                    ),
                ),
        ),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { inspectFile };
