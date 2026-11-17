import { cloudflare } from '@cloudflare/vite-plugin';
import { nxReadySignal } from '@nx/vite/plugins/nx-ready-signal.plugin';
import { nodeBundle } from '@rasm/hosting/bundle';
import { worker } from '@rasm/hosting/cloudflare';
import { siteHead } from '@rasm/hosting/head';
import { pdfjs } from '@rasm/media/vite';
import tailwindcss from '@tailwindcss/vite';
import react from '@vitejs/plugin-react';
import { defineConfig } from 'vite';
import hosting from '#/hosting.config';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig({
    build: { target: 'esnext', rolldownOptions: { experimental: { lazyBarrel: true }, output: { strictExecutionOrder: true } } },
    server: { port: 5173 },
    preview: { port: 4173 },
    plugins: [
        react({ compiler: { panicThreshold: 'all_errors' } }),
        tailwindcss(),
        pdfjs(),
        nxReadySignal(),
        siteHead({ master: 'favicon.svg', stylesheet: 'site/theme.css', fill: '--color-accent', background: '--color-background' }),
        nodeBundle(hosting, './server/main.ts'),
        worker.plugin(hosting),
        cloudflare({ types: { generate: false } }),
    ],
});
