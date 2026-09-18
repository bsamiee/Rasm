/// <reference path="./prelude.ts"/>

declare global {
    enum SaveOptions {}
}

// --- [PRELUDE] -------------------------------------------------------------------------

const { collect, created, nth, present, run, saved }: Prelude = $.evalFile(new File(`${new File($.fileName).path}/prelude.jsx`));

// --- [ENTRY] ---------------------------------------------------------------------------

const UNTITLED = 'Untitled-';

const buildSizeCatalog = (request: {
    readonly sizes: { readonly name: string; readonly width: number; readonly height: number; readonly colorSpace: 'RGB' | 'CMYK' }[];
    readonly raster: RasterSpec;
    readonly outputDir: string;
    readonly stationery: boolean;
}): Reading<JsonObject> =>
    present({
        kind: 'saved',
        paths: collect(request.sizes, (row): JsonObject => {
            const doc = created(row.colorSpace, row.width, row.height, request.raster);
            const artboard = nth(doc.artboards, 0);
            artboard.artboardRect = [0, row.height, row.width, 0];
            artboard.name = row.name;
            const file = saved(doc, `${request.outputDir}/${row.name}.ai`);
            if (request.stationery) {
                file.rename(`${row.name}.ait`);
            }
            const reopened = app.open(file);
            const readback = {
                name: row.name,
                path: file.fsName,
                artboardRect: nth(reopened.artboards, 0).artboardRect,
                opensUntitled: reopened.name.indexOf(UNTITLED) === 0 && reopened.fullName.fsName.indexOf(`/${UNTITLED}`) === 0,
            };
            reopened.close(SaveOptions.DONOTSAVECHANGES);
            return readback;
        }),
    });

run(buildSizeCatalog);
