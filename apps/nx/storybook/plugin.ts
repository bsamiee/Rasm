import { inferred, type Request } from '@rasm/nx-devkit/plugin';
import { type Answer, type Asked, consulted, Timed } from '@rasm/nx-devkit/reader';
import { commanded, type Inputs, NODE, SIGNALLED } from '@rasm/nx-devkit/target';
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
        const inputs: Inputs = ['default', '^default', ...chain.map((at) => path.join('{workspaceRoot}', at)), NODE, { externalDependencies: Array.dedupe(['storybook', ...packages]) }];
        const output = placed('storybook');
        return [
            {
                file,
                root,
                technologies: ['storybook'],
                clean: 'storybook',
                members: {
                    build: {
                        storybook: commanded(
                            [['storybook', 'build', ...Option.match(output, { onNone: () => [], onSome: (at) => ['--output-dir', path.relative(root, at)] })]],
                            root,
                            { description: 'Builds the static Storybook of the project stories', help: `${exec} storybook build --help`, example: { args: ['--test'] } },
                            { cache: true, inputs, outputs: [Option.match(output, { onNone: () => '{projectRoot}/storybook-static', onSome: (at) => path.join('{workspaceRoot}', at) })] },
                        ),
                    },
                    ...(signals && {
                        serve: {
                            storybook: commanded([['storybook', 'dev', '--ci']], root, { description: 'Serves the project stories from the Storybook development server', help: `${exec} storybook dev --help`, example: { args: ['--port=<port>'] } }, { ...SIGNALLED, inputs }),
                        },
                    }),
                },
            },
        ];
    },
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
