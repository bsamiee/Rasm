import { bindings, defineConfig, defineWorker } from 'cf/config';
import manifest from './package.json' with { type: 'json' };

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig({
    worker: defineWorker(({ mode }) => ({
        name: manifest.name,
        compatibilityDate: '2026-10-01',
        entrypoint: './worker/index.ts',
        assets: { runWorkerFirst: ['/', '/api/*'] },
        observability: { enabled: true, traces: { enabled: true } },
        env: {
            ASSETS: bindings.assets(),
            DB: bindings.d1(),
            BUCKET: bindings.r2(),
            OWNER_EMAIL: mode === 'production' ? bindings.secret() : bindings.text('seedy@sites.test'),
        },
    })),
});
