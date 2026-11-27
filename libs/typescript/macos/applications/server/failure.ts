import { Unmatched } from '@rasm/search/unmatched';
import { Schema, SchemaTransformation } from 'effect';

// --- [ERRORS] --------------------------------------------------------------------------

class ApplicationFailure extends Schema.TaggedError<ApplicationFailure, { readonly brand: unique symbol }>()('ApplicationFailure', {
    reason: Schema.String,
    subject: Schema.String,
    message: Schema.String,
}) {
    static readonly stated = { reason: Schema.String, subject: Schema.String, message: Schema.optionalKey(Schema.String) };
    static readonly Thrown = Schema.Union([
        Unmatched,
        ApplicationFailure,
        Schema.TaggedStruct('AppleEventFailure', { reason: Schema.String, subject: Schema.String, message: Schema.String }).pipe(
            Schema.decodeTo(ApplicationFailure, SchemaTransformation.transform({ decode: (event) => ({ ...event, _tag: 'ApplicationFailure' as const }), encode: (failure) => ({ ...failure, _tag: 'AppleEventFailure' as const }) })),
        ),
        Schema.ErrorInstance(),
    ]);
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { ApplicationFailure };
