import { join } from 'node:path';
import { cloudflare } from '@cloudflare/vite-plugin';
import { sites } from '@openai/sites-vite-plugin';
import { pdfjs } from '@rasm/media/vite.ts';
import tailwindcss from '@tailwindcss/vite';
import react from '@vitejs/plugin-react';
import { defineConfig, normalizePath } from 'vite';
import { web } from '../../../vite.config.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig(
    web(import.meta.dirname, {
        plugins: [
            react({ compiler: { panicThreshold: 'all_errors' }, include: [import.meta.dirname, join(import.meta.dirname, '../../../libs/typescript/web')].map((directory) => `${normalizePath(directory)}/**/*.{ts,tsx}`) }),
            tailwindcss(),
            pdfjs(),
            sites(),
            cloudflare({ persistState: { path: '../../../.cache/cloudflare/apps/web/portfolio' }, types: { generate: false } }),
        ],
    }),
);
