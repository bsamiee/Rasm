// Hooks module that reads the options once and registers every event file

// --- [IMPORTS] -------------------------------------------------------------------------

import type { Register } from 'claude-code';
import { agentOffer } from './events/agent-offer.ts';
import { agentSpawn } from './events/agent-spawn.ts';
import { anyEvent } from './events/any-event.ts';
import { attributionText } from './events/attribution-text.ts';
import { engineCreate } from './events/engine-create.ts';
import { promptContext } from './events/prompt-context.ts';
import { promptSection } from './events/prompt-section.ts';
import { promptSubmit } from './events/prompt-submit.ts';
import { sessionStart } from './events/session-start.ts';
import { skillPrompt } from './events/skill-prompt.ts';
import { toolCall } from './events/tool-call.ts';
import { toolDescribe } from './events/tool-describe.ts';
import { turnComplete } from './events/turn-complete.ts';
import { turnStart } from './events/turn-start.ts';
import { turnStep } from './events/turn-step.ts';
import { uiInput } from './events/ui-input.ts';
import { uiPress } from './events/ui-press.ts';
import { uiRender } from './events/ui-render.ts';
import { uiResolve } from './events/ui-resolve.ts';
import { uiSelect } from './events/ui-select.ts';
import { options } from './host/options.ts';

// --- [REGISTRATION] --------------------------------------------------------------------

// Registration order is nesting order, the audit wraps every later hook and the rest follows the engine lifecycle
const register: Register = (on, raw) => {
    const settings = options(raw);
    anyEvent(on, settings);
    engineCreate(on, settings);
    sessionStart(on, settings);
    promptSubmit(on, settings);
    promptSection(on, settings);
    promptContext(on, settings);
    toolDescribe(on, settings);
    turnStart(on, settings);
    toolCall(on, settings);
    agentOffer(on, settings);
    agentSpawn(on, settings);
    skillPrompt(on, settings);
    attributionText(on, settings);
    turnStep(on, settings);
    turnComplete(on, settings);
    uiRender(on, settings);
    uiResolve(on, settings);
    uiPress(on, settings);
    uiInput(on, settings);
    uiSelect(on, settings);
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { register };
