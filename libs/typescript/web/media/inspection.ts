import { Array, Data, Duration, Effect, Match, Option, Schema, String, type Types } from 'effect';
import { MediaError } from '#/error';

// --- [TYPES] ---------------------------------------------------------------------------

type Inspection = Data.TaggedEnum<{
    Image: { readonly width: number; readonly height: number; readonly renditions: readonly { readonly width: number; readonly file: File }[] };
    Video: { readonly width: number; readonly height: number; readonly duration: Duration.Duration };
    Pdf: { readonly pages: Array.NonEmptyReadonlyArray<{ readonly width: number; readonly height: number; readonly label?: string }> };
}>;
type Options = ImageEncodeOptions & { readonly scaling: Scaling; readonly concurrency: Types.Concurrency };

// --- [MODELS] --------------------------------------------------------------------------

class Scaling extends Schema.Class<Scaling, { readonly brand: unique symbol }>('Scaling')({ minimumWidth: Schema.Finite.check(Schema.isGreaterThan(0)), ratio: Schema.Finite.check(Schema.isGreaterThan(1)) }) {
    nextWidth(width: number) {
        return Math.floor(width / this.ratio);
    }
}
const { $is, $match, Image, Video, Pdf } = Data.taggedEnum<Inspection>();

// --- [OPERATIONS] ----------------------------------------------------------------------

const bitmap = (source: ImageBitmapSource, fail: (cause: unknown) => MediaError, options?: ImageBitmapOptions) => Effect.acquireRelease(Effect.tryPromise({ try: () => createImageBitmap(source, options), catch: fail }), (opened) => Effect.sync(() => opened.close()));
const image = Effect.fnUntraced(function* (file: File, { scaling, concurrency, ...encoding }: Options) {
    const unsupported = MediaError.of('Unsupported', file.name);
    const source = yield* bitmap(file, MediaError.of('Unreadable', file.name));
    const renditions = yield* Effect.forEach(
        Array.unfold(scaling.nextWidth(source.width), (width) => (width >= scaling.minimumWidth ? Option.some([width, scaling.nextWidth(width)] as const) : Option.none())),
        Effect.fnUntraced(function* (width: number) {
            const resized = yield* bitmap(source, unsupported, { resizeWidth: width, resizeQuality: 'high' });
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
    const unreadable = MediaError.of('Unreadable', file.name);
    const url = yield* Effect.acquireRelease(
        Effect.sync(() => URL.createObjectURL(file)),
        (created) => Effect.sync(() => URL.revokeObjectURL(created)),
    );
    const element = yield* Effect.acquireRelease(
        Effect.sync(() => Object.assign(document.createElement('video'), { preload: 'metadata' })),
        (opened) =>
            Effect.sync(() => {
                opened.removeAttribute('src');
                opened.load();
            }),
    );
    return yield* Effect.callback<Inspection, MediaError>((resume, signal) => {
        const options = { once: true, signal };
        element.addEventListener('loadedmetadata', () => resume(element.videoWidth > 0 && element.videoHeight > 0 ? Effect.succeed(Video({ width: element.videoWidth, height: element.videoHeight, duration: Duration.seconds(element.duration) })) : unreadable()), options);
        element.addEventListener('error', () => resume(unreadable(element.error)), options);
        element.src = url;
    });
}, Effect.scoped);
const sourceSet = ({ width, renditions }: { readonly width: number; readonly renditions?: readonly { readonly width: number }[] | undefined }, url: (width?: number) => string) => renditions && [...renditions.map((rendition) => `${url(rendition.width)} ${rendition.width}w`), `${url()} ${width}w`].join(', ');
const pdf = Effect.fnUntraced(function* (file: File, options: Options) {
    const { PdfDocument } = yield* Effect.tryPromise({ try: () => import('#/pdf-document'), catch: MediaError.of('Unavailable', file.name) });
    return Pdf({ pages: yield* PdfDocument.pages(file, options) });
});
const Inspection = Object.assign(
    Effect.fn('Inspection')((file: File, options: Options) =>
        Match.value(file.type).pipe(
            Match.when(String.startsWith('image/'), () => image(file, options)),
            Match.when(String.startsWith('video/'), () => video(file)),
            Match.when('application/pdf', () => pdf(file, options)),
            Match.orElse(() => MediaError.of('Unsupported', file.name)()),
        ),
    ),
    { Scaling, sourceSet, $is, $match },
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { Inspection };
