import { worker } from '@rasm/hosting/cloudflare';
import { bindings, defineConfig } from 'cf/config';
import hosting from '#/hosting.config';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig({
    worker: worker(hosting, { compatibilityDate: '2026-10-01', entrypoint: './server/worker.ts', observability: { enabled: true, traces: { enabled: true } }, assets: { runWorkerFirst: ['/'] }, env: { ASSETS: bindings.assets() } }),
});
