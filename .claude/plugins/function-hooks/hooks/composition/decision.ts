// Decision of a rule over an event: the event to run beneath with its context lines, a refusal, or an answer in the engine's place

// --- [TYPES] ---------------------------------------------------------------------------

type Decision<E, R = unknown> =
    | { readonly kind: 'rewrite'; readonly e: E; readonly context: readonly string[] }
    | { readonly kind: 'deny'; readonly reason: string }
    | { readonly kind: 'answer'; readonly result: R };

type Rule<E, R = unknown> = (e: E) => Decision<E, R>;

// --- [CONSTRUCTORS] --------------------------------------------------------------------

const rewrite = <E, R = unknown>(e: E, context: readonly string[] = []): Decision<E, R> => ({ kind: 'rewrite', e, context });

const deny = <E, R = unknown>(reason: string): Decision<E, R> => ({ kind: 'deny', reason });

const answer = <E, R>(result: R): Decision<E, R> => ({ kind: 'answer', result });

// --- [OPERATIONS] ----------------------------------------------------------------------

// Lifts a rule over the events a refinement selects, every other event passes
const when =
    <E, N extends E, R = unknown>(refine: (e: E) => e is N, rule: Rule<N, R>): Rule<E, R> =>
    (e: E): Decision<E, R> =>
        refine(e) ? rule(e) : rewrite(e);

// Runs the rules in table order, each rewrite feeds the next rule and keeps every context line, a deny or an answer ends the fold
const fold =
    <E, R = unknown>(rules: readonly Rule<E, R>[]): Rule<E, R> =>
    (e: E): Decision<E, R> =>
        rules.reduce<Decision<E, R>>((decision, rule) => {
            if (decision.kind !== 'rewrite') {
                return decision;
            }
            const next = rule(decision.e);
            return next.kind === 'rewrite' ? rewrite(next.e, [...decision.context, ...next.context]) : next;
        }, rewrite(e));

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Decision, Rule };
export { answer, deny, fold, rewrite, when };
