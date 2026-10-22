import { bindings, defineConfig, defineWorker } from 'cf/config';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig({
    worker: defineWorker(({ mode }) => ({
        name: 'portfolio',
        compatibilityDate: '2026-10-01',
        entrypoint: './server/worker.ts',
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
