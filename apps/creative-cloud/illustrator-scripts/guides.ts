/// <reference path="./prelude.ts"/>

// --- [HOST] ----------------------------------------------------------------------------

declare const $: $;
declare const app: Application;

declare global {
    enum CoordinateSystem {}
    enum DocumentColorSpace {}
    enum ElementPlacement {}
    enum TextType {}
    enum Transformation {}
    enum ZOrderMethod {}
}

// --- [PRELUDE] -------------------------------------------------------------------------

const { collect, color, flatten, fold, items, layer, nth, pairs, range, run, select, typed, visit }: Prelude = $.evalFile(new File(`${new File($.fileName).path}/prelude.jsx`));

// --- [REQUEST] -------------------------------------------------------------------------

interface Commands {
    readonly makeGuide: string;
    readonly releaseGuide: string;
}

interface Range {
    readonly min: number;
    readonly max: number;
}

interface Lockup {
    readonly operation: 'lockup';
    readonly mode: 'horizontal' | 'vertical' | 'condensed';
}

interface ObjectGuides {
    readonly operation: 'object';
    readonly layerName: string;
    readonly clearLayer: boolean;
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
    readonly extension: { readonly mode: 'percentage' | 'absolute'; readonly value: number };
    readonly bounds: 'geometric' | 'visible';
    readonly drawAs: 'guides' | 'strokedPaths';
}

interface BentoGrid {
    readonly operation: 'bento';
    readonly mode: 'grid';
    readonly columns: Range;
    readonly rows: Range;
    readonly columnGutter: number;
    readonly rowGutter: number;
    readonly splitColumns?: Range;
    readonly splitRows?: Range;
    readonly cornerRadius: number;
    readonly seed: number;
}

interface BentoTotal {
    readonly operation: 'bento';
    readonly mode: 'total';
    readonly maxCount: number;
    readonly gutter: number;
    readonly minSize: number;
    readonly hero: boolean;
    readonly centerHero: boolean;
    readonly cornerRadius: number;
    readonly seed: number;
}

interface Isometric {
    readonly operation: 'isometric';
    readonly spacing: number;
    readonly angle: number;
    readonly artboards: number[] | 'all';
}

type Request = { readonly operation: 'from_selection' } | { readonly operation: 'release' } | Lockup | ObjectGuides | BentoGrid | BentoTotal | Isometric;

interface Box {
    readonly left: number;
    readonly top: number;
    readonly width: number;
    readonly height: number;
}

interface Frame {
    readonly left: number;
    readonly top: number;
    readonly right: number;
    readonly bottom: number;
}

// --- [DRAWING] -------------------------------------------------------------------------

const LOCKUP_RED = 232;
const LOCKUP_GREEN = 74;
const LOCKUP_BLUE = 255;
const LOCKUP_COLOR: ColorSpec = { model: 'RGB', values: [LOCKUP_RED, LOCKUP_GREEN, LOCKUP_BLUE] };
const LOCKUP_LAYER = 'Lockup Grid';
const LOCKUP_MARGIN = 30;
const LABEL_OFFSET = 15;
const THIRDS = 3;
const QUARTERS = 4;
const RIGHT_ANGLE = 90;
const HALF = 0.5;
const QUARTER = 0.25;
const OBJECT_GUIDE_WEIGHT = 0.25;
const PERCENT = 100;
const CHANNEL = 255;
const HSV_LOW_SATURATION = 20;
const HSV_HIGH_VALUE = 80;
const HSV_HALF_VALUE = 50;
const HSV_SATURATED = 10;
const HUE_SECTORS = 6;
const HSV_RED = 5;
const HSV_GREEN = 3;
const HSV_BLUE = 1;
const SPLIT_CHANCE = 0.5;
const FLIP_CHANCE = 0.3;
const HERO_SCALE_BASE = 0.45;
const HERO_SCALE_SPAN = 0.2;
const PACK_ITERATIONS = 1000;
const PRNG_MULTIPLIER = 16_807;
const PRNG_MODULUS = 2_147_483_647;

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

const label = (container: Layer, text: string, size: number, left: number, top: number, vertical: boolean, width: number, fill: Color): TextFrame => {
    const frame = container.textFrames.add();
    frame.contents = text;
    frame.textRange.characterAttributes.size = size;
    frame.textRange.characterAttributes.fillColor = fill;
    frame.position = vertical ? [left + width * HALF - frame.width * HALF, top] : [left, top - frame.height * HALF];
    return frame;
};

