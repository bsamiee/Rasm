import { Option, Schema } from 'effect';

// --- [MODELS] --------------------------------------------------------------------------

const Thrown = Schema.Struct({ name: Schema.optionalKey(Schema.String), message: Schema.optionalKey(Schema.String) }).annotate({ description: 'Name and message a thrown value carries, own or inherited' });

// --- [ERRORS] --------------------------------------------------------------------------

class HarnessError extends Schema.TaggedError<HarnessError, { readonly brand: unique symbol }>()('HarnessError', {
    reason: Schema.TaggedUnion({ Refused: {}, Unavailable: { because: Schema.String }, Disagreed: { mismatches: Schema.Int } }),
    subject: Schema.String,
    cause: Schema.optionalKey(Schema.Defect()),
}) {
    static readonly refused = (subject: string) => (cause: unknown) => HarnessError.make({ reason: { _tag: 'Refused' }, subject, cause });
    static readonly unavailable = (subject: string) => (because: string) => HarnessError.make({ reason: { _tag: 'Unavailable', because }, subject });

    override get message(): string {
        return HarnessError.fields.reason.match(this.reason, {
            Refused: () => `${this.subject}: ${thrown(this.cause)}`,
            Unavailable: ({ because }) => `${this.subject}: ${because}`,
            Disagreed: ({ mismatches }) => `${this.subject} disagrees with the host on ${mismatches} cases`,
        });
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const thrown = (error: unknown): string =>
    Option.match(Schema.decodeUnknownOption(Thrown)(error), {
        onNone: () => `Error: ${String(error)}`,
        onSome: ({ name = 'Error', message = String(error) }) => `${name}: ${message}`,
    });

// --- [EXPORTS] -------------------------------------------------------------------------

export { HarnessError, thrown };
