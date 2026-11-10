import { nodeServer } from '@rasm/hosting/vite';
import { defineConfig } from 'vite';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig({ plugins: [nodeServer({ entry: './main.ts' })] });
