import { FileSystem, Path } from '@effect/platform';
import type { PlatformError } from '@effect/platform/Error';
import { NodeContext } from '@effect/platform-node';
import { describe, expect, it, layer } from '@effect/vitest';
import { Array, DateTime, Effect, type Scope, String } from 'effect';
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
    startTime: DateTime.toEpochMillis(DateTime.unsafeMake('2026-02-01T00:00:00Z')),
    testResults: [
        {
            assertionResults: [
                {
                    benchmarks: [
                        {
                            name: 'test support benchmarks',
                            tasks: [
                                { name: 'summarize', throughput: { mean: _LATEST_HZ, rme: 1 } },
                                { name: 'stored baseline', throughput: { mean: _BASELINE_HZ, rme: 1 }, fromStore: true },
                            ],
                        },
                    ],
                },
            ],
        },
    ],
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const _results = (name: string, hz: readonly number[], rme = 1): readonly BenchmarkResult[] =>
    Array.map(hz, (value, index) => ({ timestamp: `2026-01-0${index + 1}T00:00:00Z`, name, hz: value, rme }));

const _benchmarkDirectory = (
    history: readonly BenchmarkResult[],
    latest = _LATEST,
): Effect.Effect<string, PlatformError, FileSystem.FileSystem | Path.Path | Scope.Scope> =>
    Effect.gen(function* () {
        const fs = yield* FileSystem.FileSystem;
        const path = yield* Path.Path;
        const directory = yield* fs.makeTempDirectoryScoped();
        // Array.map passes the index into the optional replacer of JSON.stringify
        const lines = Array.map(history, (row) => JSON.stringify(row));
        yield* fs.writeFileString(path.join(directory, 'history.ndjson'), `${lines.join('\n')}\n`);
        yield* fs.writeFileString(path.join(directory, 'latest.json'), latest);
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
    test.effect(
        'the check returns a typed error for a sustained regression',
        Effect.fnUntraced(function* () {
            const error = yield* Effect.flip(_check([...Array.replicate(_BASELINE_HZ, Benchmark.policy.minHistory - 1), _SLOW_HZ, _SLOW_HZ]));
            expect(error).toBeInstanceOf(BenchmarkError);
            expect(error.reason).toBe('regression');
            expect(error.detail).toContain(_NAME);
        }),
    );

    test.effect(
        'the check returns every benchmark result for history without regressions',
        Effect.fnUntraced(function* () {
            const report = yield* _check(Array.replicate(_SLOW_HZ, Benchmark.policy.minHistory + 1));
            expect(report.verdict).toBe('pass');
            expect(Array.map(report.benchmarks, (benchmark) => benchmark.name)).toEqual([_NAME]);
        }),
    );

    test.scoped(
        'missing benchmark output files return a typed unreadable error',
        Effect.fnUntraced(function* () {
            const fs = yield* FileSystem.FileSystem;
            const directory = yield* fs.makeTempDirectoryScoped();
            const error = yield* Effect.flip(Effect.provideService(Benchmark.checkRegression(), BenchmarkDirectory, directory));
            expect(error.reason).toBe('unreadable');
        }),
    );
});

