// --- [IMPORTS] -------------------------------------------------------------------------

import { describe, expect, it } from 'vitest';
import { absurd, type Decision } from '../composition/decision.ts';
import { pairs, redact, restore } from './secrets.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface Prompt {
    readonly text: string;
}

interface Call {
    readonly command: string;
}

interface Plain<E> {
    readonly e: E;
    readonly context: readonly string[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const _NOTE = 'Secret values were replaced by their ids before you read the prompt';
const _PAIRS = pairs({ tokenA: 'value-a', tokenB: '', tokenC: 'value-c' });

// --- [OPERATIONS] ----------------------------------------------------------------------

// Reads the one arm a rule typed never on deny and answer can take
const _plain = <E>(decision: Decision<E, never, never>): Plain<E> =>
    decision.match<Plain<E>>({ rewrite: (e, context) => ({ e, context }), deny: absurd, answer: absurd });

// --- [TESTS] ---------------------------------------------------------------------------

describe('pairs', () => {
    it('maps each non-empty value to its id with the one note', () => {
        expect(_PAIRS).toStrictEqual([
            { from: 'value-a', to: 'tokenA', note: _NOTE },
            { from: 'value-c', to: 'tokenC', note: _NOTE },
        ]);
    });
});

describe('redact', () => {
    it('replaces every value by its id and names the replacement once', () => {
        expect(_plain(redact<Prompt>(_PAIRS)({ text: 'use value-a and value-c' }))).toStrictEqual({
            e: { text: 'use tokenA and tokenC' },
            context: [_NOTE],
        });
    });

    it('passes a prompt without a value through with no context', () => {
        expect(_plain(redact<Prompt>(_PAIRS)({ text: 'plain' }))).toStrictEqual({ e: { text: 'plain' }, context: [] });
    });
});

describe('restore', () => {
    it('puts the value back in the command with no context', () => {
        expect(_plain(restore<Call>(_PAIRS)({ command: 'curl -H tokenA' }))).toStrictEqual({
            e: { command: 'curl -H value-a' },
            context: [],
        });
    });
});
