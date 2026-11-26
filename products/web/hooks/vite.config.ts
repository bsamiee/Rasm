import { nodeBundle } from '@rasm/hosting/bundle';
import { checks } from '@rasm/vite/checks';
import { defineConfig } from 'vite';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig({ plugins: [checks(), nodeBundle('./server/main.ts')] });
