import { HostError } from '@rasm/object-model';
import { Unmatched } from '@rasm/search';
import { Cause, Effect, Option, Schema } from 'effect';

// --- [MODELS] --------------------------------------------------------------------------

const Completions = Schema.Union([
    Schema.TaggedStruct('Value', { value: Schema.optionalKey(Schema.Json) }),
    Unmatched.stated,
    Schema.TaggedStruct('IllegalArgument', { message: Schema.String }),
    Schema.TaggedStruct('Thrown', { message: Schema.String, number: Schema.OptionFromOptionalKey(Schema.Finite), line: Schema.OptionFromOptionalKey(Schema.Int) }),
])
    .annotate({ description: 'Reply of an ExtendScript evaluation, the value it returned or the failure it threw' })
    .pipe(Schema.toTaggedUnion('_tag'));
const Completion = Object.assign(Completions, {
    decode: <S extends Schema.Top>(success: S, subject: string) =>
        Completions.match({
            Value: ({ value }) => Schema.decodeEffect(success)(value),
            Unmatched: Effect.fail<Unmatched>,
            IllegalArgument: ({ message }) => Effect.fail(new Cause.IllegalArgumentError(message)),
            Thrown: (failed) => {
                const coded = Option.match(failed.number, { onNone: () => subject, onSome: (code) => `${subject}: Error ${code}` });
                return Effect.fail(Option.isSome(failed.line) ? HostError.thrown(`${coded}: ${failed.message}`, failed.line)(failed) : HostError.refused(coded)(failed));
            },
        }),
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Completion };
