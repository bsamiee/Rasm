import { vitestProject } from '@rasm/test-support/vitest';
import application from '#/vite.config.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default vitestProject(import.meta.dirname, { portable: application });
