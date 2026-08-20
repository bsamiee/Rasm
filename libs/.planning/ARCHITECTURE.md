# [MONOREPO_ARCHITECTURE]

Rasm carries a platform tier and a product tier: `libs/` holds independently adoptable library estates, and every app, plugin, and service composes them exactly as it takes an external package. Capability lands in the platform first, parameterized for consumers that do not yet exist; a product shell declares intent, binds host edges, and emits output.

Estates couple through defined contracts alone: no estate imports a peer, and none is a prerequisite, producer, or build-order edge for another. Tier-0 owns the law no branch or package can own and names languages alone; each branch and folder owns its own package roster, charters, and reference direction.

## [01]-[STRATA]

Stratification is the law every branch answers: ranks run `S0` upward, the dependency edge runs strictly upward, the graph stays acyclic, and shared machinery seats at the lowest stratum every consumer reaches, since seating above a consumer's reach manufactures per-folder twins. Each branch orders its own packages under that law at its `[02]-[STRATA]`, and that table is the roster a reader resolves.

- Dependency edges stay abstract — project reference, module import, composed layer, or root-bound port — and the upward law binds every spelling.
- Cross-package coupling reaches a published boundary or a content-keyed wire, never a peer's interior.
- Peers at one rank never reference each other; alignment travels seam contracts and the content-keyed wire, so each package stays usable alone.
- Counter-edges carry a value the lower stratum consumes, never an owner it imports, and member-seating rows state that qualifier.
- Plane-distinct members seat at a rank yet stay outside the runtime graph, carrying the same upward law.
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
- Host-boundary contracts name the host-free concept, since host vocabulary crossing the seam binds every peer decode to that one host's model.

## [04]-[GEOMETRY_FLOW]

Geometry, meshing, and semantic exchange each carry exactly one owner per runtime; the runtimes meet only at the contract. No concern is owned twice within a runtime, and no runtime re-implements a peer's kernel.

- Each runtime carries one geometry owner, one meshing owner, and one semantic-exchange implementation; a second is the duplication defect.
- Runtimes meet at content identity, the tessellation rail, and the appearance rail the manifest's appearance entries define.
- Decoders at the tessellation and appearance rails compose the payload rather than re-deriving it.
- Independent peer producers stay independent: a host-free geometry owner produces for its own domain rather than consuming a peer's kernel.

## [05]-[PLANNING_LIFECYCLE]

`.planning/` is a transient greenfield scaffold, not a permanent fixture. It exists to bring an under-developed folder to the decision-complete bar and dissolves as source lands; the eventual source tree is authored only when code is written.

- Greenfield packages keep their design pages inside one `.planning/` at the package root, sub-domain sub-folders mirroring the eventual source tree.
- Package roots carry the index docs alone; all planning lives under the single `.planning/`, never inside a real source sub-folder.
- Each package maps its full folder structure, planned page-less sub-domains included, so the map fuels ideas and tasks.
- Mature folders with real code carry no `.planning/`; the co-located source architecture note is the only design surface.
- Mature folders route open split, cleanup, and re-architect work to task cards in the branch `TASKLOG.md`.
- One exception stands: a genuinely new unbuilt sub-domain inside a mature package keeps its scaffold in that sub-domain folder.
- Host-boundary planning folders carry a folder `.api/` tier over host assemblies, its rows outside the branch build root under the host gate.

## [06]-[PER_LANGUAGE_ROLES]

Each branch is an independently adoptable library estate. Branch-local capability originates, composes, operates, and evolves from that branch's packages, manifests, generated bindings, and toolchain.

- Each row names the domain its estate carries; rank and dependency sit outside the row.
- C# carries the host-bound AEC domain: the geometry kernel, the element seam and its AEC peers, the app platform, and the host boundaries.
- Python carries the host-free science, compute, data, geometry, exchange, and artifact domain.
- TypeScript carries the host-free web, edge, runtime, persistence, security, UI, and deployment domain.
- Each branch composes, mints, verifies, and deploys its own schema state — schema, migration, recovery, evidence — with no peer branch present.
- Cross-language composition adds peers to a complete branch; it never supplies a branch's missing operational base.
- Domains widen at their owning branch under `[12]-[ADMISSION]`; the rows above carry no ceiling on what a branch admits next.

Within each language one organization principle holds: real higher-order domain folders (a small isolated concern folds into the bigger concept it belongs to), source-mirroring sub-domain organization, OOP capsules at boundaries, and FP-ROP internals. Each branch re-derives its topology from the finalized owner set, never from a stale layout.

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

