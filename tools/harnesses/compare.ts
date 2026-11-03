import { pathToFileURL } from 'node:url';
import { Array, Effect, Match, Option, Predicate, Record, Result, Schema } from 'effect';
import { HarnessError, thrown } from '#/error';
import type { Compare } from '#/results';
import { bury } from '#/values';

// --- [TYPES] ---------------------------------------------------------------------------

interface Pair {
    readonly name: string;
    readonly fn: string;
    readonly args: readonly unknown[];
    readonly host: { readonly value: unknown } | { readonly thrown: string };
    readonly settled: (rebuilt: unknown) => unknown;
}
interface Difference {
    readonly path: string;
    readonly host: unknown;
    readonly rebuilt: unknown;
}
type Reply = Result.Result<unknown, string>;

// --- [MODELS] --------------------------------------------------------------------------

const Module = Schema.Record(Schema.String, Schema.Unknown);

// --- [OPERATIONS] ----------------------------------------------------------------------

const differences = (host: unknown, rebuilt: unknown, path = ''): readonly Difference[] =>
    Match.value([host, rebuilt] as const).pipe(
        Match.when([Array.isArray, Array.isArray], ([left, right]: readonly [readonly unknown[], readonly unknown[]]) => (left.length === right.length ? left.flatMap((item, index) => differences(item, right[index], `${path}[${index}]`)) : [{ path: `${path}.length`, host: left.length, rebuilt: right.length }])),
        Match.when([Predicate.isObject, Predicate.isObject], ([left, right]) => Array.union(Object.keys(left), Object.keys(right)).flatMap((key) => differences(left[key], right[key], `${path}.${key}`))),
        Match.orElse(() => (Object.is(host, rebuilt) ? [] : [{ path, host, rebuilt }])),
    );
const observed = (pair: Pair) =>
    Result.match({
        onSuccess: (value: unknown): readonly Difference[] => ('value' in pair.host ? differences(pair.host.value, pair.settled(value)) : [{ path: '.thrown', host: pair.host.thrown, rebuilt: value }]),
        onFailure: (message: string): readonly Difference[] => ('thrown' in pair.host ? differences(pair.host.thrown, message, '.thrown') : [{ path: '.thrown', host: pair.host.value, rebuilt: message }]),
    });
const imported = Effect.fn('imported')(function* (module: string) {
    const loaded = yield* Effect.tryPromise({ try: (): Promise<unknown> => import(pathToFileURL(module).href), catch: HarnessError.refused(module) }).pipe(Effect.flatMap(Schema.decodeUnknownEffect(Module)));
    return Effect.fnUntraced(function* (pair: Pair) {
        const made = yield* Record.get(loaded, pair.fn).pipe(
            Option.filter(Predicate.isFunction),
            Effect.fromOption(() => HarnessError.unavailable(module)(`exports no function ${pair.fn}`)),
        );
        return yield* Effect.result(Effect.try({ try: () => made(...pair.args), catch: thrown }));
    });
});
const compared = (module: string, pairs: readonly Pair[], replies: ReadonlyMap<string, Reply>, unproven: readonly string[]): typeof Compare.Type => {
    const rows = Array.getSomes(pairs.map((pair) => Option.map(Option.fromUndefinedOr(replies.get(pair.name)), (reply) => ({ name: pair.name, fn: pair.fn, args: bury(pair.args), differences: observed(pair)(reply).map(({ path, host, rebuilt }) => ({ path, host: bury(host), rebuilt: bury(rebuilt) })) }))));
    const mismatches = rows.filter(({ differences: found }) => found.length > 0);
    return { module, agreed: rows.length - mismatches.length, mismatches, unproven: [...unproven, ...pairs.filter(({ name }) => !replies.has(name)).map(({ name }) => name)] };
};
const compare = Effect.fn('compare')(function* (module: string, pairs: readonly Pair[], unproven: readonly string[] = []) {
    const reply = yield* imported(module);
    const replies = new Map(yield* Effect.forEach(pairs, (pair) => Effect.map(reply(pair), (held) => [pair.name, held] as const)));
    return compared(module, pairs, replies, unproven);
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { compare, compared, type Pair, type Reply };
