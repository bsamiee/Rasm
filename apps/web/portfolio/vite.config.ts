import { sites } from '@openai/sites-vite-plugin';
import { node } from '@rasm/hosting/node';
import { pdfjs } from '@rasm/media/vite';
import tailwindcss from '@tailwindcss/vite';
import { defineConfig } from 'vite';
import { react, worker } from '../../../vite.shared.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig(worker(import.meta.dirname, [react(), tailwindcss(), pdfjs()], [sites(), node('./server/node.ts')]));
