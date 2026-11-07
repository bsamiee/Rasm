import { bindings, defineConfig, defineWorker } from 'cf/config';
import { compatibilityDate } from '#/cloudflare.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig({ worker: defineWorker({ name: 'hosting', compatibilityDate, env: { BUCKET: bindings.r2() } }) });
