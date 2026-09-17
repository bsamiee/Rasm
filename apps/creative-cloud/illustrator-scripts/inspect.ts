/// <reference path="./prelude.ts"/>

// --- [HOST] ----------------------------------------------------------------------------

declare const $: $;
declare const app: Application;

// --- [PRELUDE] -------------------------------------------------------------------------

const { all, dump, each, reference, run, walk }: Prelude = $.evalFile(new File(`${new File($.fileName).path}/prelude.jsx`));

// --- [READERS] -------------------------------------------------------------------------

const readLayer = (layer: Layer, at: Site): Reading<Json> => walk(at, layer, [['layers', (site): Reading<Json> => each(site, layer.layers, readLayer)]]);

const pageItems = (doc: Document, layer: string | undefined): PageItems => (layer === undefined ? doc.pageItems : doc.layers.getByName(layer).pageItems);

// --- [ENTRY] ---------------------------------------------------------------------------

const inspect = (request: { readonly document?: string; readonly items?: { readonly layer?: string; readonly offset: number; readonly limit: number } }, at: Site): Reading<JsonObject> => {
    const doc = request.document === undefined ? app.activeDocument : app.open(new File(request.document));
    const paging = request.items;
    return all(at, [
        ['kind', (site): Reading<Json> => reference('inspection', site)],
        ['document', (site): Reading<Json> => walk(site, doc, [['artboards', (inner): Reading<Json> => each(inner, doc.artboards, dump)]])],
        [
            'swatches',
            (site): Reading<Json> =>
                each(site, doc.swatchGroups, (group, inner): Reading<Json> => walk(inner, group, [['swatches', (deeper): Reading<Json> => each(deeper, group.getAllSwatches(), dump)]])),
        ],
        [
            'gradients',
            (site): Reading<Json> =>
                each(site, doc.gradients, (gradient, inner): Reading<Json> => walk(inner, gradient, [['gradientStops', (deeper): Reading<Json> => each(deeper, gradient.gradientStops, dump)]])),
        ],
        ['patterns', (site): Reading<Json> => each(site, doc.patterns, dump)],
        ['brushes', (site): Reading<Json> => each(site, doc.brushes, dump)],
        ['symbols', (site): Reading<Json> => each(site, doc.symbols, dump)],
        ['graphicStyles', (site): Reading<Json> => each(site, doc.graphicStyles, dump)],
        [
            'characterStyles',
            (site): Reading<Json> =>
                each(site, doc.characterStyles, (style, inner): Reading<Json> => walk(inner, style, [['characterAttributes', (deeper): Reading<Json> => dump(style.characterAttributes, deeper)]])),
        ],
        [
            'paragraphStyles',
            (site): Reading<Json> =>
                each(
                    site,
                    doc.paragraphStyles,
                    (style, inner): Reading<Json> =>
                        walk(inner, style, [
                            ['characterAttributes', (deeper): Reading<Json> => dump(style.characterAttributes, deeper)],
                            ['paragraphAttributes', (deeper): Reading<Json> => dump(style.paragraphAttributes, deeper)],
                        ]),
                ),
        ],
        ['layers', (site): Reading<Json> => each(site, doc.layers, readLayer)],
        ['itemCount', (site): Reading<Json> => reference(pageItems(doc, paging?.layer).length, site)],
        [
            'items',
            (site): Reading<Json> => {
                const items = pageItems(doc, paging?.layer);
                const [offset, limit] = paging === undefined ? [0, items.length] : [paging.offset, paging.limit];
                return each(
                    site,
                    Array.prototype.slice.call(items, offset, offset + limit),
                    (item: PageItem, inner): Reading<Json> =>
                        all(inner, [
                            ['typename', (deeper): Reading<Json> => reference(item.typename, deeper)],
                            ['uuid', (deeper): Reading<Json> => reference(item.uuid, deeper)],
                            ['name', (deeper): Reading<Json> => reference(item.name, deeper)],
                            ['layer', (deeper): Reading<Json> => reference(item.layer.name, deeper)],
                        ]),
                );
            },
        ],
    ]);
};

run(inspect);
