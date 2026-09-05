import { FileSystem, Path } from '@effect/platform';
import type { PlatformError } from '@effect/platform/Error';
import { NodeContext } from '@effect/platform-node';
import { describe, expect, it, layer } from '@effect/vitest';
import { Array, Effect, type Scope, String } from 'effect';
import { Benchmark, BenchmarkDirectory, BenchmarkError, type BenchmarkReport, type BenchmarkResult } from './bench.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _BASELINE_HZ = 100;
const _SLOW_HZ = 60;
const _LATEST_HZ = 58;
const _NOISY_RME = 25;
const _TOLERATED_HZ = _BASELINE_HZ * (1 - Benchmark.policy.tolerance);
const _NAME = 'test support benchmarks::summarize';
const _BASELINE = Array.replicate(_BASELINE_HZ, Benchmark.policy.minHistory);
const _SLOW = Array.replicate(_SLOW_HZ, Benchmark.policy.window);
const _LATEST = JSON.stringify({
    files: [
        {
            filepath: '/abs/support.bench.ts',
            groups: [{ fullName: 'test support benchmarks', benchmarks: [{ name: 'summarize', hz: _LATEST_HZ, rme: 1 }] }],
        },
    ],
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const _results = (name: string, hz: readonly number[], rme = 1): readonly BenchmarkResult[] =>
    Array.map(hz, (value, index) => ({ timestamp: `2026-01-0${index + 1}T00:00:00Z`, name, hz: value, rme }));

const _benchmarkDirectory = (
    history: readonly BenchmarkResult[],
): Effect.Effect<string, PlatformError, FileSystem.FileSystem | Path.Path | Scope.Scope> =>
    Effect.gen(function* () {
        const fs = yield* FileSystem.FileSystem;
        const path = yield* Path.Path;
        const directory = yield* fs.makeTempDirectoryScoped();
        const lines = Array.map(history, (row) => JSON.stringify(row));
        yield* fs.writeFileString(path.join(directory, 'history.ndjson'), `${lines.join('\n')}\n`);
        yield* fs.writeFileString(path.join(directory, 'latest.json'), _LATEST);
        return directory;
    });

const _check = (history: readonly number[]): Effect.Effect<BenchmarkReport, BenchmarkError, FileSystem.FileSystem | Path.Path> =>
    Benchmark.checkRegression().pipe(
        Effect.provideServiceEffect(BenchmarkDirectory, Effect.orDie(_benchmarkDirectory(_results(_NAME, history)))),
        Effect.scoped,
    );

describe('sustained regression detection', () => {
    it('consecutive slow runs against the baseline are a regression', () => {
        const report = Benchmark.summarize(_results('summarize', [..._BASELINE, ..._SLOW]));
        expect(report.verdict).toBe('regression');
        expect(Array.map(report.benchmarks, (benchmark) => benchmark.verdict)).toEqual(['regression']);
    });

    it('single slow results pass', () => {
        const report = Benchmark.summarize(_results('spike', [..._BASELINE, _SLOW_HZ, ..._BASELINE]));
        expect(report.verdict).toBe('pass');
    });

    it('performance changes inside tolerance pass', () => {
        const report = Benchmark.summarize(_results('within-tolerance', [..._BASELINE, ...Array.replicate(_TOLERATED_HZ, Benchmark.policy.window)]));
        expect(report.verdict).toBe('pass');
    });

    it('noisy recent windows report the noisy verdict', () => {
        const report = Benchmark.summarize(_results('noisy', [..._BASELINE, ..._BASELINE], _NOISY_RME));
        expect(report.verdict).toBe('noisy');
    });

    it('short history passes because it cannot establish a sustained regression', () => {
        const report = Benchmark.summarize(_results('short-history', [_BASELINE_HZ, ..._SLOW]));
        expect(report.verdict).toBe('pass');
    });

    it('empty benchmark history produces an empty passing report', () => {
        const report = Benchmark.summarize([]);
        expect(report.benchmarks).toEqual([]);
        expect(report.verdict).toBe('pass');
    });
});

layer(NodeContext.layer)('benchmark regression check', (test) => {
    test.effect('the check returns a typed error for a sustained regression', () =>
        Effect.gen(function* () {
            const error = yield* Effect.flip(_check([...Array.replicate(_BASELINE_HZ, Benchmark.policy.minHistory - 1), _SLOW_HZ, _SLOW_HZ]));
            expect(error).toBeInstanceOf(BenchmarkError);
            expect(error.reason).toBe('regression');
            expect(error.detail).toContain(_NAME);
        }),
    );

    test.effect('the check returns every benchmark result for history without regressions', () =>
        Effect.gen(function* () {
            const report = yield* _check(Array.replicate(_SLOW_HZ, Benchmark.policy.minHistory + 1));
            expect(report.verdict).toBe('pass');
            expect(Array.map(report.benchmarks, (benchmark) => benchmark.name)).toEqual([_NAME]);
        }),
    );

    test.effect('missing benchmark output files return a typed unreadable error', () =>
        Effect.scoped(
            Effect.gen(function* () {
                const fs = yield* FileSystem.FileSystem;
                const directory = yield* fs.makeTempDirectoryScoped();
                const error = yield* Effect.flip(Effect.provideService(Benchmark.checkRegression(), BenchmarkDirectory, directory));
                expect(error.reason).toBe('unreadable');
            }),
        ),
    );
});

layer(NodeContext.layer)('benchmark history file', (test) => {
    test.effect('reprocessing one benchmark output file leaves the history unchanged', () =>
        Effect.scoped(
            Effect.gen(function* () {
                const fs = yield* FileSystem.FileSystem;
                const path = yield* Path.Path;
                const directory = yield* _benchmarkDirectory(_results(_NAME, [_SLOW_HZ, _SLOW_HZ]));
                const check = Effect.provideService(Benchmark.checkRegression(), BenchmarkDirectory, directory);
                yield* check;
                yield* check;
                const raw = yield* fs.readFileString(path.join(directory, 'history.ndjson'));
                const appended = Array.filter(String.split(raw, '\n'), (line) => line.includes(`"hz":${_LATEST_HZ}`));
                expect(appended).toHaveLength(1);
            }),
        ),
    );

    test.effect('corrupted history lines return a typed malformed error', () =>
        Effect.scoped(
            Effect.gen(function* () {
                const fs = yield* FileSystem.FileSystem;
                const path = yield* Path.Path;
                const directory = yield* fs.makeTempDirectoryScoped();
                yield* fs.writeFileString(path.join(directory, 'history.ndjson'), 'not-a-benchmark-result\n');
                yield* fs.writeFileString(path.join(directory, 'latest.json'), _LATEST);
                const error = yield* Effect.flip(Effect.provideService(Benchmark.checkRegression(), BenchmarkDirectory, directory));
                expect(error).toBeInstanceOf(BenchmarkError);
                expect(error.reason).toBe('malformed');
            }),
        ),
    );
});
