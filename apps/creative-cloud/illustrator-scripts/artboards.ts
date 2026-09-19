/// <reference path="./prelude.ts"/>

// --- [PRELUDE] -------------------------------------------------------------------------

const { all, assign, collect, each, items, nth, present, properties, range, reference, run, split, visit }: Prelude = $.evalFile(new File(`${new File($.fileName).path}/prelude.jsx`));

// --- [ENTRY] ---------------------------------------------------------------------------

const artboards = (
    request: {
        readonly artboard: { readonly index: number } | { readonly active: true };
        readonly change:
            | { readonly operation: 'properties'; readonly values: Readonly<Partial<Pick<Artboard, 'rulerOrigin' | 'rulerPAR' | 'showCenter' | 'showCrossHairs' | 'showSafeAreas'>>> }
            | {
                  readonly operation: 'duplicate';
                  readonly copies: number;
                  readonly columns: number;
                  readonly spacing: number;
                  readonly nameTemplate: string;
                  readonly insertLast: boolean;
                  readonly copyArtwork: boolean;
              };
    },
    at: Site,
): Reading<JsonObject> => {
    const doc = app.activeDocument;
    const boards = doc.artboards;
    const index = 'index' in request.artboard ? request.artboard.index : boards.getActiveArtboardIndex();
    if (index < 0 || index >= boards.length) {
        return present(split([{ reason: 'artboardOutOfRange', count: boards.length }]));
    }
    const target = nth(boards, index);
    const { change } = request;
    if (change.operation === 'properties') {
        const changed = properties(change.values);
        visit(changed, (info): void => assign(target, info.name, change.values[info.name as keyof typeof change.values]));
        const reading = all(
            at,
            collect(changed, (info): [string, Reader] => [info.name, (site): Reading<Json> => reference(target[info.name as keyof typeof change.values], site)]),
        );
        return { value: split([{ index, properties: reading.value }]), unavailable: reading.unavailable };
    }
    const system = app.coordinateSystem;
    const { selection } = doc;
    const active = boards.getActiveArtboardIndex();
    const count = boards.length;
    const layers = change.copyArtwork ? items(doc.layers) : [];
    for (let cursor = 0; cursor < layers.length; cursor += 1) {
        layers.push(...items(nth(layers, cursor).layers));
    }
    const heldLayers = collect(layers, (layer) => ({ layer, locked: layer.locked, visible: layer.visible }));
    const heldItems = collect(change.copyArtwork ? items(doc.pageItems) : [], (item) => ({ item, locked: item.locked, hidden: item.hidden }));
    app.coordinateSystem = CoordinateSystem.DOCUMENTCOORDINATESYSTEM;
    try {
        visit(heldLayers, ({ layer }): void => {
            layer.locked = false;
            layer.visible = true;
        });
        visit(heldItems, ({ item }): void => {
            item.locked = false;
            item.hidden = false;
        });
        boards.setActiveArtboardIndex(index);
        doc.selection = null;
        if (change.copyArtwork) {
            doc.selectObjectsOnActiveArtboard();
        }
        const sources = change.copyArtwork ? items<PageItem>(doc.selection) : [];
        visit(heldItems.slice().reverse(), ({ item, locked, hidden }): void => {
            item.hidden = hidden;
            item.locked = locked;
            heldItems.pop();
        });
        const ab = target.artboardRect;
        const width = String(change.copies).length;
        const zeros = collect(range(width), (): string => '0').join('');
        const created = each(at, range(change.copies), (copy): Reading<{ index: number; name: string; rect: Rect; copiedItems: number }> => {
            const n = copy + 1;
            const x = (n % change.columns) * (ab[2] - ab[0] + change.spacing);
            const y = Math.floor(n / change.columns) * (ab[3] - ab[1] - change.spacing);
            const rect: Rect = [ab[0] + x, ab[1] + y, ab[2] + x, ab[3] + y];
            const destination = change.insertLast ? boards.length : index + 1 + boards.length - count;
            boards.insert(rect, destination);
            const added = nth(boards, destination);
            added.name = change.nameTemplate
                .split('%a')
                .join(target.name)
                .split('%n')
                .join((zeros + n).slice(-width));
            return present({ index: destination, name: added.name, rect: added.artboardRect, copiedItems: 0 });
        });
        const copied = each(at, range(created.value.length * sources.length), (copy): Reading<null> => {
            const board = nth(created.value, Math.floor(copy / sources.length));
            const item = nth(sources, copy % sources.length).duplicate();
            try {
                item.translate(board.rect[0] - ab[0], board.rect[1] - ab[1]);
            } catch (error) {
                item.remove();
                throw error;
            }
            board.copiedItems += 1;
            return present(null);
        });
        return { value: split(created.value), unavailable: created.unavailable.concat(copied.unavailable) };
    } finally {
        visit(heldItems.reverse(), ({ item, locked, hidden }): void => {
            item.hidden = hidden;
            item.locked = locked;
        });
        visit(heldLayers.reverse(), ({ layer, locked, visible }): void => {
            layer.visible = visible;
            layer.locked = locked;
        });
        boards.setActiveArtboardIndex(active + (!change.insertLast && active > index ? boards.length - count : 0));
        doc.selection = selection;
        app.coordinateSystem = system;
    }
};

run(artboards);
