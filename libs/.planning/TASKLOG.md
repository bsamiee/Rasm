# [CROSS_LIBS_TASKLOG]

Open and closed cross-language tasks — the wire seams that span two or more of C# / Python / TypeScript. Per-language and per-folder work lives in the branch and folder `TASKLOG.md`; this node carries only the seam each branch consumes at the boundary, never a re-aggregation of branch work. Each task names its producer and consumer touchpoints in `lang:pkg/page#CLUSTER` notation with the considerations that scope it; a closed task compacts to one or two lines.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with. `Atomic` flags a minor-scope task so a later session sizes its turn correctly and does not overscope a batch of small items.

## [01]-[OPEN]

<!-- source-only: open task card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
- Ripple: <origin/counterpart card this entry pairs with across folders, as `pkg` `[SLUG]`; present only on a cross-folder ripple counterpart card>.
- Atomic: <present only on a minor-scope task; one short phrase naming the small unit so a later session does not overscope its turn>.
-->

[ESTATE_OTLP_BACKEND]-[QUEUED]: A live estate backend receives the three runtimes' OTLP egress.
- Capability: Collector gateway and store set per the iac observe page models — Prometheus reference row, log and trace stores, Pyroscope, Grafana — run as estate infrastructure, so the published OTLP endpoint resolves to a live sink for every runtime.
- Shape: `typescript:iac/operate/observe.md#_stores` realizes the stack; app roots read the endpoint from stack outputs; the dev loop rides the single all-in-one row with byte-identical SDK export config.
- Anchors: `typescript:iac/program/spec.md#_Observe`; `typescript:runtime/otel/emit.md#POLICY`; `python:runtime/observability/telemetry.md#TELEMETRY`; `csharp:Rasm.AppHost/Observability/telemetry.md#SIGNAL_GOVERNANCE` exporter seam.
- Tension: Container placement lands in the estate host repos, never in libs; the iac package stays host-agnostic and the deployment consumes it.

[WIRE_LAW_PARITY]-[QUEUED]: Per-branch wire-law transcriptions prove row-identical against the canonical table.
- Capability: `[UNIFIED_SIGNAL_FABRIC]` holds only while the three transcriptions of the wire rows — resource triple, `rasm.<domain>.<measure>` naming, scope law, schema pin, propagators, exemplar filter — stay byte-equivalent in meaning; a drifted row is repaired at the owning branch page, never re-shared as a library.
- Shape: Row-by-row compare of `csharp:Rasm.AppHost/Observability/telemetry.md#TELEMETRY_IDENTITY`, `python:runtime/observability/telemetry.md#TELEMETRY`, and `typescript:runtime/otel/emit.md#POLICY` against the `libs/.planning/architecture.md` wire rows.
- Anchors: `[UNIFIED_SIGNAL_FABRIC]`; the wire-law-transcription Tension on that card.
- Atomic: three-page row compare with in-place repairs.

[FLEET_ROW_ARM_MAP]-[QUEUED]: Every fleet-escalation row names its arming coordinate.
- Capability: `[FLEET_TELEMETRY_SCALE_ROWS]` decomposes into named row-to-coordinate pins — broker-buffered collector legs, the tail-sampling gateway row, the Mimir store row, per-app agent topology — so re-arming is a coordinate flip, never a re-design.
- Shape: Arm-coordinate rows against `typescript:iac/operate/observe.md#_stores`, the collector rows, and the `csharp:Rasm.AppHost/Observability/telemetry.md#SIGNAL_GOVERNANCE` exporter seam.
- Anchors: `[FLEET_TELEMETRY_SCALE_ROWS]`; the iac mimir and collector-depth cards carrying the store end.
- Atomic: coordinate pin rows, no new surfaces.

