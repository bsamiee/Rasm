# [TS_CORE_API_MITATA]

`mitata` is a zero-dependency high-precision microbenchmark harness whose real value to core is not its runner but its `stats` SHAPE — the percentile ladder, GC timing band, JS-heap estimate, and optional CPU hardware-counter block it produces per benchmark. Core mirrors that shape field-for-field into `BenchmarkClaimWire`'s measurement band, so a TS-lane benchmark run lands claims in the exact admitted family the C# corpus gate feeds, and the observe `bench` pack trends them beside every other signal.

Mechanism is a two-layer split: a REGISTRATION-and-RENDER layer (`bench`/`group`/`run` and the plot wrappers) that mutates a module-global benchmark list and prints a formatted report, and a MEASUREMENT layer (`measure`, re-exported from the deep `src/lib.mjs` module) that samples one function and returns a pure `stats` record with no global state. Core consumes the MEASUREMENT SHAPE only; the registration layer is module-global by construction and therefore never composed into library domain code — it stays in the runtime/tests bench lane, and core carries the typed band the wire mirrors.

`stats.counters` is populated only when the optional native addon `@mitata/counters` is installed and the process holds counter access (root on Linux, Xcode toolchain on macOS); `stats.gc`/`stats.heap` appear only on a runtime exposing manual GC and heap metrics (`node --expose-gc`, bun). Every band field is therefore honestly optional at the boundary — the wire landing decodes `counters`/`gc`/`heap` as present-or-absent, never assumed.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `mitata`
- package: `mitata` (MIT)
- entry: `.` → `src/main.mjs` (types `src/main.d.mts`) — the registration/render surface re-exporting `measure` and `do_not_optimize`.
- deep entry: `src/lib.mjs` (types `src/lib.d.mts`) — the state-free measurement kernel; reachable by explicit file path since the package declares no `exports` map, so `mitata/src/lib.mjs` resolves and the main entry does not surface its extra members.
- optional addon: `@mitata/counters` — separate native package the harness dynamically imports at `run` time for the CPU hardware-counter block; absent, `stats.counters` is undefined and the report drops the counter rows.
- plane: `plane:runtime`, edge-fenced to the bench/proof lane — the registration surface is module-global and never enters `scope:core` domain code.
- role: the measurement-band MODEL for `interchange/codec.md#LANDING_WIRE` `BenchmarkClaimWire` and the statistic source the observe `bench` pack and the baseline-versus-candidate regression fold read; core owns the typed shape, runtime/tests owns the runner.

## [02]-[MEASUREMENT_MODEL]

`stats` is the load-bearing surface — the per-benchmark record `measure` returns and the exact band `BenchmarkClaimWire` mirrors. Timing fields are nanoseconds; the percentile ladder is the regression fold's comparison basis; `gc`/`heap`/`counters` are the conditionally-present enrichment bands.

| [INDEX] | [FIELD]                        | [SHAPE]                              | [WIRE_MIRROR]                                             |
| :-----: | :----------------------------- | :----------------------------------- | :-------------------------------------------------------- |
|  [01]   | `min`/`max`/`avg`              | `number` (ns)                        | the claim's central-tendency triple                       |
|  [02]   | `p25`/`p50`/`p75`/`p99`/`p999` | `number` (ns)                        | the percentile ladder the regression fold grades on       |
|  [03]   | `samples`                      | `number[]` raw per-sample timings    | the histogram/boxplot source; sample-count evidence       |
|  [04]   | `ticks`                        | `number` total sampled iterations    | sampling-depth evidence on the claim                      |
|  [05]   | `kind`                         | `'fn' \| 'iter' \| 'yield'`          | the measurement modality discriminant                     |
|  [06]   | `gc?`                          | `{ avg, min, max, total }` (ns)      | GC-timing band — present only under exposed manual GC     |
|  [07]   | `heap?`                        | `{ avg, min, max, total }` (bytes)   | heap-delta band — present only under heap-metric runtimes |
|  [08]   | `counters?`                    | `HardwareCounters`                   | IPC/cache/branch band; addon-gated                        |
|  [09]   | `debug`                        | `string` engine/JIT debug annotation | diagnostic, not landed on the wire                        |

