// --- [TYPES] ---------------------------------------------------------------------------

type Decision<E> = { readonly kind: 'pass'; readonly e: E } | { readonly kind: 'deny'; readonly reason: string };

type Rule<E> = (e: E) => Decision<E>;

// --- [CONSTRUCTORS] --------------------------------------------------------------------

const pass = <E>(e: E): Decision<E> => ({ kind: 'pass', e });

const deny = <E>(reason: string): Decision<E> => ({ kind: 'deny', reason });

// --- [OPERATIONS] ----------------------------------------------------------------------

const when =
    <E, N extends E>(refine: (e: E) => e is N, rule: Rule<N>): Rule<E> =>
    (e: E): Decision<E> =>
        refine(e) ? rule(e) : pass(e);

const fold =
    <E>(rules: readonly Rule<E>[]): Rule<E> =>
    (e: E): Decision<E> =>
        rules.reduce<Decision<E>>((decision, rule) => (decision.kind === 'pass' ? rule(decision.e) : decision), pass(e));

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Decision, Rule };
export { deny, fold, pass, when };
