// prompt.submit, each secret value in the prompt becomes its id and the redacted text is recorded for the operator-named-target check

// --- [IMPORTS] -------------------------------------------------------------------------

import type { On, PromptSubmitInput, PromptSubmitResult } from 'claude-code';
import { absurd } from '../composition/decision.ts';
import { fromBoolean, fromPredicate } from '../composition/option.ts';
import type { Options } from '../host/options.ts';
import { key, secretsOf } from '../host/store.ts';
import { pairs, redact } from '../policies/secrets.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

const _proceeded = fromPredicate(
    (result: PromptSubmitResult): result is Extract<PromptSubmitResult, { readonly drop?: undefined }> => result.drop === undefined,
);

// --- [REGISTRATION] --------------------------------------------------------------------

const promptSubmit = (on: On, _options: Options): void => {
    on('prompt.submit', async ($, e, next) => {
        const [session, stored] = await Promise.all([$.session.id(), $.store.get(key('secrets'))]);
        const secretPairs = pairs(secretsOf(stored));
        return redact<PromptSubmitInput>(secretPairs)(e).match<Promise<PromptSubmitResult>>({
            rewrite: async (input, context) => {
                await $.store.set(key('prompt', session), { session, text: input.text });
                const result = await next(input);
                return _proceeded(result).match<PromptSubmitResult>({
                    some: (proceeded) =>
                        fromBoolean(context.length > 0).match<PromptSubmitResult>({
                            some: () => ({ ...proceeded, context: [...(proceeded.context ?? []), ...context] }),
                            none: () => proceeded,
                        }),
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
