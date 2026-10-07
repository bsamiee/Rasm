import { displaySize } from '@embedpdf/core-geometry';
import { localEngine } from '@embedpdf/engine';
import { Cause, Effect, Match, Schema, type Scope } from 'effect';
import { Asset, ImageAsset, PdfAsset } from '../model/asset.ts';

// --- [SERVICES] ------------------------------------------------------------------------

const pdfEngine = localEngine();

// --- [OPERATIONS] ----------------------------------------------------------------------

const inspectFile = Effect.fnUntraced(function* (file: Blob, id: string, name: string): Effect.fn.Return<typeof Asset.Type, Cause.UnknownError | Schema.SchemaError, Scope.Scope> {
    const pdfPages = Effect.gen(function* () {
        const bytes = yield* Effect.tryPromise(() => file.arrayBuffer());
        const handle = yield* Effect.acquireRelease(
            Effect.tryPromise(() => pdfEngine.open({ kind: 'bytes', id, bytes }, { scope: ['*'] })),
            (opened) => Effect.promise(() => opened.close()),
        );
        const { pages } = yield* Effect.tryPromise(() => handle.pages.list());
        return { pages: pages.map(({ size, rotation, userUnit, label }) => ({ ...displaySize({ width: size.width * userUnit, height: size.height * userUnit }, rotation), ...(label === null ? {} : { label }) })) };
    });
    const imageSize = Effect.acquireRelease(
        Effect.tryPromise(() => createImageBitmap(file)),
        (bitmap) => Effect.sync(() => bitmap.close()),
    ).pipe(Effect.map(({ width, height }) => ({ width, height })));
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
    const measured = yield* Match.value(file.type).pipe(
        Match.when(Match.is(...PdfAsset.fields.mime.literals), () => pdfPages),
        Match.when(Match.is(...ImageAsset.fields.mime.literals), () => imageSize),
        Match.orElse(() => videoSize),
    );
    return yield* Schema.decodeUnknownEffect(Asset)({ id, name, size: file.size, mime: file.type, ...measured });
}, Effect.scoped);

// --- [EXPORTS] -------------------------------------------------------------------------

export { inspectFile, pdfEngine };
