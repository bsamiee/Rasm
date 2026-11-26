import { Array, Data, Equal, HashMap, HashSet, Match, Option, Predicate, String } from 'effect';
import { type ArithmExpr, BinAritOperator, type BinaryArithm, LangVariant, UnAritOperator, type UnaryArithm, type Word } from 'sh-syntax';
import { Binding, binding, Origin, type Scope } from '#/evaluation/value';
import type { Dialect } from '#/syntax/dialect';

// --- [TYPES] ---------------------------------------------------------------------------

type Integer = Data.TaggedEnum<{ Known: { readonly value: bigint }; Unknown: object; Refused: object }>;
type Evaluated = readonly [Scope, Integer];
type Result = bigint | boolean | Integer;
type Apply = (x: bigint, y: bigint, zsh: boolean) => Result;
interface Operands {
    readonly dialect: Dialect;
    readonly text: (word: Word, scope: Scope) => readonly [Scope, Option.Option<string>];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const Integer = Data.taggedEnum<Integer>();
const bits = 64;
const digits = '0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ@_';
const identifier = /^[A-Za-z_]\w*$/u;
const numeral = /^(?<sign>[-+]?)(?:(?<hex>0[xX][\da-fA-F]+)|(?<binary>0[bB][01]+)|(?<octal>0[0-7]+)|(?<zeroed>0\d+)|(?<decimal>\d+)|(?<radix>\d+)#(?<places>[\w@]+))$/u;
const wrap = (result: Result): Integer => (Predicate.isObject(result) ? result : Integer.Known({ value: BigInt.asIntN(bits, BigInt(result)) }));
const exceed = (zsh: boolean) => (zsh ? Integer.Unknown() : Integer.Refused());
const raise: Apply = (x, y, zsh) => {
    const [power] = Array.makeBy(bits, BigInt).reduce<readonly [bigint, bigint]>(([result, square], bit) => [((y >> bit) & 1n) === 1n ? BigInt.asIntN(bits, result * square) : result, BigInt.asIntN(bits, square * square)], [1n, x]);
    return y < 0n ? exceed(zsh) : power;
};
const unary: Readonly<Record<UnAritOperator, (x: bigint) => Result>> = {
    [UnAritOperator.Not]: (x) => x === 0n,
    [UnAritOperator.BitNegation]: (x) => ~x,
    [UnAritOperator.Minus]: (x) => -x,
    [UnAritOperator.Plus]: (x) => x,
    [UnAritOperator.Inc]: (x) => x + 1n,
    [UnAritOperator.Dec]: (x) => x - 1n,
};
const binary = new Map<number, { readonly apply: Apply; readonly stores: boolean }>(
    (
        [
            [(x, y) => x + y, BinAritOperator.Add, BinAritOperator.AddAssgn],
            [(x, y) => x - y, BinAritOperator.Sub, BinAritOperator.SubAssgn],
            [(x, y) => x * y, BinAritOperator.Mul, BinAritOperator.MulAssgn],
            [(x, y) => (y === 0n ? Integer.Refused() : x / y), BinAritOperator.Quo, BinAritOperator.QuoAssgn],
            [(x, y) => (y === 0n ? Integer.Refused() : x % y), BinAritOperator.Rem, BinAritOperator.RemAssgn],
            [raise, BinAritOperator.Pow, BinAritOperator.PowAssgn],
            [(x, y) => x << (y & BigInt(bits - 1)), BinAritOperator.Shl, BinAritOperator.ShlAssgn],
            [(x, y) => x >> (y & BigInt(bits - 1)), BinAritOperator.Shr, BinAritOperator.ShrAssgn],
            [(x, y) => x & y, BinAritOperator.And, BinAritOperator.AndAssgn],
            [(x, y) => x | y, BinAritOperator.Or, BinAritOperator.OrAssgn],
            [(x, y) => x ^ y, BinAritOperator.Xor, BinAritOperator.XorAssgn],
            [(x, y) => x !== 0n && y !== 0n, BinAritOperator.AndArit, BinAritOperator.AndBoolAssgn],
            [(x, y) => x !== 0n || y !== 0n, BinAritOperator.OrArit, BinAritOperator.OrBoolAssgn],
            [(x, y) => (x !== 0n) !== (y !== 0n), BinAritOperator.XorBool, BinAritOperator.XorBoolAssgn],
            [(x, y) => x === y, BinAritOperator.Eql],
            [(x, y) => x !== y, BinAritOperator.Neq],
            [(x, y) => x < y, BinAritOperator.Lss],
            [(x, y) => x > y, BinAritOperator.Gtr],
            [(x, y) => x <= y, BinAritOperator.Leq],
            [(x, y) => x >= y, BinAritOperator.Geq],
        ] satisfies readonly (readonly [Apply, BinAritOperator, BinAritOperator?])[]
    ).flatMap(([apply, plain, ...storing]) => [[plain, { apply, stores: false }] as const, ...storing.map((op) => [op, { apply, stores: true }] as const)]),
);

// --- [OPERATIONS] ----------------------------------------------------------------------

const lift = (values: readonly Integer[], apply: (...operands: readonly bigint[]) => Result): Integer =>
    Option.getOrElse(Array.findFirst(values, Integer.$is('Refused')), () =>
        Option.match(Option.liftPredicate(values, Array.every(Integer.$is('Known'))), {
            onNone: () => Integer.Unknown(),
            onSome: (known) => wrap(apply(...known.map(({ value }) => value))),
        }),
    );
const parse = (text: string, { options, variant }: Dialect): Integer => {
    const zsh = variant === LangVariant.LangZsh;
    const { sign, hex, binary: dyadic, octal, zeroed, decimal, radix, places = '' } = numeral.exec(text)?.groups ?? {};
    const base = Number(radix);
    const values = Array.fromIterable(places).map((digit) => digits.indexOf(base <= 36 ? digit.toLowerCase() : digit));
    const magnitude = Match.value({ hex, dyadic, octal, zeroed, decimal, radix }).pipe(
        Match.when({ hex: Match.string }, (found) => wrap(BigInt(found.hex))),
        Match.when({ dyadic: Match.string }, (found) => (zsh ? wrap(BigInt(found.dyadic)) : Integer.Unknown())),
        Match.when({ octal: Match.string }, (found) => wrap(BigInt(options.octalzeroes ? `0o${found.octal.slice(1)}` : found.octal))),
        Match.when({ zeroed: Match.string }, (found) => (options.octalzeroes ? Integer.Refused() : wrap(BigInt(found.zeroed)))),
        Match.when({ decimal: Match.string }, (found) => wrap(BigInt(found.decimal))),
        Match.when({ radix: Match.undefined }, () => Integer.Unknown()),
        Match.orElse(() => (base >= 2 && base <= (zsh ? 36 : digits.length) && values.every((place) => place >= 0 && place < base) ? wrap(values.reduce((sum, place) => sum * BigInt(base) + BigInt(place), 0n)) : exceed(zsh))),
    );
    return sign === '-' ? lift([magnitude], (value) => -value) : magnitude;
};
const read = (text: string, scope: Scope, dialect: Dialect, visited: HashSet.HashSet<string>): Integer => {
    const follow = (value: string) => (HashSet.has(visited, text) ? Integer.Refused() : read(value.trim(), scope, dialect, HashSet.add(visited, text)));
    return Match.value(text).pipe(
        Match.when('', () => wrap(0n)),
        Match.when(
            (written) => identifier.test(written),
            (held) =>
                Binding.$match(scope.get(held), {
                    Unset: () => wrap(0n),
                    Unknown: () => Integer.Unknown(),
                    Known: ({ value }) => follow(value),
                    Listed: ({ items }) => (dialect.variant === LangVariant.LangZsh ? Integer.Refused() : follow(items[0] ?? '')),
                }),
        ),
        Match.orElse((written) => parse(written, dialect)),
    );
};
const name = (expression: ArithmExpr | null): Option.Option<string> => Option.flatMap(Option.liftPredicate(expression?.type === 'Word' ? expression.parts : [], Predicate.isTupleOf(1)), ([part]) => (part.type === 'Lit' && identifier.test(part.value) ? Option.some(part.value) : Option.none()));
const join = (start: Scope, ends: readonly Scope[]): Scope => ends.reduce((scope, end) => HashMap.reduce(end.variables, scope, (merged, value, held) => (Equal.equals(Option.some(value), HashMap.get(start.variables, held)) ? merged : merged.set(held, binding(Option.none(), [Origin.Arithmetic()])))), start);
const store = (target: Option.Option<string>, [scope, value]: Evaluated): Evaluated => {
    const text = Option.map(Option.liftPredicate(value, Integer.$is('Known')), (known) => `${known.value}`);
    return [Option.match(target, { onNone: () => scope, onSome: (held) => scope.set(held, binding(text, [Origin.Arithmetic()])) }), value];
};
const evaluate = (expression: ArithmExpr | null, scope: Scope, at: Operands): Evaluated =>
    Option.match(Option.fromNullOr(expression), {
        onNone: () => [scope, wrap(0n)] as const,
        onSome: Match.type<ArithmExpr>().pipe(
            Match.discriminatorsExhaustive('type')({
                Word: (word): Evaluated => {
                    const [next, text] = at.text(word, scope);
                    const written = Option.filter(Option.map(text, String.trim), String.isNonEmpty);
                    const quoted = word.parts.some((part) => part.type === 'SglQuoted' || (part.type === 'DblQuoted' && !at.dialect.supports('quotedarithmetic')));
                    return [next, quoted ? Integer.Refused() : Option.match(written, { onNone: () => Integer.Unknown(), onSome: (raw) => read(raw, next, at.dialect, HashSet.empty()) })];
                },
                ParenArithm: ({ x }) => evaluate(x, scope, at),
                UnaryArithm: (node) => step(node, scope, at),
                BinaryArithm: (node) => combine(node, scope, at),
                CallArithm: () => [scope, Integer.Unknown()] as const,
                FlagsArithm: () => [scope, Integer.Unknown()] as const,
            }),
        ),
    });
const step = (node: UnaryArithm, scope: Scope, at: Operands): Evaluated => {
    const [next, value] = evaluate(node.x, scope, at);
    const result = lift([value], unary[node.op]);
    return node.op === UnAritOperator.Inc || node.op === UnAritOperator.Dec ? [store(name(node.x), [next, result])[0], node.post ? value : result] : [next, result];
};
const combine = (node: BinaryArithm, scope: Scope, at: Operands): Evaluated => {
    const zsh = at.dialect.variant === LangVariant.LangZsh;
    const [after, left] = node.op === BinAritOperator.Assgn ? ([scope, wrap(0n)] as const) : evaluate(node.x, scope, at);
    const right = (from: Scope) => evaluate(node.y, from, at);
    const choose = (branches: readonly (ArithmExpr | null)[], taken: (value: bigint) => Evaluated): Evaluated =>
        Integer.$match(left, {
            Refused: () => [after, left] as const,
            Unknown: () => {
                const ends = branches.map((branch) => evaluate(branch, after, at)[0]);
                return [join(after, ends), left] as const;
            },
            Known: ({ value }) => taken(value),
        });
    const short = (needs: boolean) =>
        choose([node.y], (value): Evaluated => {
            const [end, result] = (value !== 0n) === needs ? right(after) : [after, wrap(!needs)];
            return [end, lift([result], (y) => y !== 0n)];
        });
    const arms = Option.filter(Option.fromNullOr(node.y), (colon): colon is BinaryArithm => colon.type === 'BinaryArithm' && colon.op === BinAritOperator.TernColon);
    return Match.value(node.op).pipe(
        Match.when(BinAritOperator.Assgn, () => store(name(node.x), right(scope))),
        Match.when(BinAritOperator.Comma, () => right(after)),
        Match.when(BinAritOperator.AndArit, () => short(true)),
        Match.when(BinAritOperator.OrArit, () => short(false)),
        Match.when(BinAritOperator.TernQuest, () =>
            Option.match(arms, {
                onNone: () => [after, Integer.Unknown()] as const,
                onSome: (colon) => choose([colon.x, colon.y], (value) => evaluate(value === 0n ? colon.y : colon.x, after, at)),
            }),
        ),
        Match.orElse((op): Evaluated => {
            const { apply, stores } = binary.get(op) ?? { apply: () => Integer.Unknown(), stores: false };
            const [end, value] = right(after);
            const result = [end, lift([left, value], (x, y) => apply(x, y, zsh))] as const;
            return stores ? store(name(node.x), result) : result;
        }),
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { evaluate, Integer };
