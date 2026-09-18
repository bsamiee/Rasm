/// <reference path="./prelude.ts"/>

declare global {
    enum CoordinateSystem {}
    enum ElementPlacement {}
    enum TextType {}
    enum Transformation {}
    enum ZOrderMethod {}
}

// --- [PRELUDE] -------------------------------------------------------------------------

const { collect, color, flatMap, flatten, fold, items, nth, owned, present, range, run, select, split, typed, visit }: Prelude = $.evalFile(new File(`${new File($.fileName).path}/prelude.jsx`));

// --- [REQUEST] -------------------------------------------------------------------------

interface Requests {
    readonly menu: { readonly command: string };
    readonly measure: { readonly convertAreaText: boolean };
    readonly lockup: {
        readonly layerName: string;
        readonly color: ColorSpec;
        readonly weight: number;
        readonly filled: boolean;
        readonly lines: { readonly from: [number, number]; readonly to: [number, number] }[];
        readonly labels: { readonly text: string; readonly size: number; readonly left: number; readonly top: number; readonly vertical: boolean; readonly width: number }[];
        readonly copies: { readonly position: [number, number]; readonly rotate: number }[];
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
        readonly extendTo: 'object' | 'artboard' | 'canvas';
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

interface Frame {
    readonly left: number;
    readonly top: number;
    readonly right: number;
    readonly bottom: number;
}

interface Segment {
    readonly from: [number, number];
    readonly to: [number, number];
    readonly dimension: number;
}

// --- [DRAWING] -------------------------------------------------------------------------

const line = (container: { readonly pathItems: PathItems }, from: [number, number], to: [number, number], weight: number, stroke: Color, filled: boolean): PathItem => {
    const path = container.pathItems.add();
    path.setEntirePath([from, to]);
    path.stroked = true;
    path.strokeWidth = weight;
    path.strokeColor = stroke;
    path.fillColor = stroke;
    path.filled = filled;
    return path;
};

const rectangleOf = (bounds: Rect): Frame => ({ left: bounds[0], top: bounds[1], right: bounds[2], bottom: bounds[3] });

const within = ([x, y]: [number, number], rect: Frame): boolean => x >= rect.left && x <= rect.right && y >= rect.bottom && y <= rect.top;

const ends = (request: Requests['object'], frame: Frame, scale: number, segment: Segment): [[number, number], [number, number]][] => {
    const d = request.extension.mode === 'fraction' ? segment.dimension * request.extension.value : request.extension.value / scale;
    const dx = segment.to[0] - segment.from[0];
    const dy = segment.to[1] - segment.from[1];
    const length = (dx * dx + dy * dy) ** (1 / 2);
    const ux = dx / length;
    const uy = dy / length;
    const from: [number, number] = [segment.from[0] - ux * d, segment.from[1] - uy * d];
    const to: [number, number] = [segment.to[0] + ux * d, segment.to[1] + uy * d];
    if (request.extendTo === 'object') {
        return [[from, to]];
    }
    if (within(from, frame) && within(to, frame)) {
        return [];
    }
    const diagonal = from[0] !== to[0] && from[1] !== to[1];
    if (!diagonal) {
        return [[from, to]];
    }
    const k = (to[1] - from[1]) / (to[0] - from[0]);
    const b = from[1] - k * from[0];
    const candidates: [number, number][] = [
        [frame.left, k * frame.left + b],
        [frame.right, k * frame.right + b],
        [(frame.top - b) / k, frame.top],
        [(frame.bottom - b) / k, frame.bottom],
    ];
    const points = select(candidates, (point): boolean => within(point, frame));
    return points.length === 2 ? [[nth(points, 0), nth(points, 1)]] : [];
};

// --- [OPERATIONS] ----------------------------------------------------------------------

const OPERATIONS: { readonly [K in keyof Requests]: (doc: Document, request: Requests[K]) => JsonObject } = {
    menu: (doc, request) => {
        const count = flatten(items<PageItem>(doc.selection)).length;
        app.executeMenuCommand(request.command);
        return { command: request.command, drawn: count };
    },
    measure: (doc, request) => {
        app.coordinateSystem = CoordinateSystem.ARTBOARDCOORDINATESYSTEM;
        const artboard = nth(doc.artboards, doc.artboards.getActiveArtboardIndex()).artboardRect;
        const listed = collect(items<PageItem>(doc.selection), (item): JsonObject => {
            const areaText = request.convertAreaText && typed<TextFrame>('TextFrame')(item);
            const measured = areaText && item.kind !== TextType.POINTTEXT ? item.convertAreaObjectToPointObject() : item;
            return {
                uuid: measured.uuid,
                typename: measured.typename,
                position: [measured.left, measured.top],
                width: measured.width,
                height: measured.height,
                geometricBounds: measured.geometricBounds,
            };
        });
        return { artboard, items: listed };
    },
    lockup: (doc, request) => {
        const [selected] = items<PageItem>(doc.selection);
        if (selected === undefined) {
            return { reason: 'nothingSelected' };
        }
        app.coordinateSystem = CoordinateSystem.ARTBOARDCOORDINATESYSTEM;
        const grid = owned(doc.layers, request.layerName);
        grid.locked = false;
        const stroke = color(doc, request.color);
        visit(request.lines, ({ from, to }): void => {
            line(grid, from, to, request.weight, stroke, request.filled);
        });
        visit(request.labels, (label): void => {
            const frame = grid.textFrames.add();
            frame.contents = label.text;
            const attributes = frame.textRange.characterAttributes;
            attributes.size = label.size;
            attributes.fillColor = stroke;
            frame.position = label.vertical ? [label.left + (label.width - frame.width) / 2, label.top] : [label.left, label.top - frame.height / 2];
        });
        visit(request.copies, (copy): void => {
            const duplicate = selected.duplicate();
            duplicate.rotate(copy.rotate);
            duplicate.position = copy.position;
            duplicate.move(grid, ElementPlacement.PLACEATEND);
        });
        grid.locked = true;
        return { drawn: request.lines.length };
    },
    object: (doc, request) => {
        const target = owned(doc.layers, request.layerName);
        visit(request.clearLayer ? items(target.pageItems) : [], (item): void => item.remove());
        const stroke = color(doc, request.color);
        const { scaleFactor: scale } = doc;
        const { margins, edges } = request;
        const artboard = nth(doc.artboards, doc.artboards.getActiveArtboardIndex()).artboardRect;
        const board = rectangleOf(artboard);
        const frame = request.extendTo === 'artboard' ? board : rectangleOf(doc.visibleBounds);
        const drawn = fold(items<PageItem>(doc.selection), 0, (total, item): number => {
            const bounds = request.bounds === 'geometric' ? item.geometricBounds : item.visibleBounds;
            const box = rectangleOf([bounds[0] - margins.left / scale, bounds[1] + margins.top / scale, bounds[2] + margins.right / scale, bounds[3] - margins.bottom / scale]);
            const width = request.extendTo === 'artboard' ? board.right - board.left : box.right - box.left;
            const height = request.extendTo === 'artboard' ? board.top - board.bottom : box.top - box.bottom;
            const centerX = (box.left + box.right) / 2;
            const centerY = (box.top + box.bottom) / 2;
            const diagonal = (width * width + height * height) ** (1 / 2);
            const rows: [boolean, Segment][] = [
                [edges.top, { from: [box.left, box.top], to: [box.right, box.top], dimension: width }],
                [edges.bottom, { from: [box.left, box.bottom], to: [box.right, box.bottom], dimension: width }],
                [edges.centerY, { from: [box.left, centerY], to: [box.right, centerY], dimension: width }],
                [edges.left, { from: [box.left, box.top], to: [box.left, box.bottom], dimension: height }],
                [edges.right, { from: [box.right, box.top], to: [box.right, box.bottom], dimension: height }],
                [edges.centerX, { from: [centerX, box.top], to: [centerX, box.bottom], dimension: height }],
                [edges.leftDiagonal, { from: [box.left, box.top], to: [box.right, box.bottom], dimension: diagonal }],
                [edges.rightDiagonal, { from: [box.right, box.top], to: [box.left, box.bottom], dimension: diagonal }],
            ];
            const group = target.groupItems.add();
            group.name = `${item.name}_guides`;
            const drawnPaths = collect(
                flatMap(
                    select(rows, ([on]): boolean => on),
                    ([, segment]): [[number, number], [number, number]][] => ends(request, frame, scale, segment),
                ),
                ([from, to]): PathItem => line(group, from, to, request.weight, stroke, false),
            );
            visit(drawnPaths, (path): void => {
                const guide = path;
                guide.guides = request.drawAs === 'guides';
            });
            if (drawnPaths.length === 0) {
                group.remove();
            }
            return total + drawnPaths.length;
        });
        target.zOrder(ZOrderMethod.BRINGTOFRONT);
        return { drawn };
    },
    bento: (doc, request) => {
        const canvas = doc;
        visit(request.removeSelection ? items<PageItem>(canvas.selection) : [], (item): void => item.remove());
        const container = canvas.activeLayer;
        const group = container.groupItems.add();
        group.name = request.groupName;
        const drawn = collect(request.cells, (cell): PathItem => container.pathItems.roundedRectangle(cell.y, cell.x, cell.w, cell.h, request.cornerRadius, request.cornerRadius));
        visit(drawn.slice(0).reverse(), (rect): void => {
            rect.move(group, ElementPlacement.PLACEATEND);
        });
        canvas.selection = null;
        return { drawn: drawn.length };
    },
    isometric: (doc, request) => {
        const ruled = doc;
        const { rulerOrigin, defaultStrokeColor: stroke } = ruled;
        ruled.rulerOrigin = [0, ruled.height];
        app.coordinateSystem = CoordinateSystem.ARTBOARDCOORDINATESYSTEM;
        try {
            const parent = owned(ruled.layers, request.layerName);
            const indices = request.artboards === 'all' ? range(ruled.artboards.length) : select(request.artboards, (index): boolean => index >= 0 && index < ruled.artboards.length);
            const drawn = fold(indices, 0, (total, index): number => {
                const artboard = nth(ruled.artboards, index);
                const [left, top, right, bottom] = artboard.artboardRect;
                ruled.rulerOrigin = [left, top];
                const sub = parent.layers.add();
                sub.name = request.sublayerTemplate.split('%a').join(artboard.name);
                const width = right - left;
                const height = top - bottom;
                const x2 = left + width / 2;
                const seeds: [number, number][] = [
                    [0, 0],
                    [0, -request.angle],
                    [0, request.angle],
                ];
                const rows = seeds.concat(
                    flatMap(range(Math.floor((right - x2) / request.spacing)), (ring): [number, number][] => {
                        const step = request.spacing * (ring + 1);
                        return [
                            [step, 0],
                            [-step, 0],
                            [step / 2, 0],
                            [-step / 2, 0],
                            [step, -request.angle],
                            [-step, -request.angle],
                            [step, request.angle],
                            [-step, request.angle],
                        ];
                    }),
                );
                const lines = collect(rows, ([dx, turn]): PathItem => {
                    const item = line(sub, [x2 + dx, top - height], [x2 + dx, bottom + 2 * height], 1, stroke, false);
                    if (turn !== 0) {
                        item.rotate(turn, true, true, true, true, Transformation.CENTER);
                    }
                    return item;
                });
                const group = sub.groupItems.add();
                const clip = sub.pathItems.rectangle(top, left, width, height);
                clip.move(group, ElementPlacement.PLACEATBEGINNING);
                visit(lines, (item): void => {
                    item.move(group, ElementPlacement.PLACEATEND);
                });
                group.clipped = true;
                return total + lines.length;
            });
            return { drawn };
        } finally {
            ruled.rulerOrigin = rulerOrigin;
        }
    },
};

// --- [ENTRY] ---------------------------------------------------------------------------

const guides = <const K extends keyof Requests>(request: { readonly operation: K } & Requests[K]): Reading<JsonObject> => {
    const doc = app.activeDocument;
    const system = app.coordinateSystem;
    try {
        const row = OPERATIONS[request.operation](doc, request);
        row['operation'] = request.operation;
        return present(split([row]));
    } finally {
        app.coordinateSystem = system;
    }
};

run<{ readonly [K in keyof Requests]: { readonly operation: K } & Requests[K] }[keyof Requests]>(guides);
