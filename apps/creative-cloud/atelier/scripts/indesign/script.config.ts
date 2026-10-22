import { script } from '../../../../../vite.config.ts';
import npm from './package.json' with { type: 'json' };

// --- [COMPOSITION] ---------------------------------------------------------------------

export default script(import.meta.dirname, { main: npm.main, extension: 'idjs' });
