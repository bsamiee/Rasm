import fs from 'adobe:fs';
import type { BaseCollection, EndnoteTextFrame, MasterSpread, Page, PageItem, Spread, TextFrame } from 'adobe:indesign';
import indesign from 'adobe:indesign';
import { fitted } from '@rasm/ai/vision';
import { Array, Effect, Match, Number, Option, Schema, Struct } from 'effect';
import type { Rpc } from 'effect/rpc';
import { JPG } from 'image-size/types/jpg';
import { type Box, GalleyError, Inspect, type Render, Shot } from './contract.ts';
import { active, type Host, host, missing, read, resolve, scope, Temporary } from './dom.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

const moved = (box: typeof Box.Type, x: number, y: number): typeof Box.Type => Struct.evolve(box, { top: Number.sum(y), left: Number.sum(x), bottom: Number.sum(y), right: Number.sum(x) });
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
const ruled = (page: Page, box: typeof Box.Type): typeof Box.Type => Array.reduce(page.parent.resolve([[0, 0], page.index], indesign.CoordinateSpaces.SPREAD_COORDINATES, true), box, (at, [x, y]) => moved(at, -x, -y));
const holder = ({ parent }: PageItem): Option.Option<PageItem> =>
    Match.value(parent).pipe(
        Match.discriminators('constructorName')({
            MasterSpread: () => Option.none(),
            Spread: () => Option.none(),
            XMLElement: () => Option.none(),
            Snippet: () => Option.none(),
            PlaceGun: () => Option.none(),
            Character: (character) => Option.map(Array.head(character.parentTextFrames), (frame) => (frame.constructorName === 'TextPath' ? frame.parent : frame)),
            Cell: (cell) => Option.liftPredicate(cell.parent.parent, (frame): frame is TextFrame | EndnoteTextFrame => frame.constructorName === 'TextFrame' || frame.constructorName === 'EndnoteTextFrame'),
            State: (state) => Option.some(state.parent),
        }),
        Match.orElse(Option.some<PageItem>),
    );
