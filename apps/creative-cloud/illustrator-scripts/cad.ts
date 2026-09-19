/// <reference path="./prelude.ts"/>

// --- [PRELUDE] -------------------------------------------------------------------------

const { all, collect, contains, each, flatMap, fold, hosted, items, nth, owned, paths, present, run, select, split, typed }: Prelude = $.evalFile(new File(`${new File($.fileName).path}/prelude.jsx`));

// --- [ENTRY] ---------------------------------------------------------------------------

const cad = (
    request:
        | { readonly mode: 'read'; readonly scope: 'selection' | 'document' }
        | {
              readonly mode: 'write';
              readonly layers: { readonly path: string[]; readonly color: [number, number, number]; readonly visible: boolean; readonly locked: boolean; readonly printable: boolean }[];
              readonly paths: { readonly uuid: string; readonly remove: boolean; readonly weight: number | null; readonly pattern: string | null }[];
              readonly moves: { readonly uuid: string; readonly layer: string[] }[];
              readonly groups: string[];
          },
    at: Site,
): Reading<JsonObject> => {
    const doc = app.activeDocument;
    if (request.mode === 'read') {
        const layers = collect(items(doc.layers), (layer) => ({ layer, path: [layer.name] }));
        for (let index = 0; index < layers.length; index += 1) {
            const row = nth(layers, index);
            layers.push(...collect(items(row.layer.layers), (layer) => ({ layer, path: row.path.concat([layer.name]) })));
        }
        const selected = request.scope === 'selection' ? items<PageItem>(doc.selection) : select(items(doc.pageItems), (item): boolean => item.parent === item.layer);
        const roots = select(selected, (item): boolean => {
            let { parent } = item;
            while (hosted(parent) && typed<GroupItem>('GroupItem')(parent) && !contains<object>(selected, parent)) {
                ({ parent } = parent);
            }
            return !contains<object>(selected, parent);
        });
        const queued = roots.slice();
        const groups: string[] = [];
        for (let index = 0; index < queued.length; index += 1) {
            const item = nth(queued, index);
            if (typed<GroupItem>('GroupItem')(item)) {
                groups.push(item.uuid);
                queued.push(...items(item.pageItems));
            }
        }
        const measured = each(
            at,
            flatMap(queued, paths),
            (path): Reading<JsonObject> =>
                present({
                    uuid: path.uuid,
                    points: path.pathPoints.length,
                    stroked: path.stroked,
                    weight: path.strokeWidth,
                    pattern: path.filled && typed<PatternColor>('PatternColor')(path.fillColor) ? path.fillColor.pattern.name : null,
                }),
        );
        return {
            value: {
                kind: 'cad',
                layers: collect(layers, ({ path }) => path),
                roots: collect(
                    roots,
                    (item): JsonObject => ({
                        uuid: item.uuid,
                        layer: nth(
                            select(layers, ({ layer }): boolean => item.layer === layer),
                            0,
                        ).path,
                    }),
                ),
                paths: measured.value,
                groups,
                patterns: collect(items(doc.patterns), ({ name }): string => name),
            },
            unavailable: measured.unavailable,
        };
    }
    const reached: { readonly layer: Layer; readonly row: (typeof request.layers)[number] }[] = [];
    const unavailable: JsonObject[] = [];
    const results: JsonObject[] = [];
    try {
        const created = each(at, request.layers, (row): Reading<JsonObject> => {
            const layer = fold<string, Document | Layer>(row.path, doc, (parent, name): Layer => owned(parent.layers, name));
            if (!typed<Layer>('Layer')(layer)) {
                throw new Error('The layer path must contain a name.');
            }
            reached.push({ layer, row });
            layer.locked = false;
            layer.visible = true;
            const color = new RGBColor();
            [color.red, color.green, color.blue] = row.color;
            layer.color = color;
            layer.printable = row.printable;
            return present({ kind: 'layer', path: row.path });
        });
        results.push(...created.value);
        unavailable.push(...created.unavailable);
        const normalized = each(at, request.paths, (row): Reading<JsonObject> => {
            const path = doc.getPageItemFromUuid(row.uuid);
            if (!typed<PathItem>('PathItem')(path)) {
                return present({ kind: 'path', uuid: row.uuid, reason: 'notAPath' });
            }
            if (row.remove) {
                path.remove();
                return present({ kind: 'removed', uuid: row.uuid });
            }
            if (row.weight !== null && row.weight !== path.strokeWidth) {
                path.strokeWidth = row.weight;
            }
            if (row.pattern !== null && path.filled && typed<PatternColor>('PatternColor')(path.fillColor) && path.fillColor.pattern.name !== row.pattern) {
                const color = path.fillColor;
                color.pattern = doc.patterns.getByName(row.pattern);
                path.fillColor = color;
            }
            return present({
                kind: 'path',
                uuid: path.uuid,
                weight: path.strokeWidth,
                pattern: path.filled && typed<PatternColor>('PatternColor')(path.fillColor) ? path.fillColor.pattern.name : null,
            });
        });
        results.push(...normalized.value);
        unavailable.push(...normalized.unavailable);
        const moved = each(at, request.moves, (row): Reading<JsonObject> => {
            const destination = fold<string, Document | Layer>(row.layer, doc, (parent, name): Layer => parent.layers.getByName(name));
            const item = doc.getPageItemFromUuid(row.uuid);
            item.move(destination, ElementPlacement.PLACEATEND);
            return present({ kind: 'moved', uuid: row.uuid, layer: row.layer });
        });
        results.push(...moved.value);
        unavailable.push(...moved.unavailable);
        const removed = each(at, request.groups, (uuid): Reading<JsonObject> => {
            const group = doc.getPageItemFromUuid(uuid);
            if (!typed<GroupItem>('GroupItem')(group) || group.pageItems.length > 0) {
                return present({ kind: 'retained', uuid });
            }
            group.remove();
            return present({ kind: 'removed', uuid });
        });
        results.push(...removed.value);
        unavailable.push(...removed.unavailable);
    } finally {
        const finalized = each(
            at,
            reached.reverse(),
            ({ layer, row }, site): Reading<JsonObject> =>
                all(site, [
                    [
                        'order',
                        (): Reading<null> => {
                            layer.zOrder(ZOrderMethod.BRINGTOFRONT);
                            return present(null);
                        },
                    ],
                    [
                        'visible',
                        (): Reading<null> => {
                            layer.visible = row.visible;
                            return present(null);
                        },
                    ],
                    [
                        'locked',
                        (): Reading<null> => {
                            layer.locked = row.locked;
                            return present(null);
                        },
                    ],
                ]),
        );
        unavailable.push(...finalized.unavailable);
    }
    return { value: split(results), unavailable };
};

run(cad);
