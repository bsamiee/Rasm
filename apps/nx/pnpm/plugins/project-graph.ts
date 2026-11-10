import type { CreateNodes, CreateNodesContext, CreateNodesResultArray } from '@nx/devkit';
import { getLockFileName } from '@nx/devkit/internal';
import { aggregated, identity, inferred } from '@rasm/nx-devkit/inference';
import { Effect, FileSystem, Path } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const WORKSPACE = 'pnpm-workspace.yaml';
const TECHNOLOGIES = ['pnpm'];

// --- [OPERATIONS] ----------------------------------------------------------------------

const nodes = Effect.fnUntraced(function* (files: readonly string[], { workspaceRoot }: CreateNodesContext) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const roots = yield* Effect.filter(files.map(path.dirname), (root) => fs.exists(path.join(workspaceRoot, root, 'package.json')), { concurrency: 'unbounded' });
    const plugin = yield* identity(import.meta.url, workspaceRoot);
    return roots.map((root): CreateNodesResultArray[number] => [
        path.join(root, WORKSPACE),
        {
            projects: {
                [root]: {
                    targets: aggregated(
                        {
                            upgrade: {
                                pnpm: {
                                    executor: `${plugin.name}:upgrade`,
                                    outputs: [`{projectRoot}/${WORKSPACE}`, `{projectRoot}/${getLockFileName('pnpm')}`],
                                    metadata: { technologies: TECHNOLOGIES, description: 'Moves every catalog row to the highest version its dist-tags name, then refreshes the lockfile with pnpm update', help: { command: 'pnpm update --help', example: { args: ['--recursive'] } } },
                                },
                            },
                            clean: {
                                pnpm: {
                                    executor: plugin.cleaner,
                                    options: { commands: [['pnpm', 'store', 'prune']], cwd: root },
                                    metadata: { technologies: TECHNOLOGIES, description: 'Removes the packages no project on this machine references from the pnpm store', help: { command: 'pnpm store prune --help', example: {} } },
                                },
                            },
                        },
                        TECHNOLOGIES,
                    ),
                },
            },
        },
    ]);
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes = [`**/${WORKSPACE}`, (files, _options, context) => inferred(nodes(files, context))];

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
