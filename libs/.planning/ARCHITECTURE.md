# [MONOREPO_ARCHITECTURE]

Canonical monorepo hierarchy law: the strata, the dependency direction, the universal-vs-Rhino-capture rule, the geometry/mesh/IFC flow, the `.planning/` lifecycle, the per-language roles, the cross-language wire seams, and the four-signal telemetry wire law. This Tier-0 law owns the cross-cutting topology that no single branch or package can own; it is distinct from the authoring standard (`README.md`, which owns the doc-set tiers and page grammar) and the campaign method (`campaign-method.md`, which owns the planning loop). Per-package detail — owner registries, source trees, intra-package seams — lives in each folder `ARCHITECTURE.md` and is never restated here. Tier-0 is the only tier that names a language; branches and folders consume this topology as settled vocabulary.

## [01]-[STRATA]

This repository is one tri-language AEC platform organized into strict strata. Each stratum depends only upward; each package is a genuine higher-order domain, never a weak or mini sibling. C# carries the durable host-bound source and the geometry/AEC capability; Python and TypeScript are host-free peer runtimes that consume the wire.

[KERNEL]:
- Folder(s): `Rasm`
- `Rasm` is the RhinoCommon-aware geometry/numeric kernel — planning-scoped, one `.planning/` root whose sub-domains are folder-true namespaces.
- Branch base: referenced by every higher stratum, references none.

[AEC-DOMAIN]:
- Folder(s): the lowest-AEC element seam `Rasm.Element`, then the AEC peers `Rasm.Materials`, `Rasm.Bim`, `Rasm.Fabrication`.
- `Rasm.Element` is the shared lowest-AEC sub-stratum: the canonical `ElementGraph` property graph and the contracts the peers implement.
- `ElementGraph` folds entity, objectified relationships, and typed property, quantity, material, assessment, and coverage payloads.
- `IElementProjection` and `IGraphConstraint` are the seam contracts, declared on the seam and implemented by every peer.
- `Rasm.Element` references the kernel only, owns no IFC stack and no host geometry (geometry by content hash), and re-mints nothing the kernel owns.
- AEC peers carry host-neutral capability: profiles, appearance, and construction; the BIM model with IFC exchange; portable fabrication.
- Each AEC peer depends up on `{Rasm, Rasm.Element}` and projects its foreign source onto the shared graph through an `IElementProjection`.
- Peers never reference each other.
- Peer alignment travels through seam contracts and the content-keyed wire, so each package stays fully usable in isolation.

[APP-PLATFORM]:
- Folder(s): `Rasm.AppHost`, `Rasm.Compute`, `Rasm.Persistence`, `Rasm.AppUi`
- Generic application capability: runtime spine, measured execution, durable stores, product UI.
- It composes the kernel and AEC-domain and owns no geometry, BIM semantics, or fabrication algorithms — it consumes them.
- Target: `Rasm.Generation` — the layout/generation/assembly orchestration library.
- `Rasm.Generation` turns a sited occurrence, inherited generative data, construction primitives, and bond/layout policy into kernel geometry.
- It composes the kernel's geometry operations rather than owning primitives, and depends up on the kernel, the seam, and the AEC peers.
- Live-document bake stays at the host boundary.

[HOST-BOUNDARY]:
- Folder(s): `Rasm.Rhino`, `Rasm.Grasshopper`
- Self-contained, host-bound RhinoCommon/GH2 boundaries referencing only the kernel.
- Admitted only at the app roots, never as an interior dependency of a host-neutral package.

[APP]:
- Folder pattern: `apps/<concern>` product shells.
- Compose APP-PLATFORM + HOST-BOUNDARY into product shells that declare intent, bind host edges, and emit output.

## [02]-[DEPENDENCY_DIRECTION]

Dependency is strictly upward through the strata; the graph is acyclic with the kernel at the base and the app shells at the leaf.

