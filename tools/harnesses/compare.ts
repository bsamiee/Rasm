import { pathToFileURL } from 'node:url';
import { Array, Effect, Match, Option, Predicate, Record, Result, Schema, Struct } from 'effect';
import { HarnessError } from '#/error';

// --- [TYPES] ---------------------------------------------------------------------------

type Observed = { readonly value: unknown } | { readonly thrown: string };
interface Pair {
    readonly name: string;
    readonly fn: string;
    readonly args: readonly unknown[];
    readonly host: Observed;
}
interface Difference {
    readonly path: string;
    readonly host: unknown;
    readonly rebuilt: unknown;
}

// --- [MODELS] --------------------------------------------------------------------------

const Tolerance = Schema.Union([Schema.Finite, Schema.Record(Schema.String, Schema.Finite)]).annotate({ description: 'Relative tolerance a rebuilt module exports, whole or per function' });
const Exports = Schema.Struct({ tolerance: Schema.optionalKey(Tolerance) });
const Module = Schema.Record(Schema.String, Schema.Unknown);
const Thrown = Schema.Struct({ message: Schema.String });

// --- [OPERATIONS] ----------------------------------------------------------------------

const differences =
    (tolerance: number) =>
    (host: unknown, rebuilt: unknown, path = ''): readonly Difference[] =>
        Match.value([host, rebuilt] as const).pipe(
            Match.when([Predicate.isNumber, Predicate.isNumber], ([left, right]) => (Object.is(left, right) || (tolerance > 0 && Math.abs(left - right) <= tolerance * Math.max(1, Math.abs(left), Math.abs(right))) ? [] : [{ path, host, rebuilt }])),
            Match.when([Array.isArray, Array.isArray], ([left, right]: readonly [readonly unknown[], readonly unknown[]]) =>
                left.length === right.length ? left.flatMap((item, index) => differences(tolerance)(item, right[index], `${path}[${index}]`)) : [{ path: `${path}.length`, host: left.length, rebuilt: right.length }],
            ),
            Match.when([Predicate.isObject, Predicate.isObject], ([left, right]) => Array.union(Object.keys(left), Object.keys(right)).flatMap((key) => differences(tolerance)(left[key], right[key], `${path}.${key}`))),
            Match.orElse(() => (Object.is(host, rebuilt) ? [] : [{ path, host, rebuilt }])),
        );
const observed = (host: Observed, tolerance: number) =>
    Result.match({
        onSuccess: (value: unknown): readonly Difference[] => ('value' in host ? differences(tolerance)(host.value, value) : [{ path: '', host, rebuilt: value }]),
        onFailure: (thrown: unknown): readonly Difference[] => ('thrown' in host ? [] : [{ path: '', host: host.value, rebuilt: { thrown: Option.getOrElse(Option.map(Schema.decodeUnknownOption(Thrown)(thrown), Struct.get('message')), () => thrown) } }]),
    });
const checked = (module: string, loaded: typeof Module.Type, tolerance: (fn: string) => number) =>
    Effect.fn('checked')(function* (pair: Pair) {
        const absent = () => HarnessError.make({ reason: { _tag: 'Unavailable', because: `exports no function ${pair.fn}` }, subject: module });
        const exported = yield* Effect.fromOption(Option.filter(Record.get(loaded, pair.fn), Predicate.isFunction), absent);
        return { name: pair.name, fn: pair.fn, args: pair.args, differences: observed(pair.host, tolerance(pair.fn))(Result.try(() => Reflect.apply(exported, undefined, pair.args))) };
    });
const compare = Effect.fn('compare')(function* (module: string, pairs: readonly Pair[]) {
    const loaded = yield* Effect.tryPromise({ try: (): Promise<unknown> => import(pathToFileURL(module).href), catch: HarnessError.refused(module) }).pipe(Effect.flatMap(Schema.decodeUnknownEffect(Module)));
    const { tolerance } = yield* Schema.decodeEffect(Exports)(loaded);
    const toleranceOf = (fn: string) =>
        Option.getOrElse(
            Option.flatMap(Option.fromUndefinedOr(tolerance), (row) => (Predicate.isNumber(row) ? Option.some(row) : Record.get(row, fn))),
            () => 0,
        );
    const rows = yield* Effect.forEach(pairs, checked(module, loaded, toleranceOf));
    const mismatches = rows.filter(({ differences: found }) => found.length > 0);
    return { cases: rows.length, agreed: rows.length - mismatches.length, mismatches };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Pair };
export { compare };
