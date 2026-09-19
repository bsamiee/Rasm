/// <reference path="./prelude.ts"/>

// --- [PRELUDE] -------------------------------------------------------------------------

const { contains, each, failure, items, present, run, select }: Prelude = $.evalFile(new File(`${new File($.fileName).path}/prelude.jsx`));

// --- [ENTRY] ---------------------------------------------------------------------------

const writeLibrary = (request: { readonly library: keyof typeof LibraryType & string; readonly source: string; readonly output: string }, at: Site): Reading<JsonObject> => {
    const retained = items(app.documents);
    const active = retained.length > 0 ? [app.activeDocument] : [];
    const opened: Document[] = [];
    const unavailable: JsonObject[] = [];
    let value: JsonObject;
    try {
        const doc = app.open(new File(request.source));
        opened.push(doc);
        doc.writeAsLibrary(new File(request.output), LibraryType[request.library]);
        value = { kind: 'saved', path: request.output };
    } catch (error) {
        unavailable.push(failure(error, at));
        value = { kind: 'notSaved', path: request.output };
    } finally {
        const closed = each(
            at,
            select(opened, (document): boolean => !contains(retained, document)),
            (document): Reading<null> => {
                document.close(SaveOptions.DONOTSAVECHANGES);
                return present(null);
            },
        );
        const restored = each(at, active, (document): Reading<null> => {
            app.activeDocument = document;
            return present(null);
        });
        Array.prototype.push.apply(unavailable, closed.unavailable.concat(restored.unavailable));
    }
    return { value, unavailable };
};

run(writeLibrary);
