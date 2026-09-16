// --- [TYPES] ---------------------------------------------------------------------------

type Option<A> = { readonly kind: 'some'; readonly value: A } | { readonly kind: 'none' };

type Result<T> = { readonly kind: 'ok'; readonly value: T } | { readonly kind: 'fault'; readonly reason: string };

type Decision<E> = { readonly kind: 'pass'; readonly e: E } | { readonly kind: 'deny'; readonly reason: string };

type Policy<E> = (e: E) => Decision<E>;

// --- [CONSTRUCTORS] --------------------------------------------------------------------

const none: Option<never> = { kind: 'none' };

const some = <A>(value: A): Option<A> => ({ kind: 'some', value });

const fromNullable = <A>(value: A | undefined): Option<A> => (value === undefined ? none : some(value));

const ok = <T>(value: T): Result<T> => ({ kind: 'ok', value });

const fault = <T>(reason: string): Result<T> => ({ kind: 'fault', reason });

const pass = <E>(e: E): Decision<E> => ({ kind: 'pass', e });

const deny = <E>(reason: string): Decision<E> => ({ kind: 'deny', reason });

// --- [OPERATIONS] ----------------------------------------------------------------------

const map = <A, B>(result: Result<A>, f: (value: A) => B): Result<B> => (result.kind === 'ok' ? ok(f(result.value)) : result);

const all = <R extends readonly Result<unknown>[]>(results: readonly [...R]): Result<{ readonly [K in keyof R]: R[K] extends Result<infer T> ? T : never }> => {
    const reasons = results.flatMap((result) => (result.kind === 'fault' ? [result.reason] : []));
    return reasons.length === 0
        ? ok(results.flatMap((result) => (result.kind === 'ok' ? [result.value] : [])) as { readonly [K in keyof R]: R[K] extends Result<infer T> ? T : never })
        : fault(reasons.join(', '));
};

const fold =
    <E>(policies: readonly Policy<E>[]): Policy<E> =>
    (e: E): Decision<E> =>
        policies.reduce<Decision<E>>((decision, policy) => (decision.kind === 'pass' ? policy(decision.e) : decision), pass(e));

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Decision, Option, Policy, Result };
export { all, deny, fault, fold, fromNullable, map, none, ok, pass, some };
