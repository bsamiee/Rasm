import { Array, Effect, Option, Schema } from 'effect';

// --- [ERRORS] --------------------------------------------------------------------------

class InspectError extends Schema.TaggedError<InspectError, { readonly brand: unique symbol }>()('InspectError', { reason: Schema.TaggedUnion({ Protected: {}, Unreadable: {} }), cause: Schema.Defect() }) {
    override get message(): string {
        return InspectError.fields.reason.match(this.reason, { Protected: () => 'The PDF is password protected', Unreadable: () => 'The browser cannot read this file' });
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const imageRenditions = Effect.fn('imageRenditions')(function* (file: Blob, { minimumWidth, type }: { readonly minimumWidth: number; readonly type: string }) {
    const source = yield* Effect.acquireRelease(Effect.tryPromise({ try: () => createImageBitmap(file), catch: (cause) => InspectError.make({ reason: { _tag: 'Unreadable' }, cause }) }), (bitmap) => Effect.sync(() => bitmap.close()));
    const renditions = yield* Effect.forEach(
        Array.unfold(Math.floor(source.width / 2), (width) => (width >= minimumWidth ? Option.some([width, Math.floor(width / 2)] as const) : Option.none())),
        Effect.fnUntraced(function* (width: number) {
            const bitmap = yield* Effect.tryPromise(() => createImageBitmap(source, { resizeWidth: width, resizeQuality: 'high' }));
            const canvas = new OffscreenCanvas(bitmap.width, bitmap.height);
            (yield* Effect.fromNullishOr(canvas.getContext('bitmaprenderer'))).transferFromImageBitmap(bitmap);
            const blob = yield* Effect.tryPromise(() => canvas.convertToBlob({ type }));
            return { width, file: new File([blob], String(width), { type: blob.type }) };
        }),
        { concurrency: 'unbounded' },
    );
    return { width: source.width, height: source.height, renditions: renditions.filter((rendition) => rendition.file.size < file.size) };
}, Effect.scoped);
const videoDimensions = Effect.fn('videoDimensions')(function* (file: Blob) {
    const { element, url } = yield* Effect.acquireRelease(
        Effect.sync(() => ({ element: Object.assign(document.createElement('video'), { preload: 'metadata' }), url: URL.createObjectURL(file) })),
        (video) =>
            Effect.sync(() => {
                video.element.removeAttribute('src');
                video.element.load();
                URL.revokeObjectURL(video.url);
            }),
    );
    return yield* Effect.callback<{ readonly width: number; readonly height: number }, InspectError>((resume, signal) => {
        element.addEventListener('loadedmetadata', () => resume(Effect.succeed({ width: element.videoWidth, height: element.videoHeight })), { once: true, signal });
        element.addEventListener('error', () => resume(Effect.fail(InspectError.make({ reason: { _tag: 'Unreadable' }, cause: element.error }))), { once: true, signal });
        element.src = url;
    });
}, Effect.scoped);

// --- [EXPORTS] -------------------------------------------------------------------------

export { InspectError, imageRenditions, videoDimensions };
