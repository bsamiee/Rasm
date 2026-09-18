/// <reference path="./prelude.ts"/>

declare global {
    enum ColorModel {}
    enum ElementPlacement {}
    enum SaveOptions {}
}

// --- [PRELUDE] -------------------------------------------------------------------------

const { collect, contains, flatMap, items, named, nth, present, run, select, spec, split, swatches, typed, visit }: Prelude = $.evalFile(new File(`${new File($.fileName).path}/prelude.jsx`));

// --- [CARRIERS] ------------------------------------------------------------------------

const SIDE = 500;

const chosen = <T extends { readonly name: string }>(list: T[], names: 'all' | string[]): T[] => (names === 'all' ? list : select(list, (item): boolean => contains(names, item.name)));

const carried = <T extends { readonly name: string }>(list: T[], collection: { readonly getByName: (name: string) => unknown }, carry: (item: T) => PageItem, target: Document): JsonObject[] =>
    collect(list, (item): JsonObject => {
        if (named(collection, item.name).length > 0) {
            return { name: item.name, reason: 'nameCollision' };
        }
        const carrier = carry(item);
        const copy = carrier.duplicate(nth(target.layers, 0), ElementPlacement.PLACEATEND);
        carrier.remove();
        copy.remove();
        return named(collection, item.name).length > 0 ? { name: item.name } : { name: item.name, reason: 'notCarried' };
    });

const specs = (group: SwatchGroup, names: 'all' | string[]): SwatchSpec[] =>
    flatMap(chosen(group.getAllSwatches(), names), (swatch): SwatchSpec[] => {
        const value = swatch.color;
        const registration = typed<SpotColor>('SpotColor')(value) && value.spot.colorType === ColorModel.REGISTRATION;
        return registration
            ? []
            : collect(spec(value), (read): SwatchSpec => (read.model === 'Spot' ? { name: swatch.name, global: true, color: read.ink } : { name: swatch.name, global: false, color: read }));
    });

const CARRIERS = {
    swatches: (source, target, names): JsonObject[] => {
        const [root, ...groups] = items(source.swatchGroups);
        return swatches(target, { root: root === undefined ? [] : specs(root, names), groups: collect(groups, (group) => ({ name: group.name, swatches: specs(group, names) })) }, false);
    },
    symbols: (source, target, names): JsonObject[] => carried(chosen(items(source.symbols), names), target.symbols, (symbol): PageItem => source.symbolItems.add(symbol), target),
    brushes: (source, target, names): JsonObject[] =>
        carried(
            chosen(items(source.brushes), names),
            target.brushes,
            (brush): PageItem => {
                const path = source.pathItems.add();
                path.setEntirePath([
                    [0, 0],
                    [SIDE, 0],
                ]);
                brush.applyTo(path);
                return path;
            },
            target,
        ),
    patterns: (source, target, names): JsonObject[] =>
        carried(
            chosen(items(source.patterns), names),
            target.patterns,
            (pattern): PageItem => {
                const rect = source.pathItems.rectangle(0, 0, SIDE, SIDE);
                const fill = new PatternColor();
                fill.pattern = pattern;
                rect.fillColor = fill;
                return rect;
            },
            target,
        ),
    graphicStyles: (source, target, names, commands): JsonObject[] =>
        collect(chosen(items(source.graphicStyles), names), (style): JsonObject => {
            if (named(target.graphicStyles, style.name).length > 0) {
                return { name: style.name, reason: 'nameCollision' };
            }
            const carrier = source.pathItems.rectangle(0, 0, SIDE, SIDE);
            style.applyTo(carrier);
            const copy = carrier.duplicate(nth(target.layers, 0), ElementPlacement.PLACEATEND);
            carrier.remove();
            app.activeDocument = target;
            visit(items<PageItem>(target.selection), (item): void => {
                const deselected = item;
                deselected.selected = false;
            });
            copy.selected = true;
            app.executeMenuCommand(commands.newGraphicStyle);
            const added = nth(target.graphicStyles, target.graphicStyles.length - 1);
            added.name = style.name;
            copy.remove();
            return { name: style.name };
        }),
} as const satisfies { readonly [kind: string]: (source: Document, target: Document, names: 'all' | string[], commands: { readonly newGraphicStyle: string }) => JsonObject[] };

// --- [ENTRY] ---------------------------------------------------------------------------

const transferResources = (request: {
    readonly source: string;
    readonly kinds: (keyof typeof CARRIERS)[];
    readonly names: 'all' | string[];
    readonly commands: { readonly newGraphicStyle: string };
}): Reading<JsonObject> => {
    const target = app.activeDocument;
    const source = app.open(new File(request.source));
    try {
        return present(
            split(
                flatMap(request.kinds, (kind): JsonObject[] => {
                    const rows = CARRIERS[kind](source, target, request.names, request.commands);
                    visit(rows, (row): void => {
                        const tagged = row;
                        tagged['kind'] = kind;
                    });
                    return rows;
                }),
            ),
        );
    } finally {
        source.close(SaveOptions.DONOTSAVECHANGES);
        app.activeDocument = target;
    }
};

run(transferResources);
