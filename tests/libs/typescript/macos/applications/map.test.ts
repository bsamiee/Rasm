import { layer as services } from '@effect/platform-node/NodeServices';
import { SqliteClient } from '@effect/sql-sqlite-node';
import { assert, describe, it } from '@effect/vitest';
import { InterfaceMap } from '@rasm/applications';
import { Catalog } from '@rasm/database';
import { Sqlite } from '@rasm/database/sqlite';
import { Array, DateTime, Effect, FileSystem, Layer, Option, Record, Schema } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Observed = typeof InterfaceMap.Observed.Encoded;
type Truth = { readonly [K in keyof typeof Truth]: (typeof Truth)[K]['Type'] };

// --- [CONSTANTS] -----------------------------------------------------------------------

const PRIVATE = '<private>';
const TOGGLE = '["AXCheckBox","Facing Pages"]';
const NOTE = '["AXTextField","Note"]';
const START = Date.UTC(2026, 10, 25, 9);

// --- [MODELS] --------------------------------------------------------------------------

const Condition = Schema.TaggedUnion({
    Always: {},
    When: { value: Schema.Boolean },
    Until: { read: Schema.Int.check(Schema.isBetween({ minimum: 0, maximum: 5 })).annotate({ description: 'Read it vanishes at, early so later reads can miss it' }) },
    Sometimes: {},
});
const Read = Schema.Struct({
    opens: Schema.Int.check(Schema.isBetween({ minimum: 0, maximum: 3 })).annotate({ description: 'Zero opens a new epoch, so epochs hold several reads' }),
    toggle: Schema.Boolean,
    complete: Schema.Int.check(Schema.isBetween({ minimum: 0, maximum: 3 })).annotate({ description: 'Zero cuts the read short, so most reads can miss an entry' }),
    rebuilt: Schema.Int.check(Schema.isBetween({ minimum: 0, maximum: 15 })).annotate({ description: 'Zero starts a new build, so a build holds several epochs' }),
    noise: Schema.Int.check(Schema.isBetween({ minimum: 0, maximum: 1023 })),
});
const Truth = {
    conditions: Schema.Array(Condition).check(Schema.isMinLength(1), Schema.isMaxLength(10)),
    reads: Schema.NonEmptyArray(Read).check(Schema.isMinLength(14), Schema.isMaxLength(24)),
    owner: Schema.Int.check(Schema.isBetween({ minimum: 1, maximum: 999 })),
    draft: Schema.Int.check(Schema.isBetween({ minimum: 1, maximum: 999 })),
};

// --- [OPERATIONS] ----------------------------------------------------------------------

