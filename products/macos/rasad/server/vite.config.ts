import { join } from 'node:path';
import { layer } from '@effect/platform-node/NodeServices';
import { checks } from '@rasm/vite/checks';
import { singleExecutable } from '@rasm/vite/executable';
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
