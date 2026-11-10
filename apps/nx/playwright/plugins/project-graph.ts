import type { CreateNodes, CreateNodesContext, CreateNodesResultArray, TargetConfiguration } from '@nx/devkit';
import { aggregated, identity, inferred, launcher, line, reported } from '@rasm/nx-devkit/inference';
import { resolved } from '@rasm/nx-devkit/reader';
import { Array, Effect, FileSystem, Option, Path, Record, Schema } from 'effect';
import { Job, Report } from '#/plugins/reader';

// --- [CONSTANTS] -----------------------------------------------------------------------

const READER = import.meta.resolve('#/plugins/reader');
const TECHNOLOGIES = ['playwright'];
const DIRECTED = { PLAYWRIGHT_JUNIT_OUTPUT_FILE: ['junit', 'junit.xml'], PLAYWRIGHT_HTML_OUTPUT_DIR: ['html', 'playwright-report'], PLAYWRIGHT_BLOB_OUTPUT_DIR: ['blob', 'blob-report'] } as const;

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({ outputPath: Schema.optionalKey(Schema.String), deadline: Schema.DurationFromString.pipe(Schema.withDecodingDefaultKey(Effect.succeed('1 minute'))) }).annotate({ description: 'Plugin options from the nx.json row' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const placed = (path: Path.Path, root: string, outputPath: Option.Option<string>) => Option.map(outputPath, (at) => path.relative(root, path.join(at, 'playwright', '{projectName}')));

const tested = (path: Path.Path, exec: string, { root, flags }: typeof Job.Type, read: readonly string[], folders: readonly string[], outputPath: Option.Option<string>): TargetConfiguration => {
    const at = placed(path, root, outputPath);
    const within = Option.map(at, (folder) => path.join(root, folder));
    return {
        executor: 'nx:run-commands',
        options: { command: line(['playwright', 'test', ...flags]), cwd: root, ...Option.getOrUndefined(Option.map(at, (folder) => ({ env: Record.map(DIRECTED, ([, name]) => path.join(folder, name)) }))) },
        cache: true,
        inputs: ['default', '^default', ...read.filter((loaded) => !loaded.startsWith(`${root}/`)).map((loaded) => path.join('{workspaceRoot}', loaded)), { env: 'CI' }, { externalDependencies: ['@playwright/test'] }],
        outputs: [...Option.toArray(within), ...folders.filter((written) => !Option.exists(within, (folder) => written.startsWith(`${folder}/`)))].map((written) => path.join('{workspaceRoot}', written)),
        metadata: { technologies: TECHNOLOGIES, description: 'Runs every Playwright project of the suite with playwright test', help: { command: `${exec} playwright test --help`, example: { args: ['--debug=cli'] } } },
    };
};

const nodes = Effect.fnUntraced(function* (files: readonly string[], options: typeof Options.Encoded | undefined, { workspaceRoot }: CreateNodesContext) {
    const [fs, path, decoded] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Schema.decodeEffect(Options)(options ?? {})]);
    const outputPath = Option.fromUndefinedOr(decoded.outputPath);
    const suites = yield* Effect.filter(
        files.map((file) => {
            const root = path.dirname(file);
            return Job.make({ root, file, ...Option.match(placed(path, root, outputPath), { onNone: () => ({ flags: [], directed: [] }), onSome: (at) => ({ flags: ['--output', path.join(at, 'test-results')], directed: Object.values(DIRECTED).map(([reporter]) => reporter) }) }) });
        }),
        ({ root }) => fs.exists(path.join(workspaceRoot, root, 'package.json')),
        { concurrency: 'unbounded' },
    );
    const plugin = yield* identity(import.meta.url, workspaceRoot);
    const { answers, failures } = yield* resolved({ entry: READER, job: Job, report: Report, launch: () => ({}), plugin, options, workspaceRoot, jobs: suites, deadline: decoded.deadline });
    const exec = launcher(workspaceRoot);
    return yield* reported(
        Array.getSomes(
            answers.map(({ job, report, chain, loaded }) =>
                Option.map(Option.fromUndefinedOr(report.folders), (folders): CreateNodesResultArray[number] => [
                    job.file,
                    { projects: { [job.root]: { targets: aggregated({ test: { playwright: tested(path, exec, job, [...chain, ...loaded], folders, outputPath) } }, TECHNOLOGIES, { executor: plugin.cleaner, tool: 'playwright' }) } } },
                ]),
            ),
        ),
        failures,
    );
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes<typeof Options.Encoded> = ['**/playwright.config.{js,ts,cjs,cts,mjs,mts}', (files, options, context) => inferred(nodes(files, options, context))];

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
