import { HostError } from '@rasm/object-model/model';
import { Unmatched } from '@rasm/search/unmatched';
import { Cause, Effect, Option, Schema } from 'effect';

// --- [MODELS] --------------------------------------------------------------------------

const Completions = Schema.Union([
    Schema.TaggedStruct('Value', { value: Schema.optionalKey(Schema.Json) }),
    Unmatched.stated,
    Schema.TaggedStruct('IllegalArgument', { message: Schema.String }),
    Schema.TaggedStruct('Thrown', { message: Schema.String, number: Schema.optionalKey(Schema.Finite), line: Schema.optionalKey(Schema.Int) }),
])
    .annotate({ description: 'Reply of an ExtendScript evaluation, the value it returned or the failure it threw' })
    .pipe(Schema.toTaggedUnion('_tag'));
const Completion = Object.assign(Completions, {
    decode: <S extends Schema.Top>(success: S, subject: string) =>
        Completions.match({
            Value: ({ value }) => Schema.decodeEffect(success)(value),
            Unmatched: Effect.fail<Unmatched>,
            IllegalArgument: ({ message }) => Effect.fail(new Cause.IllegalArgumentError(message)),
            Thrown: (failed) => Effect.fail(failed.line === undefined ? HostError.refused(subject)(failed) : HostError.thrown(`${subject}: ${failed.message}`, Option.some(failed.line))(failed)),
        }),
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Completion };
