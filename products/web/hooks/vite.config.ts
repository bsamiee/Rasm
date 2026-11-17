import { nodeBundle } from '@rasm/hosting/bundle';
import { defineConfig } from 'vite';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig({ plugins: [nodeBundle('./server/main.ts')] });
