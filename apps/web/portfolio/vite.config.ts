import { sites } from '@openai/sites-vite-plugin';
import { cloudflareWorker } from '@rasm/hosting/vite';
import { pdfjs } from '@rasm/media/vite';
import tailwindcss from '@tailwindcss/vite';
import { hostedLayout, react } from '../../../vite.shared.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default cloudflareWorker({
    plugins: [react(), tailwindcss(), pdfjs()],
    hosts: sites(),
    head: { master: 'favicon.svg', stylesheet: 'site/theme.css', ink: '--color-accent', surface: '--color-background' },
    nodeEntry: './server/main.ts',
    ...hostedLayout(import.meta.dirname),
});
