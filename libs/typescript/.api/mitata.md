# [TS_BRANCH_API_MITATA]

`mitata` mints the benchmark measurement shape — the `stats` percentile ladder, the optional `gc`/`heap` timing bands, and the addon-gated CPU hardware-counter block `measure` returns per run — and the state-free `measure` kernel a caller reaches for one raw sample.

`bench`/`group`/`run` and the plot wrappers mutate a module-global registration list and render a report. The split is the branch's admission line: the measurement SHAPE derives off `measure` and lands in the interchange claim family, while the global registration surface is fenced to the bench lane the runtime and proof estates own.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: shapes `measure` and `run` return, every one declared WITHOUT `export` — timing fields nanoseconds, the percentile ladder the regression basis, `gc`/`heap`/`counters` conditionally-present enrichment. Every consumer names one by deriving off the member returning it, never by importing the name.

| [INDEX] | [SYMBOL] | [TYPE_FAMILY]        | [CAPABILITY]                          |
| :-----: | :------- | :------------------- | :------------------------------------ |
|  [01]   | `stats`  | unexported interface | timing ladder plus optional bands     |
|  [02]   | `Run`    | unexported union     | per-run success-or-error result       |
|  [03]   | `trial`  | unexported interface | one benchmark's runs and render style |
|  [04]   | `ctx`    | unexported interface | run-scoped host and baseline print    |

[DERIVATION]: `stats = Awaited<ReturnType<typeof measure>>` `trial = Awaited<ReturnType<B["run"]>>` `Run = trial["runs"][number]` `ctx = Awaited<ReturnType<typeof run>>["context"]`

[STATS]: `stats.min: number` `stats.max: number` `stats.avg: number` `stats.p25: number` `stats.p50: number` `stats.p75: number` `stats.p99: number` `stats.p999: number` `stats.samples: number[]` `stats.ticks: number` `stats.kind: 'fn'|'iter'|'yield'` `stats.debug: string` `stats.gc?: {avg;min;max;total}` `stats.heap?: {_;avg;min;max;total}` `stats.counters?: object`
[STATS_DIVERGENCE]: the declaration understates the runtime object on two axes — `heap` carries a `_` field the interface omits, the count of samples whose delta was non-negative and the denominator `avg` divides by, so a run the collector interleaved throughout returns `_: 0` beside `Infinity`/`-Infinity`/`NaN`; the rung ladder is read off SORTED and tail-trimmed samples, and `debug` is the generated sampler source rather than a label.
[TRIAL]: `trial.runs: Run[]` `trial.alias: string` `trial.group` `trial.baseline: boolean` `trial.args: Record<string,any[]>` `trial.kind: 'args'|'static'|'multi-args'` `trial.style: {compact:boolean;highlight:false|string}` — `group` rides the runtime object and the declaration omits it.
[RUN]: `Run = ({stats;error:undefined}|{stats:undefined;error})&{name:string;args:Record<string,any>}`
[CONTEXT]: `ctx.now` `ctx.arch` `ctx.version` `ctx.runtime` `ctx.cpu: {name;freq}` `ctx.noop: {fn;iter;fn_gc}` — the host fingerprint a claim compares within; the declaration omits `version` and the `fn_gc` baseline, and `arch` is the platform string (`arm64-darwin`) the render layer switches its counter block on.
[COUNTERS]: the band is PLATFORM-FORKED and the two shapes share only two leaves — linux answers `cycles.avg` `instructions.avg` `cache.avg` `cache.misses.avg` `_bmispred.avg`, darwin answers `cycles.avg` `cycles.stalls.avg` `instructions.avg` `instructions.loads_and_stores.avg` `l1.miss_loads.avg` `l1.miss_stores.avg`. A reader keyed on one shape silently drops every leaf the other names, so a path table covering the linux five yields two leaves on darwin.

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the module-global registration/render surface the bench lane composes; `measure` and `do_not_optimize` cross into state-free use, and the deep `src/lib.mjs` kernel carries the primitives `main.mjs` does not re-export.

| [INDEX] | [SURFACE]                                              | [SHAPE]  | [CAPABILITY]                    |
| :-----: | :----------------------------------------------------- | :------- | :------------------------------ |
|  [01]   | `measure(fn\|gen\|iter, opts?) -> Promise<stats>`      | static   | state-free one-shot sample      |
|  [02]   | `do_not_optimize(v)`                                   | static   | DCE barrier                     |
|  [03]   | `bench(name?, fn\|gen\|iter) -> B`                     | static   | global registration builder     |
|  [04]   | `run(opts?) -> Promise<{layout, context, benchmarks}>` | static   | list execution returning trials |
|  [05]   | `group\|summary\|compact(name?, f)`                    | static   | registration scope wrappers     |
|  [06]   | `boxplot\|barplot\|lineplot(f)`                        | static   | plot-render wrappers            |
|  [07]   | `flags`                                                | property | `{compact, baseline}` bit-flags |
|  [08]   | `B`                                                    | class    | fluent per-benchmark builder    |

