/// <reference path="./prelude.ts"/>

// --- [HOST] ----------------------------------------------------------------------------

declare const $: $;
declare const app: Application;

declare global {
    enum CoordinateSystem {}
    enum ElementPlacement {}
    enum Transformation {}
    enum ZOrderMethod {}
}

// --- [PRELUDE] -------------------------------------------------------------------------

const { collect, color, flatten, fold, items, layer, run, select, visit }: Prelude = $.evalFile(new File(`${new File($.fileName).path}/prelude.jsx`));

// --- [REQUEST] -------------------------------------------------------------------------

interface Commands {
    readonly makeGuide: string;
    readonly releaseGuide: string;
    readonly noCompoundPath: string;
    readonly group: string;
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
    readonly edges: { readonly left: boolean; readonly top: boolean; readonly right: boolean; readonly bottom: boolean; readonly centerX: boolean; readonly centerY: boolean; readonly leftDiagonal: boolean; readonly rightDiagonal: boolean };
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
}

interface Isometric {
    readonly operation: 'isometric';
    readonly spacing: number;
    readonly angle: number;
    readonly artboards?: number[];
}

type Request = { readonly operation: 'from_selection' } | { readonly operation: 'release' } | Lockup | ObjectGuides | BentoGrid | BentoTotal | Isometric;

