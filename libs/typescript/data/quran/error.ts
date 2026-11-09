import { Formatter, Schema } from 'effect';

// --- [ERRORS] --------------------------------------------------------------------------

class QuranError extends Schema.TaggedError<QuranError, { readonly brand: unique symbol }>()('QuranError', {
    subject: Schema.String.annotate({ description: 'Source the text was read from' }),
    cause: Schema.Defect(),
}) {
    override get message() {
        return `Unreadable ${this.subject}: ${Formatter.format(this.cause)}`;
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { QuranError };
