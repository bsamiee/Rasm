/// <reference path="./prelude.ts"/>

// --- [HOST] ----------------------------------------------------------------------------

declare const $: $;
declare const app: Application;

// --- [PRELUDE] -------------------------------------------------------------------------

const { run, swatches }: Prelude = $.evalFile(new File(`${new File($.fileName).path}/prelude.jsx`));

// --- [ENTRY] ---------------------------------------------------------------------------

const importSwatches = (request: { readonly groups: SwatchGroupSpec[]; readonly replaceByName: boolean }, _at: Site): Reading<JsonObject> => {
    const rows = swatches(app.activeDocument, request.groups, request.replaceByName);
    return { value: { kind: 'swatchesImported', applied: rows.applied, rejected: rows.rejected }, unavailable: [] };
};

run(importSwatches);
