import { assert, describe, it } from '@effect/vitest';
import { Model } from '@rasm/applications';
import { Description } from '@rasm/applications/source';
import { Array, Effect, Option, Stream, SubscriptionRef } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const INTERNED = 2 ** 40;
const RUNS = 2000;
const LISTED = ['select', 'pen', 'hand'] as const;
const REFUSED = 'hand';
const HOLDING = ['/document[@id=1]/rectangle[@id=2]'];

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
    assert.deepStrictEqual(new Set(Object.keys(routes)), new Set(asked.map((_, index) => `${INTERNED + index}`)), 'every interned facet locator routes back to its steps');
    assert.deepStrictEqual(
        { shown, tool: composed.pointer?.tool, holding: composed.pointer?.holding, cursor: composed.pointer?.cursor, dragging: composed.pointer?.dragging },
        { shown: stated, tool: described.pointer?.tool ?? model.pointer?.tool, holding: described.pointer?.holding ?? model.pointer?.holding, cursor: model.pointer?.cursor, dragging: model.pointer?.dragging },
    );
});
const named = (id: string) => ({ id, label: id.toUpperCase() });
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
const chosen = ({ containers }: Description) => Effect.fromOption(Array.findFirst(containers, ({ kind, controls }) => (kind === 'toolbar' ? Array.head(controls) : Option.none())));
const states = Effect.fnUntraced(function* () {
    const { facet } = yield* hosted([]);
    const described = yield* facet.describe({});
    const choice = yield* chosen(described);
    assert.deepStrictEqual<unknown>({ value: choice.value, options: Array.map(choice.options ?? [], ({ label }) => label), pointer: described.pointer }, { value: { text: { value: 'SELECT' } }, options: ['SELECT', 'PEN', 'HAND'], pointer: { holding: HOLDING, tool: 'SELECT' } });
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
            ['pointer', { holding: HOLDING, tool: 'PEN' }],
            ['pointer', { holding: HOLDING, tool: 'SELECT' }],
        ],
    );
});

// --- [COMPOSITION] ---------------------------------------------------------------------

describe(Description, () => {
    it.effect.prop(
        'folds the one Accessibility container of a top-level facet kind into the facet container with its frame kept, takes each pointer field the facet states over the one read and the cursor and drag from the read alone, leaves every other Accessibility container as read, and gives each facet locator the reference interned for it',
        { model: Model, described: Description },
        folds,
        {
            arbitrary: { runs: RUNS },
        },
    );
});

describe(Description.tools, () => {
    it.effect('describe states the current tool by its label, every listed tool as an option labelled by its label, and the pointer with the held fields the host states', states);
    it.effect('operate takes a tool by its option and by its label on the choice, and fails Unmatched where the host reads back another tool than the one asked for', takes);
    it.effect('changes emits the pointer once for each change a host notice reveals and nothing for a notice that changes nothing', changes);
});
