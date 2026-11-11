import { inferred, line, type Members, NODE, type Request } from '@rasm/nx-devkit/plugin';
import { type Answer, type Asked, consulted, Timed } from '@rasm/nx-devkit/reader';
import { Option, Record } from 'effect';
import { Job, Report } from '#/reader';

// --- [CONSTANTS] -----------------------------------------------------------------------

const MODE = { NODE_ENV: 'production' } as const;

// --- [COMPOSITION] ---------------------------------------------------------------------

const { createNodes } = inferred({
    module: import.meta.url,
    files: ['vite.config.{js,mjs,ts,cjs,mts,cts}'],
    options: Timed,
    ...consulted({
        entry: import.meta.resolve('#/reader'),
        job: Job,
        report: Report,
        launch: () => MODE,
        jobs: ({ files, path, placed }: Request<typeof Timed.Type>) => files.map((file) => Job.make({ root: path.dirname(file), file, ...Record.getSomes({ outDir: Option.map(placed('vite'), (at) => path.relative(path.dirname(file), at)) }) })),
    }),
    projects: ({ report: { folders, signals, ports } }: Answer<typeof Report.Type>, { job: { root, file, outDir }, chain }: Asked<typeof Job>, { path, exec }) => {
        const needs = { dependsOn: ['codegen--*', '^codegen'], inputs: ['default', '^default', ...chain, { dependentTasksOutputFiles: '**/*', transitive: true }, NODE, { externalDependencies: ['vite'] }] };
        const placed = Option.fromUndefinedOr(outDir);
        const built = Option.fromUndefinedOr(folders);
        const served = Option.map(
            Option.fromUndefinedOr(ports),
            Record.map((port) => ({ ...needs, continuous: true, readyWhen: signals ? { signal: true as const } : { port } })),
        );
        return [
            {
                file,
                root,
                technologies: ['vite'],
                clean: 'vite',
                members: Record.getSomes<string, Members[string]>({
                    build: Option.map(built, (written) => ({
                        vite: {
                            command: line(['vite', 'build', ...Option.match(placed, { onNone: () => [], onSome: (at) => ['--outDir', at, '--emptyOutDir'] })]),
                            ...needs,
                            dependsOn: [...needs.dependsOn, '^build'],
                            cache: true,
                            outputs: written.map((at) => path.join('{workspaceRoot}', at)),
                            metadata: { description: 'Builds every environment of the Vite config with vite build', help: { command: `${exec} vite build --help`, example: { args: ['--mode', 'production'] } } },
                        },
                    })),
                    serve: Option.map(served, ({ serve }) => ({
                        vite: { ...serve, command: 'vite', metadata: { description: 'Serves every environment of the Vite config with the vite development server', help: { command: `${exec} vite --help`, example: { args: ['--port', '<port>'] } } } },
                    })),
                    preview: Option.map(Option.andThen(built, served), ({ preview }) => ({
                        vite: {
                            ...preview,
                            command: line(['vite', 'preview', ...Option.match(placed, { onNone: () => [], onSome: (at) => ['--outDir', at] })]),
                            dependsOn: ['build--vite'],
                            metadata: { description: 'Serves the build output of the Vite config with vite preview', help: { command: `${exec} vite preview --help`, example: { args: ['--port', '<port>'] } } },
                        },
                    })),
                }),
            },
        ];
    },
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
