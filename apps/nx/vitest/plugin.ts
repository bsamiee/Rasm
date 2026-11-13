import { matchesGlob } from 'node:path';
import { inferred, line, NODE, type Request } from '@rasm/nx-devkit/plugin';
import { type Answer, type Asked, chained, consulted, Timed } from '@rasm/nx-devkit/reader';
import { Array, Effect, Option, type Path, Record, Result, Schema, Struct } from 'effect';
import { Job, Report } from '#/reader';

// --- [TYPES] ---------------------------------------------------------------------------

interface Chained extends Answer<typeof Report.Type> {
    readonly chains: Record.ReadonlyRecord<string, readonly string[]>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const EXTENSIONS = '{js,mjs,ts,cjs,mts,cts}';
const SUITE = `vitest.config.${EXTENSIONS}`;
const MODE = { TEST: 'true', VITEST: 'true', NODE_ENV: 'test' } as const;

// --- [MODELS] --------------------------------------------------------------------------

const Options = Timed.mapFields(Struct.assign({ ciTargetName: Schema.optionalKey(Schema.String) })).annotate({
    description: 'deadline: longest wait for one config to answer before its reader child is stopped. ciTargetName: name of a target family running each test file as its own cached task, <ciTargetName>--<test file>, under a cached parent <ciTargetName>; absent infers none',
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const directed = (path: Path.Path, to: string) => ['--coverage.reportsDirectory', path.join(to, 'coverage'), '--attachmentsDir', path.join(to, 'attachments'), '--outputFile.junit', path.join(to, 'junit.xml')];

const reader = consulted({
    entry: import.meta.resolve('#/reader'),
    job: Job,
    report: Report,
    launch: () => MODE,
    jobs: ({ files, path, placed }: Request<typeof Options.Type>) =>
        Record.collect(
            Array.groupBy(files, (file) => path.dirname(file)),
            (root, group) => {
                const flags = Option.match(placed('vitest'), { onNone: () => [], onSome: (at) => directed(path, path.relative(root, at)) });
                return Option.match(
                    Array.findFirst(group, (file) => matchesGlob(path.basename(file), SUITE)),
                    { onSome: (file) => Job.cases.Suite.make({ root, file, flags }), onNone: () => Job.cases.Embedded.make({ root, file: Array.headNonEmpty(group), flags }) },
                );
            },
        ),
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const { createNodes } = inferred({
    module: import.meta.url,
    files: [SUITE, `vite.config.${EXTENSIONS}`],
    options: Options,
    units: reader.units,
    read: Effect.fnUntraced(function* (missing: Array.NonEmptyReadonlyArray<Asked<typeof Job>>, request: Request<typeof Options.Type>) {
        const readings = yield* reader.read(missing, request);
        const suites = readings.flatMap(Result.match({ onFailure: () => [], onSuccess: ({ report }) => report.report.run?.suites ?? [] }));
        const chains = Record.fromEntries(yield* chained(request.context.workspaceRoot, Array.dedupe(suites.map(({ root }) => root))));
        return readings.map(Result.map((reading) => ({ ...reading, report: { ...reading.report, chains } })));
    }),
    sources: (answer: Chained, asked: Asked<typeof Job>, { path }) => ({
        tracked: [...reader.sources(answer, asked).tracked, ...(answer.report.run?.suites ?? []).flatMap(({ root, folders }) => [...(answer.chains[root] ?? []), ...folders.map((folder) => path.join(folder, '**'))])],
    }),
    projects: ({ report: { run }, chains }: Chained, { job: { root: home, file, flags } }: Asked<typeof Job>, { path, exec, placed, options: { ciTargetName } }) =>
        Option.toArray(Option.fromUndefinedOr(run)).flatMap(({ suites, folders, coverage, loaded }) => {
            const within = placed('vitest');
            const held = Option.toArray(within);
            const outputs = [...held, ...folders.filter((at) => !held.some((folder) => at.startsWith(`${folder}/`)))].map((at) => path.join('{workspaceRoot}', at));
            return suites.map(({ root, projects, files }) => {
                const selected = ['vitest', 'run', ...projects.flatMap((name) => ['--project', name])];
                const outside = (at: string) => !at.startsWith(`${root}/`);
                const suite = {
                    command: [line(selected), ...Array.match(flags, { onEmpty: () => [], onNonEmpty: (stated) => ['{args}', line(stated)] })].join(' '),
                    options: { cwd: home },
                    dependsOn: ['codegen--*', '^codegen'],
                    cache: true,
                    inputs: ['default', '^default', ...[...(chains[root] ?? []), ...loaded].filter(outside).map((at) => path.join('{workspaceRoot}', at)), { dependentTasksOutputFiles: '**/*', transitive: true }, { env: 'CI' }, NODE, { externalDependencies: ['vitest'] }],
                    metadata: { description: 'Runs every Vitest project of the suite with vitest run', help: { command: `${exec} vitest run --help`, example: { args: ['--project', '<name>'] } } },
                    outputs,
                };
                const alone = (at: string) => {
                    const spec = path.relative(home, path.join(root, at));
                    const written = Option.match(within, { onNone: () => path.join(coverage, spec), onSome: (folder) => path.join(folder, at) });
                    const stated = Option.match(within, { onNone: () => ['--coverage.reportsDirectory', path.relative(home, written)], onSome: () => directed(path, path.relative(home, written)) });
                    return { ...suite, command: line([...selected, spec, ...stated]), outputs: [path.join('{workspaceRoot}', written)], metadata: { ...suite.metadata, description: `Runs ${at} alone with vitest run` } };
                };
                return {
                    file,
                    root,
                    technologies: ['vitest'],
                    clean: 'vitest',
                    members: { test: { vitest: suite } },
                    ...(ciTargetName !== undefined && { atoms: { name: ciTargetName, verb: 'test', tool: 'vitest', each: Record.fromIterableWith(files, (at) => [at, alone(at)] as const) } }),
                };
            });
        }),
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
