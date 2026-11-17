import { Schema } from 'effect';
import { createSerializableError } from 'nx/src/utils/serializable-error';

// --- [MODELS] --------------------------------------------------------------------------

const Refused = Schema.Struct({
    message: Schema.optionalKey(Schema.String),
    errors: Schema.optionalKey(Schema.Array(Schema.Tuple([Schema.NullOr(Schema.String), Schema.Struct({ message: Schema.optionalKey(Schema.String) })]))),
}).annotate({ description: 'An error as Nx serializes it out of a plugin worker, an aggregate holding the error of each file it names' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const crossed = (error: Error) => [structuredClone(error).message, createSerializableError(error).message];

const refused = (refusal: RegExp) => (error: Error) => {
    const serialized = createSerializableError(error);
    return Schema.is(Refused)(serialized) && (serialized.errors === undefined ? [serialized.message] : serialized.errors.map(([, each]) => each.message)).some((stated) => stated !== undefined && refusal.test(stated));
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { crossed, refused };
