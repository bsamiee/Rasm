// Secret pairs from the store, redaction forward at prompt.submit and restoration backward at tool.call

// --- [IMPORTS] -------------------------------------------------------------------------

import { type Decision, type Rule, rewrite } from '../composition/decision.ts';
import type { Secrets } from '../host/store.ts';
import { notes, type Pair, replace } from '../text/replace.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _NOTE = 'Secret values were replaced by their ids before you read the prompt';

// --- [OPERATIONS] ----------------------------------------------------------------------

const pairs = (secrets: Secrets): readonly Pair[] =>
    Object.entries(secrets)
        .filter(([, value]) => value !== '')
        .map(([id, value]): Pair => ({ from: value, to: id, note: _NOTE }));

// Redaction rewrites the prompt text and its context names the replacement, never the value
const redact =
    <E extends { readonly text: string }>(secretPairs: readonly Pair[]): Rule<E, never, never> =>
    (e: E): Decision<E, never, never> => {
        const replacement = replace(secretPairs, 'forward')(e.text);
        return rewrite({ ...e, text: replacement.text }, notes(replacement));
    };

// Restoration rewrites the command for the guards to read the real value, with no context because a line names the value
const restore =
    <E extends { readonly command: string }>(secretPairs: readonly Pair[]): Rule<E, never, never> =>
    (e: E): Decision<E, never, never> =>
        rewrite({ ...e, command: replace(secretPairs, 'backward')(e.command).text }, []);

// --- [EXPORTS] -------------------------------------------------------------------------

export { pairs, redact, restore };
