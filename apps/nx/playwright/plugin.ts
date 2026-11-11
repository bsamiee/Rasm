import { inferred, line, NODE, type Request } from '@rasm/nx-devkit/plugin';
import { type Answer, type Asked, consulted, Timed } from '@rasm/nx-devkit/reader';
import { Option, Record } from 'effect';
import { Job, Report } from '#/reader';

// --- [CONSTANTS] -----------------------------------------------------------------------

const DIRECTED = { PLAYWRIGHT_JUNIT_OUTPUT_FILE: ['junit', 'junit.xml'], PLAYWRIGHT_HTML_OUTPUT_DIR: ['html', 'playwright-report'], PLAYWRIGHT_BLOB_OUTPUT_DIR: ['blob', 'blob-report'] } as const;

// --- [COMPOSITION] ---------------------------------------------------------------------

const { createNodes } = inferred({
    module: import.meta.url,
    files: ['playwright.config.{js,ts,cjs,cts,mjs,mts}'],
    options: Timed,
    ...consulted({
        entry: import.meta.resolve('#/reader'),
        job: Job,
        report: Report,
        launch: () => ({}),
        jobs: ({ files, path, placed }: Request<typeof Timed.Type>) =>
            files.map((file) =>
                Job.make({
                    root: path.dirname(file),
                    file,
                    ...Option.match(placed('playwright'), { onNone: () => ({ flags: [], directed: [] }), onSome: (at) => ({ flags: ['--output', path.join(path.relative(path.dirname(file), at), 'test-results')], directed: Object.values(DIRECTED).map(([reporter]) => reporter) }) }),
                }),
            ),
    }),
    projects: ({ report: { folders, servers }, loaded }: Answer<typeof Report.Type>, { job: { root, file, flags }, chain }: Asked<typeof Job>, { path, exec, placed }) =>
        Option.toArray(Option.fromUndefinedOr(folders)).map((written) => {
            const within = Option.toArray(placed('playwright'));
            return {
                file,
                root,
                technologies: ['playwright'],
                clean: 'playwright',
                members: {
                    test: {
                        playwright: {
                            command: line(['playwright', 'test', ...flags]),
                            options: Option.getOrElse(
                                Option.map(placed('playwright'), (at) => ({ env: Record.map(DIRECTED, ([, name]) => path.join(path.relative(root, at), name)) })),
                                () => ({}),
                            ),
                            dependsOn: servers.map(({ project, target }) => ({ projects: [project], target, waitFor: 'ready' as const })),
                            cache: true,
                            inputs: ['default', '^default', ...[...chain, ...loaded].filter((at) => !at.startsWith(`${root}/`)).map((at) => path.join('{workspaceRoot}', at)), { env: 'CI' }, NODE, { externalDependencies: ['@playwright/test'] }],
                            outputs: [...within, ...written.filter((at) => !within.some((folder) => at.startsWith(`${folder}/`)))].map((at) => path.join('{workspaceRoot}', at)),
                            metadata: { description: 'Runs every Playwright project of the suite with playwright test', help: { command: `${exec} playwright test --help`, example: { args: ['--debug=cli'] } } },
                        },
                    },
                },
            };
        }),
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
