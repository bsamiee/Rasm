import { assert, describe, it } from '@effect/vitest';
import { KeyStep, Model } from '@rasm/applications/contract';
import { Description, type FacetContainer, type FacetControl } from '@rasm/applications/source';
import { Array, Cause, Effect, Exit, Option, Order, Record, Ref, Schema, Stream, Struct, SubscriptionRef } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const INTERNED = 2 ** 40;
const RUNS = 2000;
const LISTED = ['select', 'pen', 'hand'] as const;
const REFUSED = 'hand';
const HOLDING = ['/document[@id=1]/rectangle[@id=2]'];
const SHELF: FacetControl['locator'] = [['Panels', 'Application']];
const BAR: FacetControl['locator'] = [['MenuActions', 'Application']];
const FILE: FacetControl['locator'] = [...BAR, ['MenuAction', 'File Menu']];
const STRAY: FacetControl['locator'] = [['Windows', 'Application']];

// --- [MODELS] --------------------------------------------------------------------------

const Stated = { label: Schema.NonEmptyString, shortcut: Schema.optionalKey(KeyStep), help: Schema.optionalKey(Schema.NonEmptyString) };
const Listing = {
    tools: Schema.NonEmptyArray(Schema.Struct({ id: Schema.NonEmptyString, ...Stated })),
    current: Schema.Struct({ at: Schema.Natural.annotate({ description: 'Listed tool the host reads as current, taken modulo the deduplicated listing' }), ...Stated }),
};

// --- [OPERATIONS] ----------------------------------------------------------------------

