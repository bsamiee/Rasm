import npm from '#/package.json' with { type: 'json' };
import { script } from '../../../../../vite.shared.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default script(import.meta.dirname, { main: npm.main, extension: 'idjs' });
