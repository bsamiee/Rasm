# [MONOREPO_ARCHITECTURE]

Tier-0 topology law owns the cross-cutting hierarchy no branch or package can own, and it alone names a language — a branch or folder consumes the topology as settled vocabulary. Per-package details lives at each folder `ARCHITECTURE.md`, never restated here.

## [01]-[STRATA]

Rasm is a polyglot library monorepo organized into strict strata. Each language branch is independently adoptable, and each package must stand on it's own as a complete concept and set of capability, aligned with sibling packages and branches through alignment of shape, functionality logic, and signature, cross package integration can only occure with strict adherance to the `ARCHITECTURE.md` hierarchy, packages are higher-order domains. Polyglot applications compose branch-owned capability through language-neutral contracts.

Stratification is the law every branch answers: ranks run `S0` upward, the dependency edge runs strictly upward, the graph stays acyclic, and shared machinery homes at the lowest stratum every consumer reaches — homing above a consumer's reach manufactures per-folder twins. Each branch orders its own packages under that law at its `[02]-[STRATA]`, and that table is the roster a reader resolves; Tier-0 names no branch's packages.

- Strata is the only rank vocabulary; wave, band, and tier never name a rank.
- Dependency edges stay abstract: a branch spells one as a project reference, module import, composed layer, or port bound at the composition root, the upward law binds every spelling.
- Cross-package coupling reaches a published boundary or a content-keyed wire, never a peer's interior.
- Peers at one rank never reference each other; alignment travels the seam contracts and the content-keyed wire, so each package stays usable in isolation.
- Counter-edges carry a value the lower stratum consumes, never an owner it imports; the type graph stays acyclic, member-seating rows state that qualifier.
- Plane-distinct members seat at a rank yet stay outside the runtime graph, carrying the same upward law.
- Higher strata consume lower-stratum capability and re-own none of it.
- Composition roots are the leaf: host binding, port satisfaction, and cross-branch composition happen there, nowhere below. Product shells outside `libs/` compose a branch's top strata as consumers of the estate, never as a stratum of it.

## [02]-[DEPENDENCY_DIRECTION]

Direction inside a branch is the branch's own `[02]-[STRATA]` to state per owner. Across branches direction dissolves: no branch is a dependency edge, producer, or prerequisite of another.

- Cross-branch relations are contracts, never dependencies: each carries data, obligates no build order, and admits no reference in either direction.
- Single-language applications resolve their branch's whole graph with no peer present; branch contributions meet only at the composition root, merged by artifact key under the `[09]-[BACKEND_STATE]` order.
- Each branch proves its own direction with its own gate, and that gate parses the owning branch's strata table live rather than carrying a transcribed copy.

## [03]-[UNIVERSAL_VS_CAPTURE]

"Universal" names a contract `tests/contracts/` defines and freezes. Corpus definition is the whole discriminant: a concept several branches happen to spell carries no cross-language meaning until a manifest entry defines it, and an entry binds every branch it names.

- Universal owners exist only for corpus-defined contracts; `[07]-[CROSS_LANGUAGE_WIRE]` splits them into the infrastructure and domain classes.
- Branch-local owners retain host, toolchain, and native capability no contract carries, at full richness.
- `Rasm.Rhino` exchange and drafting stay rich Rhino features and are NOT thinned; Rhino owns Make2D, sheet layout, and native file IO.
- `Rasm.Bim` implements IFC in C# while IfcOpenShell implements it in Python; both conform to the portable IFC contract, and neither branch reads the other.
- Host boundaries own native surfaces, and branch adapters project them into a corpus contract at the seam alone.

## [04]-[GEOMETRY_FLOW]

Geometry, meshing, and IFC each have exactly one owner per runtime; the runtimes meet only at the wire. No concern is owned twice within a runtime, and no runtime re-implements a peer's geometry.

- C# geometry source-of-truth is `Rasm`; the compute, persistence, and BIM owners consume it and never own or re-implement it.
- Python owns host-free geometry for offline scan/IFC work — an independent peer producer, not a `Rasm` consumer.
- `Rasm` and Python geometry meet only at the wire: content identity and the GLB tessellation rail.
- TypeScript decodes that wire at its render tier; the contract carries geometry across a runtime boundary, and no runtime re-implements a peer's kernel.
- Meshing has one owner per runtime — the C# kernel, the Python scan plane, the TypeScript render tier.
- IFC has one semantic implementation per runtime — `Rasm.Bim` in C# and IfcOpenShell in Python — aligned by the portable IFC contract.

