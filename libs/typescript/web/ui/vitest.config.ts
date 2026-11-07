import { react } from '../../../../vite.shared.ts';
import { vitestProject } from '../../../../vitest.config.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default vitestProject(import.meta.dirname, { portable: () => ({ test: { projects: [{ plugins: [react()], test: { name: 'component', browser: { enabled: true } } }] } }) });
