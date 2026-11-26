import { claude } from '@rasm/vite/claude';
import { defineConfig } from 'vite';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig(claude(import.meta.dirname));
