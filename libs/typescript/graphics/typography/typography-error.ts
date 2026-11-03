import { Predicate, Schema } from 'effect';

// --- [ERRORS] --------------------------------------------------------------------------

class TypographyError extends Schema.TaggedError<TypographyError, { readonly brand: unique symbol }>()('TypographyError', { subject: Schema.String, cause: Schema.Defect() }) {
    override get message() {
        return `Unreadable ${this.subject}, ${Predicate.isError(this.cause) ? this.cause.message : String(this.cause)}`;
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { TypographyError };