const instant = (minute: number) => DateTime.formatIso(DateTime.makeUnsafe(START + minute * 60_000));
const app = ({ conditions, reads, owner, draft }: Truth) => {
    const [user, stem] = [`owner${owner}`, `brief-${draft}`];
    const [home, name] = [`/Users/${user}`, `${stem}.indd`];
    const readings = Array.makeBy(reads.length, (read) => read);
    const label = (index: number) => `Item ${index} of ${index % 2 === 0 ? name : stem}`;
    const bit = (read: number, offset: number) => ((reads[read]?.noise ?? 0) >> offset) % 2 === 1;
    const shown = (read: number, index: number) =>
        Condition.match(conditions[index] ?? { _tag: 'Always' }, {
            Always: () => true,
            When: ({ value }) => reads[read]?.toggle === value,
            Until: ({ read: until }) => read < until,
            Sometimes: () => bit(read, index),
        });
    const present = (read: number, index: number) => shown(read, index) && (reads[read]?.complete !== 0 || !bit(read, (index + 3) % 10));
    const opener = (read: number) =>
        Option.getOrElse(
            Array.findLastIndex(Array.take(reads, read + 1), (entry) => entry.opens === 0),
            () => 0,
        );
    const generation = (read: number) => Array.countBy(Array.take(Array.drop(reads, 1), read), (entry) => entry.rebuilt === 0);
    const build = (read: number) => `22.1.0.${19 + generation(read)}`;
    const observation = (read: number): Observed => {
        const { toggle, complete } = reads[read] ?? reads[0];
        return {
            build: build(read),
            container: JSON.stringify(['AXWindow', stem]),
            kind: 'window',
            title: `${name} @ 50%`,
            document: false,
            modal: false,
            layer: 0,
            size: [800, 600],
            opened: instant(opener(read)),
            at: instant(read),
            complete: complete !== 0,
            documents: [`${home}/Briefs/${name}`],
            scope: [
                { _tag: 'Value', at: TOGGLE, value: toggle },
                { _tag: 'Documents', held: true },
                { _tag: 'Modal', at: null },
            ],
            controls: {
                [TOGGLE]: { kind: 'toggle', role: 'AXCheckBox', label: 'Facing Pages', positional: false },
                [NOTE]: { kind: 'text', role: 'AXTextField', label: 'Note', positional: false },
                ...Record.fromIterableWith(
                    Array.filter(
                        Array.makeBy(conditions.length, (index) => index),
                        (index) => present(read, index),
                    ),
                    (index) => [JSON.stringify(['AXButton', label(index)]), { kind: 'action', role: 'AXButton', label: label(index), positional: false }],
                ),
            },
            layout: {},
            values: { [TOGGLE]: toggle, [NOTE]: `Saved by ${user} in ${home}/Briefs/${name}` },
        };
    };
    const oracle = (index: number) => {
        const sightings = readings.filter((read) => present(read, index));
        const retirement =
            conditions[index]?._tag === 'Until'
                ? Option.flatMap(Array.last(sightings), (seen) => {
                      const misses = readings.filter((read) => read > seen && reads[read]?.complete !== 0 && reads[read]?.toggle === reads[seen]?.toggle);
                      return Array.findFirst(misses, (read, count) => build(read) !== build(seen) || (count >= 2 && new Set(misses.slice(count - 2, count + 1).map(opener)).size >= 2));
                  })
                : Option.none();
        return { condition: conditions[index]?._tag, sightings, retirement, epochs: new Set(sightings.map(opener)).size };
    };
    return { user: { username: user, homedir: home }, stem, readings, present, generation, build, observation, oracle, stored: (index: number) => JSON.stringify(['AXButton', `Item ${index} of ${PRIVATE}`]), window: JSON.stringify(['AXWindow', PRIVATE]) };
};

// --- [COMPOSITION] ---------------------------------------------------------------------

