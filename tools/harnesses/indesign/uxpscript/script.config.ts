import { ENGINES } from '@rasm/harnesses/indesign/contract';
import { script } from '../../../../vite.shared.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default script(import.meta.dirname, ENGINES.uxpscript.extensions[0]);