const selectionBox = (first: PageItem): Box => {
    const item = typed<TextFrame>('TextFrame')(first) && first.kind !== TextType.POINTTEXT ? first.convertAreaObjectToPointObject() : first;
    return { left: item.left, top: item.top, width: item.width, height: item.height };
};

const counted = (operation: Request['operation'], count: number): JsonObject => ({ kind: 'guidesDrawn', operation, drawn: count });

// --- [LOCKUP] --------------------------------------------------------------------------

const horizontalLockup = (grid: Layer, box: Box, stroke: Color): number => {
    const { left, top, width, height } = box;
    const bottom = top - height;
    const horizontals = [top, bottom, top - height * HALF, top - height * QUARTER, top - height * THIRDS * QUARTER, top + height * QUARTER, bottom - height * QUARTER];
    const verticals = [left - height * QUARTER, left + width + height * QUARTER, left, left + width];
    visit(horizontals, (y): void => {
        line(grid, [left - width * HALF, y], [left + width + width * HALF, y], 1, stroke, false);
    });
    visit(verticals, (x): void => {
        line(grid, [x, top + height * HALF], [x, bottom - height * HALF], 1, stroke, false);
    });
    return horizontals.length + verticals.length;
};

const verticalLockup = (grid: Layer, selected: PageItem, box: Box, stroke: Color): number => {
    const { left, top, width, height } = box;
    const bottom = top - height;
    const third = height / THIRDS;
    const ninth = third / THIRDS;
    const wthird = width / THIRDS;
    const offset = left - width - LOCKUP_MARGIN;
    visit([left - width, left + width], (x): void => {
        const copy = selected.duplicate();
        copy.position = [x, top];
        copy.move(grid, ElementPlacement.PLACEATEND);
    });
    const upper = [top, top - third, top - 2 * third];
    const lower = [bottom, bottom - third, bottom - 2 * third, bottom - height];
    const ninths = select(range(THIRDS * THIRDS - 1), (band): boolean => (band + 1) % THIRDS !== 0);
    const verticals = [left, left + wthird, left + width, left + width - wthird];
    visit(upper, (y): void => {
        line(grid, [left - width, y], [left + 2 * width, y], 2, stroke, true);
    });
    visit(lower, (y): void => {
        line(grid, [offset, y], [left + 2 * width + LOCKUP_MARGIN, y], 2, stroke, true);
    });
    visit(ninths, (band): void => {
        line(grid, [left - width, bottom - (band + 1) * ninth], [left + 2 * width, bottom - (band + 1) * ninth], 2, stroke, true);
    });
    visit(verticals, (x): void => {
        line(grid, [x, top], [x, bottom - height], 2, stroke, true);
    });
    visit(range(THIRDS), (band): void => {
        label(grid, String(band + 1), height * HALF, offset - LABEL_OFFSET, top - band * third, false, 0, stroke);
        label(grid, String(band + 1), height * HALF, offset - LABEL_OFFSET, bottom - band * third, false, 0, stroke);
    });
    visit(range(THIRDS * THIRDS), (band): void => {
        label(grid, String((band % THIRDS) + 1), height / THIRDS, left - width, bottom - band * ninth, false, 0, stroke);
    });
    return upper.length + lower.length + ninths.length + verticals.length;
};

const condensedLockup = (grid: Layer, selected: PageItem, box: Box, stroke: Color): number => {
    const { left, top, width, height } = box;
    const bottom = top - height;
    const third = height / THIRDS;
    const wthird = width / THIRDS;
    const quarter = width / QUARTERS;
    const horizontals = [top, top - third, top - 2 * third, bottom, bottom - third, bottom - 2 * third, bottom - height, top + third, top + 2 * third];
    visit(horizontals, (y): void => {
        line(grid, [left - wthird, y], [left + width + wthird, y], 2, stroke, true);
    });
    const verticals = [left, left + quarter, left + 2 * quarter, left + THIRDS * quarter, left + width];
    visit(verticals, (x): void => {
        line(grid, [x, bottom - height + width + 2 * wthird], [x, bottom - height], 2, stroke, true);
    });
    visit([left - height, left + width], (x): void => {
        const copy = selected.duplicate();
        copy.rotate(RIGHT_ANGLE);
        copy.position = [x, bottom - height + copy.height];
        copy.move(grid, ElementPlacement.PLACEATEND);
    });
    visit(range(THIRDS), (band): void => {
        label(grid, String(band + 1), height * HALF, left - wthird - LABEL_OFFSET, top - band * third, false, 0, stroke);
    });
    visit(range(QUARTERS), (column): void => {
        label(grid, String(column + 1), height / THIRDS, left + column * quarter, top, true, quarter, stroke);
    });
    return horizontals.length + verticals.length;
};

