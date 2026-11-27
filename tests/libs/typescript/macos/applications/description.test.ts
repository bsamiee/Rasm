import { assert, describe, it } from '@effect/vitest';
import { Description } from '@rasm/applications/description';
import { Element } from '@rasm/applications/server/element';
import { Array, Cause, Effect, Exit, Option, Order, Record, Ref, Schema, Stream, Struct, SubscriptionRef } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const INTERNED = 2 ** 40;
const RUNS = 2000;
const LISTED = ['select', 'pen', 'hand'] as const;
const REFUSED = 'hand';
const HOLDING = ['/document[@id=1]/rectangle[@id=2]'];
const SHELF: typeof Description.Locator.Type = [['Panels', 'Application']];
const BAR: typeof Description.Locator.Type = [['MenuActions', 'Application']];
const FILE: typeof Description.Locator.Type = [...BAR, ['MenuAction', 'File Menu']];
const STRAY: typeof Description.Locator.Type = [['Windows', 'Application']];

// --- [MODELS] --------------------------------------------------------------------------

const Stated = { label: Schema.NonEmptyString, shortcut: Schema.optionalKey(Element.Shortcut), help: Schema.optionalKey(Schema.NonEmptyString) };
const Listing = {
    tools: Schema.NonEmptyArray(Schema.Struct({ id: Schema.NonEmptyString, ...Stated })),
    current: Schema.Struct({ at: Schema.Natural.annotate({ description: 'Listed tool the host reads as current, taken modulo the deduplicated listing' }), ...Stated }),
};

// --- [OPERATIONS] ----------------------------------------------------------------------

const heldRef = ({ ref }: { readonly ref: number }) => ref;
const sited = (elements: (typeof Element.Model.Type)['elements'], above: Option.Option<typeof Element.cases.container.Type>): readonly { readonly container: typeof Element.cases.container.Type; readonly above: Option.Option<typeof Element.cases.container.Type> }[] =>
    Array.filter(elements, Element.isAnyOf(['container'])).flatMap((container) => [{ container, above }, ...sited(container.children, Option.some(container))]);
