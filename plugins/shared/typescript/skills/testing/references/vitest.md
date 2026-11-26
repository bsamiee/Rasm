# [VITEST]

## [01]-[ISOLATION]

Vitest options separate files and projects sharing a resource or state:
- `// @vitest-environment <name>` at a file's top overrides one file's environment
- Pool `forks` is the default, and native addons that are not thread-safe need it
- `isolate: false` reuses one module graph across files that share no state
- `sequence.groupOrder` orders projects that compete for one resource (a database container)
- `fileParallelism: false` serializes files that touch tracked fixtures or git state

## [02]-[TYPE_TESTS]

Runtime tests cover executed behavior, and type tests cover inference, assignability, rejection, and displayed types:
- Vitest `typecheck` runs `*.test-d.ts` files under `typecheck.enabled`, or `--typecheck` on one run (`nx test <project> -- --typecheck`)
- `expectTypeOf` and `assertType` state type assertions Vitest analyzes and never runs
- `typecheck.checker` defaults to `tsc`, run with `--noEmit`, and `typecheck.tsconfig` to the nearest `tsconfig.json`
- Source errors fail type tests unless `typecheck.ignoreSourceErrors` is on

## [03]-[BENCHMARKS]

```ts
import { test } from 'vitest';

test('decode', async ({ bench }) => {
    await bench('decode', () => decode(input)).run();
});
```

`decode` and `input` are the function and value under measurement:
- Benchmarks sit in `*.bench.ts`, matched by `benchmark.include`, and the `bench` fixture throws in any other file
- `vitest bench` runs benchmarks alone, and `benchmark.enabled: true` runs them after regular tests
- `bench.compare()` interleaves iterations of each case and prints one table
- `writeResult` stores a result as JSON, and `bench.from()` reads it back for comparison
- `expect(a).toBeFasterThan(b, { delta })` asserts a relative difference
- Benchmarks import built output, as Vite's module runner wraps each cross-module call of source in a getter
- Gates compare deterministic quantities alone: bundle bytes against the base, `tsc --extendedDiagnostics` instantiation counts
- Wall time needs CPU simulation, a dedicated machine, or base and head runs paired in one job
- Process-level timing runs `hyperfine --warmup <n> --min-runs <n> --export-json <file>` over the built CLI
- Benchmark tasks depend on the build they measure and stay out of task caching
