import { pathToFileURL } from 'node:url';
import { Array, Effect, Match, Option, Predicate, Record, Result, Schema } from 'effect';
import { HarnessError } from '#/error';

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

// --- [MODELS] --------------------------------------------------------------------------

const Module = Schema.Record(Schema.String, Schema.Unknown);
const Thrown = Schema.Struct({ name: Schema.String, message: Schema.String });

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
        onFailure: (thrown: unknown): readonly Difference[] => {
            const message = Option.getOrElse(
                Option.map(Schema.decodeUnknownOption(Thrown)(thrown), ({ name, message: text }) => `${name}: ${text}`),
                () => String(thrown),
            );
            return 'thrown' in pair.host ? differences(pair.host.thrown, message, '.thrown') : [{ path: '.thrown', host: pair.host.value, rebuilt: message }];
        },
    });
const compare = Effect.fn('compare')(function* (module: string, pairs: readonly Pair[]) {
    const loaded = yield* Effect.tryPromise({ try: (): Promise<unknown> => import(pathToFileURL(module).href), catch: HarnessError.refused(module) }).pipe(Effect.flatMap(Schema.decodeUnknownEffect(Module)));
    const exported = (fn: string) => Effect.fromOption(Option.filter(Record.get(loaded, fn), Predicate.isFunction), () => HarnessError.make({ reason: { _tag: 'Unavailable', because: `exports no function ${fn}` }, subject: module }));
    const checked = (pair: Pair) => (made: Parameters<typeof Reflect.apply>[0]) => ({ name: pair.name, fn: pair.fn, args: pair.args, differences: observed(pair)(Result.try(() => Reflect.apply(made, undefined, pair.args))) });
    const rows = yield* Effect.forEach(pairs, (pair) => Effect.map(exported(pair.fn), checked(pair)));
    const mismatches = rows.filter(({ differences: found }) => found.length > 0);
    return { cases: rows.length, agreed: rows.length - mismatches.length, mismatches };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { compare, type Pair };
