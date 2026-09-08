// prompt.context, the open-question block from the summary row joins the first message's context blocks

// --- [IMPORTS] -------------------------------------------------------------------------

import type { On, PromptContextResult } from 'claude-code';
import { flatMap } from '../composition/option.ts';
import type { Options } from '../host/options.ts';
import { decodeSummary, key } from '../host/store.ts';
import { questions } from '../policies/findings.ts';

// --- [REGISTRATION] --------------------------------------------------------------------

// The answer is cached until compaction, /clear, or an invalidate re-reads it, the block is a snapshot of the row session.start wrote
const promptContext = (on: On, _options: Options): void => {
    on('prompt.context', async ($, e, next) =>
        flatMap(questions)(decodeSummary(await $.store.get(key('summary')))).match<Promise<PromptContextResult>>({
            some: (block) => next({ ...e, blocks: [...e.blocks, block] }),
            none: () => next(e),
        }),
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { promptContext };
