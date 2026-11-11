import { matchesGlob } from 'node:path';
import { inferred, line, NODE, type Request } from '@rasm/nx-devkit/plugin';
import { type Answer, type Asked, consulted, Timed } from '@rasm/nx-devkit/reader';
import { Array, Option, type Path, Record, Schema, Struct } from 'effect';
import { Job, Report } from '#/reader';

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

// --- [COMPOSITION] ---------------------------------------------------------------------

const { createNodes } = inferred({
    module: import.meta.url,
    files: [SUITE, `vite.config.${EXTENSIONS}`],
    options: Options,
    ...consulted({
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
    }),
    projects: ({ report: { run } }: Answer<typeof Report.Type>, { job: { root, file, flags }, chain }: Asked<typeof Job>, { path, exec, placed, options: { ciTargetName } }) =>
        Option.toArray(Option.fromUndefinedOr(run)).map(({ folders, files, coverage }) => {
            const within = placed('vitest');
            const held = Option.toArray(within);
            const suite = {
                command: ['vitest run', ...Array.match(flags, { onEmpty: () => [], onNonEmpty: (stated) => ['{args}', line(stated)] })].join(' '),
                dependsOn: ['codegen--*', '^codegen'],
                cache: true,
                inputs: ['default', '^default', ...chain, { dependentTasksOutputFiles: '**/*', transitive: true }, { env: 'CI' }, NODE, { externalDependencies: ['vitest'] }],
                metadata: { description: 'Runs every Vitest project of the suite with vitest run', help: { command: `${exec} vitest run --help`, example: { args: ['--project', '<name>'] } } },
                outputs: [...held, ...folders.filter((at) => !held.some((folder) => at.startsWith(`${folder}/`)))].map((at) => path.join('{workspaceRoot}', at)),
            };
            const alone = (at: string) => {
                const written = Option.match(within, { onNone: () => path.join(coverage, at), onSome: (folder) => path.join(folder, at) });
                const stated = Option.match(within, { onNone: () => ['--coverage.reportsDirectory', path.relative(root, written)], onSome: () => directed(path, path.relative(root, written)) });
                return { ...suite, command: line(['vitest', 'run', at, ...stated]), outputs: [path.join('{workspaceRoot}', written)], metadata: { ...suite.metadata, description: `Runs ${at} alone with vitest run` } };
            };
            return {
                file,
                root,
                technologies: ['vitest'],
                clean: 'vitest',
                members: { test: { vitest: suite } },
                ...(ciTargetName !== undefined && { atoms: { name: ciTargetName, verb: 'test', tool: 'vitest', each: Record.fromIterableWith(files, (at) => [at, alone(at)] as const) } }),
            };
        }),
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
