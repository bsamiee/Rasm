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
            { from: 'value-a', to: 'tokenA' },
            { from: 'value-c', to: 'tokenC' },
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

    it('applies the pairs in table order, the second reads the first result', () => {
        expect(_plain(redact<Prompt>(pairs({ b: 'a', c: 'b' }))({ text: 'a' }))).toStrictEqual({ e: { text: 'c' }, context: [_NOTE] });
    });
});

describe('restore', () => {
    it('puts the value back in the command with no context', () => {
        expect(_plain(restore<Call>(_PAIRS)({ command: 'curl -H tokenA' }))).toStrictEqual({
            e: { command: 'curl -H value-a' },
            context: [],
        });
    });

    it('keeps a $ in the value literal', () => {
        expect(_plain(restore<Call>(pairs({ tokenA: '$&$1' }))({ command: 'curl -H tokenA' }))).toStrictEqual({
            e: { command: 'curl -H $&$1' },
            context: [],
        });
    });
});
