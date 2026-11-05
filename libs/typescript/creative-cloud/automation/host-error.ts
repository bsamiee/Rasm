import { Formatter, Match, Option, Schema, String } from 'effect';

// --- [ERRORS] --------------------------------------------------------------------------

class HostError extends Schema.TaggedError<HostError, { readonly brand: unique symbol }>()('HostError', {
    reason: Schema.TaggedUnion({
        Refused: {},
        Unavailable: {},
        Cancelled: {},
        Thrown: { line: Schema.OptionFromOptionalKey(Schema.Int.annotate({ description: 'Body line that threw' })) },
    }),
    subject: Schema.String,
    cause: Schema.optionalKey(Schema.Defect()),
}) {
    static readonly refused = (subject: string) => (cause?: unknown) => HostError.make({ reason: { _tag: 'Refused' }, subject, ...(cause === undefined ? {} : { cause }) });
    static readonly thrown = (subject: string, line: Option.Option<number>) => (cause: unknown) => HostError.make({ reason: { _tag: 'Thrown', line }, subject, cause });

    override get message(): string {
        return Match.value(this.reason).pipe(
            Match.tag('Thrown', ({ line }) => [...Option.toArray(line).map((at) => `line ${at}`), this.subject].join(': ')),
            Match.orElse(({ _tag }) => [`${this.subject} ${String.toLowerCase(_tag)}`, ...(this.cause === undefined ? [] : [Formatter.format(this.cause)])].join(': ')),
        );
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { HostError };
