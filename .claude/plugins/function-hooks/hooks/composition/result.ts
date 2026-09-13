// --- [TYPES] ---------------------------------------------------------------------------

type Result<T> = { readonly kind: 'ok'; readonly value: T } | { readonly kind: 'fault'; readonly reason: string };

// Value of each result by position, a tuple keeps each member's type
type Values<R extends readonly Result<unknown>[]> = { readonly [K in keyof R]: R[K] extends Result<infer T> ? T : never };

// --- [CONSTRUCTORS] --------------------------------------------------------------------

const ok = <T>(value: T): Result<T> => ({ kind: 'ok', value });

const fault = <T>(reason: string): Result<T> => ({ kind: 'fault', reason });

// --- [OPERATIONS] ----------------------------------------------------------------------

const map = <A, B>(result: Result<A>, f: (value: A) => B): Result<B> => (result.kind === 'ok' ? ok(f(result.value)) : result);

const bind = <A, B>(result: Result<A>, f: (value: A) => Result<B>): Result<B> => (result.kind === 'ok' ? f(result.value) : result);

// Array methods over a tuple answer an array, the assertion restores the positions Values names
const all = <R extends readonly Result<unknown>[]>(results: readonly [...R]): Result<Values<R>> => {
    const reasons = results.flatMap((result) => (result.kind === 'fault' ? [result.reason] : []));
    return reasons.length === 0
        ? ok(results.flatMap((result) => (result.kind === 'ok' ? [result.value] : [])) as Values<R>)
        : fault(reasons.join(', '));
};

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Result };
export { all, bind, fault, map, ok };
