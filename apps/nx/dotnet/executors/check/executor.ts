import { fileURLToPath } from 'node:url';
import { layer } from '@effect/platform-node/NodeServices';
import type { ExecutorContext } from '@nx/devkit';
import { Array, Console, Effect, FileSystem, flow, Path, Schema, Struct } from 'effect';

// --- [MODELS] --------------------------------------------------------------------------

const Check = Schema.Struct({ reports: Schema.Array(Schema.String) });
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

const execute = flow(
    Effect.fnUntraced(function* (options: unknown, context: ExecutorContext) {
        const [path, fs] = yield* Effect.all([Path.Path, FileSystem.FileSystem]);
        const { reports } = yield* Schema.decodeUnknownEffect(Check)(options);
        const read = yield* Effect.forEach(reports, (report) => Effect.flatMap(fs.readFileString(path.resolve(context.root, report)), Schema.decodeEffect(Report)), { concurrency: 'unbounded' });
        const findings = read
            .flatMap(Struct.get('runs'))
            .flatMap(Struct.get('results'))
            .filter((result) => result.suppressions === undefined && Schema.is(Failing)(result.level));
        yield* Effect.forEach(findings, (finding) => Console.log(rendered(finding)));
        return { success: findings.length === 0 };
    }, Effect.provide(layer)),
    Effect.runPromise,
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
