import { sites } from '@openai/sites-vite-plugin';
import { vite } from '@rasm/hosting/node/vite';
import { pdfjs } from '@rasm/media/pdf/vite';
import tailwindcss from '@tailwindcss/vite';
import { react, worker } from '../../../vite.shared.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default worker(import.meta.dirname, { plugins: [react(), tailwindcss(), pdfjs()], hosting: [sites(), vite('./server/node.ts')] });
