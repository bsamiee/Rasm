import { react } from '../../../../vite.config.ts';
import { createVitestConfig } from '../../../../vitest.config.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default createVitestConfig(import.meta.dirname, { test: { projects: [{ plugins: [react()], test: { name: 'component', browser: { enabled: true } } }] } });
