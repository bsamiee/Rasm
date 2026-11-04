import { HostError, type Requests } from '@rasm/automation';
import { Unmatched } from '@rasm/search';
import { Cause, Effect, Match, Option, Schema } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Envelope<Current extends Requests = Requests> = Current extends Requests ? Schema.Struct<{ readonly tag: Schema.Literal<Current['_tag']>; readonly label: Schema.String; readonly payload: Current['payloadSchema'] }>['Encoded'] : never;

// --- [MODELS] --------------------------------------------------------------------------

const completions = Schema.Union([
    Schema.TaggedStruct('Value', { value: Schema.optionalKey(Schema.Json) }),
    Unmatched,
    Schema.TaggedStruct('IllegalArgument', { message: Schema.String }),
    Schema.TaggedStruct('Thrown', { message: Schema.String, number: Schema.OptionFromOptionalKey(Schema.Finite), line: Schema.OptionFromOptionalKey(Schema.Int) }),
]);
const Completion = Object.assign(completions, {
    answered: <S extends Schema.Top>(success: S, subject: string) =>
        Match.typeTags<typeof completions.Type>()({
            Value: ({ value }) => Schema.decodeEffect(success)(value),
            Unmatched: Effect.fail<Unmatched>,
            IllegalArgument: ({ message }) => Effect.fail(new Cause.IllegalArgumentError(message)),
            Thrown: (failed) => {
                const coded = [subject, ...Option.toArray(Option.map(failed.number, (code) => `Error ${code}`))].join(': ');
                return Effect.fail(Option.isSome(failed.line) ? HostError.thrown(`${coded}: ${failed.message}`, failed.line)(failed) : HostError.refused(coded)(failed));
            },
        }),
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Completion, type Envelope };
