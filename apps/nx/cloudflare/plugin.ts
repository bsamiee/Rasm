import { inferred, manifested, type Request } from '@rasm/nx-devkit/plugin';
import { commanded, NODE } from '@rasm/nx-devkit/target';
import { Effect, Path, Result, Schema } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const CONFIG = 'cloudflare.config.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const { createNodes } = inferred({
    module: import.meta.url,
    files: [CONFIG],
    options: Schema.Struct({}),
    units: Effect.fnUntraced(function* ({ files, context }: Request<unknown>) {
        const path = yield* Path.Path;
        const packages = yield* manifested(context.workspaceRoot, files.map(path.dirname));
        return files.filter((file) => packages.has(path.dirname(file))).map((file) => ({ key: file, file }));
    }),
    read: (missing) => Effect.succeed(missing.map(() => Result.succeed({ report: {} }))),
    sources: () => ({ tracked: [] }),
    projects: (_, { file }, { path, exec }) => [
        {
            file,
            root: path.dirname(file),
            technologies: ['cloudflare'],
            clean: 'cf',
            members: {
                build: {
                    cf: commanded(
                        [['cf', 'workers', 'types']],
                        path.dirname(file),
                        { description: `Generates the Worker binding and runtime types from ${CONFIG}`, help: `${exec} cf workers types --help`, example: { args: ['--mode', 'production'] } },
                        { cache: true, inputs: ['default', '^default', NODE, { externalDependencies: ['cf'] }], outputs: ['{projectRoot}/.cloudflare/types'] },
                    ),
                },
            },
        },
    ],
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
