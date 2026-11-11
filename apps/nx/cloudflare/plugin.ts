import { findPackageJSON } from 'node:module';
import { pathToFileURL } from 'node:url';
import { inferred, listed, type Request, type Unit } from '@rasm/nx-devkit/plugin';
import { type Array, Effect, Option, Path, Result, Schema } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

interface Worker extends Unit {
    readonly packaged: boolean;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const CONFIG = 'cloudflare.config.ts';
const RUNTIME = '@cloudflare/workers-types';

// --- [COMPOSITION] ---------------------------------------------------------------------

const { createNodes } = inferred({
    module: import.meta.url,
    files: [CONFIG],
    options: Schema.Struct({}),
    units: Effect.fnUntraced(function* ({ files, context: { workspaceRoot } }: Request<unknown>) {
        const path = yield* Path.Path;
        const packages = yield* listed(workspaceRoot, files.map(path.dirname));
        const resolved = Option.liftThrowable(findPackageJSON);
        return files.filter((file) => packages.has(path.dirname(file))).map((file): Worker => ({ key: file, file, packaged: Option.isSome(resolved(RUNTIME, pathToFileURL(path.join(workspaceRoot, path.dirname(file), 'package.json')))) }));
    }),
    read: (missing: Array.NonEmptyReadonlyArray<Worker>) => Effect.succeed(missing.map(() => Result.succeed({ report: {} }))),
    projects: (_, { file, packaged }, { path, exec }) => [
        {
            file,
            root: path.dirname(file),
            technologies: ['cloudflare'],
            clean: 'cf',
            members: {
                codegen: {
                    cf: {
                        command: packaged ? 'cf workers types --include-runtime=false' : 'cf workers types',
                        dependsOn: ['^codegen'],
                        cache: true,
                        inputs: ['default', '^default', { externalDependencies: ['cf'] }],
                        outputs: ['{projectRoot}/.cloudflare/types'],
                        metadata: {
                            description: packaged ? `Generates the Worker binding types from ${CONFIG}, runtime types coming from ${RUNTIME}` : `Generates the Worker binding and runtime types from ${CONFIG}`,
                            help: { command: `${exec} cf workers types --help`, example: { args: ['--mode', 'production'] } },
                        },
                    },
                },
            },
        },
    ],
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
