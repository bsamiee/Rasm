// Secret pairs from the store, redaction forward at prompt.submit and restoration backward at tool.call

// --- [IMPORTS] -------------------------------------------------------------------------

import type { StringRecord } from '../host/store.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const REDACTED = 'Secret values in the prompt appear as their ids';

// --- [OPERATIONS] ----------------------------------------------------------------------

// Every secret value becomes its id, the function replacer keeps a $ in a value literal
const redact = (text: string, secrets: StringRecord): string =>
    Object.entries(secrets)
        .filter(([, value]) => value !== '')
        .reduce((replaced, [id, value]) => replaced.replaceAll(value, () => id), text);

// Every id becomes its value, the form the guards read
const restore = (text: string, secrets: StringRecord): string =>
    Object.entries(secrets)
        .filter(([, value]) => value !== '')
        .reduce((replaced, [id, value]) => replaced.replaceAll(id, () => value), text);

// --- [EXPORTS] -------------------------------------------------------------------------

export { REDACTED, redact, restore };
