/// <reference path="./prelude.ts"/>

declare global {
    enum LibraryType {}
    enum SaveOptions {}
}

// --- [PRELUDE] -------------------------------------------------------------------------

const { present, run }: Prelude = $.evalFile(new File(`${new File($.fileName).path}/prelude.jsx`));

// --- [ENTRY] ---------------------------------------------------------------------------

const writeLibrary = (request: { readonly library: keyof typeof LibraryType & string; readonly source: string; readonly output: string }): Reading<JsonObject> => {
    const doc = app.open(new File(request.source));
    try {
        doc.writeAsLibrary(new File(request.output), LibraryType[request.library]);
    } finally {
        doc.close(SaveOptions.DONOTSAVECHANGES);
    }
    return present({ kind: 'saved', path: request.output });
};

run(writeLibrary);
