/// <reference path="./prelude.ts"/>

// --- [PRELUDE] -------------------------------------------------------------------------

const { run, split, swatches }: Prelude = $.evalFile(new File(`${new File($.fileName).path}/prelude.jsx`));

// --- [ENTRY] ---------------------------------------------------------------------------

const importSwatches = (request: { readonly palette: SwatchPalette; readonly replaceByName: boolean }, at: Site): Reading<JsonObject> => {
    const reading = swatches(app.activeDocument, request.palette, request.replaceByName, at);
    return { value: split(reading.value), unavailable: reading.unavailable };
};

run(importSwatches);
