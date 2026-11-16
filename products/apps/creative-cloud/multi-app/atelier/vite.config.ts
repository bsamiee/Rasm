import { createRequire } from 'node:module';
import { join } from 'node:path';
import { layer } from '@effect/platform-node/NodeServices';
import { helper } from '@rasm/applications/vite';
import { script } from '@rasm/extendscript/vite';
import { Effect } from 'effect';
import type { UserConfigFnPromise } from 'vite';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default Effect.fnUntraced(
    function* () {
        const directory = import.meta.dirname;
        return {
            build: { rolldownOptions: { experimental: { lazyBarrel: true }, output: { codeSplitting: false } } },
            plugins: [script(createRequire(join(directory, 'package.json')).resolve('@rasm/extendscript/host'), 'runtime'), yield* helper(join(directory, 'main.ts'), 'server', { KeepAlive: true }, { project: join(directory, 'macos', 'Atelier.xcodeproj'), target: 'Atelier' })],
        };
    },
    Effect.provide(layer),
    Effect.runPromise,
) satisfies UserConfigFnPromise;
