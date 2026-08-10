# [LIBS_RULINGS]

`libs/` rulings settle estate-spanning decisions.

## [01]-[PACKAGES]

- CloudEvents semantics belong to the specification and an SDK accelerates it — a narrower package surface written as law forecloses spec capability.
- `CloudNative.CloudEvents.Mqtt` rejected — it pins MQTTnet 4.x against the estate's 5.x, reads a set-only `PayloadSegment`, and is structured-only.
- `CloudNative.CloudEvents.NATS` rejected — it targets net6.0 on the retired `NATS.Client` v1, so the NATS binding stays branch-owned in all three.
- `CloudNative.CloudEvents.AvroEventFormatter` never lands — the `.Avro` namespace formatter is the one Avro codec, and the shim ships obsolete.
- `CloudNative.CloudEvents.NewtonsoftJson` rejected — the System.Text.Json formatter owns JSON, and a second formatter forks the codec identity.
- Python Kafka is `confluent-kafka` — one librdkafka engine serves all three branches, and a pure-Python client forks semantics peers prove against.

## [02]-[SHAPE]

- Infrastructure planes take shape from the category algebra, never the first consumer — the occasioning domain enters as a ROW like every later one.
- Branch-interior wires mint no `MANIFEST.md` entry — the `[WIRE]` pair at both folder `[03]-[SEAMS]` maps IS the whole registration.
- Each branch composes its backend contract from its own artifacts and mints its own generation — a shared minter re-imposes the removed prerequisite.
- Branch prose states its own domain and never a peer's ceiling — a graduation rail names what crosses and leaves policy to whoever reads it.
- PostgreSQL extension rosters stay branch-owned deployment state — no corpus entry defines the set, so a parity demand manufactures meaning.
- Backend artifact dependency validation homes at each branch's projection funnel, never its sort — artifact key order is the whole wire order.
- Library tiers stay app-neutral — instrumentation binds the API surface alone, and no exporter, broker, or sink enters below the composition root.
- Columnar-lake query ends ride ONE Flight plane per runtime, Flight SQL layered as a dialect — a sidecar transport re-derives admission and typing.
- Retrieval fusion stays host-local — each end folds reciprocal-rank fusion in one database statement, and no cross-runtime projection exists.
- Reliability-indicator vocabulary is one algebra spelled twice — `Sli`, burn row, severity, and panel kind land in BOTH branch spellings.
- Success share rides one tag-partitioned counter as an indicator case — a twin carrying the numerator strands its denominator on any emission edit.
- Telemetry and event domain segments name capability subjects and stay branch-declared — a segment minted per emitter forks the series a board joins.
- Telemetry conformance proves through `TELEMETRY_CONVENTION` alone — parity across the three mints IS the proof; naming similarity gates nothing.
- OTLP is the ONE metrics-store ingest door and remote-write is declined estate-wide — a second leg forks the ingest contract per store.
- Analytics-residence families add no descriptor column and foreclose `cap` — unbounded dimensionality is exactly what a cardinality budget destroys.
- Descriptor families extend with what their own plane decides — a plane answering a coordinate it decides nothing about answers by guess.
- Transport families extend the descriptor columns with `deliver`, `order`, `settle`, `replay`, `bound`, and `refuse` — six an engine decides alone.
- Transport families foreclose `retry` — each row names its retry owner, since one owner holds every schedule and a row carrying one forks it.
- `order` carries the ordering domain beside the key member selecting it — a partition key IS that member, never a column standing beside it.
- Transports state a missing coordinate on `degrade` — replay, backpressure, and settlement each go absent on engines the estate already ships.
- Operation identity derives apart from payload identity — equal payloads stay distinct operations, so a causal log never keys an entry on content.
- Wires pinning divergent schema or `dataschema` generations refuse at the CONSUMER — proto3 files a retired field to the unknown set, never a raise.
- One descriptor source carries ONE minter — a peer's messages seated in another family couple two release cadences under one breaking gate.
- Host-boundary producers name their family for the host-free concept — a host-spelled family binds every peer's generated bindings to that host.
- Every branch shipping a diagnostic archive carries a BACKEND-FREE read plane — an archive is pulled exactly when the egress is what failed.
- Unmeasured instruments read UNMEASURED, never zero — a fabricated zero and a dead producer are the two states an operator needs separated.
- Unbounded append retry admits only behind content-keyed idempotence AND a rail whose faults are transient by TYPE — both are preconditions.
- Landings and batch settlements split accepted from matched-duplicate — one merged tally cannot tell zero redelivery from a wedged retry.
- Never-shedding planes close by FLUSHING — cancelling the consumer at teardown abandons the window in flight and everything queued behind it.
- Unlanded rows stay readable as a roster settled by identity — a tally names nothing to re-offer, and clearing on first success sheds the debt.
- Evidence planes settle on their own stamped coordinate, never a storage write clock — that stamp lifts into its own indexed column.
- Exact-decimal claims start at the ACCUMULATOR, never the multiply — a float or 53-bit sum loses the value before the decimal carrier ever sees it.
- Pulled levels bind to their OWNER's lifetime, never a name — a dead owner's last value otherwise freezes into a level reading as live.
- Columnar DDL plants at the tier that installs the residence and whose writer fills the relation — the planter leads its sort key with tenant.
- Reliability rendering rides the deploy tier's realized backend projection — an objective naming a residence is a contradicting second selection.
- HLC carriage is per-branch and only the layout is shared — the packed `physical_ticks<<64 | logical` `UInt128` and the composed attribute slots.
- Shared byte layouts fix their UNIT — the HLC physical half is 100-ns ticks, so a minter reading milliseconds SCALES rather than transcribes.
- Layout value domains bind at the MINT, not the cell — the HLC physical half is I63 inside a `uint64` slot, and the cell admits what none writes.
- Cross-package references to a twice-carried name scope-qualify at the reference site — the ambiguity is the citation's defect, never the owner's.
- Producing folders declare a fact's `Retain` CLASS and never a window, ledger, or groom — the plane arrives bound at the composition root.
- Append-only log planes own truth and derived planes carry zero authority — a crossing shape names its truth plane and its rebuild route.
- Registered member rosters claim what their fences must honor — zero call sites repairs to a consumer or a recorded negative, never removal.
- Vector clocks encode with slots SORTED by origin — bucket order gives one causal position a digest per runtime, unfreezing every fixture over it.
- Causal-log replay dedups on operation IDENTITY — a content test reads a second edit of identical bytes as redelivery and discards it.
- Commutation is ONE `Ordered | Commutative | Semilattice` triple per mutation kind — a lone convergent flag counts a lost total order as convergence.
- Compaction admits only where the entry's causal context DOMINATES its declared horizon — a state fold holds no frontier and refuses nothing.
- Op-log entries carry no descriptor family — the msgpack roster IS canonical, and a proto message envelope over opaque payload bytes types nothing.
- Scene descriptors and event payloads REFERENCE bulk bodies by key — re-carrying the octets mints a second producer for the entry that owns them.
- Descriptor lengths cross in METRES with the host unit as provenance — a peer-side rescale forks one conversion across three decoders.
- Solved solar angles cross as `[0,360)` azimuth EAST OF NORTH beside `[-90,90]` altitude — a renderer's own convention converts at its edge.
- Descriptor angles ARE the derivation — a consumer re-solving them substitutes a second almanac's answer for the same instant.
- Sited and authored suns are the whole solar discriminant — a manually posed sun carries no site, so no consumer back-solves coordinates.
- Tessellation fidelity crosses DECLARED and consumers GRADE it — a payload silently coarser than a consuming tolerance reads as geometry.
- Descriptor bands split incomplete from wrong — an unusable column counts onto the receipt and only a wrong result refuses the crossing.
- Recovery frontier lag admits at ZERO, the freshest measured recency — a stamp AFTER its observation is skew, dropping to unmeasured and refusing.
- Host families extend with `recovery` and provider families foreclose it — a bound port supplies capability and decides no durability window.
- Magnitudes cross EXACT and presentation stays the consumer's — a producer-rounded string draws a sub-micron tolerance as an unachievable zero.
- Composite identities cross WHOLE, discriminating components beside the digest — a digest-only join merges two identities minted over equal bytes.
- Framed-binary wires frame an `Option` as a count of 0 or 1 — presence on the collection rule leaves an absent field no second spelling to fork on.
- Kind-discriminated payload tails carry NO arm tag — the discriminant in hand fixes the tail, and a second spelling of it can contradict the first.
- One message-envelope owner per branch mints the attribute map — a second mint in one branch is the `[07]` drift defect the corpus class forecloses.
- Format roster is JSON, Protobuf, and Avro with each `-batch` sibling — CBOR, XML, and avro-compact stay drafts no peer decode is held to.
- CloudEvents WebSockets is declined, and Pulsar, Redis Streams, and ClickHouse stay house message-envelope legs — no spec binding names them.
- Subscriptions and all seven filter dialects land, `sql` being CESQL — reading CESQL as the filter concept collapses six dialects into one arm.
- CESQL evaluation is TOTAL — every operator returns a value beside an accumulated error list, so the rail accumulates rather than short-circuits.
- CESQL parses through a parser-combinator or table-driven expression owner — recursive descent over mutable state and downloading codegen refuse.
- Signing is DSSE over the attribute digests, never JWS — the format registry carries no JWS member, so a signed event rides a type nothing resolves.
- Event identity rides `id` and content identity `subject` and `dataref` — `(source, id)` is the uniqueness composite, so no dedup keys on bytes.
- `subject` and `dataref` carry the content key as 32 LOWERCASE hex — an upper-spelling branch maps at the envelope edge, re-casing no shared codec.
- `ContentAddress` is a bare digest and `ContentKey` a kind-discriminated composite — neither renames onto the other, and a citation scope-qualifies.
- `dataschema` binds the registry subject and version, `datacontenttype` is row data off the serdes arrow — a literal content type forks codec choice.
- Branches mount a decoder only for families they decode themselves — a family whose one consumer is a peer branch reads bytes it never receives.
- Hook points are the in-process best-effort tap and message envelopes the durable cross-process fact — re-firing one as a hook merges two custodies.

