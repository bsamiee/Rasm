import { Formatter, Schema } from 'effect';

// --- [ERRORS] --------------------------------------------------------------------------

class MediaError extends Schema.TaggedError<MediaError, { readonly brand: unique symbol }>()('MediaError', {
    reason: Schema.TaggedUnion({ Protected: {}, Missing: {}, Unreadable: {}, Unsupported: {}, Unavailable: {} }),
    subject: Schema.String,
    cause: Schema.optionalKey(Schema.Defect()),
    message: Schema.String,
}) {
    static readonly of = (_tag: MediaError['reason']['_tag'], subject: string) => (cause?: unknown) => MediaError.make({ reason: { _tag }, subject, ...(cause === undefined ? {} : { cause }), message: [`${_tag}: ${subject}`, ...(cause === undefined ? [] : [Formatter.format(cause)])].join(': ') });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { MediaError };
