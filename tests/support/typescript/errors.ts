import { Match, Schema } from 'effect';
import { createSerializableError } from 'nx/src/utils/serializable-error';

// --- [MODELS] --------------------------------------------------------------------------

const Aggregate = Schema.Struct({ errors: Schema.Array(Schema.Unknown) }).annotate({ description: 'An error as Nx serializes it out of a plugin worker that holds further errors, a ProjectConfigurationsError or an AggregateCreateNodesError' });
const Filed = Schema.Tuple([Schema.NullOr(Schema.String), Schema.Unknown]).annotate({ description: 'One entry of an AggregateCreateNodesError: the file a plugin read and the error it raised there' });
const Stated = Schema.Struct({ message: Schema.String }).annotate({ description: 'A leaf error and the message it states' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const stated = (value: unknown): readonly string[] =>
    Match.value(value).pipe(
        Match.when(Schema.is(Aggregate), ({ errors }) => errors.flatMap((entry) => stated(Schema.is(Filed)(entry) ? entry[1] : entry))),
        Match.when(Schema.is(Stated), ({ message }) => [message]),
        Match.orElse(() => []),
    );

const refused = (refusal: RegExp) => (error: Error) => stated(createSerializableError(error)).some((message) => refusal.test(message));

// --- [EXPORTS] -------------------------------------------------------------------------

export { refused };
