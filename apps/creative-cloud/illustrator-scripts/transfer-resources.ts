/// <reference path="./prelude.ts"/>

// --- [HOST] ----------------------------------------------------------------------------

declare const $: $;
declare const app: Application;
declare const PatternColor: new () => PatternColor;

declare global {
    enum ElementPlacement {}
    enum SaveOptions {}
}

// --- [PRELUDE] -------------------------------------------------------------------------

const { collect, contains, fold, items, nth, run, select, swatches, typed, visit }: Prelude = $.evalFile(new File(`${new File($.fileName).path}/prelude.jsx`));

// --- [CARRIERS] ------------------------------------------------------------------------

type Kind = 'swatches' | 'symbols' | 'brushes' | 'graphicStyles' | 'patterns';

interface Named {
    readonly name: string;
}

interface Rows {
    readonly applied: JsonObject[];
    readonly rejected: JsonObject[];
}

const SIDE = 500;
const REGISTRATION = '[Registration]';
const NONE = '[None]';
const NEW_STYLE = 'Adobe New Style Shortcut';

const chosen = <T extends Named>(list: T[], names: string[] | undefined): T[] => (names === undefined ? list : select(list, (item): boolean => contains(names, item.name)));

const present = (collection: { getByName: (name: string) => unknown }, name: string): boolean => {
    try {
        collection.getByName(name);
        return true;
    } catch {
        return false;
    }
};

const spec = (swatch: Swatch): SwatchSpec[] => {
    const value = swatch.color;
    const global = typed<SpotColor>('SpotColor')(value);
    const solid = global ? value.spot.color : value;
    if (typed<RGBColor>('RGBColor')(solid)) {
        return [{ name: swatch.name, model: 'RGB', values: [solid.red, solid.green, solid.blue], global }];
    }
    if (typed<CMYKColor>('CMYKColor')(solid)) {
        return [{ name: swatch.name, model: 'CMYK', values: [solid.cyan, solid.magenta, solid.yellow, solid.black], global }];
    }
    return typed<GrayColor>('GrayColor')(solid) ? [{ name: swatch.name, model: 'Gray', values: [solid.gray], global }] : [];
};

const structure = (source: Document, names: string[] | undefined): SwatchGroupSpec[] =>
    collect(items(source.swatchGroups), (group, index): SwatchGroupSpec => {
        const listed = select(chosen(group.getAllSwatches(), names), (swatch): boolean => swatch.name !== REGISTRATION && swatch.name !== NONE);
        return { name: index === 0 ? '' : group.name, swatches: fold<Swatch, SwatchSpec[]>(listed, [], (flat, swatch): SwatchSpec[] => flat.concat(spec(swatch))) };
    });

const carried = <T extends Named>(kind: Kind, rows: Rows, list: T[], collection: { getByName: (name: string) => unknown }, carry: (item: T) => PageItem, target: Document): void => {
    visit(list, (item): void => {
        if (present(collection, item.name)) {
            rows.rejected.push({ kind, name: item.name, reason: 'nameCollision' });
            return;
        }
        const carrier = carry(item);
        const copy = carrier.duplicate(nth(target.layers, 0), ElementPlacement.PLACEATEND);
        carrier.remove();
        copy.remove();
        if (present(collection, item.name)) {
            rows.applied.push({ kind, name: item.name });
        } else {
            rows.rejected.push({ kind, name: item.name, reason: 'notCarried' });
        }
    });
};

const graphicStyles = (rows: Rows, source: Document, target: Document, names: string[] | undefined): void => {
    visit(chosen(items(source.graphicStyles), names), (style): void => {
        if (present(target.graphicStyles, style.name)) {
            rows.rejected.push({ kind: 'graphicStyles', name: style.name, reason: 'nameCollision' });
            return;
        }
        const carrier = source.pathItems.rectangle(0, 0, SIDE, SIDE);
        style.applyTo(carrier);
        const copy = carrier.duplicate(nth(target.layers, 0), ElementPlacement.PLACEATEND);
        carrier.remove();
        app.activeDocument = target;
        visit(items<PageItem>(target.selection), (item): void => {
            const chosenItem = item;
            chosenItem.selected = false;
        });
        copy.selected = true;
        app.executeMenuCommand(NEW_STYLE);
        const added = nth(target.graphicStyles, target.graphicStyles.length - 1);
        added.name = style.name;
        copy.remove();
        rows.applied.push({ kind: 'graphicStyles', name: style.name });
    });
};

const brushCarrier = (source: Document, brush: Brush): PageItem => {
    const path = source.pathItems.add();
    path.setEntirePath([
        [0, 0],
        [SIDE, 0],
    ]);
    brush.applyTo(path);
    return path;
};

const patternCarrier = (source: Document, pattern: Pattern): PageItem => {
    const rect = source.pathItems.rectangle(0, 0, SIDE, SIDE);
    const fill = new PatternColor();
    fill.pattern = pattern;
    rect.fillColor = fill;
    return rect;
};

const transferred = (kind: Kind, rows: Rows, source: Document, target: Document, names: string[] | undefined): void => {
    if (kind === 'swatches') {
        const imported = swatches(target, structure(source, names), false);
        rows.applied.push(...collect(imported.applied, (row): JsonObject => ({ kind, name: row.swatch })));
        rows.rejected.push(...collect(imported.rejected, (row): JsonObject => ({ kind, name: row.swatch, reason: row.reason })));
    } else if (kind === 'symbols') {
        carried(kind, rows, chosen(items(source.symbols), names), target.symbols, (symbol): PageItem => source.symbolItems.add(symbol), target);
    } else if (kind === 'brushes') {
        carried(kind, rows, chosen(items(source.brushes), names), target.brushes, (brush): PageItem => brushCarrier(source, brush), target);
    } else if (kind === 'patterns') {
        carried(kind, rows, chosen(items(source.patterns), names), target.patterns, (pattern): PageItem => patternCarrier(source, pattern), target);
    } else {
        graphicStyles(rows, source, target, names);
    }
};

// --- [ENTRY] ---------------------------------------------------------------------------

const transferResources = (request: { readonly source: string; readonly kinds: Kind[]; readonly names?: string[] }, _at: Site): Reading<JsonObject> => {
    const target = app.activeDocument;
    const source = app.open(new File(request.source));
    const rows: Rows = { applied: [], rejected: [] };
    try {
        visit(request.kinds, (kind): void => transferred(kind, rows, source, target, request.names));
    } finally {
        source.close(SaveOptions.DONOTSAVECHANGES);
        app.activeDocument = target;
    }
    return { value: { kind: 'resourcesTransferred', applied: rows.applied, rejected: rows.rejected }, unavailable: [] };
};

run(transferResources);
