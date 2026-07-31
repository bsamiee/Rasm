# [MONOREPO_ARCHITECTURE]

Rasm carries a platform tier and a product tier: `libs/` holds independently adoptable library estates, and every app, plugin, and service composes them exactly as it takes an external package. Capability lands in the platform first, parameterized for consumers that do not yet exist; a product shell declares intent, binds host edges, and emits output.

Estates couple through defined contracts alone — no estate imports a peer, and none is a prerequisite, producer, or build-order edge for another. Tier-0 owns the law no branch or package can own and names languages alone; each branch and folder owns its own package roster, charters, and reference direction.

## [01]-[STRATA]

Stratification is the law every branch answers: ranks run `S0` upward, the dependency edge runs strictly upward, the graph stays acyclic, and shared machinery seats at the lowest stratum every consumer reaches — seating above a consumer's reach manufactures per-folder twins. Each branch orders its own packages under that law at its `[02]-[STRATA]`, and that table is the roster a reader resolves.

- Strata is the only rank vocabulary; wave, band, and tier never name a rank.
- Dependency edges stay abstract — project reference, module import, composed layer, or root-bound port — and the upward law binds every spelling.
- Cross-package coupling reaches a published boundary or a content-keyed wire, never a peer's interior.
- Peers at one rank never reference each other; alignment travels seam contracts and the content-keyed wire, so each package stays usable alone.
- Counter-edges carry a value the lower stratum consumes, never an owner it imports, and member-seating rows state that qualifier.
- Plane-distinct members seat at a rank yet stay outside the runtime graph, carrying the same upward law.
- Higher strata consume lower-stratum capability and re-own none of it.
- Composition roots take host binding, port satisfaction, and cross-branch composition as the leaf, nowhere below.
- Product shells compose a branch's top strata as consumers of the estate, never as a stratum of it.

## [02]-[DEPENDENCY_DIRECTION]

Direction inside a branch is the branch's own `[02]-[STRATA]` to state per owner. Across branches direction dissolves: no branch is a dependency edge, producer, or prerequisite of another.

- Cross-branch relations are contracts, never dependencies: each carries data, obligates no build order, and admits no reference in either direction.
- Single-language applications resolve their branch's whole graph with no peer present.
- Branch contributions meet at the composition root alone, merged by artifact key under the `[09]-[SCHEMA_STATE]` order.
- Each branch proves its own direction with its own gate, and that gate parses the owning strata table live, never a transcribed copy.

## [03]-[UNIVERSAL_VS_CAPTURE]

"Universal" names a contract `tests/contracts/` defines and freezes. Corpus definition is the whole discriminant: a concept several branches happen to spell carries no cross-language meaning until a manifest entry defines it, and an entry binds every branch it names.

- Universal owners exist only for corpus-defined contracts; `[07]-[CROSS_LANGUAGE_WIRE]` splits them into the infrastructure and domain classes.
- Branch-local owners retain host, toolchain, and native capability no contract carries, at full richness.
- Host-boundary packages own their host's native surface whole — exchange, drafting, sheet layout, and file IO stay rich and thin toward no contract.
- One semantic implementation per runtime conforms to a portable domain contract, and neither implementation reads the other.
- Branch adapters project a native surface into a corpus contract at the seam that contract declares, never earlier.

## [04]-[GEOMETRY_FLOW]

Geometry, meshing, and semantic exchange each carry exactly one owner per runtime; the runtimes meet only at the contract. No concern is owned twice within a runtime, and no runtime re-implements a peer's kernel.

- Each runtime carries one geometry owner, one meshing owner, and one semantic-exchange implementation; a second is the duplication defect.
- Runtimes meet at content identity, the tessellation rail, and the appearance rail the manifest's appearance entries define.
- Decoders at the tessellation and appearance rails compose the payload rather than re-deriving it.
- Independent peer producers stay independent: a host-free geometry owner produces for its own domain rather than consuming a peer's kernel.
- Each branch names its own runtime's owners.

## [05]-[PLANNING_LIFECYCLE]

`.planning/` is a transient greenfield scaffold, not a permanent fixture. It exists to bring an under-developed folder to the decision-complete bar and dissolves as source lands; the eventual source tree is authored only when code is written.

