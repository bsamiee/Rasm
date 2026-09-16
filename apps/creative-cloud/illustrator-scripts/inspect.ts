/// <reference path="./prelude.ts"/>

// --- [PRELUDE] -------------------------------------------------------------------------

const { all, dump, each, reference, run, walk }: Prelude = $.evalFile(new File(`${new File($.fileName).path}/prelude.jsx`));

// --- [READERS] -------------------------------------------------------------------------

const readLayer = (layer: Layer, at: Site): Reading<Json> => walk(at, layer, [['layers', (site): Reading<Json> => each(site, layer.layers, readLayer)]]);

const readSwatchGroup = (group: SwatchGroup, at: Site): Reading<Json> => walk(at, group, [['swatches', (site): Reading<Json> => each(site, group.getAllSwatches(), dump)]]);

const readGradient = (gradient: Gradient, at: Site): Reading<Json> => walk(at, gradient, [['gradientStops', (site): Reading<Json> => each(site, gradient.gradientStops, dump)]]);

const readCharacterStyle = (style: CharacterStyle, at: Site): Reading<Json> => walk(at, style, [['characterAttributes', (site): Reading<Json> => dump(style.characterAttributes, site)]]);

const readParagraphStyle = (style: ParagraphStyle, at: Site): Reading<Json> =>
    walk(at, style, [
        ['characterAttributes', (site): Reading<Json> => dump(style.characterAttributes, site)],
        ['paragraphAttributes', (site): Reading<Json> => dump(style.paragraphAttributes, site)],
    ]);

const readItem = (item: PageItem, at: Site): Reading<Json> =>
    all(at, [
        ['typename', (site): Reading<Json> => reference(item.typename, site)],
        ['uuid', (site): Reading<Json> => reference(item.uuid, site)],
        ['name', (site): Reading<Json> => reference(item.name, site)],
        ['layer', (site): Reading<Json> => reference(item.layer.name, site)],
    ]);

const pageItems = (doc: Document, layer: string | undefined): PageItems => (layer === undefined ? doc.pageItems : doc.layers.getByName(layer).pageItems);

// --- [ENTRY] ---------------------------------------------------------------------------

const inspect = (request: { readonly document?: string; readonly items?: { readonly layer?: string; readonly offset: number; readonly limit: number } }, at: Site): Reading<JsonObject> => {
    const doc = request.document === undefined ? app.activeDocument : app.open(new File(request.document));
    const paging = request.items;
    return all(at, [
        ['kind', (site): Reading<Json> => reference('inspection', site)],
        ['document', (site): Reading<Json> => walk(site, doc, [['artboards', (inner): Reading<Json> => each(inner, doc.artboards, dump)]])],
        ['swatches', (site): Reading<Json> => each(site, doc.swatchGroups, readSwatchGroup)],
        ['gradients', (site): Reading<Json> => each(site, doc.gradients, readGradient)],
        ['patterns', (site): Reading<Json> => each(site, doc.patterns, dump)],
        ['brushes', (site): Reading<Json> => each(site, doc.brushes, dump)],
        ['symbols', (site): Reading<Json> => each(site, doc.symbols, dump)],
        ['graphicStyles', (site): Reading<Json> => each(site, doc.graphicStyles, dump)],
        ['characterStyles', (site): Reading<Json> => each(site, doc.characterStyles, readCharacterStyle)],
        ['paragraphStyles', (site): Reading<Json> => each(site, doc.paragraphStyles, readParagraphStyle)],
        ['layers', (site): Reading<Json> => each(site, doc.layers, readLayer)],
        ['itemCount', (site): Reading<Json> => reference(pageItems(doc, paging?.layer).length, site)],
        [
            'items',
            (site): Reading<Json> => {
                const items = pageItems(doc, paging?.layer);
                const [offset, limit] = paging === undefined ? [0, items.length] : [paging.offset, paging.limit];
                return each(site, Array.prototype.slice.call(items, offset, offset + limit), readItem);
            },
        ],
    ]);
};

run(inspect);
