import { Array, Data, Duration, Effect, Match, Option, Schema, String, type Types } from 'effect';
import { MediaError } from '#/media-error';

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
const video = (file: File) => {
    const unreadable = MediaError.of('Unreadable', file.name);
    return Effect.acquireUseRelease(
        Effect.sync(() => ({ element: Object.assign(document.createElement('video'), { preload: 'metadata' }), url: URL.createObjectURL(file) })),
        ({ element, url }) =>
            Effect.callback<Inspection, MediaError>((resume, signal) => {
                const listener: EventListenerObject = {
                    handleEvent(event) {
                        switch (event.type) {
                            case 'error':
                                resume(unreadable(element.error));
                                break;
                            default:
                                resume(element.videoWidth > 0 && element.videoHeight > 0 ? Effect.succeed(Video({ width: element.videoWidth, height: element.videoHeight, duration: Duration.seconds(element.duration) })) : unreadable());
                                break;
                        }
                    },
                };
                const options = { once: true, signal };
                element.addEventListener('loadedmetadata', listener, options);
                element.addEventListener('error', listener, options);
                element.src = url;
            }),
        (opened) =>
            Effect.sync(() => {
                opened.element.removeAttribute('src');
                opened.element.load();
                URL.revokeObjectURL(opened.url);
            }),
    );
};
const pdf = Effect.fnUntraced(function* (file: File, options: Options) {
    const { PdfDocument } = yield* Effect.tryPromise(() => import('#/pdf/pdf-document'));
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
    { Scaling, $is, $match },
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { Inspection };
