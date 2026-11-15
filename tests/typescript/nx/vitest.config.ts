import { rows } from '@rasm/testing/nx';
import { reporting } from '@rasm/testing/vitest';
import { Array, Effect } from 'effect';
import { defineConfig } from 'vitest/config';
import nx from '../../../nx.json' with { type: 'json' };

// --- [CONSTANTS] -----------------------------------------------------------------------

const READ = ['pnpm-lock.yaml', 'pnpm-workspace.yaml', '**/Cargo.lock', '**/Package.resolved', '.config/mise.toml', '.config/mise.*.toml'] as const;

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig(() =>
    Effect.runPromise(
        Effect.map(
            Effect.forEach(rows(nx), ({ plugin }) => Effect.promise((): Promise<{ readonly createNodes?: readonly [string, unknown] }> => import(plugin))),
            (plugins) => ({
                test: {
                    ...reporting,
                    name: '@rasm/test-nx',
                    globalSetup: ['setup.ts'],
                    fileParallelism: false,
                    testTimeout: 120_000,
                    forceRerunTriggers: [...READ, ...plugins.flatMap(({ createNodes }) => Array.fromNullishOr(createNodes?.[0]))],
                },
            }),
        ),
    ),
);
