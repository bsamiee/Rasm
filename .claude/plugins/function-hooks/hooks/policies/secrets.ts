// Secret pairs from the store, redaction forward at prompt.submit and restoration backward at tool.call

// --- [IMPORTS] -------------------------------------------------------------------------

import { type Decision, type Rule, rewrite } from '../composition/decision.ts';
import { liftPredicate, toArray } from '../composition/option.ts';
import type { Secrets } from '../host/store.ts';

// --- [TYPES] ---------------------------------------------------------------------------

// One secret value with its id, redaction reads the pair as written and restoration reads it swapped
interface Pair {
    readonly from: string;
    readonly to: string;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const _NOTE = 'Secret values were replaced by their ids before you read the prompt';

// --- [OPERATIONS] ----------------------------------------------------------------------

const pairs = (secrets: Secrets): readonly Pair[] =>
    Object.entries(secrets)
        .filter(([, value]) => value !== '')
        .map(([id, value]): Pair => ({ from: value, to: id }));

// Every pair applied in table order, the function replacer keeps a $ in a secret value literal
const _replace = (text: string, secretPairs: readonly Pair[]): string =>
    secretPairs.reduce((replaced, pair) => replaced.replaceAll(pair.from, () => pair.to), text);

// Redaction rewrites the prompt text and its context names the replacement once, never the value
const redact =
    <E extends { readonly text: string }>(secretPairs: readonly Pair[]): Rule<E, never, never> =>
    (e: E): Decision<E, never, never> => {
        const text = _replace(e.text, secretPairs);
        return rewrite({ ...e, text }, toArray(liftPredicate<string>(() => text !== e.text)(_NOTE)));
    };

const _swapped = (pair: Pair): Pair => ({ from: pair.to, to: pair.from });

// Restoration rewrites the command for the guards to read the real value, with no context because a line names the value
const restore =
    <E extends { readonly command: string }>(secretPairs: readonly Pair[]): Rule<E, never, never> =>
    (e: E): Decision<E, never, never> =>
        rewrite({ ...e, command: _replace(e.command, secretPairs.map(_swapped)) }, []);

// --- [EXPORTS] -------------------------------------------------------------------------

export { pairs, redact, restore };
