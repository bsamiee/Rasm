// The matched hook over each answered turn, the spoken summary registered under its option

// --- [IMPORTS] -------------------------------------------------------------------------

import type { On } from 'claude-code';
import type { Options } from '../host/options.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _MODEL = 'haiku';
const _SUMMARY_TOKENS = 60;
const _SUMMARY_SYSTEM = 'The message is an assistant answer to the person, reply with one spoken sentence summarising it and nothing else.';

// --- [REGISTRATION] --------------------------------------------------------------------

// A failed call drops the line and never the turn
const _speak = (on: On): void => {
    on('turn.complete', { reason: 'answer' }, async ($, e, next) => {
        const result = await next(e);
        if (e.answer !== '') {
            await $.model
                .complete({ model: _MODEL, system: _SUMMARY_SYSTEM, prompt: e.answer, maxTokens: _SUMMARY_TOKENS })
                .then((line) => (line === '' ? undefined : $.audio.speak(line)))
                .catch(() => undefined);
        }
        return result;
    });
};

const turnComplete = (on: On, options: Options): void => {
    if (options.speak) {
        _speak(on);
    }
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { turnComplete };
