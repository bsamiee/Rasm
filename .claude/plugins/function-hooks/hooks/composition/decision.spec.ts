import { describe, expect, it } from 'vitest';
import { type Decision, deny, fold, type Rule, rewrite, when } from './decision.ts';

interface Call {
    readonly n: number;
}

const _add: Rule<Call> = (e) => rewrite({ n: e.n + 1 }, [`added at ${e.n}`]);

const _even = (e: Call): e is Call & { readonly n: number } => e.n % 2 === 0;

describe('fold', () => {
    it('feeds each rewrite to the next rule and keeps every context line', () => {
        expect(fold([_add, _add])({ n: 0 })).toStrictEqual({ kind: 'rewrite', e: { n: 2 }, context: ['added at 0', 'added at 1'] });
    });

    it('ends at the first deny and skips the rules after it', () => {
        expect(fold<Call>([_add, (): Decision<Call> => deny('stop'), _add])({ n: 0 })).toStrictEqual({ kind: 'deny', reason: 'stop' });
    });

    it('lifts a rule over the events its refinement selects and passes the rest', () => {
        expect(when(_even, _add)({ n: 2 })).toStrictEqual({ kind: 'rewrite', e: { n: 3 }, context: ['added at 2'] });
        expect(when(_even, _add)({ n: 1 })).toStrictEqual({ kind: 'rewrite', e: { n: 1 }, context: [] });
    });
});
