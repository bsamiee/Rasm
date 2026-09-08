// agent.spawn, the brief of the spawned type joins the prompt

// --- [IMPORTS] -------------------------------------------------------------------------

import type { On } from 'claude-code';
import { AGENTS, briefed } from '../policies/agents.ts';

// --- [REGISTRATION] --------------------------------------------------------------------

const agentSpawn = (on: On): void => {
    on('agent.spawn', ($, e, next) => {
        if (AGENTS[e.subagentType] === undefined) {
            return next(e);
        }
        $.ui.notice(e.tool_use_id, `Appended the ${e.subagentType} brief`);
        return next({ ...e, prompt: briefed(e.subagentType, e.prompt) });
    });
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { agentSpawn };
