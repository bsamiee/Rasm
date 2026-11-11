import { bindings, defineConfig, defineWorker, triggers } from 'cf/config';

// --- [CONSTANTS] -----------------------------------------------------------------------

const queue = 'queue';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig({
    worker: defineWorker({
        name: 'workers',
        entrypoint: './server/worker.ts',
        compatibilityDate: '2026-10-01',
        compatibilityFlags: ['service_binding_extra_handlers'],
        env: { QUEUE: bindings.text(queue), RESULTS: bindings.kv(), PRODUCER: bindings.queue({ name: queue }) },
        triggers: [triggers.queue({ name: queue, maxBatchTimeout: 0 })],
    }),
});
