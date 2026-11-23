import { chrome } from '@rasm/vite/chrome';
import { defineConfig } from 'vite';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig(chrome(import.meta.dirname));
