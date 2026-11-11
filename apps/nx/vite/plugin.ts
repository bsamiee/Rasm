import { inferred, type Request } from '@rasm/nx-devkit/plugin';
import { type Answer, type Asked, consulted, Timed } from '@rasm/nx-devkit/reader';
import { commanded, type Inputs, line, NODE, SIGNALLED } from '@rasm/nx-devkit/target';
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
    projects: ({ report: { folders, signals } }: Answer<typeof Report.Type>, { job: { root, file, outDir }, chain }: Asked<typeof Job>, { path, exec }) => {
        const inputs: Inputs = ['default', '^default', ...chain.map((at) => path.join('{workspaceRoot}', at)), NODE, { externalDependencies: ['vite'] }];
        const placed = Option.fromUndefinedOr(outDir);
        const built = Option.fromUndefinedOr(folders);
        const served = Option.liftPredicate({ ...SIGNALLED, inputs }, () => signals);
        return [
            {
                file,
                root,
                technologies: ['vite'],
                clean: 'vite',
                members: Record.getSomes({
                    build: Option.map(built, (written) => ({
                        vite: commanded(
                            [['vite', 'build', ...Option.match(placed, { onNone: () => [], onSome: (at) => ['--outDir', at, '--emptyOutDir'] })]],
                            root,
                            { description: 'Builds every environment of the Vite config with vite build', help: `${exec} ${line(['vite', 'build', '--help'])}`, example: { args: ['--mode', 'production'] } },
                            { cache: true, inputs, outputs: written.map((at) => path.join('{workspaceRoot}', at)) },
                        ),
                    })),
                    serve: Option.map(served, (server) => ({
                        vite: commanded([['vite']], root, { description: 'Serves every environment of the Vite config with the vite development server', help: `${exec} ${line(['vite', '--help'])}`, example: { args: ['--port', '<port>'] } }, server),
                    })),
                    preview: Option.map(Option.andThen(built, served), (server) => ({
                        vite: commanded(
                            [['vite', 'preview', ...Option.match(placed, { onNone: () => [], onSome: (at) => ['--outDir', at] })]],
                            root,
                            { description: 'Serves the build output of the Vite config with vite preview', help: `${exec} ${line(['vite', 'preview', '--help'])}`, example: { args: ['--port', '<port>'] } },
                            { ...server, dependsOn: ['build--vite'] },
                        ),
                    })),
                }),
            },
        ];
    },
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
