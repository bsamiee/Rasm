# [CROSS_LIBS_TASKLOG]

Open and closed cross-language tasks — the wire seams that span two or more of C# / Python / TypeScript. Per-language and per-folder work lives in the branch and folder `TASKLOG.md`; this node carries only the seam each branch consumes at the boundary, never a re-aggregation of branch work. Each task names its producer and consumer touchpoints in `lang:pkg/page#CLUSTER` notation with the considerations that scope it; a closed task compacts to one or two lines.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with. `Atomic` flags a minor-scope task so a later session sizes its turn correctly and does not overscope a batch of small items.

## [01]-[OPEN]

<!-- source-only: open task card template:
[ID]-[STATUS]: <ambitious concise thesis — the capability outcome, never the landing motion>.
- Capability: <the higher-order invariant, owner capability, or concept established — altitude only, never a page path, row list, or member spelling>.
- Shape: <where the work lands and at what grain — repo-relative page with section/row, or a new-page path; the concrete surface, so Capability never names it>.
- Unlocks: <the downstream capability at the consumer grain — a task narrows its parent idea's Unlocks to THIS slice as `IDEAS.md [SLUG] — consequence`; a set-completion card states the completeness bar that is its acceptance contract>.
- Anchors: <owners, seams, packages, catalogs, doctrines, and techniques making the work plausible — anchors, never procedures>.
- Arms: <BLOCKED or gated cards only; the exact observable flipping it actionable — catalog row landing, member query evidence, package admitted>.
- Route: <present only on a probe, research, or member-pin card; the ordered verification path run before any fence lands>.
- Tension: <only when an unresolved constraint, boundary, or bet shapes the work — the genuine bet, never the arming condition Arms carries>.
- Ripple: <counterpart — cross-folder `pkg` `[SLUG]`, same-folder prerequisite `[SLUG]`; load-bearing build order prefixes follows/precedes/mirrors>.
- Atomic: <present only on a minor-scope task; names the small unit so a later session sizes its turn>.
Capability, Shape, Unlocks, and Anchors are required on every open card, Atomic included; statuses closed — `ACTIVE|QUEUED|BLOCKED` open, `COMPLETE|DROPPED` closed; IDs are SEMANTIC UPPERCASE_SNAKE slugs carrying meaning — never numeric (`[0007]`-class NNNN IDs are a defect), for cards AND research tokens alike; a hyphenated slug anywhere is a defect; repo-relative paths only. Design pages carry the terminal `[RESEARCH]` section always — `(none)` marks empty, absence is an error. Tasks state landing-grain work decomposing an idea.
-->

[ESTATE_OTLP_BACKEND]-[BLOCKED]: A live estate backend receives the three runtimes' OTLP egress.
- Capability: Collector gateway and store set per the iac observe page models — Prometheus reference row, log and trace stores, Pyroscope, Grafana — run as estate infrastructure, so the published OTLP endpoint resolves to a live sink for every runtime.
- Shape: `typescript:iac/operate/observe.md#_stores` realizes the stack; app roots read the endpoint from stack outputs; the dev loop rides the single all-in-one row with byte-identical SDK export config.
- Unlocks: every runtime's OTLP egress resolves to a live estate sink — queryable trace, metric, log, and profile stores plus Grafana boards — so an app reads the endpoint from stack outputs and embeds no backend.
- Anchors: `typescript:iac/program/spec.md#_Observe`; `typescript:runtime/otel/emit.md#POLICY`; `python:runtime/observability/telemetry.md#TELEMETRY`; `csharp:Rasm.AppHost/Observability/telemetry.md#SIGNAL_GOVERNANCE` exporter seam.
- Arms: an estate host repo declares the container placement composing the iac `Lgtm`/`Dev` rows and the published `StackOutputs.otlp` endpoint resolves live — libs-side design is landed whole and only the deployment leg remains.
- Tension: Container placement lands in the estate host repos, never in libs; the iac package stays host-agnostic and the deployment consumes it.

