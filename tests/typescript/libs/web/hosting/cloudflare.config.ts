import { compatibilityDate } from '@rasm/hosting/cloudflare';
import { bindings, defineConfig, defineWorker } from 'cf/config';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig({ worker: defineWorker({ name: 'hosting', compatibilityDate, env: { BUCKET: bindings.r2() } }) });
