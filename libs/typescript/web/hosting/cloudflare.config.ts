import { bindings, defineConfig, defineWorker } from 'cf/config';
import { compatibilityDate } from '#/cloudflare';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig({ worker: defineWorker({ name: 'hosting', compatibilityDate, env: { BUCKET: bindings.r2() } }) });
