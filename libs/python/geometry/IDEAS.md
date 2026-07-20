# [PY_GEOMETRY_IDEAS]

Forward pool of higher-order concepts for `geometry`, grounded in the host-free companion role. Each idea is a card — slug leader with the capability, what it unlocks, and the gap or technique it draws on — and spawns one or more tasks in `TASKLOG.md`. `[1]-[OPEN]` holds live concepts; `[2]-[CLOSED]` records dispositions so an idea is never re-litigated.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
- Ripple: <origin/counterpart card this entry pairs with across folders, as `pkg` `[SLUG]`; present only on a cross-folder ripple counterpart card>.
-->

[KERNEL_BENCH_FAMILY]-[QUEUED]: mesh-kernel macro-benchmarks through the runtime bench runner.
- Capability: tessellation, repair, and boolean kernels gain repeatable warmup-disciplined latency and throughput benchmarks beside the per-call `rasm.geometry.evidence.duration` histogram.
- Shape: one bench lane per kernel subject riding the runtime `Bench.run` latency/throughput modes; receipts stream through the standing contributor fold; a process-terminal bench run rides the runtime job envelope so the final projection flushes before exit.
- Unlocks: kernel-regression evidence across mesh library upgrades without a C# harness round-trip.
- Anchors: runtime `observability/profiles#BENCH` `Bench.run`/`BenchmarkReceipt`; the graduation `EvidenceScope` vocabulary keying subjects; the bench growth law absorbing new subjects with zero runtime edits.
- Tension: HOSTILE worker kernels bench through their entry seam, never in-kernel — the same boundary the mid-operation pulse card records.

[EVIDENCE_TRACE_LINKS]-[BLOCKED]: cross-producer trace links over the content-keyed crossings.
- Capability: `Span.add_link` joins a consumer span — deviation's reference-GLB read, reconstruction's transform compose — to the upstream producer's trace.
- Shape: a `SpanContext` field beside the `ContentKey` on `GeometryHandoff.wire()`, folded as a `Link` at `evidence_run` span open.
- Unlocks: backend metric-to-trace-to-upstream-trace click-through across the scan plane's producer chain.
- Anchors: the `evidence_run` weave on the graduation spine; `Link`/`Span.add_link` on the branch `opentelemetry-api` catalogue.
- Tension: the crossing is content-keyed data carrying no trace identity; widening `wire()` is a frozen-wire change the compute decode must co-ship.

[PRODUCER_DISTRIBUTIONS]-[BLOCKED]: per-producer geometry distributions beside the one evidence-duration instrument.
- Capability: deviation magnitude, mesh genus/aspect, and EUI cross from receipt-only facts to backend histograms.
- Shape: one runtime `INSTRUMENTS` row per measure and one `Metrics.record` mapping-arm call at the producing fold — no second projection rail.
- Unlocks: geometry-domain dashboards aggregate distributions without receipt post-processing.
- Anchors: the `rasm.geometry.evidence.duration` row and the `_DOMAIN_SLOT` mapping arm on the runtime metrics owner.
- Tension: the one-instrument spine is the ruled conformant shape — receipts stay the data plane; a row lands only when a dashboard charter names its measure.

[MID_OPERATION_PULSE]-[BLOCKED]: hook-rail pulse points for the long-running producers.
- Capability: `rasm.geometry.<domain>.<point>` OBSERVE rows on the runtime `Hooks` registry stream tessellation, convergence, and simulation-phase facts mid-operation.
- Shape: one graduation-owned pulse surface firing `Hooks.fire` and `Span.add_event` on the weave's live span; producers compose it, never a per-page registry.
- Unlocks: live progress taps for the C# rail and the TS viewer without polling receipts.
- Anchors: the runtime `Hooks` registry and its telemetry taps; `Span.add_event` on the branch `opentelemetry-api` catalogue.
- Tension: every candidate loop runs inside a `KernelTrait.HOSTILE` worker kernel or inside the runtime recipe owner's own span, so no parent-side mid-operation point exists to fire from; a parent-side streaming drain is the admission gate.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

(none)