const heldRef = ({ ref }: { readonly ref: number }) => ref;
const folds = Effect.fnUntraced(function* ({ model, described }: { readonly model: Model; readonly described: Description }) {
    const asked: string[] = [];
    const intern = (locators: readonly string[]) =>
        Effect.sync(() => {
            asked.push(...locators);
            return Array.makeBy(locators.length, (index) => INTERNED + index);
        });
    const { model: composed, routes } = yield* Description.compose(model, described, intern);
    const refOf = (locator: readonly (readonly [string, string])[]) => INTERNED + asked.indexOf(JSON.stringify(locator.map(([role, value]) => ({ key: { identifier: { value } }, role }))));
    const optionRef = ({ locator }: { readonly locator: readonly (readonly [string, string])[] }) => refOf(locator);
    const levels = described.containers;
    const sole = (kind: string) => model.containers.filter((container) => container.kind === kind);
    const owned = new Set(levels.filter((level) => level.parent === undefined && sole(level.kind).length === 1).map(({ kind }) => kind));
    assert.deepStrictEqual(
        composed.containers.filter(({ ref }) => ref < INTERNED),
        model.containers.filter(({ kind }) => !owned.has(kind)),
    );
    const shown = levels.map((level) => {
        const placed = composed.containers.find(({ ref }) => ref === refOf(level.locator));
        return { frame: placed?.frame, window: placed?.window, controls: placed?.controls.map(({ ref, options }) => ({ ref, options: options?.map(heldRef) })) };
    });
    const stated = levels.map((level) => {
        const [folded] = level.parent === undefined && sole(level.kind).length === 1 ? sole(level.kind) : [];
        return { frame: folded?.frame, window: folded?.window, controls: level.controls.map(({ locator, options }) => ({ ref: refOf(locator), options: options?.map(optionRef) })) };
    });
    const placed: Model['containers'] = composed.containers;
    const carried = Array.flatMap(placed, ({ controls }) => Array.flatMap(controls, ({ options = [] }) => options));
    const declared = Array.flatMap(levels, ({ controls }) => Array.flatMap(controls, ({ options = [] }) => options));
    assert.deepStrictEqual(
        Record.fromIterableWith(
            Array.filter(carried, ({ ref }) => ref >= INTERNED),
            ({ ref, ...facts }) => [`${ref}`, facts] as const,
        ),
        Record.fromIterableWith(declared, ({ locator, ...facts }) => [`${refOf(locator)}`, facts] as const),
        'every facet option keeps the label, shortcut, and help it states',
    );
    assert.deepStrictEqual(new Set(Object.keys(routes)), new Set(asked.map((_, index) => `${INTERNED + index}`)), 'every interned facet locator routes back to its steps');
    assert.deepStrictEqual(
        { shown, tool: composed.pointer?.tool, shortcut: composed.pointer?.shortcut, help: composed.pointer?.help, holding: composed.pointer?.holding, cursor: composed.pointer?.cursor, draggingFrame: composed.pointer?.draggingFrame },
        {
            shown: stated,
            tool: described.pointer?.tool ?? model.pointer?.tool,
            shortcut: described.pointer?.shortcut ?? model.pointer?.shortcut,
            help: described.pointer?.help ?? model.pointer?.help,
            holding: described.pointer?.holding ?? model.pointer?.holding,
            cursor: model.pointer?.cursor,
            draggingFrame: model.pointer?.draggingFrame,
        },
    );
});
const named = (id: string) => ({ id, label: id.toUpperCase(), shortcut: { key: id.charAt(0), modifiers: ['shift'] }, help: `Click to ${id}` }) as const;
const hosted = Effect.fnUntraced(function* (script: readonly (typeof LISTED)[number][]) {
    const held = yield* SubscriptionRef.make<string>(Array.headNonEmpty(LISTED));
    const facet = Description.tools({
        current: Effect.map(SubscriptionRef.get(held), named),
        listed: Effect.succeed(Array.map(LISTED, named)),
        select: (id: string) => (id === REFUSED ? Effect.void : SubscriptionRef.set(held, id)),
        changes: Stream.mapEffect(Stream.fromIterable(script), (id) => SubscriptionRef.set(held, id)),
        pointer: Effect.succeed({ holding: HOLDING }),
    });
    return { held, facet };
});
const shown = (id: string) => ({ id, label: `${id} tool` });
const part = (root: FacetControl['locator'], kind: FacetContainer['kind'], operated: Ref.Ref<readonly string[]>, refusal: Option.Option<string>) => ({
    roots: Effect.succeed([root]),
    describe: ({ within }: { readonly within?: FacetControl['locator'] }) =>
        Option.match(refusal, {
            onSome: (refused) => Effect.fail(new Cause.UnknownError(refused, refused)),
            onNone: () => Effect.succeed(Description.make({ containers: [{ locator: within ?? root, kind, controls: [] }] })),
        }),
    operate: ({ locator }: { readonly locator: FacetControl['locator'] }) => Ref.update(operated, Array.append(`${kind} ${locator.length}`)),
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
    const { facet } = yield* hosted([]);
    const described = yield* facet.describe({});
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
    const { held, facet } = yield* hosted([]);
    const choice = yield* Effect.flatMap(facet.describe({}), chosen);
    const option = (label: string) => Effect.fromOption(Array.findFirst(choice.options ?? [], (each) => each.label === label));
    yield* facet.operate({ locator: (yield* option('PEN')).locator });
    const byOption = yield* SubscriptionRef.get(held);
    yield* facet.operate({ locator: choice.locator, to: 'SELECT' });
    const byLabel = yield* SubscriptionRef.get(held);
    const refused = yield* Effect.flip(facet.operate({ locator: (yield* option('HAND')).locator }));
    assert.deepStrictEqual({ byOption, byLabel, refused: refused._tag, kept: yield* SubscriptionRef.get(held) }, { byOption: 'pen', byLabel: 'select', refused: 'Unmatched', kept: 'select' });
});
const changes = Effect.fnUntraced(function* () {
    const { facet } = yield* hosted(['select', 'pen', 'pen', 'select']);
    const events = yield* Stream.runCollect(facet.changes);
    assert.deepStrictEqual(
        Array.map(events, ({ name, detail }) => [name, Option.getOrUndefined(detail)]),
        [
            ['pointer', { holding: HOLDING, tool: 'PEN', shortcut: { key: 'p', modifiers: ['shift'] }, help: 'Click to pen' }],
            ['pointer', { holding: HOLDING, tool: 'SELECT', shortcut: { key: 's', modifiers: ['shift'] }, help: 'Click to select' }],
        ],
    );
});
const relabels = Effect.fnUntraced(function* () {
    const facet = Description.tools({ current: Effect.succeed(shown('pen')), listed: Effect.succeed(Array.map(LISTED, (id) => ({ id, label: id }))), select: () => Effect.void, changes: Stream.empty });
    const choice = yield* Effect.flatMap(facet.describe({}), chosen);
    assert.deepStrictEqual<unknown>({ value: choice.value, options: Array.map(choice.options ?? [], ({ label }) => label) }, { value: { _tag: 'text', value: 'pen tool' }, options: ['select', 'pen tool', 'hand'] });
});
const lists = Effect.fnUntraced(function* ({ tools, current: { at, ...taken } }: Schema.Struct.Type<typeof Listing>) {
    const listed = Array.dedupeWith(tools, (left, right) => left.id === right.id);
    const { id, ...own } = Array.getUnsafe(listed, at % listed.length);
    const facet = Description.tools({ current: Effect.succeed({ ...taken, id }), listed: Effect.succeed(listed), select: () => Effect.void, changes: Stream.empty });
    const described = yield* facet.describe({});
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
    yield* merged.operate({ locator: FILE });
    yield* merged.operate({ locator: SHELF });
    const within = yield* merged.describe({ within: FILE });
    const stray = yield* Effect.flip(merged.operate({ locator: STRAY }));
    assert.deepStrictEqual<unknown>(
        { operated: yield* Ref.get(operated), within: Array.map(within.containers, ({ kind, locator }) => [kind, locator.length]), stray: stray._tag === 'Unmatched' ? [stray.subject, Array.length(stray.candidates)] : stray._tag },
        { operated: ['menuBar 2', 'panel 1'], within: [['menuBar', 2]], stray: ['facet root', 2] },
    );
});
const joins = Effect.fnUntraced(function* () {
    const operated = yield* Ref.make<readonly string[]>([]);
    const both = yield* Effect.exit(Description.merge(part(SHELF, 'panel', operated, Option.some('shelf unread')), part(BAR, 'menuBar', operated, Option.some('bar unread'))).describe({}));
    const one = yield* Effect.exit(Description.merge(part(SHELF, 'panel', operated, Option.none()), part(BAR, 'menuBar', operated, Option.some('bar unread'))).describe({}));
    assert.deepStrictEqual({ both: errors(both), one: errors(one) }, { both: ['bar unread', 'shelf unread'], one: ['bar unread'] });
});
const isolates = Effect.fnUntraced(function* () {
    const operated = yield* Ref.make<readonly string[]>([]);
    const failing = { ...part(SHELF, 'panel', operated, Option.none()), changes: Stream.fail('panels unheard') };
    const heard = { changes: Stream.fromIterable(Array.map(['afterOpen', 'afterSave', 'beforeClose'], (name) => ({ name, target: Option.none<string>(), detail: Option.none() }))) };
    const events = yield* Stream.runCollect(Description.merge(failing, heard).changes);
    assert.deepStrictEqual(
        Array.map(events, ({ name }) => name),
        ['afterOpen', 'afterSave', 'beforeClose'],
    );
});

