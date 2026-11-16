import { nodeBundle } from '@rasm/hosting/bundle';
import { defineConfig } from 'vite';
import hosting from '#/hosting.config';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig({ plugins: [nodeBundle(hosting, './main.ts')] });
