import { Formatter, Schema } from 'effect';

// --- [ERRORS] --------------------------------------------------------------------------

class MediaError extends Schema.TaggedError<MediaError, { readonly brand: unique symbol }>()('MediaError', {
    reason: Schema.TaggedUnion({ Protected: {}, Missing: {}, Unreadable: {}, Unsupported: {} }),
    subject: Schema.String,
    cause: Schema.optionalKey(Schema.Defect()),
}) {
    static readonly of = (_tag: MediaError['reason']['_tag'], subject: string) => (cause?: unknown) => MediaError.make({ reason: { _tag }, subject, ...(cause === undefined ? {} : { cause }) });

    override get message() {
        return [`${this.reason._tag}: ${this.subject}`, ...(this.cause === undefined ? [] : [Formatter.format(this.cause)])].join(': ');
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { MediaError };
