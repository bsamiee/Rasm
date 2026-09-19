/// <reference path="./prelude.ts"/>

// --- [PRELUDE] -------------------------------------------------------------------------

const { collect, colors, contains, each, failure, flatMap, items, named, nth, owned, present, range, replacement, replaceStops, run, select, spec, split, typed, visit }: Prelude = $.evalFile(
    new File(`${new File($.fileName).path}/prelude.jsx`),
);

// --- [CARRIERS] ------------------------------------------------------------------------

const chosen = <T extends { readonly name: string }>(list: T[], names: 'all' | string[]): T[] => (names === 'all' ? list : select(list, (item): boolean => contains(names, item.name)));

const carried = <T extends { readonly name: string }>(
    at: Site,
    list: T[],
    collection: { readonly getByName: (name: string) => unknown },
    carry: (item: T) => PageItem,
    target: Layer,
): Reading<JsonObject[]> =>
    each(at, list, (item): Reading<JsonObject> => {
        if (named(collection, item.name).length > 0) {
            return present({ name: item.name, reason: 'nameCollision' });
        }
        carry(item).duplicate(target, ElementPlacement.PLACEATEND);
        return present(named(collection, item.name).length > 0 ? { name: item.name } : { name: item.name, reason: 'notCarried' });
    });

const CARRIERS = {
    swatches: (source, target, names, replaceByName, at, layers): Reading<JsonObject[]> => {
        const reading = each(
            at,
            flatMap(items(source.swatchGroups), (group, index) => collect(chosen(group.getAllSwatches(), names), (swatch) => ({ swatch, index, group: group.name }))),
            ({ swatch, index, group }, site): Reading<JsonObject[]> => {
                const value = swatch.color;
                const checked = replacement(target, swatch.name, value, replaceByName);
                if ('reason' in checked) {
                    return present([{ name: swatch.name, reason: checked.reason }]);
                }
                const paint = colors(target, spec(value), replaceByName, site);
                if ('rejected' in paint.value) {
                    return { value: collect(paint.value.rejected, ({ reason }): JsonObject => ({ name: swatch.name, reason })), unavailable: paint.unavailable };
                }
                const copied: Color[] = [];
                if (paint.value.values.length > 0) {
                    owned(target.swatches, swatch.name).color = nth(paint.value.values, 0);
                }
                if (paint.value.values.length === 0) {
                    const carrier = nth(layers, 0).pathItems.rectangle(0, 0, 1, 1);
                    carrier.strokeColor = new NoColor();
                    carrier.fillColor = value;
                    copied.push(carrier.duplicate(nth(layers, 1), ElementPlacement.PLACEATEND).fillColor);
                }
                const resources = flatMap(copied, (painted): (Gradient | Pattern)[] => {
                    if (typed<GradientColor>('GradientColor')(painted)) {
                        return [painted.gradient];
                    }
                    return typed<PatternColor>('PatternColor')(painted) ? [painted.pattern] : [];
                });
                if (resources.length < copied.length) {
                    return present([{ name: swatch.name, reason: 'notCarried' }]);
                }
                const [previous] = select(
                    collect(checked.swatches, ({ color }): Color => color),
                    typed<GradientColor>('GradientColor'),
                );
                const [imported] = select(copied, typed<GradientColor>('GradientColor'));
                const [resource] = resources;
                const importedName = resource === undefined ? swatch.name : resource.name;
                if (previous !== undefined && imported !== undefined && previous.gradient !== imported.gradient) {
                    replaceStops(previous.gradient, items(imported.gradient.gradientStops));
                    previous.gradient.type = imported.gradient.type;
                    target.swatches.getByName(imported.gradient.name).remove();
                }
                const canonical = previous === undefined ? importedName : previous.gradient.name;
                const added = target.swatches.getByName(canonical);
                const owner = index === 0 ? nth(target.swatchGroups, 0) : owned(target.swatchGroups, group);
                owner.addSwatch(added);
                return present([{ name: added.name }]);
            },
        );
        return { value: Array.prototype.concat.apply([], reading.value), unavailable: reading.unavailable };
    },
    symbols: (source, target, names, _replaceByName, at, layers): Reading<JsonObject[]> =>
        carried(at, chosen(items(source.symbols), names), target.symbols, (symbol): PageItem => nth(layers, 0).symbolItems.add(symbol), nth(layers, 1)),
    brushes: (source, target, names, _replaceByName, at, layers): Reading<JsonObject[]> =>
        carried(
            at,
            chosen(items(source.brushes), names),
            target.brushes,
            (brush): PageItem => {
                const path = nth(layers, 0).pathItems.add();
                path.fillColor = new NoColor();
                path.setEntirePath([
                    [0, 0],
                    [1, 0],
                ]);
                brush.applyTo(path);
                return path;
            },
            nth(layers, 1),
        ),
    patterns: (source, target, names, _replaceByName, at, layers): Reading<JsonObject[]> =>
        carried(
            at,
            chosen(items(source.patterns), names),
            target.patterns,
            (pattern): PageItem => {
                const path = nth(layers, 0).pathItems.rectangle(0, 0, 1, 1);
                const fill = new PatternColor();
                fill.pattern = pattern;
                path.strokeColor = new NoColor();
                path.fillColor = fill;
                return path;
            },
            nth(layers, 1),
        ),
    graphicStyles: (source, target, names, _replaceByName, at, layers): Reading<JsonObject[]> =>
        carried(
            at,
            chosen(items(source.graphicStyles), names),
            target.graphicStyles,
            (style): PageItem => {
                const carrier = nth(layers, 0).pathItems.rectangle(0, 0, 1, 1);
                style.applyTo(carrier);
                return carrier;
            },
            nth(layers, 1),
        ),
} as const satisfies { readonly [kind: string]: (source: Document, target: Document, names: 'all' | string[], replaceByName: boolean, at: Site, layers: Layer[]) => Reading<JsonObject[]> };

