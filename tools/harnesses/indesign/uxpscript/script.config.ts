import { ENGINES } from '@rasm/harnesses/indesign/contract';
import { uxpScript } from '@rasm/uxp/vite';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default uxpScript(import.meta.dirname, ENGINES.uxpscript.extensions[0]);
