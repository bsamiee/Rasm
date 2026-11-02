import { Schema } from 'effect';

// --- [ERRORS] --------------------------------------------------------------------------

class HarnessError extends Schema.TaggedError<HarnessError, { readonly brand: unique symbol }>()('HarnessError', {
    reason: Schema.TaggedUnion({ Refused: {}, Unavailable: { because: Schema.String }, Disagreed: { mismatches: Schema.Int } }),
    subject: Schema.String,
    cause: Schema.optionalKey(Schema.Defect()),
}) {
    static readonly refused = (subject: string) => (cause: unknown) => HarnessError.make({ reason: { _tag: 'Refused' }, subject, cause });

    override get message(): string {
        return HarnessError.fields.reason.match(this.reason, {
            Refused: () => this.subject,
            Unavailable: ({ because }) => `${this.subject}: ${because}`,
            Disagreed: ({ mismatches }) => `${this.subject} disagrees with the host on ${mismatches} cases`,
        });
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { HarnessError };
