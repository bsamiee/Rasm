// --- [IMPORTS] -------------------------------------------------------------------------

import { describe, expect, it } from 'vitest';
import { notes, type Pair, replace } from './replace.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _SECRET: Pair = { from: 'secret-value', to: 'TOKEN_A', note: 'Secret values were replaced by their ids before you read the prompt' };
const _NPM: Pair = { from: /^npm /gu, to: 'pnpm ', note: 'Ran pnpm in place of npm' };
const _DOLLAR: Pair = { from: 'a', to: '$&$1', note: '' };
const _ABSENT: Pair = { from: 'zzz', to: 'yyy', note: 'absent' };
const _A_TO_B: Pair = { from: 'a', to: 'b', note: 'x' };
const _B_TO_C: Pair = { from: 'b', to: 'c', note: 'x' };

// --- [TESTS] ---------------------------------------------------------------------------

describe('replace', () => {
    it('replaces a string pair forward', () => {
        expect(replace([_SECRET], 'forward')('use secret-value now')).toStrictEqual({ text: 'use TOKEN_A now', applied: [_SECRET] });
    });

    it('replaces a string pair backward', () => {
        expect(replace([_SECRET], 'backward')('use TOKEN_A now')).toStrictEqual({ text: 'use secret-value now', applied: [_SECRET] });
    });

    it('applies a RegExp pair forward', () => {
        expect(replace([_NPM], 'forward')('npm install')).toStrictEqual({ text: 'pnpm install', applied: [_NPM] });
    });

    it('leaves the text unchanged for a RegExp pair backward', () => {
        expect(replace([_NPM], 'backward')('pnpm install')).toStrictEqual({ text: 'pnpm install', applied: [] });
    });

    it('keeps a $ in the replacement literal', () => {
        expect(replace([_DOLLAR], 'forward')('a')).toStrictEqual({ text: '$&$1', applied: [_DOLLAR] });
    });

    it('records no pair that does not occur', () => {
        expect(replace([_ABSENT, _SECRET], 'forward')('secret-value')).toStrictEqual({ text: 'TOKEN_A', applied: [_SECRET] });
    });

    it('applies pairs in table order, the second reads the first result', () => {
        expect(replace([_A_TO_B, _B_TO_C], 'forward')('a')).toStrictEqual({ text: 'c', applied: [_A_TO_B, _B_TO_C] });
    });
});

describe('notes', () => {
    it('drops the empty note and the duplicate and keeps table order', () => {
        expect(notes({ text: '', applied: [_A_TO_B, _DOLLAR, _B_TO_C, _NPM] })).toStrictEqual(['x', 'Ran pnpm in place of npm']);
    });
});
