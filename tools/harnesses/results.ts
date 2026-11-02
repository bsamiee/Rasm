import { Schema } from 'effect';

// --- [MODELS] --------------------------------------------------------------------------

const State = Schema.Record(Schema.String, Schema.Json);
const Outcome = Schema.Struct({
    name: Schema.String,
    ran: Schema.Boolean,
    thrown: Schema.optionalKey(Schema.String.annotate({ description: 'Error the original raised, with its line' })),
    exited: Schema.optionalKey(Schema.Boolean),
    failed: Schema.optionalKey(Schema.String.annotate({ description: 'Precondition or harness failure, original not run' })),
    value: Schema.optionalKey(Schema.Json),
    before: Schema.optionalKey(State),
    read: Schema.optionalKey(State),
    readFailed: Schema.optionalKey(Schema.Record(Schema.String, Schema.String)),
    alerts: Schema.Array(Schema.String),
    alertTitles: Schema.Array(Schema.NullOr(Schema.String)),
    confirms: Schema.Array(Schema.String),
    prompts: Schema.Array(Schema.String),
    windows: Schema.Array(Schema.Json),
    dialogs: Schema.Array(Schema.Json),
    files: Schema.Array(Schema.String),
    menus: Schema.Array(Schema.Json),
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const results = <E extends Schema.Top>(engine: E) =>
    Schema.fromJsonString(
        Schema.Struct({
            application: Schema.String,
            version: Schema.String,
            engine,
            failed: Schema.optionalKey(Schema.String),
            cases: Schema.Array(Outcome),
        }),
        { space: 2 },
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { Outcome, results };