- `Rasm` references no sibling and is referenced by every C# stratum above it.
- `Rasm.Element` references only `Rasm` and names no IFC or provider package.
- Every AEC peer and the app-platform stores that persist or read the `ElementGraph` reference the seam as the shared lower stratum.
- AEC peers depend up on `{Rasm, Rasm.Element}` and never reference each other; alignment travels the seam contracts and the wire.
- App-platform references the kernel and the seam; it consumes the element seam and AEC-domain capability, never the reverse.
- `Rasm.Generation` follows the same direction, and nothing references it downward.
- No host-neutral package references a HOST-BOUNDARY package; `Rasm.Rhino` and `Rasm.Grasshopper` reference only `Rasm`, composed at the app roots.
- `Rasm.AppUi` is the app-platform consuming leaf, not the app composition root; host-binding composition lives at the APP stratum.
- `Rasm.AppUi` references only `{Rasm, Rasm.AppHost, Rasm.Compute, Rasm.Persistence}`; host boundaries enter at the composing app root.

## [03]-[UNIVERSAL_VS_CAPTURE]

"Universal" never means host-free C#. Every C# stratum is RhinoCommon-aware, the kernel included. Universal means the portable cross-runtime contracts the host-free runtimes consume: content-identity, geometry payload/GLB, IFC/BIM semantics, material-profile-as-data, and the typed receipts. Host-free peers (Python, TypeScript) consume those contracts; they never consume RhinoCommon.

- A host-neutral owner exists only for a genuine non-Rhino consumer contract; otherwise the Rhino-native surface is captured as a rich host feature.
- Consumer absence never lowers the capability bar, but a contract with no cross-runtime consumer is a Rhino feature, not a universal owner.
- `Rasm.Rhino` exchange and drafting stay rich Rhino features and are NOT thinned; Rhino owns Make2D, sheet layout, and native file IO.
- `Rasm.Bim` independently owns the universal IFC/exchange semantic model; native capture and host-neutral semantics coexist, neither gutted.
- A host-neutral owner expresses the universal contract, the host boundary the native surface; they meet only at the contract.

## [04]-[GEOMETRY_FLOW]

Geometry, meshing, and IFC each have exactly one owner per runtime; the runtimes meet only at the wire. No concern is owned twice within a runtime, and no runtime re-implements a peer's geometry.

- C# geometry source-of-truth is `Rasm`; the compute, persistence, and BIM owners consume it and never own or re-implement it.
- Python owns host-free geometry for offline scan/IFC work — an independent peer producer, not a `Rasm` consumer.
- `Rasm` and Python geometry meet only at the wire: content identity plus the GLB tessellation rail.
- TypeScript consumes that wire for web render and owns no geometry.
- Meshing has one owner per runtime — the C# kernel, the Python scan plane, the TypeScript render tier.
- IFC has one semantic owner per runtime — `Rasm.Bim` in C#, the IfcOpenShell companion in Python — meeting at the wire, never by duplication.

## [05]-[PLANNING_LIFECYCLE]

`.planning/` is a transient greenfield scaffold, not a permanent fixture. It exists to bring an under-developed folder to the decision-complete bar and dissolves as source lands; the eventual source tree is authored only when code is written.

- A greenfield package keeps its design pages inside one `.planning/` at the package root, sub-domain sub-folders mirroring the eventual source tree.
- A package root carries the index docs and nothing else.
- All planning lives under the single `.planning/`, never inside a real source sub-folder.
- Each package `ARCHITECTURE.md` maps the full folder structure including planned sub-domains without pages, so the map fuels ideas and tasks.
- Mature folders with real code carry NO `.planning/`; the co-located source architecture note is the only design surface.
- A mature folder's open split, cleanup, or re-architect work lives as task cards in the branch `TASKLOG.md`.
- One exception stands: a genuinely new unbuilt sub-domain inside a mature package keeps its scaffold in that sub-domain folder.
- `Rasm.Rhino` and `Rasm.Grasshopper` are HOST-BOUNDARY planning folders with a folder `.api/` tier over their host assemblies.
- Their solution rows stay out of `Workspace.slnx` under the architecture-test host-boundary gate.