## [05]-[PLANNING_LIFECYCLE]

`.planning/` is a transient greenfield scaffold, not a permanent fixture. It exists to bring an under-developed folder to the decision-complete bar and dissolves as source lands; the eventual source tree is authored only when code is written.

- Greenfield packages keep their design pages inside one `.planning/` at the package root, sub-domain sub-folders mirroring the eventual source tree.
- Package roots carry the index docs and nothing else.
- All planning lives under the single `.planning/`, never inside a real source sub-folder.
- Each package `ARCHITECTURE.md` maps the full folder structure including planned sub-domains without pages, so the map fuels ideas and tasks.
- Mature folders with real code carry NO `.planning/`; the co-located source architecture note is the only design surface.
- Mature folders route open split, cleanup, and re-architect work to task cards in the branch `TASKLOG.md`.
- One exception stands: a genuinely new unbuilt sub-domain inside a mature package keeps its scaffold in that sub-domain folder.
- `Rasm.Rhino` and `Rasm.Grasshopper` are HOST-BOUNDARY planning folders with a folder `.api/` tier over their host assemblies.
- Their solution rows stay out of `Workspace.slnx` under the architecture-test host-boundary gate.

## [06]-[PER_LANGUAGE_ROLES]

Each branch is an independently adoptable library estate. Branch-local capability originates, composes, operates, and evolves from that branch's packages, manifests, generated bindings, and toolchain.

- Each row names the domain its estate carries; rank, dependency, and a ceiling on what an estate may carry all sit outside the row.
- C# carries the Rhino 9/GH2-aware AEC domain: the geometry kernel, the element seam and its AEC peers, the app platform, and the host boundaries.
- Python carries the host-free science, compute, data, geometry, IFC, and artifact domain.
- TypeScript carries the host-free web, edge, runtime, persistence, security, UI, and deployment domain.
- Each branch composes, mints, verifies, and deploys its own backend state — schema, migration, recovery, evidence — with no peer branch present.
- Cross-language composition adds peers to a complete branch; it never supplies a branch's missing operational base.

Within each language the same organization principle holds: real higher-order domain folders (no weak or mini sibling — a small isolated concern folds into the bigger concept it belongs to), source-mirroring sub-domain organization, OOP capsules at boundaries and FP-ROP internals. Each branch re-derives its topology from the finalized owner set, never from a stale layout.

## [07]-[CROSS_LANGUAGE_WIRE]

`tests/contracts/` owns every cross-language contract: the seam schema defines the shape, the frozen assets prove conformance, and `MANIFEST.md` binds each contract to its class. Each branch generates local bindings and owns its implementation adapters; no branch imports another's.

[INFRASTRUCTURE]: `MANIFEST.md` classes an entry infrastructure when every branch needs the shape to run alone.
- Every branch mints its own instance from its own inputs; the corpus schema is the definition, and corpus parity is the proof.
- No branch is a prerequisite for another — a single-language application resolves the contract whole with no peer present.
- Drift defect: a second mint of one infrastructure contract inside one branch forks the parity the corpus proves.

[DOMAIN]: `MANIFEST.md` classes an entry domain when one branch's capability originates the semantic model.
- One producer emits the artifact, named by the domain capability it holds and never by language rank; peers decode and re-encode.
- Producers project a branch-local concept into a shared contract at the seam that contract declares, never before it.
- Drift defect: a second producer for one domain contract forks the semantic model that contract carries.

Composition merges branch contributions at the application root by artifact key under one total order. Content identity derives bit-identically in every branch from the `docs/laws/patterns.md` `[CONTENT_KEY]` law.

## [08]-[TELEMETRY_WIRE_LAW]

Four-signal telemetry — metrics, logs, traces, profiles — correlates across the three runtimes through rows each branch transcribes identically in meaning: canonical here, transcribed at `csharp:Rasm.AppHost/Observability/telemetry`, `python:runtime/observability/telemetry`, and `typescript:runtime/otel/emit`, the name vocabulary at `typescript:core/observe/convention`. Drifted rows repair at their owning branch page, never through a shared library — transcription is the conformance mechanism because the three SDK trains move on split maturity channels.

