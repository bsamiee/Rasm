import type { Document, Layer, MasterSpread, Page, PageItem, Spread } from 'adobe:indesign';
import { Array, Cause, Effect, flow, Number, Option, Predicate, Record, Schema, Struct } from 'effect';
import { Model } from './model.ts';
import { Reference, read, Specifier } from './references.ts';
import { Composition } from './text.ts';

// --- [MODELS] --------------------------------------------------------------------------

const Box = Schema.Struct(Struct.Record(['top', 'left', 'bottom', 'right'], Schema.Finite)).annotate({ description: 'Outer stroke box in spread points, x 0 at the spine of a facing-pages spread and at the center of any other spread' });
class LayerView extends Schema.TaggedClass<LayerView, { readonly brand: unique symbol }>()('Layer', {
    ...Reference.fields,
    index: Schema.Int,
    ...Struct.Record(['visible', 'locked', 'printable', 'ignoreWrap'], Schema.Boolean),
    layerColor: Schema.Json.annotate({ description: 'UIColors member or RGB values' }),
    items: Schema.Int,
}) {
    static readonly viewed = ({ owning }: Model['Service']) => flow(Option.liftPredicate(owning<Layer>('layerColor')), Option.map(LayerView.of));
    static readonly of = (layer: Layer) => read(LayerView, layer.toSpecifier(), () => ({ _tag: 'Layer', ...Reference.of(layer), index: layer.index, visible: layer.visible, locked: layer.locked, printable: layer.printable, ignoreWrap: layer.ignoreWrap, layerColor: layer.layerColor, items: layer.pageItems.length }));
}
class DocumentView extends Schema.TaggedClass<DocumentView, { readonly brand: unique symbol }>()('Document', {
    ...Reference.fields,
    saved: Schema.Boolean,
    modified: Schema.Boolean,
    path: Schema.OptionFromOptionalKey(Schema.String),
    windows: Schema.Array(Specifier),
    intent: Schema.String,
    facingPages: Schema.Boolean,
    size: Schema.Struct({ width: Schema.Finite, height: Schema.Finite }).annotate({ description: 'Default page size in points' }),
    units: Schema.Struct({ horizontal: Schema.String, vertical: Schema.String, origin: Schema.String }).annotate({ description: 'Ruler units and origin the user sees; values this library returns stay in points' }),
    pages: Schema.Int,
    spreads: Schema.Int,
    parents: Schema.Array(Reference.mapFields(Struct.assign({ pages: Schema.Int }))).annotate({ description: 'Parent (master) spreads' }),
    sections: Schema.Array(Reference.mapFields(Struct.assign({ start: Specifier, length: Schema.Int, pageNumberStart: Schema.Int, continueNumbering: Schema.Boolean }))),
    layers: Schema.Array(Reference.mapFields(Struct.assign(Struct.pick(LayerView.fields, ['visible', 'locked', 'printable'])))),
    stories: Schema.Struct({ count: Schema.Int, overset: Schema.Array(Specifier) }),
    links: Schema.Record(Schema.String, Schema.Int).annotate({ description: 'Link count per LinkStatus member' }),
    fonts: Schema.Array(Schema.Struct({ name: Schema.String, status: Schema.String })).annotate({ description: 'Fonts the document uses with their FontStatus' }),
    swatches: Schema.Array(Reference),
    styles: Schema.Struct(Struct.Record(['paragraph', 'character', 'object', 'table', 'cell'], Schema.Int)).annotate({ description: 'Style counts, grouped styles included' }),
    preflightOptions: Schema.Struct({
        ...Struct.Record(['preflightOff', 'preflightIncludeObjectsOnPasteboard', 'preflightIncludeNonprintingObjects'], Schema.Boolean),
        ...Struct.Record(['preflightProfilePolicy', 'preflightWorkingProfile', 'preflightScope', 'preflightWhichLayers'], Schema.String),
    }),
    activeProcess: Schema.OptionFromOptionalKey(Reference).annotate({ description: 'Live preflight process, absent while live preflight is off' }),
    defaults: Composition.annotate({ description: 'Composition rules of the document text defaults: tab stops, GREP, nested, and line styles, bullets and numbering' }),
}) {
    static readonly Row = Schema.Struct(Struct.pick(DocumentView.fields, ['type', 'specifier', 'name', 'valid', 'saved', 'modified', 'path', 'windows']));
    static readonly viewed = ({ document }: Model['Service']) => flow(Option.liftPredicate(document), Option.map(DocumentView.of));
    static readonly overset = ({ stories }: Document): readonly string[] => Array.flatMap(Array.zip(stories.everyItem().overflows, stories.everyItem().id), ([overflows, id]) => (overflows === true ? [stories.itemByID(id).toSpecifier()] : []));
    static readonly row = Effect.fn('DocumentView.row')(function* (document: Document) {
        const { file } = yield* Model;
        const { saved } = document;
        const path = yield* Effect.when(file(document, 'fullName'), Effect.succeed(saved));
        return { ...Reference.of(document), saved, modified: document.modified, path: Option.flatten(path), windows: Array.map(document.windows.everyItem().getElements(), (window) => window.toSpecifier()) };
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
                    defaults: Composition.of(found.textDefaults),
                };
            }),
        );
}
class PageView extends Schema.TaggedClass<PageView, { readonly brand: unique symbol }>()('Page', {
    ...Reference.fields,
    documentOffset: Schema.Int,
    side: Schema.String,
    extent: Box,
    parent: Reference.annotate({ description: 'Spread or parent spread holding the page' }),
    appliedMaster: Schema.OptionFromOptionalKey(Reference),
    appliedSection: Reference,
    margins: Schema.Struct({
        ...Struct.Record(['top', 'bottom', 'inside', 'outside'], Schema.Finite),
        columns: Schema.Array(Schema.Tuple([Schema.Finite, Schema.Finite])).annotate({ description: 'Left and right edge of each column from the left margin' }),
    }),
    items: Schema.Struct({ own: Schema.Int, inherited: Schema.Int }).annotate({ description: 'Page items held, and parent items shown' }),
}) {
    static readonly viewed = ({ page }: Model['Service']) => flow(Option.liftPredicate(page), Option.map(PageView.of));
    static readonly of = (page: Page) =>
        Effect.flatMap(extent(page), (box) =>
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
}
class SpreadView extends Schema.TaggedClass<SpreadView, { readonly brand: unique symbol }>()('Spread', {
    ...Reference.fields,
    index: Schema.Int,
    showMasterItems: Schema.Boolean,
    appliedMaster: Schema.OptionFromOptionalKey(Reference).annotate({ description: 'Parent applied to a spread, or the parent a parent is based on' }),
    kind: Schema.TaggedUnion({ Spread: { allowPageShuffle: Schema.Boolean }, MasterSpread: { namePrefix: Schema.String, baseName: Schema.String } }),
    pages: Schema.Array(Reference.mapFields(Struct.assign(Struct.pick(PageView.fields, ['side', 'extent'])))).annotate({ description: 'Pages in order' }),
    items: Schema.Int,
}) {
    static readonly viewed = ({ owning, spread }: Model['Service']) => flow(Option.liftPredicate(Predicate.or(spread, owning<MasterSpread>('namePrefix'))), Option.map(SpreadView.of));
    static readonly of = (found: Spread | MasterSpread) =>
        Effect.flatMap(
            Effect.forEach(found.pages.everyItem().getElements(), (page) => Effect.map(extent(page), (box) => ({ ...Reference.of(page), side: page.side, extent: box }))),
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
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const extent = Effect.fn('extent')(function* (...placed: readonly (Page | PageItem)[]) {
    const { AnchorPoint, BoundingBoxLimits, CoordinateSpaces } = yield* Model;
    return yield* Effect.try({
        try: () => {
            const corners: ReadonlyArray<readonly [number, number]> = Array.flatMap(Array.cartesian(placed, [AnchorPoint.TOP_LEFT_ANCHOR, AnchorPoint.BOTTOM_RIGHT_ANCHOR]), ([each, anchor]) =>
                each.resolve([anchor, BoundingBoxLimits.OUTER_STROKE_BOUNDS, CoordinateSpaces.SPREAD_COORDINATES], CoordinateSpaces.SPREAD_COORDINATES),
            );
            return Struct.makeReducer<typeof Box.Type>({ top: Number.ReducerMin, left: Number.ReducerMin, bottom: Number.ReducerMax, right: Number.ReducerMax }).combineAll(Array.map(corners, ([x, y]) => ({ top: y, left: x, bottom: y, right: x })));
        },
        catch: (cause) => new Cause.UnknownError(cause, `extent of ${Array.map(placed, (each) => each.toSpecifier()).join(', ')}`),
    });
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { DocumentView, extent, LayerView, PageView, SpreadView };
