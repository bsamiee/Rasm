import { bindings, defineConfig, defineWorker } from 'cf/config';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig({ worker: defineWorker({ name: 'hosting', compatibilityDate: '2026-10-01', compatibilityFlags: ['enable_request_signal'], env: { BUCKET: bindings.r2() } }) });
