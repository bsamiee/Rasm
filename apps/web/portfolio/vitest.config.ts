import { vitestProject } from '../../../vitest.config.ts';
import application from './vite.config.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default vitestProject(import.meta.dirname, { portable: (env) => application({ ...env, mode: 'test' }) });