`B` chains argument matrices, sweeps, GC policy, and render styling, each method returning `this`.

[B_METHODS]: `args(values[])` `args(name, values[])` `args(map)` `range(name, s, e, mult?)` `dense_range(name, s, e, acc?)` `gc('once'|'inner'|bool)` `baseline(bool?)` `compact(bool?)` `highlight(color?)` `name(name, color?)` `run(throw?) -> Promise<trial>`
[SAMPLERS]: `measure(fn\|gen\|iter, opts?)` `fn(fn, opts?)` `generator(gen, opts?)` `iter(iter, opts?)` — modality-specific `src/lib.mjs` samplers, each `-> Promise<stats>`; `measure` dispatches on `kind` and the root re-exports that same function, so the deep and root `measure` differ in DECLARATION alone, the kernel's `stats` omitting the `counters` field the root's declares.
[MODALITY_BANDS]: `fn` accumulates `gc`, `heap`, and `counters` inside its generated loop and `generator` inherits all three by delegating to it, while the `iter` sampler's result literal carries NO band field under any knob — band availability is a modality fact, not an option. A generator's first yield can also veto `heap` or `counters` and set `concurrency` for its own run.
[KERNEL_UTIL]: `gc()` `now()` `print(line)` `do_not_optimize(v)` `kind(fn)` — GC trigger, monotonic clock, writer, DCE barrier, modality classifier. `gc` resolves the first available host hook and otherwise returns a `fallback`-marked allocation of a gigabyte, which still runs under `inner_gc` and still costs while the band it would fill is suppressed.
[SAMPLING_KNOBS]: `k_options` per-measure knob bag — `now` the clock override and `heap` a BYTE READER (not a clock), `gc`/`inner_gc`, `concurrency`, and the sample/batch/warmup/cpu-time bounds — overriding the exported defaults `k_min_samples` `k_max_samples` `k_min_cpu_time` `k_warmup_samples` `k_batch_samples` `k_concurrency` `k_batch_unroll` `k_warmup_threshold` `k_batch_threshold` `k_samples_threshold`. A supplied `heap` reader rescales the cpu-time budget and `inner_gc` doubles it.
[INTERNAL_KNOBS]: `$counters` `params` `manual` `args` ride the same bag undeclared — `$counters` takes the addon module namespace and is the ONLY route by which the counter block reaches a `measure` call, since the registration builder is what supplies it and the defaulting pass otherwise pins it false. Every counter call in the generated loop sits inside the engine's own try and the whole block drops when init fails, so a renamed knob degrades to an absent band and never to a wrong number.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `measure` returns a pure `stats` record; the registration/render layer mutates a module-global list, so a domain folder composes the measurement shape alone.
- Every enrichment band states a handle, and a bare `measure` supplies none of the three: the defaulting pass pins the heap reader null and the counters knob false, so `heap` and `counters` are structurally unreachable until a caller passes them, and `gc` needs `inner_gc` set BESIDE a non-fallback collector (`node --expose-gc`, bun). Counters need the `@mitata/counters` addon on darwin or linux, where darwin refuses outright below root and linux defers the same refusal to the addon's own load. Timing fields carry nanoseconds, `heap` bytes.
- A present band is not a measured band: the heap accumulator admits only non-negative deltas and divides by their count, so an interleaved collector yields a structurally present band holding `Infinity`, `-Infinity`, and `NaN`. A consumer declaring finite band values strips that shape rather than publishing it.

[STACKING]:
- `interchange/codec`(`core/.planning/interchange/codec.md`, the benchmark claim family): `run({ format: { json: { samples: true } } })` feeds the codec, which folds each `stats` rung into the claim band's measured-rung map and pairs the document with the host fingerprint so a claim compares within one host print; the package computes neither `p95` nor a standard deviation, so those two rungs stay unmeasured on every mitata-minted claim while a .NET-side sweep fills them.
- `observe/board`(bench pack): trends landed claims through a percentile-ladder panel per alias, a GC-timing panel where `gc` is present, and an IPC/cache/branch panel over the counter block; claim-shaped rows ride the shared pack dispatch.
- `observe/board`(regression fold): a pure fold grades a candidate against the `baseline`-flagged claim on the one rung its tolerance names, yielding a gate-read verdict; a cross-fingerprint pair and an unmeasured rung each yield the refusal verdict.
- `runtime/proc/exec`(`runtime/.planning/proc/exec.md`, the trial owner): routes a caller's effect through the deep kernel's zero-arity overload, declares the whole `k_options` bag off the exported `k_*` anchors, supplies the heap reader and the addon handle the enrichment bands need, and folds the returned `stats` through the claim owner's own mitata seam — the state-free half of the split, with registration and render left to the bench lane.

[LOCAL_ADMISSION]:
- A domain folder reaches `measure` through the deep `src/lib.mjs` specifier and derives every shape off its own signature; the root specifier loads the registration list, so importing it into domain code is what breaks the fence, not merely using the surface behind it.
