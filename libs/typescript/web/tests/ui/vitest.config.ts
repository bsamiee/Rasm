import { testPolicy } from '@rasm/testing/vitest';
import react from '@vitejs/plugin-react';
import { defineConfig } from 'vitest/config';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig({
    plugins: [testPolicy(), react({ compiler: { panicThreshold: 'all_errors' } })],
    test: { api: { allowWrite: true }, browser: { enabled: true }, include: ['*.test.{ts,tsx}'] },
});
