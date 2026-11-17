import { Formatter, Schema } from 'effect';

// --- [ERRORS] --------------------------------------------------------------------------

class HostingError extends Schema.TaggedError<HostingError, { readonly brand: unique symbol }>()('HostingError', {
    reason: Schema.TaggedUnion({ Build: {}, Template: {} }),
    subject: Schema.String,
    cause: Schema.optionalKey(Schema.Defect()),
    message: Schema.String,
}) {
    static readonly of = (reason: HostingError['reason']['_tag'], subject: string) => (cause?: unknown) => HostingError.make({ reason: { _tag: reason }, subject, ...(cause === undefined ? {} : { cause }), message: [`${reason}: ${subject}`, ...(cause === undefined ? [] : [Formatter.format(cause)])].join(': ') });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { HostingError };
