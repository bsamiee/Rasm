import { cloudflare } from '@cloudflare/vite-plugin';
import { sites } from '@openai/sites-vite-plugin';
import tailwindcss from '@tailwindcss/vite';
import react from '@vitejs/plugin-react';
import { defineConfig } from 'vite';
import hosting from './.openai/hosting.json' with { type: 'json' };

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig(({ command }) => ({
    plugins: [
        react({ compiler: true }),
        tailwindcss(),
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
