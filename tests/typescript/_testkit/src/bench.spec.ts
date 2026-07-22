import { FileSystem, Path } from '@effect/platform';
import { NodeContext } from '@effect/platform-node';
import { describe, expect, it, layer } from '@effect/vitest';
import { Array, Effect, String } from 'effect';
import { Bench, BenchFault, BenchHome, BenchRow } from './bench.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

// A vitest-shaped latest.json whose lane continues an already-slumped history: harvesting it must trip the gate.
const _SLUMPED_LATEST = JSON.stringify({
    files: [{ filepath: '/abs/kit.bench.ts', groups: [{ fullName: 'kit hot paths', benchmarks: [{ name: 'fold', hz: 58, rme: 1 }] }] }],
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const _run = (name: string, hzTrail: ReadonlyArray<number>, rme = 1): ReadonlyArray<BenchRow> =>
    Array.map(hzTrail, (hz, index) => new BenchRow({ at: `2026-01-0${index + 1}T00:00:00Z`, name, hz, rme }));

const _seededHome = (history: ReadonlyArray<BenchRow>) =>
    Effect.gen(function* () {
        const fs = yield* FileSystem.FileSystem;
        const path = yield* Path.Path;
        const home = yield* fs.makeTempDirectoryScoped();
        const lines = Array.map(history, (row) => JSON.stringify({ at: row.at, name: row.name, hz: row.hz, rme: row.rme }));
        yield* fs.writeFileString(path.join(home, 'history.ndjson'), `${lines.join('\n')}\n`);
        yield* fs.writeFileString(path.join(home, 'latest.json'), _SLUMPED_LATEST);
        return home;
    });

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

    it('an empty ledger folds to an empty pass', () => {
        const report = Bench.fold([]);
        expect(report.receipts).toEqual([]);
        expect(report.verdict).toBe('pass');
    });
});

layer(NodeContext.layer)('gate verb', (it) => {
    it.effect('the gate FAILS typed when the harvested run sustains a slump', () =>
        Effect.scoped(
            Effect.gen(function* () {
                const home = yield* _seededHome(_run('kit hot paths::fold', [100, 101, 99, 100, 60, 58]));
                const fault = yield* Effect.flip(Effect.provideService(Bench.gate(), BenchHome, home));
                expect(fault).toBeInstanceOf(BenchFault);
                expect(fault.reason).toBe('breach');
                expect(fault.detail).toContain('kit hot paths::fold');
            }),
        ),
    );

    it.effect('the gate passes a healthy ledger and reports every lane receipt', () =>
        Effect.scoped(
            Effect.gen(function* () {
                const home = yield* _seededHome(_run('kit hot paths::fold', [60, 59, 61, 60, 58, 61]));
                const report = yield* Effect.provideService(Bench.gate(), BenchHome, home);
                expect(report.verdict).toBe('pass');
                expect(Array.map(report.receipts, (receipt) => receipt.name)).toEqual(['kit hot paths::fold']);
            }),
        ),
    );

    it.effect('a second gate over one autosave harvests nothing: duplicated rows cannot forge the slump evidence', () =>
        Effect.scoped(
            Effect.gen(function* () {
                const fs = yield* FileSystem.FileSystem;
                const path = yield* Path.Path;
                const home = yield* _seededHome(_run('kit hot paths::fold', [60, 61]));
                const gate = Effect.provideService(Bench.gate(), BenchHome, home);
                yield* gate;
                yield* gate;
                const raw = yield* fs.readFileString(path.join(home, 'history.ndjson'));
                const appended = Array.filter(String.split(raw, '\n'), (line) => String.isNonEmpty(line) && line.includes('"hz":58'));
                expect(appended).toHaveLength(1);
            }),
        ),
    );

    it.effect('a missing autosave is a typed fault: the gate never runs without a harvest', () =>
        Effect.scoped(
            Effect.gen(function* () {
                const fs = yield* FileSystem.FileSystem;
                const home = yield* fs.makeTempDirectoryScoped();
                const fault = yield* Effect.flip(Effect.provideService(Bench.gate(), BenchHome, home));
                expect(fault.reason).toBe('unreadable');
            }),
        ),
    );

    it.effect('a corrupted history line is a typed malformed fault, never a die', () =>
        Effect.scoped(
            Effect.gen(function* () {
                const fs = yield* FileSystem.FileSystem;
                const path = yield* Path.Path;
                const home = yield* fs.makeTempDirectoryScoped();
                yield* fs.writeFileString(path.join(home, 'history.ndjson'), 'not-a-ledger-row\n');
                yield* fs.writeFileString(path.join(home, 'latest.json'), _SLUMPED_LATEST);
                const fault = yield* Effect.flip(Effect.provideService(Bench.gate(), BenchHome, home));
                expect(fault).toBeInstanceOf(BenchFault);
                expect(fault.reason).toBe('malformed');
            }),
        ),
    );
});
