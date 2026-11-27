import { Unmatched } from '@rasm/search/unmatched';
import { Schema, SchemaTransformation, String } from 'effect';

// --- [ERRORS] --------------------------------------------------------------------------

class ApplicationFailure extends Schema.TaggedError<ApplicationFailure, { readonly brand: unique symbol }>()('ApplicationFailure', {
    subject: Schema.String,
    reason: Schema.String,
    values: Schema.Array(Schema.String),
    message: Schema.String,
}) {
    static readonly of = (reason: string, subject: string, values: readonly string[] = []) => ApplicationFailure.make({ subject, reason, values, message: [subject, String.noCase()(reason), values.join(', ')].filter(String.isNonEmpty).join(' ') });
    static readonly stated = { subject: Schema.String, reason: Schema.String, values: Schema.Array(Schema.String) };
    static readonly Stated = Schema.Struct(ApplicationFailure.stated).pipe(
        Schema.decodeTo(Schema.instanceOf(ApplicationFailure), SchemaTransformation.transform({ decode: ({ reason, subject, values }) => ApplicationFailure.of(reason, subject, values), encode: ({ reason, subject, values }) => ({ reason, subject, values }) })),
    );
    static readonly Thrown = Schema.Union([Unmatched, ApplicationFailure.Stated, Schema.ErrorInstance()]);
    static readonly read = <A extends Schema.Top>(value: A) => Schema.Result(value, ApplicationFailure.Thrown);
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { ApplicationFailure };
