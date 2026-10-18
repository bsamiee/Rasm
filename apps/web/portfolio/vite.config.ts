import { sites } from '@openai/sites-vite-plugin';
import { node } from '@rasm/hosting/vite.ts';
import { pdfjs } from '@rasm/media/vite.ts';
import tailwindcss from '@tailwindcss/vite';
import { defineConfig } from 'vite';
import { react, worker } from '../../../vite.config.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig(worker(import.meta.dirname, [react(), tailwindcss(), pdfjs()], [sites(), node('./worker/node.ts')]));