Four-signal telemetry (metrics, logs, traces, profiles) correlates across the runtimes through rows each branch transcribes identically in meaning: canonical here, transcribed at `csharp:Rasm.AppHost/Observability/telemetry`, `python:runtime/observability/telemetry`, and `typescript:runtime/otel/emit`, the name vocabulary at `typescript:core/observe/convention`. SDK trains move on split maturity channels, so no shared library carries those rows.

Transcription spells the rows per branch and `tests/contracts/` `TELEMETRY_CONVENTION` proves the branch spellings agree, so a drift surfaces at the digest and repairs at its owning branch page.

- Resource triple: `service.namespace` `rasm`, `service.name` the service row, `service.instance.id` a per-process mint.
- Detector rows enrich the minted triple, never replace it; deployment-time resource overrides win the merge.
- Metric names: dotted `rasm.<domain>.<measure>` under UCUM units, no baked `_total` or unit suffixes.
- `<domain>` names the capability subject a query joins on, never the package, host, or verb emitting it.
- Runtimes serving one subject share the segment and `service.name` separates the emitters; a package spanning two subjects emits under both.
- Subject test: subjects survive a second emitter, so a segment spelled like its package or host stands.
- Segments two branches claim carry one subject spelling both transcribe byte-identical, so the roster reads as one vocabulary, never three.
- Domain rows stay branch-declared at each conformance minter; a Tier-0 instance roster re-anchors the vocabulary to one day's segment set.
- Each row carries its segment beside the admitted subject.
- Emitter rosters stay branch-local, earned where a branch's own module declaration closes them against drift.
- Carve: estate identity dimensions are the one unsegmented spelling the grammar admits.
- Carve: package-keyed hook-point ids beside a runtime's own module namespace sit outside the grammar whole.
- Carve: transport coordinates beside residence relation and session-setting names share the prefix and answer to their own owners.
- Gate: every signal name a branch mints — instrument, dimension, resource, span, log, and event alike — carries a segment the roster resolves.
- Unresolved segments refuse at their declaration owner rather than reaching an exporter.
- Grammar-derived routing facts state their derivation at each emission owner, never the census owner alone; a foreign prefix forks knowingly.
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
- Absent entries on any sometimes-absent dimension — tenant, level-family, or substrate key — read as the untagged whole, never as a sentinel value.
- Sometimes-absent keys stay rostered on every branch allow-list and census; an unrostered key strips from the entries that do carry it.
- Keyed families whose key is the cell's own identity never construct the absent-key state; an unmeasured cell publishes nothing, spelling absence.
- Receipts stay the truth: signals project from typed receipts through each branch's one instrument owner.
- Metrics minted beside a receipt fan are a second truth.

