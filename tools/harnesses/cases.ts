import { Array, Effect, FileSystem, Option, Path, Record, Schema, UndefinedOr } from 'effect';
import { JsonObject } from '#/values';

// --- [TYPES] ---------------------------------------------------------------------------

type Case = (typeof Batch.Type.cases)[number];

// --- [CONSTANTS] -----------------------------------------------------------------------

const CASES = '.cases.json';
const EXTENDSCRIPT = ['jsx', 'jsxbin', 'js'] as const;

// --- [MODELS] --------------------------------------------------------------------------

const Name = Schema.NonEmptyString.annotate({ description: 'Case name, unique in its batch, the question a probe asks' });
const Expression = Schema.NonEmptyString.annotate({ description: 'Host expression over the case document, application, and made items' });
const Reads = Schema.Record(Schema.String, Expression).annotate({ description: 'Result key to host expression' });
const Precondition = JsonObject.annotate({ description: 'Host precondition rows, case rows merged over batch rows before the host decodes them' });
const Keywords = JsonObject.annotate({ description: 'Keyword arguments beside args, operator properties among them' });
const answers = {
    dialogs: Schema.optionalKey(Schema.Array(Schema.Struct({ answer: Schema.optionalKey(Schema.Boolean), fields: Schema.optionalKey(JsonObject) })).annotate({ description: 'Answers to each shown script dialog in order' })),
    windows: Schema.optionalKey(Schema.Array(Schema.Struct({ answer: Schema.optionalKey(Schema.Int), fields: Schema.optionalKey(JsonObject), presses: Schema.optionalKey(Schema.Array(Schema.String)) })).annotate({ description: 'Answers to each shown script window in order' })),
    confirms: Schema.optionalKey(Schema.Array(Schema.Boolean)),
    prompts: Schema.optionalKey(Schema.Array(Schema.NullOr(Schema.String))),
    files: Schema.optionalKey(Schema.Array(Schema.NullOr(Schema.String)).annotate({ description: 'Paths under the product folder each file or folder picker returns in order' })),
    presses: Schema.optionalKey(Schema.Array(Schema.String).annotate({ description: 'Answer the watcher gives each host dialog in order: accept, discard, cancel, or a button label' })),
};
const expected = {
    expect: Schema.optionalKey(Schema.Literals(['value', 'read', 'writes']).annotate({ description: 'Host answer compare checks: call value, read map, or rebuilt writes settled over before' })),
    keys: Schema.optionalKey(Schema.Array(Schema.String).annotate({ description: 'Read keys compare checks, every key when absent' })),
};
const Lines = Schema.Array(Schema.Tuple([Schema.Int, Schema.Int])).annotate({ description: 'Inclusive one-based line ranges of the script kept' });
const Vector = Schema.fromJsonString(Schema.Struct({ cases: Schema.Array(Schema.Struct({ name: Name, fn: Schema.NonEmptyString, args: Schema.Array(Schema.Json) })) }));
const shared = {
    name: Name,
    precondition: Schema.optionalKey(Precondition),
    read: Schema.optionalKey(Reads),
    until: Schema.optionalKey(Expression.annotate({ description: 'Completion condition evaluated between host event-loop passes' })),
    commands: Schema.optionalKey(Schema.Array(Schema.NonEmptyString).annotate({ description: 'Macros the host runs one per command on the case document, each answering its own prompts' })),
    ...answers,
    ...expected,
};
const Probe = Schema.Struct({ ...shared, evaluate: Expression });
const Call = Schema.Struct({ ...shared, call: Expression, args: Schema.Array(Schema.Json), fn: Schema.optionalKey(Schema.NonEmptyString), script: Schema.optionalKey(Schema.NonEmptyString), lines: Schema.optionalKey(Lines), keywords: Schema.optionalKey(Keywords) });
const Run = Schema.Struct({ ...shared, script: Schema.optionalKey(Schema.NonEmptyString), fn: Schema.optionalKey(Schema.NonEmptyString), args: Schema.optionalKey(Schema.Array(Schema.Json)), keywords: Schema.optionalKey(Keywords) });
const Batch = Schema.Struct({
    script: Schema.optionalKey(Schema.NonEmptyString.annotate({ description: 'Script path under the product folder for cases naming none' })),
    module: Schema.optionalKey(Schema.NonEmptyString.annotate({ description: 'Rebuilt module path under the product folder compare imports or runs in the host' })),
    limit: Schema.DurationFromString.pipe(Schema.withDecodingDefaultKey(Effect.succeed('2 minutes' as const))).annotate({ description: 'Bound on a launch, on a case writing no record before it records hung, and on the batch-end quit before the instance is reported left running' }),
    plugin: Schema.optionalKey(Schema.NonEmptyString.annotate({ description: 'Plug-in binary under the product folder the host loads before the cases' })),
    precondition: Schema.optionalKey(Precondition),
    read: Schema.optionalKey(Reads),
    ...expected,
    vector: Schema.optionalKey(
        Schema.Struct({
            file: Schema.NonEmptyString.annotate({ description: 'Vector file under the product folder' }),
            calls: Schema.Record(Schema.String, Expression).annotate({ description: 'Vector function to the original expression called with its args' }),
        }),
    ),
    cases: Schema.Array(Schema.Union([Probe, Call, Run])),
});
const Encoded = Schema.fromJsonString(Batch);

// --- [OPERATIONS] ----------------------------------------------------------------------

const scriptOf = (batch: Pick<typeof Batch.Type, 'script'>, row: Case) => Option.fromUndefinedOr(Schema.is(Probe)(row) ? undefined : (row.script ?? batch.script));
const vectored =
    (calls: Record.ReadonlyRecord<string, string>) =>
    (row: (typeof Vector.Type.cases)[number]): Option.Option<typeof Call.Type> =>
        Option.map(Record.get(calls, row.fn), (call) => ({ name: row.name, fn: row.fn, call, args: row.args }));
const expand = Effect.fn('expand')(function* (product: string, file: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const batch = yield* Schema.decodeEffect(Encoded)(yield* fs.readFileString(file));
    const called: readonly (typeof Call.Type)[] = yield* UndefinedOr.match(batch.vector, {
        onUndefined: () => Effect.succeed([]),
        onDefined: ({ file: vector, calls }) =>
            fs.readFileString(path.join(product, vector)).pipe(
                Effect.flatMap(Schema.decodeEffect(Vector)),
                Effect.map(({ cases: rows }) => Array.getSomes(rows.map(vectored(calls)))),
            ),
    });
    return { batch, rows: [...batch.cases, ...called] };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { answers, Batch, CASES, Call, type Case, EXTENDSCRIPT, Expression, expand, Keywords, Name, Probe, Reads, scriptOf };
