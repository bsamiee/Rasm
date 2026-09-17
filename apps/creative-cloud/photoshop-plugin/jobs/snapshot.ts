// --- [IMPORTS] -------------------------------------------------------------------------

import { type Document, imaging, type PhotoshopImageData } from 'adobe:photoshop';
import { type Handler, handler, thrown } from '@rasm/creative-cloud-server/client';
import { HostRejection } from '@rasm/creative-cloud-server/errors';
import { Bounds, dpi, type PixelBudget, pixels, points, type Region } from '@rasm/creative-cloud-server/images';
import { Jpeg, Snapshot } from '@rasm/creative-cloud-server/photoshop/jobs';
import { Array, Effect, Match, Option, Predicate, Schema } from 'effect';
import { document, flatten } from './get-document.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface Pixels {
    readonly imageData: PhotoshopImageData;
    readonly sourceBounds: Bounds;
    readonly level: number;
}

type Capture = (typeof Snapshot)['Type'];

type Size = (typeof _Size)['Type'];

// --- [CONSTANTS] -----------------------------------------------------------------------

const _PROFILE = 'sRGB IEC61966-2.1';
const _COMPONENTS = 3;
const _COMPONENT_SIZE = 8;

// --- [MODELS] --------------------------------------------------------------------------

const _Size = Schema.Struct({ width: Schema.Int, height: Schema.Int });

const _pixels = Schema.encodeSync(
    Schema.Struct({
        documentId: Schema.Int,
        layerId: Schema.OptionFromOptionalKey(Schema.Int),
        sourceBounds: Bounds,
        targetSize: _Size,
        colorSpace: Schema.Literal('RGB'),
        colorProfile: Schema.String,
        componentSize: Schema.Literal(_COMPONENT_SIZE),
        applyAlpha: Schema.Boolean,
    }).pipe(Schema.encodeKeys({ documentId: 'documentID', layerId: 'layerID' })),
);

const _selection = Schema.encodeSync(Schema.Struct({ documentId: Schema.Int, sourceBounds: Bounds, targetSize: _Size }).pipe(Schema.encodeKeys({ documentId: 'documentID' })));

// --- [GEOMETRY] ------------------------------------------------------------------------

const _region = (open: Document, region: Option.Option<Region>): Bounds =>
    Option.match(region, {
        onNone: () => ({ left: 0, top: 0, right: open.width, bottom: open.height }),
        onSome: ([x0, y0, x1, y1]) => ({ left: Math.round(x0 * open.width), top: Math.round(y0 * open.height), right: Math.round(x1 * open.width), bottom: Math.round(y1 * open.height) }),
    });

const _fit = (budget: PixelBudget, bounds: Bounds, resolution: number): Size => {
    const width = bounds.right - bounds.left;
    const height = bounds.bottom - bounds.top;
    const resolved = dpi(budget, points(width, resolution), points(height, resolution));
    return { width: Math.min(width, pixels(points(width, resolution), resolved)), height: Math.min(height, pixels(points(height, resolution), resolved)) };
};

const _scaled = (bounds: Bounds, factor: number): Bounds => ({ left: bounds.left * factor, top: bounds.top * factor, right: bounds.right * factor, bottom: bounds.bottom * factor });

const _layerBounds = Effect.fnUntraced(function* (open: Document, capture: Capture) {
    const layerId = yield* Effect.fromOption(capture.layerId, () => HostRejection.cases.malformedParams.make({ cause: 'layerId' }));
    const placed = yield* Effect.fromOption(
        Array.findFirst(flatten(open.layers, 0, Option.none()), ({ layer }) => layer.id === layerId),
        () => HostRejection.cases.itemNotFound.make({ itemId: layerId }),
    );
    const { left, top, right, bottom } = placed.layer.boundsNoEffects;
    return { left, top, right, bottom };
});

// --- [PIXELS] --------------------------------------------------------------------------

