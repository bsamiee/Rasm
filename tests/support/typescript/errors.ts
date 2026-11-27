import { Match, Predicate, Schema } from 'effect';
import { createSerializableError } from 'nx/src/utils/serializable-error';

// --- [MODELS] --------------------------------------------------------------------------

const Aggregate = Schema.Struct({ errors: Schema.Array(Schema.Unknown) }).annotate({ description: 'An error as Nx serializes it out of a plugin worker that holds further errors, a ProjectConfigurationsError or an AggregateCreateNodesError' });
const Filed = Schema.Tuple([Schema.NullOr(Schema.String), Schema.Unknown]).annotate({ description: 'One entry of an AggregateCreateNodesError: the file a plugin read and the error it raised there' });
const Caused = Schema.Struct({ cause: Schema.Unknown }).annotate({ description: 'An error wrapping the error that raised it, as an UnknownError a plugin runner makes legible wraps a PlatformError' });
const Stated = Schema.Struct({ message: Schema.String }).annotate({ description: 'An error and the message it states' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const raised = (value: unknown): readonly unknown[] =>
    Match.value(value).pipe(
        Match.when(Schema.is(Aggregate), ({ errors }) => errors.flatMap((entry) => raised(Schema.is(Filed)(entry) ? entry[1] : entry))),
        Match.when(Schema.is(Caused), (error) => [error, ...raised(error.cause)]),
        Match.when(Predicate.isObject, (error) => [error]),
        Match.orElse(() => []),
    );

const refused = (refusal: RegExp | Schema.Constraint) => (error: Error) => raised(createSerializableError(error)).some(Predicate.isRegExp(refusal) ? (each) => Schema.is(Stated)(each) && refusal.test(each.message) : Schema.is(refusal));

// --- [EXPORTS] -------------------------------------------------------------------------

export { refused };
