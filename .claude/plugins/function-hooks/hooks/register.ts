// Hooks module that reads the options once and registers every event file

// --- [IMPORTS] -------------------------------------------------------------------------

import type { Register } from 'claude-code';
import { agentSpawn } from './events/agent-spawn.ts';
import { promptContext } from './events/prompt-context.ts';
import { promptSubmit } from './events/prompt-submit.ts';
import { sessionStart } from './events/session-start.ts';
import { skillPrompt } from './events/skill-prompt.ts';
import { toolCall } from './events/tool-call.ts';
import { toolDescribe } from './events/tool-describe.ts';
import { turnComplete } from './events/turn-complete.ts';
import { uiRender } from './events/ui-render.ts';
import { options } from './host/options.ts';

// --- [REGISTRATION] --------------------------------------------------------------------

// Registration order is nesting order, and the files follow the engine lifecycle
const register: Register = (on, raw) => {
    const settings = options(raw);
    sessionStart(on, settings);
    promptSubmit(on);
    promptContext(on);
    toolDescribe(on);
    toolCall(on, settings);
    agentSpawn(on);
    skillPrompt(on);
    turnComplete(on, settings);
    uiRender(on, settings);
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { register };
