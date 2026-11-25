import { Array, Data, Equal, HashMap, HashSet, Match, Option } from 'effect';
import { type ArithmExpr, BinAritOperator, type BinaryArithm, LangVariant, UnAritOperator, type UnaryArithm, type Word } from 'sh-syntax';
import { Binding, Origin, type Scope } from '#/evaluation/value';
import type { Dialect } from '#/syntax/dialect';

// --- [TYPES] ---------------------------------------------------------------------------

type Integer = Data.TaggedEnum<{ Known: { readonly value: bigint }; Unknown: object; Refused: object }>;
type Valued = (word: Word, scope: Scope) => readonly [Option.Option<string>, Scope, ...(readonly unknown[])];
type Evaluated = readonly [Integer, Scope];

// --- [CONSTANTS] -----------------------------------------------------------------------

const Integer = Data.taggedEnum<Integer>();
const bits = 64;
const digits = '0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ@_';
const identifier = /^[A-Za-z_]\w*$/u;
const known = (value: bigint) => Integer.Known({ value: BigInt.asIntN(bits, value) });
const truth = (holds: boolean) => known(holds ? 1n : 0n);
const divided = (apply: (x: bigint, y: bigint) => bigint) => (x: bigint, y: bigint) => (y === 0n ? Integer.Refused() : known(apply(x, y)));
const binary: ReadonlyMap<number, (x: bigint, y: bigint, dialect: Dialect) => Integer> = new Map([
    [BinAritOperator.Add, (x: bigint, y: bigint) => known(x + y)],
    [BinAritOperator.Sub, (x: bigint, y: bigint) => known(x - y)],
    [BinAritOperator.Mul, (x: bigint, y: bigint) => known(x * y)],
    [BinAritOperator.Quo, divided((x, y) => x / y)],
    [BinAritOperator.Rem, divided((x, y) => x % y)],
    [
        BinAritOperator.Pow,
        (x: bigint, y: bigint, dialect: Dialect) =>
            Match.value({ negative: y < 0n, zsh: dialect.variant === LangVariant.LangZsh }).pipe(
                Match.when({ negative: true, zsh: true }, () => Integer.Unknown()),
                Match.when({ negative: true }, () => Integer.Refused()),
                Match.orElse(() => known(Array.makeBy(bits, BigInt).reduce<readonly [bigint, bigint]>(([result, square], bit) => [((y >> bit) & 1n) === 1n ? BigInt.asIntN(bits, result * square) : result, BigInt.asIntN(bits, square * square)], [1n, x])[0])),
            ),
    ],
    [BinAritOperator.Shl, (x: bigint, y: bigint) => known(x << (y & BigInt(bits - 1)))],
    [BinAritOperator.Shr, (x: bigint, y: bigint) => known(x >> (y & BigInt(bits - 1)))],
    [BinAritOperator.And, (x: bigint, y: bigint) => known(x & y)],
    [BinAritOperator.Or, (x: bigint, y: bigint) => known(x | y)],
    [BinAritOperator.Xor, (x: bigint, y: bigint) => known(x ^ y)],
    [BinAritOperator.AndArit, (x: bigint, y: bigint) => truth(x !== 0n && y !== 0n)],
    [BinAritOperator.OrArit, (x: bigint, y: bigint) => truth(x !== 0n || y !== 0n)],
    [BinAritOperator.XorBool, (x: bigint, y: bigint) => truth((x !== 0n) !== (y !== 0n))],
    [BinAritOperator.Eql, (x: bigint, y: bigint) => truth(x === y)],
    [BinAritOperator.Neq, (x: bigint, y: bigint) => truth(x !== y)],
    [BinAritOperator.Lss, (x: bigint, y: bigint) => truth(x < y)],
    [BinAritOperator.Gtr, (x: bigint, y: bigint) => truth(x > y)],
    [BinAritOperator.Leq, (x: bigint, y: bigint) => truth(x <= y)],
    [BinAritOperator.Geq, (x: bigint, y: bigint) => truth(x >= y)],
]);
const assigning: ReadonlyMap<number, number> = new Map([
    [BinAritOperator.AddAssgn, BinAritOperator.Add],
    [BinAritOperator.SubAssgn, BinAritOperator.Sub],
    [BinAritOperator.MulAssgn, BinAritOperator.Mul],
    [BinAritOperator.QuoAssgn, BinAritOperator.Quo],
    [BinAritOperator.RemAssgn, BinAritOperator.Rem],
    [BinAritOperator.PowAssgn, BinAritOperator.Pow],
    [BinAritOperator.ShlAssgn, BinAritOperator.Shl],
    [BinAritOperator.ShrAssgn, BinAritOperator.Shr],
    [BinAritOperator.AndAssgn, BinAritOperator.And],
    [BinAritOperator.OrAssgn, BinAritOperator.Or],
    [BinAritOperator.XorAssgn, BinAritOperator.Xor],
    [BinAritOperator.AndBoolAssgn, BinAritOperator.AndArit],
    [BinAritOperator.OrBoolAssgn, BinAritOperator.OrArit],
    [BinAritOperator.XorBoolAssgn, BinAritOperator.XorBool],
]);
const unary: ReadonlyMap<number, (x: bigint) => bigint> = new Map([
    [UnAritOperator.Not, (x: bigint) => (x === 0n ? 1n : 0n)],
    [UnAritOperator.BitNegation, (x: bigint) => ~x],
    [UnAritOperator.Minus, (x: bigint) => -x],
    [UnAritOperator.Plus, (x: bigint) => x],
    [UnAritOperator.Inc, (x: bigint) => x + 1n],
    [UnAritOperator.Dec, (x: bigint) => x - 1n],
]);

