import { Formatter, Schema } from 'effect';

// --- [ERRORS] --------------------------------------------------------------------------

class HostingError extends Schema.TaggedError<HostingError, { readonly brand: unique symbol }>()('HostingError', {
    reason: Schema.TaggedUnion({ Build: {}, Template: {} }),
    subject: Schema.String,
    cause: Schema.optionalKey(Schema.Defect()),
}) {
    static readonly of = (reason: HostingError['reason']['_tag'], subject: string) => (cause?: unknown) => HostingError.make({ reason: { _tag: reason }, subject, ...(cause === undefined ? {} : { cause }) });

    override get message() {
        return [`${this.reason._tag}: ${this.subject}`, ...(this.cause === undefined ? [] : [Formatter.format(this.cause)])].join(': ');
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { HostingError };
