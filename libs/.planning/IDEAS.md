# [CROSS_LIBS_IDEAS]

Cross-language ideas span two or more branch estates through a language-neutral contract. Single-branch concepts live in that branch's register.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis — the capability outcome, never the landing motion>.
- Capability: <the higher-order invariant, owner capability, or concept established — altitude only, never a page path, row list, or member spelling>.
- Shape: <where the work lands and at what grain — repo-relative page with section/row, or a new-page path; the concrete surface, so Capability never names it>.
- Unlocks: <the downstream capability at the consumer grain — a task narrows its parent idea's Unlocks to THIS slice as `IDEAS.md [SLUG] — consequence`; a set-completion card states the completeness bar that is its acceptance contract>.
- Anchors: <owners, seams, packages, catalogs, doctrines, and techniques making the work plausible — anchors, never procedures>.
- Arms: <BLOCKED or gated cards only; the exact observable flipping it actionable — catalog row landing, member query evidence, package admitted>.
- Route: <present only on a probe, research, or member-pin card; the ordered verification path run before any fence lands>.
- Tension: <only when an unresolved constraint, boundary, or bet shapes the work — the genuine bet, never the arming condition Arms carries>.
- Ripple: <counterpart — cross-folder `pkg` `[SLUG]`, same-folder prerequisite `[SLUG]`; load-bearing build order prefixes follows/precedes/mirrors>.
Capability, Shape, Unlocks, and Anchors are required on every open card; statuses closed — `ACTIVE|QUEUED|BLOCKED` open, `COMPLETE|DROPPED` closed; IDs are SEMANTIC UPPERCASE_SNAKE slugs carrying meaning — never numeric (`[0007]`-class NNNN IDs are a defect), for cards AND research tokens alike; a hyphenated slug anywhere is a defect; repo-relative paths only. Design pages carry the terminal `[RESEARCH]` section always — `(none)` marks empty, absence is an error. Ideas state higher-order concepts, never landing-grain tasks.
-->

[PROFILE_SIGNAL_OTLP]-[BLOCKED]: Continuous profiles migrate from vendor push onto the OTLP profiles signal the moment it stabilizes.
- Capability: Profiles, the fourth signal, ride the same gateway, resource identity, and scope law as traces/metrics/logs — Pyroscope push SDKs in all three runtimes retire into OTLP exporters, and profile-to-span correlation becomes wire-native.
- Shape: One exporter-row swap per runtime composition root and one collector pipeline row per the `libs/.planning/ARCHITECTURE.md` `[PROFILE_SWAP]` table; the span-profile correlation processors and dashboards survive unchanged.
- Unlocks: Vendor-neutral profiling, one ingress for all four signals, and profile exemplar links alongside the metric-trace jumps.
- Anchors: `csharp` Pyroscope span-profile correlation; `python:runtime/observability/profiles.md`; `typescript:runtime/otel/profile.md`; `typescript:iac/operate/observe.md` Pyroscope row; the collector gateway.
- Arms: an SDK train ships a profiles provider and an OTLP profiles exporter. Python's train proves the whole signal's stage today — `opentelemetry.proto.profiles.v1development.profiles_pb2` and its collector service resolve, while `opentelemetry.sdk` carries `trace`, `metrics`, and `_logs` and no profiles module at all, and neither the HTTP nor the gRPC exporter package publishes a profile exporter beside its trace, metric, and log ones. That `v1development` package segment turning `v1` beside a landed SDK module IS the observable; the swap then executes as row replacement per the `[PROFILE_SWAP]` table.

[LAYER_TOPOLOGY_GRAPH_FACTS]-[QUEUED]: Shared `LayerTopologyFact` wire rows carry host organization into every `ElementGraph` peer.
- Capability: `LayerTopologyFact` projects `LayerStamp` identity, `LayerPath` nesting, membership, and per-viewport overrides as detached entity and containment facts, so each runtime answers layer organization without a host handle.
- Shape: one `tests/contracts/MANIFEST.md` entry owns the schema and the fact identity; each branch projects it through generated local bindings under `libs/.planning/ARCHITECTURE.md` `[07]-[CROSS_LANGUAGE_WIRE]`.
- Unlocks: Host-organized element queries, spatial-structure round-tripping, and one organizational producer for the graph's containment axis.
- Anchors: `Rasm.Rhino` `Document/layers.md` `LayerStamp`/`LayerPath`/`Layers.Ask`; `libs/csharp/.planning/ARCHITECTURE.md` `[02]-[STRATA]` beside `libs/.planning/ARCHITECTURE.md` `[07]-[CROSS_LANGUAGE_WIRE]`; `Rasm.Element/.planning/Projection/projection.md`; `Rasm.Bim/.planning/Projection/semantic.md`.
- Tension: Detached values cross the wire; host handles remain inside `Rasm.Rhino`, and each peer projects the same canonical fact identity.
- Ripple: `python:data` `[LAYER_TOPOLOGY_GRAPH_FACTS]`; `typescript:data` `[LAYER_TOPOLOGY_GRAPH_FACTS]`.

