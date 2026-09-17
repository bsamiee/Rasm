/// <reference path="./prelude.ts"/>

// --- [HOST] ----------------------------------------------------------------------------

declare const $: $;
declare const app: Application;

declare global {
    enum ElementPlacement {}
    enum SaveOptions {}
}

// --- [PRELUDE] -------------------------------------------------------------------------

const { collect, contains, document, items, run, select, swatches, visit }: Prelude = $.evalFile(new File(`${new File($.fileName).path}/prelude.jsx`));

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

const rgbOf = (value: Color): number[] => {
    const rgb = value as RGBColor;
    return [rgb.red, rgb.green, rgb.blue];
};

const cmykOf = (value: Color): number[] => {
    const cmyk = value as CMYKColor;
    return [cmyk.cyan, cmyk.magenta, cmyk.yellow, cmyk.black];
};

const spec = (swatch: Swatch): SwatchSpec[] => {
    const value = swatch.color;
    const global = value.typename === 'SpotColor';
    const solid = global ? (value as SpotColor).spot.color : value;
    if (solid.typename === 'RGBColor') {
        return [{ name: swatch.name, model: 'RGB', values: rgbOf(solid), global }];
    }
    if (solid.typename === 'CMYKColor') {
        return [{ name: swatch.name, model: 'CMYK', values: cmykOf(solid), global }];
    }
    if (solid.typename === 'GrayColor') {
        return [{ name: swatch.name, model: 'Gray', values: [(solid as GrayColor).gray], global }];
    }
    return [];
};

const structure = (source: Document, names: string[] | undefined): SwatchGroupSpec[] =>
    collect(items(source.swatchGroups), (group, index): SwatchGroupSpec => {
        const listed = select(chosen(group.getAllSwatches(), names), (swatch): boolean => swatch.name !== REGISTRATION && swatch.name !== NONE);
        const specs: SwatchSpec[] = [];
        visit(listed, (swatch): void => {
            specs.push(...spec(swatch));
        });
        return { name: index === 0 ? '' : group.name, swatches: specs };
    });

const carried = <T extends Named>(kind: Kind, rows: Rows, list: T[], collection: { getByName: (name: string) => unknown }, carry: (item: T) => PageItem, target: Document): void => {
    visit(list, (item): void => {
        if (present(collection, item.name)) {
            rows.rejected.push({ kind, name: item.name, reason: 'nameCollision' });
            return;
        }
        const carrier = carry(item);
        const copy = carrier.duplicate(target.layers[0], ElementPlacement.PLACEATEND);
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
        const copy = carrier.duplicate(target.layers[0], ElementPlacement.PLACEATEND);
        carrier.remove();
        app.activeDocument = target;
        app.activeDocument.selection = [copy];
        app.executeMenuCommand(NEW_STYLE);
        const last = target.graphicStyles.length - 1;
        const added = target.graphicStyles[last] as ArtStyle;
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
    const fill: PatternColor = new $.global.PatternColor();
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
    const source = document(request.source);
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
