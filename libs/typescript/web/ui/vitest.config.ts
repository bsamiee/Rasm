import { react } from '../../../../vite.shared.ts';
import { project } from '../../../../vitest.config.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default project(import.meta.dirname, { test: { projects: [{ plugins: [react()], test: { name: 'component', browser: { enabled: true } } }] } });
