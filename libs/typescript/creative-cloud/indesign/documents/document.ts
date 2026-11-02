import type { Document, Gradient, Layer, Link, MasterSpread, MixedInk, MixedInkGroup, Page, PageItem, PlainColor, PlainSwatch, Spread, Tint } from 'adobe:indesign';
import { HostError, type Open, Reference } from '@rasm/automation/requests';
import { Array, Effect, Match, Number, Option, Predicate, Record, Schema, Struct } from 'effect';
import { ParagraphAttributes } from '#/documents/text';
import { Model } from '#/dom/model';
import { reference } from '#/dom/specifier';
import { picked, read } from '#/dom/value';

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
            catch: HostError.refused(`extent of ${Array.map(placed, (each) => each.toSpecifier()).join(', ')}`),
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
    static readonly owns = (model: typeof Model.Service) => model.owning<Layer>('layerColor');
    static readonly of = (layer: Layer) => read(LayerView, layer.toSpecifier(), () => ({ ...picked(LayerView.fields)(layer), ...reference(layer), _tag: 'Layer', items: layer.pageItems.length }));
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
    static readonly owns = (model: typeof Model.Service) => model.owning<Link>('linkResourceURI');
    static readonly of = (link: Link) => read(LinkView, link.toSpecifier(), () => ({ ...picked(LinkView.fields)(link), ...reference(link), _tag: 'Link' }));
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
    static readonly owns = (model: typeof Model.Service) => model.owning<PlainSwatch | PlainColor | Tint | Gradient | MixedInk | MixedInkGroup>('parentColorGroup');
    static readonly of = (swatch: PlainSwatch | PlainColor | Tint | Gradient | MixedInk | MixedInkGroup) => {
        const { cases } = SwatchView.fields.value;
        const value = Match.value(swatch).pipe(
            Match.when({ constructorName: 'Color' }, (color) => ({ ...picked(cases.Color.fields)(color), _tag: 'Color' })),
            Match.when({ constructorName: 'Tint' }, (tint) => ({ ...picked(cases.Tint.fields)(tint), _tag: 'Tint' })),
            Match.when({ constructorName: 'Gradient' }, (gradient) => ({ ...picked(cases.Gradient.fields)(gradient), _tag: 'Gradient', gradientStops: Array.map(gradient.gradientStops.everyItem().getElements(), picked(cases.Gradient.fields.gradientStops.value.fields)) })),
            Match.when({ constructorName: 'MixedInk' }, (ink) => ({ ...picked(cases.MixedInk.fields)(ink), _tag: 'MixedInk' })),
            Match.orElse(() => ({ _tag: 'Swatch' })),
        );
        return read(SwatchView, swatch.toSpecifier(), () => ({ _tag: 'Swatch', ...reference(swatch), group: Option.map(Option.fromNullOr(swatch.parentColorGroup), Struct.get('name')), value }));
    };
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
    static readonly owns = (model: typeof Model.Service) => model.page;
    static readonly of = (page: Page) =>
        Effect.flatMap(Extent.of(page), (extent) =>
            read(PageView, page.toSpecifier(), () => {
                const { marginPreferences: margins } = page;
                return {
                    _tag: 'Page',
                    ...reference(page),
                    ...Struct.pick(page, ['documentOffset', 'side', 'parent', 'appliedSection']),
                    extent,
                    appliedMaster: Option.fromNullOr(page.appliedMaster),
                    margins: { top: margins.top, bottom: margins.bottom, inside: margins.left, outside: margins.right, columns: Array.chunksOf(margins.columnsPositions, 2) },
                    items: { own: page.pageItems.length, inherited: page.masterPageItems.length },
                };
            }),
        );
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
    static readonly owns = (model: typeof Model.Service) => Predicate.or(model.spread, model.owning<MasterSpread>('namePrefix'));
    static readonly of = (found: Spread | MasterSpread) =>
        Effect.flatMap(
            Effect.forEach(found.pages.everyItem().getElements(), (page) => Effect.map(Extent.of(page), (extent) => ({ ...reference(page), side: page.side, extent }))),
            (pages) =>
                read(SpreadView, found.toSpecifier(), () => ({
                    _tag: 'Spread',
                    ...reference(found),
                    index: found.index,
                    showMasterItems: found.showMasterItems,
                    appliedMaster: Option.fromNullOr(found.appliedMaster),
                    kind: found.constructorName === 'Spread' ? { _tag: 'Spread', allowPageShuffle: found.allowPageShuffle } : { _tag: 'MasterSpread', namePrefix: found.namePrefix, baseName: found.baseName },
                    pages,
                    items: found.pageItems.length,
                })),
        );
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
    static readonly owns = (model: typeof Model.Service) => model.document;
    static readonly overset = ({ stories }: Document): readonly string[] => Array.flatMap(Array.zip(stories.everyItem().overflows, stories.everyItem().id), ([overflows, id]) => (overflows === true ? [stories.itemByID(id).toSpecifier()] : []));
    static readonly row = Effect.fn('DocumentView.row')(function* (document: Document) {
        const { saved } = document;
        return { ...reference(document), saved, modified: document.modified, path: saved ? yield* Model.file(document, 'fullName') : Option.none(), windows: Array.map(document.windows.everyItem().getElements(), (window) => window.toSpecifier()) };
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
                    parents: Array.map(found.masterSpreads.everyItem().getElements(), (each) => ({ ...reference(each), pages: each.pages.length })),
                    sections: Array.map(found.sections.everyItem().getElements(), (each) => ({ ...reference(each), start: each.pageStart.toSpecifier(), length: each.length, pageNumberStart: each.pageNumberStart, continueNumbering: each.continueNumbering })),
                    layers: Array.map(found.layers.everyItem().getElements(), (each) => ({ ...reference(each), visible: each.visible, locked: each.locked, printable: each.printable })),
                    stories: { count: found.stories.length, overset: DocumentView.overset(found) },
                    links: Record.map(Array.groupBy(found.links.everyItem().status, String), Array.length),
                    fonts: Array.zipWith(found.fonts.everyItem().name, found.fonts.everyItem().status, (name, status) => ({ name, status })),
                    swatches: found.swatches.everyItem().getElements(),
                    styles: { paragraph: found.allParagraphStyles.length, character: found.allCharacterStyles.length, object: found.allObjectStyles.length, table: found.allTableStyles.length, cell: found.allCellStyles.length },
                    preflightOptions: { ...picked(DocumentView.fields.preflightOptions.fields)(preflight), preflightWorkingProfile: Predicate.isString(preflight.preflightWorkingProfile) ? preflight.preflightWorkingProfile : preflight.preflightWorkingProfile.name },
                    activeProcess: preflight.preflightOff ? Option.none() : Option.some(found.activeProcess),
                    textDefaults: ParagraphAttributes.of(found.textDefaults),
                };
            }),
        );
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const open = ({ file }: Open) => Model.use(({ app }) => Effect.try({ try: () => reference(app.open(decodeURIComponent(file.pathname), false)), catch: HostError.refused(`open of ${file.href}`) }));

// --- [EXPORTS] -------------------------------------------------------------------------

export { DocumentView, Extent, LayerView, LinkView, open, PageView, SpreadView, SwatchView };
