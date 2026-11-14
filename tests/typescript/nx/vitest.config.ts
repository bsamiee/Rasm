import { workspaceRoot } from '@nx/devkit';
import { Array, Effect, Predicate } from 'effect';
import { readNxJson } from 'nx/src/config/nx-json';
import { configDefaults, defineConfig } from 'vitest/config';

// --- [CONSTANTS] -----------------------------------------------------------------------

const READ = ['README.md', '.github/workflows/ci.yml', '.config/mise.toml', '.config/mise.*.toml', 'pnpm-lock.yaml', 'pnpm-workspace.yaml', '**/Cargo.lock', '**/Package.resolved'] as const;

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig(() =>
    Effect.runPromise(
        Effect.map(
            Effect.forEach(readNxJson(workspaceRoot).plugins ?? [], (row) => Effect.promise((): Promise<{ readonly createNodes?: readonly [string, unknown] }> => import(Predicate.isString(row) ? row : row.plugin))),
            (plugins) => ({
                test: {
                    name: '@rasm/test-nx',
                    chaiConfig: { includeStack: true, truncateThreshold: 0 },
                    reporters: [...configDefaults.reporters, 'junit'],
                    globalSetup: ['setup.ts'],
                    fileParallelism: false,
                    testTimeout: 120_000,
                    forceRerunTriggers: [...READ, ...plugins.flatMap(({ createNodes }) => Array.fromNullishOr(createNodes?.[0]))],
                },
            }),
        ),
    ),
);
