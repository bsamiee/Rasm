import { Schema } from 'effect';

// --- [ERRORS] --------------------------------------------------------------------------

class InDesignError extends Schema.TaggedError<InDesignError, { readonly brand: unique symbol }>()('InDesignError', {
    reason: Schema.TaggedUnion({ Refused: {}, Unavailable: { because: Schema.String } }),
    subject: Schema.String,
    cause: Schema.optionalKey(Schema.Defect()),
}) {
    static readonly refused = (subject: string) => (cause: unknown) => InDesignError.make({ reason: { _tag: 'Refused' }, subject, cause });

    override get message(): string {
        return InDesignError.fields.reason.match(this.reason, { Refused: () => this.subject, Unavailable: ({ because }) => `${this.subject}: ${because}` });
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { InDesignError };
