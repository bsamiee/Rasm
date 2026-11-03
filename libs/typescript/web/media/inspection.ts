import { Array, Data, Duration, Effect, Match, Option, String } from 'effect';
import { MediaError } from '#/error';

// --- [TYPES] ---------------------------------------------------------------------------

type Inspection = Data.TaggedEnum<{
    Image: { readonly width: number; readonly height: number; readonly renditions: readonly { readonly width: number; readonly file: File }[] };
    Video: { readonly width: number; readonly height: number; readonly duration: Duration.Duration };
    Pdf: { readonly pages: Array.NonEmptyReadonlyArray<{ readonly width: number; readonly height: number; readonly label?: string }> };
}>;
type Options = ImageEncodeOptions & { readonly minimumWidth: number; readonly ratio: number; readonly concurrency: number };

// --- [OPERATIONS] ----------------------------------------------------------------------

const { $is, $match, Image, Video, Pdf } = Data.taggedEnum<Inspection>();
const bitmap = (decode: () => Promise<ImageBitmap>, fail: (cause: unknown) => MediaError) => Effect.acquireRelease(Effect.tryPromise({ try: decode, catch: fail }), (opened) => Effect.sync(() => opened.close()));
const image = Effect.fnUntraced(function* (file: File, { minimumWidth, ratio, concurrency, ...encoding }: Options) {
    const unsupported = MediaError.of('Unsupported', file.name);
    const source = yield* bitmap(() => createImageBitmap(file), MediaError.of('Unreadable', file.name));
    const renditions = yield* Effect.forEach(
        Array.unfold(Math.floor(source.width / ratio), (width) => (width >= minimumWidth ? Option.some([width, Math.floor(width / ratio)] as const) : Option.none())),
        Effect.fnUntraced(function* (width: number) {
            const resized = yield* bitmap(() => createImageBitmap(source, { resizeWidth: width, resizeQuality: 'high' }), unsupported);
            const canvas = new OffscreenCanvas(resized.width, resized.height);
            const context = yield* Effect.mapError(Effect.fromNullishOr(canvas.getContext('bitmaprenderer')), unsupported);
            context.transferFromImageBitmap(resized);
            const blob = yield* Effect.tryPromise({ try: () => canvas.convertToBlob(encoding), catch: unsupported });
            return { width, file: new File([blob], `${width}`, { type: blob.type }) };
        }, Effect.scoped),
        { concurrency },
    );
    return Image({ width: source.width, height: source.height, renditions: renditions.filter((rendition) => rendition.file.size < file.size) });
}, Effect.scoped);
const video = Effect.fnUntraced(function* (file: File) {
    const { element, url } = yield* Effect.acquireRelease(
        Effect.sync(() => ({ element: Object.assign(document.createElement('video'), { preload: 'metadata' }), url: URL.createObjectURL(file) })),
        (opened) =>
            Effect.sync(() => {
                opened.element.removeAttribute('src');
                opened.element.load();
                URL.revokeObjectURL(opened.url);
            }),
    );
    const unreadable = MediaError.of('Unreadable', file.name);
    return yield* Effect.callback<Inspection, MediaError>((resume, signal) => {
        element.addEventListener('loadedmetadata', () => resume(element.videoWidth > 0 && element.videoHeight > 0 ? Effect.succeed(Video({ width: element.videoWidth, height: element.videoHeight, duration: Duration.seconds(element.duration) })) : unreadable()), { once: true, signal });
        element.addEventListener('error', () => resume(unreadable(element.error)), { once: true, signal });
        element.src = url;
    });
}, Effect.scoped);
const read = Effect.fn('read')((file: File, options: Options) =>
    Match.value(file.type).pipe(
        Match.when(String.startsWith('image/'), () => image(file, options)),
        Match.when(String.startsWith('video/'), () => video(file)),
        Match.when('application/pdf', () =>
            Effect.tryPromise(() => import('#/pdf/document')).pipe(
                Effect.flatMap(({ PdfDocument }) => PdfDocument.pages(file, options)),
                Effect.map((pages) => Pdf({ pages })),
            ),
        ),
        Match.orElse(() => MediaError.of('Unsupported', file.name)()),
    ),
);
const Inspection = { read, $is, $match };

// --- [EXPORTS] -------------------------------------------------------------------------

export { Inspection };