const lockup = (request: Lockup): JsonObject => {
    const doc = app.activeDocument;
    const [selected] = items<PageItem>(doc.selection);
    if (selected === undefined) {
        return { kind: 'guidesRejected', operation: request.operation, reason: 'nothingSelected' };
    }
    const box = selectionBox(selected);
    app.coordinateSystem = CoordinateSystem.ARTBOARDCOORDINATESYSTEM;
    const grid = layer(doc, LOCKUP_LAYER);
    grid.locked = false;
    const stroke = color(LOCKUP_COLOR);
    const count = fold(
        select(
            [
                { mode: 'horizontal', draw: (): number => horizontalLockup(grid, box, stroke) },
                { mode: 'vertical', draw: (): number => verticalLockup(grid, selected, box, stroke) },
                { mode: 'condensed', draw: (): number => condensedLockup(grid, selected, box, stroke) },
            ],
            (row): boolean => row.mode === request.mode,
        ),
        0,
        (total, row): number => total + row.draw(),
    );
    grid.locked = true;
    return counted(request.operation, count);
};

// --- [OBJECT] --------------------------------------------------------------------------

const hueOf = (r: number, g: number, b: number, max: number, delta: number): number => {
    if (delta === 0) {
        return 0;
    }
    if (max === r) {
        const sector = ((g - b) / delta) % HUE_SECTORS;
        return sector < 0 ? sector + HUE_SECTORS : sector;
    }
    return max === g ? (b - r) / delta + 2 : (r - g) / delta + 2 * 2;
};

const guideColor = (doc: Document): Color => {
    const unit = (name: string): number => app.preferences.getRealPreference(`Guide/Color/${name}`);
    const [r, g, b] = [unit('red'), unit('green'), unit('blue')];
    if (doc.documentColorSpace === DocumentColorSpace.RGB) {
        return color({ model: 'RGB', values: [r * CHANNEL, g * CHANNEL, b * CHANNEL] });
    }
    const max = Math.max(r, g, b);
    const delta = max - Math.min(r, g, b);
    const saturation = max === 0 ? 0 : (delta / max) * PERCENT;
    const value = max * PERCENT;
    const s = (saturation > HSV_SATURATED ? PERCENT : saturation) / PERCENT;
    const v = (saturation < HSV_LOW_SATURATION && value > HSV_HIGH_VALUE ? HSV_HALF_VALUE : value) / PERCENT;
    const hue = hueOf(r, g, b, max, delta);
    const channel = (n: number): number => {
        const k = (n + hue) % HUE_SECTORS;
        return v - v * s * Math.max(0, Math.min(k, QUARTERS - k, 1));
    };
    const [r1, g1, b1] = [channel(HSV_RED), channel(HSV_GREEN), channel(HSV_BLUE)];
    const k = 1 - Math.max(r1, g1, b1);
    const cmyk = collect([r1, g1, b1], (part): number => (k === 1 ? 0 : ((1 - part - k) / (1 - k)) * PERCENT));
    return color({ model: 'CMYK', values: cmyk.concat([k * PERCENT]) });
};

const rectangleOf = (bounds: Rect): Frame => ({ left: bounds[0], top: bounds[1], right: bounds[2], bottom: bounds[3] });

const extended = (from: [number, number], to: [number, number], d: number): [[number, number], [number, number]] => {
    const dx = to[0] - from[0];
    const dy = to[1] - from[1];
    const length = (dx * dx + dy * dy) ** HALF;
    const ux = dx / length;
    const uy = dy / length;
    return [
        [from[0] - ux * d, from[1] - uy * d],
        [to[0] + ux * d, to[1] + uy * d],
    ];
};

const within = ([x, y]: [number, number], rect: Frame): boolean => x >= rect.left && x <= rect.right && y >= rect.bottom && y <= rect.top;

