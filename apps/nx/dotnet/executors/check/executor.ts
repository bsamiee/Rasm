import { fileURLToPath } from 'node:url';
import type { ExecutorContext } from '@nx/devkit';
import { Array, Cause, Console, Effect, FileSystem, Path, Record, Schema, Stream, Struct } from 'effect';
import { run, streamed } from '#/executors/build/executor';

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

const rendered = ({ ruleId, level, message, locations }: typeof Result.Type) => [...Array.take(locations ?? [], 1).map(({ physicalLocation: { artifactLocation, region } }) => `${fileURLToPath(artifactLocation.uri)}(${region.startLine},${region.startColumn})`), `${level} ${ruleId}`, message.text].join(': ');

const findings = Effect.fnUntraced(function* (root: string, { reports }: typeof Check.Type) {
    const [path, fs] = yield* Effect.all([Path.Path, FileSystem.FileSystem]);
    const read = yield* Effect.forEach(reports, (report) => Effect.flatMap(fs.readFileString(path.resolve(root, report)), Schema.decodeEffect(Report)), { concurrency: 'unbounded' });
    return read
        .flatMap(Struct.get('runs'))
        .flatMap(Struct.get('results'))
        .filter((result) => result.suppressions === undefined && Schema.is(Failing)(result.level))
        .map(rendered);
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = Effect.fnUntraced(function* (check: typeof Check.Type, context: ExecutorContext) {
    const lines = yield* findings(context.root, check);
    yield* Effect.forEach(lines, (line) => Console.log(line));
    return { success: lines.length === 0 };
}, run);

const batch = (_: unknown, inputs: Readonly<Record<string, typeof Check.Type>>, __: unknown, context: ExecutorContext) =>
    streamed(
        Stream.mapEffect(
            Stream.fromIterable(Record.toEntries(inputs)),
            ([task, check]) =>
                Effect.matchCause(findings(context.root, check), {
                    onSuccess: (lines) => ({ task, result: { success: lines.length === 0, terminalOutput: lines.join('\n') } }),
                    onFailure: (cause) => ({ task, result: { success: false, terminalOutput: Cause.pretty(cause) } }),
                }),
            { concurrency: 'unbounded' },
        ),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { batch, Check, execute };