[COUNTER_AGGREGATE]: `CounterAggregate.avg: number`
[HARDWARE_COUNTERS]: `HardwareCounters.cycles: CounterAggregate` `HardwareCounters.instructions: CounterAggregate` `HardwareCounters.cache: CounterAggregate&{misses:CounterAggregate}` `HardwareCounters._bmispred: CounterAggregate`
[STATS]: `stats.debug: string` `stats.ticks: number` `stats.samples: number[]` `stats.kind: 'fn'|'iter'|'yield'` `stats.min: number` `stats.max: number` `stats.avg: number` `stats.p25: number` `stats.p50: number` `stats.p75: number` `stats.p99: number` `stats.p999: number` `stats.gc: {avg:number;min:number;max:number;total:number}` `stats.heap: {avg:number;min:number;max:number;total:number}` `stats.counters: HardwareCounters`
[TRIAL]: `trial.runs: Run[]` `trial.alias: string` `trial.baseline: boolean` `trial.args: Record<string,any[]>` `trial.kind: 'args'|'static'|'multi-args'` `trial.style: {compact:boolean;highlight:false|string}`
[RUN]: `Run = ({stats:stats;error:undefined}|{stats:undefined;error:unknown})&{name:string;args:Record<string,any>}`

## [03]-[HARNESS_SURFACE]

Registration and execution — the module-global surface the runtime/tests bench lane composes, never core domain code. `measure` and `do_not_optimize` are the two members that cross into state-free use.

| [INDEX] | [MEMBER]                        | [SHAPE]                                          | [ROLE]                                    |
| :-----: | :------------------------------ | :----------------------------------------------- | :---------------------------------------- |
|  [01]   | `measure(fn\|gen\|iter, opts?)` | `Promise<stats>`                                 | state-free one-shot sample — core's basis |
|  [02]   | `do_not_optimize(v)`            | `void`                                           | DCE barrier around a measured expression  |
|  [03]   | `bench(name?, fn\|gen\|iter)`   | `B`                                              | register globally; returns the builder    |
|  [04]   | `run(opts?)`                    | `Promise<{ context: ctx; benchmarks: trial[] }>` | execute the list; return context + trials |
|  [05]   | `group`/`summary`/`compact`     | `(name?, f) => void \| Promise<void>`            | scope wrappers grouping registrations     |
|  [06]   | `boxplot`/`barplot`/`lineplot`  | `(f) => void \| Promise<void>`                   | plot-render wrappers over registrations   |
|  [07]   | `flags`                         | `{ compact: number; baseline: number }`          | render bit-flags                          |
|  [08]   | `class B`                       | the fluent benchmark builder (below)             | per-benchmark parameterization and policy |

`B` is the fluent builder `bench` returns — argument matrices, sweeps, GC policy, and render styling, each method returning `this`.

[SURFACES]: `B` `ctx` `args` `range` `dense_range` `gc` `baseline` `compact` `highlight` `name` `run`

## [04]-[COUNTERS_AND_KERNEL]

`stats.counters` is typed as `object` upstream. `Schema.decodeUnknown(HardwareCounters)` narrows the optional block before metric derivation; every counter field below carries `.avg`.

| [INDEX] | [COUNTER_PATH]              | [MEANING]                           | [DERIVED_METRIC]                        |
| :-----: | :-------------------------- | :---------------------------------- | :-------------------------------------- |
|  [01]   | `counters.cycles.avg`       | CPU cycles per iteration            | IPC denominator                         |
|  [02]   | `counters.instructions.avg` | retired instructions per iteration  | IPC numerator (`instructions / cycles`) |
|  [03]   | `counters.cache.avg`        | cache references per iteration      | cache-hit-rate denominator              |
|  [04]   | `counters.cache.misses.avg` | cache misses per iteration          | miss-rate numerator                     |
|  [05]   | `counters._bmispred.avg`    | branch mispredictions per iteration | branch-prediction quality band          |

Deep `src/lib.mjs` carries the state-free primitives the main entry does not re-export — reach them by explicit path when a caller needs raw measurement without the registration global:

