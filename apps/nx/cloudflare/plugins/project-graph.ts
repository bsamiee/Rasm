import type { CreateNodes, CreateNodesContext, CreateNodesResultArray } from '@nx/devkit';
import { aggregated, identity, inferred, launcher, NODE } from '@rasm/nx-devkit/inference';
import { Effect, FileSystem, Path } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const CONFIG = 'cloudflare.config.ts';
const TECHNOLOGIES = ['cloudflare'];

// --- [OPERATIONS] ----------------------------------------------------------------------

const nodes = Effect.fnUntraced(function* (files: readonly string[], { workspaceRoot }: CreateNodesContext) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const roots = yield* Effect.filter(files.map(path.dirname), (root) => fs.exists(path.join(workspaceRoot, root, 'package.json')), { concurrency: 'unbounded' });
    const plugin = yield* identity(import.meta.url, workspaceRoot);
    const exec = launcher(workspaceRoot);
    return roots.map((root): CreateNodesResultArray[number] => [
        path.join(root, CONFIG),
        {
            projects: {
                [root]: {
                    targets: {
                        ...aggregated(
                            {
                                build: {
                                    cf: {
                                        executor: 'nx:run-commands',
                                        options: { command: 'cf workers types', cwd: root },
                                        cache: true,
                                        inputs: ['default', '^default', NODE, { externalDependencies: ['cf'] }],
                                        outputs: ['{projectRoot}/.cloudflare/types'],
                                        metadata: { technologies: TECHNOLOGIES, description: `Generates the Worker binding and runtime types from ${CONFIG}`, help: { command: `${exec} cf workers types --help`, example: { args: ['--mode', 'production'] } } },
                                    },
                                },
                            },
                            TECHNOLOGIES,
                            { executor: plugin.cleaner, tool: 'cf' },
                        ),
                        'check--tsc': { dependsOn: ['...', 'build--cf'] },
                    },
                },
            },
        },
    ]);
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes = [`**/${CONFIG}`, (files, _options, context) => inferred(nodes(files, context))];

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
