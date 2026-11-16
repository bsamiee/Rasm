import { createRequire } from 'node:module';
import { join } from 'node:path';
import { layer } from '@effect/platform-node/NodeServices';
import { helper } from '@rasm/applications/vite';
import { bundle } from '@rasm/automation/vite';
import { script } from '@rasm/cep/vite';
import { Effect } from 'effect';
import type { UserConfigFnPromise } from 'vite';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default Effect.fnUntraced(
    function* () {
        const directory = import.meta.dirname;
        return bundle({
            build: { rolldownOptions: { experimental: { lazyBarrel: true } } },
            plugins: [script(createRequire(join(directory, 'package.json')).resolve('@rasm/atelier-extendscript'), 'runtime'), yield* helper(join(directory, 'main.ts'), 'server', { KeepAlive: true }, { project: join(directory, 'macos', 'Atelier.xcodeproj'), target: 'Atelier' })],
        });
    },
    Effect.provide(layer),
    Effect.runPromise,
) satisfies UserConfigFnPromise;
