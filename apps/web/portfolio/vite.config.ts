import { cloudflareWorker } from '@rasm/hosting/vite';
import { react } from '@rasm/ui/vite';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default cloudflareWorker({
    directory: import.meta.dirname,
    plugins: () => [react(), import('@tailwindcss/vite').then(({ default: tailwindcss }) => tailwindcss()), import('@rasm/media/vite').then(({ pdfjs }) => pdfjs())],
    hosts: () => import('@openai/sites-vite-plugin').then(({ sites }) => sites()),
    head: { master: 'favicon.svg', stylesheet: 'site/theme.css', fill: '--color-accent', background: '--color-background' },
    nodeEntry: './server/main.ts',
});
