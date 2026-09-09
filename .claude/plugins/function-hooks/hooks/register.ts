// --- [IMPORTS] -------------------------------------------------------------------------

import type { Register } from 'claude-code';
import { toolCall } from './events/tool-call.ts';

// --- [REGISTRATION] --------------------------------------------------------------------

const register: Register = (on) => toolCall(on);

// --- [EXPORTS] -------------------------------------------------------------------------

export { register };
