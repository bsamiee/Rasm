import { Effect, Schema } from 'effect';

// --- [MODELS] --------------------------------------------------------------------------

const WorkerConfig = Schema.Struct({
    env: Schema.Record(
        Schema.String,
        Schema.Union([
            Schema.Struct({ type: Schema.Literal('assets') }),
            Schema.Struct({ type: Schema.Literal('d1'), name: Schema.optionalKey(Schema.String) }),
            Schema.Struct({ type: Schema.Literal('r2'), name: Schema.optionalKey(Schema.String), jurisdiction: Schema.optionalKey(Schema.String) }),
            Schema.Struct({ type: Schema.Literal('hyperdrive') }),
            Schema.Struct({ type: Schema.Literal('secret') }),
            Schema.Struct({ type: Schema.Literal('text'), value: Schema.String }),
        ]),
    ).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
    domains: Schema.Array(Schema.Union([Schema.String, Schema.Struct({ hostname: Schema.String, production: Schema.Boolean })])).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
    triggers: Schema.Array(Schema.Struct({ type: Schema.Literal('fetch'), pattern: Schema.String, zone: Schema.String })).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const migratorVariable = (variable: string) => `${variable}_MIGRATOR`;

// --- [EXPORTS] -------------------------------------------------------------------------

export { migratorVariable, WorkerConfig };
