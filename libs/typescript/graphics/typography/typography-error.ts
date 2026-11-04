import { Formatter, Schema } from 'effect';

// --- [ERRORS] --------------------------------------------------------------------------

class TypographyError extends Schema.TaggedError<TypographyError, { readonly brand: unique symbol }>()('TypographyError', { subject: Schema.String, cause: Schema.Defect() }) {
    override get message() {
        return `Unreadable ${this.subject}, ${Formatter.format(this.cause)}`;
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { TypographyError };
