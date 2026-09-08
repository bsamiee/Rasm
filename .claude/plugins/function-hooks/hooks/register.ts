// Hooks module that reads the options once and registers every event file

// --- [IMPORTS] -------------------------------------------------------------------------

import type { Register } from 'claude-code';
import { promptSubmit } from './events/prompt-submit.ts';
import { sessionStart } from './events/session-start.ts';
import { skillPrompt } from './events/skill-prompt.ts';
import { toolCall } from './events/tool-call.ts';
import { toolDescribe } from './events/tool-describe.ts';
import { turnComplete } from './events/turn-complete.ts';
import { options } from './host/options.ts';

// --- [REGISTRATION] --------------------------------------------------------------------

// Registration order is nesting order, and the files follow the engine lifecycle
const register: Register = (on, raw) => {
    const settings = options(raw);
    sessionStart(on);
    promptSubmit(on);
    toolDescribe(on);
    toolCall(on, settings);
    skillPrompt(on);
    turnComplete(on, settings);
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { register };