## [06]-[PER_LANGUAGE_ROLES]

All three branches are first-class peers, each a complete library adoptable in any monorepo, coupled only at the wire. Their roles are distinct in kind, not a name-mirror of one another.

- C# is the Rhino/GH2-aware AEC platform: the geometry kernel, the AEC domain, the app platform, and the host boundaries.
- C# produces the wire vocabulary and the capability descriptors every peer consumes.
- Python is the host-free science/compute/data/geometry/IFC companion, integrating only through the wire and the companion seams.
- TypeScript is the first-class host-free web/edge platform; wire decode is a boundary concern inside `core`, never the branch identity.
- TypeScript consumes the C# wire and the GLB rail only, owns no geometry, and keeps its test infrastructure under `tests/`.

Within each language the same organization principle holds: real higher-order domain folders (no weak or mini sibling — a small isolated concern folds into the bigger concept it belongs to), source-mirroring sub-domain organization, OOP capsules at boundaries and FP-ROP internals. A branch re-derives its topology from the finalized owner set, never from a stale layout.

## [07]-[CROSS_LANGUAGE_WIRE]

Branches couple ONLY through the wire contracts and the companion/offline seams; coupling beyond them is a defect. This section states the wire law; the concrete integration point for any one seam lives as a boundary/wire consideration on the task that builds it, not in a standalone cross-reference ledger that drifts.

- Each shared canonical concept carries ONE name and ONE owner across the three branches, consumed at the boundary and never re-minted.
- Shared owners: content-address identity, the proto wire vocabulary, the suite wire law, and the op-log CRDT payload.
- Shared owners, continued: the capability descriptor and SDK source, tenant/causal identity, the GLB tessellation rail, and graduation evidence.
- C# owns the wire vocabulary; Python and TypeScript decode it and never mint a parallel.
- Python owns the geometry/IFC evaluation companion; C# requests and re-imports; TypeScript consumes the GLB for web render.
- Content identity reproduces bit-identically in all three runtimes from the one C#-owned seed, so any runtime's artifact is reusable by the others.
- A second mint of a shared concept in any branch is the named cross-language drift defect.
- Cross-`libs/` `IDEAS.md` and `TASKLOG.md` carry the cross-language concert; each branch states only its own owner of a shared concept.

## [08]-[TELEMETRY_WIRE_LAW]

Four-signal telemetry — metrics, logs, traces, profiles — correlates across the three runtimes through rows each branch transcribes identically in meaning: canonical here, transcribed at `csharp:Rasm.AppHost/Observability/telemetry#TELEMETRY_IDENTITY` with `#SIGNAL_GOVERNANCE`, `python:runtime/observability/telemetry#TELEMETRY`, and `typescript:runtime/otel/emit#POLICY`, the name vocabulary at `typescript:core/observe/convention`. A drifted row repairs at its owning branch page, never through a shared library — transcription is the conformance mechanism because the three SDK trains move on split maturity channels.

