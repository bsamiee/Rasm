import { Array } from 'effect';
import { bench, describe } from 'vitest';
import { Bench, BenchRow } from './bench.ts';
import { Imports } from './gauges.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _LEDGER = Array.flatMap(Array.range(0, 49), (run) =>
    Array.map(
        Array.range(0, 9),
        (lane) => new BenchRow({ at: `2026-01-01T00:${String(run).padStart(2, '0')}:00Z`, name: `lane-${lane}`, hz: 100 + run, rme: 2 }),
    ),
);

const _SOURCE = { path: 'probe.ts', text: "import { a } from './a.ts';\nimport type { B } from './b.ts';\nexport { a };\n" };

// --- [OPERATIONS] ----------------------------------------------------------------------

describe('kit hot paths', () => {
    bench('sustained-regression fold over a 500-row ledger', () => {
        Bench.fold(_LEDGER);
    });

    bench('import scan over a small module', () => {
        Imports.scan([_SOURCE]);
    });
});
