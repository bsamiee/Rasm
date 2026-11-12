import { defineConfig, defineWorker } from 'cf/config';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig({ worker: defineWorker({ name: 'webhooks', compatibilityDate: '2026-10-01' }) });