## [03]-[COLLAPSE]

- (none)

## [04]-[STRUCTURE]

- `region-map/` dirs and `api-catalogues.md` do NOT re-enter — the folder `ARCHITECTURE.md` codemap and the two-tier `.api/` catalogs own that truth.
- Per-folder `FEATURES.md` and `existing-work.md` do NOT re-enter — the README router and `TASKLOG.md` own file navigation and work state.
- Per-folder `[OWNER_REGISTRY]` and `[DEPENDENCY_DIRECTION]` ledgers do NOT re-enter — the branch `ARCHITECTURE.md` states dependency direction once.
- Standalone seam ledgers do NOT re-enter — the folder `ARCHITECTURE.md` `[03]-[SEAMS]` is the one seam record, mirrored at both endpoints.
- SPIKE owner-state tracking does NOT re-enter — a SPIKE marker rides its design page beside the deterministic floor, never a state registry.
- Folder `ARCHITECTURE.md` seats `[02]-[STRATA]` between `[01]-[DOMAIN_MAP]` and `[03]-[SEAMS]` — corpus-canonical, never folder-specific.
- `ONE_WIRE_FIXTURE_CORPUS` does NOT re-enter — `tests/contracts/MANIFEST.md` is the one federation index, pin authority, and fixture registry.
- Broker deployments do NOT enter `iac` — addresses are operator-supplied, and a decode against an absent estate fails rather than provisioning one.

## [05]-[PROCESS]

- Refuted `[COMPLETE]` cards delete and re-author as a new open card under a new slug with all four fields — no re-open mechanic exists at any tier.
- `[BLOCKED]` is the TERMINAL hold naming a live upstream blocker beside its arms and route — a corpus at zero open work still carries it.
- Holds close by re-running their own route — deleting one discards the probe that reverses it, and the next pass re-derives the blocker.
- Blockers an in-pass probe clears were never holds — the card re-marks `[QUEUED]` and lands in that same pass.
