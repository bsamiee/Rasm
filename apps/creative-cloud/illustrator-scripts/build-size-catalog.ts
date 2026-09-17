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

const { run, visit }: Prelude = $.evalFile(new File(`${new File($.fileName).path}/prelude.jsx`));

// --- [ROWS] ----------------------------------------------------------------------------

interface Size {
    readonly name: string;
    readonly width: number;
    readonly height: number;
    readonly colorSpace: 'RGB' | 'CMYK';
}

const UNTITLED = 'Untitled-';

const saved = (row: Size, raster: { readonly resolution: number; readonly antiAliasing: boolean; readonly padding: number }, outputDir: string, stationery: boolean): string => {
    const doc = app.documents.add(DocumentColorSpace[row.colorSpace], row.width, row.height, 1);
    const artboard = doc.artboards[0] as Artboard;
    artboard.artboardRect = [0, row.height, row.width, 0];
    artboard.name = row.name;
    const settings = doc.rasterEffectSettings;
    settings.resolution = raster.resolution;
    settings.antiAliasing = raster.antiAliasing;
    settings.padding = raster.padding;
    doc.rasterEffectSettings = settings;
    const options: IllustratorSaveOptions = new $.global.IllustratorSaveOptions();
    options.embedICCProfile = true;
    options.pdfCompatible = true;
    options.compatibility = Compatibility.ILLUSTRATOR24;
    const file = new File(`${outputDir}/${row.name}.ai`);
    doc.saveAs(file, options);
    doc.close(SaveOptions.DONOTSAVECHANGES);
    if (!stationery) {
        return file.fsName;
    }
    file.rename(`${row.name}.ait`);
    return file.fsName;
};

// --- [ENTRY] ---------------------------------------------------------------------------

const buildSizeCatalog = (
    request: {
        readonly sizes: Size[];
        readonly raster: { readonly resolution: number; readonly antiAliasing: boolean; readonly padding: number };
        readonly outputDir: string;
        readonly stationery: boolean;
    },
    _at: Site,
): Reading<JsonObject> => {
    const paths: JsonObject[] = [];
    const readback: JsonObject[] = [];
    visit(request.sizes, (row): void => {
        const path = saved(row, request.raster, request.outputDir, request.stationery);
        paths.push({ name: row.name, path });
        const reopened = app.open(new File(path));
        readback.push({
            name: row.name,
            artboardRect: (reopened.artboards[0] as Artboard).artboardRect,
            opensUntitled: reopened.name.indexOf(UNTITLED) === 0 && reopened.fullName.fsName.indexOf(`/${UNTITLED}`) === 0,
        });
        reopened.close(SaveOptions.DONOTSAVECHANGES);
    });
    return { value: { kind: 'catalogSaved', paths, readback }, unavailable: [] };
};

run(buildSizeCatalog);
