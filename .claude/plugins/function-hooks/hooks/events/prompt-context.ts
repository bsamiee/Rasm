// prompt.context, the open-question block from the summary row joins the first message's context blocks

// --- [IMPORTS] -------------------------------------------------------------------------

import type { On } from 'claude-code';
import { decode, isSummary, key } from '../host/store.ts';
import { questions } from '../policies/findings.ts';

// --- [REGISTRATION] --------------------------------------------------------------------

// The answer is cached until compaction, /clear, or an invalidate re-reads it, the block is a snapshot of the row session.start wrote
const promptContext = (on: On): void => {
    on('prompt.context', async ($, e, next) => {
        const summary = decode(isSummary)(await $.store.get(key('summary')));
        const block = summary === undefined ? undefined : questions(summary);
        return block === undefined ? next(e) : next({ ...e, blocks: [...e.blocks, block] });
    });
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { promptContext };