// --- [OPERATIONS] ----------------------------------------------------------------------

const lifted = (values: readonly Integer[], apply: (...operands: readonly bigint[]) => Integer): Integer =>
    Option.getOrElse(Array.findFirst(values, Integer.$is('Refused')), () =>
        Option.match(Option.all(values.map((value) => (value._tag === 'Known' ? Option.some(value.value) : Option.none()))), {
            onNone: () => Integer.Unknown(),
            onSome: (operands) => apply(...operands),
        }),
    );
const numeral = (text: string, dialect: Dialect): Integer => {
    const zsh = dialect.variant === LangVariant.LangZsh;
    const octal = dialect.options.octalzeroes;
    const based = /^(?<base>\d+)#(?<places>[\w@]+)$/u.exec(text)?.groups;
    const radix = Number(based?.['base'] ?? 0);
    const places = Array.fromIterable(based?.['places'] ?? '').map((digit) => digits.indexOf(radix <= 36 ? digit.toLowerCase() : digit));
    return Match.value(text).pipe(
        Match.when(
            (raw: string) => /^0[xX][\da-fA-F]+$/u.test(raw) || (zsh && /^0[bB][01]+$/u.test(raw)),
            (raw) => known(BigInt(raw)),
        ),
        Match.when(
            (raw: string) => octal && /^0\d+$/u.test(raw),
            (raw) => (/^0[0-7]+$/u.test(raw) ? known(BigInt(`0o${raw.slice(1)}`)) : Integer.Refused()),
        ),
        Match.when(
            (raw: string) => /^\d+$/u.test(raw),
            (raw) => known(BigInt(raw)),
        ),
        Match.when(
            () => based !== undefined && radix >= 2 && radix <= (zsh ? 36 : digits.length) && places.every((place) => place >= 0 && place < radix),
            () => known(places.reduce((sum, place) => sum * BigInt(radix) + BigInt(place), 0n)),
        ),
        Match.orElse(() => (based === undefined || zsh ? Integer.Unknown() : Integer.Refused())),
    );
};
const signed = (text: string, dialect: Dialect): Integer => (text.startsWith('-') ? lifted([numeral(text.slice(1), dialect)], (value) => known(-value)) : numeral(text.replace(/^\+/u, ''), dialect));
const variable = (name: string, scope: Scope, dialect: Dialect, visited: ReadonlySet<string>): Integer => {
    const followed = (value: string) =>
        Match.value(value.trim()).pipe(
            Match.when('', () => known(0n)),
            Match.when(
                (text: string) => identifier.test(text),
                (text) => (visited.has(text) ? Integer.Refused() : variable(text, scope, dialect, new Set([...visited, text]))),
            ),
            Match.orElse((text) => signed(text, dialect)),
        );
    return Binding.$match(scope.get(name), {
        Unset: () => known(0n),
        Unknown: () => Integer.Unknown(),
        Known: ({ value }) => followed(value),
        Listed: ({ items }) => (dialect.variant === LangVariant.LangZsh ? Integer.Refused() : followed(items[0] ?? '')),
    });
};
const named = (expression: ArithmExpr | null): Option.Option<string> => (expression?.type === 'Word' && expression.parts.length === 1 && expression.parts[0]?.type === 'Lit' && identifier.test(expression.parts[0].value) ? Option.some(expression.parts[0].value) : Option.none());
const joined = (start: Scope, ...ends: readonly Scope[]): Scope =>
    ends.reduce((scope, end) => HashMap.reduce(end.variables, scope, (merged, binding, name) => (Equal.equals(Option.some(binding), HashMap.get(start.variables, name)) ? merged : merged.set(name, Binding.Unknown({ origins: HashSet.make(Origin.Arithmetic()) })))), start);
