import { Array, Cause, Effect, Match, Option, Schema, type Scope, Struct } from 'effect';
import { Asset, ImageAsset, PdfAsset } from '../model/asset.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

const inspectFile = Effect.fnUntraced(function* (file: Blob, id: string, name: string, mime: string): Effect.fn.Return<{ readonly asset: typeof Asset.Type; readonly renditions: File[] }, Cause.UnknownError | Schema.SchemaError, Scope.Scope> {
    const image = Effect.gen(function* () {
        const source = yield* Effect.acquireRelease(
            Effect.tryPromise(() => createImageBitmap(file)),
            (bitmap) => Effect.sync(() => bitmap.close()),
        );
        const minimumPreviewWidth = 256;
        const previews = yield* Effect.forEach(
            Array.unfold(Math.floor(source.width / 2), (width) => (width >= minimumPreviewWidth ? Option.some([width, Math.floor(width / 2)] as const) : Option.none())),
            Effect.fnUntraced(function* (width: number) {
                const bitmap = yield* Effect.acquireRelease(
                    Effect.tryPromise(() => createImageBitmap(source, { resizeWidth: width, resizeQuality: 'high' })),
                    (resized) => Effect.sync(resized.close.bind(resized)),
                );
                const canvas = new OffscreenCanvas(bitmap.width, bitmap.height);
                const context = canvas.getContext('2d');
                if (context === null) {
                    return yield* Effect.fail(new Cause.UnknownError('The browser could not create an image preview.'));
                }
                context.drawImage(bitmap, 0, 0);
                const blob = yield* Effect.tryPromise(() => canvas.convertToBlob({ type: mime === 'image/jpeg' ? mime : 'image/png' }));
                return blob.size < file.size ? Option.some({ width: bitmap.width, file: new File([blob], String(bitmap.width), { type: blob.type }) }) : Option.none();
            }, Effect.scoped),
        ).pipe(Effect.map(Array.getSomes));
        return { metadata: { width: source.width, height: source.height, ...(Array.isArrayNonEmpty(previews) && { renditions: previews.map((preview) => ({ width: preview.width, size: preview.file.size, mime: preview.file.type })) }) }, renditions: previews.map(Struct.get('file')) };
    });
    const videoSize = Effect.gen(function* () {
        const { element, url } = yield* Effect.acquireRelease(
            Effect.sync(() => ({ element: Object.assign(document.createElement('video'), { preload: 'metadata' }), url: URL.createObjectURL(file) })),
            (video) =>
                Effect.sync(() => {
                    video.element.removeAttribute('src');
                    video.element.load();
                    URL.revokeObjectURL(video.url);
                }),
        );
        return yield* Effect.callback<{ width: number; height: number }, Cause.UnknownError>((resume, signal) => {
            element.addEventListener('loadedmetadata', () => resume(Effect.succeed({ width: element.videoWidth, height: element.videoHeight })), { once: true, signal });
            element.addEventListener('error', () => resume(Effect.fail(new Cause.UnknownError(element.error, 'This browser cannot play the video. Export it as H.264 MP4 or VP9 WebM.'))), { once: true, signal });
            element.src = url;
        });
    });
    const measured = yield* Match.value(mime).pipe(
        Match.when(Match.is(...PdfAsset.fields.mime.literals), () =>
            Effect.tryPromise(() => import('./engine.ts')).pipe(
                Effect.flatMap(({ inspectPdf }) => inspectPdf(file)),
                Effect.map((metadata) => ({ metadata, renditions: [] })),
            ),
        ),
        Match.when(Match.is(...ImageAsset.fields.mime.literals), () => image),
        Match.orElse(() => videoSize.pipe(Effect.map((metadata) => ({ metadata, renditions: [] })))),
    );
    const asset = yield* Schema.decodeUnknownEffect(Asset)({ id, name, size: file.size, mime, ...measured.metadata });
    return { asset, renditions: measured.renditions };
}, Effect.scoped);

// --- [EXPORTS] -------------------------------------------------------------------------

export { inspectFile };
