import uFuzzy from '@leeoniya/ufuzzy';
import { Array, Effect, flow, Option, Predicate, Schema } from 'effect';

// --- [ERRORS] --------------------------------------------------------------------------

class Unmatched extends Schema.TaggedError<Unmatched, { readonly brand: unique symbol }>()('Unmatched', { subject: Schema.String, query: Schema.OptionFromOptionalKey(Schema.String), candidates: Schema.Array(Schema.String) }) {
    static readonly Refusal = Schema.Union([Unmatched, Schema.ErrorInstance()]);
    static readonly matching =
        (subject: string, query: Option.Option<string>) =>
        <A>(candidates: readonly A[], names: (candidate: A) => readonly string[]): Effect.Effect<readonly A[], Unmatched> => {
            const equal = new Intl.Collator('en', { usage: 'search', sensitivity: 'accent' }).compare;
            return Option.match(query, {
                onNone: () => Effect.succeed(candidates),
                onSome: (wanted) =>
                    Effect.fromOption(
                        Option.liftPredicate(
                            candidates.filter((candidate) => names(candidate).some((name) => equal(name, wanted) === 0)),
                            Array.isReadonlyArrayNonEmpty,
                        ),
                        () => Unmatched.make({ subject, query, candidates: Array.dedupe(Array.flatMap(candidates, flow(names, Array.take(1)))) }),
                    ),
            });
        };
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

// --- [OPERATIONS] ----------------------------------------------------------------------

const ranked = (query: string, candidates: readonly string[]): readonly string[] => {
    const [found, info, order] = new uFuzzy({ intraMode: 1, unicode: true, interSplit: "[^\\p{L}\\d']+", intraSplit: '\\p{Ll}\\p{Lu}', intraBound: '\\p{L}\\d|\\d\\p{L}|\\p{Ll}\\p{Lu}', intraChars: "[\\p{L}\\d']", intraContr: "'\\p{L}{1,2}\\b" }).search([...candidates], query, 0, Number.POSITIVE_INFINITY);
    const matched = (order === null || info === null ? (found ?? []) : order.flatMap((position) => info.idx[position] ?? [])).flatMap((index) => candidates[index] ?? []);
    return matched.length === 0 ? candidates : matched;
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { ranked, Unmatched };
