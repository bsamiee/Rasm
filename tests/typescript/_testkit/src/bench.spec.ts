import { describe, expect, it } from '@effect/vitest';
import { Array } from 'effect';
import { Bench, BenchRow } from './bench.ts';

// --- [OPERATIONS] ------------------------------------------------------------------------

const _run = (name: string, hzTrail: ReadonlyArray<number>, rme = 1): ReadonlyArray<BenchRow> =>
    Array.map(hzTrail, (hz, index) => new BenchRow({ at: `2026-01-0${index + 1}T00:00:00Z`, name, hz, rme }));

describe('sustained-regression fold', () => {
    it('a window of consecutive slow runs against the baseline is a breach', () => {
        const report = Bench.fold(_run('fold', [100, 101, 99, 100, 100, 60, 58, 61]));
        expect(report.verdict).toBe('breach');
        expect(Array.map(report.receipts, (receipt) => receipt.verdict)).toEqual(['breach']);
    });

    it('a single spike never trips the gate', () => {
        const report = Bench.fold(_run('spike', [100, 101, 99, 100, 60, 100, 101]));
        expect(report.verdict).toBe('pass');
    });

    it('a drift inside tolerance never trips the gate', () => {
        const report = Bench.fold(_run('drift', [100, 101, 99, 100, 100, 95, 94, 96]));
        expect(report.verdict).toBe('pass');
    });

    it('a noisy recent window is reported, not silently passed', () => {
        const report = Bench.fold(_run('noisy', [100, 101, 99, 100, 100, 98, 97, 99], 25));
        expect(report.verdict).toBe('noisy');
    });

    it('short history never breaches: the gate demands evidence before it fires', () => {
        const report = Bench.fold(_run('young', [100, 60, 58, 61]));
        expect(report.verdict).toBe('pass');
    });

    it('an empty ledger folds to an empty pass, and the gate verb refuses to run without a harvest', () => {
        const report = Bench.fold([]);
        expect(report.receipts).toEqual([]);
        expect(report.verdict).toBe('pass');
    });
});