const folds = Effect.fnUntraced(function* ({ model, described }: { readonly model: typeof Element.Model.Type; readonly described: Description }) {
    const asked: string[] = [];
    const intern = (locators: readonly string[]) =>
        Effect.sync(() => {
            asked.push(...locators);
            return Array.makeBy(locators.length, (index) => INTERNED + index);
        });
    const { model: composed, routes } = yield* Description.compose(model, described, intern);
    const refOf = (locator: typeof Description.Locator.Type) => INTERNED + asked.indexOf(JSON.stringify(locator.map(([role, value]) => ({ key: { identifier: { value } }, role }))));
    const levels = described.containers;
    const [read, sites] = [sited(model.elements, Option.none()), sited(composed.elements, Option.none())];
    const stands = (level: (typeof levels)[number]) => {
        const [alike, held] = [levels.filter(({ parent, kind }) => parent === undefined && kind === level.kind), read.filter(({ container }) => container.kind === level.kind)];
        return level.parent === undefined && alike.length === 1 && held.length === 1 ? Array.head(held) : Option.none();
    };
    const replaced = new Map(levels.flatMap((level) => Option.toArray(Option.map(stands(level), ({ container }) => [container, refOf(level.locator)] as const))));
    assert.deepStrictEqual(
        sites.filter(({ container }) => container.ref < INTERNED).map(({ container }) => container.ref),
        read.filter(({ container }) => !replaced.has(container)).map(({ container }) => container.ref),
        'every Accessibility container no described container stands for stays in tree order',
    );
    const found = levels.map((level) => {
        const site = sites.find(({ container }) => container.ref === refOf(level.locator));
        const held = site?.container.children ?? [];
        return {
            above: site?.above.pipe(Option.map(heldRef), Option.getOrUndefined),
            frame: site?.container.frame,
            window: site?.container.window,
            controls: Array.filter(held, Element.isAnyOf(['control'])).map(({ ref, children }) => ({ ref, options: children.map(heldRef) })),
            kept: Array.filter(held, Element.isAnyOf(['container']))
                .map(heldRef)
                .filter((ref) => ref < INTERNED),
        };
    });
    const optionRef = ({ locator }: { readonly locator: typeof Description.Locator.Type }) => refOf(locator);
    const within = new Set(levels.map(({ locator }) => refOf(locator)));
    const moved = (container: typeof Element.cases.container.Type) => replaced.get(container) ?? container.ref;
    const wanted = levels.map((level) => {
        const stood = stands(level);
        const under = Option.filter(Option.map(Option.fromUndefinedOr(level.parent), refOf), (ref) => within.has(ref));
        const held = Array.flatMap(Option.toArray(stood), ({ container }) => Array.filter(container.children, Element.isAnyOf(['container'])));
        return {
            above: Option.getOrUndefined(
                Option.orElse(
                    Option.flatMap(stood, ({ above }) => Option.map(above, moved)),
                    () => under,
                ),
            ),
            frame: Option.getOrUndefined(stood)?.container.frame,
            window: Option.getOrUndefined(stood)?.container.window,
            controls: level.controls.map(({ locator, options = [] }) => ({ ref: refOf(locator), options: options.map(optionRef) })),
            kept: held.filter((container) => !replaced.has(container)).map(heldRef),
        };
    });
    const carried = sites.flatMap(({ container }) => Array.filter(container.children, Element.isAnyOf(['control'])).flatMap(({ children }) => Array.filter(children, Element.isAnyOf(['option']))));
    const declared = Array.flatMap(levels, ({ controls }) => Array.flatMap(controls, ({ options = [] }) => options));
    assert.deepStrictEqual(
        Record.fromIterableWith(
            Array.filter(carried, ({ ref }) => ref >= INTERNED),
            (option) => [`${option.ref}`, Struct.omit(option, ['_tag', 'ref', 'children'])] as const,
        ),
        Record.fromIterableWith(declared, ({ locator, ...facts }) => [`${refOf(locator)}`, facts] as const),
        'every described option keeps the label, shortcut, and help it states',
    );
    assert.deepStrictEqual(new Set(Object.keys(routes)), new Set(asked.map((_, index) => `${INTERNED + index}`)), 'every interned locator routes back to its steps');
    assert.deepStrictEqual(
        { levels: found, tool: composed.pointer?.tool, shortcut: composed.pointer?.shortcut, help: composed.pointer?.help, holding: composed.pointer?.holding, cursor: composed.pointer?.cursor, draggingFrame: composed.pointer?.draggingFrame },
        {
            levels: wanted,
            tool: described.pointer?.tool ?? model.pointer?.tool,
            shortcut: described.pointer?.tool === undefined ? (described.pointer?.shortcut ?? model.pointer?.shortcut) : described.pointer.shortcut,
            help: described.pointer?.tool === undefined ? (described.pointer?.help ?? model.pointer?.help) : described.pointer.help,
            holding: described.pointer?.holding ?? model.pointer?.holding,
            cursor: model.pointer?.cursor,
            draggingFrame: model.pointer?.draggingFrame,
        },
    );
});
const named = (id: string) => ({ id, label: id.toUpperCase(), shortcut: { key: id.charAt(0), modifiers: ['shift'] }, help: `Click to ${id}` }) as const;
const hosted = Effect.fnUntraced(function* (script: readonly (typeof LISTED)[number][]) {
    const held = yield* SubscriptionRef.make<string>(Array.headNonEmpty(LISTED));
    const source = Description.tools({
        current: Effect.map(SubscriptionRef.get(held), named),
        listed: Effect.succeed(Array.map(LISTED, named)),
        select: (id: string) => (id === REFUSED ? Effect.void : SubscriptionRef.set(held, id)),
        changes: Stream.mapEffect(Stream.fromIterable(script), (id) => SubscriptionRef.set(held, id)),
        pointer: Effect.succeed({ holding: HOLDING }),
    });
    return { held, source };
});
const shown = (id: string) => ({ id, label: `${id} tool` });
const part = (root: typeof Description.Locator.Type, kind: Description['containers'][number]['kind'], operated: Ref.Ref<readonly string[]>, refusal: Option.Option<string>) => ({
    roots: Effect.succeed([root]),
    describe: ({ within }: { readonly within?: typeof Description.Locator.Type }) =>
        Option.match(refusal, {
            onSome: (refused) => Effect.fail(new Cause.UnknownError(refused, refused)),
            onNone: () => Effect.succeed(Description.make({ containers: [{ locator: within ?? root, kind, controls: [] }] })),
        }),
    operate: ({ locator }: { readonly locator: typeof Description.Locator.Type }) => Ref.update(operated, Array.append(`${kind} ${locator.length}`)),
});
const errors = (exit: Exit.Exit<unknown, { readonly message: string }>) =>
    Exit.isFailure(exit)
        ? Array.sort(
              Array.map(exit.cause.reasons.filter(Cause.isFailReason), ({ error }) => error.message),
              Order.String,
          )
        : [];
