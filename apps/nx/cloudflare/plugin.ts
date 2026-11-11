import { inferred, listed, NODE, type Request } from '@rasm/nx-devkit/plugin';
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
        const packages = yield* listed(context.workspaceRoot, files.map(path.dirname));
        return files.filter((file) => packages.has(path.dirname(file))).map((file) => ({ key: file, file }));
    }),
    read: (missing) => Effect.succeed(missing.map(() => Result.succeed({ report: {} }))),
    projects: (_, { file }, { path, exec }) => [
        {
            file,
            root: path.dirname(file),
            technologies: ['cloudflare'],
            clean: 'cf',
            members: {
                build: {
                    cf: {
                        command: 'cf workers types',
                        cache: true,
                        inputs: ['default', '^default', NODE, { externalDependencies: ['cf'] }],
                        outputs: ['{projectRoot}/.cloudflare/types'],
                        metadata: { description: `Generates the Worker binding and runtime types from ${CONFIG}`, help: { command: `${exec} cf workers types --help`, example: { args: ['--mode', 'production'] } } },
                    },
                },
            },
        },
    ],
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
