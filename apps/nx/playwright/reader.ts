import { createRequire } from 'node:module';
import { execPath } from 'node:process';
import { loadConfigFile } from '@nx/devkit/internal';
import type { PlaywrightTestConfig } from '@playwright/test';
import { ran, runtime, ToolError } from '@rasm/nx-devkit/process';
import { outermost, served } from '@rasm/nx-devkit/reply';
import { Array, Effect, Option, Path, Predicate, Record, Schema } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const REPORTERS: Record.ReadonlyRecord<string, readonly [name: string, defaulted: boolean, variable?: string]> = {
    junit: ['junit.xml', false, 'PLAYWRIGHT_JUNIT_OUTPUT_FILE'],
    html: ['playwright-report', true, 'PLAYWRIGHT_HTML_OUTPUT_DIR'],
    blob: ['blob-report', true, 'PLAYWRIGHT_BLOB_OUTPUT_DIR'],
    coverage: ['coverage', true],
    perfetto: ['test-results', true],
};
const TASK = /(?:^|\s)nx (?:run (?<project>[^\s:]+):(?<target>[^\s:]+)(?::\S*)?|(?<target>\S+) (?<project>\S+))$/u;

// --- [MODELS] --------------------------------------------------------------------------

const Job = Schema.Struct({ root: Schema.String, file: Schema.String, flags: Schema.Array(Schema.String), directed: Schema.Array(Schema.String) }).annotate({
    description: 'One Playwright config read under the flags its target passes, with the reporters its target directs through a variable',
});
const Served = Schema.Struct({ project: Schema.String, target: Schema.String });
const Report = Schema.Struct({
    folders: Schema.optionalKey(Schema.Array(Schema.String)),
    servers: Schema.Array(Served),
    files: Schema.Array(Schema.String),
    output: Schema.String,
    merged: Schema.optionalKey(Schema.Struct({ blob: Schema.String, reports: Schema.Array(Schema.String) })),
}).annotate({
    description:
        'Workspace-relative paths the run writes, absent where Playwright lists no test; the Nx tasks its reused webServer commands run; its test files relative to the config folder in name order; the workspace-relative output folder of its first project; and, for a config listing the blob reporter, the workspace-relative blob folder and the outputs of its other reporters',
});
const Reported = Schema.Struct({ outputFile: Schema.optionalKey(Schema.String), outputDir: Schema.optionalKey(Schema.String), outputFolder: Schema.optionalKey(Schema.String) }).annotate({
    description: 'The output options of a Playwright reporter entry, read in the order @nx/playwright reads them',
});
const Listing = Schema.fromJsonString(Schema.Struct({ config: Schema.Struct({ rootDir: Schema.String, projects: Schema.NonEmptyArray(Schema.Struct({ outputDir: Schema.String })) }), suites: Schema.Array(Schema.Struct({ file: Schema.String })) })).annotate({
    description: 'The JSONReport of playwright test --list --reporter=json',
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const read = Effect.fnUntraced(function* ({ root, file, flags, directed }: typeof Job.Type, workspaceRoot: string) {
    const path = yield* Path.Path;
    const folder = path.join(workspaceRoot, root);
    const { reporter = [], webServer = [], projects = [] } = yield* Effect.tryPromise({ try: () => loadConfigFile<PlaywrightTestConfig>(path.join(workspaceRoot, file)), catch: ToolError.of(file, 'does not load as a Playwright config') });
    const cli = yield* Effect.try({ try: () => createRequire(path.join(folder, 'package.json')).resolve('@playwright/test/cli'), catch: ToolError.of(file, `resolves no @playwright/test from ${root}`) });
    const listing = yield* Effect.mapError(ran(execPath, [cli, 'test', '--list', '--pass-with-no-tests', '--reporter=json', '--config', path.join(workspaceRoot, file), ...flags], { cwd: folder }), ToolError.of(file, 'cannot start the Playwright listing'));
    const { config, suites } = yield* Effect.mapError(Schema.decodeEffect(Listing)(listing.stdout), ToolError.of(file, `lists no report: ${listing.stderr.trim()}`));
    const defaults = Record.map(
        Record.filter(REPORTERS, ([, defaulted]) => defaulted),
        ([at]) => at,
    );
    const stated = (Predicate.isString(reporter) ? [[reporter, {}] as const] : reporter).map(([name, options]) => [name, Option.flatMap(Schema.decodeUnknownOption(Reported)(options), ({ outputFile, outputDir, outputFolder }) => Option.fromUndefinedOr(outputFile ?? outputDir ?? outputFolder))] as const);
    const undirected = Record.filter(defaults, (_, name) => !directed.includes(name));
    const reports = Array.getSomes(stated.map(([name, at]) => Option.orElse(at, () => Record.get(undirected, name))));
    const written = stated.map(
        ([name, at]) =>
            [
                name,
                Option.map(
                    Option.orElse(at, () => Record.get(defaults, name)),
                    (to) => path.relative(workspaceRoot, path.resolve(folder, to)),
                ),
            ] as const,
    );
    const merged = Option.map(
        Option.flatMap(
            Array.findFirst(written, ([name]) => name === 'blob'),
            ([, at]) => at,
        ),
        (blob) => ({ blob: path.extname(blob) === '.zip' ? path.dirname(blob) : blob, reports: Array.getSomes(written.filter(([name]) => name !== 'blob').map(([, at]) => at)) }),
    );
    const folders = outermost(
        path,
        workspaceRoot,
        [...config.projects.map(({ outputDir }) => outputDir), ...reports].map((at) => path.resolve(folder, at)),
    );
    const listed = listing.code !== 0 || suites.length > 0;
    const servers = Array.getSomes(
        [webServer, ...projects.map((project) => project.webServer ?? [])]
            .flat()
            .filter(({ reuseExistingServer, wait, env }) => reuseExistingServer === true && wait === undefined && env === undefined)
            .map(({ command }) => Schema.decodeUnknownOption(Served)(TASK.exec(command)?.groups)),
    );
    const files = Array.dedupe(suites.map(({ file: at }) => path.relative(folder, path.resolve(config.rootDir, at)))).toSorted();
    return { report: Report.make({ ...(listed && { folders }), servers, files, output: path.relative(workspaceRoot, Array.headNonEmpty(config.projects).outputDir), ...Record.getSomes({ merged }) }), sources: [path.join(folder, '**')] };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

if (import.meta.main) {
    runtime.runPromise(served(Job, Report, read));
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Job, REPORTERS, Report };
