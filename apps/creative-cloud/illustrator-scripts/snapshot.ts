/// <reference path="./prelude.ts"/>

// --- [PRELUDE] -------------------------------------------------------------------------

const { nth, present, run }: Prelude = $.evalFile(new File(`${new File($.fileName).path}/prelude.jsx`));

// --- [ENTRY] ---------------------------------------------------------------------------

const POINTS_PER_INCH = 72;

const snapshot = (request: {
    readonly path: string;
    readonly clip: { readonly bounds: [number, number, number, number] } | { readonly artboard: 'active' };
    readonly budget: { readonly longEdgePx: number; readonly pixels: number };
    readonly resolution: { readonly minimum: number; readonly maximum: number };
}): Reading<JsonObject> => {
    const doc = app.activeDocument;
    const [left, top, right, bottom] = 'bounds' in request.clip ? request.clip.bounds : nth(doc.artboards, doc.artboards.getActiveArtboardIndex()).artboardRect;
    const widthIn = (right - left) / POINTS_PER_INCH;
    const heightIn = (top - bottom) / POINTS_PER_INCH;
    const fitted = Math.round(Math.min(request.budget.longEdgePx / Math.max(widthIn, heightIn), Math.sqrt(request.budget.pixels / (widthIn * heightIn))));
    const dpi = Math.min(request.resolution.maximum, Math.max(request.resolution.minimum, fitted));
    const options = new ImageCaptureOptions();
    options.resolution = dpi;
    options.antiAliasing = true;
    options.transparency = false;
    doc.imageCapture(new File(request.path), [left, top, right, bottom], options);
    return present({ kind: 'image', path: request.path, widthPx: Math.round(widthIn * dpi), heightPx: Math.round(heightIn * dpi), dpi });
};

run(snapshot);
