# [PY_ARTIFACTS_IDEAS]

Forward pool of the folder's higher-order concepts, each grounded in artifact production and the host-free companion charter. Open ideas are cards in `[1]-[OPEN]`; a finished or dropped idea moves to `[2]-[CLOSED]` with a one-line disposition. Each idea drives one or more `TASKLOG.md` tasks.

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

[PRODUCTION-HOOKS]-[QUEUED]: Production-fact hook rail over the issue lifecycle and receipt fold, composed from the runtime `Hooks` registry.
- Capability: veto/observe/replay taps on artifact production — issue admitted, planned, refused, front drained, producer emitted, transmittal issued — with subscriber-fault isolation onto `BoundaryFault` and telemetry-as-tap, so observability subscribes to production facts and no emit-call lands in producer code.
- Shape: one new page `libs/python/artifacts/.planning/core/hooks.md` registering the `rasm.artifacts.<domain>.<point>` `HookPoint` rows with closed msgspec payload Structs projected from receipts, its fire seams threaded into `libs/python/artifacts/.planning/core/issue.md` and `libs/python/artifacts/.planning/core/receipt.md`; no artifacts-local registry, the point table an app composes per instance.
- Unlocks: an app vetoes an issue pre-drain, audits every emitted receipt, and replays the last drain facts without touching a producer page; two apps compose disjoint subscriber sets over the same points without collision.
- Anchors: runtime `observability/hooks.md` `HookPoint`/`Hooks`/`Modality`; `core/issue.md` `_nodes`/`_driven`; `core/receipt.md` `contribute` fold; the runtime `serve.md` `LIFECYCLE_POINTS` row pattern.
- Tension: hook payloads project FROM receipts, never re-narrate them — the fired fact is the receipt projection, and a parallel fact family is the defect the tap law exists to prevent.

[ISSUE-ATTRIBUTION]-[QUEUED]: Tenant and issue baggage spine attributing every producer signal to its paying issue.
- Capability: `rasm.tenant` and issue-scope baggage bound once at `ArtifactIssue.issue`, carried across the lane and worker crossings under the W3C composite, landing as dimensions on the `Metrics.record` fold and keys on `bind_contextvars` structured logs, so per-tenant production cost — bytes, renders, elision hits — slices off the one metric stream.
- Shape: baggage bind and context-carry rows on `libs/python/artifacts/.planning/core/issue.md`, attribution dimensions on the `_METRIC` projection and span policy in `libs/python/artifacts/.planning/core/receipt.md`.
- Unlocks: the folder arm of the cross-libs `[COST_ATTRIBUTION_BAGGAGE]` card — billing-grade attribution with zero artifacts-local metric state and zero producer edits.
- Anchors: `opentelemetry-api` `baggage.set_baggage(name, value, context)` and `CompositePropagator(propagators)`; `structlog` `bind_contextvars(**kw)`; `core/receipt.md` `[METRIC_SIGNALS]`/`[SPAN_POLICY]` signal rows; the runtime lane crossing.

[TRANSMITTAL-NOTICE]-[QUEUED]: CloudEvents transmittal notice sealing the ISO 19650 issue as a wire-ready envelope.
- Capability: delivery close emits one content-keyed, trace-continuous CloudEvents notice — structured JSON binding, distributed-tracing extension, container digests, register row references — an app transports over any carrier, so downstream systems ingest an issue-for-construction event without opening the archive.
- Shape: one new page `libs/python/artifacts/.planning/delivery/notice.md` owning `TransmittalNotice`; `libs/python/artifacts/.planning/delivery/transmittal.md` folds the notice as a terminal member of the issue closure.
- Unlocks: trace continuity from production spans into every notice consumer; the delivery boundary's transports-axis realization without a broker client.
- Anchors: `cloudevents` structured/binary bindings and distributed-tracing extension; `opentelemetry-api` `propagate.inject(carrier, context, setter)` under the composite propagator; `Transmittal`; the `SignedArtifact` seam to Rasm.Persistence.
- Tension: python holds no broker clients by ruled asymmetry — the notice ends at envelope bytes, and transport belongs to the composing app.

