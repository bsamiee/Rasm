import { matchesGlob } from 'node:path';
import { inferred, line, NODE, type Request } from '@rasm/nx-devkit/plugin';
import { type Answer, type Asked, consulted, Timed } from '@rasm/nx-devkit/reader';
import { Array, Option, Record } from 'effect';
import { Job, Report } from '#/reader';

// --- [CONSTANTS] -----------------------------------------------------------------------

const EXTENSIONS = '{js,mjs,ts,cjs,mts,cts}';
const SUITE = `vitest.config.${EXTENSIONS}`;
const MODE = { TEST: 'true', VITEST: 'true', NODE_ENV: 'test' } as const;

// --- [COMPOSITION] ---------------------------------------------------------------------

const { createNodes } = inferred({
    module: import.meta.url,
    files: [SUITE, `vite.config.${EXTENSIONS}`],
    options: Timed,
    ...consulted({
        entry: import.meta.resolve('#/reader'),
        job: Job,
        report: Report,
        launch: () => MODE,
        jobs: ({ files, path, placed }: Request<typeof Timed.Type>) =>
            Record.collect(
                Array.groupBy(files, (file) => path.dirname(file)),
                (root, group) => {
                    const flags = Option.match(placed('vitest'), {
                        onNone: () => [],
                        onSome: (at) => ['--coverage.reportsDirectory', path.join(path.relative(root, at), 'coverage'), '--attachmentsDir', path.join(path.relative(root, at), 'attachments'), '--outputFile.junit', path.join(path.relative(root, at), 'junit.xml')],
                    });
                    return Option.match(
                        Array.findFirst(group, (file) => matchesGlob(path.basename(file), SUITE)),
                        { onSome: (file) => Job.cases.Suite.make({ root, file, flags }), onNone: () => Job.cases.Embedded.make({ root, file: Array.headNonEmpty(group), flags }) },
                    );
                },
            ),
    }),
    projects: ({ report: { folders } }: Answer<typeof Report.Type>, { job: { root, file, flags }, chain }: Asked<typeof Job>, { path, exec, placed }) =>
        Option.toArray(Option.fromUndefinedOr(folders)).map((reports) => {
            const within = Option.toArray(placed('vitest'));
            return {
                file,
                root,
                technologies: ['vitest'],
                clean: 'vitest',
                members: {
                    test: {
                        vitest: {
                            command: ['vitest run', ...Array.match(flags, { onEmpty: () => [], onNonEmpty: (directed) => ['{args}', line(directed)] })].join(' '),
                            dependsOn: ['generate', '^generate'],
                            cache: true,
                            inputs: ['default', '^default', ...chain.map((at) => path.join('{workspaceRoot}', at)), { dependentTasksOutputFiles: '**/*', transitive: true }, { env: 'CI' }, NODE, { externalDependencies: ['vitest'] }],
                            metadata: { description: 'Runs every Vitest project of the suite with vitest run', help: { command: `${exec} vitest run --help`, example: { args: ['--project', '<name>'] } } },
                            outputs: [...within, ...reports.filter((at) => !within.some((folder) => at.startsWith(`${folder}/`)))].map((at) => path.join('{workspaceRoot}', at)),
                        },
                    },
                },
            };
        }),
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
