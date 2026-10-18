import fs from 'adobe:fs';
import indesign, { type BaseCollection, type EndnoteTextFrame, type MasterSpread, type Page, type PageItem, type PageSideOptions, type Spread, type TextFrame } from 'adobe:indesign';
import { storage } from 'adobe:uxp';
import { Unmatched } from '@rasm/mcp/server';
import { Array, Context, Effect, Layer, Match, Number, Option, Result, Schema, Struct } from 'effect';
import { JPG } from 'image-size/types/jpg';
import { type Box, InDesignError, Specifier, Target } from './contract.ts';
import { active, owning, resolve, transacted } from './dom.ts';

// --- [SERVICES] ------------------------------------------------------------------------

class Scratch extends Context.Service<Scratch>()('@rasm/indesign/render/Scratch', {
    make: Effect.flatMap(
        Effect.promise(() => storage.localFileSystem.getTemporaryFolder()),
        (folder) => Effect.promise(() => folder.createFile('render.jpg', { overwrite: true })),
    ),
}) {
    static readonly layer = Layer.effect(this, this.make);
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [GEOMETRY]
const extent = (...located: readonly Pick<Page | PageItem, 'parent' | 'resolve'>[]): typeof Box.Type => {
    const { AnchorPoint, BoundingBoxLimits, CoordinateSpaces } = indesign;
    return Struct.makeReducer<typeof Box.Type>({ top: Number.ReducerMin, left: Number.ReducerMin, bottom: Number.ReducerMax, right: Number.ReducerMax }).combineAll(
        Array.flatMap(located, (each) => {
            if (each.parent.constructorName === 'Character') {
                each.parent.parentStory.recompose();
            }
            return Array.flatMap([AnchorPoint.TOP_LEFT_ANCHOR, AnchorPoint.BOTTOM_RIGHT_ANCHOR], (anchor) => Array.map(each.resolve([anchor, BoundingBoxLimits.OUTER_STROKE_BOUNDS, CoordinateSpaces.SPREAD_COORDINATES], CoordinateSpaces.SPREAD_COORDINATES), ([x, y]) => ({ top: y, left: x, bottom: y, right: x })));
        }),
    );
};
const ruled = (page: Page, box: typeof Box.Type): typeof Box.Type => page.parent.resolve([[0, 0], page.index], indesign.CoordinateSpaces.SPREAD_COORDINATES, true).reduce((at, [x, y]) => Struct.evolve(at, { top: Number.subtract(y), left: Number.subtract(x), bottom: Number.subtract(y), right: Number.subtract(x) }), box);
const holder = ({ parent }: PageItem): Option.Option<PageItem> =>
    Match.value(parent).pipe(
        Match.discriminator('constructorName')('MasterSpread', 'Spread', 'XMLElement', 'Snippet', 'PlaceGun', () => Option.none()),
        Match.discriminators('constructorName')({
            Character: (character) => Option.map(Array.head(character.parentTextFrames), (frame) => (frame.constructorName === 'TextPath' ? frame.parent : frame)),
            Cell: (cell) => Option.liftPredicate(cell.parent.parent, (frame): frame is TextFrame | EndnoteTextFrame => frame.constructorName === 'TextFrame' || frame.constructorName === 'EndnoteTextFrame'),
            State: (state) => Option.some(state.parent),
        }),
        Match.orElse(Option.some<PageItem>),
    );
const shown = (item: PageItem): boolean => item.itemLayer.visible && item.itemLayer.printable && item.visible && !item.nonprinting;
const touches = (region: typeof Box.Type, item: PageItem): boolean =>
    Array.some([item, ...item.allPageItems], (part) => {
        const box = extent(part);
        return box.left < region.right && box.right > region.left && box.top < region.bottom && box.bottom > region.top;
    });

// --- [RENDERS]
const reference = (item: PageItem) => ({ type: item.constructorName, specifier: Specifier.make(item.toSpecifier()), name: Option.some(item.name) });
const composited = (region: typeof Box.Type, spread: Spread | MasterSpread) => {
    const { app, PageBindingOptions } = indesign;
    const contained = owning<PageItem>('allPageItems');
    const [pages, stacked]: readonly [readonly Page[], readonly PageItem[]] = [spread.pages.everyItem().getElements(), spread.pageItems.everyItem().getElements()];
    const leading = spread.parent.documentPreferences.pageBinding.equals(PageBindingOptions.RIGHT_TO_LEFT) ? indesign.PageSideOptions.RIGHT_HAND : indesign.PageSideOptions.LEFT_HAND;
    const spine = (sides: readonly PageSideOptions[]): number => sides.filter((each) => each.equals(leading)).length;
    const offset = (page: Page, master: MasterSpread): typeof Box.Type => {
        const [own, base] = [extent(page), extent(master.pages.item(Number.clamp(page.index - spine(spread.pages.everyItem().side) + spine(master.pages.everyItem().side), { minimum: 0, maximum: master.pages.length - 1 })))];
        return Struct.evolve(region, { top: Number.sum(base.top - own.top), left: Number.sum(base.left - own.left), bottom: Number.sum(base.top - own.top), right: Number.sum(base.left - own.left) });
    };
    const mastered = Array.flatMap(Array.reverse(pages), (page) => Array.map(Option.toArray(Option.fromNullishOr(page.appliedMaster)), (master) => ({ page, frame: offset(page, master) })));
    const masters = Array.flatMap(mastered, ({ page, frame }) => Array.flatMap(Array.reverse(page.masterPageItems), (each) => (contained(each) ? [{ each, frame }] : [])));
    const layered: readonly { readonly each: PageItem; readonly frame: typeof Box.Type }[] = [...masters, ...Array.map(Array.reverse(stacked), (each) => ({ each, frame: region }))];
    const chosen = Array.dedupeWith(layered, (left, right) => left.each.id === right.each.id).filter(({ each, frame }) => shown(each) && touches(frame, each));
    const scratch = app.documents.add(false, undefined, { documentPreferences: { facingPages: false, pageWidth: region.right - region.left, pageHeight: region.bottom - region.top } });
    Array.range(1, spread.parent.layers.length - 1).forEach(() => {
        scratch.layers.add();
    });
    const corner = extent(scratch.pages.item(0));
    const copied = ({ each, frame }: (typeof chosen)[number]) => Result.try(() => each.duplicate(scratch.layers.item(each.itemLayer.index), [corner.left - frame.left, corner.top - frame.top]));
    return { output: scratch, omitted: Array.flatMap(chosen, (row) => Option.toArray(Option.map(Result.getFailure(copied(row)), (cause) => ({ ...reference(row.each), cause })))) };
};
const render = Effect.fn('render')(function* (target: typeof Target.Type) {
    const { app, ExportFormat, ExportRangeOrAllPages, JpegColorSpaceEnum } = indesign;
    const resolution = app.jpegExportPreferences.exportResolution;
    const framed = owning<PageItem>('itemLayer');
    const indexed = <T>(items: Pick<BaseCollection<T>, 'length' | 'item'>, index: number, kind: string): Effect.Effect<T, Unmatched> =>
        index < items.length ? Effect.succeed(items.item(index)) : Effect.fail(Unmatched.make({ subject: `${kind} among indexes 0 to ${items.length - 1}`, query: `${index}`, candidates: [] }));
    const placed = (child: PageItem): Option.Option<readonly [Spread | MasterSpread, PageItem]> => (child.parent.constructorName === 'Spread' || child.parent.constructorName === 'MasterSpread' ? Option.some([child.parent, child]) : Option.flatMap(holder(child), placed));
    const sized = (box: typeof Box.Type): typeof Box.Type => ({ ...box, bottom: Math.max(box.bottom, box.top + 1), right: Math.max(box.right, box.left + 1) });
    const plan = yield* Match.valueTags(target, {
        Page: Effect.fnUntraced(function* ({ index, region: part }) {
            const page = yield* indexed((yield* active).pages, index, 'page');
            const whole = extent(page);
            const [down, across] = [(at: number): number => whole.top + at * (whole.bottom - whole.top), (at: number): number => whole.left + at * (whole.right - whole.left)];
            const region = Option.match(part, { onNone: () => whole, onSome: (fraction) => sized(Struct.evolve(fraction, { top: down, left: across, bottom: down, right: across })) });
            return { source: page.parent.parent, dpi: resolution, range: `+${page.documentOffset + 1}`, spreading: false, composed: Option.as(part, { region, spread: page.parent }), bounds: ruled(page, region), item: Option.none() };
        }),
        Spread: Effect.fnUntraced(function* ({ index }) {
            const spread = yield* indexed((yield* active).spreads, index, 'spread');
            const page = spread.pages.firstItem();
            return { source: spread.parent, dpi: resolution, range: `+${page.documentOffset + 1}`, spreading: true, composed: Option.none(), bounds: ruled(page, extent(...spread.pages.everyItem().getElements())), item: Option.none() };
        }),
        Item: Effect.fnUntraced(function* ({ specifier, isolate }) {
            const found = yield* Effect.filterOrFail(resolve(specifier), framed, () => Unmatched.make({ subject: 'page item', query: specifier, candidates: [] }));
            const [spread, top] = yield* Effect.fromOption(placed(found), () => Unmatched.make({ subject: 'spread holding the item', query: specifier, candidates: [] }));
            const page = found.parentPage ?? spread.pages.firstItem();
            const region = sized(extent(found));
            const stacked: readonly PageItem[] = spread.pageItems.everyItem().getElements();
            const [ahead, behind] = Array.span(stacked, (other) => other.id !== top.id);
            const overlaps = [...Array.map(ahead, (other) => ({ other, front: true })), ...Array.map(Array.drop(behind, 1), (other) => ({ other, front: false }))].filter(({ other }) => shown(other) && touches(region, other));
            const ppi = Option.map(
                Array.findFirst([found, ...found.allGraphics], (each) => Schema.decodeUnknownOption(Schema.NonEmptyArray(Schema.Finite))(Reflect.get(each, 'effectivePpi'))),
                (pair) => Math.max(...pair),
            );
            const capped = Math.min(resolution, ...Option.toArray(Option.filter(ppi, () => Option.isSome(isolate) || overlaps.length === 0)));
            return {
                source: Option.isSome(isolate) ? found : spread.parent,
                dpi: capped,
                range: `+${page.documentOffset + 1}`,
                spreading: false,
                composed: Option.liftPredicate({ region, spread }, () => Option.isNone(isolate)),
                bounds: ruled(page, region),
                item: Option.some({ type: found.constructorName, effectivePpi: ppi, capped: capped < resolution, overlaps: Array.map(overlaps, ({ other, front }) => ({ ...reference(other), front })) }),
            };
        }),
    });
    const { output, omitted } = Option.match(plan.composed, { onNone: () => ({ output: plan.source, omitted: [] }), onSome: ({ region, spread }) => composited(region, spread) });
    const file = yield* Scratch;
    fs.writeFileSync(file.url, '');
    Object.assign(app.jpegExportPreferences, { jpegColorSpace: JpegColorSpaceEnum.RGB, antiAlias: true, useDocumentBleeds: false, jpegExportRange: ExportRangeOrAllPages.EXPORT_RANGE, pageString: Option.isSome(plan.composed) ? '+1' : plan.range, exportingSpread: plan.spreading, exportResolution: plan.dpi });
    output.exportFile(ExportFormat.JPG, file.nativePath);
    const image = yield* Effect.filterOrFail(
        Effect.sync(() => new Uint8Array(fs.readFileSync(file.url))),
        (bytes) => bytes.length > 0,
        () => InDesignError.make({ reasons: [{ _tag: 'Unrendered', at: Target.match(target, { Page: ({ index }) => `page ${index}`, Spread: ({ index }) => `spread ${index}`, Item: ({ specifier }) => specifier }) }] }),
    );
    return { image, ...JPG.calculate(image), bounds: plan.bounds, omitted: Option.liftPredicate(omitted, Array.isArrayNonEmpty), item: plan.item };
});
const previewed = <A, E, R>({ description, preview }: { readonly description: string; readonly preview: Option.Option<true | typeof Target.Type> }, body: Effect.Effect<A, E, R>) =>
    Effect.map(transacted(description, Option.isNone(preview), Effect.all([body, Effect.transposeOption(Option.map(Option.filter(preview, Schema.is(Target)), render))])), ([result, image]) => ({ result, preview: image }));

// --- [EXPORTS] -------------------------------------------------------------------------

export { extent, holder, previewed, render, ruled, Scratch };