// --- [COMPOSITION] ---------------------------------------------------------------------

describe(Description, () => {
    it.effect.prop(
        'folds the one Accessibility container of a top-level facet kind into the facet container with its frame kept, takes each pointer field the facet states over the one read and the cursor and drag from the read alone, leaves every other Accessibility container as read, and gives each facet locator the reference interned for it, each option keeping the label, shortcut, and help it states',
        { model: Model, described: Description },
        folds,
        {
            arbitrary: { runs: RUNS },
        },
    );
});

describe(Description.tools, () => {
    it.effect('describe states the current tool by its label, every listed tool as an option with its label, shortcut, and help, and the pointer with the current shortcut and help beside the held fields the host states', states);
    it.effect('operate takes a tool by its option and by its label on the choice, and fails Unmatched where the host reads back another tool than the one asked for', takes);
    it.effect('changes emits the pointer once for each change a host notice reveals and nothing for a notice that changes nothing', changes);
    it.effect('describe labels the option of the current tool with the label the host reads for it, every other option keeping its listed label', relabels);
    it.effect.prop('describe lists every listed tool once as an option with its own label, shortcut, and help, the current tool taking the fields the host reads for it over its listed ones, and states the current label, shortcut, and help alone on the pointer', Listing, lists, { arbitrary: { runs: RUNS } });
});

describe(Description.merge, () => {
    it.effect('operate and describe within reach the part whose root heads the locator, and a locator under no root fails Unmatched naming every root', routes);
    it.effect('describe fails with the failure of every part that failed, joined in one cause', joins);
    it.effect('changes keep every event of a part while another part ends with a failure', isolates);
});

describe('Description.tools over a host listing no tools', () => {
    it.effect(
        'describe shows the current tool on the choice by the id its select takes, and operate with that shown value takes the tool back',
        Effect.fnUntraced(function* () {
            const held = yield* SubscriptionRef.make<string>('pen');
            const facet = Description.tools({ current: Effect.map(SubscriptionRef.get(held), named), select: (id: string) => (Array.contains(LISTED, id) ? SubscriptionRef.set(held, id) : Effect.void), changes: Stream.empty });
            const choice = yield* Effect.flatMap(facet.describe({}), chosen);
            const value = yield* Effect.fromOption(Option.fromUndefinedOr(choice.value?._tag === 'text' ? choice.value.value : undefined));
            yield* SubscriptionRef.set(held, 'select');
            yield* facet.operate({ locator: choice.locator, to: value });
            assert.deepStrictEqual({ value, taken: yield* SubscriptionRef.get(held) }, { value: 'pen', taken: 'pen' });
        }),
    );
});
