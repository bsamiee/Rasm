import { Array } from 'effect';
import { bench, describe } from 'vitest';
import { Bench, BenchRow } from './bench.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _LEDGER = Array.flatMap(Array.range(0, 49), (run) =>
    Array.map(
        Array.range(0, 9),
        (lane) =>
            new BenchRow({
                at: `2026-01-01T00:${String(run).padStart(2, '0')}:00Z`,
                name: `lane-${lane}`,
                hz: 100 + run,
                rme: 2,
            }),
    ),
);

// --- [OPERATIONS] ----------------------------------------------------------------------

describe('kit hot paths', () => {
    bench('sustained-regression fold over a 500-row ledger', () => {
        Bench.fold(_LEDGER);
    });
});