const _tripled = (data: Uint8Array | Uint16Array | Float32Array): Uint8Array | Uint16Array | Float32Array => {
    const values = Array.flatMap(Array.fromIterable(data), (value) => [value, value, value]);
    return Match.value(data).pipe(
        Match.when(Match.instanceOf(Uint8Array), () => Uint8Array.from(values)),
        Match.when(Match.instanceOf(Uint16Array), () => Uint16Array.from(values)),
        Match.orElse(() => Float32Array.from(values)),
    );
};

const _rgb = (mask: Pixels): Effect.Effect<Pixels, HostRejection> =>
    Effect.tryPromise({
        try: async () => {
            const data = await mask.imageData.getData({ chunky: true });
            const imageData = await imaging.createImageDataFromBuffer(_tripled(data), {
                width: mask.imageData.width,
                height: mask.imageData.height,
                components: _COMPONENTS,
                chunky: true,
                colorSpace: 'RGB',
                colorProfile: _PROFILE,
            });
            await mask.imageData.dispose();
            return { ...mask, imageData };
        },
        catch: thrown,
    });

const _mask = (open: Document, capture: Capture): Effect.Effect<Pixels, HostRejection> => {
    const sourceBounds = _region(open, capture.region);
    return Effect.flatMap(
        Effect.tryPromise({ try: () => imaging.getSelection(_selection({ documentId: open.id, sourceBounds, targetSize: _fit(capture.budget, sourceBounds, open.resolution) })), catch: thrown }),
        (selection) => _rgb({ ...selection, level: 0 }),
    );
};

const _composite = (open: Document, capture: Capture, sourceBounds: Bounds): Effect.Effect<Pixels, HostRejection> =>
    Effect.tryPromise({
        try: () =>
            imaging.getPixels(
                _pixels({
                    documentId: open.id,
                    layerId: Option.filter(capture.layerId, () => capture.target === 'layer'),
                    sourceBounds,
                    targetSize: _fit(capture.budget, sourceBounds, open.resolution),
                    colorSpace: 'RGB',
                    colorProfile: _PROFILE,
                    componentSize: _COMPONENT_SIZE,
                    applyAlpha: true,
                }),
            ),
        catch: thrown,
    });

const _read = (open: Document, capture: Capture): Effect.Effect<Pixels, HostRejection> =>
    Match.value(capture.target).pipe(
        Match.when('selection', () => _mask(open, capture)),
        Match.when('layer', () => Effect.flatMap(_layerBounds(open, capture), (sourceBounds) => _composite(open, capture, sourceBounds))),
        Match.when('document', () => _composite(open, capture, _region(open, capture.region))),
        Match.exhaustive,
    );

const _encoded = ({ imageData, sourceBounds, level }: Pixels): Effect.Effect<(typeof Jpeg)['Type'], HostRejection> =>
    Effect.map(
        Effect.filterOrFail(Effect.tryPromise({ try: () => imaging.encodeImageData({ imageData, base64: true }), catch: thrown }), Predicate.isString, (encoded) =>
            HostRejection.cases.resultNotJson.make({ cause: encoded }),
        ),
        (base64) => ({
            kind: 'jpeg' as const,
            base64,
            widthPx: imageData.width,
            heightPx: imageData.height,
            level,
            scale: 2 ** -level,
            sourceBounds: _scaled(sourceBounds, 2 ** level),
            colorProfile: imageData.colorProfile,
        }),
    );

const _disposed = ({ imageData }: Pixels): Effect.Effect<void> => Effect.promise(() => imageData.dispose());

// --- [HANDLER] -------------------------------------------------------------------------

const snapshot: Handler = handler(Snapshot, Jpeg, (capture) => Effect.flatMap(document(capture.documentId), (open) => Effect.acquireUseRelease(_read(open, capture), _encoded, _disposed)));

// --- [EXPORTS] -------------------------------------------------------------------------

export { snapshot };
