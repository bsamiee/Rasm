/// <reference path="./prelude.ts"/>

declare global {
    enum ColorModel {}
    enum SaveOptions {}
}

// --- [PRELUDE] -------------------------------------------------------------------------

const { collect, contains, flatMap, items, present, run, select, spec, split }: Prelude = $.evalFile(new File(`${new File($.fileName).path}/prelude.jsx`));

// --- [CHANNELS] ------------------------------------------------------------------------

const spots = (doc: Document): Spot[] => select(items(doc.spots), (spot): boolean => spot.colorType !== ColorModel.REGISTRATION);

const keyed = (spot: Spot, model: 'RGB' | 'CMYK'): string[] =>
    collect(
        select(spec(spot.color), (chosen): chosen is ProcessSpec => chosen.model === model),
        (chosen): string => chosen.values.join(','),
    );

const opened = (path: string): Document => {
    const [open] = select(items(app.documents), (doc): boolean => doc.fullName.fsName === path);
    return open === undefined ? app.open(new File(path)) : open;
};

// --- [ENTRY] ---------------------------------------------------------------------------

const syncSwatches = (request: { readonly source: string; readonly targets: string[]; readonly match: 'RGB' | 'CMYK' }): Reading<JsonObject> => {
    const wasOpen = collect(items(app.documents), (doc): string => doc.fullName.fsName);
    const rows = flatMap(spots(opened(request.source)), (spot): { readonly name: string; readonly key: string }[] => collect(keyed(spot, request.match), (key) => ({ name: spot.name, key })));
    return present(
        split(
            flatMap(request.targets, (path): JsonObject[] => {
                const target = opened(path);
                const changes = collect(spots(target), (spot): JsonObject => {
                    const keys = keyed(spot, request.match);
                    const [row] = select(rows, (candidate): boolean => contains(keys, candidate.key));
                    const from = spot.name;
                    if (row === undefined) {
                        return { document: path, name: from, reason: 'unmatched' };
                    }
                    const renamable = spot;
                    renamable.name = row.name;
                    return { document: path, from, to: row.name };
                });
                if (!contains(wasOpen, path)) {
                    target.close(SaveOptions.DONOTSAVECHANGES);
                }
                return changes;
            }),
        ),
    );
};

run(syncSwatches);