const clipped = (from: [number, number], to: [number, number], rect: Frame): [number, number][] => {
    const k = (to[1] - from[1]) / (to[0] - from[0]);
    const b = from[1] - k * from[0];
    const candidates: [number, number][] = [
        [rect.left, k * rect.left + b],
        [rect.right, k * rect.right + b],
        [(rect.top - b) / k, rect.top],
        [(rect.bottom - b) / k, rect.bottom],
    ];
    return select(candidates, (point): boolean => within(point, rect));
};

interface Segment {
    readonly from: [number, number];
    readonly to: [number, number];
    readonly dimension: number;
}

const segments = (request: ObjectGuides, bounds: Rect, artboard: Rect): Segment[] => {
    const box = rectangleOf(bounds);
    const board = rectangleOf(artboard);
    const width = request.extendTo === 'artboard' ? board.right - board.left : box.right - box.left;
    const height = request.extendTo === 'artboard' ? board.top - board.bottom : box.top - box.bottom;
    const centerX = (box.left + box.right) * HALF;
    const centerY = (box.top + box.bottom) * HALF;
    const diagonal = (width * width + height * height) ** HALF;
    const { edges } = request;
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
    return collect(
        select(rows, ([on]): boolean => on),
        ([, segment]): Segment => segment,
    );
};

const ends = (request: ObjectGuides, frame: Frame, scale: number, segment: Segment): [[number, number], [number, number]][] => {
    const d = request.extension.mode === 'percentage' ? (segment.dimension * request.extension.value) / PERCENT : request.extension.value / scale;
    const [from, to] = extended(segment.from, segment.to, d);
    if (request.extendTo === 'object') {
        return [[from, to]];
    }
    if (within(from, frame) && within(to, frame)) {
        return [];
    }
    const diagonal = from[0] !== to[0] && from[1] !== to[1];
    const points = diagonal ? clipped(from, to, frame) : [from, to];
    return points.length === 2 ? pairs(points) : [];
};

const objectGuides = (request: ObjectGuides): JsonObject => {
    const doc = app.activeDocument;
    const target = layer(doc, request.layerName);
    if (request.clearLayer) {
        visit(items(target.pageItems), (item): void => item.remove());
    }
    const stroke = guideColor(doc);
    const { scaleFactor: scale } = doc;
    const { margins } = request;
    const artboard = nth(doc.artboards, doc.artboards.getActiveArtboardIndex()).artboardRect;
    const frame = request.extendTo === 'artboard' ? rectangleOf(artboard) : rectangleOf(doc.visibleBounds);
    const drawn = fold(items<PageItem>(doc.selection), 0, (total, item): number => {
        const bounds = request.bounds === 'geometric' ? item.geometricBounds : item.visibleBounds;
        const expanded: Rect = [bounds[0] - margins.left / scale, bounds[1] + margins.top / scale, bounds[2] + margins.right / scale, bounds[3] - margins.bottom / scale];
        const group = target.groupItems.add();
        group.name = `${item.name}_guides`;
        const paths = collect(
            fold<Segment, [[number, number], [number, number]][]>(segments(request, expanded, artboard), [], (list, segment) => list.concat(ends(request, frame, scale, segment))),
            ([from, to]): PathItem => line(group, from, to, OBJECT_GUIDE_WEIGHT, stroke, false),
        );
        visit(paths, (path): void => {
            const guide = path;
            guide.guides = request.drawAs === 'guides';
        });
        if (paths.length === 0) {
            group.remove();
        }
        return total + paths.length;
    });
    target.zOrder(ZOrderMethod.BRINGTOFRONT);
    return counted(request.operation, drawn);
};

// --- [BENTO] ---------------------------------------------------------------------------

interface Cell {
    readonly x: number;
    readonly y: number;
    readonly w: number;
    readonly h: number;
}

type Random = () => number;

const uniform = (seed: number): Random => {
    let state = (Math.abs(Math.floor(seed)) % (PRNG_MODULUS - 1)) + 1;
    return (): number => {
        state = (state * PRNG_MULTIPLIER) % PRNG_MODULUS;
        return state / PRNG_MODULUS;
    };
};

const randomInt = (span: Range, random: Random): number => span.min + Math.floor(random() * (span.max - span.min + 1));

const randomRatio = (span: Range, random: Random): number => span.min / PERCENT + (random() * (span.max - span.min)) / PERCENT;

