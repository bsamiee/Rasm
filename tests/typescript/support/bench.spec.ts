import { FileSystem, Path } from '@effect/platform';
import { NodeContext } from '@effect/platform-node';
import { describe, expect, it, layer } from '@effect/vitest';
import { Array, Effect, String } from 'effect';
import {
    Benchmark,
    BenchmarkDirectory,
    BenchmarkError,
    BenchmarkResult,
} from './bench.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _SLUMPED_LATEST = JSON.stringify({
    files: [
        {
            filepath: '/abs/support.bench.ts',
            groups: [
                {
                    fullName: 'test support benchmarks',
                    benchmarks: [{ name: 'summarize', hz: 58, rme: 1 }],
                },
            ],
        },
    ],
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const _run = (
    name: string,
    hzTrail: ReadonlyArray<number>,
    rme = 1,
): ReadonlyArray<BenchmarkResult> =>
    Array.map(
        hzTrail,
        (hz, index) =>
            new BenchmarkResult({
                timestamp: `2026-01-0${index + 1}T00:00:00Z`,
                name,
                hz,
                rme,
            }),
    );

const _seededDirectory = (history: ReadonlyArray<BenchmarkResult>) =>
    Effect.gen(function* () {
        const fs = yield* FileSystem.FileSystem;
        const path = yield* Path.Path;
        const home = yield* fs.makeTempDirectoryScoped();
        const lines = Array.map(history, (row) =>
            JSON.stringify({
                timestamp: row.timestamp,
                name: row.name,
                hz: row.hz,
                rme: row.rme,
            }),
        );
        yield* fs.writeFileString(
            path.join(home, 'history.ndjson'),
            `${lines.join('\n')}\n`,
        );
        yield* fs.writeFileString(
            path.join(home, 'latest.json'),
            _SLUMPED_LATEST,
        );
        return home;
    });

describe('sustained regression detection', () => {
    it('a window of consecutive slow runs against the baseline is a regression', () => {
        const report = Benchmark.summarize(
            _run('summarize', [100, 101, 99, 100, 100, 60, 58, 61]),
        );
        expect(report.verdict).toBe('regression');
        expect(
            Array.map(report.benchmarks, (benchmark) => benchmark.verdict),
        ).toEqual(['regression']);
    });

    it('a single slow result does not count as a sustained regression', () => {
        const report = Benchmark.summarize(
            _run('spike', [100, 101, 99, 100, 60, 100, 101]),
        );
        expect(report.verdict).toBe('pass');
    });

    it('a performance change inside tolerance passes', () => {
        const report = Benchmark.summarize(
            _run('within-tolerance', [100, 101, 99, 100, 100, 95, 94, 96]),
        );
        expect(report.verdict).toBe('pass');
    });

    it('a noisy recent window is reported, not silently passed', () => {
        const report = Benchmark.summarize(
            _run('noisy', [100, 101, 99, 100, 100, 98, 97, 99], 25),
        );
        expect(report.verdict).toBe('noisy');
    });

    it('short history passes because it cannot establish a sustained regression', () => {
        const report = Benchmark.summarize(
            _run('short-history', [100, 60, 58, 61]),
        );
        expect(report.verdict).toBe('pass');
    });

    it('empty benchmark history produces an empty passing report', () => {
        const report = Benchmark.summarize([]);
        expect(report.benchmarks).toEqual([]);
        expect(report.verdict).toBe('pass');
    });
});

layer(NodeContext.layer)('benchmark regression check', (it) => {
    it.effect(
        'the check returns a typed error for a sustained regression',
        () =>
            Effect.scoped(
                Effect.gen(function* () {
                    const home = yield* _seededDirectory(
                        _run(
                            'test support benchmarks::summarize',
                            [100, 101, 99, 100, 60, 58],
                        ),
                    );
                    const error = yield* Effect.flip(
                        Effect.provideService(
                            Benchmark.checkRegression(),
                            BenchmarkDirectory,
                            home,
                        ),
                    );
                    expect(error).toBeInstanceOf(BenchmarkError);
                    expect(error.reason).toBe('regression');
                    expect(error.detail).toContain(
                        'test support benchmarks::summarize',
                    );
                }),
            ),
    );

    it.effect(
        'the check returns every benchmark result for history without regressions',
        () =>
            Effect.scoped(
                Effect.gen(function* () {
                    const home = yield* _seededDirectory(
                        _run('test support benchmarks::summarize', [60, 59, 61, 60, 58, 61]),
                    );
                    const report = yield* Effect.provideService(
                        Benchmark.checkRegression(),
                        BenchmarkDirectory,
                        home,
                    );
                    expect(report.verdict).toBe('pass');
                    expect(
                        Array.map(
                            report.benchmarks,
                            (benchmark) => benchmark.name,
                        ),
                    ).toEqual(['test support benchmarks::summarize']);
                }),
            ),
    );

    it.effect(
        'reprocessing one autosave does not duplicate benchmark history',
        () =>
            Effect.scoped(
                Effect.gen(function* () {
                    const fs = yield* FileSystem.FileSystem;
                    const path = yield* Path.Path;
                    const home = yield* _seededDirectory(
                        _run('test support benchmarks::summarize', [60, 61]),
                    );
                    const check = Effect.provideService(
                        Benchmark.checkRegression(),
                        BenchmarkDirectory,
                        home,
                    );
                    yield* check;
                    yield* check;
                    const raw = yield* fs.readFileString(
                        path.join(home, 'history.ndjson'),
                    );
                    const appended = Array.filter(
                        String.split(raw, '\n'),
                        (line) =>
                            String.isNonEmpty(line) && line.includes('"hz":58'),
                    );
                    expect(appended).toHaveLength(1);
                }),
            ),
    );

    it.effect(
        'a missing autosave returns a typed unreadable error',
        () =>
            Effect.scoped(
                Effect.gen(function* () {
                    const fs = yield* FileSystem.FileSystem;
                    const home = yield* fs.makeTempDirectoryScoped();
                    const error = yield* Effect.flip(
                        Effect.provideService(
                            Benchmark.checkRegression(),
                            BenchmarkDirectory,
                            home,
                        ),
                    );
                    expect(error.reason).toBe('unreadable');
                }),
            ),
    );

    it.effect(
        'a corrupted history line returns a typed malformed error rather than an Effect defect',
        () =>
            Effect.scoped(
                Effect.gen(function* () {
                    const fs = yield* FileSystem.FileSystem;
                    const path = yield* Path.Path;
                    const home = yield* fs.makeTempDirectoryScoped();
                    yield* fs.writeFileString(
                        path.join(home, 'history.ndjson'),
                        'not-a-benchmark-result\n',
                    );
                    yield* fs.writeFileString(
                        path.join(home, 'latest.json'),
                        _SLUMPED_LATEST,
                    );
                    const error = yield* Effect.flip(
                        Effect.provideService(
                            Benchmark.checkRegression(),
                            BenchmarkDirectory,
                            home,
                        ),
                    );
                    expect(error).toBeInstanceOf(BenchmarkError);
                    expect(error.reason).toBe('malformed');
                }),
            ),
    );
});
