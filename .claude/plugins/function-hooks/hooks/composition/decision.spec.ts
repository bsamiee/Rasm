// --- [IMPORTS] -------------------------------------------------------------------------

import { describe, expect, it } from 'vitest';
import { absurd, answer, type Decision, deny, fold, type Rule, rewrite, when } from './decision.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface Call {
    readonly tool: string;
    readonly command: string;
}

type Plain<E, R, D> =
    | { readonly kind: 'rewrite'; readonly e: E; readonly context: readonly string[] }
    | { readonly kind: 'deny'; readonly reason: D }
    | { readonly kind: 'answer'; readonly result: R };

// --- [OPERATIONS] ----------------------------------------------------------------------

// Reads a decision as plain data for value comparison in assertions
const _plain = <E, R, D>(decision: Decision<E, R, D>): Plain<E, R, D> =>
    decision.match<Plain<E, R, D>>({
        rewrite: (e, context) => ({ kind: 'rewrite', e, context }),
        deny: (reason) => ({ kind: 'deny', reason }),
        answer: (result) => ({ kind: 'answer', result }),
    });

const _isBash = (e: Call): e is Call & { readonly tool: 'Bash' } => e.tool === 'Bash';

const _upper: Rule<Call, string, string> = (e) => rewrite({ ...e, command: e.command.toUpperCase() }, ['upper']);

const _suffix: Rule<Call, string, string> = (e) => rewrite({ ...e, command: `${e.command}!` }, ['suffix']);

const _refuse: Rule<Call, string, string> = () => deny('refused');

const _reply: Rule<Call, string, string> = (e) => answer(`ran ${e.command}`);

// --- [TESTS] ---------------------------------------------------------------------------

describe('fold', () => {
    it('passes an event through an empty table with no context', () => {
        expect(_plain(fold<Call, string, string>([])({ tool: 'Bash', command: 'ls' }))).toStrictEqual({
            kind: 'rewrite',
            e: { tool: 'Bash', command: 'ls' },
            context: [],
        });
    });

    it('feeds each rewrite to the next rule and accumulates context in table order', () => {
        expect(_plain(fold([_upper, _suffix])({ tool: 'Bash', command: 'ls' }))).toStrictEqual({
            kind: 'rewrite',
            e: { tool: 'Bash', command: 'LS!' },
            context: ['upper', 'suffix'],
        });
    });

    it('ends on a deny and skips the rules after it', () => {
        expect(_plain(fold([_upper, _refuse, _reply])({ tool: 'Bash', command: 'ls' }))).toStrictEqual({ kind: 'deny', reason: 'refused' });
    });

    it('ends on an answer and skips the rules after it', () => {
        expect(_plain(fold([_reply, _refuse])({ tool: 'Bash', command: 'ls' }))).toStrictEqual({ kind: 'answer', result: 'ran ls' });
    });

    it('folds a table with deny type never', () => {
        const rules: readonly Rule<Call, string, never>[] = [(e): Decision<Call, string, never> => rewrite(e, ['seen'])];
        expect(_plain(fold(rules)({ tool: 'Read', command: '' }))).toStrictEqual({
            kind: 'rewrite',
            e: { tool: 'Read', command: '' },
            context: ['seen'],
        });
    });
});

describe('absurd', () => {
    it('stands in the deny and answer arms of a decision with a type that rules them out', () => {
        const rules: readonly Rule<Call, never, never>[] = [(e): Decision<Call, never, never> => rewrite(e, ['seen'])];
        expect(
            fold(rules)({ tool: 'Read', command: '' }).match<readonly string[]>({ rewrite: (_e, context) => context, deny: absurd, answer: absurd }),
        ).toStrictEqual(['seen']);
    });
});

describe('when', () => {
    it('passes a non-matching input through unchanged', () => {
        expect(_plain(when(_isBash, _refuse)({ tool: 'Read', command: 'x' }))).toStrictEqual({
            kind: 'rewrite',
            e: { tool: 'Read', command: 'x' },
            context: [],
        });
    });

    it('applies the rule on a matching input', () => {
        expect(_plain(when(_isBash, _refuse)({ tool: 'Bash', command: 'x' }))).toStrictEqual({ kind: 'deny', reason: 'refused' });
    });
});