- Resource triple: `service.namespace` is `rasm`, `service.name` the service row, `service.instance.id` a per-process mint; detector rows enrich the minted triple and never replace it, and deployment-time resource overrides win the merge.
- Metric names: dotted `rasm.<domain>.<measure>` under UCUM units with no pre-baked `_total` or unit suffixes; the metrics store's OTLP receiver pins the `NoUTF8EscapingWithSuffixes` translation, so dotted names survive byte-identical from every runtime.
- Scope: instrumentation scope is the emitting package id, version-stamped and pinned to one semconv schema coordinate on tracer, meter, and logger alike; each branch spells the coordinate once and the three bump together.
- Egress: OTLP over HTTP+protobuf with gzip, one collector base endpoint fanned per signal on `/v1/<signal>`; the endpoint is deploy-plane data — `typescript:iac/program/spec#OUTPUT_PLANES` `StackOutputs.otlp` into the workload env — never an in-code literal.
- Propagation: one W3C composite — trace-context beside baggage — registers as the global propagator in every runtime; parent-based sampling honors the minted parent, so one distributed trace never fractures at a language boundary.
- Exemplars: trace-based admission — a measurement recorded inside a sampled active span carries its trace and span ids; the metric-to-trace click-through gates on the selected store row's exemplar column at `typescript:iac/operate/observe#STORE_ROWS`.
- Histograms: base2 exponential aggregation is the wire default in every branch; explicit-bucket advisory rows are the per-instrument fallback a backend or deployment view re-arms.
- Tenant: `rasm.tenant` is the one tenant dimension — a W3C baggage entry promoted onto spans and logs through allowlisted processors and folded onto metric attributes under per-view cardinality caps; an absent entry is single-tenant, never a sentinel value.
- Receipts stay the truth: instruments, spans, and log records are projections of typed receipts through per-branch projection owners — `csharp` `InstrumentFan`, `python` `Metrics.record`, `typescript` `Pulse` — and a metric minted beside a receipt fan is a second truth, the named defect.

[TENANT_COST_JOIN]: per-tenant cost attribution is one three-pin join — the C# grant spend family (`rasm.apphost.grant.spend.<unit>` off `GrantBroker` cost vectors through the instrument fan), the SDK-side `rasm.tenant` promotion every runtime registers (the one gate; no collector processor re-mints the dimension), and the iac cost read (OpenCost against the selected store row, boards compiled into the default and tenant organizations). Past a store row's tenant series cap, attribution rides exemplar-sampled traces — trace-scoped spend evidence, never a second metering pipeline.

[FLEET_ESCALATION]: every escalation row is OFF at estate scale by ruling; re-arming is the named coordinate flip against `typescript:iac/operate/observe#CHART_ROWS`, never a re-design.

| [INDEX] | [ESCALATION]           | [ARM_COORDINATE]                                                     | [STANDING_RULING]                     |
| :-----: | :--------------------- | :------------------------------------------------------------------- | :------------------------------------ |
|  [01]   | Mimir scale-out store  | `spec.profile.observe.store: "mimir"` + `Lgtm.Args.objects`          | `prometheus` row holds at estate load |
|  [02]   | Broker-buffered leg    | contrib image row + paired `kafka` exporter/receiver pipeline rows   | `file_storage` queue owns durability  |
|  [03]   | Tail-sampling gateway  | `tail_sampling` processor row on the collector traces pipeline       | head sampling rides SDK parent ratios |
|  [04]   | Per-app agent topology | second collector row, `mode: "daemonset"`, aimed at the gateway door | one `deployment` gateway serves all   |

[PROFILE_SWAP]: profiles migrate from vendor push onto the OTLP profiles signal by row replacement, armed only when the signal reaches stable across the three SDK trains; span-profile correlation processors, the Pyroscope store row, and every dashboard survive the swap unchanged. Swap-point owners: `csharp:Rasm.AppHost/Observability/telemetry#SIGNAL_GOVERNANCE`, `python:runtime/observability/profiles#PROFILES` with `telemetry#TELEMETRY`, `typescript:runtime/otel/profile`, and `typescript:iac/operate/observe#CHART_ROWS`.

| [INDEX] | [RUNTIME]  | [PUSH_ROW_TODAY]                                          | [SWAP_POINT]                                           |
| :-----: | :--------- | :-------------------------------------------------------- | :----------------------------------------------------- |
|  [01]   | C#         | `AddProcessor<PyroscopeSpanProcessor>()` + agent env rows | OTLP profiles exporter row on the same otelExport arm  |
|  [02]   | Python     | `Profiles.install` `pyroscope.configure` push             | one `EGRESS` profiles factory row + `SignalSpec` row   |
|  [03]   | TypeScript | `Profile.live` push bracket over the node profiler        | a profiles lane row beside the `Export.live` exporters |
|  [04]   | Collector  | `otlphttp/profiles` exporter + `profiles` pipeline        | already OTLP-shaped; the ingest door holds unchanged   |
