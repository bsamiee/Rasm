import { createRequire } from 'node:module';
import { execPath } from 'node:process';
import { loadConfigFile } from '@nx/devkit/internal';
import type { PlaywrightTestConfig } from '@playwright/test';
import { ran, runtime, ToolError } from '@rasm/nx-devkit/process';
import { served } from '@rasm/nx-devkit/reply';
import { Array, Effect, Option, Path, Predicate, Record, Schema } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const REPORTS: Record.ReadonlyRecord<string, string> = { html: 'playwright-report', blob: 'blob-report', coverage: 'coverage', perfetto: 'test-results' };

// --- [MODELS] --------------------------------------------------------------------------

const Job = Schema.Struct({ root: Schema.String, file: Schema.String, flags: Schema.Array(Schema.String), directed: Schema.Array(Schema.String) }).annotate({
    description: 'One Playwright config read under the flags its target passes, with the reporters its target directs through a variable',
});
const Report = Schema.Struct({ folders: Schema.optionalKey(Schema.Array(Schema.String)) }).annotate({ description: 'Workspace-relative paths the run writes, absent where Playwright lists no test' });
const Reported = Schema.Struct({ outputFile: Schema.optionalKey(Schema.String), outputDir: Schema.optionalKey(Schema.String), outputFolder: Schema.optionalKey(Schema.String) }).annotate({
    description: 'The output options of a Playwright reporter entry, read in the order @nx/playwright reads them',
});
const Listing = Schema.fromJsonString(Schema.Struct({ config: Schema.Struct({ projects: Schema.Array(Schema.Struct({ outputDir: Schema.String })) }), suites: Schema.Array(Schema.Unknown) })).annotate({
    description: 'The JSONReport of playwright test --list --reporter=json',
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const read = Effect.fnUntraced(function* ({ root, file, flags, directed }: typeof Job.Type, workspaceRoot: string) {
    const path = yield* Path.Path;
    const folder = path.join(workspaceRoot, root);
    const { reporter = [] } = yield* Effect.tryPromise({ try: () => loadConfigFile<PlaywrightTestConfig>(path.join(workspaceRoot, file)), catch: ToolError.of(file, 'does not load as a Playwright config') });
    const cli = yield* Effect.try({ try: () => createRequire(path.join(folder, 'package.json')).resolve('@playwright/test/cli'), catch: ToolError.of(file, `resolves no @playwright/test from ${root}`) });
    const listing = yield* Effect.mapError(ran(execPath, [cli, 'test', '--list', '--pass-with-no-tests', '--reporter=json', '--config', path.join(workspaceRoot, file), ...flags], { cwd: folder }), ToolError.of(file, 'cannot start the Playwright listing'));
    const { config, suites } = yield* Effect.mapError(Schema.decodeEffect(Listing)(listing.stdout), ToolError.of(file, `lists no report: ${listing.stderr.trim()}`));
    const reports = Array.getSomes(
        (Predicate.isString(reporter) ? [[reporter, {}] as const] : reporter).map(([name, options]) =>
            Option.firstSomeOf([Option.flatMap(Schema.decodeUnknownOption(Reported)(options), ({ outputFile, outputDir, outputFolder }) => Option.fromUndefinedOr(outputFile ?? outputDir ?? outputFolder)), Option.filter(Record.get(REPORTS, name), () => !directed.includes(name))]),
        ),
    );
    const folders = Array.dedupe([...config.projects.map(({ outputDir }) => outputDir), ...reports].map((at) => path.relative(workspaceRoot, path.resolve(folder, at)))).filter((at, _, all) => !all.some((other) => at.startsWith(`${other}/`)));
    const listed = listing.code !== 0 || suites.length > 0;
    return { report: Report.make(listed ? { folders } : {}), sources: listed ? [] : [path.join(folder, '**')] };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

if (import.meta.main) {
    runtime.runPromise(served(Job, Report, read));
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Job, Report };
