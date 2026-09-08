// prompt.submit, each secret value in the prompt becomes its id and the redacted text is recorded for the operator-named-target check

// --- [IMPORTS] -------------------------------------------------------------------------

import type { On } from 'claude-code';
import { decode, isStringRecord, key } from '../host/store.ts';
import { REDACTED, redact } from '../policies/secrets.ts';

// --- [REGISTRATION] --------------------------------------------------------------------

const promptSubmit = (on: On): void => {
    on('prompt.submit', async ($, e, next) => {
        const [session, secrets] = await Promise.all([$.session.id(), $.store.get(key('secrets'))]);
        const text = redact(e.text, decode(isStringRecord)(secrets) ?? {});
        await $.store.set(key('prompt', session), { text });
        const result = await next({ ...e, text });
        // Proceeded results take the line, and an unchanged text leaves the result as next resolved it
        return result.drop === undefined && text !== e.text ? { ...result, context: [...(result.context ?? []), REDACTED] } : result;
    });
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { promptSubmit };
