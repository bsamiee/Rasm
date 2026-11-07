import { sites } from '@openai/sites-vite-plugin';
import { hosting } from '@rasm/hosting/vite';
import { pdfjs } from '@rasm/media/vite';
import { react } from '@rasm/ui/vite';
import tailwindcss from '@tailwindcss/vite';
import { defineConfig } from 'vite';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig(({ mode }) => ({
    build: { target: 'esnext', rolldownOptions: { experimental: { lazyBarrel: true }, output: { strictExecutionOrder: true } } },
    plugins: [react(), tailwindcss(), pdfjs(), sites(), hosting({ head: { master: 'favicon.svg', stylesheet: 'site/theme.css', fill: '--color-accent', background: '--color-background' }, nodeEntry: './server/main.ts', mode })],
}));
