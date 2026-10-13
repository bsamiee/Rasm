import { fileURLToPath } from 'node:url';
import { cloudflare } from '@cloudflare/vite-plugin';
import { sites } from '@openai/sites-vite-plugin';
import tailwindcss from '@tailwindcss/vite';
import react from '@vitejs/plugin-react';
import { defineConfig, normalizePath } from 'vite';
import { viteStaticCopy } from 'vite-plugin-static-copy';
import { project } from '../../../vite.config.ts';
import hosting from './.openai/hosting.json' with { type: 'json' };

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig(({ command }) =>
    project(import.meta.dirname, {
        plugins: [
            react({ compiler: { panicThreshold: 'all_errors' }, include: `${normalizePath(import.meta.dirname)}/**/*.{ts,tsx}` }),
            tailwindcss(),
            viteStaticCopy({ targets: ['cmaps', 'standard_fonts', 'wasm'].map((directory) => ({ src: normalizePath(fileURLToPath(new URL(`${directory}/*`, import.meta.resolve('pdfjs-dist/package.json')))), dest: `pdfjs/${directory}`, rename: { stripBase: true } })) }),
            sites(),
            cloudflare({
                viteEnvironment: { name: 'server' },
                persistState: { path: '../../../.cache/wrangler/apps/web/portfolio' },
                config: {
                    main: './worker/index.ts',
                    vars: command === 'serve' ? { OWNER_EMAIL: 'seedy@sites.test' } : {},
                    assets: { binding: 'ASSETS', run_worker_first: ['/', '/api/*'] },
                    d1_databases: [{ binding: hosting.d1 }],
                    r2_buckets: [{ binding: hosting.r2 }],
                },
            }),
        ],
    }),
);