const stored = (name: Option.Option<string>, [value, scope]: Evaluated): Evaluated => [value, Option.match(name, { onNone: () => scope, onSome: (target) => scope.set(target, value._tag === 'Known' ? Binding.Known({ value: `${value.value}` }) : Binding.Unknown({ origins: HashSet.make(Origin.Arithmetic()) })) })];
const evaluated = (expression: ArithmExpr | null, scope: Scope, dialect: Dialect, valued: Valued): Evaluated =>
    Option.match(Option.fromNullishOr(expression), {
        onNone: () => [known(0n), scope] as const,
        onSome: Match.type<ArithmExpr>().pipe(
            Match.discriminatorsExhaustive('type')({
                Word: (word) =>
                    Option.match(named(word), {
                        onSome: (name) => [variable(name, scope, dialect, new Set([name])), scope] as const,
                        onNone: () => {
                            const [text, next] = valued(word, scope);
                            return [Option.match(text, { onNone: () => Integer.Unknown(), onSome: (raw) => signed(raw.trim(), dialect) }), next] as const;
                        },
                    }),
                ParenArithm: ({ x }) => evaluated(x, scope, dialect, valued),
                UnaryArithm: (node) => unaried(node, scope, dialect, valued),
                BinaryArithm: (node) => binaried(node, scope, dialect, valued),
                CallArithm: () => [Integer.Unknown(), scope] as const,
                FlagsArithm: () => [Integer.Unknown(), scope] as const,
            }),
        ),
    });
const unaried = (node: UnaryArithm, scope: Scope, dialect: Dialect, valued: Valued): Evaluated => {
    const [value, next] = evaluated(node.x, scope, dialect, valued);
    const result = lifted([value], (x) =>
        known(
            Option.getOrElse(
                Option.map(Option.fromUndefinedOr(unary.get(node.op)), (apply) => apply(x)),
                () => x,
            ),
        ),
    );
    const stepping = node.op === UnAritOperator.Inc || node.op === UnAritOperator.Dec;
    return stepping ? [node.post ? value : result, stored(named(node.x), [result, next])[1]] : [result, next];
};
const binaried = (node: BinaryArithm, scope: Scope, dialect: Dialect, valued: Valued): Evaluated => {
    const [left, after] = node.op === BinAritOperator.Assgn ? ([known(0n), scope] as const) : evaluated(node.x, scope, dialect, valued);
    const right = (from: Scope) => evaluated(node.y, from, dialect, valued);
    const combined = (op: number, [value, from]: Evaluated): Evaluated => [
        lifted([left, value], (x, y) =>
            Option.getOrElse(
                Option.map(Option.fromUndefinedOr(binary.get(op)), (apply) => apply(x, y, dialect)),
                () => Integer.Unknown(),
            ),
        ),
        from,
    ];
    const branch = (runs: (x: bigint) => boolean, settled: bigint): Evaluated =>
        Integer.$match(left, {
            Refused: () => [left, after] as const,
            Unknown: () => [Integer.Unknown(), joined(after, right(after)[1])] as const,
            Known: ({ value }) => {
                const [result, end] = runs(value) ? right(after) : ([known(settled), after] as const);
                return [runs(value) ? lifted([result], (y) => truth(y !== 0n)) : result, end] as const;
            },
        });
    return Match.value(node.op).pipe(
        Match.when(BinAritOperator.Assgn, () => stored(named(node.x), right(scope))),
        Match.when(
            (op: number) => assigning.has(op),
            (op) => stored(named(node.x), combined(assigning.get(op) ?? op, right(after))),
        ),
        Match.when(BinAritOperator.Comma, () => right(after)),
        Match.when(BinAritOperator.AndArit, () => branch((x) => x !== 0n, 0n)),
        Match.when(BinAritOperator.OrArit, () => branch((x) => x === 0n, 1n)),
        Match.when(BinAritOperator.TernQuest, () => {
            const arms = node.y?.type === 'BinaryArithm' && node.y.op === BinAritOperator.TernColon ? Option.some(node.y) : Option.none();
            return Option.match(arms, {
                onNone: () => [Integer.Unknown(), after] as const,
                onSome: (colon) =>
                    Integer.$match(left, {
                        Refused: () => [left, after] as const,
                        Unknown: () => [Integer.Unknown(), joined(after, evaluated(colon.x, after, dialect, valued)[1], evaluated(colon.y, after, dialect, valued)[1])] as const,
                        Known: ({ value }) => evaluated(value === 0n ? colon.y : colon.x, after, dialect, valued),
                    }),
            });
        }),
        Match.orElse((op) => combined(op, right(after))),
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { evaluated, Integer };
