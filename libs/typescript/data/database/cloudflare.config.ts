import { bindings, defineConfig, defineWorker, exports } from 'cf/config';
import * as entrypoint from './durable.ts' with { type: 'cf-worker' };

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig({
    worker: defineWorker({
        name: 'database',
        compatibilityDate: '2026-10-01',
        compatibilityFlags: ['nodejs_compat'],
        entrypoint,
        exports: { Store: exports.durableObject({ storage: 'sqlite' }) },
        env: { DB: bindings.d1() },
    }),
});
