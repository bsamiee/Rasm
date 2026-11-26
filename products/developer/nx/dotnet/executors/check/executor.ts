import { fileURLToPath } from 'node:url';
import { batched, executor } from '@rasm/nx/inference';
import { Array, Cause, Clock, Console, Effect, FileSystem, Path, Record, Schema, Stream, Struct } from 'effect';

// --- [MODELS] --------------------------------------------------------------------------

const Check = Schema.Struct({ reports: Schema.Array(Schema.String).annotate({ description: "Workspace-relative SARIF files the configuration's compile writes, which the plugin infers from each evaluation's ErrorLog" }) }).annotate({
    title: 'Check',
    description: "Reads the SARIF 2.1 reports a project's compile wrote through ErrorLog and fails on each result at level warning or error that no suppression answers, printed as file(line,col): level id: text",
});
const Failing = Schema.Literals(['warning', 'error']);
const Result = Schema.Struct({
    ruleId: Schema.String,
    level: Schema.Literals(['none', 'note', ...Failing.literals]),
    message: Schema.Struct({ text: Schema.String }),
    suppressions: Schema.optionalKey(Schema.Array(Schema.Unknown)),
    locations: Schema.optionalKey(Schema.Array(Schema.Struct({ physicalLocation: Schema.Struct({ artifactLocation: Schema.Struct({ uri: Schema.URLFromString }), region: Schema.Struct({ startLine: Schema.Int, startColumn: Schema.Int }) }) }))),
});
const Report = Schema.fromJsonString(Schema.Struct({ runs: Schema.Array(Schema.Struct({ results: Schema.Array(Result) })) }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const findings = Effect.fnUntraced(function* (root: string, reports: readonly string[]) {
    const [path, fs] = yield* Effect.all([Path.Path, FileSystem.FileSystem]);
    const read = yield* Effect.forEach(reports, (report) => Effect.flatMap(fs.readFileString(path.resolve(root, report)), Schema.decodeEffect(Report)), { concurrency: 'unbounded' });
    return read
        .flatMap(Struct.get('runs'))
        .flatMap(Struct.get('results'))
        .filter((result) => result.suppressions === undefined && Schema.is(Failing)(result.level))
        .map(({ ruleId, level, message, locations = [] }) => [...Array.take(locations, 1).map(({ physicalLocation: at }) => `${fileURLToPath(at.artifactLocation.uri)}(${at.region.startLine},${at.region.startColumn})`), `${level} ${ruleId}`, message.text].join(': '));
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = executor(Check, function* ({ reports }, context) {
    const lines = yield* findings(context.root, reports);
    yield* Effect.forEach(lines, (line) => Console.log(line));
    return { success: lines.length === 0 };
});

const batch = batched(Check, (_, inputs, context) =>
    Stream.mapEffect(
        Stream.fromIterable(Record.toEntries(inputs)),
        ([task, { reports }]) =>
            Effect.map(
                Effect.all([Clock.currentTimeMillis, Effect.matchCause(findings(context.root, reports), { onSuccess: (lines) => ({ success: lines.length === 0, terminalOutput: lines.join('\n') }), onFailure: (cause) => ({ success: false, terminalOutput: Cause.pretty(cause) }) }), Clock.currentTimeMillis]),
                ([startTime, read, endTime]) => ({ task, result: { startTime, endTime, ...read } }),
            ),
        { concurrency: 'unbounded' },
    ),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { batch, Check, execute };
