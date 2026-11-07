import { layer } from '@effect/platform-node/NodeServices';
import { Workspace } from '@rasm/eng';
import { defaults } from '@rasm/test-support/vitest';
import { Array, Effect, flow } from 'effect';
import type { ConfigEnv } from 'vite';
import type { ViteUserConfigFnPromise } from 'vitest/config';

// --- [CONSTANTS] -----------------------------------------------------------------------

const ROOT = import.meta.dirname;

// --- [COMPOSITION] ---------------------------------------------------------------------

const rootConfig: ViteUserConfigFnPromise = flow(
    Effect.fnUntraced(function* ({ mode }: ConfigEnv) {
        const [test, { packages }] = yield* Effect.all([defaults(ROOT, mode), Workspace.read(ROOT)], { concurrency: 'unbounded' });
        return { test: { ...test, coverage: { ...test.coverage, include: Array.cartesianWith(packages, test.coverage.include, (glob, pattern) => `${glob}/${pattern}`) }, projects: packages.map((glob) => `${glob}/vitest.config.ts`) } };
    }, Effect.provide(layer)),
    Effect.runPromise,
);

// --- [EXPORTS] -------------------------------------------------------------------------

export default rootConfig;
