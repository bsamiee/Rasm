import { fileURLToPath } from 'node:url';
import { executor } from '@rasm/nx-devkit/executor';
import { Array, Console, Effect, FileSystem, Path, Schema, Struct } from 'effect';

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

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = executor(
    Check,
    Effect.fnUntraced(function* ({ reports }, context) {
        const [path, fs] = yield* Effect.all([Path.Path, FileSystem.FileSystem]);
        const read = yield* Effect.forEach(reports, (report) => Effect.flatMap(fs.readFileString(path.resolve(context.root, report)), Schema.decodeEffect(Report)), { concurrency: 'unbounded' });
        const findings = read
            .flatMap(Struct.get('runs'))
            .flatMap(Struct.get('results'))
            .filter((result) => result.suppressions === undefined && Schema.is(Failing)(result.level));
        yield* Effect.forEach(findings, (finding) => Console.log(rendered(finding)));
        return findings.length === 0;
    }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
