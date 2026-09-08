// tool.describe, the skill that owns a server's or a built-in tool is named before the tool's own description

// --- [IMPORTS] -------------------------------------------------------------------------

import type { On } from 'claude-code';
import { describeLine } from '../policies/tools.ts';

// --- [REGISTRATION] --------------------------------------------------------------------

// Reads the tables alone, the per-session description cache then holds with no invalidate
const toolDescribe = (on: On): void => {
    on('tool.describe', (_$, e, next) => {
        const line = describeLine(e.tool);
        return line === undefined ? next(e) : { description: `${line}\n${e.description}` };
    });
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { toolDescribe };
