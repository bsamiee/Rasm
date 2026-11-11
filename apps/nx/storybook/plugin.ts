import { inferred, line, NODE, type Request } from '@rasm/nx-devkit/plugin';
import { type Answer, type Asked, consulted, Timed } from '@rasm/nx-devkit/reader';
import { Array, Option } from 'effect';
import { Job, Report } from '#/reader';

// --- [CONSTANTS] -----------------------------------------------------------------------

const MODE = { NODE_ENV: 'production' } as const;

// --- [COMPOSITION] ---------------------------------------------------------------------

const { createNodes } = inferred({
    module: import.meta.url,
    files: ['.storybook/main.{js,ts,jsx,tsx,mjs,mts,cjs,cts}'],
    options: Timed,
    ...consulted({ entry: import.meta.resolve('#/reader'), job: Job, report: Report, launch: () => MODE, jobs: ({ files, path }: Request<typeof Timed.Type>) => files.map((file) => Job.make({ root: path.dirname(path.dirname(file)), file })) }),
    projects: ({ report: { packages, signals } }: Answer<typeof Report.Type>, { job: { root, file }, chain }: Asked<typeof Job>, { path, exec, placed }) => {
        const inputs = ['default', '^default', ...chain.map((at) => path.join('{workspaceRoot}', at)), NODE, { externalDependencies: Array.dedupe(['storybook', ...packages]) }];
        const output = placed('storybook');
        return [
            {
                file,
                root,
                technologies: ['storybook'],
                clean: 'storybook',
                members: {
                    build: {
                        storybook: {
                            command: line(['storybook', 'build', ...Option.match(output, { onNone: () => [], onSome: (at) => ['--output-dir', path.relative(root, at)] })]),
                            cache: true,
                            inputs,
                            outputs: [Option.match(output, { onNone: () => '{projectRoot}/storybook-static', onSome: (at) => path.join('{workspaceRoot}', at) })],
                            metadata: { description: 'Builds the static Storybook of the project stories', help: { command: `${exec} storybook build --help`, example: { args: ['--test'] } } },
                        },
                    },
                    ...(signals && {
                        serve: {
                            storybook: { command: 'storybook dev --ci', continuous: true, readyWhen: { signal: true }, inputs, metadata: { description: 'Serves the project stories from the Storybook development server', help: { command: `${exec} storybook dev --help`, example: { args: ['--port=<port>'] } } } },
                        },
                    }),
                },
            },
        ];
    },
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
