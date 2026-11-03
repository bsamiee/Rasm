import type { Document, Gradient, Layer, Link, MasterSpread, MixedInk, MixedInkGroup, Page, PageItem, PlainColor, PlainSwatch, Spread, Tint } from 'adobe:indesign';
import { HostError, type Open } from '@rasm/automation/requests';
import { Array, Effect, Match, Number, Option, Path, Predicate, Record, Result, Struct } from 'effect';
import { DocumentView, Extent, LayerView, LinkView, PageView, SpreadView, SwatchView } from '#/contract';
import { HostSettings } from '#/documents/settings';
import { paragraphAttributes } from '#/documents/text';
import { Model } from '#/dom/model';
import { Value } from '#/dom/value';

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [GEOMETRY]

const extent = Effect.fn('extent')(function* (...placed: readonly (Page | PageItem)[]) {
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

// --- [VIEWS]

const layerView = Model.view(
    (model) => model.layer,
    (layer: Layer) => Model.read(LayerView, layer.toSpecifier(), () => ({ ...Value.picked(LayerView.fields)(layer), ...Model.referenceUnsafe(layer), _tag: 'Layer', items: layer.pageItems.length })),
);
const linkView = Model.view(
    (model) => model.link,
    (link: Link) => Model.read(LinkView, link.toSpecifier(), () => ({ ...Value.picked(LinkView.fields)(link), ...Model.referenceUnsafe(link), _tag: 'Link' })),
);
const swatchView = Model.view(
    (model) => model.swatch,
    (swatch: PlainSwatch | PlainColor | Tint | Gradient | MixedInk | MixedInkGroup) => {
        const { cases } = SwatchView.fields.value;
        return Model.read(SwatchView, swatch.toSpecifier(), () => ({
            _tag: 'Swatch',
            ...Model.referenceUnsafe(swatch),
            group: Option.map(Option.fromNullishOr(swatch.parentColorGroup), Struct.get('name')),
            value: Match.value(swatch).pipe(
                Match.when({ constructorName: 'Color' }, (color) => ({ ...Value.picked(cases.Color.fields)(color), _tag: 'Color' })),
                Match.when({ constructorName: 'Tint' }, (tint) => ({ ...Value.picked(cases.Tint.fields)(tint), _tag: 'Tint' })),
                Match.when({ constructorName: 'Gradient' }, (gradient) => ({ ...Value.picked(cases.Gradient.fields)(gradient), _tag: 'Gradient', gradientStops: Array.map(gradient.gradientStops.everyItem().getElements(), Value.picked(cases.Gradient.fields.gradientStops.value.fields)) })),
                Match.when({ constructorName: 'MixedInk' }, (ink) => ({ ...Value.picked(cases.MixedInk.fields)(ink), _tag: 'MixedInk' })),
                Match.orElse(() => ({ _tag: 'Swatch' })),
            ),
        }));
    },
);
const pageView = Model.view(
    (model) => model.page,
    (page: Page) =>
        Effect.flatMap(extent(page), (bounds) =>
            Model.read(PageView, page.toSpecifier(), () => ({
                _tag: 'Page',
                ...Model.referenceUnsafe(page),
                ...Struct.pick(page, ['documentOffset', 'side', 'parent', 'appliedSection']),
                extent: bounds,
                appliedMaster: Option.fromNullishOr(page.appliedMaster),
                margins: { ...Value.picked(PageView.fields.margins.fields)(page.marginPreferences), columns: Array.chunksOf(page.marginPreferences.columnsPositions, 2) },
                items: { own: page.pageItems.length, inherited: page.masterPageItems.length },
            })),
        ),
);
const spreadView = Model.view(
    (model) => Predicate.or(model.spread, model.masterSpread),
    Effect.fnUntraced(function* (spread: Spread | MasterSpread) {
        const pages = yield* Effect.forEach(spread.pages.everyItem().getElements(), (page) => Effect.map(extent(page), (bounds) => ({ page, bounds })));
        return yield* Model.read(SpreadView, spread.toSpecifier(), () => ({
            _tag: 'Spread',
            ...Model.referenceUnsafe(spread),
            index: spread.index,
            showMasterItems: spread.showMasterItems,
            appliedMaster: Option.fromNullishOr(spread.appliedMaster),
            kind: spread.constructorName === 'Spread' ? { _tag: 'Spread', allowPageShuffle: spread.allowPageShuffle } : { _tag: 'MasterSpread', namePrefix: spread.namePrefix, baseName: spread.baseName },
            pages: Array.map(pages, ({ page, bounds }) => ({ ...Model.referenceUnsafe(page), side: page.side, extent: bounds })),
            items: spread.pageItems.length,
        }));
    }),
);
const overset = ({ stories }: Document): readonly string[] => Array.filterMap(Array.zip(stories.everyItem().overflows, stories.everyItem().id), ([overflows, id]) => (overflows === true ? Result.succeed(stories.itemByID(id).toSpecifier()) : Result.failVoid));
const summary = Effect.fn('summary')(function* (document: Document) {
    const [own, saved] = yield* Effect.all([Model.reference(document), Effect.try({ try: () => document.saved, catch: HostError.refused(`saved of ${document.toSpecifier()}`) })]);
    return {
        ...own,
        saved,
        modified: document.modified,
        path: saved ? yield* Model.file(document, 'fullName') : Option.none(),
        windows: Array.map(document.windows.everyItem().getElements(), (window) => window.toSpecifier()),
    };
});
const documentView = Model.view(
    (model) => model.document,
    Effect.fnUntraced(function* (document: Document) {
        const [summarized, settings] = yield* Effect.all([summary(document), HostSettings.read(document.textDefaults)]);
        return yield* Model.read(DocumentView, document.toSpecifier(), () => {
            const { documentPreferences: preferences, viewPreferences: view, preflightOptions: preflight } = document;
            return {
                _tag: 'Document',
                ...summarized,
                intent: preferences.intent,
                facingPages: preferences.facingPages,
                size: { width: preferences.pageWidth, height: preferences.pageHeight },
                units: { horizontal: view.horizontalMeasurementUnits, vertical: view.verticalMeasurementUnits, origin: view.rulerOrigin },
                pages: document.pages.length,
                spreads: document.spreads.length,
                parents: Array.map(document.masterSpreads.everyItem().getElements(), (each) => ({ ...Model.referenceUnsafe(each), pages: each.pages.length })),
                sections: Array.map(document.sections.everyItem().getElements(), (each) => ({ ...Model.referenceUnsafe(each), start: each.pageStart.toSpecifier(), length: each.length, pageNumberStart: each.pageNumberStart, continueNumbering: each.continueNumbering })),
                layers: Array.map(document.layers.everyItem().getElements(), (each) => ({ ...Model.referenceUnsafe(each), visible: each.visible, locked: each.locked, printable: each.printable })),
                stories: { count: document.stories.length, overset: overset(document) },
                links: Record.map(Array.groupBy(document.links.everyItem().status, String), Array.length),
                fonts: Array.zipWith(document.fonts.everyItem().name, document.fonts.everyItem().status, (name, status) => ({ name, status })),
                swatches: document.swatches.everyItem().getElements(),
                styles: { paragraph: document.allParagraphStyles.length, character: document.allCharacterStyles.length, object: document.allObjectStyles.length, table: document.allTableStyles.length, cell: document.allCellStyles.length },
                preflightOptions: { ...Value.picked(DocumentView.fields.preflightOptions.fields)(preflight), preflightWorkingProfile: Predicate.isString(preflight.preflightWorkingProfile) ? preflight.preflightWorkingProfile : preflight.preflightWorkingProfile.name },
                activeProcess: preflight.preflightOff ? Option.none() : Option.some(document.activeProcess),
                textDefaults: paragraphAttributes(document.textDefaults, settings),
            };
        });
    }),
);

// --- [HANDLERS]

const open = Effect.fnUntraced(function* (payload: Open) {
    const [{ app }, path] = yield* Effect.all([Model, Path.Path]);
    const native = yield* path.fromFileUrl(payload.file);
    return yield* Model.reference(yield* Effect.try({ try: () => app.open(native, false), catch: HostError.refused(payload.label) }));
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { documentView, extent, layerView, linkView, open, overset, pageView, spreadView, summary, swatchView };