layer(NodeContext.layer)('benchmark history file', (test) => {
    test.scoped(
        'reprocessing a benchmark report after its file timestamp changes leaves history unchanged',
        Effect.fnUntraced(function* () {
            const fs = yield* FileSystem.FileSystem;
            const path = yield* Path.Path;
            const directory = yield* _benchmarkDirectory(_results(_NAME, [_SLOW_HZ, _SLOW_HZ]));
            const check = Effect.provideService(Benchmark.checkRegression(), BenchmarkDirectory, directory);
            yield* check;
            const modifiedAt = DateTime.toDateUtc(DateTime.unsafeMake('2030-01-01T00:00:00Z'));
            yield* fs.utimes(path.join(directory, 'latest.json'), modifiedAt, modifiedAt);
            yield* check;
            const raw = yield* fs.readFileString(path.join(directory, 'history.ndjson'));
            const appended = Array.filter(String.split(raw, '\n'), (line) => line.includes(`"hz":${_LATEST_HZ}`));
            expect(appended).toHaveLength(1);
            expect(raw).not.toContain('stored baseline');
            expect(raw).toContain('2026-02-01T00:00:00.000Z');
        }),
    );

    test.scoped(
        'corrupted history lines return a typed malformed error',
        Effect.fnUntraced(function* () {
            const fs = yield* FileSystem.FileSystem;
            const path = yield* Path.Path;
            const directory = yield* fs.makeTempDirectoryScoped();
            yield* fs.writeFileString(path.join(directory, 'history.ndjson'), 'not-a-benchmark-result\n');
            yield* fs.writeFileString(path.join(directory, 'latest.json'), _LATEST);
            const error = yield* Effect.flip(Effect.provideService(Benchmark.checkRegression(), BenchmarkDirectory, directory));
            expect(error).toBeInstanceOf(BenchmarkError);
            expect(error.reason).toBe('malformed');
        }),
    );
});

layer(NodeContext.layer)('benchmark report projection', (test) => {
    test.scoped(
        'malformed reports never append a partially projected result',
        Effect.fnUntraced(function* () {
            const fs = yield* FileSystem.FileSystem;
            const path = yield* Path.Path;
            yield* Effect.forEach(
                [
                    `${_LATEST} invalid`,
                    `${_LATEST} ${_LATEST}`,
                    '{"startTime":0,"testResults":[{"assertionResults":[{"benchmarks":[{"name":"valid","tasks":[{"name":"task","throughput":{"mean":1,"rme":1}}]},{"name":"late invalid","tasks":{}}]}]}]}',
                    '{"startTime":0,"testResults":{}}',
                    '{"startTime":0,"testResults":[{"assertionResults":{}}]}',
                    '{"startTime":0,"testResults":[{"assertionResults":[{"benchmarks":{}}]}]}',
                    '{"startTime":0,"testResults":[{"assertionResults":[{"benchmarks":[{"name":null,"tasks":[]}]}]}]}',
                    _LATEST.replace('"name":"summarize"', '"name":null'),
                    _LATEST.replace('"fromStore":true', '"fromStore":null'),
                    _LATEST.replace('"fromStore":true', '"fromStore":"true"'),
                    _LATEST.replace('"mean":100', '"mean":"invalid"'),
                ],
                Effect.fnUntraced(function* (latest: string) {
                    const directory = yield* _benchmarkDirectory([], latest);
                    const before = yield* fs.readFileString(path.join(directory, 'history.ndjson'));
                    const error = yield* Effect.flip(Effect.provideService(Benchmark.importLatestResults, BenchmarkDirectory, directory));
                    expect(error.reason).toBe('malformed');
                    expect(yield* fs.readFileString(path.join(directory, 'history.ndjson'))).toBe(before);
                }),
            );
        }),
    );
    test.scoped(
        'invalid measured and stored tasks report their independent errors together',
        Effect.fnUntraced(function* () {
            const directory = yield* _benchmarkDirectory(
                [],
                _LATEST.replace('"name":"summarize"', '"name":null').replace('"mean":100', '"mean":"invalid"'),
            );
            const error = yield* Effect.flip(Effect.provideService(Benchmark.importLatestResults, BenchmarkDirectory, directory));
            expect(error.reason).toBe('malformed');
            expect(error.detail).toContain('name');
            expect(error.detail).toContain('mean');
        }),
    );
    test.scoped(
        'empty native reports return no rows and retain the append format',
        Effect.fnUntraced(function* () {
            const fs = yield* FileSystem.FileSystem;
            const path = yield* Path.Path;
            const directory = yield* _benchmarkDirectory([], '{"startTime":0,"testResults":[]}');
            const rows = yield* Effect.provideService(Benchmark.importLatestResults, BenchmarkDirectory, directory);
            expect(rows).toEqual([]);
            expect(yield* fs.readFileString(path.join(directory, 'history.ndjson'))).toBe('\n\n');
        }),
    );
});