const gridBox = (doc: Document): Cell => {
    const selected = items<PageItem>(doc.selection);
    if (selected.length === 0) {
        const rect = nth(doc.artboards, doc.artboards.getActiveArtboardIndex()).artboardRect;
        return { x: rect[0], y: rect[1], w: rect[2] - rect[0], h: rect[1] - rect[3] };
    }
    const union = fold<PageItem, Rect>(selected, [Number.POSITIVE_INFINITY, Number.NEGATIVE_INFINITY, Number.NEGATIVE_INFINITY, Number.POSITIVE_INFINITY], (bounds, item): Rect => {
        const [left, top, right, bottom] = item.geometricBounds;
        return [Math.min(bounds[0], left), Math.max(bounds[1], top), Math.max(bounds[2], right), Math.min(bounds[3], bottom)];
    });
    visit(selected, (item): void => item.remove());
    return { x: union[0], y: union[1], w: union[2] - union[0], h: union[1] - union[3] };
};

const cells = (doc: Document, list: Cell[], radius: number, name: string): number => {
    const container = doc.activeLayer;
    const group = container.groupItems.add();
    group.name = name;
    const drawn = collect(list, (cell): PathItem => container.pathItems.roundedRectangle(cell.y, cell.x, cell.w, cell.h, radius, radius));
    visit(drawn.slice(0).reverse(), (rect): void => {
        rect.move(group, ElementPlacement.PLACEATEND);
    });
    return drawn.length;
};

const splitCell = (cell: Cell, request: BentoGrid, previous: 'single' | 'row' | 'column', random: Random): { readonly kind: 'single' | 'row' | 'column'; readonly cells: Cell[] } => {
    if (request.splitRows !== undefined && random() > SPLIT_CHANCE && previous !== 'row') {
        const left = (cell.w - request.columnGutter) * randomRatio(request.splitRows, random);
        const right = cell.w - request.columnGutter - left;
        return {
            kind: 'row',
            cells: [
                { x: cell.x, y: cell.y, w: left, h: cell.h },
                { x: cell.x + left + request.columnGutter, y: cell.y, w: right, h: cell.h },
            ],
        };
    }
    if (request.splitColumns !== undefined && random() > SPLIT_CHANCE && previous !== 'column') {
        const top = (cell.h - request.rowGutter) * randomRatio(request.splitColumns, random);
        const bottom = cell.h - request.rowGutter - top;
        return {
            kind: 'column',
            cells: [
                { x: cell.x, y: cell.y, w: cell.w, h: top },
                { x: cell.x, y: cell.y - top - request.rowGutter, w: cell.w, h: bottom },
            ],
        };
    }
    return { kind: 'single', cells: [cell] };
};

interface Column {
    readonly previous: 'single' | 'row' | 'column';
    readonly listed: Cell[];
}

const bentoColumn = (request: BentoGrid, grid: Cell, x: number, columnWidth: number, random: Random): Cell[] => {
    const rowCount = randomInt(request.rows, random);
    const rowHeight = (grid.h - (rowCount - 1) * request.rowGutter) / rowCount;
    return fold<number, Column>(range(rowCount), { previous: 'single', listed: [] }, ({ previous, listed }, row): Column => {
        const split = splitCell({ x, y: grid.y - row * (rowHeight + request.rowGutter), w: columnWidth, h: rowHeight }, request, previous, random);
        return { previous: split.kind, listed: listed.concat(split.cells) };
    }).listed;
};

const bentoGrid = (doc: Document, request: BentoGrid, grid: Cell, random: Random): number => {
    const columnCount = randomInt(request.columns, random);
    const columnWidth = (grid.w - (columnCount - 1) * request.columnGutter) / columnCount;
    const listed = fold<number, Cell[]>(range(columnCount), [], (all, column): Cell[] =>
        all.concat(bentoColumn(request, grid, grid.x + column * (columnWidth + request.columnGutter), columnWidth, random)),
    );
    return cells(doc, listed, request.cornerRadius, 'BENTO');
};