const chosen = ({ containers }: Description) => Effect.fromOption(Array.findFirst(containers, ({ kind, controls }) => (kind === 'toolbar' ? Array.head(controls) : Option.none())));
const states = Effect.fnUntraced(function* () {
    const { source } = yield* hosted([]);
    const described = yield* source.describe({});
    const choice = yield* chosen(described);
    assert.deepStrictEqual<unknown>(
        { value: choice.value, options: Array.map(choice.options ?? [], (option) => Struct.omit(option, ['locator'])), pointer: described.pointer },
        {
            value: { _tag: 'text', value: 'SELECT' },
            options: [
                { label: 'SELECT', shortcut: { key: 's', modifiers: ['shift'] }, help: 'Click to select' },
                { label: 'PEN', shortcut: { key: 'p', modifiers: ['shift'] }, help: 'Click to pen' },
                { label: 'HAND', shortcut: { key: 'h', modifiers: ['shift'] }, help: 'Click to hand' },
            ],
            pointer: { holding: HOLDING, tool: 'SELECT', shortcut: { key: 's', modifiers: ['shift'] }, help: 'Click to select' },
        },
    );
});
const takes = Effect.fnUntraced(function* () {
    const { held, source } = yield* hosted([]);
    const choice = yield* Effect.flatMap(source.describe({}), chosen);
    const option = (label: string) => Effect.fromOption(Array.findFirst(choice.options ?? [], (each) => each.label === label));
    yield* source.operate({ _tag: 'press', locator: (yield* option('PEN')).locator });
    const byOption = yield* SubscriptionRef.get(held);
    yield* source.operate({ _tag: 'press', locator: choice.locator, to: 'SELECT' });
    const byLabel = yield* SubscriptionRef.get(held);
    const refused = yield* Effect.flip(source.operate({ _tag: 'press', locator: (yield* option('HAND')).locator }));
    assert.deepStrictEqual({ byOption, byLabel, refused: refused._tag, kept: yield* SubscriptionRef.get(held) }, { byOption: 'pen', byLabel: 'select', refused: 'Unmatched', kept: 'select' });
});
const points = Effect.fnUntraced(function* () {
    const { source } = yield* hosted(['select', 'pen', 'pen', 'select']);
    assert.deepStrictEqual<unknown>(yield* Stream.runCollect(source.pointer), [
        { holding: HOLDING, tool: 'PEN', shortcut: { key: 'p', modifiers: ['shift'] }, help: 'Click to pen' },
        { holding: HOLDING, tool: 'SELECT', shortcut: { key: 's', modifiers: ['shift'] }, help: 'Click to select' },
    ]);
});
const relabels = Effect.fnUntraced(function* () {
    const source = Description.tools({ current: Effect.succeed(shown('pen')), listed: Effect.succeed(Array.map(LISTED, (id) => ({ id, label: id }))), select: () => Effect.void, changes: Stream.empty });
    const choice = yield* Effect.flatMap(source.describe({}), chosen);
    assert.deepStrictEqual<unknown>({ value: choice.value, options: Array.map(choice.options ?? [], ({ label }) => label) }, { value: { _tag: 'text', value: 'pen tool' }, options: ['select', 'pen tool', 'hand'] });
});
const lists = Effect.fnUntraced(function* ({ tools, current: { at, ...taken } }: Schema.Struct.Type<typeof Listing>) {
    const listed = Array.dedupeWith(tools, (left, right) => left.id === right.id);
    const { id, ...own } = Array.getUnsafe(listed, at % listed.length);
    const source = Description.tools({ current: Effect.succeed({ ...taken, id }), listed: Effect.succeed(listed), select: () => Effect.void, changes: Stream.empty });
    const described = yield* source.describe({});
    const options = (yield* chosen(described)).options ?? [];
    const byId = Record.fromIterableWith(options, ({ locator, ...stated }) => [Array.lastNonEmpty(locator)[1], stated] as const);
    assert.deepStrictEqual<unknown>(
        { count: options.length, others: Record.remove(byId, id), current: byId[id], pointer: described.pointer },
        {
            count: listed.length,
            others: Record.remove(
                Record.fromIterableWith(listed, ({ id: each, ...stated }) => [each, stated] as const),
                id,
            ),
            current: { ...own, ...taken },
            pointer: { tool: taken.label, ...Struct.pick(taken, ['shortcut', 'help']) },
        },
    );
});
const routes = Effect.fnUntraced(function* () {
    const operated = yield* Ref.make<readonly string[]>([]);
    const merged = Description.merge(part(SHELF, 'panel', operated, Option.none()), part(BAR, 'menuBar', operated, Option.none()));
    yield* merged.operate({ _tag: 'press', locator: FILE });
    yield* merged.operate({ _tag: 'press', locator: SHELF });
    const within = yield* merged.describe({ within: FILE });
    const stray = yield* Effect.flip(merged.operate({ _tag: 'press', locator: STRAY }));
    assert.deepStrictEqual<unknown>({ operated: yield* Ref.get(operated), within: Array.map(within.containers, ({ kind, locator }) => [kind, locator.length]), stray: stray._tag === 'Unmatched' ? Array.length(stray.candidates) : stray._tag }, { operated: ['menuBar 2', 'panel 1'], within: [['menuBar', 2]], stray: 2 });
});
const joins = Effect.fnUntraced(function* () {
    const operated = yield* Ref.make<readonly string[]>([]);
    const both = yield* Effect.exit(Description.merge(part(SHELF, 'panel', operated, Option.some('shelf unread')), part(BAR, 'menuBar', operated, Option.some('bar unread'))).describe({}));
    const one = yield* Effect.exit(Description.merge(part(SHELF, 'panel', operated, Option.none()), part(BAR, 'menuBar', operated, Option.some('bar unread'))).describe({}));
    assert.deepStrictEqual({ both: errors(both), one: errors(one) }, { both: ['bar unread', 'shelf unread'], one: ['bar unread'] });
});
const isolates = Effect.fnUntraced(function* () {
    const operated = yield* Ref.make<readonly string[]>([]);
    const pointers = Array.map(LISTED, (tool) => ({ tool }));
    const merged = Description.merge({ ...part(SHELF, 'panel', operated, Option.none()), pointer: Stream.fail('shelf unread') }, { ...part(BAR, 'menuBar', operated, Option.none()), pointer: Stream.fromIterable(pointers) });
    assert.deepStrictEqual<unknown>(yield* Stream.runCollect(merged.pointer), pointers);
});

