/// <reference path="./prelude.ts"/>

// --- [HOST] ----------------------------------------------------------------------------

declare const $: $;
declare const app: Application;

declare global {
    enum LibraryType {}
    enum SaveOptions {}
}

// --- [PRELUDE] -------------------------------------------------------------------------

const { run }: Prelude = $.evalFile(new File(`${new File($.fileName).path}/prelude.jsx`));

// --- [ENTRY] ---------------------------------------------------------------------------

const writeLibrary = (request: { readonly library: 'SWATCHES' | 'BRUSHES' | 'SYMBOLS' | 'GRAPHICSTYLES'; readonly source: string; readonly output: string }, _at: Site): Reading<JsonObject> => {
    const doc = app.open(new File(request.source));
    try {
        doc.writeAsLibrary(new File(request.output), LibraryType[request.library]);
    } finally {
        doc.close(SaveOptions.DONOTSAVECHANGES);
    }
    return { value: { kind: 'saved', path: request.output }, unavailable: [] };
};

run(writeLibrary);