- `measure(fn|gen|iter, opts?)`, `fn`, `generator`, `iter` — the modality-specific one-shot samplers, all returning `Promise<stats>`.
- `gc()`, `now()`, `print(line)`, `do_not_optimize(v)`, `kind(fn)` — the GC trigger, monotonic clock, writer, DCE barrier, and modality classifier.
- `k_min_samples`/`k_max_samples`/`k_min_cpu_time`/`k_warmup_samples`/`k_batch_samples`/`k_concurrency`/`k_batch_unroll`/`k_warmup_threshold`/`k_batch_threshold`/`k_samples_threshold` — the sampling-policy defaults `k_options` overrides.
- `k_options` — per-measure knob bag: `now`/`heap` clock overrides, `gc`/`inner_gc`, `concurrency`, and the sample/batch/warmup/cpu-time bounds; a claim records which non-default knobs shaped its band.

## [05]-[STACKING]

- Stack with `interchange/codec.md#LANDING_WIRE` (the primary consumer): `BenchmarkClaimWire`'s measurement band mirrors `stats` field-for-field — the `min`/`max`/`avg` triple, the `p25`/`p50`/`p75`/`p99`/`p999` ladder, and the optional `gc`/`heap`/`counters` bands land as decoded shapes, each honestly present-or-absent. `run({ format: { json: { samples: true } } })` feeds it; the codec decodes the JSON trial, never runs the harness, and pairs the band with `HostFingerprintWire` so a claim compares only within one host print.
- Stack with `observe/board.md` bench pack: the `bench` pack trends landed claims on the boards — a percentile-ladder panel per benchmark alias, a GC-timing panel where the `gc` band is present, and an IPC/cache/branch panel where `counters` landed. Pack dispatch is the same every metric pack rides; the bench pack adds no new query kind, only claim-shaped datasource rows.
- Stack with the baseline-versus-candidate regression fold (`observe/board.md`): a pure fold over two claims sharing one `HostFingerprint` grades candidate against `baseline`-flagged claim per percentile, yielding a graded verdict a gate reads instead of eyeballed numbers; a cross-host pair yields the refusal verdict the fingerprint admission forces, never a number.
- Stack with runtime/tests as the runner owner: the registration surface (`bench`/`group`/`run`) is module-global, so it composes in the bench lane only — a benchmark file registers, calls `run`, and emits the JSON claim; core imports the `stats` TYPE, never the harness. App-neutrality holds because no library domain module touches the global list.
- Stack against tinybench (the excluded peer): the vitest `bench()` runner bundles tinybench for in-suite timing; mitata is carried beside it as the STRONGER claim primitive — tinybench has no hardware-counter or GC band, so a durable `BenchmarkClaimWire` mirrors the mitata shape, and the vitest-bundled runner stays the in-suite convenience path.

## [06]-[RAIL_LAW]

- Owns: the benchmark measurement SHAPE — the `stats` percentile ladder, GC/heap bands, and the optional CPU hardware-counter block — as the model `BenchmarkClaimWire` mirrors, and the state-free `measure` kernel a caller reaches for raw sampling.
- Accept: the `stats` record as the wire measurement-band model; `run({ format: 'json' })` output as the claim-landing feed decoded by the codec; every enrichment band (`gc`/`heap`/`counters`) as honestly optional; the deep `src/lib.mjs` `measure` for state-free measurement; the observe `bench` pack and the per-fingerprint regression fold as the only claim consumers.
- Reject: composing the module-global registration surface (`bench`/`group`/`run`/plots) into library domain code — it is the runtime/tests bench lane's, and pulling it into `scope:core` breaks app-neutrality; comparing claims across differing host fingerprints (the fingerprint admission refuses it); assuming `counters`/`gc`/`heap` present when the addon or runtime capability is absent; re-deriving the band shape by hand where the `stats` record already types it.
- Boundary: pure ESM, zero deps; the hardware-counter block is a dynamically-imported optional native addon, so the counter band is a runtime-gated capability the wire decodes as optional. Core carries the typed shape; runtime/tests carries the runner; the wire and the boards carry the landed claims.