const geometry = Effect.fnUntraced(function* (found: Rpc.Success<typeof Inspect>) {
    const kinds = ['Page', 'Spread', 'MasterSpread'];
    const laid = (value: unknown): value is Page | Spread | MasterSpread => host(value) && kinds.includes(value.constructorName);
    const measured = (target: Page | Spread | MasterSpread): unknown => {
        const [pages, first, { documentPreferences: setup, gridPreferences: grid }] = target.constructorName === 'Page' ? ([[target], target, target.parent.parent] as const) : ([target.pages.everyItem().getElements(), target.pages.firstItem(), target.parent] as const);
        return {
            bleed: { top: setup.documentBleedTopOffset, bottom: setup.documentBleedBottomOffset, inside: setup.documentBleedInsideOrLeftOffset, outside: setup.documentBleedOutsideOrRightOffset },
            baseline: { start: grid.baselineStart, division: grid.baselineDivision, relativeTo: grid.baselineGridRelativeOption },
            pages: Array.map(pages, (page) => {
                const [margins, bounds] = [page.marginPreferences, ruled(page, extent(page))];
                const [near, far] = page.side.equals(indesign.PageSideOptions.LEFT_HAND) ? [margins.right, margins.left] : [margins.left, margins.right];
                const content = { top: bounds.top + margins.top, left: bounds.left + near, bottom: bounds.bottom - margins.bottom, right: bounds.right - far };
                return {
                    specifier: page,
                    master: page.appliedMaster ?? undefined,
                    bounds,
                    margins: { top: margins.top, bottom: margins.bottom, inside: margins.left, outside: margins.right },
                    content,
                    columns: Array.filter(
                        Array.zipWith(margins.columnsPositions, Array.drop(margins.columnsPositions, 1), (left, right) => ({ left: content.left + left, right: content.left + right })),
                        (_, index) => index % 2 === 0,
                    ),
                };
            }),
            items: Array.map(target.allPageItems, (item) => ({
                type: item.constructorName,
                specifier: item,
                name: item.name,
                layer: item.itemLayer.name,
                appliedObjectStyle: item.appliedObjectStyle.name,
                locked: item.locked || undefined,
                anchored: item.parent.constructorName === 'Character' || undefined,
                parent: Option.getOrUndefined(holder(item)),
                story: 'parentStory' in item ? item.parentStory : undefined,
                label: item.label || undefined,
                visible: item.visible ? undefined : false,
                bounds: ruled(item.parentPage ?? first, extent(item)),
                overflows: Option.getOrUndefined(Schema.decodeUnknownOption(Schema.Boolean)(Reflect.get(item, 'overflows'))),
                effectivePpi: Option.getOrUndefined(Schema.decodeUnknownOption(Schema.Tuple([Schema.Finite, Schema.Finite]))(Reflect.get(item, 'effectivePpi'))),
            })),
        };
    };
    const placed = (target: unknown): Effect.Effect<(typeof Inspect.successSchema.cases.Object.fields.geometry)['Type'], GalleyError> =>
        Effect.map(
            read(Inspect.successSchema.cases.Object.mapFields(Struct.pick(['geometry'])), () => ({ geometry: laid(target) ? measured(target) : undefined })),
            Struct.get('geometry'),
        );
    return yield* Match.valueTags(found, {
        Object: (object) => Effect.map(Effect.flatMap(Effect.andThen(scope, resolve(object.specifier)), placed), (shown) => ({ ...object, geometry: shown })),
        Items: (listed) =>
            Array.some(listed.items, ({ type }) => kinds.includes(type))
                ? Effect.map(
                      Effect.flatMap(Effect.andThen(scope, resolve(listed.specifier)), (members) => Effect.forEach(Array.ensure(members), placed)),
                      (measures) => ({ ...listed, items: Array.zipWith(listed.items, measures, (item, shown) => ({ ...item, geometry: shown })) }),
                  )
                : Effect.succeed(listed),
        State: Effect.succeed,
        Class: Effect.succeed,
        Enumeration: Effect.succeed,
    });
}, Effect.scoped);
const planned = Effect.fnUntraced(function* ({ target, tokens }: Rpc.Payload<typeof Render>) {
    const inch = 72;
    const [side, name] = [1, 'render.jpg'];
    const [native, local] = [`${yield* Temporary}${name}`, `plugin-temp:/${name}`];
    yield* scope;
    const indexed = <T>(items: Pick<BaseCollection<T>, 'length' | 'item'>, index: number, kind: string): Effect.Effect<T, GalleyError> =>
        index < items.length ? Effect.succeed(items.item(index)) : Effect.fail(new GalleyError({ reason: { _tag: 'Missing', subject: `${kind} ${index}`, query: `${index}`, candidates: [], range: Option.some({ first: 0, last: items.length - 1 }) } }));
    const placed = (item: PageItem): Option.Option<readonly [Spread | MasterSpread, PageItem]> => (item.parent.constructorName === 'Spread' || item.parent.constructorName === 'MasterSpread' ? Option.some([item.parent, item]) : Option.flatMap(holder(item), placed));
    const framed = (value: Host | readonly Host[]): value is Host & PageItem => host(value) && 'itemLayer' in value;
    const touches = (region: typeof Box.Type, item: PageItem): boolean =>
        Array.some([item, ...item.allPageItems], (part) => {
            const box = extent(part);
            return box.left < region.right && box.right > region.left && box.top < region.bottom && box.bottom > region.top;
        });
    const sized = (box: typeof Box.Type): typeof Box.Type => ({ ...box, bottom: Math.max(box.bottom, box.top + side), right: Math.max(box.right, box.left + side) });
    const resolution = (region: typeof Box.Type, ...caps: readonly number[]): number => {
        const sides = [region.right - region.left, region.bottom - region.top] as const;
        const fit = fitted(tokens, ...sides);
        return Number.clamp(Math.min((Math.max(fit.width, fit.height) * inch) / Math.max(...sides), ...caps), { minimum: 1, maximum: 2400 });
    };
    const { subject, source, dpi, range, spreading, composed, ...described } = yield* Match.valueTags(target, {
        Page: ({ index, region: part }) =>
            Effect.gen(function* () {
                const document = yield* active;
                const page = yield* indexed(document.pages, index, 'page');
                const whole = extent(page);
                const [down, across] = [(at: number): number => whole.top + at * (whole.bottom - whole.top), (at: number): number => whole.left + at * (whole.right - whole.left)];
                const region = Option.match(part, { onNone: () => whole, onSome: (fraction) => sized(Struct.evolve(fraction, { top: down, left: across, bottom: down, right: across })) });
                return { subject: `page ${index}`, source: Option.isSome(part) ? page.parent : document, dpi: resolution(region), range: `+${index + 1}`, spreading: false, composed: Option.map(part, () => region), bounds: ruled(page, region) };
            }),
        Spread: ({ index }) =>
            Effect.gen(function* () {
                const document = yield* active;
                const spread = yield* indexed(document.spreads, index, 'spread');
                const [page, region] = [spread.pages.firstItem(), extent(...spread.pages.everyItem().getElements())];
                return { subject: `spread ${index}`, source: document, dpi: resolution(region), range: `+${page.documentOffset + 1}`, spreading: true, composed: Option.none(), bounds: ruled(page, region) };
            }),
        Item: ({ specifier, isolate }) =>
            Effect.gen(function* () {
                const item = yield* Effect.filterOrFail(resolve(specifier), framed, () => missing(`page item ${specifier}`, specifier, []));
                const [spread, top] = yield* Effect.fromOption(placed(item), () => missing(`spread of ${specifier}`, specifier, []));
                const page = item.parentPage ?? spread.pages.firstItem();
                const [region, stacked]: readonly [typeof Box.Type, readonly PageItem[]] = [sized(extent(item)), spread.pageItems.everyItem().getElements()];
                const [ahead, behind] = Array.span(stacked, (other) => other.id !== top.id);
                const overlaps = Array.filter([...Array.map(ahead, (other) => ({ other, front: true })), ...Array.map(Array.drop(behind, 1), (other) => ({ other, front: false }))], ({ other }) => other.itemLayer.visible && other.itemLayer.printable && other.visible && !other.nonprinting && touches(region, other));
                const ppi = Option.map(
                    Array.findFirst([item, ...item.allGraphics], (each) => Schema.decodeUnknownOption(Schema.NonEmptyArray(Schema.Finite))(Reflect.get(each, 'effectivePpi'))),
                    (pair) => Math.max(...pair),
                );
                const limited = resolution(region, ...Option.toArray(Option.filter(ppi, () => isolate || overlaps.length === 0)));
                return {
                    subject: specifier,
                    source: isolate ? item : spread,
                    dpi: limited,
                    range: `+${page.documentOffset + 1}`,
                    spreading: false,
                    composed: Option.liftPredicate(region, () => !isolate),
                    bounds: ruled(page, region),
                    item: { type: item.constructorName, effectivePpi: Option.getOrUndefined(ppi), capped: limited < resolution(region), overlaps: Array.map(overlaps, ({ other, front }) => ({ type: other.constructorName, specifier: other, name: other.name, front })) },
                };
            }),
    });
    yield* Effect.sync(() => fs.writeFileSync(local, ''));
    return {
        program: 'Render' as const,
        args: [native, source.toSpecifier(), dpi, range, spreading, Option.getOrNull(composed)],
        refusals: [],
        notes: [],
        settle: Effect.fnUntraced(function* (result: unknown) {
            const omitted = yield* Effect.orDie(Schema.decodeUnknownEffect(Schema.Array(Schema.Tuple([Schema.String, Schema.String, Schema.String, Schema.String, Schema.String])))(result));
            const image = yield* Effect.filterOrFail(
                Effect.sync(() => new Uint8Array(fs.readFileSync(local))),
                (bytes) => bytes.length > 0,
                () => missing(`image of ${subject}`, subject, []),
            );
            const { width, height } = JPG.calculate(image);
            const shown = yield* read(Shot.mapFields(Struct.omit(['image'])), () => ({
                ...described,
                width,
                height,
                omitted: Array.match(omitted, { onEmpty: () => undefined, onNonEmpty: Array.map(([specifier, type, named, error, message]) => ({ type, specifier, name: named, cause: { name: error, message } })) }),
            }));
            return { image, ...shown };
        }),
    };
}, Effect.scoped);

// --- [EXPORTS] -------------------------------------------------------------------------

export { geometry, planned };
