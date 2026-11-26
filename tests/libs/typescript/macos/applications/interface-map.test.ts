import { layer as services } from '@effect/platform-node/NodeServices';
import { SqliteClient } from '@effect/sql-sqlite-node';
import { assert, describe, it } from '@effect/vitest';
import { InterfaceMap, Observation } from '@rasm/applications';
import { Catalog } from '@rasm/database';
import { Sqlite } from '@rasm/database/sqlite';
import { Array, DateTime, Effect, FileSystem, Layer, Option, Predicate, Record, Schema, Struct } from 'effect';
import { SqlClient } from 'effect/sql';
import { TestConsole } from 'effect/testing';

// --- [TYPES] ---------------------------------------------------------------------------

type Truth = { readonly [K in keyof typeof Truth]: (typeof Truth)[K]['Type'] };

// --- [CONSTANTS] -----------------------------------------------------------------------

const PRIVATE = '<private>';
const TOGGLE = '["AXCheckBox","Facing Pages"]';
const NOTE = '["AXTextField","Note"]';
const MENU = '["AXMenuBarItem","Window","AXMenu"]';
const SUBMENU = '["AXMenuBarItem","Window","AXMenu","AXMenuItem","Recent","AXMenu"]';
const RECENT = '["AXMenuItem","Recent"]';
const CLOSE = '["AXMenuItem","Close"]';
const CLOSE_ALL = '["AXMenuItem","Close All"]';
const LAYOUT = '["AXWindow","Layout"]';
const NEW = '["AXMenuItem","New Document…"]';
const DIALOG = '["AXDialog","New Document"]';
const UNITS = '["AXPopUpButton","Units"]';
const WIDTH = '["AXButton","Width"]';
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
const store = Effect.map(
    FileSystem.FileSystem.use((fs) => fs.makeTempFileScoped({ suffix: '.sqlite' })),
    (filename) =>
        Catalog.layer(
            InterfaceMap.catalog,
            Sqlite.layer((names) => SqliteClient.layer({ filename, ...names })),
        ),
);
const newDocument = Effect.forEach(
    [
        {
            build: '22.1.0.19',
            container: LAYOUT,
            kind: 'window',
            document: false,
            modal: false,
            layer: 0,
            size: [1440, 900],
            opened: instant(0),
            at: instant(0),
            complete: true,
            documents: [],
            scope: [
                { _tag: 'Documents', held: false },
                { _tag: 'Modal', at: null },
            ],
            controls: { [NEW]: { kind: 'item', role: 'AXMenuItem', label: { text: 'New Document…', attribute: 'AXTitle' }, shortcut: { key: 'n', modifiers: ['command'] }, positional: false } },
            layout: {},
            values: {},
        },
        {
            build: '22.1.0.19',
            container: DIALOG,
            kind: 'dialog',
            title: 'New Document',
            document: false,
            modal: true,
            layer: 8,
            size: [720, 480],
            opened: instant(1),
            at: instant(1),
            complete: true,
            documents: [],
            cause: { container: LAYOUT, control: NEW },
            scope: [
                { _tag: 'Value', at: UNITS, value: 'Millimeters' },
                { _tag: 'Documents', held: false },
                { _tag: 'Modal', at: DIALOG },
            ],
            controls: {
                [UNITS]: { kind: 'choice', role: 'AXPopUpButton', label: { text: 'Units', attribute: 'AXTitle' }, options: ['Inches', 'Millimeters', 'Points'], positional: false },
                [WIDTH]: { kind: 'measure', role: 'AXButton', label: { text: 'Width', attribute: 'AXTitleUIElement' }, help: 'Page width', positional: true, parent: DIALOG },
            },
            layout: {
                [WIDTH]: [
                    [24, 96],
                    [120, 22],
                ],
            },
            values: { [UNITS]: 'Millimeters', [WIDTH]: '210 mm' },
        },
    ] satisfies readonly (typeof Observation.Encoded)[],
    (each) => Schema.decodeEffect(Observation)(each),
);
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
    const observation = (read: number): typeof Observation.Encoded => {
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
                [TOGGLE]: { kind: 'toggle', role: 'AXCheckBox', label: { text: 'Facing Pages', attribute: 'AXTitle' }, positional: false },
                [NOTE]: { kind: 'text', role: 'AXTextField', label: { text: 'Note', attribute: 'AXTitle' }, positional: false },
                ...Record.fromIterableWith(
                    Array.filter(
                        Array.makeBy(conditions.length, (index) => index),
                        (index) => present(read, index),
                    ),
                    (index) => [JSON.stringify(['AXButton', label(index)]), { kind: 'action', role: 'AXButton', label: { text: label(index), attribute: 'AXDescription' }, positional: false }],
                ),
            },
            layout: {},
            values: { [TOGGLE]: toggle, [NOTE]: `Saved by ${user} in ${home}/Briefs/${name}` },
        };
    };
    const level = (read: number, at: number): typeof Observation.Encoded => ({
        build: build(read),
        container: MENU,
        kind: 'menu',
        document: false,
        modal: false,
        layer: 101,
        size: [0, 0],
        opened: instant(opener(read)),
        at: instant(at),
        complete: true,
        documents: [],
        scope: [
            { _tag: 'Documents', held: true },
            { _tag: 'Modal', at: null },
        ],
        controls: {
            [RECENT]: { kind: 'item', role: 'AXMenuItem', label: { text: 'Recent', attribute: 'AXTitle' }, ...(bit(read, 0) ? {} : { opens: SUBMENU }), positional: false },
            [CLOSE]: { kind: 'item', role: 'AXMenuItem', label: { text: 'Close', attribute: 'AXTitle' }, shortcut: { key: 'w', modifiers: ['command'] }, separated: true, positional: false },
            [CLOSE_ALL]: { kind: 'item', role: 'AXMenuItem', label: { text: 'Close All', attribute: 'AXTitle' }, shortcut: { key: 'w', modifiers: ['command', 'option'] }, alternate: CLOSE, positional: false },
        },
        layout: {},
        values: {},
    });
    const submenu = (read: number): typeof Observation.Encoded => ({
        ...level(read, read),
        container: SUBMENU,
        cause: { container: MENU, control: RECENT },
        controls: { '["AXMenuItem","Clear Menu"]': { kind: 'item', role: 'AXMenuItem', label: { text: 'Clear Menu', attribute: 'AXTitle' }, positional: false } },
    });
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
    return { user: { username: user, homedir: home }, stem, readings, present, generation, build, observation, level, submenu, opener, oracle, stored: (index: number) => JSON.stringify(['AXButton', `Item ${index} of ${PRIVATE}`]), window: JSON.stringify(['AXWindow', PRIVATE]) };
};

