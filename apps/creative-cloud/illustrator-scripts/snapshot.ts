/// <reference path="./prelude.ts"/>

// --- [HOST] ----------------------------------------------------------------------------

declare const $: $;
declare const app: Application;

// --- [PRELUDE] -------------------------------------------------------------------------

const { run }: Prelude = $.evalFile(new File(`${new File($.fileName).path}/prelude.jsx`));

// --- [BUDGET] --------------------------------------------------------------------------

const POINTS_PER_INCH = 72;

const resolution = (budget: { readonly longEdgePx: number; readonly pixels: number }, bounds: { readonly minimum: number; readonly maximum: number }, widthPt: number, heightPt: number): number => {
    const widthIn = widthPt / POINTS_PER_INCH;
    const heightIn = heightPt / POINTS_PER_INCH;
    const fitted = Math.round(Math.min(budget.longEdgePx / Math.max(widthIn, heightIn), Math.sqrt(budget.pixels / (widthIn * heightIn))));
    return Math.min(bounds.maximum, Math.max(bounds.minimum, fitted));
};

// --- [ENTRY] ---------------------------------------------------------------------------

const snapshot = (
    request: {
        readonly path: string;
        readonly clipBounds?: [number, number, number, number];
        readonly budget: { readonly longEdgePx: number; readonly pixels: number };
        readonly resolution: { readonly minimum: number; readonly maximum: number };
    },
    _at: Site,
): Reading<JsonObject> => {
    const doc = app.activeDocument;
    const [left, top, right, bottom] = request.clipBounds ?? (doc.artboards[doc.artboards.getActiveArtboardIndex()] as Artboard).artboardRect;
    const widthPt = right - left;
    const heightPt = top - bottom;
    const dpi = resolution(request.budget, request.resolution, widthPt, heightPt);
    const options: ImageCaptureOptions = new $.global.ImageCaptureOptions();
    options.resolution = dpi;
    options.antiAliasing = true;
    options.transparency = false;
    doc.imageCapture(new File(request.path), [left, top, right, bottom], options);
    return {
        value: { kind: 'image', path: request.path, widthPx: Math.round((widthPt * dpi) / POINTS_PER_INCH), heightPx: Math.round((heightPt * dpi) / POINTS_PER_INCH), dpi },
        unavailable: [],
    };
};

run(snapshot);
