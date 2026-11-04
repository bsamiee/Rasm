import { Effect, Schema } from 'effect';

// --- [OPERATIONS] ----------------------------------------------------------------------

const defaulted = <S extends Schema.Top & Schema.WithoutConstructorDefault>(schema: S, value: S['Type'] & S['Encoded']) => schema.pipe(Schema.withDecodingDefaultKey(Effect.succeed(value)), Schema.withConstructorDefault(Effect.succeed(value)));

// --- [EXPORTS] -------------------------------------------------------------------------

export { defaulted };