[DAYLIGHTING_SCENE_DESCRIPTOR]-[QUEUED]: Owned sun astronomy, scene lights, and GLB tessellation compose one daylighting scene descriptor the Python geometry analysis owner consumes for EnergyPlus/OpenStudio-grade solar and daylight studies.
- Capability: One content-keyed, host-free scene descriptor — sun state (`SunSolver` astronomy), photometric light roster (`LightStamp` rows with `Radiance` power and the `PhotometricWeb` distribution payload), shading geometry as GLB tessellation — emitted by the Rhino-aware capture owner and folded by the Python geometry energy owner into radiation, shading, and daylight-autonomy analyses.
- Shape: One descriptor emitter on the `Rasm.Rhino` Render/Exchange surface stacking `SunState` + `Objects/lights.md` stamps + the GLB rail over the content-keyed wire; a Python consumer on `python:geometry/energy/simulate.md` driving the machine's `energyplus`/`openstudio` engines through the runtime recipe binding; results return as wire receipts keyed by the same content identity.
- Unlocks: Closed-loop environmental analysis from the live model — solar exposure, shading studies, daylight metrics — without a host dependency in the analysis runtime, and a reusable scene-descriptor vocabulary for any future physics consumer.
- Anchors: `Rasm.Rhino` rendering owners; `libs/.planning/ARCHITECTURE.md` `[04]-[GEOMETRY_FLOW]`; the GLB tessellation rail at `tests/contracts/MANIFEST.md` `GLB_BY_KEY`; `python:geometry` host-free analysis.
- Tension: the descriptor needs one neutral schema; the capture producer and the Python analysis owner bind it independently.
- Ripple: `python:geometry` `[DAYLIGHTING_SCENE_DESCRIPTOR]`.

[HOST_OPLOG_CRDT_PRODUCER]-[QUEUED]: Committed host transactions become a replayable, mergeable causal op-log — the host end of the shared op-log CRDT wire owner.
- Capability: Every sealed commit folds into an `OperationId`-keyed causal log, so equal payloads remain distinct operations and cross-runtime sync, collaborative merge, and checkpoint replay become wire operations instead of file exchanges.
- Shape: `tests/contracts/MANIFEST.md` `CRDT_OP_SET` owns operation identity, canonical ordering, and payload identity; each branch emits and replays through its own named mint.
- Unlocks: Multi-runtime document sync, collaborative editing groundwork, deterministic replay for testing and audit, and the first live producer for the wire law's op-log owner.
- Anchors: `Rasm.Rhino` document events; the `CRDT_OP_SET` minters `csharp:Rasm.Persistence/Version/commits#CRDT_ALGEBRA`, `python:runtime/transport/wire#CRDT_DECODE`, `typescript:core/state/merge#INSTANCE_ROSTER`.
- Tension: Distinct from the static archive diff (`Exchange/archive.md` `ArchiveDelta`) — the op-log is causal and live, the diff is structural and at-rest; CRDT merge policy settles commutation and conflict per mutation kind without conflating operation identity with payload identity.
- Ripple: `typescript:data` `[HOST_OPLOG_CRDT_CONSUMER]`.

[GENERATION_RECOVERY_CONTRACT]-[QUEUED]: Backend-generation recovery becomes contract law every branch mints, not a per-branch runbook.
- Capability: a restored store proves which generation it carries and whether that generation is still admissible, so point-in-time recovery, replica promotion, and a rebuilt embedded store all land on one verdict; the recovery objective each host profile declares gauges the measured window against the contract rather than a branch-local table.
- Shape: recovery rows on the `BACKEND_CONTRACT` corpus schema with their per-branch mints at the three contract owners, and the measured-window comparison at each branch's recovery owner.
- Unlocks: a recovered store admits or refuses on evidence in every branch, and a polyglot application restores its merged generation without one branch's runbook standing in for the law.
- Anchors: `tests/contracts/MANIFEST.md` `BACKEND_CONTRACT` and its three minters `csharp:Rasm.Persistence/Store/schema#IDENTITY`, `python:runtime/execution/admission#BACKEND_CONTRACT`, `typescript:data/lane/capability#CONTRACT`; `csharp:Rasm.Persistence/Version/recovery` PITR choreography; `csharp:Rasm.AppHost/Runtime/profiles` `RecoveryObjective` columns.
- Tension: recovery evidence is time-shaped where the generation is content-shaped — a store restored to an earlier instant carries a valid generation whose data frontier lags it, so the verdict must separate contract identity from data recency without minting a second generation notion.
- Ripple: `python:runtime` `[GENERATION_RECOVERY_CONTRACT]`; `typescript:data` `[GENERATION_RECOVERY_CONTRACT]`.

## [02]-[CLOSED]

<!-- source-only: closed idea card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[TIER0_STRATA_DELAMINATION]-[COMPLETE]: Tier-0 `[01]-[STRATA]` now states the branch-agnostic stratification law and `[02]-[DEPENDENCY_DIRECTION]` the cross-branch no-direction law; the C# roster, its charters, and the S4 app-shell rank seat at `libs/csharp/.planning/ARCHITECTURE.md` `[02]-[STRATA]`, and the reviewer configs, workflow lanes, and routers resolve the branch table live.
[UNIFIED_SIGNAL_FABRIC]-[COMPLETE]: reopened when the disk audit refuted the fabric — the collector endpoint every workload bound resolved to nothing, fifteen python producer measures hit a producer-killing census lookup, and six C# pages failed compilation against their kernel owner; re-closed against the landed campaign: canonical rows at `[08]-[OBSERVABILITY_CONFORMANCE]` with the hook and evidence-residence planes, receipt-projected instruments (`InstrumentFan`/`Metrics.record`/`Pulse`), the census-gated `INSTRUMENTS` fill, and the iac backend whose endpoint derives from `_urls`.
[FLEET_TELEMETRY_SCALE_ROWS]-[COMPLETE]: escalation family named with arming coordinates at `ARCHITECTURE.md` `[FLEET_ESCALATION]`; every row stays OFF at estate scale by ruling, so fleet pressure flips a coordinate instead of re-deriving the design.
[COST_ATTRIBUTION_BAGGAGE]-[COMPLETE]: tenant join realized at `ARCHITECTURE.md` `[TENANT_COST_JOIN]` — the `rasm.tenant` dimension row-identical across runtimes after the python attribute-key repair, spend vectors projected through the instrument fan, cost boards joined at the OpenCost row and tenant organizations.
