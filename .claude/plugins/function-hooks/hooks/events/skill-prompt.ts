// skill.prompt stamps every loaded skill under loaded/<session>/<skill>, the tool.call once lines read the stamps

// --- [IMPORTS] -------------------------------------------------------------------------

import type { On } from 'claude-code';
import { key } from '../host/store.ts';

// --- [REGISTRATION] --------------------------------------------------------------------

const skillPrompt = (on: On): void => {
    on('skill.prompt', async ($, e, next) => {
        await $.store.set(key('loaded', await $.session.id(), e.skill), $.clock.now());
        return next(e);
    });
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { skillPrompt };
