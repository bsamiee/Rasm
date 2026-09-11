// --- [TYPES] ---------------------------------------------------------------------------

type Decision<E> = { readonly kind: 'pass'; readonly e: E } | { readonly kind: 'deny'; readonly reason: string };

type Policy<E> = (e: E) => Decision<E>;

// --- [CONSTRUCTORS] --------------------------------------------------------------------

const pass = <E>(e: E): Decision<E> => ({ kind: 'pass', e });

const deny = <E>(reason: string): Decision<E> => ({ kind: 'deny', reason });

// --- [OPERATIONS] ----------------------------------------------------------------------

const when =
    <E, N extends E>(refine: (e: E) => e is N, policy: Policy<N>): Policy<E> =>
    (e: E): Decision<E> =>
        refine(e) ? policy(e) : pass(e);

const fold =
    <E>(policies: readonly Policy<E>[]): Policy<E> =>
    (e: E): Decision<E> =>
        policies.reduce<Decision<E>>((decision, policy) => (decision.kind === 'pass' ? policy(decision.e) : decision), pass(e));

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Decision, Policy };
export { deny, fold, pass, when };