// --- [COMPOSITION] ---------------------------------------------------------------------

describe(InterfaceMap, () => {
    it.effect.prop(
        'keeps what each read showed, retires only what the rule names, holds a returning entry persistent, and stores no document name, user name, or text value',
        Truth,
        Effect.fnUntraced(function* (truth) {
            const world = app(truth);
            const reads = yield* Effect.forEach(world.readings, (read) => Schema.decodeEffect(Observation)(world.observation(read)));
            const states = Array.scan(reads, { map: InterfaceMap.empty, changes: [] as readonly (typeof InterfaceMap.Change.Type)[] }, ({ map }, read) => map.fold(read, world.user));
            const allChanges = states.flatMap(({ changes }) => changes);
            const final = Array.lastNonEmpty(world.readings);
            const last = Array.lastNonEmpty(states).map;
            const controls = last.containers[world.window]?.controls ?? {};
            assert.isTrue(Record.has(controls, TOGGLE), 'the reads reached the stored window');
            const held = (read: number, key: string) => states[read]?.map.containers[world.window]?.controls[key];
            const returning = (index: number, key: string) => {
                for (const read of world.readings) {
                    const [before, after] = [held(read, key), held(read + 1, key)];
                    if (before !== undefined && Option.isSome(before.retired) && world.present(read, index)) {
                        assert.isTrue(after !== undefined && Option.isNone(after.retired) && after.tally.persistent, `${key} shown after retirement returns persistent`);
                        assert.isTrue(
                            states[read + 1]?.changes.some((change) => change._tag === 'Appeared' && change.at === key),
                            `${key} appears again on its return`,
                        );
                    }
                    if (before?.tally.persistent === true && Option.isNone(before.retired) && after !== undefined && Option.isSome(after.retired)) {
                        assert.notStrictEqual(before.tally.verified, world.build(read), `${key} held persistent retires only on a build it was not seen in`);
                    }
                }
            };
            const settled = (index: number) => {
                const key = world.stored(index);
                const { condition, sightings, retirement, epochs } = world.oracle(index);
                const entry = controls[key];
                const live = entry !== undefined && Option.isNone(entry.retired);
                const count = (tag: (typeof InterfaceMap.Change.Type)['_tag']) => allChanges.filter((change) => change._tag === tag && change.at === key).length;
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
            const opening = yield* Schema.decodeEffect(Observation)(world.submenu(0));
            const menus = yield* Effect.forEach(world.readings, (read) => Effect.all([Schema.decodeEffect(Observation)(world.level(read, read)), Schema.decodeEffect(Observation)(world.level(read, read + 0.5))]));
            const leveled = menus.reduce((map, [first, again], read) => {
                const once = map.fold(first, world.user).map;
                const twice = once.fold(again, world.user);
                assert.deepStrictEqual(twice.changes, [], 'a menu level read twice in one opening changes nothing');
                assert.deepStrictEqual(Record.keys(twice.map.containers).toSorted(), Record.keys(once.containers).toSorted(), 'a menu level read twice stores one structure');
                assert.deepStrictEqual(Record.map(twice.map.containers[MENU]?.controls ?? {}, Struct.omit(['tally'])), Record.map(once.containers[MENU]?.controls ?? {}, Struct.omit(['tally'])), 'a second read keeps every item as the first stored it');
                return read === 0 ? twice.map.fold(opening, world.user).map : twice.map;
            }, last);
            const items = leveled.containers[MENU]?.controls ?? {};
            assert.deepStrictEqual(items[RECENT]?.opens, Option.some(SUBMENU), 'the item keeps its submenu link through reads that omit it');
            assert.deepStrictEqual(items[RECENT]?.label, Option.some({ text: 'Recent', attribute: 'AXTitle' }), 'a label keeps the attribute it was read from');
            assert.deepStrictEqual(items[CLOSE]?.separated, Option.some(true), 'a separator mark is stored');
            assert.deepStrictEqual(items[CLOSE_ALL]?.alternate, Option.some(CLOSE), 'an alternate names the item it shares a place with');
            assert.strictEqual(leveled.containers[MENU]?.container.tally.seen, new Set(world.readings.map(world.opener)).size, 'a menu level is seen once per opening');
        }),
        { arbitrary: { seed: 2026, runs: 300 } },
    );

    it.effect(
        'reads back on a second start, after the catalog reconciles, every document the first start wrote',
        Effect.fnUntraced(function* () {
            const [opened, observed] = yield* Effect.all([store, newDocument]);
            const written = yield* Effect.scoped(
                Effect.gen(function* () {
                    const map = yield* InterfaceMap.open('com.adobe.InDesign', 'en');
                    yield* Effect.forEach(observed, map.observe);
                    return yield* map.present;
                }),
            ).pipe(Effect.provide(opened));
            const read = yield* Effect.scoped(Effect.flatMap(InterfaceMap.open('com.adobe.InDesign', 'en'), ({ present }) => present)).pipe(Effect.provide(Layer.fresh(opened)));
            assert.deepStrictEqual(Record.keys(written.containers).toSorted(), [DIALOG, LAYOUT].toSorted());
            assert.deepStrictEqual(read, written);
        }, Effect.provide(services)),
    );

    it.effect(
        'reads a store holding a document an earlier shape wrote as if that document were absent, and writes it in the current shape at its next sighting',
        Effect.fnUntraced(function* () {
            const [opened, observed] = yield* Effect.all([store, newDocument]);
            const recalled = Effect.scoped(Effect.flatMap(InterfaceMap.open('com.adobe.InDesign', 'en'), ({ present }) => present));
            const sighted = (reads: typeof observed) => Effect.scoped(Effect.flatMap(InterfaceMap.open('com.adobe.InDesign', 'en'), ({ observe }) => Effect.forEach(reads, observe)));
            const [written, aged] = yield* Effect.gen(function* () {
                const sql = yield* SqlClient.SqlClient;
                yield* sighted(observed);
                const fresh = yield* recalled;
                const rows = yield* sql`UPDATE containers SET body = replace(body, '"label":{"text":"Units","attribute":"AXTitle"}', '"label":"Units"') WHERE locator = ${DIALOG} RETURNING instr(body, '"label":"Units"') > 0 AS aged`;
                return [fresh, rows] as const;
            }).pipe(Effect.provide(opened));
            const stale = yield* recalled.pipe(Effect.provide(Layer.fresh(opened)));
            const rewritten = yield* Effect.andThen(sighted(Array.drop(observed, 1)), recalled).pipe(Effect.provide(Layer.fresh(opened)));
            assert.deepStrictEqual(aged, [{ aged: 1 }], 'the dialog row holds its Units label as the earlier bare string');
            assert.deepStrictEqual(stale, new InterfaceMap({ application: written.application, containers: Record.remove(written.containers, DIALOG) }), 'the store reads as it would without the dialog row');
            assert.deepStrictEqual((yield* TestConsole.logLines).filter(Predicate.isObject), Array.replicate({ bundle: 'com.adobe.InDesign', language: 'en', stale: 1 }, 2), 'both opens before the rewrite log one line naming the bundle and the count dropped, the open after it none');
            assert.deepStrictEqual(rewritten.containers[DIALOG]?.controls[UNITS]?.label, Option.some({ text: 'Units', attribute: 'AXTitle' }), 'the dialog sighted again reads back with its label in the current shape');
        }, Effect.provide(services)),
    );
});
