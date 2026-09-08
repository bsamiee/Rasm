// prompt.submit, each secret value in the prompt becomes its id and the redacted text is recorded for the operator-named-target check

// --- [IMPORTS] -------------------------------------------------------------------------

import type { On, PromptSubmitInput, PromptSubmitResult } from 'claude-code';
import { absurd } from '../composition/decision.ts';
import { fromPredicate } from '../composition/option.ts';
import type { Options } from '../host/options.ts';
import { key, secretsOf } from '../host/store.ts';
import { pairs, redact } from '../policies/secrets.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Proceeded = Extract<PromptSubmitResult, { readonly drop?: undefined }>;

// --- [REGISTRATION] --------------------------------------------------------------------

const promptSubmit = (on: On, _options: Options): void => {
    on('prompt.submit', async ($, e, next) => {
        const [session, stored] = await Promise.all([$.session.id(), $.store.get(key('secrets'))]);
        const secretPairs = pairs(secretsOf(stored));
        return redact<PromptSubmitInput>(secretPairs)(e).match<Promise<PromptSubmitResult>>({
            rewrite: async (input, context) => {
                await $.store.set(key('prompt', session), { text: input.text });
                const result = await next(input);
                // Proceeded results take the lines, and an empty context leaves the result as next resolved it
                return fromPredicate((candidate: PromptSubmitResult): candidate is Proceeded => candidate.drop === undefined && context.length > 0)(
                    result,
                ).match<PromptSubmitResult>({
                    some: (proceeded) => ({ ...proceeded, context: [...(proceeded.context ?? []), ...context] }),
                    none: () => result,
                });
            },
            deny: absurd,
            answer: absurd,
        });
    });
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { promptSubmit };