[TENANT_COST_JOIN]-[QUEUED]: Tenant baggage joins the grant-cost vectors into one attribution read.
- Capability: `[COST_ATTRIBUTION_BAGGAGE]` decomposes into the join pins — `csharp:Rasm.AppHost/Agent/capability.md` grant/cost vectors projected with the `rasm.tenant` dimension, collector baggage promotion, and the iac cost-board compile consuming the joined series.
- Shape: Join rows naming producer instruments, baggage promotion processors, and the `typescript:iac/operate/observe.md#BOARD_APPLY` OpenCost/cost-board consumer; cardinality-cap policy stated per the origin card's Tension.
- Anchors: `[COST_ATTRIBUTION_BAGGAGE]`; `typescript:iac` `[0007]` tenant cost-attribution plane.

[PROFILE_EXPORTER_SWAP_ROWS]-[QUEUED]: Per-runtime exporter-row swap points for the OTLP profiles signal.
- Capability: `[PROFILE_SIGNAL_OTLP]` decomposes into three named composition-root swap points — the Pyroscope push rows each runtime holds today — and the collector profiles-pipeline row, so migration is row replacement with dashboards untouched.
- Shape: Swap-point rows against `csharp:Rasm.AppHost/Observability/telemetry.md#SIGNAL_GOVERNANCE`, `python:runtime/observability/profiles.md#PROFILES`, and `typescript:iac/operate/observe.md#CHART_ROWS` Pyroscope rows.
- Anchors: `[PROFILE_SIGNAL_OTLP]`.
- Tension: Rows map now; the swap arms only on profiles-signal stabilization per the origin card.

[LAYER_FACT_SCHEMA]-[QUEUED]: Canonical `LayerTopologyFact` schema and codec pin at the producing end.
- Capability: `[LAYER_TOPOLOGY_GRAPH_FACTS]` decomposes into the schema pin — identity, nesting, membership, and override fields with their typed keys — owned beside the `csharp:Rasm.Rhino/Document/layers.md#TREE_SNAPSHOT` emitter before any peer decodes.
- Shape: Schema and codec rows at the C# owner; decode landings stay with the `python:data` `[LAYER_TOPOLOGY_DECODER]` and `typescript:data` `[LAYER_TOPOLOGY_GRAPH_FACTS]` counterpart cards.
- Anchors: `[LAYER_TOPOLOGY_GRAPH_FACTS]`; `Layers.Ask` emission; `Rasm.Element` `IElementProjection` fold.

[SCENE_DESCRIPTOR_SCHEMA]-[QUEUED]: Daylighting descriptor schema lands in the shared wire vocabulary.
- Capability: `[DAYLIGHTING_SCENE_DESCRIPTOR]` decomposes into the descriptor field pin — sun state, photometric roster, GLB shading payload, tessellation-fidelity axis — ruled once before producer or consumer builds.
- Shape: Descriptor schema rows in the wire vocabulary; producer pins on `csharp:Rasm.Rhino/Render/settings.md#SUN_ASTRONOMY` and `csharp:Rasm.Rhino/Objects/lights.md#SEED_AND_EDIT`; consumer pin on `python:geometry/energy/simulate.md#SIMULATE`.
- Anchors: `[DAYLIGHTING_SCENE_DESCRIPTOR]`; the geometry-flow law crossing at content identity and the GLB rail.

[OPLOG_ENTRY_SCHEMA]-[QUEUED]: `OperationId` and the op-log entry schema pin at the commit envelope.
- Capability: `[HOST_OPLOG_CRDT_PRODUCER]` decomposes into the shared identity pin — the `OperationId` `[ComplexValueObject]` with sorted vector-clock encoding — and the entry schema over the sealed-commit facts, ruled before replay or merge lands anywhere.
- Shape: Identity and entry rows at the C# wire owner; the `csharp:Rasm.Rhino/Document/events.md#STREAM_OWNER` tap and the `typescript:data` `[HOST_OPLOG_CRDT_CONSUMER]` decode consume the pinned schema.
- Anchors: `[HOST_OPLOG_CRDT_PRODUCER]`; `DocumentCommit.Sealed` as the single tap point.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

(none)
