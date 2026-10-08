// biome-ignore lint/correctness/noNodejsModules: Vite evaluates its configuration in Node.js.
import { fileURLToPath } from 'node:url';
import { cloudflare } from '@cloudflare/vite-plugin';
import { sites } from '@openai/sites-vite-plugin';
import tailwindcss from '@tailwindcss/vite';
import react from '@vitejs/plugin-react';
import { defineConfig, normalizePath } from 'vite';
import { viteStaticCopy } from 'vite-plugin-static-copy';
import hosting from './.openai/hosting.json' with { type: 'json' };

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig(({ command }) => ({
    cacheDir: '../../.cache/vite/portfolio',
    plugins: [
        react({ compiler: true }),
        tailwindcss(),
        viteStaticCopy({ targets: ['cmaps', 'standard_fonts', 'wasm'].map((directory) => ({ src: normalizePath(fileURLToPath(new URL(`${directory}/*`, import.meta.resolve('pdfjs-dist/package.json')))), dest: `pdfjs/${directory}`, rename: { stripBase: true } })) }),
        sites(),
        cloudflare({
            viteEnvironment: { name: 'server' },
            persistState: { path: '../../.cache/wrangler/apps/portfolio' },
            config: {
                main: './worker/index.ts',
                vars: command === 'serve' ? { OWNER_EMAIL: 'seedy@sites.test' } : {},
                assets: { binding: 'ASSETS', run_worker_first: ['/', '/api/*'] },
                d1_databases: [{ binding: hosting.d1 }],
                r2_buckets: [{ binding: hosting.r2 }],
            },
        }),
    ],
}));
