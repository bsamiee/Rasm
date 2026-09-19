/// <reference path="./prelude.ts"/>

// --- [PRELUDE] -------------------------------------------------------------------------

const { collect, colors, contains, each, failure, flatten, fold, items, named, nth, paths, present, run, select, split }: Prelude = $.evalFile(new File(`${new File($.fileName).path}/prelude.jsx`));

// --- [ENTRY] ---------------------------------------------------------------------------

const strokeStyles = (
    request: {
        readonly createStyle?: string;
        readonly styles: {
            readonly name: string;
            readonly weight: number;
            readonly dash: number[];
            readonly cap: keyof typeof StrokeCap & string;
            readonly join: keyof typeof StrokeJoin & string;
            readonly miterLimit: number;
            readonly strokeColor: ColorSpec;
            readonly fillColor: ColorSpec[];
            readonly blendMode: keyof typeof BlendModes & string;
            readonly targets: 'selection' | string[];
        }[];
    },
    at: Site,
): Reading<JsonObject> => {
    const doc = app.activeDocument;
    const selected = doc.selection;
    const temporary = request.createStyle === undefined ? [] : [doc.pathItems.add()];
    const unavailable: JsonObject[] = [];
    let styled: Reading<JsonObject[][]>;
    try {
        styled = each(at, request.styles, (style, site): Reading<JsonObject[]> => {
            if (request.createStyle !== undefined && named(doc.graphicStyles, style.name).length > 0) {
                return present([{ name: style.name, reason: 'nameCollision' }]);
            }
            const reached = (style.targets === 'selection' ? flatten(items<PageItem>(selected)) : []).concat(temporary);
            const resolved = each({ path: `${site.path}.targets`, chain: site.chain }, style.targets === 'selection' ? [] : style.targets, (uuid): Reading<string> => {
                reached.push(doc.getPageItemFromUuid(uuid));
                return present(uuid);
            });
            const carriers = collect(reached, (item) => ({ item, list: paths(item) }));
            const paintable = select(carriers, ({ list }): boolean => list.length > 0);
            const rejected = collect(
                select(carriers, ({ list }): boolean => list.length === 0),
                ({ item }): JsonObject => ({ name: style.name, uuid: item.uuid, typename: item.typename, reason: 'notAPath' }),
            ).concat(
                collect(
                    select(style.targets === 'selection' ? [] : style.targets, (uuid): boolean => !contains(resolved.value, uuid)),
                    (uuid): JsonObject => ({ name: style.name, uuid, reason: 'nativeLookupFailed' }),
                ),
            );
            if (paintable.length === 0) {
                return { value: rejected.concat([{ name: style.name, items: 0 }]), unavailable: resolved.unavailable };
            }
            const paint = colors(doc, [style.strokeColor].concat(style.fillColor), false, site);
            if ('rejected' in paint.value) {
                return {
                    value: rejected.concat(collect(paint.value.rejected, ({ name, reason }): JsonObject => ({ name: style.name, color: name, reason }))),
                    unavailable: resolved.unavailable.concat(paint.unavailable),
                };
            }
            const stroke = nth(paint.value.values, 0);
            const [, fill] = paint.value.values;
            const painted = each({ path: `${site.path}.items`, chain: site.chain }, paintable, ({ list }): Reading<null> => {
                for (let index = 0; index < list.length; index += 1) {
                    const path = nth(list, index);
                    path.stroked = true;
                    path.strokeWidth = style.weight;
                    path.strokeDashes = style.dash;
                    path.strokeCap = StrokeCap[style.cap];
                    path.strokeJoin = StrokeJoin[style.join];
                    path.strokeMiterLimit = style.miterLimit;
                    path.strokeColor = stroke;
                    path.filled = fill !== undefined;
                    if (fill !== undefined) {
                        path.fillColor = fill;
                    }
                    path.blendingMode = BlendModes[style.blendMode];
                }
                return present(null);
            });
            if (request.createStyle === undefined || painted.unavailable.length > 0) {
                return {
                    value: request.createStyle === undefined ? rejected.concat([{ name: style.name, items: painted.value.length }]) : rejected,
                    unavailable: resolved.unavailable.concat(paint.unavailable, painted.unavailable),
                };
            }
            const existing = items(doc.graphicStyles);
            const [left, top, right] = nth(doc.artboards, doc.artboards.getActiveArtboardIndex()).artboardRect;
            nth(temporary, 0).setEntirePath([
                [left, top],
                [right, top],
            ]);
            doc.selection = temporary;
            app.executeMenuCommand(request.createStyle);
            const created = select(items(doc.graphicStyles), (candidate): boolean => !contains(existing, candidate));
            if (created.length !== 1) {
                const disposed = each(site, created, (candidate): Reading<null> => {
                    candidate.remove();
                    return present(null);
                });
                return {
                    value: [{ name: style.name, reason: 'nativeCreationDiffers', count: created.length }],
                    unavailable: resolved.unavailable.concat(paint.unavailable, painted.unavailable, disposed.unavailable),
                };
            }
            const resource = nth(created, 0);
            try {
                resource.name = style.name;
            } catch (error) {
                unavailable.push(failure(error, site));
                resource.remove();
                return present([]);
            }
            return { value: [{ name: style.name, items: 0, resource: resource.name }], unavailable: resolved.unavailable.concat(paint.unavailable, painted.unavailable) };
        });
    } finally {
        const disposed = each(at, temporary, (item): Reading<null> => {
            item.remove();
            return present(null);
        });
        unavailable.push(...disposed.unavailable);
        if (request.createStyle !== undefined) {
            try {
                doc.selection = selected;
            } catch (error) {
                unavailable.push(failure(error, at));
            }
        }
    }
    return { value: split(fold<JsonObject[], JsonObject[]>(styled.value, [], (flat, rows): JsonObject[] => flat.concat(rows))), unavailable: styled.unavailable.concat(unavailable) };
};

run(strokeStyles);
