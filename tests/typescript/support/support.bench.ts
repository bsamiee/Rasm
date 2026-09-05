import { Array } from 'effect';
import { bench, describe } from 'vitest';
import { Benchmark, type BenchmarkResult } from './bench.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _RUNS = 50;
const _BENCHMARKS = 10;
const _BASE_HZ = 100;
const _RME = 2;

const _HISTORY: readonly BenchmarkResult[] = Array.flatMap(Array.range(0, _RUNS - 1), (run) =>
    Array.map(Array.range(0, _BENCHMARKS - 1), (index) => ({
        timestamp: `2026-01-01T00:${String(run).padStart(2, '0')}:00Z`,
        name: `benchmark-${index}`,
        hz: _BASE_HZ + run,
        rme: _RME,
    })),
);

// --- [OPERATIONS] ----------------------------------------------------------------------

describe('test support benchmarks', () => {
    bench('summarize sustained regressions over 500 results', () => {
        Benchmark.summarize(_HISTORY);
    });
});