- Greenfield packages keep their design pages inside one `.planning/` at the package root, sub-domain sub-folders mirroring the eventual source tree.
- Package roots carry the index docs and nothing else.
- All planning lives under the single `.planning/`, never inside a real source sub-folder.
- Each package maps its full folder structure, planned page-less sub-domains included, so the map fuels ideas and tasks.
- Mature folders with real code carry NO `.planning/`; the co-located source architecture note is the only design surface.
- Mature folders route open split, cleanup, and re-architect work to task cards in the branch `TASKLOG.md`.
- One exception stands: a genuinely new unbuilt sub-domain inside a mature package keeps its scaffold in that sub-domain folder.
- Host-boundary planning folders carry a folder `.api/` tier over host assemblies, its rows outside the branch build root under the host gate.

## [06]-[PER_LANGUAGE_ROLES]

Each branch is an independently adoptable library estate. Branch-local capability originates, composes, operates, and evolves from that branch's packages, manifests, generated bindings, and toolchain.

- Each row names the domain its estate carries; rank, dependency, and a ceiling on what an estate may carry all sit outside the row.
- C# carries the host-bound AEC domain: the geometry kernel, the element seam and its AEC peers, the app platform, and the host boundaries.
- Python carries the host-free science, compute, data, geometry, exchange, and artifact domain.
- TypeScript carries the host-free web, edge, runtime, persistence, security, UI, and deployment domain.
- Each branch composes, mints, verifies, and deploys its own schema state — schema, migration, recovery, evidence — with no peer branch present.
- Cross-language composition adds peers to a complete branch; it never supplies a branch's missing operational base.
- Domains widen at their owning branch under `[12]-[ADMISSION]`; the rows above carry no ceiling on what a branch admits next.

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

Every shape crossing a branch boundary carries a `MANIFEST.md` entry; a convention-aligned shape crossing without one is coincidence that forks on first edit, never interop. Composition merges branch contributions at the application root by artifact key under one total order. Content identity derives bit-identically in every branch from the `docs/laws/patterns.md` `[CONTENT_KEY]` law.

## [08]-[OBSERVABILITY_CONFORMANCE]

Four-signal telemetry — metrics, logs, traces, profiles — correlates across the three runtimes through rows each branch transcribes identically in meaning: canonical here, transcribed at `csharp:Rasm.AppHost/Observability/telemetry`, `python:runtime/observability/telemetry`, and `typescript:runtime/otel/emit`, the name vocabulary at `typescript:core/observe/convention`. SDK trains move on split maturity channels, so no shared library carries those rows.

Transcription SPELLS the rows per branch and `tests/contracts/` `TELEMETRY_CONVENTION` PROVES the three spellings agree, so a drift surfaces at the digest and repairs at its owning branch page.

