import { assert, describe, it } from '@effect/vitest';
import { Address } from '@rasm/applications/address';
import { Model, Observation } from '@rasm/applications/contract';
import { InterfaceMap } from '@rasm/applications/interface-map';
import { Outline } from '@rasm/applications/outline';
import { Array, Effect, Option, Record, Result, Schema, Struct } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Index = ReturnType<typeof Address.index>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const CONTINUATION = /^(?<indent> *)\+(?<count>\d+) more · inspect \{(?<call>.*)\}$/u;
const ARGUMENT = /(?<key>in|after):(?<value>"(?:[^"\\]|\\.)*")/gu;
const ALTERNATE = /alternate \((?<token>@\p{Ll}\p{Nd}+)/gu;
const DEPTH = 1000;
const BRUSH = { key: 'b', modifiers: ['shift'] } as const;
const TOOLS: Model = {
    sequence: 1,
    containers: [
        {
            ref: 1,
            locator: '[["toolbar","tool"]]',
            kind: 'toolbar',
            modal: false,
            asking: false,
            text: [],
            truncated: false,
            controls: [
                {
                    ref: 2,
                    locator: '[["toolbar","tool"],["choice","tool"]]',
                    kind: 'choice',
                    role: 'tool',
                    positional: false,
                    label: { text: 'tool' },
                    value: { _tag: 'text', value: 'Eraser Tool' },
                    options: [
                        { ref: 3, label: 'Brush Tool', shortcut: BRUSH, help: 'Click and drag to paint with the foreground color' },
                        { ref: 4, label: 'Eraser Tool' },
                    ],
                },
            ],
        },
        {
            ref: 5,
            locator: '[["AXMenu","Window"]]',
            kind: 'menu',
            modal: false,
            asking: false,
            text: [],
            truncated: false,
            controls: [{ ref: 6, locator: '[["AXMenu","Window"],["AXMenuItem","Brushes"]]', kind: 'item', role: 'AXMenuItem', positional: false, label: { text: 'Brushes', attribute: 'AXTitle' }, shortcut: BRUSH }],
        },
    ],
};

// --- [MODELS] --------------------------------------------------------------------------

const Quoted = Schema.fromJsonString(Schema.String);
const Generated = {
    model: Model,
    copies: Schema.Int.check(Schema.isBetween({ minimum: 1, maximum: 4 })).annotate({ description: 'Each control repeated in place, so siblings that differ by reference alone occur' }),
    page: Schema.Int.check(Schema.isBetween({ minimum: 60, maximum: 2400 })).annotate({ description: 'Page in characters, small enough that generated models cut at every level' }),
};

// --- [OPERATIONS] ----------------------------------------------------------------------

const among = <A>(held: readonly A[], given: number | undefined) => Option.flatMap(Option.fromUndefinedOr(given), (at) => Array.get(held, at % Math.max(held.length, 1)));
const interned = (model: Model, copies: number): Model => {
    const [, numbered] = Array.mapAccum(model.containers, 1, (next, container) => {
        const [after, controls] = Array.mapAccum(
            container.controls.flatMap((control) => Array.replicate(control, copies)),
            next + 1,
            (ref, control) => {
                const [last, options] = Array.mapAccum(control.options ?? [], ref + 1, (option, each) => [option + 1, { ...each, ref: option }] as const);
                return [last, { ...control, ref, ...(control.options === undefined ? {} : { options }) }] as const;
            },
        );
        return [after, { ...container, ref: next, controls }] as const;
    });
    return {
        ...model,
        containers: numbered.map(({ parent, default: accept, cancel, close, ...container }, position) => ({
            ...container,
            ...Record.getSomes({
                parent: Option.map(among(Array.take(numbered, position), parent), Struct.get('ref')),
                default: Option.map(among(container.controls, accept), Struct.get('ref')),
                cancel: Option.map(among(container.controls, cancel), Struct.get('ref')),
                close: Option.map(among(container.controls, close), Struct.get('ref')),
            }),
        })),
    };
};
const page = (index: Index, call: string, budget: typeof Outline.budget) => {
    const given = Object.fromEntries(call.matchAll(ARGUMENT).map(({ groups }) => [groups?.['key'] ?? '', Option.getOrElse(Schema.decodeUnknownOption(Quoted)(groups?.['value']), () => '')] as const));
    const target = (key: string) => Option.map(Option.fromUndefinedOr(given[key]), (at) => Result.getOrThrowWith(Address.resolve(index, at), (refused) => new Error(`${call} names ${at}, which resolves to nothing: ${refused.message}`)));
    return Result.getOrThrowWith(Outline.render(index, { in: target('in'), after: target('after'), budget }), (refused) => new Error(`${call} renders nothing: ${refused.message}`));
};
const items = (lines: readonly string[], indent: number) =>
    lines.reduce((count, line) => {
        const [depth, cut] = [line.length - line.trimStart().length, CONTINUATION.exec(line)?.groups?.['count']];
        return depth === indent ? count + (cut === undefined ? 1 : Number(cut)) : count;
    }, 0);
const spliced = (index: Index, lines: readonly string[], budget: typeof Outline.budget, depth: number): readonly string[] =>
    lines.flatMap((line) =>
        Option.match(Option.fromNullishOr(CONTINUATION.exec(line)?.groups), {
            onNone: () => [line],
            onSome: ({ indent = '', count = '0', call = '' }) => {
                assert.isBelow(depth, DEPTH, `${line} continues past ${DEPTH} nested pages`);
                const next = page(index, call, budget);
                assert.strictEqual(items(next, indent.length), Number(count), `${line} counts the items its page holds:\n${next.join('\n')}`);
                return spliced(index, next, budget, depth + 1);
            },
        }),
    );

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('Outline.render of a tool choice', () => {
    it.effect(
        'prints each option with the shortcut a menu item of the same key step prints and its help, and an option stating neither with its label alone',
        Effect.fnUntraced(function* () {
            const lines = (yield* Effect.fromResult(Outline.render(Address.index(TOOLS)))).map((line) => line.trim());
            const item = yield* Effect.fromOption(Array.findFirst(lines, (line) => line.startsWith('@i6 ')));
            assert.deepStrictEqual(
                { brush: lines.find((line) => line.startsWith('@o3 ')), eraser: lines.find((line) => line.startsWith('@o4 ')), item },
                {
                    brush: '@o3 Brush Tool · shortcut {key:"b",modifiers:["shift"]} · help "Click and drag to paint with the foreground color"',
                    eraser: '@o4 Eraser Tool · selected',
                    item: '@i6 Brushes item · shortcut {key:"b",modifiers:["shift"]}',
                },
            );
        }),
    );
});

describe(Address.recall, () => {
    it.effect(
        'keeps remembered containers off the live page, and find over the recalled index names the live item that opens each',
        Effect.fnUntraced(function* () {
            const brushes = yield* Schema.decodeEffect(Observation)({
                build: '27.1',
                container: '[["AXWindow","Brushes"]]',
                kind: 'panel',
                title: 'Brushes',
                modal: false,
                opened: '2026-11-26T09:00:00Z',
                at: '2026-11-26T09:00:00Z',
                truncated: false,
                documents: [],
                opener: { container: '[["AXMenu","Window"]]', control: '[["AXMenu","Window"],["AXMenuItem","Brushes"]]' },
                conditions: [],
                controls: { '[["AXWindow","Brushes"],["AXSlider","Size"]]': { kind: 'number', role: 'AXSlider', label: { text: 'Size', attribute: 'AXTitle' }, positional: false } },
                layout: {},
                values: { '[["AXWindow","Brushes"],["AXSlider","Size"]]': 12 },
            });
            const { map } = InterfaceMap.empty.fold(brushes, { username: 'owner', homedir: '/Users/owner' });
            const live = Address.index(TOOLS);
            const recalled = Address.recall(live)(map);
            const printed = yield* Effect.fromResult(Outline.render(live));
            const found = Address.find(recalled, 'Size', Option.none());
            const hits = yield* Effect.fromResult(Outline.hits(found, [], { opening: Option.some(recalled) }));
            assert.deepStrictEqual(
                {
                    remembered: live.remembered.containers.length,
                    marked: printed.filter((line) => line.trimStart().startsWith('▸')),
                    recalled: recalled.remembered.containers.map(({ token }) => token),
                    hits,
                    opening: found.map((item) => Result.map(Address.opening(recalled, item), Array.map(Struct.get('token')))),
                    located: found.map(Address.locate),
                },
                {
                    remembered: 0,
                    marked: [],
                    recalled: [Option.some('Brushes')],
                    hits: ['▸ Brushes > Size was 12 number · opened by @i6'],
                    opening: [Result.succeed([Option.some('@i6')])],
                    located: [Option.some({ container: '[["AXWindow","Brushes"]]', control: '[["AXWindow","Brushes"],["AXSlider","Size"]]' })],
                },
            );
        }),
    );
});

describe(Address.opening, () => {
    it.effect(
        'refuses a remembered control nothing shown opens, and one its open container no longer holds unless that read stopped short',
        Effect.fnUntraced(function* () {
            const brushes = yield* Schema.decodeEffect(Observation)({
                build: '27.1',
                container: '[["AXWindow","Brushes"]]',
                kind: 'panel',
                title: 'Brushes',
                modal: false,
                opened: '2026-11-26T09:00:00Z',
                at: '2026-11-26T09:00:00Z',
                truncated: false,
                documents: [],
                opener: { container: '[["AXMenu","Window"]]', control: '[["AXMenu","Window"],["AXMenuItem","Brushes"]]' },
                conditions: [],
                controls: { '[["AXWindow","Brushes"],["AXSlider","Size"]]': { kind: 'number', role: 'AXSlider', label: { text: 'Size', attribute: 'AXTitle' }, positional: false } },
                layout: {},
                values: {},
            });
            const { map } = InterfaceMap.empty.fold(brushes, { username: 'owner', homedir: '/Users/owner' });
            const reach = (model: Model) => {
                const recalled = Address.recall(Address.index(model))(map);
                return Address.find(recalled, 'Size', Option.none()).map((item) => Result.map(Address.opening(recalled, item), Array.map(Struct.get('token'))));
            };
            assert.deepStrictEqual(
                {
                    closed: reach({ sequence: 1, containers: TOOLS.containers.filter(({ kind }) => kind === 'toolbar') }),
                    ...Record.fromEntries((['whole', 'partial'] as const).map((read) => [read, reach({ sequence: 1, containers: [{ ref: 9, locator: '[["AXWindow","Brushes"]]', kind: 'panel', title: 'Brushes', modal: false, asking: false, text: [], truncated: read === 'partial', controls: [] }] })] as const)),
                },
                {
                    closed: [Result.fail('"Brushes > Size" sits in a closed panel that nothing shown now opens, as none of the 1 items the map saw open it is shown; open it, then act again')],
                    whole: [Result.fail('"Brushes > Size" is missing from the open panel "Brushes", read whole now, though the map saw it there 1×')],
                    partial: [Result.succeed([])],
                },
            );
        }),
    );
});

describe(Outline.render, () => {
    it.effect.prop(
        'joins the pages each continuation names into the unbounded render, and every reference a line leads with resolves to the thing it prints',
        Generated,
        Effect.fnUntraced(function* ({ model: generated, copies, page: characters }) {
            const model = interned(generated, copies);
            const index = Address.index(model);
            const budget = { page: characters, ceiling: characters * 2 };
            const whole = yield* Effect.fromResult(Outline.render(index, { budget: { page: Number.POSITIVE_INFINITY, ceiling: Number.POSITIVE_INFINITY } }));
            const first = yield* Effect.fromResult(Outline.render(index, { budget }));
            assert.deepStrictEqual(spliced(index, first, budget, 0), whole, `page of ${characters} characters:\n${first.join('\n')}`);
            const leading = [
                ...whole
                    .map((line) => line.trimStart().split(' ')[0] ?? '')
                    .filter((token) => token.startsWith('@'))
                    .flatMap((token) => token.split(',')),
                ...whole
                    .join('\n')
                    .matchAll(ALTERNATE)
                    .map(({ groups }) => groups?.['token'] ?? ''),
            ];
            const printed = leading.map((token) => ({ token, resolved: Address.resolve(index, token) }));
            const controls = model.containers.flatMap(Struct.get('controls'));
            assert.deepStrictEqual(printed.filter(({ token, resolved }) => !(Result.isSuccess(resolved) && Option.contains(resolved.success.token, token))).map(Struct.get('token')), [], 'every reference a line leads with or folds beside it resolves to the thing printed with it');
            assert.strictEqual(leading.length, model.containers.length + controls.length + controls.flatMap((control) => control.options ?? []).length, 'every container, control, and option prints its reference once');
        }),
        { arbitrary: { seed: 2026, runs: 200 } },
    );
});
