import uFuzzy from '@leeoniya/ufuzzy';
import { Schema } from 'effect';

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

// --- [EXPORTS] -------------------------------------------------------------------------

export { ranked, Unmatched };