const packed = (leaves: Cell[], request: BentoTotal, maxCount: number, random: Random): Cell[] => {
    let list = leaves;
    for (let iteration = 0; iteration < PACK_ITERATIONS && list.length < maxCount; iteration += 1) {
        const largest = fold(list, { x: 0, y: 0, w: 0, h: 0 }, (best, cell): Cell => (cell.w * cell.h > best.w * best.h ? cell : best));
        const canH = largest.w >= 2 * request.minSize + request.gutter;
        const canV = largest.h >= 2 * request.minSize + request.gutter;
        if (!(canH || canV)) {
            return list;
        }
        const flipped = canH && canV && random() < FLIP_CHANCE;
        const splitHoriz = (canH && canV ? largest.w > largest.h : canH) !== flipped;
        const avail = (splitHoriz ? largest.w : largest.h) - request.gutter;
        const size = Math.min(Math.max(request.minSize + random() * (avail - 2 * request.minSize), request.minSize), avail - request.minSize);
        const children: Cell[] = splitHoriz
            ? [
                  { x: largest.x, y: largest.y, w: size, h: largest.h },
                  { x: largest.x + size + request.gutter, y: largest.y, w: avail - size, h: largest.h },
              ]
            : [
                  { x: largest.x, y: largest.y, w: largest.w, h: size },
                  { x: largest.x, y: largest.y - size - request.gutter, w: largest.w, h: avail - size },
              ];
        list = select(list, (cell): boolean => cell !== largest).concat(children);
    }
    return list;
};

const zones = (grid: Cell, hero: Cell, request: BentoTotal, random: Random): Cell[] => {
    const { gutter: g } = request;
    const reach = (): boolean => random() < SPLIT_CHANCE;
    const leftStrip = hero.x - grid.x - g;
    const rightStrip = grid.x + grid.w - (hero.x + hero.w) - g;
    const topStrip = grid.y - hero.y - g;
    const bottomStrip = hero.y - hero.h - (grid.y - grid.h) - g;
    const spanLeft = leftStrip >= request.minSize && reach();
    const spanRight = rightStrip >= request.minSize && reach();
    const x = spanLeft ? grid.x : hero.x;
    const w = (spanRight ? grid.x + grid.w : hero.x + hero.w) - x;
    const rows: [boolean, Cell][] = [
        [leftStrip >= request.minSize, { x: grid.x, y: grid.y, w: leftStrip, h: grid.h }],
        [rightStrip >= request.minSize, { x: hero.x + hero.w + g, y: grid.y, w: rightStrip, h: grid.h }],
        [topStrip >= request.minSize, { x, y: grid.y, w, h: topStrip }],
        [bottomStrip >= request.minSize, { x, y: hero.y - hero.h - g, w, h: bottomStrip }],
    ];
    return collect(
        select(rows, ([on]): boolean => on),
        ([, cell]): Cell => cell,
    );
};

const snapped = (offset: number, room: number, edge: number): number => {
    if (room - offset < edge) {
        return room;
    }
    return offset < edge ? 0 : offset;
};

const bentoTotal = (doc: Document, request: BentoTotal, grid: Cell, random: Random): number => {
    if (!request.hero) {
        return cells(doc, packed([grid], request, request.maxCount, random), request.cornerRadius, 'BENTO_TOTAL');
    }
    const scale = HERO_SCALE_BASE + random() * HERO_SCALE_SPAN;
    const heroW = Math.max(grid.w * scale, request.minSize);
    const heroH = Math.max(grid.h * scale, request.minSize);
    const edge = request.minSize + request.gutter;
    const dx = request.centerHero ? (grid.w - heroW) * HALF : snapped(random() * (grid.w - heroW), grid.w - heroW, edge);
    const dy = request.centerHero ? (grid.h - heroH) * HALF : snapped(random() * (grid.h - heroH), grid.h - heroH, edge);
    const hero: Cell = { x: grid.x + dx, y: grid.y - dy, w: heroW, h: heroH };
    const surrounding = zones(grid, hero, request, random);
    const extra = Math.max(0, request.maxCount - 1 - surrounding.length);
    const shares = fold(
        range(surrounding.length === 0 ? 0 : extra),
        collect(surrounding, (): number => 1),
        (counts): number[] => {
            const index = Math.floor(random() * surrounding.length);
            return collect(counts, (count, position): number => (position === index ? count + 1 : count));
        },
    );
    const listed = fold(surrounding, [hero], (all, zone, index): Cell[] => all.concat(packed([zone], request, nth(shares, index), random)));
    return cells(doc, listed, request.cornerRadius, request.centerHero ? 'BENTO_HERO_CENTER' : 'BENTO_HERO');
};

const bento = (request: BentoGrid | BentoTotal): JsonObject => {
    const doc = app.activeDocument;
    const grid = gridBox(doc);
    const random = uniform(request.seed);
    const count = request.mode === 'grid' ? bentoGrid(doc, request, grid, random) : bentoTotal(doc, request, grid, random);
    doc.selection = null;
    return counted(request.operation, count);
};

