import { pathToFileURL } from 'node:url';
import { Array, Effect, identity, Match, Option, Predicate, Record, Result, Schema, Struct } from 'effect';
import { HarnessError, thrown } from '#/error';
import type { Compare } from '#/results';
import { bury } from '#/values';

// --- [TYPES] ---------------------------------------------------------------------------

type Reply = Result.Result<unknown, string>;
type Difference = (typeof Compare.Type)['mismatches'][number]['differences'][number];
interface Pair {
    readonly name: string;
    readonly fn: string;
    readonly args: readonly unknown[];
    readonly host: Reply;
    readonly settled: (rebuilt: unknown) => unknown;
}

// --- [MODELS] --------------------------------------------------------------------------

const Module = Schema.Record(Schema.String, Schema.Unknown);

// --- [OPERATIONS] ----------------------------------------------------------------------

const differences = (host: unknown, rebuilt: unknown, path = ''): readonly Difference[] =>
    Match.value([host, rebuilt] as const).pipe(
        Match.when(
            ([left, right]) => Object.is(left, right),
            () => [],
        ),
        Match.when([Array.isArray, Array.isArray], ([left, right]: readonly [readonly unknown[], readonly unknown[]]) => Array.match(differences(left.length, right.length, `${path}.length`), { onEmpty: () => left.flatMap((item, index) => differences(item, right[index], `${path}[${index}]`)), onNonEmpty: identity })),
        Match.when([Predicate.isObject, Predicate.isObject], ([left, right]) => Array.union(Object.keys(left), Object.keys(right)).flatMap((key) => differences(left[key], right[key], `${path}.${key}`))),
        Match.orElse(() => [{ path, host: bury(host), rebuilt: bury(rebuilt) }]),
    );
const observed = (pair: Pair, reply: Reply): readonly Difference[] =>
    Match.value([pair.host, reply] as const).pipe(
        Match.when([{ _tag: 'Success' }, { _tag: 'Success' }], ([host, rebuilt]) => differences(host.success, pair.settled(rebuilt.success))),
        Match.when([{ _tag: 'Failure' }, { _tag: 'Failure' }], ([host, rebuilt]) => differences(host.failure, rebuilt.failure, '.thrown')),
        Match.orElse(([host, rebuilt]) => [{ path: '.thrown', host: bury(Result.merge(host)), rebuilt: bury(Result.merge(rebuilt)) }]),
    );
const imported = Effect.fn('imported')(function* (module: string) {
    const loaded = yield* Effect.tryPromise({ try: (): Promise<unknown> => import(pathToFileURL(module).href), catch: HarnessError.refused(module) }).pipe(Effect.flatMap(Schema.decodeUnknownEffect(Module)));
    return (pair: Pair): Effect.Effect<Reply, HarnessError> =>
        Option.match(Option.filter(Record.get(loaded, pair.fn), Predicate.isFunction), {
            onNone: () => Effect.fail(HarnessError.unavailable(module)(`exports no function ${pair.fn}`)),
            onSome: (made) => Effect.result(Effect.try({ try: () => made(...pair.args), catch: thrown })),
        });
});
const compared = (module: string, pairs: readonly Pair[], replies: ReadonlyMap<string, Reply>, unproven: readonly string[]): typeof Compare.Type => {
    const rows = Array.getSomes(pairs.map((pair) => Option.map(Option.fromUndefinedOr(replies.get(pair.name)), (reply) => ({ name: pair.name, fn: pair.fn, args: bury(pair.args), differences: observed(pair, reply) }))));
    const mismatches = rows.filter(({ differences: found }) => found.length > 0);
    return { module, agreed: rows.length - mismatches.length, mismatches, unproven: [...unproven, ...pairs.filter(({ name }) => !replies.has(name)).map(({ name }) => name)] };
};
const compare = Effect.fn('compare')(function* (module: string, pairs: readonly Pair[], unproven: readonly string[] = []) {
    const reply = yield* imported(module);
    const replies = new Map(Array.zip(pairs.map(Struct.get('name')), yield* Effect.forEach(pairs, reply)));
    return compared(module, pairs, replies, unproven);
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { compare, compared, type Pair, type Reply };
