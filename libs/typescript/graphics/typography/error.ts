import { Formatter, Schema } from 'effect';

// --- [ERRORS] --------------------------------------------------------------------------

class TypographyError extends Schema.TaggedError<TypographyError, { readonly brand: unique symbol }>()('TypographyError', {
    reason: Schema.TaggedUnion({ Unreadable: { cause: Schema.Defect() }, Unsupported: {} }),
    subject: Schema.String,
    message: Schema.String,
}) {
    static readonly unreadable = (subject: string) => (cause: unknown) => TypographyError.make({ reason: { _tag: 'Unreadable', cause }, subject, message: `Unreadable ${subject}, ${Formatter.format(cause)}` });
    static readonly unsupported = (subject: string) => TypographyError.make({ reason: { _tag: 'Unsupported' }, subject, message: `Unsupported ${subject}` });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { TypographyError };
