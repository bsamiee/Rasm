import web from '@nx/web/package.json' with { type: 'json' };
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
    projects: ({ report: { packages } }: Answer<typeof Report.Type>, { job: { root, file }, chain }: Asked<typeof Job>, { path, exec, placed }) => {
        const inputs = ['default', '^default', ...chain, NODE, { externalDependencies: Array.dedupe(['storybook', ...packages]) }];
        const output = placed('storybook');
        const written = Option.getOrElse(output, () => path.join(root, 'storybook-static'));
        const built = 'build--storybook';
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
                            dependsOn: ['codegen--*', '^codegen'],
                            cache: true,
                            inputs,
                            outputs: [path.join('{workspaceRoot}', written)],
                            metadata: { description: 'Builds the static Storybook of the project stories', help: { command: `${exec} storybook build --help`, example: { args: ['--test'] } } },
                        },
                    },
                    preview: {
                        storybook: {
                            executor: `${web.name}:file-server`,
                            dependsOn: [built],
                            continuous: true,
                            readyWhen: { signal: true },
                            inputs,
                            options: { buildTarget: built, staticFilePath: written, cors: false },
                            metadata: { description: `Serves the static Storybook ${built} writes through the ${web.name} file server, without CORS headers` },
                        },
                    },
                    serve: {
                        storybook: {
                            command: 'storybook dev',
                            continuous: true,
                            readyWhen: { port: '{options.port ?? env.SBCONFIG_PORT ?? env.PORT ?? auto}', host: '{env.SBCONFIG_HOSTNAME ?? options.host ?? localhost}' },
                            inputs,
                            metadata: { description: 'Serves the project stories from the Storybook development server', help: { command: `${exec} storybook dev --help`, example: { args: ['--port=<port>'] } } },
                        },
                    },
                },
            },
        ];
    },
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
