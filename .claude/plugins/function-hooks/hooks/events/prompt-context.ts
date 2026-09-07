// prompt.context, the open-question block from the summary row joins the first message's context blocks

// --- [IMPORTS] -------------------------------------------------------------------------

import type { On, PromptContextResult } from 'claude-code';
import type { Options } from '../host/options.ts';
import { key, summaryOf } from '../host/store.ts';
import { questions } from '../policies/findings.ts';

// --- [REGISTRATION] --------------------------------------------------------------------

// The answer is cached until compaction, /clear, or an invalidate re-reads it, the block is a snapshot of the row at that moment
const promptContext = (on: On, _options: Options): void => {
    on('prompt.context', async ($, e, next) =>
        questions(summaryOf(await $.store.get(key('summary')))).match<Promise<PromptContextResult>>({
            some: (block) => next({ ...e, blocks: [...e.blocks, block] }),
            none: () => next(e),
        }),
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { promptContext };
