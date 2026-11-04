import { sites } from '@openai/sites-vite-plugin';
import { nodeBundle } from '@rasm/hosting/vite';
import { pdfjs } from '@rasm/media/vite';
import tailwindcss from '@tailwindcss/vite';
import { cloudflareWorker, react } from '../../../vite.shared.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default cloudflareWorker(import.meta.dirname, { plugins: [react(), tailwindcss(), pdfjs()], hosting: [sites(), nodeBundle('./server/main.ts')] });
