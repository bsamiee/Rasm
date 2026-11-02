import { Schema } from 'effect';
import { JsonObject } from '#/values';

// --- [MODELS] --------------------------------------------------------------------------

const Name = Schema.NonEmptyString.annotate({ description: 'Case name, unique in its batch, the question a probe asks' });
const Expression = Schema.NonEmptyString.annotate({ description: 'Host expression over the case document, application, and made items' });
const Reads = Schema.Record(Schema.String, Expression).annotate({ description: 'Result key to host expression, [*] in both spreading over a collection' });
const Precondition = JsonObject.annotate({ description: 'Host precondition rows, case rows merged over batch rows before the host decodes them' });
const answers = {
    dialogs: Schema.optionalKey(Schema.Array(Schema.Struct({ answer: Schema.optionalKey(Schema.Boolean), fields: Schema.optionalKey(JsonObject) })).annotate({ description: 'Answers to each shown script dialog in order' })),
    windows: Schema.optionalKey(Schema.Array(Schema.Struct({ answer: Schema.optionalKey(Schema.Int), fields: Schema.optionalKey(JsonObject), presses: Schema.optionalKey(Schema.Array(Schema.String)) })).annotate({ description: 'Answers to each shown script window in order' })),
    confirms: Schema.optionalKey(Schema.Array(Schema.Boolean)),
    prompts: Schema.optionalKey(Schema.Array(Schema.NullOr(Schema.String))),
    files: Schema.optionalKey(Schema.Array(Schema.NullOr(Schema.String)).annotate({ description: 'Paths under the product folder each file or folder picker returns in order' })),
    presses: Schema.optionalKey(Schema.Array(Schema.String).annotate({ description: 'Button labels the watcher presses on each host dialog in order' })),
};
const expected = {
    expect: Schema.optionalKey(Schema.Literals(['value', 'read', 'writes']).annotate({ description: 'Host answer compare checks: call value, read map, or rebuilt writes settled over before' })),
    keys: Schema.optionalKey(Schema.Array(Schema.String).annotate({ description: 'Read keys compare checks, every key when absent' })),
};
const Lines = Schema.Array(Schema.Tuple([Schema.Int, Schema.Int])).annotate({ description: 'Inclusive one-based line ranges of the script kept' });
const Vector = Schema.fromJsonString(Schema.Struct({ cases: Schema.Array(Schema.Struct({ name: Name, fn: Schema.NonEmptyString, args: Schema.Array(Schema.Json) })) }));
const shared = { name: Name, precondition: Schema.optionalKey(Precondition), read: Schema.optionalKey(Reads), ...answers, ...expected };
const Probe = Schema.Struct({ ...shared, evaluate: Expression });
const Call = Schema.Struct({ ...shared, call: Expression, args: Schema.Array(Schema.Json), fn: Schema.optionalKey(Schema.NonEmptyString), script: Schema.optionalKey(Schema.NonEmptyString), lines: Schema.optionalKey(Lines) });
const Run = Schema.Struct({ ...shared, script: Schema.optionalKey(Schema.NonEmptyString), fn: Schema.optionalKey(Schema.NonEmptyString), args: Schema.optionalKey(Schema.Array(Schema.Json)) });
const Batch = Schema.Struct({
    script: Schema.optionalKey(Schema.NonEmptyString.annotate({ description: 'Script path under the product folder for cases naming none' })),
    module: Schema.optionalKey(Schema.NonEmptyString.annotate({ description: 'Rebuilt module path under the product folder compare imports' })),
    precondition: Schema.optionalKey(Precondition),
    read: Schema.optionalKey(Reads),
    ...expected,
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

// --- [EXPORTS] -------------------------------------------------------------------------

export { answers, Batch, Call, Expression, Name, Probe, Reads, Run, Vector };
