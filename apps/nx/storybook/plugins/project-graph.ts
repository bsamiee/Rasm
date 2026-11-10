import type { CreateNodes, CreateNodesContext, CreateNodesResultArray, TargetConfiguration } from '@nx/devkit';
import { aggregated, identity, inferred, launcher, line, NODE, reported } from '@rasm/nx-devkit/inference';
import { resolved } from '@rasm/nx-devkit/reader';
import { Array, Effect, FileSystem, Option, Path, Schema } from 'effect';
import { Job, Report } from '#/plugins/reader';

// --- [CONSTANTS] -----------------------------------------------------------------------

const READER = import.meta.resolve('#/plugins/reader');
const TECHNOLOGIES = ['storybook'];
const MODE = { NODE_ENV: 'production' } as const;

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({ outputPath: Schema.optionalKey(Schema.String), deadline: Schema.DurationFromString.pipe(Schema.withDecodingDefaultKey(Effect.succeed('1 minute'))) }).annotate({ description: 'Plugin options from the nx.json row' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const built = (path: Path.Path, exec: string, root: string, chain: readonly string[], packages: readonly string[], outputPath: Option.Option<string>): TargetConfiguration => {
    const placed = Option.map(outputPath, (at) => path.relative(root, path.join(at, 'storybook', '{projectName}')));
    return {
        executor: 'nx:run-commands',
        options: { command: line(['storybook', 'build', ...Option.match(placed, { onNone: () => [], onSome: (at) => ['--output-dir', at] })]), cwd: root },
        cache: true,
        inputs: ['default', '^default', ...chain.map((at) => path.join('{workspaceRoot}', at)), NODE, { externalDependencies: Array.dedupe(['storybook', ...packages]) }],
        outputs: [Option.match(placed, { onNone: () => '{projectRoot}/storybook-static', onSome: (at) => path.join('{workspaceRoot}', root, at) })],
        metadata: { technologies: TECHNOLOGIES, description: 'Builds the static Storybook of the project stories', help: { command: `${exec} storybook build --help`, example: { args: ['--test'] } } },
    };
};

const nodes = Effect.fnUntraced(function* (files: readonly string[], options: typeof Options.Encoded | undefined, { workspaceRoot }: CreateNodesContext) {
    const [fs, path, decoded] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Schema.decodeEffect(Options)(options ?? {})]);
    const mains = yield* Effect.filter(
        files.map((file) => Job.make({ root: path.dirname(path.dirname(file)), file })),
        ({ root }) => fs.exists(path.join(workspaceRoot, root, 'package.json')),
        { concurrency: 'unbounded' },
    );
    const plugin = yield* identity(import.meta.url, workspaceRoot);
    const { answers, failures } = yield* resolved({ entry: READER, job: Job, report: Report, launch: () => MODE, plugin, options, workspaceRoot, jobs: mains, deadline: decoded.deadline });
    const exec = launcher(workspaceRoot);
    const outputPath = Option.fromUndefinedOr(decoded.outputPath);
    return yield* reported(
        answers.map(({ job: { root, file }, report, chain }): CreateNodesResultArray[number] => [file, { projects: { [root]: { targets: aggregated({ build: { storybook: built(path, exec, root, chain, report.packages, outputPath) } }, TECHNOLOGIES, { executor: plugin.cleaner, tool: 'storybook' }) } } }]),
        failures,
    );
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes<typeof Options.Encoded> = ['**/.storybook/main.{js,ts,jsx,tsx,mjs,mts,cjs,cts}', (files, options, context) => inferred(nodes(files, options, context))];

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
