/// <reference path="./prelude.ts"/>

// --- [HOST] ----------------------------------------------------------------------------

declare const $: $;
declare const app: Application;

declare global {
    enum CoordinateSystem {}
}

// --- [PRELUDE] -------------------------------------------------------------------------

const { collect, flatten, items, run, visit }: Prelude = $.evalFile(new File(`${new File($.fileName).path}/prelude.jsx`));

// --- [CANVAS] --------------------------------------------------------------------------

const CANVAS_HALF_EXTENT = 16_383;
const PROBE_SIDE = 300;
const PADDING = '000000000';

interface Duplicate {
    readonly copies: number;
    readonly columns: number;
    readonly spacing: number;
    readonly nameTemplate: string;
    readonly insertLast: boolean;
    readonly copyArtwork: boolean;
}

interface Properties {
    readonly rulerOrigin?: [number, number];
    readonly rulerPixelAspectRatio?: number;
    readonly showCenter?: boolean;
    readonly showCrossHairs?: boolean;
    readonly showSafeAreas?: boolean;
}

interface Box {
    readonly left: number;
    readonly top: number;
    readonly right: number;
    readonly bottom: number;
}

const measured = (doc: Document, rect: Rect): Box => {
    const probe = doc.pathItems.rectangle(0, 0, PROBE_SIDE, PROBE_SIDE);
    const [probeX, probeY] = probe.position;
    const shift = 1 + ((probeX * 2 - (CANVAS_HALF_EXTENT + 1) - (probeY * 2 + CANVAS_HALF_EXTENT + 1)) / 2);
    probe.position = [probeX - shift, probeY + shift];
    const artboardRect = doc.pathItems.rectangle(rect[1], rect[0], rect[2] - rect[0], rect[1] - rect[3]);
    const left = Math.floor(artboardRect.position[0] - probe.position[0]);
    const top = Math.floor(probe.position[1] - artboardRect.position[1]);
    const box = { left, top, right: left + (rect[2] - rect[0]), bottom: top + (rect[1] - rect[3]) };
    artboardRect.remove();
    probe.remove();
    return box;
};

const fits = (box: Box, duplicate: Duplicate): { readonly fitting: number; readonly reason: string }[] => {
    const stepX = box.right - box.left + duplicate.spacing;
    const lastRight = box.right + stepX * (duplicate.columns - 1);
    if (lastRight > CANVAS_HALF_EXTENT) {
        return [{ fitting: Math.floor((CANVAS_HALF_EXTENT - box.right) / stepX) + 1, reason: 'columnsExceedCanvas' }];
    }
    let rowsUsed = 0;
    for (let index = 0; index < duplicate.copies; index += 1) {
        rowsUsed += (index + 1) % duplicate.columns === 0 ? 1 : 0;
    }
    const stepY = box.bottom - box.top + duplicate.spacing;
    if (box.bottom + stepY * rowsUsed > CANVAS_HALF_EXTENT) {
        return [{ fitting: duplicate.columns - 1 + duplicate.columns * Math.floor((CANVAS_HALF_EXTENT - box.bottom) / stepY), reason: 'rowsExceedCanvas' }];
    }
    return [];
};

const placed = (ab: Rect, previous: Rect | undefined, n: number, duplicate: Duplicate): Rect => {
    const wrap = n % duplicate.columns === 0;
    const row = n / duplicate.columns;
    const h = ab[3] - ab[1];
    const w = ab[2] - ab[0];
    if (wrap) {
        return [ab[0], ab[1] - duplicate.spacing * row + h * row, ab[2], ab[3] - duplicate.spacing * row + h * row];
    }
    if (previous === undefined) {
        return [ab[2] + duplicate.spacing, ab[1], ab[2] + duplicate.spacing + w, ab[3]];
    }
    return [previous[2] + duplicate.spacing, previous[1], previous[2] + duplicate.spacing + w, previous[3]];
};

const named = (template: string, source: string, n: number, copies: number): string =>
    template
        .split('%a')
        .join(source)
        .split('%n')
        .join((PADDING + n).slice(-String(copies).length));

// --- [ARTWORK] -------------------------------------------------------------------------

interface Held {
    readonly item: PageItem;
    readonly locked: boolean;
    readonly hidden: boolean;
}

