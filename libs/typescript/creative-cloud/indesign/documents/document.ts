import type { Document, Gradient, Layer, Link, MasterSpread, MixedInk, MixedInkGroup, Page, PageItem, PlainColor, PlainSwatch, Spread, Tint } from 'adobe:indesign';
import { Array, Effect, flow, Match, Number, Option, Predicate, Record, Schema, Struct } from 'effect';
import { ParagraphAttributes } from '#/documents/text';
import { Model } from '#/dom/model';
import { Reference } from '#/dom/specifier';
import { read } from '#/dom/value';
import { InDesignError } from '#/error';

// --- [MODELS] --------------------------------------------------------------------------

// --- [GEOMETRY]

class Extent extends Schema.Class<Extent, { readonly brand: unique symbol }>('Extent')(Struct.Record(['top', 'left', 'bottom', 'right'], Schema.Finite), { description: 'Outer stroke bounds in spread points, x 0 at spine or spread center' }) {
    static readonly of = Effect.fn('Extent.of')(function* (...placed: readonly (Page | PageItem)[]) {
        const { AnchorPoint, BoundingBoxLimits, CoordinateSpaces } = yield* Model;
        return yield* Effect.try({
            try: () => {
                const corners: ReadonlyArray<readonly [number, number]> = Array.flatMap(Array.cartesian(placed, [AnchorPoint.TOP_LEFT_ANCHOR, AnchorPoint.BOTTOM_RIGHT_ANCHOR]), ([each, anchor]) =>
                    each.resolve([anchor, BoundingBoxLimits.OUTER_STROKE_BOUNDS, CoordinateSpaces.SPREAD_COORDINATES], CoordinateSpaces.SPREAD_COORDINATES),
                );
                return Extent.make(Struct.makeReducer<typeof Extent.Encoded>({ top: Number.ReducerMin, left: Number.ReducerMin, bottom: Number.ReducerMax, right: Number.ReducerMax }).combineAll(Array.map(corners, ([x, y]) => ({ top: y, left: x, bottom: y, right: x }))));
            },
            catch: InDesignError.refused(`extent of ${Array.map(placed, (each) => each.toSpecifier()).join(', ')}`),
        });
    });
}

// --- [VIEWS]