[LAYER_FACT_SCHEMA]-[QUEUED]: Canonical `LayerTopologyFact` schema and codec pin at the producing end.
- Capability: `[LAYER_TOPOLOGY_GRAPH_FACTS]` decomposes into the schema pin — identity, nesting, membership, and override fields with their typed keys — owned beside the `csharp:Rasm.Rhino/Document/layers.md#TREE_SNAPSHOT` emitter before any peer decodes.
- Shape: Schema and codec rows at the C# owner; decode landings stay with the `python:data` `[LAYER_TOPOLOGY_DECODER]` and `typescript:data` `[LAYER_TOPOLOGY_GRAPH_FACTS]` counterpart cards.
- Unlocks: IDEAS.md [LAYER_TOPOLOGY_GRAPH_FACTS] — the Python and TypeScript decoders build against one pinned schema, host-organized element queries answered without a host handle.
- Anchors: `[LAYER_TOPOLOGY_GRAPH_FACTS]`; `Layers.Ask` emission; `Rasm.Element` `IElementProjection` fold.

[SCENE_DESCRIPTOR_SCHEMA]-[QUEUED]: Daylighting descriptor schema lands in the shared wire vocabulary.
- Capability: `[DAYLIGHTING_SCENE_DESCRIPTOR]` decomposes into the descriptor field pin — sun state, photometric roster, GLB shading payload, tessellation-fidelity axis — ruled once before producer or consumer builds.
- Shape: Descriptor schema rows in the wire vocabulary; producer pins on `csharp:Rasm.Rhino/Render/settings.md#SUN_ASTRONOMY` and `csharp:Rasm.Rhino/Objects/lights.md#SEED_AND_EDIT`; consumer pin on `python:geometry/energy/simulate.md#SIMULATE`.
- Unlocks: IDEAS.md [DAYLIGHTING_SCENE_DESCRIPTOR] — producer and consumer build against a settled descriptor, closing the loop on solar, shading, and daylight studies off the live model.
- Anchors: `[DAYLIGHTING_SCENE_DESCRIPTOR]`; the geometry-flow law crossing at content identity and the GLB rail.

[OPLOG_ENTRY_SCHEMA]-[QUEUED]: `OperationId` and the op-log entry schema pin at the commit envelope.
- Capability: `[HOST_OPLOG_CRDT_PRODUCER]` decomposes into the shared identity pin — the `OperationId` `[ComplexValueObject]` with sorted vector-clock encoding — and the entry schema over the sealed-commit facts, ruled before replay or merge lands anywhere.
- Shape: Identity and entry rows at the C# wire owner; the `csharp:Rasm.Rhino/Document/events.md#STREAM_OWNER` tap and the `typescript:data` `[HOST_OPLOG_CRDT_CONSUMER]` decode consume the pinned schema.
- Unlocks: IDEAS.md [HOST_OPLOG_CRDT_PRODUCER] — replay, merge, and cross-runtime sync build against one pinned identity, equal payloads staying distinct operations.
- Anchors: `[HOST_OPLOG_CRDT_PRODUCER]`; `DocumentCommit.Sealed` as the single tap point.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[WIRE_LAW_PARITY]-[COMPLETE]: canonical rows landed at `libs/.planning/ARCHITECTURE.md` `[08]-[TELEMETRY_WIRE_LAW]`; the three branch transcriptions proved row-identical with one repair — python `runtime/observability/metrics.md` `_attributed` now spells the metric attribute `rasm.tenant`.
[FLEET_ROW_ARM_MAP]-[COMPLETE]: arm-coordinate rows landed as the `architecture.md` `[FLEET_ESCALATION]` table; every row OFF at estate scale, re-arming a coordinate flip.
[TENANT_COST_JOIN]-[COMPLETE]: three-pin join landed as the `ARCHITECTURE.md` `[TENANT_COST_JOIN]` row — producer spend family, SDK-side `rasm.tenant` promotion, iac OpenCost read — cap overflow riding exemplar-sampled traces.
[PROFILE_EXPORTER_SWAP_ROWS]-[COMPLETE]: swap-point rows landed as the `ARCHITECTURE.md` `[PROFILE_SWAP]` table naming the four composition-root rows; the swap itself stays armed on `[PROFILE_SIGNAL_OTLP]`.
