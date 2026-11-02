import { ENGINES } from '@rasm/harnesses/indesign/contract';
import npm from '#/package.json' with { type: 'json' };
import { script } from '../../../../vite.shared.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default script(import.meta.dirname, { main: npm.main, extension: ENGINES.uxpscript.extensions[0] });
