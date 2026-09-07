// prompt.context, the open-question block from the findings rows joins the first message's context blocks

// --- [IMPORTS] -------------------------------------------------------------------------

import type { On, PromptContextResult } from 'claude-code';
import type { Options } from '../host/options.ts';
import { decodeFindings, keys } from '../host/store.ts';
import { questions } from '../policies/findings.ts';

// --- [REGISTRATION] --------------------------------------------------------------------

// The answer is cached until compaction, /clear, or an invalidate re-reads it, the block is a snapshot of the rows at that moment
const promptContext = (on: On, _options: Options): void => {
    on('prompt.context', async ($, e, next) => {
        const all = await $.store.keys();
        const values = await Promise.all(keys('findings')(all).map((name) => $.store.get(name)));
        return questions(decodeFindings(values)).match<Promise<PromptContextResult>>({
            some: (block) => next({ ...e, blocks: [...e.blocks, block] }),
            none: () => next(e),
        });
    });
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { promptContext };
