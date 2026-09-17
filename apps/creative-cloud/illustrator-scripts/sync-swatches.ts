/// <reference path="./prelude.ts"/>

// --- [HOST] ----------------------------------------------------------------------------

declare const $: $;
declare const app: Application;

declare global {
    enum SaveOptions {}
}

// --- [PRELUDE] -------------------------------------------------------------------------

const { collect, contains, fold, items, run, select, typed, visit }: Prelude = $.evalFile(new File(`${new File($.fileName).path}/prelude.jsx`));

// --- [CHANNELS] ------------------------------------------------------------------------

interface Row {
    readonly name: string;
    readonly values: number[];
}

const REGISTRATION = '[Registration]';

const channels = (value: Color, match: 'rgb' | 'cmyk'): number[] => {
    if (match === 'rgb' && typed<RGBColor>('RGBColor')(value)) {
        return [value.red, value.green, value.blue];
    }
    if (match === 'cmyk' && typed<CMYKColor>('CMYKColor')(value)) {
        return [value.cyan, value.magenta, value.yellow, value.black];
    }
    return [];
};

const same = (left: number[], right: number[]): boolean => left.length > 0 && left.length === right.length && fold(left, true, (equal, value, index): boolean => equal && value === right[index]);

const spots = (doc: Document): Spot[] => select(items(doc.spots), (spot): boolean => spot.name !== REGISTRATION);

const opened = (path: string): Document => {
    const [open] = select(items(app.documents), (doc): boolean => doc.fullName.fsName === path);
    return open === undefined ? app.open(new File(path)) : open;
};

// --- [ENTRY] ---------------------------------------------------------------------------

const syncSwatches = (request: { readonly source: string; readonly targets: string[]; readonly match: 'rgb' | 'cmyk' }, _at: Site): Reading<JsonObject> => {
    const wasOpen = collect(items(app.documents), (doc): string => doc.fullName.fsName);
    const rows: Row[] = collect(spots(opened(request.source)), (spot): Row => ({ name: spot.name, values: channels(spot.color, request.match) }));
    const renamed: JsonObject[] = [];
    const unmatched: JsonObject[] = [];
    visit(request.targets, (path): void => {
        const target = opened(path);
        visit(spots(target), (spot): void => {
            const renamable = spot;
            const values = channels(spot.color, request.match);
            const [row] = select(rows, (candidate): boolean => same(candidate.values, values));
            if (row === undefined) {
                unmatched.push({ document: path, name: spot.name });
            } else if (row.name !== spot.name) {
                renamed.push({ document: path, from: spot.name, to: row.name });
                renamable.name = row.name;
            }
        });
        if (!contains(wasOpen, path)) {
            target.close(SaveOptions.DONOTSAVECHANGES);
        }
    });
    return { value: { kind: 'swatchesSynced', renamed, unmatched }, unavailable: [] };
};

run(syncSwatches);