[QUALITY-GATE]-[QUEUED]: Graded artifact quality gate the transmittal refuses on.
- Capability: one `QualityGate` folding raster measured scores, PDF/UA and PDF/X preflight verdicts, PAdES conformance verdicts, and lens extraction audits into one graded per-artifact verdict, per-kind thresholds carried as policy rows an office tunes without code.
- Shape: one new page `libs/python/artifacts/.planning/delivery/gate.md` owning the verdict fold and threshold policy; `libs/python/artifacts/.planning/delivery/transmittal.md` gains the refusal seam; `libs/python/artifacts/.planning/document/lens.md` gains the `Page.debug_tablefinder` extraction-audit overlay feeding the gate.
- Unlocks: an issue that cannot ship a failing sheet; one graded quality surface over the folder's scattered verdict producers.
- Anchors: `graphic/raster/measure.md` perceptual scores; `document/tagged.md` `UaCheck` and preflight; `exchange/conformance.md` `ConformanceVerdict`; `pdfplumber` `Page.debug_tablefinder`; the data `QualityProfile` seam.

[DASHBOARD-ARTIFACT]-[QUEUED]: Self-contained interactive HTML dashboard as a first-class artifact kind.
- Capability: multi-pane chart, table, and diagram composition into one offline single-file HTML document — filterable charts, sortable schedule tables, register indexes — produced host-free and theme-graded, never a live UI.
- Shape: one new page `libs/python/artifacts/.planning/visualization/dashboard.md` owning `DashboardPlan`; `libs/python/artifacts/.planning/visualization/chart/export.md` gains the interactive-HTML format row.
- Unlocks: receipts, QTO, and delivery registers readable as one shareable file; the folder's dashboards-axis answer inside the no-UI charter.
- Anchors: `altair` `JupyterChart` and HTML export; `vl-convert-python`; `TablePlan` HTML arm; `DiagramDraw` layered SVG; `Theme` rows.

[PRODUCER-BENCH]-[QUEUED]: Artifact-producer benchmark corpus riding the runtime bench tier.
- Capability: per-`ArtifactKind` bench subjects — emit, render, compress, shape — run through `Bench.run` with `BenchmarkReceipt` evidence and regression thresholds concentrated on the native-offload class.
- Shape: one new page `libs/python/artifacts/.planning/core/bench.md` owning the `BenchCorpus` subject rows, deterministic-input recipes, and threshold policy; the corpus consumes the runtime bench tier and mints no timing of its own.
- Unlocks: a producer regression surfaces as a graded bench verdict before an office notices slow sheet sets.
- Anchors: runtime `observability/profiles.md` `Bench.run`/`BenchmarkReceipt`/`BenchMode`; `core/receipt.md` signals `[SPAN_CLASS]` native-offload set (`typography/layout`, `typography/shape`, `visualization/chart/export`); `media/synthesis.md` deterministic test-signal inputs.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

[DIAGRAM-DXF]-[COMPLETE]: realized as `export/dxf`'s `Diagram` arm — `DiagramLower` lowers the positioned glyph sequence to `DxfEntity` cases under `Standard.seed` regime-pen layers, glyphset owning the shared lowering derivations (`mark`, `Port.seat`, `AreaMark.centroid`, `ER_CAPS`, `ENTITY_BAND`); draw stays two-arm.

[WTPDF-AUDIT]-[COMPLETE]: landed as `UaCheck.WTPDF_ACCESSIBILITY`/`WTPDF_REUSE` clause rows on `document/tagged#ACCESS` — `pdfd:conformsTo` spellings confirmed at the PDF Association source (erratum-canonical `wtpdf#accessibility1.0`/`wtpdf#reuse1.0` with the as-published `/#` forms), read off the raw `/Metadata` packet because the pikepdf mapping view cannot decode the declarations bag.
