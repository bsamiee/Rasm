import { ENGINES } from '@rasm/harnesses/indesign/contract';
import { uxpScript } from '../../../../vite.shared.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default uxpScript(import.meta.dirname, ENGINES.uxpscript.extensions[0]);
