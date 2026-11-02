import { Schema } from 'effect';

// --- [MODELS] --------------------------------------------------------------------------

const Name = Schema.NonEmptyString.annotate({ description: 'Case name, unique in its batch' });
const Expression = Schema.NonEmptyString.annotate({ description: 'Host expression reading doc, app, and made' });
const Reads = Schema.Record(Schema.String, Expression).annotate({ description: 'Result key to host expression, [*] in both spreading over a collection' });
const answers = {
    dialogs: Schema.optionalKey(Schema.Array(Schema.Struct({ answer: Schema.optionalKey(Schema.Boolean), fields: Schema.optionalKey(Schema.Record(Schema.String, Schema.Json)) })).annotate({ description: 'Answers to each shown host dialog in order' })),
    windows: Schema.optionalKey(Schema.Array(Schema.Struct({ answer: Schema.optionalKey(Schema.Int), fields: Schema.optionalKey(Schema.Record(Schema.String, Schema.Json)), presses: Schema.optionalKey(Schema.Array(Schema.String)) })).annotate({ description: 'Answers to each shown script window in order' })),
    confirms: Schema.optionalKey(Schema.Array(Schema.Boolean)),
    prompts: Schema.optionalKey(Schema.Array(Schema.NullOr(Schema.String))),
    files: Schema.optionalKey(Schema.Array(Schema.NullOr(Schema.String)).annotate({ description: 'Paths each file or folder picker returns in order' })),
};
const Lines = Schema.Array(Schema.Tuple([Schema.Int, Schema.Int])).annotate({ description: 'Inclusive one-based line ranges of the script kept' });
const Vector = Schema.fromJsonString(Schema.Struct({ cases: Schema.Array(Schema.Struct({ name: Name, fn: Schema.NonEmptyString, args: Schema.Array(Schema.Json) })) }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const cases = <P extends Schema.Top>(precondition: P) => {
    const shared = { name: Name, precondition: Schema.optionalKey(precondition), read: Schema.optionalKey(Reads), ...answers };
    const Probe = Schema.Struct({ ...shared, evaluate: Expression });
    const Call = Schema.Struct({ ...shared, call: Expression, args: Schema.Array(Schema.Json), fn: Schema.optionalKey(Schema.NonEmptyString), script: Schema.optionalKey(Schema.NonEmptyString), lines: Schema.optionalKey(Lines) });
    const Run = Schema.Struct({ ...shared, script: Schema.optionalKey(Schema.NonEmptyString), fn: Schema.optionalKey(Schema.NonEmptyString), args: Schema.optionalKey(Schema.Array(Schema.Json)) });
    const Batch = Schema.Struct({
        script: Schema.optionalKey(Schema.NonEmptyString.annotate({ description: 'Script path under the product folder for cases naming none' })),
        module: Schema.optionalKey(Schema.NonEmptyString.annotate({ description: 'Rebuilt module path under the product folder compare imports' })),
        precondition: Schema.optionalKey(precondition),
        read: Schema.optionalKey(Reads),
        vector: Schema.optionalKey(
            Schema.Struct({
                file: Schema.NonEmptyString.annotate({ description: 'Vector file under the product folder' }),
                script: Schema.optionalKey(Schema.NonEmptyString.annotate({ description: 'Script whose kept lines load before each call' })),
                lines: Schema.optionalKey(Lines),
                calls: Schema.Record(Schema.String, Expression).annotate({ description: 'Vector function to the original expression called with its args' }),
            }),
        ),
        cases: Schema.Array(Schema.Union([Probe, Call, Run])),
    });
    return { Probe, Call, Run, Batch };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { cases, Vector };
