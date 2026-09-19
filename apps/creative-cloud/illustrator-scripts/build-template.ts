/// <reference path="./prelude.ts"/>

// --- [PRELUDE] -------------------------------------------------------------------------

const { each, failure, items, present, rasterOptions, run, saved, split, swatches }: Prelude = $.evalFile(new File(`${new File($.fileName).path}/prelude.jsx`));

// --- [ENTRY] ---------------------------------------------------------------------------

const buildTemplate = (
    request: {
        readonly colorSpace: 'RGB' | 'CMYK';
        readonly width: number;
        readonly height: number;
        readonly raster: RasterSpec;
        readonly palette: SwatchPalette;
        readonly output: string;
    },
    at: Site,
): Reading<JsonObject> => {
    const active = items(app.documents).length > 0 ? [app.activeDocument] : [];
    const created: Document[] = [];
    const inspected: Document[] = [];
    const palette: JsonObject[] = [];
    const unavailable: JsonObject[] = [];
    let value: JsonObject;
    try {
        const doc = app.documents.add(DocumentColorSpace[request.colorSpace], request.width, request.height, 1);
        created.push(doc);
        doc.rasterEffectSettings = rasterOptions(doc, request.raster);
        const reading = swatches(doc, request.palette, false, at);
        Array.prototype.push.apply(palette, reading.value);
        Array.prototype.push.apply(unavailable, reading.unavailable);
        const file = saved(doc, request.output);
        value = { kind: 'saved', path: file.fsName };
    } catch (error) {
        unavailable.push(failure(error, at));
        value = { kind: 'notSaved', path: request.output };
    }
    const closed = each(at, created, (doc): Reading<null> => {
        doc.close(SaveOptions.DONOTSAVECHANGES);
        return present(null);
    });
    const readback = each(at, value['kind'] === 'saved' && closed.unavailable.length === 0 ? [request.output] : [], (path): Reading<JsonObject> => {
        const doc = app.open(new File(path));
        inspected.push(doc);
        return present({ colorSpace: String(doc.documentColorSpace), rasterResolution: doc.rasterEffectSettings.resolution, swatchCount: doc.swatches.length });
    });
    const released = each(at, inspected, (doc): Reading<null> => {
        doc.close(SaveOptions.DONOTSAVECHANGES);
        return present(null);
    });
    const restored = each(at, active, (doc): Reading<null> => {
        app.activeDocument = doc;
        return present(null);
    });
    const [measured] = readback.value;
    if (measured !== undefined) {
        value['readback'] = measured;
    }
    const rows = split(palette);
    value['applied'] = rows.applied;
    value['rejected'] = rows.rejected;
    return { value, unavailable: unavailable.concat(closed.unavailable, readback.unavailable, released.unavailable, restored.unavailable) };
};

run(buildTemplate);
