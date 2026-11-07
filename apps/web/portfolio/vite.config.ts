import { cloudflareWorker } from '@rasm/hosting/vite';
import { hostedLayout, react } from '../../../vite.shared.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default cloudflareWorker({
    plugins: () => [react(), import('@tailwindcss/vite').then(({ default: tailwindcss }) => tailwindcss()), import('@rasm/media/vite').then(({ pdfjs }) => pdfjs())],
    hosts: () => import('@openai/sites-vite-plugin').then(({ sites }) => sites()),
    head: { master: 'favicon.svg', stylesheet: 'site/theme.css', ink: '--color-accent', surface: '--color-background' },
    nodeEntry: './server/main.ts',
    ...hostedLayout(import.meta.dirname),
});
