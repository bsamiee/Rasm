import { CloudflareAccess } from '@rasm/hosting';
import { compatibilityDate } from '@rasm/hosting/cloudflare';
import { bindings, defineConfig, defineWorker } from 'cf/config';
import { apiPrefix } from '#/model/api.ts';
import { simulatedEmail } from '#/model/sites.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig({
    worker: defineWorker(({ mode }) => ({
        name: 'portfolio',
        compatibilityDate,
        entrypoint: './server/worker.ts',
        assets: { runWorkerFirst: ['/', `${apiPrefix}/*`] },
        observability: { enabled: true, traces: { enabled: true } },
        env: {
            ASSETS: bindings.assets(),
            DB: bindings.d1(),
            BUCKET: bindings.r2(),
            OWNER_EMAIL: mode === 'production' || mode === 'cloudflare' ? bindings.secret() : bindings.text(simulatedEmail),
            ...(mode === 'cloudflare' && { [CloudflareAccess.binding]: bindings.secret() }),
        },
    })),
});
