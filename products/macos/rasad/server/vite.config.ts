import { join } from 'node:path';
import { layer } from '@effect/platform-node/NodeServices';
import { singleExecutable } from '@rasm/applications/vite';
import { checks } from '@rasm/vite';
import { Effect } from 'effect';
import type { UserConfigFnPromise } from 'vite';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default Effect.fnUntraced(
    function* () {
        return { plugins: [checks(), yield* singleExecutable(join(import.meta.dirname, 'main.ts'), 'server')] };
    },
    Effect.provide(layer),
    Effect.runPromise,
) satisfies UserConfigFnPromise;
