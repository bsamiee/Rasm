import { cloudflare } from '@cloudflare/vite-plugin';
import { nxReadySignal } from '@nx/vite/plugins/nx-ready-signal.plugin';
import { sites } from '@openai/sites-vite-plugin';
import { siteHead } from '@rasm/hosting/head';
import { Sites } from '@rasm/hosting/openai-sites';
import { nodeServer } from '@rasm/hosting/vite';
import { pdfjs } from '@rasm/media/vite';
import tailwindcss from '@tailwindcss/vite';
import react from '@vitejs/plugin-react';
import { bindings } from 'cf/config';
import { defineConfig } from 'vite';

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
        nodeServer({ entry: './server/main.ts' }),
        cloudflare({
            types: { generate: false },
            ...(command === 'serve' && { config: (loaded) => ({ ...loaded, worker: { ...loaded.worker, env: { ...loaded.worker.env, OWNER_EMAIL: bindings.text(Sites.local.email) } } }) }),
        }),
    ],
}));
