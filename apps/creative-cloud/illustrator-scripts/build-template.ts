/// <reference path="./prelude.ts"/>

// --- [HOST] ----------------------------------------------------------------------------

declare const $: $;
declare const app: Application;

declare global {
    enum Compatibility {}
    enum DocumentColorSpace {}
    enum SaveOptions {}
}

// --- [PRELUDE] -------------------------------------------------------------------------

const { run, swatches }: Prelude = $.evalFile(new File(`${new File($.fileName).path}/prelude.jsx`));

// --- [ENTRY] ---------------------------------------------------------------------------

const buildTemplate = (
    request: {
        readonly colorSpace: 'RGB' | 'CMYK';
        readonly width: number;
        readonly height: number;
        readonly raster: { readonly resolution: number; readonly antiAliasing: boolean; readonly padding: number };
        readonly groups: SwatchGroupSpec[];
        readonly output: string;
    },
    _at: Site,
): Reading<JsonObject> => {
    const doc = app.documents.add(DocumentColorSpace[request.colorSpace], request.width, request.height, 1);
    const raster = doc.rasterEffectSettings;
    raster.resolution = request.raster.resolution;
    raster.antiAliasing = request.raster.antiAliasing;
    raster.padding = request.raster.padding;
    doc.rasterEffectSettings = raster;
    const rows = swatches(doc, request.groups, false);
    const options: IllustratorSaveOptions = new $.global.IllustratorSaveOptions();
    options.embedICCProfile = true;
    options.pdfCompatible = true;
    options.compatibility = Compatibility.ILLUSTRATOR24;
    doc.saveAs(new File(request.output), options);
    doc.close(SaveOptions.DONOTSAVECHANGES);
    const reopened = app.open(new File(request.output));
    const readback = { colorSpace: String(reopened.documentColorSpace), rasterResolution: reopened.rasterEffectSettings.resolution, swatchCount: reopened.swatches.length };
    reopened.close(SaveOptions.DONOTSAVECHANGES);
    return { value: { kind: 'templateSaved', path: request.output, readback, applied: rows.applied, rejected: rows.rejected }, unavailable: [] };
};

run(buildTemplate);