// --- [ENTRY] ---------------------------------------------------------------------------

const transferResources = (
    request: {
        readonly source: string;
        readonly kinds: (keyof typeof CARRIERS)[];
        readonly names: 'all' | string[];
        readonly replaceSwatchesByName: boolean;
    },
    at: Site,
): Reading<JsonObject> => {
    const target = app.activeDocument;
    const retained = items(app.documents);
    const source = app.open(new File(request.source));
    const selections: { readonly document: Document; readonly selection: Document['selection']; readonly layer: Layer }[] = [];
    const layers: Layer[] = [];
    const transferred: JsonObject[] = [];
    const unavailable: JsonObject[] = [];
    try {
        visit([source, target], (document): void => {
            selections.push({ document, selection: document.selection, layer: document.activeLayer });
        });
        visit([source, target], (document): void => {
            layers.push(document.layers.add());
        });
        const reading = each(at, request.kinds, (kind, site): Reading<JsonObject[]> => {
            const rows = CARRIERS[kind](source, target, request.names, request.replaceSwatchesByName, site, layers);
            visit(range(rows.value.length), (index): void => {
                nth(rows.value, index)['kind'] = kind;
            });
            return rows;
        });
        Array.prototype.push.apply(transferred, Array.prototype.concat.apply([], reading.value));
        Array.prototype.push.apply(unavailable, reading.unavailable);
        const available = flatMap(request.kinds, (kind): string[] => collect(items<{ readonly name: string }>(source[kind]), ({ name }): string => name));
        const missing = request.names === 'all' ? [] : select(request.names, (name): boolean => !contains(available, name));
        Array.prototype.push.apply(
            transferred,
            collect(missing, (name): JsonObject => ({ name, reason: 'notFound' })),
        );
    } catch (error) {
        unavailable.push(failure(error, at));
    } finally {
        const removed = each(at, layers.reverse(), (layer): Reading<null> => {
            layer.remove();
            return present(null);
        });
        const selected = each(at, selections, ({ document, selection }): Reading<null> => {
            document.selection = selection;
            return present(null);
        });
        const restored = each(at, selections, ({ document, layer }): Reading<null> => {
            document.activeLayer = layer;
            return present(null);
        });
        const closed = each(at, contains(retained, source) ? [] : [source], (document): Reading<null> => {
            document.close(SaveOptions.DONOTSAVECHANGES);
            return present(null);
        });
        const active = each(at, [target], (document): Reading<null> => {
            app.activeDocument = document;
            return present(null);
        });
        Array.prototype.push.apply(unavailable, removed.unavailable.concat(selected.unavailable, restored.unavailable, closed.unavailable, active.unavailable));
    }
    return { value: split(transferred), unavailable };
};

run(transferResources);
