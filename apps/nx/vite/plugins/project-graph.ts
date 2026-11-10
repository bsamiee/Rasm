import { matchesGlob } from 'node:path';
import type { CreateNodes, CreateNodesContext, CreateNodesResultArray, TargetConfiguration } from '@nx/devkit';
import { aggregated, GENERATED, identity, inferred, launcher, line, type Members, reported } from '@rasm/nx-devkit/inference';
import { resolved } from '@rasm/nx-devkit/reader';
import { Array, Effect, FileSystem, Option, Path, pipe, Record, Schema } from 'effect';
import { Job, Report } from '#/plugins/reader';

// --- [TYPES] ---------------------------------------------------------------------------

interface Read {
    readonly job: typeof Job.Type;
    readonly report: typeof Report.Type;
    readonly chain: readonly string[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const READER = import.meta.resolve('#/plugins/reader');
const MARKERS = { vite: 'vite.config.{js,mjs,ts,cjs,mts,cts}', vitest: 'vitest.config.{js,mjs,ts,cjs,mts,cts}' } as const;
const TESTING = { TEST: 'true', VITEST: 'true', NODE_ENV: 'test' } as const;
const MODES = { Bundle: { NODE_ENV: 'production' }, Suite: TESTING, Embedded: TESTING } as const;

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({ outputPath: Schema.optionalKey(Schema.String), deadline: Schema.DurationFromString.pipe(Schema.withDecodingDefaultKey(Effect.succeed('1 minute'))) }).annotate({ description: 'Plugin options from the nx.json row' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const placed = (path: Path.Path, root: string, outputPath: Option.Option<string>, tool: keyof typeof MARKERS) => Option.map(outputPath, (at) => path.relative(root, path.join(at, tool, '{projectName}')));

const targets = (path: Path.Path, exec: string, cleaner: string, root: string, read: readonly Read[], outputPath: Option.Option<string>): Readonly<Record<string, TargetConfiguration>> => {
    const command = (argv: readonly string[], rest: readonly string[], technology: string, description: string, example: readonly string[]) => ({
        executor: 'nx:run-commands',
        options: { command: [line(argv), ...rest].join(' '), cwd: root },
        dependsOn: [...GENERATED.dependsOn],
        cache: true,
        metadata: { technologies: [technology], description, help: { command: `${exec} ${line([...argv, '--help'])}`, example: { args: [...example] } } },
    });
    const inputs = (chain: readonly string[], runner: string, ...extra: NonNullable<TargetConfiguration['inputs']>): NonNullable<TargetConfiguration['inputs']> => ['default', '^default', ...chain.map((at) => path.join('{workspaceRoot}', at)), ...GENERATED.inputs, ...extra, { externalDependencies: [runner] }];
    const written = (folders: readonly string[]) => folders.map((at) => path.join('{workspaceRoot}', at));
    const built = (folders: readonly string[], chain: readonly string[]): Members => ({
        build: {
            vite: {
                ...command(['vite', 'build'], Option.match(placed(path, root, outputPath, 'vite'), { onNone: () => [], onSome: (at) => [line(['--outDir', at, '--emptyOutDir'])] }), 'vite', 'Builds every environment of the Vite config with vite build', ['--mode', 'production']),
                inputs: inputs(chain, 'vite'),
                outputs: written(folders),
            },
        },
    });
    const within = Option.map(placed(path, root, outputPath, 'vitest'), (at) => path.join(root, at));
    const tested = ({ flags }: typeof Job.cases.Suite.Type | typeof Job.cases.Embedded.Type, folders: readonly string[], chain: readonly string[]): Members => ({
        test: {
            vitest: {
                ...command(['vitest', 'run'], Array.match(flags, { onEmpty: () => [], onNonEmpty: (directed) => ['{args}', line(directed)] }), 'vitest', 'Runs every Vitest project of the suite with vitest run', ['--project', '<name>']),
                inputs: inputs(chain, 'vitest', { env: 'CI' }),
                outputs: written([...Option.toArray(within), ...folders.filter((at) => !Option.exists(within, (folder) => at.startsWith(`${folder}/`)))]),
            },
        },
    });
    const members = read.map(({ job, report, chain }) => Option.map(Option.fromUndefinedOr(report.folders), (folders) => Job.match(job, { Bundle: () => built(folders, chain), Suite: (suite) => tested(suite, folders, chain), Embedded: (suite) => tested(suite, folders, chain) })));
    return aggregated(Record.fromEntries(Array.getSomes(members).flatMap((member) => Record.toEntries<string, Readonly<Record<string, TargetConfiguration>>>(member))), ['vite'], { executor: cleaner, tool: 'vite' });
};

const nodes = Effect.fnUntraced(function* (files: readonly string[], options: typeof Options.Encoded | undefined, { workspaceRoot }: CreateNodesContext) {
    const [fs, path, decoded] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Schema.decodeEffect(Options)(options ?? {})]);
    const found = Array.getSomes(
        files.map((file) =>
            pipe(
                Array.findFirst(Record.toEntries(MARKERS), ([, pattern]) => matchesGlob(path.basename(file), pattern)),
                Option.map(([kind]) => ({ kind, file, root: path.dirname(file) })),
            ),
        ),
    );
    const projects = yield* Effect.filter(Record.toEntries(Array.groupBy(found, ({ root }) => root)), ([root]) => fs.exists(path.join(workspaceRoot, root, 'package.json')), { concurrency: 'unbounded' });
    const plugin = yield* identity(import.meta.url, workspaceRoot);
    const outputPath = Option.fromUndefinedOr(decoded.outputPath);
    const jobs = projects.flatMap(([root, group]) => {
        const markers: Readonly<Record<string, string>> = Record.fromIterableWith(group, ({ kind, file }) => [kind, file]);
        const [vite, vitest] = [Record.get(markers, 'vite'), Record.get(markers, 'vitest')];
        const bundle = Option.map(vite, (file) => Job.cases.Bundle.make({ root, file, ...Record.getSomes({ outDir: placed(path, root, outputPath, 'vite') }) }));
        const flags = Option.match(placed(path, root, outputPath, 'vitest'), { onNone: () => [], onSome: (at) => ['--coverage.reportsDirectory', path.join(at, 'coverage'), '--attachmentsDir', path.join(at, 'attachments'), '--outputFile.junit', path.join(at, 'junit.xml')] });
        const embedded = Option.map(vite, (file) => Job.cases.Embedded.make({ root, file, flags }));
        const suite = Option.map(vitest, (file) => Job.cases.Suite.make({ root, file, flags }));
        return [...Option.toArray(bundle), ...Option.toArray(Option.orElse(suite, () => embedded))];
    });
    const { answers, failures } = yield* resolved({ entry: READER, job: Job, report: Report, launch: (job) => MODES[job._tag], plugin, options, workspaceRoot, jobs, deadline: decoded.deadline });
    const exec = launcher(workspaceRoot);
    const failed = new Set(failures.map(([file]) => path.dirname(file)));
    return yield* reported(
        Object.values(
            Array.groupBy(
                answers.filter(({ job }) => !failed.has(job.root)),
                ({ job }) => job.root,
            ),
        ).map((members): CreateNodesResultArray[number] => {
            const { root, file } = Array.headNonEmpty(members).job;
            return [file, { projects: { [root]: { targets: targets(path, exec, plugin.cleaner, root, members, outputPath) } } }];
        }),
        failures,
    );
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes<typeof Options.Encoded> = [`**/{${Object.values(MARKERS).join(',')}}`, (files, options, context) => inferred(nodes(files, options, context))];

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
