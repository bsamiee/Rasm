import { Array } from 'effect';
import { bench, describe } from 'vitest';
import { Benchmark, BenchmarkResult } from './bench.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _HISTORY = Array.flatMap(Array.range(0, 49), (run) =>
    Array.map(
        Array.range(0, 9),
        (benchmarkIndex) =>
            new BenchmarkResult({
                timestamp: `2026-01-01T00:${String(run).padStart(2, '0')}:00Z`,
                name: `benchmark-${benchmarkIndex}`,
                hz: 100 + run,
                rme: 2,
            }),
    ),
);

// --- [OPERATIONS] ----------------------------------------------------------------------

describe('test support benchmarks', () => {
    bench('summarize sustained regressions over 500 results', () => {
        Benchmark.summarize(_HISTORY);
    });
});