- Resource triple: `service.namespace` `rasm`, `service.name` the service row, `service.instance.id` a per-process mint.
- Detector rows enrich the minted triple, never replace it; deployment-time resource overrides win the merge.
- Metric names: dotted `rasm.<domain>.<measure>` under UCUM units, no baked `_total` or unit suffixes.
- Metrics-store OTLP receiver pins `NoUTF8EscapingWithSuffixes`, so dotted names survive byte-identical from every runtime.
- Scope: the emitting package id, version-stamped, one semconv coordinate on tracer, meter, and logger; a branch spells it once, all bump together.
- Egress: OTLP over HTTP+protobuf with gzip, one collector base endpoint fanned per signal on `/v1/<signal>`.
- `typescript:iac/program/spec` `StackOutputs.otlp` feeds the endpoint into the workload env — deploy-plane data, never an in-code literal.
- Propagation: one W3C composite — trace-context beside baggage — registers as the global propagator in every runtime.
- Ingress adopts an inbound parent whole — trace id continued, span id parented — so parent-based sampling never fractures a trace across runtimes.
- Exemplars: a measurement recorded inside a sampled active span carries its trace and span ids.
- Metric-to-trace click-through gates on the selected store row's exemplar column at `typescript:iac/operate/observe`.
- Histograms: base2 exponential is the wire default in every branch; explicit-bucket advisory rows are the per-instrument fallback a view re-arms.
- Counters: DELTA temporality is the wire default in every branch; cumulative is the monotonic-totals alternative a policy row selects.
- Tenant: `rasm.tenant` is the one dimension — baggage promoted onto spans and logs by allowlisted processors, folded onto metrics under view caps.
- Absent tenant entries read as single-tenant, never as a sentinel value.
- Receipts stay the truth: signals project from typed receipts through the per-branch owners `InstrumentFan`, `Metrics.record`, and `Pulse`.
- Metrics minted beside a receipt fan are a second truth.

[TENANT_COST_JOIN]: per-tenant cost attribution is one three-pin join — the C# grant spend family (`rasm.apphost.grant.spend.<unit>` off `GrantBroker` cost vectors through the instrument fan), the SDK-side `rasm.tenant` promotion every runtime registers (the one gate; no collector processor re-mints the dimension), and the iac cost read (OpenCost against the selected store row, boards compiled into the default and tenant organizations). Past a store row's tenant series cap, attribution rides exemplar-sampled traces — trace-scoped spend evidence, never a second metering pipeline.

[FLEET_ESCALATION]: every escalation row is OFF at estate scale by ruling; re-arming is the named coordinate flip against `typescript:iac/operate/observe`, never a re-design.

| [INDEX] | [ESCALATION]           | [ARM_COORDINATE]                                                     | [STANDING_RULING]                     |
| :-----: | :--------------------- | :------------------------------------------------------------------- | :------------------------------------ |
|  [01]   | Mimir scale-out store  | `spec.profile.observe.store: "mimir"` + `Lgtm.Args.objects`          | `prometheus` row holds at estate load |
|  [02]   | Broker-buffered leg    | contrib image row + paired `kafka` exporter/receiver pipeline rows   | `file_storage` queue owns durability  |
|  [03]   | Tail-sampling gateway  | `tail_sampling` processor row on the collector traces pipeline       | head sampling rides SDK parent ratios |
|  [04]   | Per-app agent topology | second collector row, `mode: "daemonset"`, aimed at the gateway door | one `deployment` gateway serves all   |

[PROFILE_SWAP]: profiles migrate from vendor push onto the OTLP profiles signal by row replacement, armed only when the signal reaches stable across the three SDK trains; span-profile correlation processors, the Pyroscope store row, and every dashboard survive the swap unchanged. Swap-point owners: `csharp:Rasm.AppHost/Observability/telemetry`, `python:runtime/observability/profiles`, `typescript:runtime/otel/profile`, and `typescript:iac/operate/observe`.

| [INDEX] | [RUNTIME]  | [PUSH_ROW_TODAY]                                          | [SWAP_POINT]                                           |
| :-----: | :--------- | :-------------------------------------------------------- | :----------------------------------------------------- |
|  [01]   | C#         | `AddProcessor<PyroscopeSpanProcessor>()` + agent env rows | OTLP profiles exporter row on the same otelExport arm  |
|  [02]   | Python     | `Profiles.install` `pyroscope.configure` push             | one `EGRESS` profiles factory row + `SignalSpec` row   |
|  [03]   | TypeScript | `Profile.live` push bracket over the node profiler        | a profiles lane row beside the `Export.live` exporters |
|  [04]   | Collector  | `otlphttp/profiles` exporter + `profiles` pipeline        | already OTLP-shaped; the ingest door holds unchanged   |

