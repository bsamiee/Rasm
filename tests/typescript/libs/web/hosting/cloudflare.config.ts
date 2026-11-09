import { bindings, defineConfig, defineWorker } from 'cf/config';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig({ worker: defineWorker({ name: 'hosting', compatibilityDate: '2026-10-01', env: { BUCKET: bindings.r2() } }) });
