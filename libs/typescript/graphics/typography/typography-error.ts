import { Formatter, Schema } from 'effect';

// --- [ERRORS] --------------------------------------------------------------------------

class TypographyError extends Schema.TaggedError<TypographyError, { readonly brand: unique symbol }>()('TypographyError', {
    reason: Schema.TaggedUnion({ Unreadable: { cause: Schema.Defect() }, Unsupported: {} }),
    subject: Schema.String,
}) {
    static readonly unreadable = (subject: string) => (cause: unknown) => TypographyError.make({ reason: { _tag: 'Unreadable', cause }, subject });
    static readonly unsupported = (subject: string) => TypographyError.make({ reason: { _tag: 'Unsupported' }, subject });

    override get message() {
        return TypographyError.fields.reason.match(this.reason, {
            Unreadable: ({ cause }) => `Unreadable ${this.subject}, ${Formatter.format(cause)}`,
            Unsupported: () => `Unsupported ${this.subject}`,
        });
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { TypographyError };
