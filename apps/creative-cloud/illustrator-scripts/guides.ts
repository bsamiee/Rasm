/// <reference path="./prelude.ts"/>

// --- [PRELUDE] -------------------------------------------------------------------------

const { collect, colors, contains, flatMap, flatten, fold, items, named, nth, owned, paths, present, range, run, select, split, typed, visit }: Prelude = $.evalFile(
    new File(`${new File($.fileName).path}/prelude.jsx`),
);

// --- [REQUEST] -------------------------------------------------------------------------

interface Requests {
    readonly menu: { readonly command: string };
    readonly measure: { readonly target: 'first' | 'selection' | 'artboard'; readonly convertAreaText: boolean };
    readonly lockup: {
        readonly source: string;
        readonly layerName: string;
        readonly color: ColorSpec;
        readonly weight: number;
        readonly filled: boolean;
        readonly lines: { readonly from: [number, number]; readonly to: [number, number] }[];
        readonly labels: { readonly text: string; readonly size: number; readonly left: number; readonly top: number; readonly vertical: boolean; readonly width: number }[];
        readonly copies: { readonly position: [number, number]; readonly rotate: number; readonly anchor: 'top' | 'bottom' }[];
    };
    readonly object: {
        readonly layerName: string;
        readonly clearLayer: boolean;
        readonly color: ColorSpec;
        readonly weight: number;
        readonly edges: {
            readonly left: boolean;
            readonly top: boolean;
            readonly right: boolean;
            readonly bottom: boolean;
            readonly centerX: boolean;
            readonly centerY: boolean;
            readonly leftDiagonal: boolean;
            readonly rightDiagonal: boolean;
        };
        readonly margins: { readonly left: number; readonly right: number; readonly top: number; readonly bottom: number };
        readonly extendTo: 'object' | 'artboard' | 'artwork';
        readonly extension: { readonly mode: 'fraction' | 'absolute'; readonly value: number };
        readonly bounds: 'geometric' | 'visible';
        readonly drawAs: 'guides' | 'strokedPaths';
    };
    readonly bento: {
        readonly cells: { readonly x: number; readonly y: number; readonly w: number; readonly h: number }[];
        readonly cornerRadius: number;
        readonly groupName: string;
        readonly removeSelection: boolean;
    };
    readonly isometric: {
        readonly layerName: string;
        readonly sublayerTemplate: string;
        readonly spacing: number;
        readonly angle: number;
        readonly artboards: number[] | 'all';
    };
}

// --- [DRAWING] -------------------------------------------------------------------------

const line = (container: { readonly pathItems: PathItems }, from: [number, number], to: [number, number], weight: number, stroke: Color, filled: boolean): PathItem => {
    const path = container.pathItems.add();
    try {
        path.setEntirePath([from, to]);
        path.stroked = true;
        path.strokeWidth = weight;
        path.strokeColor = stroke;
        path.fillColor = stroke;
        path.filled = filled;
        return path;
    } catch (error) {
        path.remove();
        throw error;
    }
};

const clipped = (rect: Rect, origin: [number, number], direction: [number, number]): [[number, number], [number, number]][] => {
    if (direction[0] === 0 && direction[1] === 0) {
        return [];
    }
    let first = Number.NEGATIVE_INFINITY;
    let last = Number.POSITIVE_INFINITY;
    for (let axis = 0; axis < 2; axis += 1) {
        const low = Math.min(nth(rect, axis), nth(rect, axis + 2));
        const high = Math.max(nth(rect, axis), nth(rect, axis + 2));
        const position = nth(origin, axis);
        const step = nth(direction, axis);
        const outside = position < low || position > high;
        if (step === 0 && outside) {
            return [];
        }
        if (step !== 0) {
            const a = (low - position) / step;
            const b = (high - position) / step;
            first = Math.max(first, Math.min(a, b));
            last = Math.min(last, Math.max(a, b));
        }
    }
    return first < last
        ? [
              [
                  [origin[0] + first * direction[0], origin[1] + first * direction[1]],
                  [origin[0] + last * direction[0], origin[1] + last * direction[1]],
              ],
          ]
        : [];
};

// --- [ENTRY] ---------------------------------------------------------------------------

