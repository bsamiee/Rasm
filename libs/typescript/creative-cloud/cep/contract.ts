import { HostError, type Requests } from '@rasm/automation/requests';
import { Unmatched } from '@rasm/search/unmatched';
import { Cause, Effect, Match, Option, Schema } from 'effect';
import type { RpcGroup } from 'effect/rpc';

// --- [TYPES] ---------------------------------------------------------------------------

type Request = RpcGroup.Rpcs<typeof Requests>;
type Envelope<Current extends Request = Request> = Current extends Request ? Schema.Struct<{ readonly tag: Schema.Literal<Current['_tag']>; readonly label: Schema.String; readonly payload: Current['payloadSchema'] }>['Encoded'] : never;

// --- [MODELS] --------------------------------------------------------------------------

const Envelope = <Current extends Request>({ _tag, payloadSchema }: Current) => Schema.Struct({ tag: Schema.Literal(_tag), label: Schema.String, payload: payloadSchema });
const Reply = Schema.Union([
    Schema.TaggedStruct('Value', { value: Schema.optionalKey(Schema.Json) }),
    Unmatched,
    Schema.TaggedStruct('IllegalArgument', { message: Schema.String }),
    Schema.TaggedStruct('Thrown', { message: Schema.String, number: Schema.OptionFromOptionalKey(Schema.Finite), line: Schema.OptionFromOptionalKey(Schema.Int) }),
]);

// --- [OPERATIONS] ----------------------------------------------------------------------

const answered =
    <S extends Schema.Top>(success: S, subject: string) =>
    (reply: typeof Reply.Type) =>
        Match.valueTags(reply, {
            Value: ({ value }) => Schema.decodeEffect(success)(value),
            Unmatched: Effect.fail,
            IllegalArgument: ({ message }) => Effect.fail(new Cause.IllegalArgumentError(message)),
            Thrown: (failed) => {
                const coded = [subject, ...Option.toArray(Option.map(failed.number, (code) => `Error ${code}`))].join(': ');
                return Effect.fail(Option.isSome(failed.line) ? HostError.thrown(`${coded}: ${failed.message}`, failed.line)(failed) : HostError.refused(coded)(failed));
            },
        });

// --- [EXPORTS] -------------------------------------------------------------------------

export { answered, Envelope, Reply, type Request };
