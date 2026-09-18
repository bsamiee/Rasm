/// <reference path="./prelude.ts"/>

declare global {
    enum CoordinateSystem {}
}

// --- [PRELUDE] -------------------------------------------------------------------------

const { assign, collect, flatten, items, nth, present, properties, range, run, split, visit, walk }: Prelude = $.evalFile(new File(`${new File($.fileName).path}/prelude.jsx`));

// --- [CANVAS] --------------------------------------------------------------------------

const CANVAS_HALF_EXTENT = 16_383;
const PROBE_SIDE = 300;

interface Changes {
    readonly duplicate: {
        readonly copies: number;
        readonly columns: number;
        readonly spacing: number;
        readonly nameTemplate: string;
        readonly insertLast: boolean;
        readonly copyArtwork: boolean;
    };
    readonly properties: Partial<Pick<Artboard, 'rulerOrigin' | 'rulerPAR' | 'showCenter' | 'showCrossHairs' | 'showSafeAreas'>>;
}

const fits = (doc: Document, rect: Rect, change: Changes['duplicate']): { readonly fitting: number; readonly reason: string }[] => {
    const probe = doc.pathItems.rectangle(0, 0, PROBE_SIDE, PROBE_SIDE);
    const [probeX, probeY] = probe.position;
    const shift = 1 + (probeX * 2 - (CANVAS_HALF_EXTENT + 1) - (probeY * 2 + CANVAS_HALF_EXTENT + 1)) / 2;
    probe.position = [probeX - shift, probeY + shift];
    const artboardRect = doc.pathItems.rectangle(rect[1], rect[0], rect[2] - rect[0], rect[1] - rect[3]);
    const left = Math.floor(artboardRect.position[0] - probe.position[0]);
    const top = Math.floor(probe.position[1] - artboardRect.position[1]);
    const right = left + (rect[2] - rect[0]);
    const bottom = top + (rect[1] - rect[3]);
    artboardRect.remove();
    probe.remove();
    const stepX = right - left + change.spacing;
    if (right + stepX * (change.columns - 1) > CANVAS_HALF_EXTENT) {
        return [{ fitting: Math.floor((CANVAS_HALF_EXTENT - right) / stepX) + 1, reason: 'columnsExceedCanvas' }];
    }
    const rowsUsed = Math.floor(change.copies / change.columns);
    const stepY = bottom - top + change.spacing;
    if (bottom + stepY * rowsUsed > CANVAS_HALF_EXTENT) {
        return [{ fitting: change.columns - 1 + change.columns * Math.floor((CANVAS_HALF_EXTENT - bottom) / stepY), reason: 'rowsExceedCanvas' }];
    }
    return [];
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

// --- [CHANGES] -------------------------------------------------------------------------

const CHANGES: { readonly [K in keyof Changes]: (doc: Document, index: number, change: Changes[K], at: Site) => Reading<JsonObject> } = {
    duplicate: (doc, sourceIndex, change) => {
        const source = nth(doc.artboards, sourceIndex);
        const ab = source.artboardRect;
        const [refusal] = fits(doc, ab, change);
        if (refusal !== undefined) {
            return present(split([{ reason: refusal.reason, fitting: refusal.fitting }]));
        }
        visit(change.copyArtwork ? items(doc.layers) : [], (layer): void => {
            const unlocked = layer;
            unlocked.locked = false;
            unlocked.visible = true;
        });
        const held = collect(change.copyArtwork ? flatten(items(doc.pageItems)) : [], (item) => {
            const row = { item, locked: item.locked, hidden: item.hidden };
            const freed = item;
            freed.locked = false;
            freed.hidden = false;
            return row;
        });
        const width = String(change.copies).length;
        const zeros = collect(range(width), (): string => '0').join('');
        const applied = collect(range(change.copies), (index): JsonObject => {
            const n = index + 1;
            const column = n % change.columns;
            const row = Math.floor(n / change.columns);
            const left = ab[0] + column * (ab[2] - ab[0] + change.spacing);
            const top = ab[1] + row * (ab[3] - ab[1] - change.spacing);
            const rect: Rect = [left, top, left + (ab[2] - ab[0]), top + (ab[3] - ab[1])];
            const name = change.nameTemplate
                .split('%a')
                .join(source.name)
                .split('%n')
                .join((zeros + n).slice(-width));
            if (!change.insertLast) {
                doc.artboards.insert(rect, sourceIndex + n);
            }
            const artboard = change.insertLast ? doc.artboards.add(rect) : nth(doc.artboards, sourceIndex + n);
            artboard.name = name;
            return { name, rect, copiedItems: change.copyArtwork ? copied(doc, sourceIndex, ab, rect) : 0 };
        });
        visit(held, (row): void => {
            const restored = row.item;
            restored.locked = row.locked;
            restored.hidden = row.hidden;
        });
        doc.artboards.setActiveArtboardIndex(sourceIndex);
        return present(split(applied));
    },
    properties: (doc, index, change, at) => {
        const target = nth(doc.artboards, index);
        visit(properties(change), (info): void => {
            assign(target, info.name, change[info.name as keyof typeof change]);
        });
        const reading = walk(at, target, []);
        return { value: split([reading.value]), unavailable: reading.unavailable };
    },
};

// --- [ENTRY] ---------------------------------------------------------------------------

const artboards = <const K extends keyof Changes>(
    request: { readonly artboard: { readonly index: number } | { readonly active: true }; readonly operation: K; readonly change: Changes[K] },
    at: Site,
): Reading<JsonObject> => {
    const doc = app.activeDocument;
    const index = 'index' in request.artboard ? request.artboard.index : doc.artboards.getActiveArtboardIndex();
    if (index < 0 || index >= doc.artboards.length) {
        return present(split([{ operation: request.operation, reason: 'artboardOutOfRange', count: doc.artboards.length }]));
    }
    const system = app.coordinateSystem;
    app.coordinateSystem = CoordinateSystem.DOCUMENTCOORDINATESYSTEM;
    try {
        return CHANGES[request.operation](doc, index, request.change, at);
    } finally {
        app.coordinateSystem = system;
    }
};

run<{ readonly [K in keyof Changes]: { readonly artboard: { readonly index: number } | { readonly active: true }; readonly operation: K; readonly change: Changes[K] } }[keyof Changes]>(artboards);
