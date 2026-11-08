import { hostedWorker } from '@rasm/hosting/cloudflare';
import { bindings, defineConfig } from 'cf/config';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig({ worker: hostedWorker({ name: 'portfolio', entrypoint: './server/worker.ts', env: { DB: bindings.d1(), BUCKET: bindings.r2() } }) });
