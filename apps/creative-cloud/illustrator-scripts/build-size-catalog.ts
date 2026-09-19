/// <reference path="./prelude.ts"/>

// --- [PRELUDE] -------------------------------------------------------------------------

const { each, failure, fold, items, nth, present, rasterOptions, run, saved }: Prelude = $.evalFile(new File(`${new File($.fileName).path}/prelude.jsx`));

// --- [ENTRY] ---------------------------------------------------------------------------

const buildSizeCatalog = (
    request: {
        readonly sizes: { readonly name: string; readonly width: number; readonly height: number; readonly colorSpace: 'RGB' | 'CMYK' }[];
        readonly raster: RasterSpec;
        readonly outputDir: string;
        readonly stationery: boolean;
    },
    at: Site,
): Reading<JsonObject> => {
    const active = items(app.documents).length > 0 ? [app.activeDocument] : [];
    const reading = each(at, request.sizes, (row, site): Reading<JsonObject[]> => {
        const created: Document[] = [];
        const inspected: Document[] = [];
        const files: File[] = [];
        const unavailable: JsonObject[] = [];
        try {
            const doc = app.documents.add(DocumentColorSpace[row.colorSpace], row.width, row.height, 1);
            created.push(doc);
            doc.rasterEffectSettings = rasterOptions(doc, request.raster);
            const artboard = nth(doc.artboards, 0);
            artboard.artboardRect = [0, row.height, row.width, 0];
            artboard.name = row.name;
            files.push(saved(doc, `${request.outputDir}/${row.name}.ai`));
        } catch (error) {
            unavailable.push(failure(error, site));
        }
        const closed = each(site, created, (doc): Reading<null> => {
            doc.close(SaveOptions.DONOTSAVECHANGES);
            return present(null);
        });
        const readback = each(site, closed.unavailable.length === 0 ? files : [], (file): Reading<JsonObject> => {
            if (request.stationery && !file.rename(`${row.name}.ait`)) {
                throw new Error(file.error);
            }
            const reopened = app.open(file);
            inspected.push(reopened);
            return present({ name: row.name, path: file.fsName, artboardRect: nth(reopened.artboards, 0).artboardRect, stationery: reopened.fullName.fsName !== file.fsName });
        });
        const released = each(site, inspected, (doc): Reading<null> => {
            doc.close(SaveOptions.DONOTSAVECHANGES);
            return present(null);
        });
        return { value: readback.value, unavailable: unavailable.concat(closed.unavailable, readback.unavailable, released.unavailable) };
    });
    const restored = each(at, active, (doc): Reading<null> => {
        app.activeDocument = doc;
        return present(null);
    });
    return {
        value: { kind: 'saved', paths: fold<JsonObject[], JsonObject[]>(reading.value, [], (paths, rows): JsonObject[] => paths.concat(rows)) },
        unavailable: reading.unavailable.concat(restored.unavailable),
    };
};

run(buildSizeCatalog);
