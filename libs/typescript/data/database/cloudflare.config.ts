import { bindings, defineConfig, defineWorker } from 'cf/config';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig({ worker: defineWorker({ name: 'database', compatibilityDate: '2026-10-01', env: { DB: bindings.d1() } }) });
