import { Schema } from 'effect';

// --- [ERRORS] --------------------------------------------------------------------------

class TypographyError extends Schema.TaggedError<TypographyError, { readonly brand: unique symbol }>()('TypographyError', { subject: Schema.String, cause: Schema.Defect() }) {
    override get message() {
        return `Unreadable ${this.subject}, decode to OpenType or TrueType bytes`;
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { TypographyError };
