import { Schema } from 'effect';

// --- [ERRORS] --------------------------------------------------------------------------

class MediaError extends Schema.TaggedError<MediaError, { readonly brand: unique symbol }>()('MediaError', {
    reason: Schema.TaggedUnion({ Protected: { code: Schema.Int }, Missing: {}, Unreadable: {}, Unsupported: {} }),
    subject: Schema.String,
    cause: Schema.optionalKey(Schema.Defect()),
}) {
    override get message() {
        return `${this.reason._tag}: ${this.subject}`;
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { MediaError };
