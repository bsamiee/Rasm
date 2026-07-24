# [TS_CORE_API_MITATA]

`mitata` mints the benchmark measurement shape — the `stats` percentile ladder, the optional `gc`/`heap` timing bands, and the addon-gated CPU hardware-counter block `measure` returns per run — and the state-free `measure` kernel a caller reaches for one raw sample.

`bench`/`group`/`run` and the plot wrappers mutate a module-global registration list and render a report; core composes the `stats` type and the state-free kernel, fencing the global surface to the runtime/tests bench lane.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `mitata`
- package: `mitata` (MIT)
- module: pure ESM, no `exports` map — `.` resolves `src/main.mjs` (registration/render, re-exporting `measure`/`do_not_optimize`); the state-free `src/lib.mjs` kernel resolves only by explicit `mitata/src/lib.mjs`
- runtime: node / bun / browser; `@mitata/counters` is the dynamically-imported optional native addon gating `stats.counters`
- rail: benchmark measurement statistics

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the `stats` record `measure` returns and the run-result shapes `run` yields — timing fields nanoseconds, the percentile ladder the regression basis, `gc`/`heap`/`counters` conditionally-present enrichment.

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CAPABILITY]                          |
| :-----: | :------- | :------------ | :------------------------------------ |
|  [01]   | `stats`  | interface     | timing ladder plus optional bands     |
|  [02]   | `Run`    | union         | per-run success-or-error result       |
|  [03]   | `trial`  | interface     | one benchmark's runs and render style |

[STATS]: `stats.min: number` `stats.max: number` `stats.avg: number` `stats.p25: number` `stats.p50: number` `stats.p75: number` `stats.p99: number` `stats.p999: number` `stats.samples: number[]` `stats.ticks: number` `stats.kind: 'fn'|'iter'|'yield'` `stats.debug: string` `stats.gc?: {avg;min;max;total}` `stats.heap?: {avg;min;max;total}` `stats.counters?: object`
[TRIAL]: `trial.runs: Run[]` `trial.alias: string` `trial.baseline: boolean` `trial.args: Record<string,any[]>` `trial.kind: 'args'|'static'|'multi-args'` `trial.style: {compact:boolean;highlight:false|string}`
[RUN]: `Run = ({stats;error:undefined}|{stats:undefined;error})&{name:string;args:Record<string,any>}`
[COUNTERS]: `counters.cycles.avg` `counters.instructions.avg` `counters.cache.avg` `counters.cache.misses.avg` `counters._bmispred.avg` — addon-runtime leaves under the `object` band, each carrying `.avg`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the module-global registration/render surface the bench lane composes; `measure` and `do_not_optimize` cross into state-free use, and the deep `src/lib.mjs` kernel carries the primitives `main.mjs` does not re-export.

| [INDEX] | [SURFACE]                                         | [SHAPE]  | [CAPABILITY]                    |
| :-----: | :------------------------------------------------ | :------- | :------------------------------ |
|  [01]   | `measure(fn\|gen\|iter, opts?) -> Promise<stats>` | static   | state-free one-shot sample      |
|  [02]   | `do_not_optimize(v)`                              | static   | DCE barrier                     |
|  [03]   | `bench(name?, fn\|gen\|iter) -> B`                | static   | global registration builder     |
|  [04]   | `run(opts?) -> Promise<{context, benchmarks}>`    | static   | list execution returning trials |
|  [05]   | `group\|summary\|compact(name?, f)`               | static   | registration scope wrappers     |
|  [06]   | `boxplot\|barplot\|lineplot(f)`                   | static   | plot-render wrappers            |
|  [07]   | `flags`                                           | property | `{compact, baseline}` bit-flags |
|  [08]   | `B`                                               | class    | fluent per-benchmark builder    |

`B` chains argument matrices, sweeps, GC policy, and render styling, each method returning `this`.

[B_METHODS]: `args(values[])` `args(name, values[])` `args(map)` `range(name, s, e, mult?)` `dense_range(name, s, e, acc?)` `gc('once'|'inner'|bool)` `baseline(bool?)` `compact(bool?)` `highlight(color?)` `name(name, color?)` `run(throw?) -> Promise<trial>`
[SAMPLERS]: `measure(fn\|gen\|iter, opts?)` `fn(fn, opts?)` `generator(gen, opts?)` `iter(iter, opts?)` — modality-specific `src/lib.mjs` samplers, each `-> Promise<stats>`.
[KERNEL_UTIL]: `gc()` `now()` `print(line)` `do_not_optimize(v)` `kind(fn)` — GC trigger, monotonic clock, writer, DCE barrier, modality classifier.
[SAMPLING_KNOBS]: `k_options` per-measure knob bag — `now`/`heap` clock overrides, `gc`/`inner_gc`, `concurrency`, and the sample/batch/warmup/cpu-time bounds — overriding the exported defaults `k_min_samples` `k_max_samples` `k_min_cpu_time` `k_warmup_samples` `k_batch_samples` `k_concurrency` `k_batch_unroll` `k_warmup_threshold` `k_batch_threshold` `k_samples_threshold`.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `measure` returns a pure `stats` record; the registration/render layer mutates a module-global list, so core composes the measurement shape alone.
- Every enrichment band is honestly optional at the boundary: `counters` populates only under the `@mitata/counters` addon and process counter access (Linux root, macOS Xcode toolchain), `gc`/`heap` only on a runtime exposing manual GC and heap metrics (`node --expose-gc`, bun); timing fields carry nanoseconds, `heap` bytes.

[STACKING]:
- `interchange/codec`(`BenchmarkClaimWire`): the wire band decodes `stats` present-or-absent per field; `run({ format: { json: { samples: true } } })` feeds the codec, which pairs the decoded band with `HostFingerprintWire` so a claim compares within one host print.
- `observe/board`(bench pack): trends landed claims through a percentile-ladder panel per alias, a GC-timing panel where `gc` is present, and an IPC/cache/branch panel over the counter block; claim-shaped rows ride the shared pack dispatch.
- `observe/board`(regression fold): a pure fold grades a candidate against the `baseline`-flagged claim per percentile, yielding a gate-read verdict; a cross-fingerprint pair yields the refusal verdict.

[LOCAL_ADMISSION]:
- Core imports the `stats` type; the registration/render surface stays in the runtime/tests bench lane, never `scope:core` domain code.

[RAIL_LAW]:
- Package: `mitata`
- Owns: the benchmark measurement shape — the `stats` percentile ladder, `gc`/`heap` bands, and addon-gated counter block — and the state-free `measure` kernel for raw sampling.
- Accept: the `stats` record as the wire measurement band; `run({ format: 'json' })` output as the codec feed; every enrichment band as optional; the deep `src/lib.mjs` `measure` for state-free sampling.
- Reject: the module-global registration surface in domain code; comparing claims across differing host fingerprints; assuming an enrichment band present when its addon or runtime capability is absent; tinybench for durable claims, its in-suite vitest timing carrying no counter or GC band.