interface Box {
    readonly left: number;
    readonly top: number;
    readonly width: number;
    readonly height: number;
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
const SECTOR_GREEN = 2;
const SECTOR_BLUE = 4;
const SPLIT_CHANCE = 0.5;
const FLIP_CHANCE = 0.3;
const HERO_SCALE_BASE = 0.45;
const HERO_SCALE_SPAN = 0.2;
const PACK_ITERATIONS = 1000;

const line = (container: { readonly pathItems: PathItems }, from: [number, number], to: [number, number], weight: number, stroke: Color, fill: Color | undefined): PathItem => {
    const path = container.pathItems.add();
    path.setEntirePath([from, to]);
    path.stroked = true;
    path.strokeWidth = weight;
    path.strokeColor = stroke;
    path.filled = fill !== undefined;
    if (fill !== undefined) {
        path.fillColor = fill;
    }
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

const selectionBox = (doc: Document): Box => {
    const [first] = items<PageItem>(doc.selection);
    if (first === undefined) {
        throw new Error('Nothing is selected');
    }
    const item = first.typename === 'TextFrame' && (first as TextFrame).kind !== $.global.TextType.POINTTEXT ? (first as TextFrame).convertAreaObjectToPointObject() : first;
    return { left: item.left, top: item.top, width: item.width, height: item.height };
};

// --- [LOCKUP] --------------------------------------------------------------------------

const horizontalLockup = (grid: Layer, box: Box, stroke: Color): number => {
    const { left, top, width, height } = box;
    const bottom = top - height;
    const horizontals = [top, bottom, top - height * HALF, top - height * QUARTER, top - height * THIRDS * QUARTER, top + height * QUARTER, bottom - height * QUARTER];
    const verticals = [left - height * QUARTER, left + width + height * QUARTER, left, left + width];
    visit(horizontals, (y): void => {
        line(grid, [left - width * HALF, y], [left + width + width * HALF, y], 1, stroke, undefined);
    });
    visit(verticals, (x): void => {
        line(grid, [x, top + height * HALF], [x, bottom - height * HALF], 1, stroke, undefined);
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
    visit(upper, (y): void => {
        line(grid, [left - width, y], [left + 2 * width, y], 2, stroke, stroke);
    });
    visit(lower, (y): void => {
        line(grid, [offset, y], [left + 2 * width + LOCKUP_MARGIN, y], 2, stroke, stroke);
    });
    let ninths = 0;
    for (let k = 1; k < THIRDS * THIRDS; k += 1) {
        if (k % THIRDS !== 0) {
            line(grid, [left - width, bottom - k * ninth], [left + 2 * width, bottom - k * ninth], 2, stroke, stroke);
            ninths += 1;
        }
    }
    const verticals = [left, left + wthird, left + width, left + width - wthird];
    visit(verticals, (x): void => {
        line(grid, [x, top], [x, bottom - height], 2, stroke, stroke);
    });
    for (let band = 0; band < THIRDS; band += 1) {
        label(grid, String(band + 1), height * HALF, offset - LABEL_OFFSET, top - band * third, false, 0, stroke);
        label(grid, String(band + 1), height * HALF, offset - LABEL_OFFSET, bottom - band * third, false, 0, stroke);
    }
    for (let band = 0; band < THIRDS * THIRDS; band += 1) {
        label(grid, String((band % THIRDS) + 1), height / THIRDS, left - width, bottom - band * ninth, false, 0, stroke);
    }
    return upper.length + lower.length + ninths + verticals.length;
};

const condensedLockup = (grid: Layer, selected: PageItem, box: Box, stroke: Color): number => {
    const { left, top, width, height } = box;
    const bottom = top - height;
    const third = height / THIRDS;
    const wthird = width / THIRDS;
    const quarter = width / QUARTERS;
    const horizontals = [top, top - third, top - 2 * third, bottom, bottom - third, bottom - 2 * third, bottom - height, top + third, top + 2 * third];
    visit(horizontals, (y): void => {
        line(grid, [left - wthird, y], [left + width + wthird, y], 2, stroke, stroke);
    });
    const verticals = [left, left + quarter, left + 2 * quarter, left + THIRDS * quarter, left + width];
    visit(verticals, (x): void => {
        line(grid, [x, bottom - height + width + 2 * wthird], [x, bottom - height], 2, stroke, stroke);
    });
    visit([left - height, left + width], (x): void => {
        const copy = selected.duplicate();
        copy.rotate(RIGHT_ANGLE);
        copy.position = [x, bottom - height + copy.height];
        copy.move(grid, ElementPlacement.PLACEATEND);
    });
    for (let band = 0; band < THIRDS; band += 1) {
        label(grid, String(band + 1), height * HALF, left - wthird - LABEL_OFFSET, top - band * third, false, 0, stroke);
    }
    for (let column = 0; column < QUARTERS; column += 1) {
        label(grid, String(column + 1), height / THIRDS, left + column * quarter, top, true, quarter, stroke);
    }
    return horizontals.length + verticals.length;
};

const lockup = (doc: Document, request: Lockup): number => {
    const [selected] = items<PageItem>(doc.selection);
    if (selected === undefined) {
        throw new Error('Nothing is selected');
    }
    const box = selectionBox(doc);
    app.coordinateSystem = CoordinateSystem.ARTBOARDCOORDINATESYSTEM;
    const grid = layer(doc, LOCKUP_LAYER);
    grid.locked = false;
    const stroke = color(LOCKUP_COLOR);
    const count = fold(
        select([{ mode: 'horizontal', draw: (): number => horizontalLockup(grid, box, stroke) }, { mode: 'vertical', draw: (): number => verticalLockup(grid, selected, box, stroke) }, { mode: 'condensed', draw: (): number => condensedLockup(grid, selected, box, stroke) }], (row): boolean => row.mode === request.mode),
        0,
        (total, row): number => total + row.draw(),
    );
    grid.locked = true;
    return count;
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
    return max === g ? (b - r) / delta + SECTOR_GREEN : (r - g) / delta + SECTOR_BLUE;
};

const guideColor = (doc: Document): Color => {
    const channels = collect(['red', 'green', 'blue'], (name): number => CHANNEL * app.preferences.getRealPreference(`Guide/Color/${name}`));
    if (doc.documentColorSpace === $.global.DocumentColorSpace.RGB) {
        return color({ model: 'RGB', values: channels });
    }
    const [r, g, b] = collect(channels, (channel): number => channel / CHANNEL) as [number, number, number];
    const max = Math.max(r, g, b);
    const delta = max - Math.min(r, g, b);
    const saturation = max === 0 ? 0 : (delta / max) * PERCENT;
    const value = max * PERCENT;
    const s = saturation > HSV_SATURATED ? PERCENT : saturation;
    const v = saturation < HSV_LOW_SATURATION && value > HSV_HIGH_VALUE ? HSV_HALF_VALUE : value;
    const hue = hueOf(r, g, b, max, delta);
    const chroma = (v / PERCENT) * (s / PERCENT);
    const x = chroma * (1 - Math.abs((hue % 2) - 1));
    const m = v / PERCENT - chroma;
    const sectors: [number, number, number][] = [
        [chroma, x, 0],
        [x, chroma, 0],
        [0, chroma, x],
        [0, x, chroma],
        [x, 0, chroma],
        [chroma, 0, x],
    ];
    const [r1, g1, b1] = sectors[Math.floor(hue)] ?? [0, 0, 0];
    const k = 1 - Math.max(r1 + m, g1 + m, b1 + m);
    const cmyk = collect([r1 + m, g1 + m, b1 + m], (channel): number => (k === 1 ? 0 : ((1 - channel - k) / (1 - k)) * PERCENT));
    return color({ model: 'CMYK', values: cmyk.concat([k * PERCENT]) });
};

const rectangleOf = (bounds: Rect): { readonly left: number; readonly top: number; readonly right: number; readonly bottom: number } => ({ left: bounds[0], top: bounds[1], right: bounds[2], bottom: bounds[3] });

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

const clipped = (from: [number, number], to: [number, number], rect: { readonly left: number; readonly top: number; readonly right: number; readonly bottom: number }): [number, number][] => {
    const k = (to[1] - from[1]) / (to[0] - from[0]);
    const b = from[1] - k * from[0];
    const candidates: [number, number][] = [
        [rect.left, k * rect.left + b],
        [rect.right, k * rect.right + b],
        [(rect.top - b) / k, rect.top],
        [(rect.bottom - b) / k, rect.bottom],
    ];
    return select(candidates, ([x, y]): boolean => x >= rect.left && x <= rect.right && y >= rect.bottom && y <= rect.top);
};

const inside = (from: [number, number], to: [number, number], rect: { readonly left: number; readonly top: number; readonly right: number; readonly bottom: number }): boolean =>
    fold([from, to], true, (within, [x, y]): boolean => within && x >= rect.left && x <= rect.right && y >= rect.bottom && y <= rect.top);

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
    const { edges } = request;
    const listed: Segment[] = [];
    visit(
        select([{ on: edges.top, y: box.top }, { on: edges.bottom, y: box.bottom }, { on: edges.centerY, y: centerY }], (row): boolean => row.on),
        (row): void => {
            listed.push({ from: [box.left, row.y], to: [box.right, row.y], dimension: width });
        },
    );
    visit(
        select([{ on: edges.left, x: box.left }, { on: edges.right, x: box.right }, { on: edges.centerX, x: centerX }], (row): boolean => row.on),
        (row): void => {
            listed.push({ from: [row.x, box.top], to: [row.x, box.bottom], dimension: height });
        },
    );
    const diagonal = (width * width + height * height) ** HALF;
    if (edges.leftDiagonal) {
        listed.push({ from: [box.left, box.top], to: [box.right, box.bottom], dimension: diagonal });
    }
    if (edges.rightDiagonal) {
        listed.push({ from: [box.right, box.top], to: [box.left, box.bottom], dimension: diagonal });
    }
    return listed;
};

const objectGuides = (doc: Document, request: ObjectGuides): number => {
    const target = layer(doc, request.layerName);
    if (request.clearLayer) {
        visit(items(target.pageItems), (item): void => item.remove());
    }
    const stroke = guideColor(doc);
    const { scaleFactor: scale } = doc;
    const { margins } = request;
    const artboard = (doc.artboards[doc.artboards.getActiveArtboardIndex()] as Artboard).artboardRect;
    const canvas = doc.visibleBounds;
    const frame = request.extendTo === 'artboard' ? rectangleOf(artboard) : rectangleOf(canvas);
    let drawn = 0;
    visit(items<PageItem>(doc.selection), (item): void => {
        const bounds = request.bounds === 'geometric' ? item.geometricBounds : item.visibleBounds;
        const expanded: Rect = [bounds[0] - margins.left / scale, bounds[1] + margins.top / scale, bounds[2] + margins.right / scale, bounds[3] - margins.bottom / scale];
        const group = target.groupItems.add();
        group.name = `${item.name}_guides`;
        visit(segments(request, expanded, artboard), (segment): void => {
            const d = request.extension.mode === 'percentage' ? (segment.dimension * request.extension.value) / PERCENT : request.extension.value / scale;
            const [from, to] = extended(segment.from, segment.to, d);
            const diagonal = from[0] !== to[0] && from[1] !== to[1];
            if (request.extendTo !== 'object' && inside(from, to, frame)) {
                return;
            }
            const ends = request.extendTo !== 'object' && diagonal ? clipped(from, to, frame) : [from, to];
            if (ends.length !== 2) {
                return;
            }
            const path = line(group, ends[0] as [number, number], ends[1] as [number, number], OBJECT_GUIDE_WEIGHT, stroke, undefined);
            path.guides = request.drawAs === 'guides';
            drawn += 1;
        });
        if (group.pageItems.length === 0) {
            group.remove();
        }
    });
    target.zOrder(ZOrderMethod.BRINGTOFRONT);
    return drawn;
};

// --- [BENTO] ---------------------------------------------------------------------------

interface Cell {
    readonly x: number;
    readonly y: number;
    readonly w: number;
    readonly h: number;
}

const randomInt = (range: Range): number => range.min + Math.floor(Math.random() * (range.max - range.min + 1));

const randomRatio = (range: Range): number => range.min / PERCENT + (Math.random() * (range.max - range.min)) / PERCENT;

const gridBox = (doc: Document, commands: Commands): Cell => {
    const selected = items<PageItem>(doc.selection);
    if (selected.length === 0) {
        const rect = (doc.artboards[doc.artboards.getActiveArtboardIndex()] as Artboard).artboardRect;
        return { x: rect[0], y: rect[1], w: rect[2] - rect[0], h: rect[1] - rect[3] };
    }
    const scratch = doc.layers.add();
    app.activeDocument.selection = collect(selected, (item): PageItem => item.duplicate(scratch, ElementPlacement.PLACEATEND));
    app.executeMenuCommand(commands.noCompoundPath);
    app.executeMenuCommand(commands.group);
    const union = fold(items(scratch.pageItems), [Number.POSITIVE_INFINITY, Number.NEGATIVE_INFINITY, Number.NEGATIVE_INFINITY, Number.POSITIVE_INFINITY], (bounds, item): number[] => {
        const { geometricBounds } = item;
        return [
            Math.min(bounds[0] ?? 0, geometricBounds[0]),
            Math.max(bounds[1] ?? 0, geometricBounds[1]),
            Math.max(bounds[2] ?? 0, geometricBounds[2]),
            Math.min(bounds[3] ?? 0, geometricBounds[3]),
        ];
    }) as [number, number, number, number];
    scratch.remove();
    visit(selected, (item): void => item.remove());
    return { x: union[0], y: union[1], w: union[2] - union[0], h: union[1] - union[3] };
};

const cells = (doc: Document, list: Cell[], radius: number, name: string): number => {
    const container = doc.activeLayer;
    const group = container.groupItems.add();
    group.name = name;
    const drawn = collect(list, (cell): PathItem => container.pathItems.roundedRectangle(cell.y, cell.x, cell.w, cell.h, radius, radius));
    for (let index = drawn.length - 1; index >= 0; index -= 1) {
        (drawn[index] as PathItem).move(group, ElementPlacement.PLACEATEND);
    }
    app.activeDocument.selection = null;
    return drawn.length;
};

const splitCell = (cell: Cell, request: BentoGrid, previous: 'single' | 'row' | 'column'): { readonly kind: 'single' | 'row' | 'column'; readonly cells: Cell[] } => {
    if (request.splitRows !== undefined && Math.random() > SPLIT_CHANCE && previous !== 'row') {
        const left = (cell.w - request.columnGutter) * randomRatio(request.splitRows);
        const right = cell.w - request.columnGutter - left;
        return { kind: 'row', cells: [{ x: cell.x, y: cell.y, w: left, h: cell.h }, { x: cell.x + left + request.columnGutter, y: cell.y, w: right, h: cell.h }] };
    }
    if (request.splitColumns !== undefined && Math.random() > SPLIT_CHANCE && previous !== 'column') {
        const top = (cell.h - request.rowGutter) * randomRatio(request.splitColumns);
        const bottom = cell.h - request.rowGutter - top;
        return { kind: 'column', cells: [{ x: cell.x, y: cell.y, w: cell.w, h: top }, { x: cell.x, y: cell.y - top - request.rowGutter, w: cell.w, h: bottom }] };
    }
    return { kind: 'single', cells: [cell] };
};

const bentoGrid = (doc: Document, request: BentoGrid, grid: Cell): number => {
    const columnCount = randomInt(request.columns);
    const columnWidth = (grid.w - (columnCount - 1) * request.columnGutter) / columnCount;
    const listed: Cell[] = [];
    let { x } = grid;
    for (let column = 0; column < columnCount; column += 1) {
        const rowCount = randomInt(request.rows);
        const rowHeight = (grid.h - (rowCount - 1) * request.rowGutter) / rowCount;
        let { y } = grid;
        let previous: 'single' | 'row' | 'column' = 'single';
        for (let row = 0; row < rowCount; row += 1) {
            const split = splitCell({ x, y, w: columnWidth, h: rowHeight }, request, previous);
            listed.push(...split.cells);
            previous = split.kind;
            y -= rowHeight + request.rowGutter;
        }
        x += columnWidth + request.columnGutter;
    }
    return cells(doc, listed, request.cornerRadius, 'BENTO');
};

const packed = (leaves: Cell[], request: BentoTotal, maxCount: number): Cell[] => {
    let list = leaves;
    for (let iteration = 0; iteration < PACK_ITERATIONS && list.length < maxCount; iteration += 1) {
        const largest = fold(list, list[0] as Cell, (best, cell): Cell => (cell.w * cell.h > best.w * best.h ? cell : best));
        const canH = largest.w >= 2 * request.minSize + request.gutter;
        const canV = largest.h >= 2 * request.minSize + request.gutter;
        if (!(canH || canV)) {
            return list;
        }
        const flipped = canH && canV && Math.random() < FLIP_CHANCE;
        const splitHoriz = (canH && canV ? largest.w > largest.h : canH) !== flipped;
        const avail = (splitHoriz ? largest.w : largest.h) - request.gutter;
        const size = Math.min(Math.max(request.minSize + Math.random() * (avail - 2 * request.minSize), request.minSize), avail - request.minSize);
        const children: Cell[] = splitHoriz
            ? [{ x: largest.x, y: largest.y, w: size, h: largest.h }, { x: largest.x + size + request.gutter, y: largest.y, w: avail - size, h: largest.h }]
            : [{ x: largest.x, y: largest.y, w: largest.w, h: size }, { x: largest.x, y: largest.y - size - request.gutter, w: largest.w, h: avail - size }];
        list = select(list, (cell): boolean => cell !== largest).concat(children);
    }
    return list;
};

const zones = (grid: Cell, hero: Cell, request: BentoTotal): Cell[] => {
    const { gutter: g } = request;
    const reach = (): boolean => Math.random() < SPLIT_CHANCE;
    const leftStrip = hero.x - grid.x - g;
    const rightStrip = grid.x + grid.w - (hero.x + hero.w) - g;
    const topStrip = grid.y - hero.y - g;
    const bottomStrip = hero.y - hero.h - (grid.y - grid.h) - g;
    const listed: Cell[] = [];
    if (leftStrip >= request.minSize) {
        listed.push({ x: grid.x, y: grid.y, w: leftStrip, h: grid.h });
    }
    if (rightStrip >= request.minSize) {
        listed.push({ x: hero.x + hero.w + g, y: grid.y, w: rightStrip, h: grid.h });
    }
    const spanLeft = leftStrip >= request.minSize && reach();
    const spanRight = rightStrip >= request.minSize && reach();
    const x = spanLeft ? grid.x : hero.x;
    const w = (spanRight ? grid.x + grid.w : hero.x + hero.w) - x;
    if (topStrip >= request.minSize) {
        listed.push({ x, y: grid.y, w, h: topStrip });
    }
    if (bottomStrip >= request.minSize) {
        listed.push({ x, y: hero.y - hero.h - g, w, h: bottomStrip });
    }
    return listed;
};

const snapped = (offset: number, room: number, edge: number): number => {
    if (room - offset < edge) {
        return room;
    }
    return offset < edge ? 0 : offset;
};

const bentoTotal = (doc: Document, request: BentoTotal, grid: Cell): number => {
    if (!request.hero) {
        return cells(doc, packed([grid], request, request.maxCount), request.cornerRadius, 'BENTO_TOTAL');
    }
    const scale = HERO_SCALE_BASE + Math.random() * HERO_SCALE_SPAN;
    const heroW = Math.max(grid.w * scale, request.minSize);
    const heroH = Math.max(grid.h * scale, request.minSize);
    const edge = request.minSize + request.gutter;
    const dx = request.centerHero ? (grid.w - heroW) * HALF : snapped(Math.random() * (grid.w - heroW), grid.w - heroW, edge);
    const dy = request.centerHero ? (grid.h - heroH) * HALF : snapped(Math.random() * (grid.h - heroH), grid.h - heroH, edge);
    const hero: Cell = { x: grid.x + dx, y: grid.y - dy, w: heroW, h: heroH };
    const surrounding = zones(grid, hero, request);
    const remaining = request.maxCount - 1;
    const shares = collect(surrounding, (): number => 1);
    for (let extra = surrounding.length; extra < remaining && surrounding.length > 0; extra += 1) {
        const index = Math.floor(Math.random() * surrounding.length);
        shares[index] = (shares[index] ?? 0) + 1;
    }
    const listed = fold(surrounding, [hero], (all, zone, index): Cell[] => all.concat(packed([zone], request, shares[index] ?? 1)));
    return cells(doc, listed, request.cornerRadius, request.centerHero ? 'BENTO_HERO_CENTER' : 'BENTO_HERO');
};

// --- [ISOMETRIC] -----------------------------------------------------------------------

const ISO_LAYER = 'ISOGrid';

const isometricArtboard = (doc: Document, parent: Layer, artboard: Artboard, request: Isometric): number => {
    const [left, top, right, bottom] = artboard.artboardRect;
    app.activeDocument.rulerOrigin = [left, top];
    const sub = parent.layers.add();
    sub.name = `${artboard.name} iso grid`;
    const width = right - left;
    const abHeight = top - bottom;
    const x2 = left + width * HALF;
    let height = abHeight;
    let seedTop = top;
    const seed = (): PathItem => line(sub, [x2, seedTop - height], [x2, bottom + height], 1, doc.defaultStrokeColor, undefined);
    const m = seed();
    height += height;
    seedTop += height * HALF;
    const l = seed();
    const k = seed();
    const lines: PathItem[] = [l, k, m];
    for (let ring = 1; x2 + request.spacing * ring <= right; ring += 1) {
        const step = request.spacing * ring;
        visit([step, -step, step * HALF, -step * HALF], (dx): void => {
            const copy = m.duplicate() as PathItem;
            copy.translate(dx, 0);
            lines.push(copy);
        });
        const rising = k.duplicate() as PathItem;
        rising.translate(step, 0);
        rising.rotate(-request.angle, true, true, true, true, Transformation.CENTER);
        if (rising.top + rising.height * HALF <= top) {
            rising.remove();
            break;
        }
        lines.push(rising);
        const falling = k.duplicate() as PathItem;
        falling.translate(-step, 0);
        falling.rotate(-request.angle, true, true, true, true, Transformation.CENTER);
        lines.push(falling);
        visit([step, -step], (dx): void => {
            const copy = l.duplicate() as PathItem;
            copy.translate(dx, 0);
            copy.rotate(request.angle, true, true, true, true, Transformation.CENTER);
            lines.push(copy);
        });
    }
    l.rotate(request.angle, true, true, true, true, Transformation.CENTER);
    k.rotate(-request.angle, true, true, true, true, Transformation.CENTER);
    const group = sub.groupItems.add();
    const clip = sub.pathItems.rectangle(top, left, width, abHeight);
    clip.move(group, ElementPlacement.PLACEATBEGINNING);
    visit(lines, (item): void => {
        item.move(group, ElementPlacement.PLACEATEND);
    });
    group.clipped = true;
    return lines.length;
};

const isometric = (doc: Document, request: Isometric): number => {
    const { rulerOrigin } = doc;
    app.activeDocument.rulerOrigin = [0, doc.height];
    app.coordinateSystem = CoordinateSystem.ARTBOARDCOORDINATESYSTEM;
    try {
        const parent = layer(doc, ISO_LAYER);
        const indices = request.artboards ?? collect(items(doc.artboards), (_artboard, index): number => index);
        return fold(indices, 0, (count, index): number => count + isometricArtboard(doc, parent, doc.artboards[index] as Artboard, request));
    } finally {
        app.activeDocument.rulerOrigin = rulerOrigin;
    }
};

// --- [ENTRY] ---------------------------------------------------------------------------

const drawn = (doc: Document, request: Request, commands: Commands): number => {
    if (request.operation === 'from_selection' || request.operation === 'release') {
        const count = flatten(items<PageItem>(doc.selection)).length;
        app.executeMenuCommand(request.operation === 'release' ? commands.releaseGuide : commands.makeGuide);
        return count;
    }
    if (request.operation === 'lockup') {
        return lockup(doc, request);
    }
    if (request.operation === 'object') {
        return objectGuides(doc, request);
    }
    if (request.operation === 'isometric') {
        return isometric(doc, request);
    }
    const grid = gridBox(doc, commands);
    if (request.mode === 'grid') {
        return bentoGrid(doc, request, grid);
    }
    return bentoTotal(doc, request, grid);
};

const guides = (request: { readonly request: Request; readonly commands: Commands }, _at: Site): Reading<JsonObject> => {
    const doc = app.activeDocument;
    const system = app.coordinateSystem;
    try {
        return { value: { kind: 'guidesDrawn', operation: request.request.operation, drawn: drawn(doc, request.request, request.commands) }, unavailable: [] };
    } finally {
        app.coordinateSystem = system;
    }
};

run(guides);
