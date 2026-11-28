import { cloudflare } from '@cloudflare/vite-plugin';
import { nxReadySignal } from '@nx/vite/plugins/nx-ready-signal.plugin';
import { nodeBundle, worker } from '@rasm/hosting/bundle';
import { siteHead } from '@rasm/hosting/head';
import { pdfjs } from '@rasm/media/vite';
import { checks } from '@rasm/vite/checks';
import react from '@vitejs/plugin-react';
import { defineConfig } from 'vite';
import hosting from '#/hosting.config';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig({
    build: { target: 'esnext', minify: true, rolldownOptions: { output: { strictExecutionOrder: true } } },
    plugins: [
        checks(),
        react({ compiler: { panicThreshold: 'all_errors' } }),
        pdfjs(),
        nxReadySignal(),
        siteHead({ stylesheet: 'site/theme.css', fill: '--color-accent', background: '--color-background' }),
        nodeBundle('./server/main.ts'),
        cloudflare({ config: worker(hosting, { compatibilityDate: '2026-10-01', entrypoint: './server/main.ts', observability: { enabled: true, traces: { enabled: true } }, assets: { runWorkerFirst: ['/'] } }) }),
    ],
});
