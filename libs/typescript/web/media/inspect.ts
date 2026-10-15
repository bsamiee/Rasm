import { Array, Effect, Formatter, Option, Schema, type Scope } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

interface Rendition {
    readonly width: number;
    readonly file: File;
}

// --- [ERRORS] --------------------------------------------------------------------------

const InspectReason = Schema.TaggedUnion({
    Undecoded: { cause: Schema.Defect() },
    Unresized: { width: Schema.Finite, cause: Schema.Defect() },
    Contextless: { width: Schema.Finite },
    Unencoded: { width: Schema.Finite, type: Schema.String, cause: Schema.Defect() },
    Unplayable: { cause: Schema.Defect() },
});
class InspectError extends Schema.TaggedError<InspectError, { readonly brand: unique symbol }>()('InspectError', { reason: InspectReason }) {
    override get message(): string {
        return InspectReason.match(this.reason, {
            Undecoded: ({ cause }) => `the browser could not decode the image: ${Formatter.format(cause)}`,
            Unresized: ({ width, cause }) => `the browser could not resize the image to ${width} px: ${Formatter.format(cause)}`,
            Contextless: ({ width }) => `the browser has no 2D canvas context for the ${width} px rendition`,
            Unencoded: ({ width, type, cause }) => `the browser could not encode the ${width} px rendition as ${type}: ${Formatter.format(cause)}`,
            Unplayable: ({ cause }) => `the browser cannot play the video: ${Formatter.format(cause)}`,
        });
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const imageRenditions = Effect.fnUntraced(function* (file: Blob, { minimumWidth, type }: { readonly minimumWidth: number; readonly type: string }): Effect.fn.Return<{ readonly width: number; readonly height: number; readonly renditions: readonly Rendition[] }, InspectError, Scope.Scope> {
    const source = yield* Effect.acquireRelease(Effect.tryPromise({ try: () => createImageBitmap(file), catch: (cause) => InspectError.make({ reason: { _tag: 'Undecoded', cause } }) }), (bitmap) => Effect.sync(() => bitmap.close()));
    const renditions = yield* Effect.forEach(
        Array.unfold(Math.floor(source.width / 2), (width) => (width >= minimumWidth ? Option.some([width, Math.floor(width / 2)] as const) : Option.none())),
        Effect.fnUntraced(function* (width: number) {
            const bitmap = yield* Effect.acquireRelease(Effect.tryPromise({ try: () => createImageBitmap(source, { resizeWidth: width, resizeQuality: 'high' }), catch: (cause) => InspectError.make({ reason: { _tag: 'Unresized', width, cause } }) }), (resized) => Effect.sync(() => resized.close()));
            const canvas = new OffscreenCanvas(bitmap.width, bitmap.height);
            const context = canvas.getContext('2d');
            if (context === null) {
                return yield* InspectError.make({ reason: { _tag: 'Contextless', width } });
            }
            context.drawImage(bitmap, 0, 0);
            const blob = yield* Effect.tryPromise({ try: () => canvas.convertToBlob({ type }), catch: (cause) => InspectError.make({ reason: { _tag: 'Unencoded', width, type, cause } }) });
            return blob.size < file.size ? Option.some<Rendition>({ width: bitmap.width, file: new File([blob], String(bitmap.width), { type: blob.type }) }) : Option.none<Rendition>();
        }, Effect.scoped),
    ).pipe(Effect.map(Array.getSomes));
    return { width: source.width, height: source.height, renditions };
});
const videoDimensions = Effect.fnUntraced(function* (file: Blob): Effect.fn.Return<{ readonly width: number; readonly height: number }, InspectError, Scope.Scope> {
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
        element.addEventListener('error', () => resume(Effect.fail(InspectError.make({ reason: { _tag: 'Unplayable', cause: element.error } }))), { once: true, signal });
        element.src = url;
    });
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { InspectError, imageRenditions, videoDimensions };