// --- [COMPOSITION] ---------------------------------------------------------------------

describe(Description, () => {
    it.effect.prop(
        'puts a described container where the one Accessibility container of its kind sat with its frame, window, and child containers kept, nests one under the described container its parent names, leaves every other Accessibility container in tree order, gives each locator the reference interned for it with controls and options as children, and takes each pointer field the description states over the one read',
        { model: Element.Model, described: Description },
        folds,
        {
            arbitrary: { runs: RUNS },
        },
    );
});

describe(Description.tools, () => {
    it.effect('describe states the current tool by its label, every listed tool as an option with its label, shortcut, and help, and the pointer with the current shortcut and help beside the held fields the host states', states);
    it.effect('operate takes a tool by its option and by its label on the choice, and fails Unmatched where the host reads back another tool than the one asked for', takes);
    it.effect('pointer emits once for each change a host notice reveals and nothing for a notice that changes nothing', points);
    it.effect('describe labels the option of the current tool with the label the host reads for it, every other option keeping its listed label', relabels);
    it.effect.prop('describe lists every listed tool once as an option with its own label, shortcut, and help, the current tool taking the fields the host reads for it over its listed ones, and states the current label, shortcut, and help alone on the pointer', Listing, lists, { arbitrary: { runs: RUNS } });
});

describe(Description.merge, () => {
    it.effect('operate and describe within reach the source whose root heads the locator, and a locator under no root fails Unmatched naming every root', routes);
    it.effect('describe fails with the failure of every source that failed, joined in one cause', joins);
    it.effect('pointer keeps every value of a source while another source ends with a failure', isolates);
});

describe('Description.tools over a host listing no tools', () => {
    it.effect(
        'describe shows the current tool on the choice by the id its select takes, and operate with that shown value takes the tool back',
        Effect.fnUntraced(function* () {
            const held = yield* SubscriptionRef.make<string>('pen');
            const source = Description.tools({ current: Effect.map(SubscriptionRef.get(held), named), select: (id: string) => (Array.contains(LISTED, id) ? SubscriptionRef.set(held, id) : Effect.void), changes: Stream.empty });
            const choice = yield* Effect.flatMap(source.describe({}), chosen);
            const value = yield* Effect.fromOption(Option.fromUndefinedOr(choice.value?._tag === 'text' ? choice.value.value : undefined));
            yield* SubscriptionRef.set(held, 'select');
            yield* source.operate({ _tag: 'press', locator: choice.locator, to: value });
            assert.deepStrictEqual({ value, taken: yield* SubscriptionRef.get(held) }, { value: 'pen', taken: 'pen' });
        }),
    );
});
