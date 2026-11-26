import { assert, describe, it } from '@effect/vitest';
import { Address, InterfaceMap, Model, Outline } from '@rasm/applications';
import { Array, Effect, Option, Record, Result, Schema, Struct } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Index = ReturnType<typeof Address.index>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const CONTINUATION = /^(?<indent> *)\+(?<count>\d+) more · inspect \{(?<call>.*)\}$/u;
const ARGUMENT = /(?<key>in|after):(?<value>"(?:[^"\\]|\\.)*")/gu;
const ALTERNATE = /alternate \((?<token>@\p{Ll}\p{Nd}+)/gu;
const DEPTH = 1000;

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

describe(Outline.render, () => {
    it.effect.prop(
        'joins the pages each continuation names into the unbounded render, and every reference a line leads with resolves to the thing it prints',
        Generated,
        Effect.fnUntraced(function* ({ model: generated, copies, page: characters }) {
            const model = interned(generated, copies);
            const index = Address.index(model, InterfaceMap.empty);
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