describe(InterfaceMap, () => {
    it.effect.prop(
        'keeps what each read showed, retires only what the rule names, holds a returning entry sticky, and stores no document name, user name, or text value',
        Truth,
        Effect.fnUntraced(function* (truth) {
            const world = app(truth);
            const reads = yield* Effect.forEach(world.readings, (read) => Schema.decodeEffect(InterfaceMap.Observed)(world.observation(read)));
            const states = Array.scan(reads, { map: InterfaceMap.empty, facts: [] as readonly (typeof InterfaceMap.Fact.Type)[] }, ({ map }, read) => map.fold(read, world.user));
            const told = states.flatMap(({ facts }) => facts);
            const final = Array.lastNonEmpty(world.readings);
            const last = Array.lastNonEmpty(states).map;
            const controls = last.containers[world.window]?.controls ?? {};
            assert.isTrue(Record.has(controls, TOGGLE), 'the reads reached the stored window');
            const held = (read: number, key: string) => states[read]?.map.containers[world.window]?.controls[key];
            const returning = (index: number, key: string) => {
                for (const read of world.readings) {
                    const [before, after] = [held(read, key), held(read + 1, key)];
                    if (before !== undefined && Option.isSome(before.retired) && world.present(read, index)) {
                        assert.isTrue(after !== undefined && Option.isNone(after.retired) && after.tally.sticky, `${key} shown after retirement returns sticky`);
                        assert.isTrue(
                            states[read + 1]?.facts.some((fact) => fact._tag === 'Appeared' && fact.at === key),
                            `${key} appears again on its return`,
                        );
                    }
                    if (before?.tally.sticky === true && Option.isNone(before.retired) && after !== undefined && Option.isSome(after.retired)) {
                        assert.notStrictEqual(before.tally.verified, world.build(read), `${key} held sticky retires only on a build it was not seen in`);
                    }
                }
            };
            const settled = (index: number) => {
                const key = world.stored(index);
                const { condition, sightings, retirement, epochs } = world.oracle(index);
                const entry = controls[key];
                const live = entry !== undefined && Option.isNone(entry.retired);
                const count = (tag: (typeof InterfaceMap.Fact.Type)['_tag']) => told.filter((fact) => fact._tag === tag && fact.at === key).length;
                if (condition !== 'Sometimes') {
                    assert.strictEqual(count('Appeared'), sightings.length > 0 ? 1 : 0, `${key} appears once when shown`);
                    assert.strictEqual(count('Gone'), Option.isSome(retirement) ? 1 : 0, `${key} goes once when the rule retires it`);
                    if (Option.isSome(retirement)) {
                        const retired = entry === undefined ? world.generation(final) - world.generation(retirement.value) >= 2 : Option.contains(entry.retired, world.build(retirement.value));
                        assert.isTrue(retired, `${key} is retired under ${world.build(retirement.value)}`);
                    } else {
                        assert.strictEqual(live, sightings.length > 0, `${key} is held live exactly when it was shown`);
                    }
                }
                if (condition === 'Always' && entry !== undefined) {
                    assert.strictEqual(entry.tally.seen, epochs, `${key} is seen once per epoch it showed in`);
                }
                if (world.present(final, index)) {
                    assert.isTrue(live, `${key} shown last is held live`);
                }
                returning(index, key);
            };
            for (const index of truth.conditions.keys()) {
                settled(index);
            }
            const written = yield* Schema.encodeEffect(Schema.fromJsonString(Schema.Unknown))(last);
            assert.include(written, PRIVATE);
            assert.notInclude(written, world.user.username);
            assert.notInclude(written, world.stem);
            assert.isTrue(Option.isNone(controls[NOTE]?.value ?? Option.none()), 'a text value is never stored');
        }),
        { arbitrary: { seed: 2026, runs: 300 } },
    );

    it.effect(
        'reads back on a second start, after the catalog reconciles, every document the first start wrote',
        Effect.fnUntraced(function* () {
            const filename = yield* FileSystem.FileSystem.use((fs) => fs.makeTempFileScoped({ suffix: '.sqlite' }));
            const opened = Catalog.layer(
                InterfaceMap.catalog,
                Sqlite.layer((names) => SqliteClient.layer({ filename, ...names })),
            );
            const [window, press, dialog, units, width] = ['["AXWindow","Layout"]', '["AXMenuItem","New Document…"]', '["AXDialog","New Document"]', '["AXPopUpButton","Units"]', '["AXButton","Width"]'];
            const shared = { build: '22.1.0.19', modal: false, layer: 0, documents: [], layout: {}, document: false, complete: true } as const;
            const encoded: readonly Observed[] = [
                {
                    ...shared,
                    container: window,
                    kind: 'window',
                    size: [1440, 900],
                    opened: instant(0),
                    at: instant(0),
                    scope: [
                        { _tag: 'Documents', held: false },
                        { _tag: 'Modal', at: null },
                    ],
                    controls: { [press]: { kind: 'item', role: 'AXMenuItem', label: 'New Document…', shortcut: '⌘N', positional: false } },
                    values: {},
                },
                {
                    ...shared,
                    container: dialog,
                    kind: 'dialog',
                    title: 'New Document',
                    modal: true,
                    layer: 8,
                    size: [720, 480],
                    opened: instant(1),
                    at: instant(1),
                    cause: { container: window, control: press },
                    scope: [
                        { _tag: 'Value', at: units, value: 'Millimeters' },
                        { _tag: 'Documents', held: false },
                        { _tag: 'Modal', at: dialog },
                    ],
                    controls: {
                        [units]: { kind: 'choice', role: 'AXPopUpButton', label: 'Units', options: ['Inches', 'Millimeters', 'Points'], positional: false },
                        [width]: { kind: 'measure', role: 'AXButton', label: 'Width', help: 'Page width', positional: true, parent: dialog },
                    },
                    layout: {
                        [width]: [
                            [24, 96],
                            [120, 22],
                        ],
                    },
                    values: { [units]: 'Millimeters', [width]: '210 mm' },
                },
            ];
            const observed = yield* Effect.forEach(encoded, (each) => Schema.decodeEffect(InterfaceMap.Observed)(each));
            const written = yield* Effect.scoped(
                Effect.gen(function* () {
                    const map = yield* InterfaceMap.open('com.adobe.InDesign', 'en');
                    yield* Effect.forEach(observed, map.observe);
                    return yield* map.present;
                }),
            ).pipe(Effect.provide(opened));
            const read = yield* Effect.scoped(Effect.flatMap(InterfaceMap.open('com.adobe.InDesign', 'en'), ({ present }) => present)).pipe(Effect.provide(Layer.fresh(opened)));
            assert.deepStrictEqual(Record.keys(written.containers).toSorted(), [dialog, window].toSorted());
            assert.deepStrictEqual(read, written);
        }, Effect.provide(services)),
    );
});