- Resource triple: `service.namespace` `rasm`, `service.name` the service row, `service.instance.id` a per-process mint.
- Detector rows enrich the minted triple, never replace it; deployment-time resource overrides win the merge.
- Metric names: dotted `rasm.<domain>.<measure>` under UCUM units, no baked `_total` or unit suffixes.
- `<domain>` names the capability subject a query joins on, never the package, host, or verb emitting it.
- Runtimes serving one subject share the segment and `service.name` separates the emitters; a package spanning two subjects emits under both.
- Subject test: subjects survive a second emitter, so a segment spelled like its package or host stands.
- Segments two branches claim carry one subject spelling both transcribe byte-identical, so the roster reads as one vocabulary, never three.
- Domain rows stay branch-declared at each conformance minter, since fixing the instances here re-anchors the vocabulary to today's segment set.
- Each row carries its segment beside the admitted subject.
- Emitter rosters stay branch-local, earned where a branch's own module declaration closes them against drift.
- Carve: estate identity dimensions are the one unsegmented spelling the grammar admits.
- Carve: package-keyed hook-point ids beside a runtime's own module namespace sit outside the grammar whole.
- Carve: transport coordinates beside residence relation and session-setting names share the prefix and answer to their own owners.
- Gate: every signal name a branch mints — instrument, dimension, resource, span, log, and event alike — carries a segment the roster resolves.
- Unresolved segments refuse at their declaration owner rather than reaching an exporter.
- A grammar-derived routing fact states its derivation beside every emission owner, never only at the census owner — a producer emitting under a foreign domain prefix forks the record call knowingly, never silently.
- Metrics-store OTLP receiver pins `NoUTF8EscapingWithSuffixes`, so dotted names survive byte-identical from every runtime.
- Scope: the emitting package id, version-stamped, one semconv coordinate on tracer, meter, and logger; a branch spells it once, all bump together.
- Egress: OTLP over HTTP+protobuf, one collector base endpoint fanned per signal on `/v1/<signal>`.
- Compression pins gzip on every sender whose transport exposes an encoding knob, bound as a composition VALUE, never an unpublished environment key.
- Senders exposing none declare the uncompressed leg a parity column at their branch minter, so an unset knob states a column, never a silent hole.
- `typescript:iac/program/spec` `StackOutputs.otlp` feeds the endpoint into the workload env — deploy-plane data, never an in-code literal.
- Buffering: each signal exports through a bounded batch leg whose queue, batch, delay, and timeout bounds are branch governance policy values.
- Export loss is accounted evidence — a dropped batch, rejected export, or wedged flush lands on the branch's own diagnostic floor, never silent.
- Durability past the process rides the gateway's persistent queue; a branch-side durable OTLP queue arms as a policy row, never a default.
- Lifecycle: one ranked drain per composition flushes and shuts every provider inside a bounded window, and a wedged flush parks as evidence there.
- Propagation: one W3C composite — trace-context beside baggage — registers as the global propagator in every runtime.
- Ingress adopts an inbound parent whole — trace id continued, span id parented — so parent-based sampling never fractures a trace across runtimes.
- Exemplars: a measurement recorded inside a sampled active span carries its trace and span ids on every plane whose SDK exposes an exemplar seat.
- Planes reaching no seat declare that as a parity column at their branch minter and click through the gateway's span-derived series instead.
- Metric-to-trace click-through gates on the selected store row's exemplar column at `typescript:iac/operate/observe`.
- Histograms: base2 exponential is the wire default on every provider-aggregated plane; a view row re-arms the explicit-bucket fallback there.
- Producer-collected distributions fix their boundary ladders at the mint row — finished buckets reach the exporter and no view recomputes them.
- Counters: DELTA temporality is the wire default in every branch; cumulative is the monotonic-totals alternative a policy row selects.
- Tenant: `rasm.tenant` is the one dimension — baggage promoted onto spans and logs by allowlisted processors, folded onto metrics under view caps.
- Absent tenant entries read as single-tenant, never as a sentinel value.
- Receipts stay the truth: signals project from typed receipts through the per-branch owners `InstrumentFan`, `Metrics.record`, and `Pulse`.
- Metrics minted beside a receipt fan are a second truth.

[HOOK_PLANE]: domain code fires typed facts at package-qualified `rasm.<pkg>.<domain>.<point>` rows on one scoped hook registry per runtime — the kernel signal capsule's `HookPoint` roster for C#, `python:runtime/observability/hooks`, `typescript:core/observe/tap` — under one closed `veto` | `observe` | `replay` modality vocabulary. Registration is composition-unique, a subscriber fault isolates as typed evidence with the emitter's value untouched, and a signal emitter is an observe subscription over fired facts, never an emit inside a domain fold.

[TENANT_COST_JOIN]: per-tenant cost attribution is one three-pin join — the C# grant spend family (`rasm.apphost.grant.spend.<unit>` off `GrantBroker` cost vectors through the instrument fan), the SDK-side `rasm.tenant` promotion every runtime registers (the one gate; no collector processor re-mints the dimension), and the iac cost read (OpenCost against the selected store row, boards compiled into the default and tenant organizations). Past a store row's tenant series cap, attribution rides exemplar-sampled traces — trace-scoped spend evidence, never a second metering pipeline.

[EVIDENCE_RESIDENCE]: telemetry is an analytics subject in every branch — signal evidence lands in a durable columnar residence through the branch's own analytics custodian: `csharp:Rasm.Persistence`, `python:data/tabular` beside the `python:runtime/observability/journal` plane, and `typescript:data`. `spec.profile.observe.analytics` arms the residence family at `typescript:iac/operate/observe` — `lake` the default cold tail, `clickhouse` the interactive escalation, `none` a declared evidence loss — so evidence outliving a store's series window is a spec flip, never a per-branch pipeline.

[FLEET_ESCALATION]: every escalation row is OFF at estate scale by ruling; re-arming is the named coordinate flip against `typescript:iac/operate/observe`, never a re-design.