// --- [ISOMETRIC] -----------------------------------------------------------------------

const ISO_LAYER = 'ISOGrid';

const isometricArtboard = (parent: Layer, artboard: Artboard, request: Isometric, stroke: Color): number => {
    const [left, top, right, bottom] = artboard.artboardRect;
    const sub = parent.layers.add();
    sub.name = `${artboard.name} iso grid`;
    const width = right - left;
    const height = top - bottom;
    const x2 = left + width * HALF;
    const vertical = (from: number, to: number): PathItem => line(sub, [x2, from], [x2, to], 1, stroke, false);
    const m = vertical(top - height, bottom + height);
    const l = vertical(top - height, bottom + 2 * height);
    const k = vertical(top - height, bottom + 2 * height);
    const lines: PageItem[] = [l, k, m];
    for (let ring = 1; x2 + request.spacing * ring <= right; ring += 1) {
        const step = request.spacing * ring;
        visit([step, -step, step * HALF, -step * HALF], (dx): void => {
            const copy = m.duplicate();
            copy.translate(dx, 0);
            lines.push(copy);
        });
        const rising = k.duplicate();
        rising.translate(step, 0);
        rising.rotate(-request.angle, true, true, true, true, Transformation.CENTER);
        if (rising.top + rising.height * HALF <= top) {
            rising.remove();
            break;
        }
        lines.push(rising);
        const falling = k.duplicate();
        falling.translate(-step, 0);
        falling.rotate(-request.angle, true, true, true, true, Transformation.CENTER);
        lines.push(falling);
        visit([step, -step], (dx): void => {
            const copy = l.duplicate();
            copy.translate(dx, 0);
            copy.rotate(request.angle, true, true, true, true, Transformation.CENTER);
            lines.push(copy);
        });
    }
    l.rotate(request.angle, true, true, true, true, Transformation.CENTER);
    k.rotate(-request.angle, true, true, true, true, Transformation.CENTER);
    const group = sub.groupItems.add();
    const clip = sub.pathItems.rectangle(top, left, width, height);
    clip.move(group, ElementPlacement.PLACEATBEGINNING);
    visit(lines, (item): void => {
        item.move(group, ElementPlacement.PLACEATEND);
    });
    group.clipped = true;
    return lines.length;
};

const isometric = (request: Isometric): JsonObject => {
    const doc = app.activeDocument;
    const { rulerOrigin } = doc;
    doc.rulerOrigin = [0, doc.height];
    app.coordinateSystem = CoordinateSystem.ARTBOARDCOORDINATESYSTEM;
    try {
        const parent = layer(doc, ISO_LAYER);
        const indices = request.artboards === 'all' ? range(doc.artboards.length) : select(request.artboards, (index): boolean => index >= 0 && index < doc.artboards.length);
        const count = fold(indices, 0, (total, index): number => {
            const artboard = nth(doc.artboards, index);
            const [left, top] = artboard.artboardRect;
            doc.rulerOrigin = [left, top];
            return total + isometricArtboard(parent, artboard, request, doc.defaultStrokeColor);
        });
        return counted(request.operation, count);
    } finally {
        doc.rulerOrigin = rulerOrigin;
    }
};

// --- [ENTRY] ---------------------------------------------------------------------------

const menuGuides = (operation: 'from_selection' | 'release', commands: Commands): JsonObject => {
    const doc = app.activeDocument;
    const count = flatten(items<PageItem>(doc.selection)).length;
    app.executeMenuCommand(operation === 'release' ? commands.releaseGuide : commands.makeGuide);
    return counted(operation, count);
};

const drawn = (request: Request, commands: Commands): JsonObject => {
    if (request.operation === 'from_selection' || request.operation === 'release') {
        return menuGuides(request.operation, commands);
    }
    if (request.operation === 'lockup') {
        return lockup(request);
    }
    if (request.operation === 'object') {
        return objectGuides(request);
    }
    return request.operation === 'isometric' ? isometric(request) : bento(request);
};

const guides = (request: { readonly request: Request; readonly commands: Commands }, _at: Site): Reading<JsonObject> => {
    const system = app.coordinateSystem;
    try {
        return { value: drawn(request.request, request.commands), unavailable: [] };
    } finally {
        app.coordinateSystem = system;
    }
};

run(guides);
