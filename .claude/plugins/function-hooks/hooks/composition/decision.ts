// Decision of a rule over an event: the event to run beneath with its context lines, or a refusal

// --- [TYPES] ---------------------------------------------------------------------------

type Decision<E> =
    | { readonly kind: 'rewrite'; readonly e: E; readonly context: readonly string[] }
    | { readonly kind: 'deny'; readonly reason: string };

type Rule<E> = (e: E) => Decision<E>;

// --- [CONSTRUCTORS] --------------------------------------------------------------------

const rewrite = <E>(e: E, context: readonly string[] = []): Decision<E> => ({ kind: 'rewrite', e, context });

const deny = <E>(reason: string): Decision<E> => ({ kind: 'deny', reason });

// --- [OPERATIONS] ----------------------------------------------------------------------

// Lifts a rule over the events a refinement selects, every other event passes
const when =
    <E, N extends E>(refine: (e: E) => e is N, rule: Rule<N>): Rule<E> =>
    (e: E): Decision<E> =>
        refine(e) ? rule(e) : rewrite(e);

// Runs the rules in table order, each rewrite feeds the next rule and keeps every context line, a deny ends the fold
const fold =
    <E>(rules: readonly Rule<E>[]): Rule<E> =>
    (e: E): Decision<E> =>
        rules.reduce<Decision<E>>((decision, rule) => {
            if (decision.kind !== 'rewrite') {
                return decision;
            }
            const next = rule(decision.e);
            return next.kind === 'rewrite' ? rewrite(next.e, [...decision.context, ...next.context]) : next;
        }, rewrite(e));

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Decision, Rule };
export { deny, fold, rewrite, when };
