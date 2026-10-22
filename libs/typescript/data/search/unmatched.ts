import { Array, Effect, flow, Option, Predicate, Schema } from 'effect';
import { ranked } from '#/ranked';

// --- [CONSTANTS] -----------------------------------------------------------------------

const collator = new Intl.Collator('en', { usage: 'search', sensitivity: 'accent' });

// --- [ERRORS] --------------------------------------------------------------------------

class Unmatched extends Schema.TaggedError<Unmatched, { readonly brand: unique symbol }>()('Unmatched', { subject: Schema.String, query: Schema.OptionFromOptionalKey(Schema.String), candidates: Schema.Array(Schema.String) }) {
    static readonly Refusal = Schema.Union([Unmatched, Schema.ErrorInstance()]);
    static readonly matching =
        (subject: string, query: Option.Option<string>) =>
        <A>(candidates: readonly A[], names: (candidate: A) => readonly string[]): Effect.Effect<readonly A[], Unmatched> =>
            Option.match(query, {
                onNone: () => Effect.succeed(candidates),
                onSome: (wanted) =>
                    Effect.fromOption(
                        Option.liftPredicate(
                            candidates.filter((candidate) => names(candidate).some((name) => collator.compare(name, wanted) === 0)),
                            Array.isReadonlyArrayNonEmpty,
                        ),
                        () => Unmatched.make({ subject, query, candidates: Array.dedupe(Array.flatMap(candidates, flow(names, Array.take(1)))) }),
                    ),
            });
    static readonly single =
        (subject: string, query: Option.Option<string>) =>
        <A>(candidates: readonly A[], names: (candidate: A) => readonly string[]): Effect.Effect<A, Unmatched> =>
            Effect.map(
                Effect.filterOrFail(Unmatched.matching(subject, query)(candidates, names), Predicate.isTupleOf(1), (matched) => Unmatched.make({ subject: `single ${subject}`, query, candidates: Array.flatMap(matched, flow(names, Array.take(1))) })),
                ([only]) => only,
            );

    override get message(): string {
        const suggestions = 100;
        const [ordered, matching] = Option.match(this.query, { onNone: () => [this.candidates, ''] as const, onSome: (query) => [ranked(query, this.candidates), ` matching ${query}`] as const });
        const shown = Array.take(ordered, suggestions);
        return `No ${this.subject}${matching}${shown.length === 0 ? '' : `, candidates: ${shown.join(', ')}`}${shown.length === this.candidates.length ? '' : `, and ${this.candidates.length - shown.length} more`}`;
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Unmatched };