const guides = (request: { readonly [K in keyof Requests]: { readonly operation: K } & Requests[K] }[keyof Requests], at: Site): Reading<JsonObject> => {
    const doc = app.activeDocument;
    const system = app.coordinateSystem;
    const rows: JsonObject[] = [];
    try {
        switch (request.operation) {
            case 'menu': {
                const count = flatten(items<PageItem>(doc.selection)).length;
                app.executeMenuCommand(request.command);
                rows.push({ operation: request.operation, command: request.command, drawn: count });
                break;
            }
            case 'measure': {
                app.coordinateSystem = CoordinateSystem.ARTBOARDCOORDINATESYSTEM;
                const artboard = nth(doc.artboards, doc.artboards.getActiveArtboardIndex()).artboardRect;
                if (request.target === 'artboard') {
                    rows.push({ operation: request.operation, artboard, items: [] });
                    break;
                }
                const converted = flatMap(items<PageItem>(doc.selection), (item, index): PageItem[] => {
                    if (request.target === 'first' && index > 0) {
                        return [];
                    }
                    if (!(request.convertAreaText && typed<TextFrame>('TextFrame')(item)) || item.kind !== TextType.AREATEXT) {
                        return [item];
                    }
                    const existing: string[] = [];
                    for (let frameIndex = 0; frameIndex < doc.textFrames.length; frameIndex += 1) {
                        existing.push(nth(doc.textFrames, frameIndex).uuid);
                    }
                    const { uuid } = item;
                    item.convertAreaObjectToPointObject();
                    const replaced = select(items(doc.textFrames), (frame): boolean => !contains(existing, frame.uuid));
                    if (replaced.length !== 1) {
                        rows.push({ operation: request.operation, uuid, reason: 'conversionChangedFrameCount', count: replaced.length });
                    }
                    return replaced;
                });
                const listed = collect(
                    converted,
                    (measured): JsonObject => ({
                        uuid: measured.uuid,
                        typename: measured.typename,
                        position: [measured.left, measured.top],
                        width: measured.width,
                        height: measured.height,
                        geometricBounds: measured.geometricBounds,
                    }),
                );
                const pending = select(converted, (): boolean => request.target === 'selection');
                const bounds = fold<PageItem, Rect[]>(pending, [], (found, item): Rect[] => {
                    if (typed<GroupItem>('GroupItem')(item)) {
                        const children = items(item.pageItems);
                        const visible = item.clipped ? select(children, (child): boolean => select(paths(child), ({ clipping }): boolean => clipping).length > 0) : children;
                        if (item.clipped && visible.length === 0) {
                            rows.push({ operation: request.operation, uuid: item.uuid, reason: 'clippingPathMissing' });
                        }
                        pending.push(...visible);
                        return found;
                    }
                    if (typed<CompoundPathItem>('CompoundPathItem')(item)) {
                        pending.push(...items(item.pathItems));
                        return found;
                    }
                    const rect = item.geometricBounds;
                    const [before] = found;
                    return [before === undefined ? rect : [Math.min(before[0], rect[0]), Math.max(before[1], rect[1]), Math.max(before[2], rect[2]), Math.min(before[3], rect[3])]];
                });
                const measurement: JsonObject = { operation: request.operation, artboard, items: listed };
                const [selectionBounds] = bounds;
                if (selectionBounds !== undefined && rows.length === 0) {
                    measurement['selectionBounds'] = selectionBounds;
                }
                rows.push(measurement);
                break;
            }
            case 'lockup': {
                const selected = doc.getPageItemFromUuid(request.source);
                const paint = request.lines.length + request.labels.length === 0 ? present({ values: [] }) : colors(doc, [request.color], false, at);
                if ('rejected' in paint.value) {
                    return { value: split(collect(paint.value.rejected, ({ name, reason }): JsonObject => ({ operation: request.operation, color: name, reason }))), unavailable: paint.unavailable };
                }
                app.coordinateSystem = CoordinateSystem.ARTBOARDCOORDINATESYSTEM;
                const { values } = paint.value;
                const grid = owned(doc.layers, request.layerName);
                try {
                    grid.locked = false;
                    visit(request.copies, (copy): void => {
                        const duplicate = selected.duplicate();
                        try {
                            duplicate.rotate(copy.rotate);
                            duplicate.position = [copy.position[0], copy.anchor === 'bottom' ? copy.position[1] + duplicate.height : copy.position[1]];
                            duplicate.move(grid, ElementPlacement.PLACEATEND);
                        } catch (error) {
                            duplicate.remove();
                            throw error;
                        }
                    });
                    visit(request.lines, ({ from, to }): void => {
                        line(grid, from, to, request.weight, nth(values, 0), request.filled);
                    });
                    visit(request.labels, (label): void => {
                        const frame = grid.textFrames.add();
                        try {
                            frame.contents = label.text;
                            const attributes = frame.textRange.characterAttributes;
                            attributes.size = label.size;
                            attributes.fillColor = nth(values, 0);
                            frame.position = label.vertical ? [label.left + (label.width - frame.width) / 2, label.top] : [label.left, label.top - frame.height / 2];
                        } catch (error) {
                            frame.remove();
                            throw error;
                        }
                    });
                } finally {
                    grid.locked = true;
                }
                return { value: split([{ operation: request.operation, drawn: request.lines.length }]), unavailable: paint.unavailable };
            }
            case 'object': {
                const selected = items<PageItem>(doc.selection);
                const { scaleFactor: scale } = doc;
                const { margins, edges } = request;
                const artboard = nth(doc.artboards, doc.artboards.getActiveArtboardIndex()).artboardRect;
                const frame = request.extendTo === 'artboard' ? artboard : doc.visibleBounds;
                const planned = collect(selected, (item) => {
                    const bounds = request.bounds === 'geometric' ? item.geometricBounds : item.visibleBounds;
                    const [left, top, right, bottom] = [bounds[0] - margins.left / scale, bounds[1] + margins.top / scale, bounds[2] + margins.right / scale, bounds[3] - margins.bottom / scale];
                    const width = request.extendTo === 'artboard' ? artboard[2] - artboard[0] : right - left;
                    const height = request.extendTo === 'artboard' ? artboard[1] - artboard[3] : top - bottom;
                    const centerX = (left + right) / 2;
                    const centerY = (top + bottom) / 2;
                    const diagonal = (width * width + height * height) ** (1 / 2);
                    const segments: [boolean, [number, number], [number, number], number][] = [
                        [edges.top, [left, top], [right, top], width],
                        [edges.bottom, [left, bottom], [right, bottom], width],
                        [edges.centerY, [left, centerY], [right, centerY], width],
                        [edges.left, [left, top], [left, bottom], height],
                        [edges.right, [right, top], [right, bottom], height],
                        [edges.centerX, [centerX, top], [centerX, bottom], height],
                        [edges.leftDiagonal, [left, top], [right, bottom], diagonal],
                        [edges.rightDiagonal, [right, top], [left, bottom], diagonal],
                    ];
                    const ends = flatMap(
                        select(segments, ([enabled]): boolean => enabled),
                        ([, from, to, dimension]): [[number, number], [number, number]][] => {
                            const dx = to[0] - from[0];
                            const dy = to[1] - from[1];
                            const length = (dx * dx + dy * dy) ** (1 / 2);
                            if (length === 0) {
                                return [];
                            }
                            const [extent]: [[number, number], [number, number]][] = request.extendTo === 'object' ? [[from, to]] : clipped(frame, from, [dx, dy]);
                            if (extent === undefined) {
                                return [];
                            }
                            const [start, end] = extent;
                            const extension = request.extension.mode === 'fraction' ? dimension * request.extension.value : request.extension.value / scale;
                            const distance = request.extendTo === 'artwork' ? 0 : extension;
                            const span = ((end[0] - start[0]) * dx + (end[1] - start[1]) * dy) / length;
                            return span + 2 * distance > 0
                                ? [
                                      [
                                          [start[0] - (dx * distance) / length, start[1] - (dy * distance) / length],
                                          [end[0] + (dx * distance) / length, end[1] + (dy * distance) / length],
                                      ],
                                  ]
                                : [];
                        },
                    );
                    return { name: item.name, ends };
                });
                const drawable = select(planned, ({ ends }): boolean => ends.length > 0);
                const paint = drawable.length === 0 ? present({ values: [] }) : colors(doc, [request.color], false, at);
                if ('rejected' in paint.value) {
                    return { value: split(collect(paint.value.rejected, ({ name, reason }): JsonObject => ({ operation: request.operation, color: name, reason }))), unavailable: paint.unavailable };
                }
                visit(request.clearLayer ? named(doc.layers, request.layerName) : [], (layer): void => {
                    visit(items(layer.pageItems), (item): void => item.remove());
                });
                if (drawable.length === 0) {
                    return present(split([{ operation: request.operation, drawn: 0 }]));
                }
                const target = owned(doc.layers, request.layerName);
                const stroke = nth(paint.value.values, 0);
                let count = 0;
                visit(drawable, ({ name, ends }): void => {
                    const group = target.groupItems.add();
                    try {
                        group.name = `${name}_guides`;
                        for (let index = 0; index < ends.length; index += 1) {
                            const [from, to] = nth(ends, index);
                            const path = line(group, from, to, request.weight, stroke, false);
                            path.guides = request.drawAs === 'guides';
                            count += 1;
                        }
                    } catch (error) {
                        group.remove();
                        throw error;
                    }
                });
                target.zOrder(ZOrderMethod.BRINGTOFRONT);
                return { value: split([{ operation: request.operation, drawn: count }]), unavailable: paint.unavailable };
            }
            case 'bento': {
                const selected = request.removeSelection ? items<PageItem>(doc.selection) : [];
                const container = doc.activeLayer;
                const group = container.groupItems.add();
                try {
                    group.name = request.groupName;
                    visit(request.cells, (cell): void => {
                        group.pathItems.roundedRectangle(cell.y, cell.x, cell.w, cell.h, request.cornerRadius, request.cornerRadius);
                    });
                } catch (error) {
                    group.remove();
                    throw error;
                }
                visit(selected, (item): void => item.remove());
                doc.selection = null;
                rows.push({ operation: request.operation, drawn: request.cells.length });
                break;
            }
            case 'isometric': {
                app.coordinateSystem = CoordinateSystem.DOCUMENTCOORDINATESYSTEM;
                const indices = flatMap(request.artboards === 'all' ? range(doc.artboards.length) : request.artboards, (index): number[] => {
                    if (index >= 0 && index < doc.artboards.length) {
                        return [index];
                    }
                    rows.push({ operation: request.operation, index, count: doc.artboards.length, reason: 'artboardOutOfRange' });
                    return [];
                });
                if (indices.length === 0) {
                    return present(split(rows));
                }
                const parent = owned(doc.layers, request.layerName);
                const spacing = request.spacing / 2;
                const degreesInHalfTurn = 180;
                const drawn = collect(indices, (index): number => {
                    const artboard = nth(doc.artboards, index);
                    const rect = artboard.artboardRect;
                    const [left, top, right, bottom] = rect;
                    const center: [number, number] = [(left + right) / 2, (top + bottom) / 2];
                    const corners: [number, number][] = [
                        [left, top],
                        [right, top],
                        [right, bottom],
                        [left, bottom],
                    ];
                    const segments = flatMap([0, -request.angle, request.angle], (angle): [[number, number], [number, number]][] => {
                        const radians = (angle * Math.PI) / degreesInHalfTurn;
                        const normal: [number, number] = [Math.cos(radians), Math.sin(radians)];
                        const direction: [number, number] = [-normal[1], normal[0]];
                        const projections = collect(corners, (point): number => ((point[0] - center[0]) * normal[0] + (point[1] - center[1]) * normal[1]) / spacing);
                        const first = Math.ceil(Math.min(...projections));
                        const last = Math.floor(Math.max(...projections));
                        return flatMap(range(last - first + 1), (offset): [[number, number], [number, number]][] =>
                            clipped(rect, [center[0] + (first + offset) * spacing * normal[0], center[1] + (first + offset) * spacing * normal[1]], direction),
                        );
                    });
                    const sub = parent.layers.add();
                    try {
                        sub.name = request.sublayerTemplate.split('%a').join(artboard.name);
                        const group = sub.groupItems.add();
                        const clip = group.pathItems.rectangle(top, left, right - left, top - bottom);
                        clip.clipping = true;
                        clip.filled = false;
                        clip.stroked = false;
                        visit(segments, ([from, to]): void => {
                            line(group, from, to, 1, doc.defaultStrokeColor, false);
                        });
                        clip.move(group, ElementPlacement.PLACEATBEGINNING);
                        group.clipped = true;
                        return segments.length;
                    } catch (error) {
                        sub.remove();
                        throw error;
                    }
                });
                rows.push({ operation: request.operation, drawn: fold(drawn, 0, (count, amount): number => count + amount) });
                break;
            }
        }
        return present(split(rows));
    } finally {
        app.coordinateSystem = system;
    }
};

run(guides);