| [INDEX] | [ESCALATION]           | [ARM_COORDINATE]                                                    | [STANDING_RULING]                     |
| :-----: | :--------------------- | :------------------------------------------------------------------ | :------------------------------------ |
|  [01]   | Mimir scale-out store  | `spec.profile.observe.store: "mimir"` + `Lgtm.Args.objects`         | `prometheus` row holds at estate load |
|  [02]   | Broker-buffered leg    | `observe.buffer: "broker"` — the paired `kafka` pipeline rows       | `file_storage` queue owns durability  |
|  [03]   | Tail-sampling gateway  | `observe.sampling: "tail"` — the traces `tail_sampling` row         | head sampling rides SDK parent ratios |
|  [04]   | Per-app agent topology | `observe.topology: "agent"` — the daemonset row on the gateway door | one `deployment` gateway serves all   |

[PROFILE_SWAP]: profiles migrate from vendor push onto the OTLP profiles signal by row replacement, armed only when the signal reaches stable across the three SDK trains; span-profile correlation processors, the Pyroscope store row, and every dashboard survive the swap unchanged. Swap-point owners: `csharp:Rasm.AppHost/Observability/telemetry`, `python:runtime/observability/profiles`, `typescript:runtime/otel/profile`, and `typescript:iac/operate/observe`.

| [INDEX] | [RUNTIME]  | [PUSH_ROW_TODAY]                                          | [SWAP_POINT]                                           |
| :-----: | :--------- | :-------------------------------------------------------- | :----------------------------------------------------- |
|  [01]   | C#         | `AddProcessor<PyroscopeSpanProcessor>()` + agent env rows | OTLP profiles exporter row on the same otelExport arm  |
|  [02]   | Python     | `Profiles.install` `pyroscope.configure` push             | one `EGRESS` profiles factory row + `SignalSpec` row   |
|  [03]   | TypeScript | `Profile.live` push bracket over the node profiler        | a profiles lane row beside the `Export.live` exporters |
|  [04]   | Collector  | `otlp_http/profiles` exporter + `profiles` pipeline       | already OTLP-shaped; the ingest door holds unchanged   |

## [09]-[SCHEMA_STATE]

Schema state is an infrastructure contract: every branch composes its own schema contract from its own framework artifacts and mints its own generation identity, so a single-language application declares, materializes, proves, and deploys its schema with no peer present.

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
- Host integration lands as one descriptor row on the `host` axis, supplied by the branch whose runtime reaches that host.
- Enumerating an open axis's instances here re-anchors the roster to whatever set exists — the anchoring defect the open form forecloses.
- Each branch spells the roster in its own types, and `tests/contracts/` `CONSUMPTION_PROFILE` proves parity like any other infrastructure contract.
- Growth lands one Tier-0 row beside one row per branch for a new axis, one case on its owning closed axis for a new value.
- Open axes grow at their supplying branch alone.
- Axis values stay data — a compile-time assumption, ambient global, build flag, or a branch on the host re-mints the assumed consumer roster.
- Packages unable to serve an axis value refuse at admission with typed evidence naming the axis, never degrading or narrowing their surface.
- Sibling presence rides an axis value — a package composes a sibling through a declared port the composition root binds.
- Unbound ports read as a refused capability, never a crash.

## [11]-[DESIGN_LANGUAGE]

One design language makes disparate estates read as one system: a consumer crossing packages, branches, or hosts meets the same shapes under different spellings, so capability composes without per-package relearning. Each row states the invariant, and `docs/stacks/<language>/` owns its spelling.

| [INDEX] | [INVARIANT]            | [LAW]                                                                                              |
| :-----: | :--------------------- | :------------------------------------------------------------------------------------------------- |
|  [01]   | one concept, one name  | One semantic name per bounded concept, spelled in the branch's casing law.                         |
|  [02]   | one polymorphic entry  | One entry folds modality, arity, tenancy, topology, and provider off the request shape.            |
|  [03]   | failure is a value     | Domain logic returns typed error rails; exception flow stays at the boundary.                      |
|  [04]   | results carry receipts | Route, status, sampling, solver, and host evidence ride typed receipt fields.                      |
|  [05]   | variation is data      | Rows, cases, tables, and policy values own variation; a bounded vocabulary dispatches.             |
|  [06]   | refusal at admission   | An unservable axis value refuses with typed evidence naming the axis, once at the admission owner. |

- `Get`/`GetMany`/`GetBy<Key>` families, per-topology entries, and boolean knob pairs each push the fold onto every caller; the entry folds instead.
- Growth is one row, arm, or case on the owner; a new consumer shape never widens the public surface.
- Each owning cluster's card `Entry` field names the entry, and the folder `README.md` router reaches it through the owning design page.
- Names align because the concept is one: a name mirrored into a peer branch for symmetry alone forks two concepts under one spelling.