const released = (doc: Document): Held[] => {
    visit(items(doc.layers), (layer): void => {
        const unlocked = layer;
        unlocked.locked = false;
        unlocked.visible = true;
    });
    return collect(flatten(items(doc.pageItems)), (item): Held => {
        const held = { item, locked: item.locked, hidden: item.hidden };
        const freed = item;
        freed.locked = false;
        freed.hidden = false;
        return held;
    });
};

const copied = (doc: Document, sourceIndex: number, ab: Rect, target: Rect): number => {
    doc.artboards.setActiveArtboardIndex(sourceIndex);
    doc.selectObjectsOnActiveArtboard();
    const selected = items<PageItem>(doc.selection);
    visit(selected, (item): void => {
        const copy = item.duplicate();
        const [x, y] = copy.position;
        copy.position = [x + (target[2] - ab[2]), y + (target[1] - ab[1])];
    });
    return selected.length;
};

// --- [OPERATIONS] ----------------------------------------------------------------------

const duplicated = (doc: Document, sourceIndex: number, duplicate: Duplicate): JsonObject => {
    const source = doc.artboards[sourceIndex] as Artboard;
    const ab = source.artboardRect;
    const [refusal] = fits(measured(doc, ab), duplicate);
    if (refusal !== undefined) {
        return { kind: 'artboardsRejected', reason: refusal.reason, fitting: refusal.fitting };
    }
    const held = duplicate.copyArtwork ? released(doc) : [];
    const applied: JsonObject[] = [];
    let previous: Rect | undefined;
    for (let n = 1; n <= duplicate.copies; n += 1) {
        const rect = placed(ab, previous, n, duplicate);
        const name = named(duplicate.nameTemplate, source.name, n, duplicate.copies);
        const added = duplicate.insertLast ? doc.artboards.add(rect) : doc.artboards.insert(rect, sourceIndex + n);
        const artboard = added ?? (doc.artboards[sourceIndex + n] as Artboard);
        artboard.name = name;
        const copies = duplicate.copyArtwork ? copied(doc, sourceIndex, ab, rect) : 0;
        applied.push({ name, rect, copiedItems: copies });
        previous = rect;
    }
    visit(held, (row): void => {
        const restored = row.item;
        restored.locked = row.locked;
        restored.hidden = row.hidden;
    });
    doc.artboards.setActiveArtboardIndex(sourceIndex);
    return { kind: 'artboardsApplied', applied };
};

const propertied = (artboard: Artboard, properties: Properties): JsonObject => {
    const target = artboard;
    if (properties.rulerOrigin !== undefined) {
        target.rulerOrigin = properties.rulerOrigin;
    }
    if (properties.rulerPixelAspectRatio !== undefined) {
        target.rulerPAR = properties.rulerPixelAspectRatio;
    }
    if (properties.showCenter !== undefined) {
        target.showCenter = properties.showCenter;
    }
    if (properties.showCrossHairs !== undefined) {
        target.showCrossHairs = properties.showCrossHairs;
    }
    if (properties.showSafeAreas !== undefined) {
        target.showSafeAreas = properties.showSafeAreas;
    }
    const [x, y] = target.rulerOrigin;
    return {
        kind: 'artboardsApplied',
        applied: [
            { name: target.name, rulerOrigin: [x, y], rulerPixelAspectRatio: target.rulerPAR, showCenter: target.showCenter, showCrossHairs: target.showCrossHairs, showSafeAreas: target.showSafeAreas },
        ],
    };
};

// --- [ENTRY] ---------------------------------------------------------------------------

const artboards = (request: { readonly index?: number; readonly properties?: Properties; readonly duplicate?: Duplicate }, _at: Site): Reading<JsonObject> => {
    const doc = app.activeDocument;
    const index = request.index ?? doc.artboards.getActiveArtboardIndex();
    const system = app.coordinateSystem;
    app.coordinateSystem = CoordinateSystem.DOCUMENTCOORDINATESYSTEM;
    try {
        if (request.duplicate !== undefined) {
            return { value: duplicated(doc, index, request.duplicate), unavailable: [] };
        }
        return { value: propertied(doc.artboards[index] as Artboard, request.properties ?? {}), unavailable: [] };
    } finally {
        app.coordinateSystem = system;
    }
};

run(artboards);
