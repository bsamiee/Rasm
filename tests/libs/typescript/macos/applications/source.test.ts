import { assert, describe, it } from '@effect/vitest';
import { Model } from '@rasm/applications';
import { Description } from '@rasm/applications/source';
import { Effect } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const INTERNED = 2 ** 40;
const RUNS = 2000;

// --- [OPERATIONS] ----------------------------------------------------------------------

const heldRef = ({ ref }: { readonly ref: number }) => ref;
const folds = Effect.fnUntraced(function* ({ model, described }: { readonly model: typeof Model.Type; readonly described: Description }) {
    const asked: string[] = [];
    const intern = (locators: readonly string[]) =>
        Effect.sync(() => {
            asked.push(...locators);
            return Array.from(locators, (_, index) => INTERNED + index);
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