[HOOK_PLANE]: domain code fires typed facts at package-qualified `rasm.<pkg>.<domain>.<point>` rows on one scoped hook registry per runtime (the kernel signal capsule's `HookPoint` roster for C#, `python:runtime/observability/hooks`, `typescript:core/observe/tap`) under one closed `veto` | `observe` | `replay` modality vocabulary. Registration is composition-unique, a subscriber fault isolates as typed evidence with the emitter's value untouched, and a signal emitter is an observe subscription over fired facts, never an emit inside a domain fold.

[TENANT_COST_JOIN]: per-tenant cost attribution is one three-pin join: the C# grant-spend instrument family off the broker's cost vectors, the SDK-side `rasm.tenant` promotion every runtime registers (the one gate; no collector processor re-mints the dimension), and the deploy plane's cost read against the selected store row. Past a store row's tenant series cap, attribution rides exemplar-sampled traces as trace-scoped spend evidence, never a second metering pipeline.

[EVIDENCE_RESIDENCE]: telemetry is an analytics subject in every branch: signal evidence lands in a durable columnar residence through the branch's own analytics custodian: `csharp:Rasm.Persistence`, `python:data/tabular` beside the `python:runtime/observability/journal` plane, and `typescript:data`. `spec.profile.observe.analytics` arms the residence family at `typescript:iac/operate/observe` (a cold-tail default, an interactive escalation, or a declared evidence loss), so evidence outliving a store's series window is a spec flip, never a per-branch pipeline.

[FLEET_ESCALATION]: every escalation row (scale-out store, broker-buffered leg, tail-sampling gateway, per-app agent topology) is off at estate scale by ruling; each arms as one spec value against `typescript:iac/operate/observe`, whose rows own the coordinates, so re-arming is a named flip, never a re-design.

[PROFILE_SWAP]: profiles migrate from vendor push onto the OTLP profiles signal by row replacement, armed only when the signal reaches stable across the SDK trains; span-profile correlation processors, the profile store row, and every dashboard survive the swap unchanged. Swap-point owners `csharp:Rasm.AppHost/Observability/telemetry`, `python:runtime/observability/profiles`, `typescript:runtime/otel/profile`, and `typescript:iac/operate/observe` each carry their own push row and its OTLP replacement.

## [09]-[SCHEMA_STATE]

Schema state is an infrastructure contract: every branch composes its own schema contract from its own framework artifacts and mints its own generation identity, so a single-language application declares, materializes, proves, and deploys its schema with no peer present.

- Contract shape, canonicalization, artifact ordering, and generation-identity derivation live in the `BACKEND_CONTRACT` corpus entry.
- That entry names each branch's minting anchor, and every branch spells the entry's law in its own types rather than sharing an implementation.
- Polyglot composition merges branch contributions at the application root by artifact key under one deterministic order.
- Merged generation is the deployment unit; a single-language application deploys the generation it minted alone, with no merge step.
- Runtime verifies the generation it observes and never mutates it; a digest change replaces the generation whole.
- Provider adapters, migration execution, protocol evolution, journal identity, and work identity stay branch-owned.
- Deployment publishes only a generation proved against the corpus schema.
- Recovery grades one verdict on two proofs — contract identity that the store carries the composed generation, and data recency of its frontier.
- Recovery windows derive from the observation's own stamps, so no provider hands in a lag it measured against a clock the verifier never saw.
- Absence splits opposite ways: an unmeasured recovery point refuses, while an absent bounce time passes on a store that never restored.
- Each branch's recovery owner gauges its measured window against the supplied profile row's objective, never a branch-local durability table.

## [10]-[CONSUMPTION_MODEL]

Every `libs/` package is an independently versioned library an unrelated application depends on exactly as it depends on any external package. One branch serves dozens of unrelated consumers at once (single-tenant and multi-tenant, in-host and headless, sidecar and companion and service and edge and CLI), so a package assumes no consumer, no sibling set, no deployment shape, and no lifecycle owner. Deployment shape arrives as data: the composition root supplies one profile row over the axis roster, and the package reads it.

| [INDEX] | [AXIS]      | [FORM] | [VOCABULARY]                                                          |
| :-----: | :---------- | :----- | :-------------------------------------------------------------------- |
|  [01]   | `tenancy`   | closed | `none` \| `single` \| `multi`                                         |
|  [02]   | `topology`  | closed | `in-host` \| `sidecar` \| `companion` \| `service` \| `edge` \| `cli` |
|  [03]   | `host`      | open   | consumption-descriptor rows the owning branch supplies                |
|  [04]   | `lifecycle` | closed | `caller-owned` \| `package-owned`                                     |
|  [05]   | `isolation` | closed | `in-proc` \| `thread` \| `process` \| `wasm` \| `remote`              |
|  [06]   | `providers` | open   | consumption-descriptor rows the owning branch supplies                |

- Closed axes fix their value vocabulary here; an open axis fixes the descriptor shape alone, and each of its rows is capability one branch supplies.
- Host integration lands as one descriptor row on the `host` axis, supplied by the branch whose runtime reaches that host.
- Open-axis instance rosters live at the supplying branch; a Tier-0 enumeration re-anchors the roster to whatever set exists.
- Each branch spells the roster in its own types, and `tests/contracts/` `CONSUMPTION_PROFILE` proves parity like any other infrastructure contract.
- Growth lands one Tier-0 row beside one row per branch for a new axis, one case on its owning closed axis for a new value.
- Open axes grow at their supplying branch alone.
- Axis values stay data — a compile-time assumption, ambient global, build flag, or a branch on the host re-mints the assumed consumer roster.
- Packages unable to serve an axis value refuse at admission with typed evidence naming the axis, never degrading or narrowing their surface.
- Sibling presence rides an axis value — a package composes a sibling through a declared port the composition root binds.
- Unbound ports read as a refused capability, never a crash.

[CONSUMPTION_DESCRIPTOR]: one row shape serves every open-axis family, so a new family is a column set and a new engine is a row. Universal columns earn their seat by the whole-family test (every family already answers them), and a coordinate one family alone decides stays that family's extension.

| [INDEX] | [COLUMN]   | [ANSWERS]                                                                   |
| :-----: | :--------- | :-------------------------------------------------------------------------- |
|  [01]   | `fits`     | the selection sentence a composition root picks this row on                 |
|  [02]   | `admit`    | the entry that puts something in, named as the row's own member             |
|  [03]   | `tenancy`  | the mechanism this row separates tenants by, in its own vocabulary          |
|  [04]   | `lifetime` | how long what entered survives, and which owner ends it — one alone is half |
|  [05]   | `degrade`  | what the row gives up against the coordinates its family names              |

- Each branch transcribes the columns in its own casing law, and a family renaming one forks the coordinate a reader crosses families to compare.
- Column values are the row's own vocabulary, so a value never re-mints a closed axis's roster and a mechanism never earns a column of its own name.
- Leaves unable to reach the axis owner name a seating defect, never a license to re-mint — the roster seats where every consumer reaches it.
- Coordinates uniform across a family — answered alike, or decided by no row in it — ride that family's lead sentence instead of a column.
- Deciding nothing is a whole answer owing `degrade` nothing, since `degrade` names forfeits and an undecided coordinate forfeits none.
- `degrade` derives from the capability columns that express the forfeit, and a stated residual carries only what no column expresses.
- Foreclosed coordinates ride the row as a value pinned false; a type-level constant or an omission strands the fold reading them beside siblings.
- Each row's key is its identity, never a coordinate, so ordinal key order stays the canonical wire order.
- Family extension columns and the coordinate each family forecloses land as one `RULINGS.md` `[02]-[SHAPE]` row, under the same anchoring law.

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

Surface appearance crosses the runtimes as two domain documents under one frozen vocabulary. `tests/contracts/appearance-vocabulary.schema.json` is the definition and `tests/contracts/MANIFEST.md` `[01]-[LEDGER]` states what it carries; each branch transcribes that definition in its own casing law and validates its own projection against it, and the two corpus entries reference it, neither restating a row.

| [INDEX] | [OWNER]                             | [MINTS]                                    | [BOUNDARY]                             |
| :-----: | :---------------------------------- | :----------------------------------------- | :------------------------------------- |
|  [01]   | `csharp:Rasm.Materials/Raster`      | the baked set behind the appearance key    | carries no environment kind            |
|  [02]   | `csharp:Rasm.Materials/Appearance`  | the environment light and stage vocabulary | branch-interior; takes no corpus entry |
|  [03]   | `python:artifacts/graphic/texture`  | the ingest and environment set manifest    | presses no graph, holds no bake key    |
|  [04]   | `typescript:core/interchange`       | nothing; it lands both documents           | derives no field a producer carries    |
|  [05]   | `typescript:data` + `typescript:ui` | the transform, serve, and bind pipeline    | reads addresses, constructs none       |
|  [06]   | `csharp:Rasm`                       | the analytic and raster atoms both compose | holds no channel vocabulary            |

- Two documents, two producers, one vocabulary: each producer owns its own document and `domain` entry, and routes a kind it lacks to the peer entry.
- Capability overlaps freely and documents do not — both producers derive geometric channels and both encode containers.
- Producers emitting the peer's document class commit the second-producer defect the `domain` class forecloses.
- Consumption is never production, and a branch decoding both documents mints neither.
- Deriving one appearance field from another at a consumer forks the semantic model its producer owns.
- Appearance identity rides content addressing whole: every plane of one set publishes under that set's key, so one directory carries the whole set.
- Persisted appearance bytes mint on the deterministic lane alone, since a second lane keying one preimage forks the address.
- Accelerator lanes produce preview products that carry no key.
- Ingest classifies and never infers, so an unclaimed name accumulates as recorded residue.
- Defaulted conventions commit the silent-inversion defect the frozen alias table forecloses.

## [14]-[EVENT_FABRIC]

Domain facts cross the runtimes as CloudEvents message envelopes under one attribute grammar, one extension roster, and one format contract, each branch transcribing them in its own casing law. Message envelopes announce a fact and never gain authority over it: the producing receipt stays the evidence truth and the announcement projects it, so a consumer routes on attributes without opening the payload.

[AUTHORITY]: the specification is the semantic owner and an SDK is an admitted acceleration. Every binding, format, filter, and extension row derives from the specification, delegating to an SDK member where one exists and standing branch-owned where none does. Package surfaces narrower than the specification state a fact about that package, never a ceiling on what the estate carries.

[KERNEL_BOUNDARY]: the message envelope, the extension roster, the attribute grammar, and the format contract reach S0, and nothing else does. Bindings, filters, and subscriptions seat at their consuming owners under `[12]-[ADMISSION]`, carry no seam constraint, and reach the message envelope as consumers of it.

[SEATING]:
- C# seats the message-envelope algebra inside `Rasm`'s `Domain` sub-domain beside its identity, rails, and telemetry owners.
- TypeScript lands the message envelope as rows on the `core/interchange` owners it already carries, and mints no page beside them.
- Python lands the message envelope and every binding under `runtime/transport`, and mints no eventing sub-domain.
- Sub-domains minted for the message envelope fail the `[12]-[ADMISSION]` earn-test — their nouns are the identity, receipt, and wire nouns S0 holds.

[GRAMMAR]: one row per attribute, spelled in each branch's own casing law.

| [INDEX] | [ATTRIBUTE]       | [CARRIES]                                 |
| :-----: | :---------------- | :---------------------------------------- |
|  [01]   | `type`            | `rasm.<domain>.<subject>.<fact>.v<N>`     |
|  [02]   | `source`          | the producing capability's URI-reference  |
|  [03]   | `subject`         | the payload's content key                 |
|  [04]   | `id`              | the producer's operation identity         |
|  [05]   | `time`            | the occurrence instant, RFC 3339          |
|  [06]   | `recordedtime`    | the receiver's ingest instant             |
|  [07]   | `dataschema`      | the registry subject and version          |
|  [08]   | `datacontenttype` | the serdes arrow's own row data           |
|  [09]   | extension names   | lowercase `[a-z0-9]` within 20 characters |

- `<domain>` is the capability subject `[08]-[OBSERVABILITY_CONFORMANCE]` fixes for metric names, so a board and a subscription join one vocabulary.
- That subject and the package segment a `[HOOK_PLANE]` id spells are two grammars, so a `type` derives from the roster, never the firing hook id.
- `<fact>` reads past tense, and `v<N>` moves only on a breaking `dataschema` change, so a compatible widening leaves every subscription standing.
- `source` names the producing capability, never a host, package, or deployment, since a redeployment re-authors the identity consumers keyed on.
- `id` is operation identity and never a content digest, and `(source, id)` is the uniqueness composite every dedup and idempotency key reads.
- `time` mints at the branch clock owner and `recordedtime` at the receiver, so the pair measures the queue that collapsing them erases.
- `subject` carries the content key in one spelling, the same spelling `dataref` publishes where the payload externalizes.
- Peer extension names unknown or past the ceiling are ignored, never a whole-message fault.
- Extension-map digests read a canonical order the message-envelope owner publishes, under `docs/laws/scars.md` `[DIGEST_OVER_UNORDERED_CONTAINER]`.

[EXTENSION_ROSTER]: each branch spells the roster once and hands it at construction and at every decode, since a decoder without it reads a declared extension as an unknown string. Where an SDK helper owns a row the branch composes that helper's own `AllAttributes`, never a hand-spelled twin beside it.

| [INDEX] | [EXTENSION]                | [CARRIES]                                    |
| :-----: | :------------------------- | :------------------------------------------- |
|  [01]   | `traceparent` `tracestate` | the creation-time W3C trace                  |
|  [02]   | `baggage`                  | the creation-time W3C baggage                |
|  [03]   | `partitionkey`             | the member a transport partitions on         |
|  [04]   | `sequence` `sequencetype`  | the per-source position and its domain       |
|  [05]   | `sampledrate`              | the producer's sampling denominator          |
|  [06]   | `dataref`                  | the externalized payload's content key       |
|  [07]   | `dataclassification`       | the handling class gating each binding       |
|  [08]   | `recordedtime`             | the receiver's ingest instant                |
|  [09]   | `expirytime`               | the instant past which delivery is moot      |
|  [10]   | `severity`                 | the fact's own operational grade             |
|  [11]   | `correlation`              | the causal chain a consumer joins on         |
|  [12]   | `deprecation`              | the superseding `type` and its window        |
|  [13]   | `authcontext`              | the producer's asserted principal            |
|  [14]   | `dssematerial`             | the DSSE material over the attribute digests |

[TWO_TRACE]: the distributed-tracing extension carries the CREATION-time trace and the transport carrier carries the CURRENT hop, so both ship and folding either onto the other loses the leg it alone records.

[VARIATION]: format, binding, filter, and content mode are rows on their owners, never types a consumer switches on; growth is one row, arm, or case, and every consumer stands untouched.

[DATAREF]: one policy row per binding and never a global constant, because a threshold fixed estate-wide either strands the smallest transport or wastes the largest. `ref` is the digest under `docs/laws/patterns.md` `[CONTENT_KEY]`, published under the addressing law `[13]-[APPEARANCE]` states for a content-keyed set.

| [INDEX] | [COLUMN]    | [ANSWERS]                               |
| :-----: | :---------- | :-------------------------------------- |
|  [01]   | `threshold` | the binding's own negotiated limit      |
|  [02]   | `residence` | the content-keyed store bound as a port |
|  [03]   | `ref`       | the content key in the chosen spelling  |
|  [04]   | `retain`    | the declared retention class            |
|  [05]   | `dual`      | whether the reference ships alone       |

- `residence` binds at the composition root as a port, and an unbound port refuses at admission rather than shipping a reference nothing resolves.
- `retain` declares a class and never a window, ledger, or groom, so the producing folder's standing obligation reaches the wire unchanged.
- `dual` gates reference-alone shipping on the subscription's `protocolsettings`, since the specification carries no capability negotiation.

[BATCH]:
- Batches settle per event, and the receipt carries accepted beside matched-duplicate as separate halves.
- `sequence` survives batching, and no re-batch reorders events inside one `source`.
- Framing reads the batch media-type prefix, so a format's batch sibling needs no second dispatch.
- Batches past the transport budget split at the producer, since a relay re-framing one cannot re-sign it.

[HOOK_ORDER]: a message-envelope emitter is an `observe` subscription over fired hook facts, never an emit inside a domain fold; `[08]-[OBSERVABILITY_CONFORMANCE]` `[HOOK_PLANE]` owns the modality vocabulary, and this join fixes the emitter's rung on it.

[AXIS_REFUSAL]: a binding a deployment cannot serve refuses on the `providers` open axis as one `[10]-[CONSUMPTION_MODEL]` `[CONSUMPTION_DESCRIPTOR]` row, since a boolean knob re-mints the assumed consumer that roster forecloses.

[SECURITY]: signing is DSSE over SHA-256 digests of the core and extension attribute sets, carried in the `dssematerial` binary extension; the format registry carries no JWS member, so a signature travels as an attribute or not at all.
- Webhook legs sign the encoded bytes once, before any reserialization, since a re-encode respells what the signer never saw.
- Abuse protection rides the HTTP `OPTIONS` handshake — `WebHook-Request-Origin` required, `WebHook-Request-Callback` and `-Rate` optional.
- Targets answer `WebHook-Allowed-Origin` beside `WebHook-Allowed-Rate`, and a target handling `OPTIONS` while declining validation answers 405.
- `WebHook-Request-Origin` rides every delivery request, so a target re-reads the claimed origin per message rather than trusting one handshake.
- Authorization uses the specification's own header field or query parameter, since a third scheme forks what every peer target implements.
- Ingress admits through the tenancy owner and inherits nothing, so a decoded message envelope carries no authority its transport happened to hold.
- `source` and `authcontext` are producer claims verified against the trust row before any routing decision reads them.
- `dataclassification` gates which binding a fact crosses, so a classification a binding cannot honor refuses at that binding.

[EVOLUTION]:
- `dataschema` resolves the registry subject and its version, and the `type` major `v<N>` moves with that version rather than beside it.
- Divergent generations refuse at the CONSUMER on every decode, so a producer never negotiates a peer's pinned generation downward.
- Retiring `type` values carry `deprecation` for a window that is a policy row, never a date literal a page goes stale against.

[OWNERS]: one message-envelope owner per branch, and a second mint inside one branch is the `[07]-[CROSS_LANGUAGE_WIRE]` drift defect.

| [INDEX] | [BRANCH]   | [ENVELOPE_SEAT]               | [BINDING_SEAT]             |
| :-----: | :--------- | :---------------------------- | :------------------------- |
|  [01]   | C#         | `csharp:Rasm/Domain`          | its consuming package      |
|  [02]   | Python     | `python:runtime/transport`    | `python:runtime/transport` |
|  [03]   | TypeScript | `typescript:core/interchange` | its consuming package      |
