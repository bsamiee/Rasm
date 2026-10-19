import uFuzzy from '@leeoniya/ufuzzy';
import { Array, Effect, Schema, String } from 'effect';

// --- [ERRORS] --------------------------------------------------------------------------

class Unmatched extends Schema.TaggedError<Unmatched, { readonly brand: unique symbol }>()('Unmatched', { subject: Schema.String, query: Schema.String, candidates: Schema.Array(Schema.String) }) {
    override get message(): string {
        return `No ${this.subject}${this.query === '' ? '' : ` matching ${this.query}`}${this.candidates.length === 0 ? '' : `, candidates: ${this.candidates.join(', ')}`}`;
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const ranked = (query: string, candidates: readonly string[]): readonly string[] => {
    const [found, info, order] = new uFuzzy({ intraMode: 1 }).search([...candidates], query, 0, Number.POSITIVE_INFINITY);
    const matched = (order === null || info === null ? (found ?? []) : order.flatMap((position) => info.idx[position] ?? [])).flatMap((index) => candidates[index] ?? []);
    return matched.length === 0 ? candidates : matched;
};
const single =
    (subject: string, query: string) =>
    <A>(candidates: readonly A[], { names, label }: { readonly names: (candidate: A) => readonly string[]; readonly label: (candidate: A) => string }): Effect.Effect<A, Unmatched> => {
        const key = String.toLowerCase(query);
        const matched = candidates.filter((candidate) => names(candidate).map(String.toLowerCase).includes(key));
        return Array.match(matched, {
            onEmpty: () => Effect.fail(Unmatched.make({ subject, query, candidates: candidates.map(label) })),
            onNonEmpty: ([only, ...others]) => (others.length === 0 ? Effect.succeed(only) : Effect.fail(Unmatched.make({ subject: `single ${subject}`, query, candidates: matched.map(label) }))),
        });
    };

// --- [EXPORTS] -------------------------------------------------------------------------

export { ranked, single, Unmatched };