## [09]-[BACKEND_STATE]

Backend state is an infrastructure contract: every branch composes its own schema contract from its own framework artifacts and mints its own generation identity, so a single-language application declares, materializes, proves, and deploys its schema with no peer present.

- Contract shape, canonicalization, artifact ordering, and generation-identity derivation live in the `BACKEND_CONTRACT` corpus entry.
- That entry names each branch's minting anchor, and every branch spells the entry's law in its own types rather than sharing an implementation.
- Polyglot composition merges branch contributions at the application root by artifact key under one deterministic order.
- Merged generation is the deployment unit; a single-language application deploys the generation it minted alone, with no merge step.
- Runtime verifies the generation it observes and never mutates it; a digest change replaces the generation whole.
- Provider adapters, migration execution, protocol evolution, journal identity, and work identity stay branch-owned.
- Deployment publishes only a generation proved against the corpus schema.

## [10]-[CONSUMPTION_MODEL]

Every `libs/` package is an independently versioned library an unrelated application depends on exactly as it depends on any external package. One branch serves dozens of unrelated consumers at once — single-tenant and multi-tenant, in-host and headless, sidecar and companion and service and edge and CLI — so a package assumes no consumer, no sibling set, no deployment shape, and no lifecycle owner. Deployment shape arrives as data: the composition root supplies one profile row over the axis roster, and the package reads it.

| [INDEX] | [AXIS]      | [FORM] | [VOCABULARY]                                                          |
| :-----: | :---------- | :----- | :-------------------------------------------------------------------- |
|  [01]   | `tenancy`   | closed | `none` \| `single` \| `multi`                                         |
|  [02]   | `topology`  | closed | `in-host` \| `sidecar` \| `companion` \| `service` \| `edge` \| `cli` |
|  [03]   | `host`      | open   | capability-descriptor rows the owning branch supplies                 |
|  [04]   | `lifecycle` | closed | `caller-owned` \| `package-owned`                                     |
|  [05]   | `isolation` | closed | `in-proc` \| `thread` \| `process` \| `wasm` \| `remote`              |
|  [06]   | `providers` | open   | capability-descriptor rows the owning branch supplies                 |

- Closed axes fix their value vocabulary here; an open axis fixes the descriptor shape alone, and each of its rows is capability one branch supplies.
- Host integration lands as one descriptor row: Rhino and GH2 are rows C# supplies, future inetgrations: energy engine, simulation host, design tools, etc.
- Enumerating an open axis's instances here re-anchors the roster to the instance set that happens to exist — the anchoring defect the open form forecloses.
- Each branch spells the roster in its own types, and `tests/contracts/` `CONSUMPTION_PROFILE` proves parity like any other infrastructure contract.
- Growth is one Tier-0 row beside one row per branch for a new axis, one case on its owning closed axis for new values; open axes grow at supplying branch alone.
- Axis values stay data: compile-time assumption, ambient global, build flag, or package branching on which product hosts it re-mint the assumed consumer roster forecloses.
- Packages unable to serve an axis value refuse at admission with typed evidence naming the axis; silent degradation and narrowed public surface are the two failed forms.
- Sibling presence is an axis value — a package composes a sibling through a declared port the composition root binds, an unbound port is a refused capability, never a crash.

## [11]-[ENTRY_POINT_LAW]

One polymorphic entry serves each bounded concept: modality, arity, tenancy, topology, and provider ride the request shape or a policy row the entry folds. Entries internalize policy resolution, routing, and lifecycle, so a consumer composes capability instead of assembling it.

- `Get`/`GetMany`/`GetBy<Key>` families, per-topology entries, and boolean knob pairs are the three refused shapes; each pushes the fold onto every caller.
- Growth is one row, arm, or case on the owner; a new consumer shape never widens the public surface.
- Each owning cluster's card `Entry` field names the entry; the folder `README.md` router reaches it through the owning design page.
- Axis admission states once at the package's admission owner, never per entry.
