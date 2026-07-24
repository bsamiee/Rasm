# [CROSS_LIBS_IDEAS]

Cross-language ideas span two or more of C# / Python / TypeScript at the wire and the companion/offline seams. A concept coupling packages within one language lives in that branch's idea register. `[1]-[OPEN]` holds the live concert; `[2]-[CLOSED]` records a finished or dropped idea with a one-line disposition so it is never re-litigated.

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
- Arms: the OTLP profiles signal reaches stable across the three SDK trains; the swap then executes as row replacement per the `[PROFILE_SWAP]` table.

[LAYER_TOPOLOGY_GRAPH_FACTS]-[QUEUED]: Shared `LayerTopologyFact` wire rows carry host organization into every `ElementGraph` peer.
- Capability: `LayerTopologyFact` projects `LayerStamp` identity, `LayerPath` nesting, membership, and per-viewport overrides as detached entity and containment facts, so each runtime answers layer organization without a host handle.
- Shape: C# owns the canonical wire schema and codec; `Rasm.Rhino` `Layers.Ask` emits rows, `Rasm.Element` folds them through `IElementProjection`, `Rasm.Bim` aligns them with IFC spatial structure, Python decodes them into IFC graph projections, and TypeScript decodes them into query-store relations. `ContentHash` identifies referenced geometry and payloads, while the schema's typed layer and relation keys identify graph facts.
- Unlocks: Host-organized element queries, spatial-structure round-tripping, and one organizational producer for the graph's containment axis.
- Anchors: `Rasm.Rhino` `Document/layers.md` `LayerStamp`/`LayerPath`/`Layers.Ask`; `libs/.planning/ARCHITECTURE.md` shared wire and AEC-domain strata; `Rasm.Element/.planning/Projection/projection.md`; `Rasm.Bim/.planning/Projection/semantic.md`.
- Tension: Detached values cross the wire; host handles remain inside `Rasm.Rhino`, and each peer projects the same canonical fact identity.
- Ripple: `python:data` `[LAYER_TOPOLOGY_GRAPH_FACTS]`; `typescript:data` `[LAYER_TOPOLOGY_GRAPH_FACTS]`.

[DAYLIGHTING_SCENE_DESCRIPTOR]-[QUEUED]: Owned sun astronomy, scene lights, and GLB tessellation compose one daylighting scene descriptor the Python analysis companion consumes for EnergyPlus/OpenStudio-grade solar and daylight studies.
- Capability: A content-keyed, host-free scene descriptor — sun state (`SunSolver` astronomy), photometric light roster (`LightStamp` rows with `Radiance` power and the `PhotometricWeb` distribution payload), shading geometry as GLB tessellation — emitted from the C# host and folded by the Python geometry energy owner into radiation, shading, and daylight-autonomy analyses.
- Shape: A descriptor emitter on the `Rasm.Rhino` Render/Exchange surface stacking `SunState` + `Objects/lights.md` stamps + the GLB rail over the content-keyed wire; a Python consumer on `python:geometry/energy/simulate.md` driving the machine's `energyplus`/`openstudio` engines through the runtime recipe binding; results return as wire receipts keyed by the same content identity.
- Unlocks: Closed-loop environmental analysis from the live model — solar exposure, shading studies, daylight metrics — without a host dependency in the analysis runtime, and a reusable scene-descriptor vocabulary for any future physics consumer.
- Anchors: `Rasm.Rhino` `Render/settings.md` `SunSolver.Solve`/`SunState`; `Objects/lights.md` `LightStamp`/`Radiance`; `Render/kinds.md` `PhotometricWeb`; `libs/.planning/ARCHITECTURE.md` `[04]` geometry-flow law (kernel and Python geometry meet only at the wire: content identity and the GLB tessellation rail) and `[06]` per-language roles (Python as the host-free science/compute companion).
- Tension: Descriptor schema is a new shared wire owner — C# mints it, Python decodes it, and the schema lands in the wire vocabulary before either end builds; tessellation fidelity policy remains an explicit descriptor axis.
- Ripple: `python:geometry` `[DAYLIGHTING_SCENE_DESCRIPTOR]`.

[HOST_OPLOG_CRDT_PRODUCER]-[QUEUED]: Committed host transactions become a replayable, mergeable causal op-log — the host end of the shared op-log CRDT wire owner.
- Capability: Every sealed commit folds into an `OperationId`-keyed causal log, so equal payloads remain distinct operations and cross-runtime sync, collaborative merge, and checkpoint replay become wire operations instead of file exchanges.
- Shape: `OperationId` is the shared `[ComplexValueObject]` over tenant, actor, actor sequence, and causal frontier; its canonical codec sorts the vector-clock rows before encoding. Each op-log entry carries `OperationId Id`, `ContentHash Payload`, and the sealed-commit facts. `Rasm.Rhino` `DocumentStream` and `DocumentCommit.Sealed` emit entries, while every branch decodes the same shared payload owner and replays or merges it against a checkpoint snapshot.
- Unlocks: Multi-runtime document sync, collaborative editing groundwork, deterministic replay for testing and audit, and the first live producer for the wire law's op-log owner.
- Anchors: `Rasm.Rhino` `Document/events.md` `DocumentStream.Observe`; `Document/tables.md` `DocumentCommit.Sealed`/`UndoBracket` (the one commit envelope every mutation walks — the natural single tap point); `libs/.planning/ARCHITECTURE.md` `[07]` shared-owner roster (the op-log CRDT payload and tenant/causal identity are named shared wire owners; C# owns the wire vocabulary, Python and TypeScript decode it).
- Tension: Distinct from the static archive diff (`Exchange/archive.md` `ArchiveDelta`) — the op-log is causal and live, the diff is structural and at-rest; CRDT merge policy settles commutation and conflict per mutation kind without conflating operation identity with payload identity.
- Ripple: `typescript:data` `[HOST_OPLOG_CRDT_CONSUMER]`.

## [02]-[CLOSED]

<!-- source-only: closed idea card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[UNIFIED_SIGNAL_FABRIC]-[COMPLETE]: realized across the three branches — canonical wire rows at `libs/.planning/ARCHITECTURE.md` `[08]-[TELEMETRY_WIRE_LAW]` with row-identical branch transcriptions, receipt-projected instruments (`InstrumentFan`/`Metrics.record`/`Pulse`), hook rails, and the iac-compiled Grafana backend landed at their anchors; the `typescript:data` `[OBJECT_PLANE_INSTRUMENT_PROJECTION]` ripple closed at its owner; one registry MECHANISM per branch homes at the branch's lowest stratum, and per-folder fact unions are the only legitimate plurality.
[FLEET_TELEMETRY_SCALE_ROWS]-[COMPLETE]: escalation family named with arming coordinates at `ARCHITECTURE.md` `[FLEET_ESCALATION]`; every row stays OFF at estate scale by ruling, so fleet pressure flips a coordinate instead of re-deriving the design.
[COST_ATTRIBUTION_BAGGAGE]-[COMPLETE]: tenant join realized at `ARCHITECTURE.md` `[TENANT_COST_JOIN]` — the `rasm.tenant` dimension row-identical across runtimes after the python attribute-key repair, spend vectors projected through the instrument fan, cost boards joined at the OpenCost row and tenant organizations.
