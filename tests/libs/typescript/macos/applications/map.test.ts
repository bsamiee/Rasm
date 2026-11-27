import { pathToFileURL } from 'node:url';
import { layer as services } from '@effect/platform-node/NodeServices';
import { SqliteClient } from '@effect/sql-sqlite-node';
import { assert, describe, it } from '@effect/vitest';
import { Description } from '@rasm/applications/description';
import { InterfaceMap } from '@rasm/applications/map';
import { Element } from '@rasm/applications/server/element';
import { ApplicationFailure } from '@rasm/applications/server/failure';
import { Catalog } from '@rasm/database/catalog';
import { Sqlite } from '@rasm/database/sqlite';
import { Array, DateTime, Effect, FileSystem, Layer, Option, Predicate, Record, Result, Schema, Struct } from 'effect';
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
const OWNER = { username: 'owner', homedir: '/Users/owner' } as const;
const BUILDS = ['22.1.0.19', '22.2.0.4'] as const;
const TOOLBAR: typeof Description.Locator.Type = [['toolbar', 'tool']];
const RECTANGLE: typeof Description.Locator.Type = [...TOOLBAR, ['tool', 'RECTANGLE_TOOL']];
const PEN: typeof Description.Locator.Type = [...TOOLBAR, ['tool', 'PEN_TOOL']];
const DOCUMENT = JSON.stringify([{ key: { document: { type: 'com.adobe.indesign-document' } }, role: 'AXWindow' }]);

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
const identified = (index: number) => JSON.stringify([{ key: { identifier: { value: `item${index}` } }, role: 'AXButton' }]);
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
            modal: false,
            layer: 0,
            size: [1440, 900],
            opened: instant(0),
            at: instant(0),
            truncated: false,
            documents: [],
            conditions: [
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
            modal: true,
            layer: 8,
            size: [720, 480],
            opened: instant(1),
            at: instant(1),
            truncated: false,
            documents: [],
            opener: { container: LAYOUT, control: NEW },
            conditions: [
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
    ] satisfies readonly (typeof Element.Observation.Encoded)[],
    (each) => Schema.decodeEffect(Element.Observation)(each),
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
    const observation = (read: number): typeof Element.Observation.Encoded => {
        const { toggle, complete } = reads[read] ?? reads[0];
        return {
            build: build(read),
            container: DOCUMENT,
            kind: 'window',
            title: `${name} @ 50%`,
            modal: false,
            layer: 0,
            size: [800, 600],
            opened: instant(opener(read)),
            at: instant(read),
            truncated: complete === 0,
            documents: [`${home}/Briefs/${name}`, pathToFileURL(`${home}/Briefs/${name}`).href],
            conditions: [
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
                    (index) => [identified(index), { kind: 'action', role: 'AXButton', label: { text: label(index), attribute: 'AXDescription' }, positional: false }],
                ),
            },
            layout: {},
            values: { [TOGGLE]: toggle },
        };
    };
    const level = (read: number, at: number): typeof Element.Observation.Encoded => ({
        build: build(read),
        container: MENU,
        kind: 'menu',
        modal: false,
        layer: 101,
        size: [0, 0],
        opened: instant(opener(read)),
        at: instant(at),
        truncated: false,
        documents: [],
        conditions: [
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
    const submenu = (read: number): typeof Element.Observation.Encoded => ({
        ...level(read, read),
        container: SUBMENU,
        opener: { container: MENU, control: RECENT },
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
    return { user: { username: user, homedir: home }, stem, readings, present, generation, build, observation, level, submenu, opener, oracle, stored: identified, window: DOCUMENT };
};

const toolbar = (current: string, [rectangle, pen]: readonly [{ readonly label: string; readonly shortcut?: typeof Element.Shortcut.Type; readonly help?: string }, { readonly label: string; readonly shortcut?: typeof Element.Shortcut.Type; readonly help?: string }]) =>
    Description.make({
        containers: [
            {
                locator: TOOLBAR,
                kind: 'toolbar',
                controls: [
                    {
                        locator: [...TOOLBAR, ['choice', 'tool']],
                        kind: 'choice',
                        role: 'tool',
                        value: { _tag: 'text', value: current },
                        options: [
                            { ...rectangle, locator: RECTANGLE },
                            { ...pen, locator: PEN },
                        ],
                    },
                ],
            },
        ],
    });
const optionLabels = ({ containers }: Description) => Array.flatMap(containers, ({ controls }) => Array.flatMap(controls, ({ options = [] }) => Array.map(options, ({ label }) => label)));
const optionFacts = ({ containers }: Description) => Array.flatMap(containers, ({ controls }) => Array.flatMap(controls, ({ options = [] }) => Array.map(options, (option) => Struct.omit(option, ['locator']))));
const stored = (locator: string) => Effect.flatMap(SqlClient.SqlClient, (sql) => sql`SELECT body ->> '$.container.tally.last' AS last FROM containers WHERE locator = ${locator}`);

// --- [COMPOSITION] ---------------------------------------------------------------------

describe(InterfaceMap, () => {
    it.effect.prop(
        'keeps what each read showed, retires only what the rule names, holds a returning entry persistent, and stores no document name or user name',
        Truth,
        Effect.fnUntraced(function* (truth) {
            const world = app(truth);
            const reads = yield* Effect.forEach(world.readings, (read) => Schema.decodeEffect(Element.Observation)(world.observation(read)));
            const states = Array.scan(reads, [[], InterfaceMap.empty] as ReturnType<InterfaceMap['fold']>, ([, map], read) => map.fold(read, world.user));
            const allChanges = states.flatMap(([changes]) => changes);
            const final = Array.lastNonEmpty(world.readings);
            const [, last] = Array.lastNonEmpty(states);
            const controls = last.containers[world.window]?.controls ?? {};
            assert.isTrue(Record.has(controls, TOGGLE), 'the reads reached the stored window');
            const held = (read: number, key: string) => states[read]?.[1].containers[world.window]?.controls[key];
            const returning = (index: number, key: string) => {
                for (const read of world.readings) {
                    const [before, after] = [held(read, key), held(read + 1, key)];
                    if (before !== undefined && Option.isSome(before.retired) && world.present(read, index)) {
                        assert.isTrue(after !== undefined && Option.isNone(after.retired) && after.tally.persistent, `${key} shown after retirement returns persistent`);
                        assert.isTrue(
                            states[read + 1]?.[0].some((change) => change._tag === 'Appeared' && change.at === key),
                            `${key} appears again on its return`,
                        );
                    }
                    if (before?.tally.persistent === true && Option.isNone(before.retired) && after !== undefined && Option.isSome(after.retired)) {
                        assert.isFalse(Option.contains(before.tally.verified, world.build(read)), `${key} held persistent retires only on a build it was not seen in`);
                    }
                }
            };
            const settled = (index: number) => {
                const key = world.stored(index);
                const { condition, sightings, retirement, epochs } = world.oracle(index);
                const entry = controls[key];
                const live = entry !== undefined && Option.isNone(entry.retired);
                const count = (tag: ReturnType<InterfaceMap['fold']>[0][number]['_tag']) => allChanges.filter((change) => change._tag === tag && change.at === key).length;
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
            const opening = yield* Schema.decodeEffect(Element.Observation)(world.submenu(0));
            const menus = yield* Effect.forEach(world.readings, (read) => Effect.all([Schema.decodeEffect(Element.Observation)(world.level(read, read)), Schema.decodeEffect(Element.Observation)(world.level(read, read + 0.5))]));
            const leveled = menus.reduce((map, [first, again], read) => {
                const [, once] = map.fold(first, world.user);
                const [changes, twice] = once.fold(again, world.user);
                assert.deepStrictEqual(changes, [], 'a menu level read twice in one opening changes nothing');
                assert.deepStrictEqual(Record.keys(twice.containers).toSorted(), Record.keys(once.containers).toSorted(), 'a menu level read twice stores one structure');
                assert.deepStrictEqual(Record.map(twice.containers[MENU]?.controls ?? {}, Struct.omit(['tally'])), Record.map(once.containers[MENU]?.controls ?? {}, Struct.omit(['tally'])), 'a second read keeps every item as the first stored it');
                return read === 0 ? twice.fold(opening, world.user)[1] : twice;
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

    it('learns the label a host source gives its selected option under the newest build, serves it while the source lists the option under another, and serves nothing a build before the newest learned', () => {
        const first = new InterfaceMap({ application: { builds: [BUILDS[0]], options: {}, cursors: {} }, containers: {} });
        const learned = first.learn(toolbar('Rectangle Tool', [{ label: 'Rectangle Tool' }, { label: 'PEN_TOOL' }]));
        const served = learned[1].learn(toolbar('Pen Tool', [{ label: 'RECTANGLE_TOOL' }, { label: 'Pen Tool' }]));
        const again = served[1].learn(toolbar('Pen Tool', [{ label: 'RECTANGLE_TOOL' }, { label: 'Pen Tool' }]));
        const updated = new InterfaceMap({ application: { builds: [BUILDS[1], BUILDS[0]], options: served[1].application.options, cursors: {} }, containers: {} }).learn(toolbar('SELECTION_TOOL', [{ label: 'RECTANGLE_TOOL' }, { label: 'PEN_TOOL' }]));
        assert.deepStrictEqual(
            { learned: optionLabels(learned[0]), served: optionLabels(served[0]), updated: optionLabels(updated[0]), kept: again[1] === served[1], unlearned: first.learn(toolbar('SELECTION_TOOL', [{ label: 'RECTANGLE_TOOL' }, { label: 'PEN_TOOL' }]))[1] === first },
            { learned: ['Rectangle Tool', 'PEN_TOOL'], served: ['Rectangle Tool', 'Pen Tool'], updated: ['RECTANGLE_TOOL', 'PEN_TOOL'], kept: true, unlearned: true },
        );
    });

    it('learns the shortcut and help beside the label a host source gives its selected option, and serves all three while the source lists the option with its id alone', () => {
        const pen = { label: 'Pen Tool', shortcut: { key: 'p', modifiers: [] }, help: 'Click to add an anchor point, drag to pull its direction handles' } as const;
        const learned = new InterfaceMap({ application: { builds: [BUILDS[0]], options: {}, cursors: {} }, containers: {} }).learn(toolbar('Pen Tool', [{ label: 'RECTANGLE_TOOL' }, pen]));
        const served = learned[1].learn(toolbar('RECTANGLE_TOOL', [{ label: 'RECTANGLE_TOOL' }, { label: 'PEN_TOOL' }]));
        assert.deepStrictEqual<unknown>({ learned: optionFacts(learned[0]), served: optionFacts(served[0]) }, { learned: [{ label: 'RECTANGLE_TOOL' }, pen], served: [{ label: 'RECTANGLE_TOOL' }, pen] });
    });

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
        'writes each changed container as its change folds, a re-read included, and leaves the opener unwritten when a reopening finds its link held',
        Effect.fnUntraced(function* () {
            const [opened, observed] = yield* Effect.all([store, newDocument]);
            const again = observed.map((read) => ({ ...read, at: DateTime.add(read.at, { minutes: 1 }) }));
            const reopening = Array.drop(observed, 1).map((read) => ({ ...read, opened: DateTime.add(read.at, { minutes: 2 }), at: DateTime.add(read.at, { minutes: 2 }) }));
            const between = yield* Effect.scoped(
                Effect.gen(function* () {
                    const map = yield* InterfaceMap.open('com.adobe.InDesign', 'en');
                    yield* Effect.forEach(observed, map.observe);
                    const first = yield* Effect.all({ layout: stored(LAYOUT), dialog: stored(DIALOG) });
                    yield* Effect.forEach(again, map.observe);
                    const [reread, linked] = yield* Effect.all([Effect.all({ layout: stored(LAYOUT), dialog: stored(DIALOG) }), map.present]);
                    yield* Effect.forEach(reopening, map.observe);
                    const reopened = yield* map.present;
                    return { first, reread, opener: linked.containers[LAYOUT], held: reopened.containers[LAYOUT] };
                }),
            ).pipe(Effect.provide(opened));
            const closed = yield* Effect.all({ layout: stored(LAYOUT), dialog: stored(DIALOG) }).pipe(Effect.provide(Layer.fresh(opened)));
            assert.deepStrictEqual(
                { first: between.first, reread: between.reread, closed },
                {
                    first: { layout: [{ last: instant(0) }], dialog: [{ last: instant(1) }] },
                    reread: { layout: [{ last: instant(1) }], dialog: [{ last: instant(2) }] },
                    closed: { layout: [{ last: instant(1) }], dialog: [{ last: instant(3) }] },
                },
                'each change reaches the store as it folds, with the map open and after its close',
            );
            assert.deepStrictEqual(between.opener?.controls[NEW]?.opens, Option.some(DIALOG), 'the opener links the dialog it opened');
            assert.strictEqual(between.held, between.opener, 'a reopening whose opener already links it leaves the opener as it was');
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
            assert.deepStrictEqual(
                (yield* TestConsole.logLines).filter((line) => Predicate.isObject(line) && !Predicate.isError(line)),
                Array.replicate({ bundle: 'com.adobe.InDesign', language: 'en', table: 'containers', locator: DIALOG }, 2),
                'both opens before the rewrite log one line naming the dropped row by its table and key, the open after it none',
            );
            assert.deepStrictEqual(rewritten.containers[DIALOG]?.controls[UNITS]?.label, Option.some({ text: 'Units', attribute: 'AXTitle' }), 'the dialog sighted again reads back with its label in the current shape');
        }, Effect.provide(services)),
    );
    it.effect(
        'starts a write at the route that last passed read-back in this build, keeps every route that failed there, and starts over once the build changes',
        Effect.fnUntraced(function* () {
            const observed = yield* newDocument;
            const read = observed.reduce((map, each) => map.fold(each, OWNER)[1], InterfaceMap.empty);
            const at = { container: DIALOG, control: UNITS };
            const passed = read.prove({
                at,
                from: 'accessibility',
                outcome: Result.succeed({
                    proof: 'change',
                    route: { _tag: 'window', number: 7 },
                    tried: [
                        { route: 'accessibility', failure: ApplicationFailure.of('unchanged', 'value') },
                        { route: 'process', failure: ApplicationFailure.of('unchanged', 'value') },
                    ],
                    popUps: [],
                }),
            });
            const failed = passed.prove({ at, from: 'window', outcome: Result.fail(new Error('every route from window failed read-back')) });
            const dialog = yield* Effect.fromOption(Array.last(observed));
            const [, rebuilt] = passed.fold({ ...dialog, build: Option.some(BUILDS[1]), opened: DateTime.add(dialog.at, { minutes: 5 }), at: DateTime.add(dialog.at, { minutes: 5 }) }, OWNER);
            const failures = (map: InterfaceMap) =>
                Record.get(map.containers, DIALOG).pipe(
                    Option.flatMap(({ controls }) => Record.get(controls, UNITS)),
                    Option.flatMap(({ route }) => route),
                    Option.match({ onNone: (): readonly string[] => [], onSome: (route) => route.failed }),
                );
            assert.deepStrictEqual({ unread: read.route(at), passed: passed.route(at), failed: failed.route(at), rebuilt: rebuilt.route(at) }, { unread: Option.none(), passed: Option.some('window'), failed: Option.none(), rebuilt: Option.none() });
            assert.deepStrictEqual({ passed: failures(passed), failed: failures(failed) }, { passed: ['accessibility', 'process'], failed: ['accessibility', 'process', 'window', 'focused', 'system'] });
            assert.strictEqual(passed.prove({ at: { container: DIALOG, control: '["AXButton","Missing"]' }, from: 'accessibility', outcome: Result.fail(new Error('gone')) }), passed, 'a proof for a control the map never read leaves the map as it was');
        }),
    );
    it.effect(
        'learns no route from a step whose Accessibility call went unanswered, whose instance exited, or whose surface already stood',
        Effect.fnUntraced(function* () {
            const observed = yield* newDocument;
            const read = observed.reduce((map, each) => map.fold(each, OWNER)[1], InterfaceMap.empty);
            const at = { container: DIALOG, control: UNITS };
            const unproven = (['unanswered', 'exit', 'standing'] as const).map((proof) => read.prove({ at, from: 'accessibility', outcome: Result.succeed({ proof, tried: [], popUps: [] }) }));
            assert.deepStrictEqual(unproven, [read, read, read]);
        }),
    );

    it.effect(
        'serves a menu level only while the item opening it read it in the current build, and states the containers verified in this build with the labels Accessibility read for their menu items, never one text recognition read',
        Effect.fnUntraced(function* () {
            const menu = yield* Schema.decodeEffect(Element.Observation)({
                build: BUILDS[0],
                container: MENU,
                kind: 'menu',
                modal: false,
                opened: instant(2),
                at: instant(2),
                truncated: false,
                documents: [],
                opener: { container: LAYOUT, control: NEW },
                conditions: [],
                controls: {
                    [RECENT]: { kind: 'item', role: 'AXMenuItem', label: { text: 'Recent', attribute: 'AXTitle' }, positional: false },
                    [CLOSE]: { kind: 'item', role: 'AXMenuItem', label: { text: 'Close', attribute: 'AXTitle' }, positional: false },
                    [CLOSE_ALL]: { kind: 'item', role: 'AXMenuItem', label: { text: 'C1ose A11', attribute: 'recognized' }, positional: false },
                },
                layout: {},
                values: {},
            });
            const layout = yield* Effect.fromOption(Array.head(yield* newDocument));
            const read = [layout, menu].reduce((map, each) => map.fold(each, OWNER)[1], InterfaceMap.empty);
            const [, later] = read.fold({ ...layout, build: Option.some(BUILDS[1]), opened: DateTime.add(layout.at, { minutes: 9 }), at: DateTime.add(layout.at, { minutes: 9 }) }, OWNER);
            assert.deepStrictEqual(Option.map(read.level(NEW), Struct.get('locator')), Option.some(MENU), 'the level its opener read in this build is served');
            assert.isTrue(Option.isNone(read.level(RECENT)), 'an item that opened nothing serves no level');
            assert.isTrue(Option.isNone(later.level(NEW)), 'a level read under an earlier build is read again');
            assert.deepStrictEqual(
                Option.map(read.known, ({ build, containers, labels }) => ({ build, containers: containers.toSorted(), labels: labels.toSorted() })),
                Option.some({ build: Option.some(BUILDS[0]), containers: [LAYOUT, MENU].toSorted(), labels: ['Close', 'Recent'] }),
            );
            assert.deepStrictEqual(Option.map(later.known, Struct.pick(['containers', 'labels'])), Option.some({ containers: [LAYOUT], labels: [] }), 'a new build verifies only what it read again, and holds no label of a menu it has not read');
        }),
    );

    it.effect(
        'reports a container read for the first time and each control it holds as appeared, and a later read by each control that appeared, changed, was relabelled, or went',
        Effect.fnUntraced(function* () {
            const observed = yield* newDocument;
            const [layout, dialog] = yield* Effect.all([Effect.fromOption(Array.get(observed, 0)), Effect.fromOption(Array.get(observed, 1))]);
            const [first, map] = InterfaceMap.empty.fold(dialog, OWNER);
            const [relabelled] = map.fold({ ...dialog, at: DateTime.add(dialog.at, { seconds: 30 }), controls: Record.map(dialog.controls, (control, at) => (at === UNITS ? { ...control, label: Option.some({ text: 'Unit', attribute: 'AXTitle' }) } : control)) }, OWNER);
            const [again] = map.fold({ ...dialog, at: DateTime.add(dialog.at, { seconds: 30 }) }, OWNER);
            const [window] = InterfaceMap.empty.fold(layout, OWNER);
            assert.deepStrictEqual(
                { first, relabelled, again, window },
                {
                    first: [
                        { _tag: 'Appeared', at: DIALOG },
                        { _tag: 'Appeared', at: UNITS },
                        { _tag: 'Appeared', at: WIDTH },
                    ],
                    relabelled: [{ _tag: 'Relabelled', at: UNITS, from: Option.some('Units'), to: Option.some('Unit') }],
                    again: [],
                    window: [
                        { _tag: 'Appeared', at: LAYOUT },
                        { _tag: 'Appeared', at: NEW },
                    ],
                },
            );
        }),
    );

    it.effect(
        'counts the containers of the newest build by kind, and lists each opener it never saw open until the container it opens is read',
        Effect.fnUntraced(function* () {
            const [, dialog] = yield* newDocument;
            const zoom = '["AXPopUpButton","Zoom"]';
            const window = yield* Schema.decodeEffect(Element.Observation)({
                build: BUILDS[0],
                container: LAYOUT,
                kind: 'window',
                modal: false,
                opened: instant(0),
                at: instant(0),
                truncated: false,
                documents: [],
                conditions: [],
                controls: {
                    [NEW]: { kind: 'item', role: 'AXMenuItem', label: { text: 'New Document…', attribute: 'AXTitle' }, positional: false },
                    [RECENT]: { kind: 'item', role: 'AXMenuItem', label: { text: 'Recent', attribute: 'AXTitle' }, opens: SUBMENU, positional: false },
                    [zoom]: { kind: 'choice', role: 'AXPopUpButton', label: { text: 'Zoom', attribute: 'AXTitle' }, positional: false },
                    [CLOSE]: { kind: 'item', role: 'AXMenuItem', label: { text: 'Close', attribute: 'AXTitle' }, positional: false },
                },
                layout: {},
                values: {},
            });
            const [, first] = InterfaceMap.empty.fold(window, OWNER);
            const [, opened] = first.fold(yield* Effect.fromNullishOr(dialog), OWNER);
            const [, rebuilt] = opened.fold({ ...window, build: Option.some(BUILDS[1]), opened: DateTime.add(window.at, { minutes: 9 }), at: DateTime.add(window.at, { minutes: 9 }) }, OWNER);
            assert.deepStrictEqual(
                Record.map({ first, opened, rebuilt }, Struct.get('coverage')),
                {
                    first: {
                        build: Option.some(BUILDS[0]),
                        containers: { window: 1 },
                        earlier: 0,
                        openers: [
                            { container: LAYOUT, control: NEW },
                            { container: LAYOUT, control: RECENT },
                            { container: LAYOUT, control: zoom },
                        ],
                    },
                    opened: {
                        build: Option.some(BUILDS[0]),
                        containers: { window: 1, dialog: 1 },
                        earlier: 0,
                        openers: [
                            { container: LAYOUT, control: RECENT },
                            { container: LAYOUT, control: zoom },
                        ],
                    },
                    rebuilt: {
                        build: Option.some(BUILDS[1]),
                        containers: { window: 1 },
                        earlier: 1,
                        openers: [
                            { container: LAYOUT, control: RECENT },
                            { container: LAYOUT, control: zoom },
                        ],
                    },
                },
                'an item ending in an ellipsis stays unopened until the dialog it opens is read, a menu item until its menu is, a choice until its options list, and a container read only in an earlier build counts apart',
            );
            assert.deepStrictEqual(InterfaceMap.empty.coverage, { build: Option.none(), containers: {}, earlier: 0, openers: [] }, 'an application never read holds no build and no opener');
        }),
    );

    it.effect(
        'holds each build once, newest first, when an earlier build runs again',
        Effect.fnUntraced(function* () {
            const layout = yield* Effect.fromOption(Array.head(yield* newDocument));
            const read = [BUILDS[0], BUILDS[1], BUILDS[0]].reduce((map, build, minute) => map.fold({ ...layout, build: Option.some(build), opened: DateTime.add(layout.at, { minutes: minute }), at: DateTime.add(layout.at, { minutes: minute }) }, OWNER)[1], InterfaceMap.empty);
            assert.deepStrictEqual(read.application.builds, [BUILDS[0], BUILDS[1]]);
        }),
    );

    it.effect(
        'counts one epoch per opening when reads alternate between two open documents of one type',
        Effect.fnUntraced(function* () {
            const layout = yield* Effect.fromOption(Array.head(yield* newDocument));
            const read = [0, 1, 0, 1].reduce((map, opening, minute) => map.fold({ ...layout, container: DOCUMENT, opened: DateTime.add(layout.at, { minutes: opening }), at: DateTime.add(layout.at, { minutes: minute }) }, OWNER)[1], InterfaceMap.empty);
            const entry = yield* Effect.fromNullishOr(read.containers[DOCUMENT]);
            assert.deepStrictEqual({ container: entry.container.tally.seen, control: entry.controls[NEW]?.tally.seen }, { container: 2, control: 2 });
        }),
    );
});
