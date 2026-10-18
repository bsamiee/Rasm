// --- [TYPES] ---------------------------------------------------------------------------

type Option<A> = { readonly kind: 'some'; readonly value: A } | { readonly kind: 'none' };
type Fault = { readonly kind: 'exited'; readonly subject: string; readonly code: number; readonly output: string } | { readonly kind: 'unstarted' | 'undecoded'; readonly subject: string; readonly cause: unknown };
type Result<T> = { readonly kind: 'ok'; readonly value: T } | { readonly kind: 'fault'; readonly faults: readonly [Fault, ...Fault[]] };

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [CONSTRUCTORS]

const none: Option<never> = { kind: 'none' };
const some = <A>(value: A): Option<A> => ({ kind: 'some', value });
const ok = <T>(value: T): Result<T> => ({ kind: 'ok', value });
const fault = (...faults: readonly [Fault, ...Fault[]]): Result<never> => ({ kind: 'fault', faults });

// --- [COMBINATORS]

const map = <A, B>(result: Result<A>, f: (value: A) => B): Result<B> => (result.kind === 'ok' ? ok(f(result.value)) : result);
const bind = <A, R extends Result<unknown> | Promise<Result<unknown>>>(result: Result<A>, f: (value: A) => R): R | Result<never> => (result.kind === 'ok' ? f(result.value) : result);
const all = <T>(results: readonly Result<T>[]): Result<readonly T[]> => {
    const [first, ...rest] = results.flatMap((result) => (result.kind === 'fault' ? result.faults : []));
    return first === undefined ? ok(results.flatMap((result) => (result.kind === 'ok' ? [result.value] : []))) : fault(first, ...rest);
};

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Fault, Option, Result };
export { all, bind, fault, map, none, ok, some };
