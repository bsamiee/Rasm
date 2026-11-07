import { vitestProject } from '@rasm/test-support/vitest';
import { react } from '#/vite';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default vitestProject(import.meta.dirname, { portable: () => ({ test: { projects: [{ plugins: [react()], test: { name: 'component', browser: { enabled: true } } }] } }) });
