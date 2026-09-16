/// <reference path="./prelude.ts"/>

// --- [PRELUDE] -------------------------------------------------------------------------

const { all, dump, each, members, run }: Prelude = $.evalFile(new File(`${new File($.fileName).path}/prelude.jsx`));

// --- [READERS] -------------------------------------------------------------------------

const readLayer = (layer: Layer, at: Site): Reading<Json> => all(at, members(layer).concat([['layers', (site): Reading<Json> => each(site, layer.layers, readLayer)]]));

const readGroup = (group: SwatchGroup, at: Site): Reading<Json> => all(at, members(group).concat([['swatches', (site): Reading<Json> => each(site, group.getAllSwatches(), dump)]]));

const readGradient = (gradient: Gradient, at: Site): Reading<Json> => all(at, members(gradient).concat([['gradientStops', (site): Reading<Json> => each(site, gradient.gradientStops, dump)]]));

const readCharacterStyle = (style: CharacterStyle, at: Site): Reading<Json> =>
    all(at, members(style).concat([['characterAttributes', (site): Reading<Json> => dump(style.characterAttributes, site)]]));

const readParagraphStyle = (style: ParagraphStyle, at: Site): Reading<Json> =>
    all(
        at,
        members(style).concat([
            ['characterAttributes', (site): Reading<Json> => dump(style.characterAttributes, site)],
            ['paragraphAttributes', (site): Reading<Json> => dump(style.paragraphAttributes, site)],
        ]),
    );

const readItem = (item: PageItem, at: Site): Reading<Json> =>
    dump(
        {
            typename: item.typename,
            uuid: item.uuid,
            name: item.name,
            layer: item.layer.name,
            geometricBounds: item.geometricBounds,
            visibleBounds: item.visibleBounds,
            hidden: item.hidden,
            locked: item.locked,
            opacity: item.opacity,
            blendingMode: item.blendingMode,
        },
        at,
    );

// --- [ENTRY] ---------------------------------------------------------------------------

const inspect = (request: JsonObject, at: Site): Json => {
    const source = request['document'];
    if (source !== undefined && typeof source !== 'string') {
        throw new Error('document: expected a path');
    }
    const doc = source === undefined ? app.activeDocument : app.open(new File(source));
    const sections = all(at, [
        ['kind', (site): Reading<Json> => dump('inspection', site)],
        ['document', (site): Reading<Json> => all(site, members(doc).concat([['artboards', (inner): Reading<Json> => each(inner, doc.artboards, dump)]]))],
        ['swatches', (site): Reading<Json> => each(site, doc.swatchGroups, readGroup)],
        ['gradients', (site): Reading<Json> => each(site, doc.gradients, readGradient)],
        ['patterns', (site): Reading<Json> => each(site, doc.patterns, dump)],
        ['brushes', (site): Reading<Json> => each(site, doc.brushes, dump)],
        ['symbols', (site): Reading<Json> => each(site, doc.symbols, dump)],
        ['graphicStyles', (site): Reading<Json> => each(site, doc.graphicStyles, dump)],
        ['characterStyles', (site): Reading<Json> => each(site, doc.characterStyles, readCharacterStyle)],
        ['paragraphStyles', (site): Reading<Json> => each(site, doc.paragraphStyles, readParagraphStyle)],
        ['layers', (site): Reading<Json> => each(site, doc.layers, readLayer)],
        ['items', (site): Reading<Json> => each(site, doc.pageItems, readItem)],
    ]);
    sections.value['unavailable'] = sections.unavailable;
    return sections.value;
};

run(inspect);
