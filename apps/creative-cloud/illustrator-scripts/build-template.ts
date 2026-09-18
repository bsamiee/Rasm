/// <reference path="./prelude.ts"/>

declare global {
    enum SaveOptions {}
}

// --- [PRELUDE] -------------------------------------------------------------------------

const { created, present, run, saved, split, swatches }: Prelude = $.evalFile(new File(`${new File($.fileName).path}/prelude.jsx`));

// --- [ENTRY] ---------------------------------------------------------------------------

const buildTemplate = (request: {
    readonly colorSpace: 'RGB' | 'CMYK';
    readonly width: number;
    readonly height: number;
    readonly raster: RasterSpec;
    readonly palette: SwatchPalette;
    readonly output: string;
}): Reading<JsonObject> => {
    const doc = created(request.colorSpace, request.width, request.height, request.raster);
    const rows = split(swatches(doc, request.palette, false));
    const file = saved(doc, request.output);
    const reopened = app.open(file);
    const readback = { colorSpace: String(reopened.documentColorSpace), rasterResolution: reopened.rasterEffectSettings.resolution, swatchCount: reopened.swatches.length };
    reopened.close(SaveOptions.DONOTSAVECHANGES);
    return present({ kind: 'saved', path: file.fsName, readback, applied: rows.applied, rejected: rows.rejected });
};

run(buildTemplate);
