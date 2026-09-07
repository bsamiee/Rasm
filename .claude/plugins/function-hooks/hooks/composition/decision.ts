// Decision as a case record of rewrite, deny, and answer, fold runs a rule table and absurd closes an arm the type rules out

// --- [IMPORTS] -------------------------------------------------------------------------

import { fromPredicate } from './option.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface DecisionCases<E, R, D, B> {
    readonly rewrite: (e: E, context: readonly string[]) => B;
    readonly deny: (reason: D) => B;
    readonly answer: (result: R) => B;
}

interface Decision<E, R, D> {
    readonly match: <B>(cases: DecisionCases<E, R, D, B>) => B;
}

type Rule<E, R, D> = (e: E) => Decision<E, R, D>;

// --- [CONSTRUCTORS] --------------------------------------------------------------------

// The event to run beneath with the context lines
const rewrite = <E, R, D>(e: E, context: readonly string[]): Decision<E, R, D> => ({ match: (cases) => cases.rewrite(e, context) });

const deny = <E, R, D>(reason: D): Decision<E, R, D> => ({ match: (cases) => cases.deny(reason) });

const answer = <E, R, D>(result: R): Decision<E, R, D> => ({ match: (cases) => cases.answer(result) });

// The arm of a case a decision's type rules out, deny where D is never and answer where R is never
const absurd = (value: never): never => value;

// --- [OPERATIONS] ----------------------------------------------------------------------

// Lifts a rule over a narrower input, a non-matching input passes through as rewrite(e, []), the pass form
const when =
    <E, N extends E, R, D>(refinement: (e: E) => e is N, rule: Rule<N, R, D>): Rule<E, R, D> =>
    (e: E): Decision<E, R, D> =>
        fromPredicate(refinement)(e).match<Decision<E, R, D>>({ some: rule, none: () => rewrite(e, []) });

// Runs the rules in table order, a rewrite feeds the next rule and accumulates context, a deny or an answer ends the fold
const fold =
    <E, R, D>(rules: readonly Rule<E, R, D>[]): Rule<E, R, D> =>
    (e: E): Decision<E, R, D> =>
        rules.reduce<Decision<E, R, D>>(
            (decision, rule) =>
                decision.match<Decision<E, R, D>>({
                    rewrite: (current, context) =>
                        rule(current).match<Decision<E, R, D>>({
                            rewrite: (next, more) => rewrite(next, [...context, ...more]),
                            deny,
                            answer,
                        }),
                    deny,
                    answer,
                }),
            rewrite(e, []),
        );

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Decision, Rule };
export { absurd, answer, deny, fold, rewrite, when };
