/// <reference path="./prelude.ts"/>

// --- [PRELUDE] -------------------------------------------------------------------------

const { present, run, split, swatches }: Prelude = $.evalFile(new File(`${new File($.fileName).path}/prelude.jsx`));

// --- [ENTRY] ---------------------------------------------------------------------------

const importSwatches = (request: { readonly palette: SwatchPalette; readonly replaceByName: boolean }): Reading<JsonObject> =>
    present(split(swatches(app.activeDocument, request.palette, request.replaceByName)));

run(importSwatches);
