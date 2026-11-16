import { cloudflare } from '@cloudflare/vite-plugin';
import { nxReadySignal } from '@nx/vite/plugins/nx-ready-signal.plugin';
import { sites } from '@openai/sites-vite-plugin';
import { nodeBundle } from '@rasm/hosting/bundle';
import { siteHead } from '@rasm/hosting/head';
import { Enrollment } from '@rasm/hosting/identity';
import { pdfjs } from '@rasm/media/vite';
import tailwindcss from '@tailwindcss/vite';
import react from '@vitejs/plugin-react';
import { bindings } from 'cf/config';
import { defineConfig } from 'vite';
import hosting from '#/hosting.config';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig(({ command }) => ({
    build: { target: 'esnext', rolldownOptions: { experimental: { lazyBarrel: true }, output: { strictExecutionOrder: true } } },
    plugins: [
        react({ compiler: { panicThreshold: 'all_errors' } }),
        tailwindcss(),
        pdfjs(),
        sites(),
        nxReadySignal(),
        siteHead({ master: 'favicon.svg', stylesheet: 'site/theme.css', fill: '--color-accent', background: '--color-background' }),
        nodeBundle(hosting, './server/main.ts'),
        cloudflare({
            types: { generate: false },
            ...(command === 'serve' && { config: (loaded) => ({ ...loaded, worker: { ...loaded.worker, env: { ...loaded.worker.env, [Enrollment.variable]: bindings.text(JSON.stringify(['seedy@sites.test'])) } } }) }),
        }),
    ],
}));
