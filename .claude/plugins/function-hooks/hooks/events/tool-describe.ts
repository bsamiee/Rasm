// tool.describe, the skill that owns a server's or a built-in tool is named before the tool's own description

// --- [IMPORTS] -------------------------------------------------------------------------

import type { On, ToolDescribeResult } from 'claude-code';
import { absurd } from '../composition/decision.ts';
import type { Options } from '../host/options.ts';
import { describeRule } from '../policies/tools.ts';

// --- [REGISTRATION] --------------------------------------------------------------------

const toolDescribe = (on: On, _options: Options): void => {
    on('tool.describe', (_$, e, next) =>
        describeRule(e).match<ToolDescribeResult | Promise<ToolDescribeResult>>({
            answer: (result) => result,
            rewrite: (input, _context) => next(input),
            deny: absurd,
        }),
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { toolDescribe };