## [12]-[ADMISSION]

New capability enters at the narrowest rung that holds it, and each rung above is earned by evidence the rung below cannot carry. Reaching past the evidence mints a surface every consumer then folds around, and the earn-test decides the rung, never scope ambition or file size.

| [INDEX] | [RUNG]                | [EARNED_BY]                                                                                                    |
| :-----: | :-------------------- | :------------------------------------------------------------------------------------------------------------- |
|  [01]   | row or case           | A new instance of a settled concept whose vocabulary already admits it.                                        |
|  [02]   | adapter               | A foreign surface reaching an existing port, translating that surface's vocabulary at the boundary.            |
|  [03]   | design page           | One owner inside a settled sub-domain, under that sub-domain's invariants.                                     |
|  [04]   | sub-domain            | New nouns under the package's existing invariants and published boundary.                                      |
|  [05]   | package               | Own nouns, own invariants, and a published boundary an unrelated application adopts alone — a bounded context. |
|  [06]   | host-boundary package | A host's native surface, referencing the branch base alone and composed at a product root.                     |
|  [07]   | branch                | A target runtime no existing branch reaches.                                                                   |

- Packages name concepts: a host name, a provider name, a verb, or a size-driven split each names something other than a bounded context.
- Hosts enter as one host-boundary package beside descriptor rows and cases on the owners their demands prove; domain packages never name a host.
- Host vocabulary translates at the boundary adapter, and the domain keeps its own nouns whole.
- Generalization pressure arrives with each new host, its demands widening the owners already holding the concept.
- Concepts only one host reveals land as a bounded context named for the concept.
- Hosts scripted in a branch's own language earn no branch; that runtime is already reached.
- Ecosystem preference earns no branch — a branch answers reach, and a capability an existing branch reaches lands as a package there.
- Every rung lands its counterparts in the same pass — `[02]-[STRATA]` rank, manifest row, `.api/` tier, router entry — per `docs/laws/topology.md`.

## [13]-[APPEARANCE]

Surface appearance crosses the runtimes as TWO domain documents under ONE frozen vocabulary. `tests/contracts/appearance-vocabulary.schema.json` is the definition and `tests/contracts/MANIFEST.md` `[01]-[LEDGER]` states what it carries; each branch TRANSCRIBES that definition in its own casing law and validates its own projection against it, and the two corpus entries reference it with neither restating a row.

| [INDEX] | [OWNER]                             | [MINTS]                                    | [BOUNDARY]                             |
| :-----: | :---------------------------------- | :----------------------------------------- | :------------------------------------- |
|  [01]   | `csharp:Rasm.Materials/Raster`      | the baked set behind the appearance key    | carries no environment kind            |
|  [02]   | `csharp:Rasm.Materials/Appearance`  | the environment light and stage vocabulary | branch-interior; takes no corpus entry |
|  [03]   | `python:artifacts/graphic/texture`  | the ingest and environment set manifest    | presses no graph, holds no bake key    |
|  [04]   | `typescript:core/interchange`       | nothing; it lands both documents           | derives no field a producer carries    |
|  [05]   | `typescript:data` + `typescript:ui` | the transform, serve, and bind pipeline    | reads addresses, constructs none       |
|  [06]   | `csharp:Rasm`                       | the analytic and raster atoms both compose | holds no channel vocabulary            |

- Two documents, two producers, one vocabulary: each producer owns its own document and `domain` entry, and routes a kind it lacks to the peer entry.
- Capability overlaps freely and DOCUMENTS do not — both producers derive geometric channels and both encode containers.
- Producers emitting the peer's document class commit the second-producer defect the `domain` class forecloses.
- Consumption is never production, and a branch decoding both documents mints neither.
- Deriving one appearance field from another at a consumer forks the semantic model its producer owns.
- Appearance identity rides content addressing whole: every plane of one set publishes under that set's key, so one directory carries the whole set.
- Persisted appearance bytes mint on the deterministic lane alone, since a second lane keying one preimage forks the address.
- Accelerator lanes produce preview products that carry no key.
- Ingest CLASSIFIES and never infers, so an unclaimed name accumulates as recorded residue.
- Defaulted conventions commit the silent-inversion defect the frozen alias table forecloses.