class LayerView extends Schema.TaggedClass<LayerView, { readonly brand: unique symbol }>()('Layer', {
    ...Reference.fields,
    index: Schema.Int,
    ...Struct.Record(['visible', 'locked', 'printable', 'ignoreWrap'], Schema.Boolean),
    layerColor: Schema.Json.annotate({ description: 'UIColors member or RGB values' }),
    items: Schema.Int,
}) {
    static readonly of = (layer: Layer) => read(LayerView, layer.toSpecifier(), () => ({ _tag: 'Layer', ...Reference.of(layer), index: layer.index, visible: layer.visible, locked: layer.locked, printable: layer.printable, ignoreWrap: layer.ignoreWrap, layerColor: layer.layerColor, items: layer.pageItems.length }));
    static readonly viewed = (model: typeof Model.Service) => flow(Option.liftPredicate(model.owning<Layer>('layerColor')), Option.map(LayerView.of));
}
class LinkView extends Schema.TaggedClass<LinkView, { readonly brand: unique symbol }>()('Link', {
    ...Reference.fields,
    status: Schema.String,
    filePath: Schema.String,
    linkResourceURI: Schema.String,
    linkType: Schema.String,
    needed: Schema.Boolean,
    edited: Schema.Boolean,
    size: Schema.Finite.annotate({ description: 'Source file size in bytes' }),
    date: Schema.String.annotate({ description: 'Source file modification date' }),
    parent: Reference.annotate({ description: 'Placed graphic or story' }),
}) {
    static readonly of = (link: Link) =>
        read(LinkView, link.toSpecifier(), () => ({ _tag: 'Link', ...Reference.of(link), status: link.status, filePath: link.filePath, linkResourceURI: link.linkResourceURI, linkType: link.linkType, needed: link.needed, edited: link.edited, size: link.size, date: link.date, parent: link.parent }));
    static readonly viewed = (model: typeof Model.Service) => flow(Option.liftPredicate(model.owning<Link>('linkResourceURI')), Option.map(LinkView.of));
}
class SwatchView extends Schema.TaggedClass<SwatchView, { readonly brand: unique symbol }>()('Swatch', {
    ...Reference.fields,
    group: Schema.OptionFromOptionalKey(Schema.String).annotate({ description: 'Color group name' }),
    value: Schema.TaggedUnion({
        Swatch: {},
        Color: { model: Schema.String, space: Schema.String, colorValue: Schema.Array(Schema.Finite) },
        Tint: { baseColor: Reference, tintValue: Schema.Finite },
        Gradient: { type: Schema.String, gradientStops: Schema.Array(Schema.Struct({ stopColor: Reference, location: Schema.Finite, midpoint: Schema.Finite })) },
        MixedInk: { baseColor: Reference, inkPercentages: Schema.Array(Schema.Finite) },
    }).annotate({ description: 'Empty Swatch for None and mixed ink groups' }),
}) {
    static readonly of = Effect.fn('SwatchView.of')(function* (swatch: PlainSwatch | PlainColor | Tint | Gradient | MixedInk | MixedInkGroup) {
        const value = Match.value(swatch).pipe(
            Match.when({ constructorName: 'Color' }, ({ model, space, colorValue }) => ({ _tag: 'Color', model, space, colorValue })),
            Match.when({ constructorName: 'Tint' }, ({ baseColor, tintValue }) => ({ _tag: 'Tint', baseColor, tintValue })),
            Match.when({ constructorName: 'Gradient' }, ({ type, gradientStops }) => ({ _tag: 'Gradient', type, gradientStops: Array.map(gradientStops.everyItem().getElements(), ({ stopColor, location, midpoint }) => ({ stopColor, location, midpoint })) })),
            Match.when({ constructorName: 'MixedInk' }, ({ baseColor, inkPercentages }) => ({ _tag: 'MixedInk', baseColor, inkPercentages })),
            Match.orElse(() => ({ _tag: 'Swatch' })),
        );
        return yield* read(SwatchView, swatch.toSpecifier(), () => ({ _tag: 'Swatch', ...Reference.of(swatch), group: Option.map(Option.fromNullOr(swatch.parentColorGroup), Struct.get('name')), value }));
    });
    static readonly viewed = (model: typeof Model.Service) => flow(Option.liftPredicate(model.owning<PlainSwatch | PlainColor | Tint | Gradient | MixedInk | MixedInkGroup>('parentColorGroup')), Option.map(SwatchView.of));
}
class PageView extends Schema.TaggedClass<PageView, { readonly brand: unique symbol }>()('Page', {
    ...Reference.fields,
    documentOffset: Schema.Int,
    side: Schema.String,
    extent: Extent,
    parent: Reference.annotate({ description: 'Spread or parent spread' }),
    appliedMaster: Schema.OptionFromOptionalKey(Reference),
    appliedSection: Reference,
    margins: Schema.Struct({
        ...Struct.Record(['top', 'bottom', 'inside', 'outside'], Schema.Finite),
        columns: Schema.Array(Schema.Tuple([Schema.Finite, Schema.Finite])).annotate({ description: 'Column left and right edges from left margin' }),
    }),
    items: Schema.Struct({ own: Schema.Int, inherited: Schema.Int }).annotate({ description: 'Own and parent page item counts' }),
}) {
    static readonly of = (page: Page) =>
        Effect.flatMap(Extent.of(page), (box) =>
            read(PageView, page.toSpecifier(), () => {
                const { marginPreferences: margins } = page;
                return {
                    _tag: 'Page',
                    ...Reference.of(page),
                    documentOffset: page.documentOffset,
                    side: page.side,
                    extent: box,
                    parent: page.parent,
                    appliedMaster: Option.fromNullOr(page.appliedMaster),
                    appliedSection: page.appliedSection,
                    margins: { top: margins.top, bottom: margins.bottom, inside: margins.left, outside: margins.right, columns: Array.chunksOf(margins.columnsPositions, 2) },
                    items: { own: page.pageItems.length, inherited: page.masterPageItems.length },
                };
            }),
        );
    static readonly viewed = (model: typeof Model.Service) => flow(Option.liftPredicate(model.page), Option.map(PageView.of));
}
class SpreadView extends Schema.TaggedClass<SpreadView, { readonly brand: unique symbol }>()('Spread', {
    ...Reference.fields,
    index: Schema.Int,
    showMasterItems: Schema.Boolean,
    appliedMaster: Schema.OptionFromOptionalKey(Reference).annotate({ description: 'Applied parent, or base parent of a parent spread' }),
    kind: Schema.TaggedUnion({ Spread: { allowPageShuffle: Schema.Boolean }, MasterSpread: { namePrefix: Schema.String, baseName: Schema.String } }),
    pages: Schema.Array(Reference.mapFields(Struct.assign(Struct.pick(PageView.fields, ['side', 'extent'])))).annotate({ description: 'Pages in order' }),
    items: Schema.Int,
}) {
    static readonly of = (found: Spread | MasterSpread) =>
        Effect.flatMap(
            Effect.forEach(found.pages.everyItem().getElements(), (page) => Effect.map(Extent.of(page), (box) => ({ ...Reference.of(page), side: page.side, extent: box }))),
            (pages) =>
                read(SpreadView, found.toSpecifier(), () => ({
                    _tag: 'Spread',
                    ...Reference.of(found),
                    index: found.index,
                    showMasterItems: found.showMasterItems,
                    appliedMaster: Option.fromNullOr(found.appliedMaster),
                    kind: found.constructorName === 'Spread' ? { _tag: 'Spread', allowPageShuffle: found.allowPageShuffle } : { _tag: 'MasterSpread', namePrefix: found.namePrefix, baseName: found.baseName },
                    pages,
                    items: found.pageItems.length,
                })),
        );
    static readonly viewed = (model: typeof Model.Service) => flow(Option.liftPredicate(Predicate.or(model.spread, model.owning<MasterSpread>('namePrefix'))), Option.map(SpreadView.of));
}
class DocumentView extends Schema.TaggedClass<DocumentView, { readonly brand: unique symbol }>()('Document', {
    ...Reference.fields,
    saved: Schema.Boolean,
    modified: Schema.Boolean,
    path: Schema.OptionFromOptionalKey(Schema.String),
    windows: Schema.Array(Schema.String),
    intent: Schema.String,
    facingPages: Schema.Boolean,
    size: Schema.Struct({ width: Schema.Finite, height: Schema.Finite }).annotate({ description: 'Default page size in points' }),
    units: Schema.Struct({ horizontal: Schema.String, vertical: Schema.String, origin: Schema.String }).annotate({ description: 'Ruler units and origin, results stay in points' }),
    pages: Schema.Int,
    spreads: Schema.Int,
    parents: Schema.Array(Reference.mapFields(Struct.assign({ pages: Schema.Int }))).annotate({ description: 'Parent (master) spreads' }),
    sections: Schema.Array(Reference.mapFields(Struct.assign({ start: Schema.String, length: Schema.Int, pageNumberStart: Schema.Int, continueNumbering: Schema.Boolean }))),
    layers: Schema.Array(Reference.mapFields(Struct.assign(Struct.pick(LayerView.fields, ['visible', 'locked', 'printable'])))),
    stories: Schema.Struct({ count: Schema.Int, overset: Schema.Array(Schema.String) }),
    links: Schema.Record(Schema.String, Schema.Int).annotate({ description: 'Link count per LinkStatus member' }),
    fonts: Schema.Array(Schema.Struct({ name: Schema.String, status: Schema.String })).annotate({ description: 'Document fonts with FontStatus' }),
    swatches: Schema.Array(Reference),
    styles: Schema.Struct(Struct.Record(['paragraph', 'character', 'object', 'table', 'cell'], Schema.Int)).annotate({ description: 'Style counts, grouped styles included' }),
    preflightOptions: Schema.Struct({
        ...Struct.Record(['preflightOff', 'preflightIncludeObjectsOnPasteboard', 'preflightIncludeNonprintingObjects'], Schema.Boolean),
        ...Struct.Record(['preflightProfilePolicy', 'preflightWorkingProfile', 'preflightScope', 'preflightWhichLayers'], Schema.String),
    }),
    activeProcess: Schema.OptionFromOptionalKey(Reference).annotate({ description: 'Absent while live preflight is off' }),
    textDefaults: ParagraphAttributes,
}) {
    static readonly Row = Schema.Struct(Struct.pick(DocumentView.fields, ['type', 'specifier', 'name', 'valid', 'saved', 'modified', 'path', 'windows']));
    static readonly overset = ({ stories }: Document): readonly string[] => Array.flatMap(Array.zip(stories.everyItem().overflows, stories.everyItem().id), ([overflows, id]) => (overflows === true ? [stories.itemByID(id).toSpecifier()] : []));
    static readonly row = Effect.fn('DocumentView.row')(function* (document: Document) {
        const { saved } = document;
        return { ...Reference.of(document), saved, modified: document.modified, path: saved ? yield* Model.file(document, 'fullName') : Option.none(), windows: Array.map(document.windows.everyItem().getElements(), (window) => window.toSpecifier()) };
    });
    static readonly of = (found: Document) =>
        Effect.flatMap(DocumentView.row(found), (row) =>
            read(DocumentView, found.toSpecifier(), () => {
                const { documentPreferences: preferences, viewPreferences: view, preflightOptions: preflight } = found;
                return {
                    _tag: 'Document',
                    ...row,
                    intent: preferences.intent,
                    facingPages: preferences.facingPages,
                    size: { width: preferences.pageWidth, height: preferences.pageHeight },
                    units: { horizontal: view.horizontalMeasurementUnits, vertical: view.verticalMeasurementUnits, origin: view.rulerOrigin },
                    pages: found.pages.length,
                    spreads: found.spreads.length,
                    parents: Array.map(found.masterSpreads.everyItem().getElements(), (each) => ({ ...Reference.of(each), pages: each.pages.length })),
                    sections: Array.map(found.sections.everyItem().getElements(), (each) => ({ ...Reference.of(each), start: each.pageStart.toSpecifier(), length: each.length, pageNumberStart: each.pageNumberStart, continueNumbering: each.continueNumbering })),
                    layers: Array.map(found.layers.everyItem().getElements(), (each) => ({ ...Reference.of(each), visible: each.visible, locked: each.locked, printable: each.printable })),
                    stories: { count: found.stories.length, overset: DocumentView.overset(found) },
                    links: Record.map(Array.groupBy(found.links.everyItem().status, String), Array.length),
                    fonts: Array.zipWith(found.fonts.everyItem().name, found.fonts.everyItem().status, (name, status) => ({ name, status })),
                    swatches: found.swatches.everyItem().getElements(),
                    styles: { paragraph: found.allParagraphStyles.length, character: found.allCharacterStyles.length, object: found.allObjectStyles.length, table: found.allTableStyles.length, cell: found.allCellStyles.length },
                    preflightOptions: {
                        ...Record.fromIterableWith(Struct.keys(DocumentView.fields.preflightOptions.fields), (key) => [key, preflight[key]]),
                        preflightWorkingProfile: Predicate.isString(preflight.preflightWorkingProfile) ? preflight.preflightWorkingProfile : preflight.preflightWorkingProfile.name,
                    },
                    activeProcess: preflight.preflightOff ? Option.none() : Option.some(found.activeProcess),
                    textDefaults: ParagraphAttributes.of(found.textDefaults),
                };
            }),
        );
    static readonly viewed = (model: typeof Model.Service) => flow(Option.liftPredicate(model.document), Option.map(DocumentView.of));
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { DocumentView, Extent, LayerView, LinkView, PageView, SpreadView, SwatchView };
