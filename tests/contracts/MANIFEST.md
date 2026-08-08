# [CONTRACTS_MANIFEST]

Corpus entries bind each contract to its class: an `infrastructure` entry names every branch anchor that mints the shape, and a `domain` entry names the one producer that emits it. Content-addressed fixtures key on the `docs/laws/patterns.md` `[CONTENT_KEY]` law; pin state records whether the byte-deriving input is frozen.

## [01]-[LEDGER]

| [INDEX] | [FIXTURE]                 | [SEAM]                      | [CLASS]        | [PAYLOAD]                       | [PIN]      |
| :-----: | :------------------------ | :-------------------------- | :------------- | :------------------------------ | :--------- |
|  [01]   | CANONICAL_BYTE_IDENTITY   | `content-identity`          | infrastructure | `wire-bytes` + `digest`         | DESIGN-PIN |
|  [02]   | MESH_ADJACENCY_GOLDEN     | `mesh-adjacency`            | domain         | `wire-bytes` + `digest`         | REAL       |
|  [03]   | MATERIAL_LAYER_GOLDEN     | `material-layer`            | domain         | `wire-bytes` + `digest`         | DESIGN-PIN |
|  [04]   | FAULT_TRIPLES             | `fault-triples`             | infrastructure | `wire-bytes` + `canonical-json` | DESIGN-PIN |
|  [05]   | CRDT_OP_SET               | `crdt-op-set`               | infrastructure | `wire-bytes`                    | DESIGN-PIN |
|  [06]   | GLB_BY_KEY                | `glb-by-key`                | domain         | `wire-bytes` + `digest`         | DESIGN-PIN |
|  [07]   | HLC_TWO_HALF              | `hlc-two-half`              | infrastructure | `wire-bytes`                    | DESIGN-PIN |
|  [08]   | IFC_WIRE                  | `ifc-wire`                  | domain         | `wire-bytes` + `digest`         | DESIGN-PIN |
|  [09]   | DESCRIPTOR_DRIFT          | `descriptor-drift`          | infrastructure | `descriptor-set`                | DESIGN-PIN |
|  [10]   | CONSUMPTION_PROFILE       | `consumption-profile`       | infrastructure | `canonical-json`                | DESIGN-PIN |
|  [11]   | BACKEND_CONTRACT          | `backend-contract`          | infrastructure | `canonical-json` + `digest`     | DESIGN-PIN |
|  [12]   | CAPABILITY_DESCRIPTOR     | `capability-descriptor`     | domain         | `canonical-json` + `digest`     | DESIGN-PIN |
|  [13]   | TELEMETRY_CONVENTION      | `telemetry-convention`      | infrastructure | `canonical-json` + `digest`     | DESIGN-PIN |
|  [14]   | BENCHMARK_CLAIM           | `benchmark-claim`           | infrastructure | `wire-bytes`                    | DESIGN-PIN |
|  [15]   | HOST_FINGERPRINT          | `host-fingerprint`          | infrastructure | `canonical-json` + `digest`     | DESIGN-PIN |
|  [16]   | BOARD_PACK                | `board-pack`                | infrastructure | `canonical-json` + `digest`     | DESIGN-PIN |
|  [17]   | TEXTURE_SET_BY_KEY        | `texture-set-by-key`        | domain         | `wire-bytes` + `digest`         | DESIGN-PIN |
|  [18]   | ASSET_SET_MANIFEST        | `asset-set-manifest`        | domain         | `wire-bytes` + `digest`         | REAL       |
|  [19]   | MATERIAL_WIRE             | `material-wire`             | domain         | `wire-bytes` + `digest`         | DESIGN-PIN |
|  [20]   | SIGNED_ARTIFACT           | `signed-artifact`           | domain         | `wire-bytes` + `digest`         | DESIGN-PIN |
|  [21]   | APPHOST_WIRE              | `apphost-wire`              | domain         | `canonical-json` + `digest`     | DESIGN-PIN |
|  [22]   | APPUI_WIRE                | `appui-wire`                | domain         | `canonical-json` + `digest`     | DESIGN-PIN |
|  [23]   | BIM_WIRE                  | `bim-wire`                  | domain         | `canonical-json` + `digest`     | DESIGN-PIN |
|  [24]   | ELEMENT_WIRE              | `element-wire`              | domain         | `wire-bytes` + `digest`         | DESIGN-PIN |
|  [25]   | ELEMENT_CORPUS            | `element-corpus`            | domain         | `wire-bytes` + `digest`         | DESIGN-PIN |
|  [26]   | DECLARATION_RECORD        | `declaration-record`        | domain         | `canonical-json` + `digest`     | DESIGN-PIN |
|  [27]   | HDF5_FIELD_CONTAINER      | `hdf5-field-container`      | domain         | `wire-bytes` + `digest`         | DESIGN-PIN |
|  [28]   | HDF5_GRADUATION_ENVELOPE  | `hdf5-graduation-envelope`  | domain         | `wire-bytes` + `digest`         | DESIGN-PIN |
|  [29]   | SPARSE_EXCHANGE_CONTAINER | `sparse-exchange-container` | domain         | `wire-bytes` + `digest`         | DESIGN-PIN |
|  [30]   | GRADUATION_EVIDENCE       | `graduation-evidence`       | domain         | `wire-bytes` + `digest`         | DESIGN-PIN |

Two appearance entries — `[02.17]` and `[02.18]` — conform to one shared definition, [appearance-vocabulary.schema.json](appearance-vocabulary.schema.json): the channel roster with its per-channel transfer, neutral, unit, mip policy, and minting branches; the ingest alias table; the transfer, normal-convention, alpha-mode, container, pack, plane-format, mip-policy, and KTX2-payload vocabularies; the three hex spellings; the level-ordered plane address; the egress grammar; and the spherical-harmonic band order with its golden vectors. Neither seam restates a row of it, and a document-local re-spelling is the fork the shared definition forecloses.

## [02]-[ENTRIES]

### [02.1]-[CANONICAL_BYTE_IDENTITY]

- Seam: `content-identity`
- Class: infrastructure
- Minters: `csharp:Rasm/Spatial/reconciliation#RECONCILIATION_BRIDGE`; `python:runtime/evidence/identity#IDENTITY`; `typescript:core/value/contentKey#DIGEST_TABLE`
- Consumers: `python:runtime/evidence/reproduction#SEED_REPRODUCTION` (the `_CORPUS` parity suite); `typescript:core/value/contentKey` delegating sites `core/interchange/frame`, `runtime/browser/fetch`, `data/object/store`, with readers in `tests/typescript/_testkit`; the C# shared-corpus harness under `tests/csharp`.
- Payload: `wire-bytes` + `digest`
- Pin: DESIGN-PIN
- Blocker: no minter has frozen a payload-agnostic framed preimage; every vector on disk carries a domain payload, so the framing and seed law holds no vector of its own.
- Shape: the framed preimage `docs/laws/patterns.md` `[PREIMAGE_FRAMING]` fixes — every variable-width field length-framed, every adjacent collection count-framed, elements contiguous and unpadded — hashed by seed-zero `XxHash128` and rendered as 16 big-endian bytes at the `:x32` spelling. Every mint frames binary octets, never serialized text: the three branch JSON encoders — source-generated `System.Text.Json`, `JSON.stringify` under `Schema.parseJson`, `msgspec.json.encode(order="deterministic")` — render one value three ways in that order under every available option, spelling one double `1E+21`/`1e+21`/`1e21`, negative zero `-0`/`0`/`-0.0`, an integral float `1`/`1`/`1.0`, an integer past 2^53 exact/rounded/exact, object keys insertion-ordered/array-index-hoisted/codepoint-sorted, and a lone surrogate `U+FFFD`-substituted/`\ud800`-escaped/`UnicodeEncodeError`-raising, with `UnsafeRelaxedJsonEscaping` alone escaping supplementary-plane characters, `U+007F`, and `U+2028`. None exposes a number-format, key-order, or escape knob reaching the other two, and a bolted-on sort forks again — `StringComparer.Ordinal` and the default JavaScript comparator order by UTF-16 code unit where `order="deterministic"` orders by codepoint, inverting every astral-versus-`U+E000`–`U+FFFF` key pair. Peers therefore carry a JSON document as opaque octets, digest it as received, and decode it for semantics alone; re-encoding a decoded document and byte-comparing against the transported bytes asserts a falsehood, and a JSON-bearing contract keys its digest over the framed binary preimage of the decoded field values. Discriminating laws: two field splits that collide onto one digest under separator-joined concatenation re-hash distinctly here; a spine of fixed-width digests concatenates injectively and carries no framing. Each branch mints the stream from its own canonical writer over its own inputs, and parity across the three mints IS the conformance; the payload a branch frames sits outside this entry.
- Regenerate when: the framing law or the seed-zero content-key law changes.

### [02.2]-[MESH_ADJACENCY_GOLDEN]

- Seam: `mesh-adjacency`
- Class: domain
- Producer: `csharp:Rasm/Spatial/reconciliation#RECONCILIATION_BRIDGE`
- Consumers: `python:runtime/evidence/reproduction#SEED_REPRODUCTION` (the `_CORPUS` parity row); `typescript:core/value/contentKey` (the `hash-wasm` bit-parity gate), with readers in `tests/typescript/_testkit`; the C# shared-corpus harness under `tests/csharp`.
- Payload: `wire-bytes` + `digest`
- Pin: REAL
- Shape: the canonical-adjacency byte stream `EncodeForm.Mesh` emits — `int32`-LE `VertexCount`, `int32`-LE `EdgeCount`, `(int32-LE Min, int32-LE Max)` per sorted edge pair, `int32`-LE `FaceCount`, per lowest-vertex-rotated face cycle `(int32-LE CycleLength, int32-LE Vertex…)` — contiguous, no padding, hashed by `XxHash128.HashToUInt128` at seed zero under the `CANONICAL_BYTE_IDENTITY` framing law. Discriminating laws: a morph (moved control points, same adjacency) re-hashes identically; a topology break re-hashes distinctly. Mesh adjacency is the kernel's domain capability, so peers decode the vector and never re-derive the topology.
- Expectation: the single-triangle topology (`VertexCount=3`; edges `(0,1),(0,2),(1,2)`; face cycle `[0,1,2]`) whose 52-byte stream is `03 00 00 00 03 00 00 00 00 00 00 00 01 00 00 00 00 00 00 00 02 00 00 00 01 00 00 00 02 00 00 00 01 00 00 00 03 00 00 00 00 00 00 00 01 00 00 00 02 00 00 00`, digest `0x9462A71A5DD13DCFA3B1D6D225FCBE70`, 16-byte LE memory `70 be fc 25 d2 d6 b1 a3 cf 3d d1 5d 1a a7 62 94` — host-validated against the native `Mesh` topology surface and frozen at the producer.
- Regenerate when: the frozen canonical-adjacency field order or the seed-zero content-key law changes.

### [02.3]-[MATERIAL_LAYER_GOLDEN]

- Seam: `material-layer`
- Class: domain
- Producer: `csharp:Rasm.Element/Projection/address#CONTENT_ADDRESS`
- Consumers: `python:runtime/evidence/reproduction#SEED_REPRODUCTION` (`_CORPUS` row, `planned`-phase obligation until pinned); `typescript:core/value/contentKey` (the `hash-wasm` bit-parity gate); `csharp:Rasm.Element/Graph/wire#WIRE_CODEC` (the `MaterialLayerWire` three-runtime round-trip).
- Payload: `wire-bytes` + `digest`
- Pin: DESIGN-PIN
- Blocker: `csharp:Rasm.Element/Projection/address` lacks a concrete `MaterialComposition.LayerSet` node with a `CanonicalWriter` counted-bag digest.
- Shape: the float-bearing `IfcMaterialLayer` node's `CanonicalWriter` bytes, hashed seed-zero; this fixture alone covers the float canon. Layered material composition is the producer's domain capability, so peers decode and never re-derive the layer semantics.
- Regenerate when: the `CanonicalWriter` layout or the pinned layer node changes.

### [02.4]-[FAULT_TRIPLES]

- Seam: `fault-triples`
- Class: infrastructure
- Minters: `csharp:Rasm.Compute/Runtime/wire#FAULT_PROJECTION`; `python:runtime/transport/shapes#VOCABULARY`; `typescript:core/value/fault#CLASS_VOCABULARY`
- Consumers: `typescript:core/interchange/codec#FAULT_RAIL` maps `faultTagOf`/`FAULT_CTOR`, defaulting to `Quarantine`; `python:runtime/evidence/reproduction#SEED_REPRODUCTION` reproduces every mint in the parity suite.
- Payload: `wire-bytes` + `canonical-json`
- Pin: DESIGN-PIN
- Blocker: no minter has pinned the concrete `(package, code, case)` triple set spanning the disjoint bands.
- Shape: `FaultDetail` triples spanning ComputeFault band 2200, HopFault band 4500, the `WireFault` sub-band 4520-4532, and store/config bands at their app roots; the round-trip law reconstructs the identical literal-discriminated union from pack to decode, and neither `package` nor `code` alone is a total key. Every branch mints the triple from its own fault vocabulary, so a branch that only decodes carries no fault identity of its own.
- Regenerate when: the band allocation or the `WireFault` case roster changes.

### [02.5]-[CRDT_OP_SET]

- Seam: `crdt-op-set`
- Class: infrastructure
- Minters: `csharp:Rasm.Persistence/Version/commits#CRDT_ALGEBRA`; `python:runtime/transport/wire#CRDT_DECODE`; `typescript:core/state/merge#INSTANCE_ROSTER`
- Consumers: `typescript:core/interchange/format#MSGPACK_ENGINE` carries the envelope into `typescript:core/state/merge`; `python:runtime/evidence/reproduction#SEED_REPRODUCTION` reproduces every mint in the parity suite.
- Payload: `wire-bytes`
- Pin: DESIGN-PIN
- Blocker: no minter has pinned the MessagePack envelope or the `Beat` state encoding.
- Shape: a `CrdtOpWire` MessagePack op multiset over the `Set | Write | Add | Remove | Increment | InsertAfter | Delete | Maintain | Beat | Leave` union with the `Hlc` 16-byte cell; the convergence law folds divergent-delivery permutations of the same op multiset to byte-identical state under the join-semilattice `Merge`. Every branch authors ops as well as merging them, so each mints the union in its own types.
- Regenerate when: the `CrdtOp` union, the `Merge` algebra, or the envelope framing changes.

### [02.6]-[GLB_BY_KEY]

- Seam: `glb-by-key`
- Class: domain
- Producer: `csharp:Rasm.Compute/Runtime/codecs#TILE_PARTITION` over the GLB tessellation result, content-keyed through `csharp:Rasm.Compute/Runtime/codecs#CONTENT_ADDRESSING`.
- Consumers: `typescript:ui/viewer/scene` through `typescript:core/interchange/frame`; `python:geometry/mesh/daemon#DAEMON`.
- Payload: `wire-bytes` + `digest`
- Pin: DESIGN-PIN
- Blocker: producer must pin source geometry with `TessellationPolicy`; Bim tile-emit codec admission gates leaf-tile content emission.
- Shape: one GLB keyed by the `ContentIdentity` seed; geometry identity uses the seed-zero content key, not a policy-seeded interchange cache key. Tessellation is the producer's domain capability, and peers consume the keyed tile.
- Regenerate when: the tessellation policy, the content-key derivation, or the pinned source geometry changes.

### [02.7]-[HLC_TWO_HALF]

- Seam: `hlc-two-half`
- Class: infrastructure
- Minters: `csharp:Rasm/Domain/telemetry#CAUSAL_FRAME`; `python:runtime/evidence/clock#CLOCK`; `typescript:core/value/clock#TWO_HALF_LAYOUT`
- Consumers: `python:runtime/transport/serve#SERVE` decodes the halves with `tenant`; `python:runtime/evidence/reproduction#SEED_REPRODUCTION` reproduces every mint in the parity suite.
- Payload: `wire-bytes`
- Pin: DESIGN-PIN
- Blocker: the C# minter's capsule lacks the indexed two-half vectors the fixture freezes.
- Shape: two-64-bit-half stamps in the `ReceiptEnvelope` order — physical half first as the `Instant` Unix-tick `int64`-LE, logical half second as the monotone `ulong`-LE, the exact order `InterchangeIdentity.Compose` seals — with vectors chosen so a logical-half-first composition corrupts by folding a fresh op as stale. Every branch stamps its own causal frames, so each mints the layout rather than reading a peer's.
- Regenerate when: the two-half compose order or the `ReceiptEnvelope` stamp layout changes.

### [02.8]-[IFC_WIRE]

- Seam: `ifc-wire`
- Class: domain
- Producer: `csharp:Rasm.Bim/Exchange/wire#WIRE_PROJECTION`
- Consumers: `typescript:core/interchange/codec`; `python:geometry` IfcOpenShell — each projects its own graph and reproduces the GraphKey.
- Payload: `wire-bytes` + `digest`
- Pin: DESIGN-PIN
- Blocker: producer lacks the canonical IFC payload the `WireParity` row freezes.
- Shape: `IfcWire` bytes with the `ContentAddress.OfGraph` GraphKey; bytes prove host-local re-seal, while GraphKey proves cross-runtime parity. GeometryGym-backed IFC exchange is the producer's domain capability; the Python IfcOpenShell implementation conforms to the same contract without reading the C# emitter.
- Regenerate when: the canonical authoring order, the `ContentAddress.OfGraph` law, or the pinned corpus payload changes.

### [02.9]-[DESCRIPTOR_DRIFT]

- Seam: `descriptor-drift`
- Class: infrastructure
- Minters: `csharp:Rasm.Materials/Appearance/interchange#TEXTURE_EGRESS` ([rasm/channels.proto](rasm/channels.proto), the `rasm` descriptor source, `python:runtime/transport/shapes#VOCABULARY` its co-transcribed roster); `csharp:Rasm.Element/Graph/wire#WIRE_CODEC` (`Graph/element.proto`, the `rasm.element.v1` descriptor source); `csharp:Rasm.Compute/Runtime/wire#CONTRACT_EVOLUTION` (the suite proto vocabulary); `python:runtime/transport/shapes#REGISTRY_AND_DRIFT`; `typescript:core/interchange/contract#DRIFT_VERDICT`
- Consumers: each minter's gate reads the snapshot pair it owns; no branch reads a peer's verdict.
- Payload: `descriptor-set`
- Pin: DESIGN-PIN
- Blocker: [rasm/channels.proto](rasm/channels.proto) is the first landed source — the texture-family messages under `package rasm` with `option csharp_namespace = "Rasm.Channels"`, its `FileDescriptorSet` snapshot frozen beside it at [rasm/channels.descriptor.binpb](rasm/channels.descriptor.binpb) — but the remaining descriptor sources still hold no `.proto` (`Graph/element.proto` and the suite service vocabulary — the appearance families are NOT a pending source: their wire is the `[02.19]` producer's MessagePack integer-keyed roster, mirrored field-for-field at every peer, and a proto declaration beside it would be a second schema for one wire), and `buf breaking` gating FILE against `main` is unwired: `buf` is absent from the machine PATH, so the gate holds no runnable form.
- Shape: each proto source emits a `FileDescriptorSet`; the gate gives `Identical`/`Additive`/`Breaking`; numbers only append, and removals reserve name and number. Each descriptor source is one mint unit, so a branch owning two sources carries two snapshot rows under one gate law and the forked-parity defect is two snapshots of one source. Each source's snapshot freezes beside it as `<source>.descriptor.binpb` — one baseline per source, the `buf breaking` FILE gate reading each against `main` — so a later source lands into a spelled home rather than re-deciding the layout.
- Regenerate when: any owning `.proto` contract changes.

### [02.10]-[CONSUMPTION_PROFILE]

- Seam: `consumption-profile`
- Class: infrastructure
- Minters: `csharp:Rasm.AppHost/Runtime/profiles#PROFILE_AXIS`; `python:runtime/execution/admission#CONTEXT`; `typescript:runtime/proc/config#ADMISSION_ROWS`
- Consumers: every package reading a supplied profile row instead of assuming a deployment shape; each branch resolves the row at its own admission owner.
- Payload: `canonical-json`
- Pin: DESIGN-PIN
- Blocker: the C# minter's preimage is landed — `ConsumptionProfile.CanonicalJson()` at `csharp:Rasm.AppHost/Runtime/profiles#PROFILE_AXIS` renders the six axis rows in roster order under an ordinal provider-key sort as one UTF-8 `canonical-json` string — and no frozen vector stands against it, so the python and typescript minters carry no equivalent renderer and the three landed rosters carry no proved parity.
- Shape: one canonical row over the axis roster `libs/.planning/ARCHITECTURE.md` `[10]-[CONSUMPTION_MODEL]` fixes — a closed axis carrying its fixed vocabulary, an open axis carrying capability-descriptor rows its supplying branch names — the composition root supplying the row and each minter spelling the same axes in its own types; that section owns the axis set, each axis's form, and the refusal contract. Each branch spells the four closed axes as exhaustive vocabularies in its own type system and the two open axes as one common descriptor shape — key, the capability it supplies, the isolation it reaches — whose rows and capability vocabulary stay branch-owned. Refusal is one grammar everywhere: axis, value, and reason.
- Regenerate when: the Tier-0 roster gains an axis, or a closed axis gains a value.

### [02.11]-[BACKEND_CONTRACT]

- Seam: `backend-contract`
- Class: infrastructure
- Minters: `csharp:Rasm.Persistence/Store/schema#IDENTITY`; `typescript:data/lane/capability#CONTRACT`; `python:runtime/execution/admission#BACKEND_CONTRACT`
- Consumers: `typescript:iac/operate/converge#PROJECTION` realizes the merged generation onto the deployment plane; each branch's runtime admission verifies the generation it observes.
- Payload: `canonical-json` + `digest`
- Pin: DESIGN-PIN
- Blocker: no minter has frozen its canonical artifact set, so the generation digest carries no pinned preimage.
- Shape: one branch contribution is the artifact set sorted by artifact key under one ordinal total order over a key alphabet bounded to printable ASCII, since the branch comparators split above it — `StringComparer.Ordinal` and the default JavaScript comparator order by UTF-16 code unit where Python `sorted` orders by codepoint, inverting every key pair mixing an astral character with `U+E000`-`U+FFFF` — so the bounded alphabet is what makes the three orders one order, and a key admitting a non-ASCII character forks the generation. Each artifact carries key, role, canonical bytes, its provider set, and its dependency key set, every collection deduplicated and ordinally sorted before framing, with generation identity the seed-zero `XxHash128` digest over the framed canonical artifact stream. Minters digest the stream they frame and verifiers digest the bytes they receive, so no path re-encodes a decoded document and compares — `[02.1]-[CANONICAL_BYTE_IDENTITY]` fixes why a re-encode never reproduces foreign bytes, and a branch digesting its own re-encode mints a generation its peers cannot match from the same artifacts. Artifact key order is the whole order, so a dependency-depth or topological rank inside the stream mints a second generation from one artifact set. Dependency keys are digest-bearing payload the projection funnel proves closed and acyclic, so every mint path carries the proof and no path validates by sorting. Capability rows carry key, requirement, and value beside a `failureRank` of `required | degradable | observational` and a `restartClass` of `session | reload | restart`, whose rank order is load-bearing so an aggregated repair reports the worst disruption across its gap set rather than the least; the extension roster filling those rows is branch-owned deployment state, never a corpus set. Polyglot merge unions branch contributions by artifact key under that same order and refuses any key two branches claim with differing content, artifact and capability rows alike, so neither first-wins nor last-wins resolves a collision. Runtime verifies a generation and never mutates it; a digest change replaces the generation whole.
- Regenerate when: the artifact ordering, the preimage framing, or the generation-identity derivation changes.

### [02.12]-[CAPABILITY_DESCRIPTOR]

- Seam: `capability-descriptor`
- Class: domain
- Producer: `csharp:Rasm.AppHost/Agent/capability#SDK_CODEGEN` over the broker registry, shape identity carried by `SuiteContracts.Schema`.
- Consumers: `python:runtime/transport/serve#CAPABILITY_INVOKE` decodes the descriptor into one dispatch; the typescript SDK target binds the same schema.
- Payload: `canonical-json` + `digest`
- Pin: DESIGN-PIN
- Blocker: DISCHARGED — `DescriptorPin.Of(registry, schemaOf, wire)` at `csharp:Rasm.AppHost/Agent/capability#SDK_CODEGEN` is the frozen preimage: one ordinal-ordered fixed-field JSON document over the frozen catalog beside its content address, with `CapabilityDescriptorWire` the pinned-row projection every SDK target binds and `DescriptorPinWire` the carrying document a peer grades its own catalog against. A fixture freezing that document and its digest is the remaining proof obligation.
- Shape: one descriptor per brokered op — name, argument and result schema references, and the effect, idempotency, and cost-unit keys as the broker's own vocabulary — with cross-language shape identity the JSON Schema the producer exports and every SDK target binds unchanged. Brokered capability is the producer's domain capability, so peers decode the descriptor and re-author no capability shape; schema evolution is additive, and a generated SDK is a build artifact rather than a second mint.
- Regenerate when: the descriptor vocabulary, the exported schema layout, or the effect and cost key sets change.

### [02.13]-[TELEMETRY_CONVENTION]

- Seam: `telemetry-convention`
- Class: infrastructure
- Minters: `csharp:Rasm.AppHost/Observability/telemetry#SIGNAL_GOVERNANCE`; `python:runtime/observability/telemetry#TELEMETRY`; `typescript:core/observe/convention#IDENTITY_PROJECTION`
- Consumers: `typescript:iac/operate/observe#STORE_ROWS` reads the translation strategy the wire row pins and `#CHART_ROWS` the collector receiver and exporter rows compiled from it; each branch's composition root realizes its own mint and asserts it through that branch's telemetry harness.
- Payload: `canonical-json` + `digest`
- Pin: DESIGN-PIN
- Blocker: the C# minter projects its rows as a document — `SignalGovernance.Conformance(TelemetryComposition)` at `csharp:Rasm.AppHost/Observability/telemetry#SIGNAL_GOVERNANCE` returns the `ConformanceDocument` carrying the branch's role, schema url, and ordinally-joined `ConformanceRow` set — while the python telemetry owner and the typescript convention rows still hold their values as composition code with no canonical projection member, so two of three branches carry no byte-deriving input and the parity this entry proves has nothing to compare against.
- Shape: the closed conformance row set `libs/.planning/ARCHITECTURE.md` `[08]-[OBSERVABILITY_CONFORMANCE]` legislates, projected as one document per minter — the resource triple and its detector-merge precedence, the scope coordinate (emitting package id, version stamp, semconv schema url) shared by tracer, meter, and logger, the metric-name grammar with its UCUM unit vocabulary and the minter's own domain roster of segment beside admitted subject, wire temporality and default histogram aggregation, exemplar posture, the propagation dialect set with its one global composite registration and the inbound parent-adoption posture, the tenant key with its promotion allowlist and view cap, the absent-entry posture every sometimes-absent dimension shares — tenant, level-family, and substrate keys read as the untagged whole when absent, stay rostered on the allow-list while sometimes absent, and a key that is its cell's own identity never constructs the absent state, egress protocol and compression beside the metrics-receiver translation strategy, buffer and loss posture, and lifecycle drain bounds. Role-aware absence is part of the shape rather than an exception to it: each row carries the disposition its minter's role admits over `process` | `browser` | `deploy`, so a browser minter marks the process profiler, the process health probe, and the process support archive `absent` while carrying logs, metrics, traces, vitals, and crash evidence, and a reader distinguishes a role that cannot carry a signal from a branch that dropped one. Capability absence rides the same column under its own disposition and never under role absence: a row whose plane exposes no seat at the minter's pinned distribution — the exemplar filter and the sender encoding knob are the standing pair — projects the value the plane carries beside the pin that withholds the seat, so the digest reads a stated ceiling rather than a dropped row and a pin bump that opens the seat fails here until the row re-values. Every branch mints from its own composition inputs and parity across the three mints IS the conformance; transcription remains how each branch spells the rows, and this entry is what proves the three spellings agree. Domain rows carry that proof furthest: two minters claiming one segment project one subject spelling byte-identical or fail the digest here, so a vocabulary each branch declares locally still resolves to one estate namespace.
- Regenerate when: Tier-0 `[08]-[OBSERVABILITY_CONFORMANCE]` gains, drops, or re-values a row, or the semconv schema pin bumps.

### [02.14]-[BENCHMARK_CLAIM]

- Seam: `benchmark-claim`
- Class: infrastructure
- Minters: `csharp:Rasm.Compute/Runtime/receipts#TS_PROJECTION`; `typescript:core/interchange/codec#LANDING_WIRE`
- Consumers: `typescript:core/observe/board#BENCH` grades a decoded claim against its baseline and trends the bench pack; `typescript:security/crypt/sign#CALIBRATION` and `typescript:runtime/proc/exec#MEASURED_RUN` mint through that landing class and grade each row against its own ceiling; `csharp:Rasm.Compute/Runtime/receipts#BENCHMARK_CLAIMS` admits the claim the projection carries and `csharp:Rasm.Compute/Runtime/transport#TRANSPORT_AXIS` folds it into the winning wire encoding, both resolving through the durable summary at `csharp:Rasm.Persistence/Query/cache#BENCHMARK_INDEX`.
- Payload: `wire-bytes`
- Pin: DESIGN-PIN
- Blocker: neither minter freezes a claim document — the C# equivalence sweep pins no host fingerprint and the typescript harness pins no suite, so the shape carries no byte-deriving input on either branch.
- Shape: one host-admitted claim document — `suite`, `host` the executing fingerprint, `minted` the document instant, and a non-empty `metrics` array — with fingerprint and instant riding the document because one measurement run carries one of each and a per-row copy restates them for every metric that run holds. Each metric row carries its own subject, band, and cost columns, and the subject discriminates a bare probe spelling label, unit, and modality from a kernel run spelling that triple beside its tensor input, substrate, family, case, route, provider, corpus key, artifact key, equivalence deviation, tolerance class, and profile artifacts, so neither coordinate widens the other into optionality. Band rungs land exactly where the minting harness computes them — a sampling harness carries the raw vector beside its tick count and whole ladder while an equivalence sweep persisting a summary row carries median, p95, and deviation — so a grading policy names the rung it reads and a pair missing that rung on either side refuses by axis rather than grading a fabricated value; an enrichment band the executing runtime cannot fill stays absent under the same law. Each minting harness owns its own `unit` vocabulary rather than the instrument census's — a timing harness spells nanoseconds, a render probe spells a per-second rate beside a bare count, a C# sweep spells its own — so the column stays a free non-empty string the grade compares verbatim as an equality axis, and narrowing it onto the telemetry unit roster refuses every measure that roster was never built to carry. Admission is one gate on the document: a claim whose host print differs from the executing identity refuses, so no consumer compares measurements across fingerprints and no branch grades a peer's runtime. Every branch measuring its own runtime mints its own claims and parity across the mints IS the conformance; a branch whose benchmark evidence never crosses its own boundary mints no instance here.
- Regenerate when: the subject union, the band ladder, or the host-admission gate changes.

### [02.15]-[HOST_FINGERPRINT]

- Seam: `host-fingerprint`
- Class: infrastructure
- Minters: `csharp:Rasm.AppHost/Runtime/determinism#DETERMINISM_KERNEL` (`EnvFingerprint` composing the record and `EnvFingerprint.Digest` minting its print through the kernel content-hash entry); `typescript:ui/viewer/probe#HOST_MIRROR` (`Probe.host`, the browser-role capture)
- Consumers: `csharp:Rasm.Compute/Runtime/receipts#BENCHMARK_CLAIMS` reads the record as the claim's `host` column and refuses a claim whose print differs from the executing identity; `typescript:core/interchange/codec#LANDING_WIRE` decodes the wire into `Claim.Host`; `typescript:ui/viewer/probe#CLAIM_BOARD` joins the local capture against an admitted claim's own host to display divergence context
- Payload: `canonical-json` + `digest`
- Pin: DESIGN-PIN
- Blocker: the C# minter's preimage is landed — `HostFingerprintWire.Canonical()` at `csharp:Rasm.AppHost/Runtime/determinism#DETERMINISM_KERNEL` emits the seven columns in roster order as the byte-deriving input — while the browser minter pins no adapter capture, so `typescript:ui/viewer/probe#HOST_MIRROR` carries no frozen document and the two-role parity this entry proves stands on one side alone.
- Shape: one environment-identity record — `print` the digest identifying the environment, `machine`, `os`, `arch`, `processors`, `runtime`, and a `stamps` map carrying the dimensions a role reaches beyond the fixed columns. Role-aware absence is part of the shape rather than an exception to it: a process minter composes every column from its own host and derives `print` over them, while a browser minter takes `print` as the identity it was handed, reads `machine` and `arch` off the graphics adapter, and marks the operating-system name with the declared-unavailable sentinel because no stable browser surface exposes it — so a reader distinguishes a role that cannot reach a fact from a branch that dropped one, and a fabricated value is unspellable. Registration lands here rather than resolving as `[02.14]`'s column alone because the record crosses on its own seam edge as well as inside a benchmark claim: the viewer mirrors the field set with no claim in hand, so the two spellings agree even where no claim exists to carry them. Every branch reaching a host mints its own record and parity across the mints IS the conformance — a branch that only decodes carries no fingerprint of its own.
- Regenerate when: the record's column set changes, the sentinel spelling for a role-unreachable fact changes, or the print derivation moves.

### [02.16]-[BOARD_PACK]

- Seam: `board-pack`
- Class: infrastructure
- Minters: `csharp:Rasm.Compute/Runtime/receipts#TS_PROJECTION`; `typescript:core/observe/board#PACKS`
- Consumers: `typescript:iac/operate/observe#BOARD_APPLY` admits every pack under the closed `_PACKS` provenance tuple, tags each compiled board with its wire, and folds pack alerts into the one burn-rate compile leg.
- Payload: `canonical-json` + `digest`
- Pin: DESIGN-PIN
- Blocker: neither minter freezes a pack document — the C# projection pins no concrete pack vector and the typescript owner encodes packs in-process with no frozen instance, so the shape carries no byte-deriving input on either branch.
- Shape: one producer-pack document `{ wire, boards, alerts }` — `wire` a provenance key the deploy tuple closes, `boards` core-encoded `DashboardModel` documents, `alerts` burn-derived alert-spec rows — whose wire vocabularies mirror the kernel closed families arm for arm under `docs/laws/topology.md` `[FENCE_SEAM]`'s projection law: one `SliWire` arm per kernel `Sli` case with the `LevelBreach` polarity crossing beside its bound, and the instrument-kind, panel-kind, burn-row, and `page` | `ticket` severity rosters each transcribed whole. Parity across the two spellings IS the conformance — a lagging arm refuses a whole producer at the typed boundary, and a dropped discriminant column compiles the inverse comparison — and a branch minting no pack carries no instance here.
- Regenerate when: a kernel closed family — `Sli`, `LevelBreach`, `InstrumentKind`, `PanelKind`, `BurnRow`, `AlertSeverity` — gains or retires a case, or the pack document layout changes.

### [02.17]-[TEXTURE_SET_BY_KEY]

- Seam: `texture-set-by-key`
- Class: domain
- Producer: `csharp:Rasm.Materials/Raster/set#TEXTURE_SET`, keyed and projected through `csharp:Rasm.Materials/Appearance/interchange#TEXTURE_EGRESS`
- Consumers: `typescript:core/interchange/codec#WIRE_CENSUS` carries the family and `#LANDING_WIRE` lands it as `TextureSet`; `typescript:ui/viewer/scene#APPEARANCE_BIND` seats it through `Pbr.seat`/`Pbr.index` over the served asset directory; `python:runtime/transport/shapes#VOCABULARY` decodes it on the `texture_set_wire` row, decode-only.
- Payload: `wire-bytes` + `digest`
- Pin: DESIGN-PIN
- Blocker: the producer has pressed no concrete set — `Rasm.Materials` is a fences-only planning surface whose `Raster/` press estate holds no realized source, so no runtime mints the channel-ordered plane-digest preimage the set key streams over. The descriptor half is closed: [rasm/channels.proto](rasm/channels.proto) carries the texture messages both decode legs bind.
- Shape: conforms to [appearance-vocabulary.schema.json](appearance-vocabulary.schema.json) — the appearance-coupled BAKED set document, riding BEHIND the `AppearanceKey` the seam `AppearanceSummary` freezes over its seven-value preimage — the set key is a payload field, never a summary column, because widening `AppearanceSummary` forks the Bim dedup key and re-ids every appearance node. It carries NO kind discriminant: its one kind is the baked PBR set, and an environment or IBL product rides `[02.18]` instead. Field names are `camelCase` — the mechanical projection of the producer's own members under `JsonSerializerDefaults.Web`, never a third spelling — and every hex field carries the uppercase `ContentAddress` spelling the shared definition's `keySpelling` fixes. The document declares the set extent, its layer law and layer count, the ingest-source normal convention, the set-level alpha mode, the millimetre span the normalized `height` plane resolves against, the tiling coherence a synthesis gate proved, the Mari tile indices when the set is UDIM, the material and conductor identities, the capture provenance the appearance wire already owns, and one press receipt. Channel and pack rows each carry a LEVEL-ORDERED list of address triples under the shared `planeLevels` law: a self-pyramiding container holds one entry whatever the declared depth, and a container holding no pyramid of its own holds one entry per level, so a pyramid is addressed whole and every level is digested. Each row also carries its container beside its storage format, because the alpha-association conversion the shared definition fixes selects on the container and no other column recovers it; the payload column admits only the wire-legal KTX2 classes, and a desktop-native block payload refuses at decode. Baked bytes are ALWAYS CPU-minted: the press receipt's backend column reads `cpu` on every receipt reaching the wire, the GPU lane is an accelerator whose products carry no set and therefore no key, and its divergence column stays absent until a parity run measures it so a zero never reads as a perfect match. Plane bytes cross as content-addressed blobs in the write-once object store; the tessellation entry `[02.6]` stays untouched, because planes never embed in tiles and a texture edit never re-keys a tessellation. Baked appearance is the producer's domain capability, so peers decode the document, join its leaves to the served asset directory, and re-derive no address and no key.
- Regenerate when: [appearance-vocabulary.schema.json](appearance-vocabulary.schema.json) gains, drops, or re-values a row; the document's field roster changes; or the channel-ordered key preimage moves.

### [02.18]-[ASSET_SET_MANIFEST]

- Seam: `asset-set-manifest`
- Class: domain
- Producer: `python:artifacts/graphic/texture/set#TEXTURE_SET`, its egress leaves joined at `python:artifacts/graphic/texture/set#EGRESS`
- Consumers: `typescript:core/interchange/codec#WIRE_CENSUS` carries the family and `#LANDING_WIRE` lands it; `typescript:runtime/browser/fetch#RUNNER_ENTRY` decodes it on the worker `Survey` arm and `#DEPOT_SCHEDULER` folds the dome the `typescript:ui/viewer/scene#ENVIRONMENT_FOLD` port composes; `csharp:Rasm.Materials/Raster/set#SET_INGEST` decodes it as a classification input through `SetIngest.Peer` and refuses a non-`pbr_set` kind.
- Payload: `wire-bytes` + `digest`
- Pin: REAL
- Expectation: the frozen assets ride [asset-set-manifest/](asset-set-manifest/) — `asset_set_manifest.bin` (476 wire bytes, deterministic proto serialization, seed-zero digest `87aa8c48b73c71fc6d9d131a57331a77`), `asset_set_manifest.json` (the `preserving_proto_field_name` canonical projection), and the three byte-deriving planes under `planes/`. The byte input is settled-design-determined: 8×8 planes over `idx = y·8 + x` row-major — `base_color` u16 `((idx·1021) mod 65536, (idx·2039) mod 65536, (idx·4093) mod 65536)` as `png16`, `geometry_normal` the constant u16 neutral `(32768, 32768, 65535)` as `png16`, `height` float32 `idx/63` as `exr` — encoded through the pinned `imagecodecs` legs whose versions each map row's `tool_version` records. Frozen values the producer emit must reproduce: plane digests (policy-folded `texture-plane` namespace, lowercase `ContentKey` wire spelling) `base_color = 72b07d26416e03d501713d3781dd99c2` (134 B), `geometry_normal = 1cb09264272c513b6a78a6f83566ce47` (82 B), `height = 40f4dd4b6fbdf17d18f3d04bbac4e31b` (578 B); `manifest_key = adf06145b592fc08fedd963c5170f974`, the `texture-set` merkle over the three 16-byte little-endian digests in roster order; document facts `kind = pbr_set`, `normal_convention = gl`, `alpha_mode = none`, `height_scale = 10.0` mm, `license_class = permissive`, `ktx_payload = none` and `mips = 1` on every row. The landed instance schema-validates against [asset-set-manifest/contract.schema.json](asset-set-manifest/contract.schema.json), round-trips byte-identically through the generated message, and re-derives every digest and the manifest key from the landed plane bytes.
- Shape: conforms to [appearance-vocabulary.schema.json](appearance-vocabulary.schema.json) — the ingest-assembled and environment-assembled set manifest, the SECOND appearance producer, distinct from `[02.17]` by capability rather than by language rank: it discriminates `pbr_set`, `hdri`, and `ibl` on a kind column, records the ingest root or generator id, and carries the classification residue no alias claimed, none of which the baked document holds. Environment and IBL products ride HERE and never the baked document, which carries no environment kind. Field names are `snake_case` because the vocabulary row binds each declaration to its generated message under preserved proto field names — the declaration IS the wire contract, with no rename layer — and every digest carries the lowercase `ContentKey` spelling the shared definition's `keySpelling` fixes, so a consumer joining a key across the two documents lowers and never uppercases. Map and pack rows carry the same LEVEL-ORDERED address list, container column, and payload legality the baked document carries, plus the producing tool and its recorded version PER MAP, because one set legitimately mixes the spawned encode floor with an in-process acceleration leg and a set-level tool column would erase which leaf came from which. The environment leg carries the irradiance harmonics under the shared band order and layout, the equirect source, the roughness-indexed prefilter pyramid as address triples rather than a bare file roster, the split-sum BRDF table, the importance-sampling guide, and the read-side intensity and rotation — read-side because re-orienting or re-exposing a dome re-keys no blob and triggers no re-prefilter, and a producer baking either into the coefficients forks every consumer reading the same digest at another orientation. Ingest classification and environment assembly are the producer's domain capability; the C# consumer folds a decoded manifest into its own ingest intent and classifies nothing the manifest already resolved.
- Regenerate when: [appearance-vocabulary.schema.json](appearance-vocabulary.schema.json) gains, drops, or re-values a row; the document's field roster changes; or the roster-ordered merkle fold moves.

### [02.19]-[MATERIAL_WIRE]

- Seam: `material-wire`
- Class: domain
- Producer: `csharp:Rasm.Materials/Appearance/interchange#MATERIAL_WIRE`
- Consumers: `typescript:core/interchange/codec#LANDING_WIRE` lands `Material` and `PbrGroups` for `typescript:ui/viewer/scene#APPEARANCE_BIND`; `python:runtime/transport/shapes#VOCABULARY` decodes both on its appearance mirror rows, decode-only. The seam `AppearanceSummary` is NOT a shape of this crossing: no producer emits a standalone summary document — it crosses as the `rasm.element.v1` `AppearanceWire` payload inside `NodeWire` (`csharp:Rasm.Element/Graph/wire#WIRE_CODEC`), and each peer's `AppearanceSummary` landing shape seats that payload, never a document from this codec.
- Payload: `wire-bytes` + `digest`
- Pin: DESIGN-PIN
- Blocker: the producer has pinned no concrete material, so the appearance content hash carries no byte-deriving input.
- Shape: the OpenPBR parameter algebra as it crosses — the layered surface projection, its group wire over the OpenPBR Surface 1.1 inputs, the conductor key, the capture receipt, and the appearance content hash the seam summary keys. The producer is SOLE: peers mirror the projection field for field and a peer-side lowering, conductor table, colour conversion, or key derivation is the cross-language drift defect this entry names — a decoded group carries verbatim onto a consumer's own material, and deriving one field from another is the standing defect the single-producer law forecloses. Colour crosses as the scene-linear triple the graph's working space fixes, never a display-encoded byte triple. The entry registers a PRE-EXISTING crossing rather than a new one: the typescript branch already decodes both shapes and the python branch already rosters them, so until this entry landed a shape crossed three branches with no contract binding it — convention-aligned interop that forks on first edit. Map and texture-transform fields stay OFF this document by ruling: a baked plane set is `[02.17]`, and widening the group wire with map digests would seat a second appearance producer behind one shape. Schema authority is the producer's MessagePack integer-keyed record roster — appended keys past the frozen block, mirrored structurally at each peer under the mirror-census law — and the family holds NO descriptor source under `[02.9]` because the wire is not proto-shaped.
- Regenerate when: the OpenPBR input roster, the conductor vocabulary, the capture-receipt columns, or the appearance content-hash preimage changes.

### [02.20]-[SIGNED_ARTIFACT]

- Seam: `signed-artifact`
- Class: domain
- Producer: `python:artifacts/exchange/credential#CREDENTIAL`
- Consumers: `csharp:Rasm.Persistence/Query/federation#PLAN_INGRESS` admits the artifact as a `SignedArtifact` federation source and resolves its binding through the attested ledger before a federated read executes.
- Payload: `wire-bytes` + `digest`
- Pin: DESIGN-PIN
- Blocker: both ends declare the crossing GATED — the producer's evidence carries a manifest id, signer facet, assertion set, and validation state but pins no attestation value, and the consumer names the binding a declared gap rather than a working stub, so neither end holds the attestation preimage.
- Shape: one C2PA-signed asset carried as its signed bytes beside the seed-zero content key minted over exactly those bytes, so the manifest and the key co-identify one durable artifact across the boundary and the consumer re-derives the same key over the same bytes rather than trusting a transported value. Embed modality is a value the producer discriminates, not a knob: an embedded manifest travels inside the asset while a no-embed manifest travels as detached bytes beside a remote store reference, and the consumer's validation reads the detached bytes rather than degrading to an unsigned verdict. Attestation is what makes a federated read over an externally computed result tamper-evident LOCALLY before it executes, so a consumer admitting the source without resolving the attestation has admitted an unattested plan. Content credentials are the producer's domain capability: the consumer verifies and never re-signs, mints no certificate or key material, and re-authors no manifest.
- Regenerate when: the signed-bytes canon, the attestation binding, or the seed-zero content-key law changes.

### [02.21]-[APPHOST_WIRE]

- Seam: `apphost-wire`
- Class: domain
- Producer: `csharp:Rasm.AppHost/Runtime/ports#WIRE_LAW` — the source-generated System.Text.Json roster (`AppHostWireContext` merged into `SuiteContracts` under `JsonSerializerDefaults.Web`), one `[JsonSerializable]` row per family.
- Consumers: `typescript:core/interchange/codec#WIRE_CENSUS` census rows sourced `Rasm.AppHost` decode each family under the census `json` arm into its landing class; `typescript:ui` dashboard surfaces read the decoded `ui`-consumer rows.
- Payload: `canonical-json` + `digest`
- Pin: DESIGN-PIN
- Blocker: the producer freezes no family vector — every family crosses as source-generated JSON with no pinned instance, so no family carries a byte-deriving input on either branch.
- Shape: the AppHost runtime-evidence family set as ONE registration — `ReceiptEnvelopeWire`, `TenantContextWire`, `CommandAvailabilityWire`, `CredentialPemWire`, `BindingStatusWire`, `CoercedValueWire`, `WriteReceiptWire`, `FlagVerdictWire`, `SupportCaptureWire` — each family one `[JsonSerializable]` row on the producer roster and one census row at the consumer, field names the mechanical `Web`-defaults projection of the producer's own members. Carriage is JSON by the producer's landed wire law, so the consumer census carries these rows under its `json` arm, never `proto` — no descriptor source exists or is owed under `[02.9]`. Families a sibling entry already owns register THERE and not here: `HlcStampWire` byte layout is `[02.7]`'s shape (this producer carries it as an envelope field), `CapabilityDescriptorWire` is `[02.12]`, `HostFingerprintWire` is `[02.15]`, and `BenchmarkClaimWire` is `[02.14]` with `csharp:Rasm.Compute` the minter — a census row sourcing it to AppHost mis-names the producer. Host runtime evidence is the producer's domain capability, so peers decode the families and re-author none.
- Regenerate when: a family's field roster changes, the producer roster gains or retires a family, or the `Web`-defaults naming projection moves.

### [02.22]-[APPUI_WIRE]

- Seam: `appui-wire`
- Class: domain
- Producer: `csharp:Rasm.AppUi` — the source-generated System.Text.Json roster (`AppUiWireContext` under the suite wire law's camelCase Strict emission), one `[JsonSerializable]` row per family, minted per owning page: `Shell/commands#TS_PROJECTION` (`CommandPayloadWire`, `CommandGateWire`), `Shell/controls#TS_PROJECTION` (`ControlIntentWire`), `Shell/solver#TS_PROJECTION` (`LayoutConstraintWire`), `Render/pipeline` (`GeometryResidencyWire`), `Diagnostics/evidence` (`EvidenceTimelineWire`).
- Consumers: `typescript:core/interchange/codec#WIRE_CENSUS` census rows sourced `Rasm.AppUi` decode each family into its landing class (`ControlIntent`, `LayoutProgram`, `CommandGate`, `RenderReceipt`); `typescript:ui/viewer/panel` materializes the shell families and `ui/viewer/probe` reads the render receipt; `typescript:runtime` consumes the residency manifest on its frame home.
- Payload: `canonical-json` + `digest`
- Pin: DESIGN-PIN
- Blocker: the producer freezes no family vector — every family crosses as source-generated JSON with no pinned instance, so no family carries a byte-deriving input on either branch.
- Shape: the AppUi product-shell family set as ONE registration — `CommandPayloadWire`, `CommandGateWire`, `ControlIntentWire`, `LayoutConstraintWire`, `GeometryResidencyWire`, `EvidenceTimelineWire` — each family one producer roster row and one census row at the consumer; render evidence is a MEMBER of the evidence family (`EvidenceTimelineWire`'s `render` kind arm), never a standalone `RenderReceiptWire` — no producer mints that name, and the ts frame-hash compare shape decoded under it awaits its producer reconciliation at the consumer's own card; sibling wire records an owning page mints beside a family (`CommandIntentWire`/`CommandInvocationWire`/`CommandOutcomeWire`/`CommandReceiptWire`, `IntentBindingWire`/`ControlReceiptWire`, `LayoutVarWire`/`LayoutTermWire`/`LayoutProgramWire`, `ViewpointWire`/`ResidencyTileWire`/`MeshoptStreamWire`/`MeshletWire`) are family MEMBERS riding inside their family's payload, never separate entries. Carriage is JSON by the producer's landed wire law, so the consumer census carries these rows under its `json` arm, never `proto` — no descriptor source exists or is owed under `[02.9]`, and a census `proto` arm over these families names a descriptor no producer publishes. Ordered-program parity binds `LayoutConstraintWire` whole — variable-introduction order, edit-variable set, authored and resolved measurement suggestions — because both tableaus re-solve rather than trusting solved positions. Product-shell vocabulary is the producer's domain capability, so peers decode the families and re-author none.
- Regenerate when: a family's field roster changes, the producer roster gains or retires a family, or the camelCase Strict emission law moves.

### [02.23]-[BIM_WIRE]

- Seam: `bim-wire`
- Class: domain
- Producer: `csharp:Rasm.Bim` — the JSON wire families the openBIM owner mints beside the `[02.8]` IFC artifact, per owning page: `Review/issues#TS_PROJECTION` (`BcfTopicWire` with its nested comment/viewpoint/camera family under `BcfWireContext`), `Semantics/geospatial#GEOSPATIAL_SEAM` (`GeoFeatureWire`, the GeoJSON text projection), `Review/diff#MODEL_DIFF` (`ModelDiff`, the typed change-set), `Model/query#PREDICATE_WIRE` (`PredicateWire`, the `[JsonPolymorphic]` predicate family under `PredicateCodec.Seal`/`Admit`).
- Consumers: `typescript:core/interchange/codec#WIRE_CENSUS` census rows sourced `Rasm.Bim` decode each family into its landing class; `typescript:ui` materializes the BCF board, viewpoint, diff, and geo overlays; `python:data` decodes `GeoFeatureWire` on its geo ingest.
- Payload: `canonical-json` + `digest`
- Pin: DESIGN-PIN
- Blocker: the producer freezes no family vector — every family crosses as source-generated or codec-sealed JSON with no pinned instance, so no family carries a byte-deriving input on either branch.
- Shape: the Bim coordination-and-context family set as ONE registration — `BcfTopicWire`, `BcfViewpointWire`, `GeoFeatureWire`, `ModelDiff`, `PredicateWire` — each family one producer roster row and one census row at the consumer, the entry registering PRE-EXISTING crossings exactly as `[02.19]` did: the typescript census already decoded these families, so until this entry landed they crossed as convention-aligned coincidence. Carriage is JSON by each producer page's landed wire law, so the consumer census carries these rows under its `json` arm, never `proto` — no descriptor source exists or is owed under `[02.9]`. Sibling wire records a page mints beside a family (`BcfCommentWire`/`BcfCameraWire`/`BcfColoringWire`, the `ValueMatchWire`/`MeasureWire`/`BoundWire`/`NodeMatchWire` predicate members, the per-arm diff records) are family MEMBERS riding inside their family's payload, never separate entries. Selection semantics bind `PredicateWire` whole — a browser client composes the same closed predicate algebra the model owner evaluates, an unrostered `arm`/`match` refusing at BOTH ends — and re-admission facts only the producer can decide (pattern compilability, vocabulary membership, measure dimension) stay the producer's gates. The IFC artifact itself stays `[02.8]` and the openBIM issue/geo/diff/predicate vocabulary is the producer's domain capability, so peers decode the families and re-author none.
- Regenerate when: a family's field roster changes, the producer roster gains or retires a family, or either dialect register's emission law moves.

### [02.24]-[ELEMENT_WIRE]

- Seam: `element-wire`
- Class: domain
- Producer: `csharp:Rasm.Element/Graph/wire#WIRE_CODEC` — the `rasm.element.v1` protobuf family (`ElementGraphWire`, `GraphDeltaWire`, `NodeWire`, `RelationshipWire` and their nested payload messages) minted by `ElementWire.Encode` under the append-only contract-evolution law.
- Consumers: `typescript:core/interchange/codec#WIRE_CENSUS` census rows sourced `Rasm.Element/Graph` with the `#KEYED_REGISTRY` `admittedGraph` decode; `python:runtime/transport/shapes#VOCABULARY` holds its roster deferral until the descriptor source lands.
- Payload: `wire-bytes` + `digest`
- Pin: DESIGN-PIN
- Blocker: `Graph/element.proto` holds no landed source — the `[02.9]` descriptor-drift entry names it a pending mint unit — so no message carries a frozen byte-deriving instance on either branch.
- Shape: the one element-graph crossing, registering a PRE-EXISTING decode — the typescript census already lands `ElementGraphWire`, so until this entry the flow crossed as convention-aligned coincidence. Content keys ride the wire verbatim under the `docs/laws/patterns.md` `[CONTENT_KEY]` law (16 big-endian bytes), decode re-admission runs the producer's own value gates at the consumer's landing class, and descriptor-drift gating for the `.proto` stays `[02.9]`'s obligation — this entry owns the semantic model, never the descriptor snapshot. Sibling messages a page mints inside the family (`PropertyValueWire`, `MeasureValueWire`, `MaterialPropertySetWire`, the event envelope) are family MEMBERS riding the payload, never separate entries.
- Regenerate when: the message roster or a field number changes, or the descriptor source lands.

### [02.25]-[ELEMENT_CORPUS]

- Seam: `element-corpus`
- Class: domain
- Producer: `csharp:Rasm.Element/Graph/corpus#CORPUS_ROSTER` — four graded deterministic models (`S`/`M`/`L`/`XL`) minted by the seeded `GraphForge` through `CorpusGate.Mint`, each an `ElementWire`-encoded snapshot beside its `ContentAddress`.
- Consumers: `python:runtime/evidence/reproduction#SEED_REPRODUCTION` (the `_CORPUS` `element-corpus` design-pin row, bytes at graduation); `typescript:core/value/contentKey` (the bit-parity gate) with the `core/interchange/codec` decode leg.
- Payload: `wire-bytes` + `digest`
- Pin: DESIGN-PIN
- Blocker: execution route absent, knowledge complete — no source tree exists to run `GraphForge.Mint` (`libs/.planning/ARCHITECTURE.md` `[05]-[PLANNING_LIFECYCLE]` authors source only when code is written), `tests/csharp/libs/Rasm.Element` is an `AssayTestShell` shell, and the DESIGN-PIN law forbids fabricated bytes; the four grade addresses pin when the corpus harness executes the settled forge.
- Shape: whole-graph parity — each peer decodes a grade's wire bytes and re-derives its snapshot address, equality across the three runtimes proving canonical-bytes, sorting, and hash-seed agreement in one gate; producer-side `CorpusOp` witnesses prove op stability before any pin lands. Grade profiles and seeds are the producer's roster rows, so a forge or layout edit re-derives all four addresses and re-freezes the fixtures whole.
- Regenerate when: any forge kernel, grade profile row, canonical-bytes layout, or wire message changes.

### [02.26]-[DECLARATION_RECORD]

- Seam: `declaration-record`
- Class: domain
- Producer: `python:data/impact/declaration#DECLARATION` — the declaration-registry ingest owner minting one `DeclarationRecord` per verified product declaration (Ökobaudat the first source row, EC3 and offline bundles rows on the same axis) keyed to an estate material identity by the ingest keying row.
- Consumers: `csharp:Rasm.Materials/Properties/assessment#ASSESSMENT_RECORD` — the `DeclarationWire.Decode` leg lowering a record onto `EpdRow` and `AssessmentRecord.Declared`, reaching `AssessmentSet.Of` unchanged.
- Payload: `canonical-json` + `digest`
- Pin: DESIGN-PIN
- Blocker: no frozen Ökobaudat sample declaration pins the byte-deriving input — the producer freezes one registry record per source row when the ingest lane executes; the DESIGN-PIN law forbids fabricated bytes.
- Shape: one product declaration keyed to a material identity — issuer + registration the duplicate-check pair, declared unit admitted at its own functional unit, issue and expiry dates, and the per-indicator per-module impact map at declaration granularity (the frozen 13-indicator EN 15804+A2 roster over the 15-module EN 15978 roster) where KEY PRESENCE is the coverage census — an absent cell is undeclared absence, never a zero. Discriminating laws: a declared cell is a measured value with negative biogenic/avoided-burden carbon valid; the two resource fractions are optional scenario data, absent when undeclared; consumers band modules as their own seam requires (the C# leg sums declared cells onto its six-band `LifecycleStage` axis and constructs the full matrix only when every core indicator covers every band). The declaration semantic model is the python data branch's host-free ingestion capability; peers decode and re-author none.
- Regenerate when: the indicator or module roster, the declared-unit vocabulary, or the canonical key order changes.

### [02.27]-[HDF5_FIELD_CONTAINER]

- Seam: `hdf5-field-container`
- Class: domain
- Producer: `csharp:Rasm.Compute/Runtime/codecs#FIELD_RESULT_CODEC` — `FieldCodec.Hdf5Encode` emitting the station×component chunk model as an HDF5 1.10 container over the `#HDF_ARCHIVE` cursor-guarded writer.
- Consumers: `python:data/gridded/field#CONTAINER` reads the container on the h5py rail (`FieldContainer`, with the `phony_dims` labelled lift) and `python:data/gridded/virtual#MANIFEST` virtualizes it through the `hdf` parser arm; `csharp:Rasm.Compute/Runtime/codecs#FIELD_RESULT_CODEC` `Hdf5Decode` is the round-trip leg.
- Payload: `wire-bytes` + `digest`
- Pin: DESIGN-PIN
- Blocker: the producer freezes no container vector — `Hdf5Encode` is a landed fence with no realized source, so no field corpus carries a byte-deriving instance on either branch.
- Shape: one field crosses as ONE dataset at the `/field` path — this entry MINTS that layout, because today's convention let the dataset path double as a station label and two producers spelled it two ways — with the field extent leading and the trailing axis the COMPONENT axis of that one dataset; one-dataset-per-component is the refuted sibling layout, forking the chunk address every consumer computes. Chunks are station-outermost through the one `FieldCodec.Grid` derivation, so `Chunk(ordinal)` and a station-slab `HyperslabSelection` window resolve identically at both ends. Filters run Shuffle (id 2) then Deflate (id 1) — the h5py `compression='gzip', shuffle=True` pipeline — with C# WRITERS constrained to the `DeflateGrade` four-value set `{-1, 0, 1, 9}` while READERS accept any level a foreign producer wrote. Element bytes are little-endian ONLY: the managed rail refuses big-endian at open, so a big-endian corpus re-encodes upstream and never crosses. Writes are create-only and chunk-aligned in index order — no append, no in-place edit — so an accumulating series segments at its producer's cadence edge. Dimension-scale stance is PICKED here: the writer emits RAW HDF5 with NO dimension-scale datasets (netCDF semantics resolve above the rail on both branches), so python consumers read via h5py directly — or `phony_dims` under h5netcdf — and a writer-side coordinate-variable roster is growth this entry re-values, never a consumer workaround. VDS stance is PROVEN: an h5py-authored virtual container READS on the managed rail — `VirtualStorage` layout resolves whole and hyperslab reads across source boundaries, relative source paths resolve beside the containing file, and an unresolvable source region yields the declared fill value, never a fault — so a virtual container is a lawful read-side carrier of this entry, while C# writers never emit VDS because the write model carries no virtual layout, leaving virtual authorship on the h5py side. Field-container emission is the producer's domain capability, so peers decode the container and re-derive no layout.
- Regenerate when: the dataset path or component-axis law, the `FieldCodec.Grid` derivation, the filter pipeline, or the create-only chunk law changes.

### [02.28]-[HDF5_GRADUATION_ENVELOPE]

- Seam: `hdf5-graduation-envelope`
- Class: domain
- Producer: `python:compute/experiments/model#ENVELOPE` — `GraduationEnvelope.fit`/`write` fitting reference bands from the training population and emitting the create-only h5py container.
- Consumers: `csharp:Rasm.Compute/Model/identity#MODEL_IDENTITY` — `GraduationEnvelope.Admit(HdfHandle)` re-running every Wellformed gate on the read roster.
- Payload: `wire-bytes` + `digest`
- Pin: DESIGN-PIN
- Blocker: no frozen envelope vector — the producer freezes one container when the fit lane executes; the DESIGN-PIN law forbids fabricated bytes.
- Shape: one root `bands` group carrying the `evidence-key` attribute as the 32-hex `ContentKey` rendering parsed `NumberStyles.HexNumber`; one group per feature carrying the `kind` attribute (`numeric`/`categorical`); numeric bands the `edges` float64[k] and `mass` float64[k+1] datasets (both outer bins covered — the consumer's half-open bisection addresses them), categorical bands the vlen-string `categories` and float64 `mass` datasets. Admission gates hold at BOTH ends: finite strictly-increasing edges, mass length edges+1, strictly positive mass summing to one within 1e-9, non-blank unique features and categories, non-zero evidence key. Reference mass is fitted by the python producer at graduation and never at the consumer; the reverse JSON `GraduationEvidence` leg keeps its own container.
- Regenerate when: the group/attribute roster, a band case, or a Wellformed gate changes.

### [02.29]-[SPARSE_EXCHANGE_CONTAINER]

- Seam: `sparse-exchange-container`
- Class: domain
- Producer: `csharp:Rasm.Compute/Tensor/factor#SPARSE_SOLVE` — `WriteArchive` emitting the scipy-convention group beside the `.mtx` `WriteExchange` leg.
- Consumers: `python:compute/solvers/linear#EXCHANGE` reads and reproduces both containers; `ReadArchive`/`ReadExchange` are the C# return legs of the same correspondence.
- Payload: `wire-bytes` + `digest`
- Pin: DESIGN-PIN
- Blocker: no frozen operator vector — no factorization corpus carries a byte-deriving instance on either branch.
- Shape: two containers carry one portable sparse-operator correspondence. `.mtx` serves SuiteSparse interop (pattern symmetry only); its HDF5 sibling carries the scipy sparse group convention — `indptr`/`indices` int32, `values` float64, extents in the `shape` int64[2] attribute, `format` naming the major axis — and the reproduction metadata `.mtx` drops: `kind` (factor-kind key), `ordering` (CSparse `ColumnOrdering` ordinal), `fill` (symbolic fill), `frobenius`, `symmetric`, and the applied AMD `permutation` as its own int32 dataset. Int32 index width is exchange law: an operand whose nnz or pointer run exceeds int32 refuses at write. Both ends re-run the full admission gates (extent congruence, monotone pointer run, index bounds, finiteness) because both routes end at each side's one admission fold.
- Regenerate when: the dataset/attribute roster, the int32 pin, or the admission gate set changes.

### [02.30]-[GRADUATION_EVIDENCE]

- Seam: `graduation-evidence`
- Class: domain
- Producer: `csharp:Rasm.Compute/Model/identity#GRADUATION_EVIDENCE` — `GraduationEvidence.Admit`/`Bundle` writing the canonical UTF-8 JSON payload under the CamelCase contract.
- Consumers: `python:compute/graduation/codegen#STUB_CODEGEN` — `StubCodegen.emit` decoding the bundle onto `EvidenceBundle`/`OwnerDescriptor`/`FieldNode` (`rename="camel"`).
- Payload: `wire-bytes` + `digest`
- Pin: DESIGN-PIN
- Blocker: no frozen bundle vector — the C# minter freezes one bundle beside its `BundleKey` when the mint lane executes.
- Shape: `schemaVersion` `"1"`; `owners[]` of `{name, fields[]}`; `bundleKey` the bare 32-hex content-key render parsed `NumberStyles.HexNumber`, never a raw integer; the `kind` discriminator literals `scalar|array|nested|mapping|optional|union` and the `FieldScalar` rows `i32|i64|f64|bool|string|key|bytes|decimal` are the decode contract at both ends; the scalar leaf's payload property spells `"scalar"` because CamelCase would seat it on the `"kind"` discriminator STJ refuses to double-book.
- Regenerate when: the kind literals, the scalar rows, the bundle columns, or `SchemaVersion` change.

## [03]-[DEBT]

Cross-branch flows the corpus observed crossing WITHOUT a binding entry; each row names the divergence and its resolution owner, and a row retires only by entry mint or recorded negative at both ends.

- `DoeDataset` — declared at BOTH ends (`csharp:Rasm.Compute/Solver/sweep` emitting, `python:data` ingesting) with the branch maps carrying the seam edge, yet no entry binds the bytes; the flow meets at Arrow IPC, which both ends already speak, so the resolution is an Arrow-payload entry minted by the C# producer or a recorded negative declaring the crossing content-key-only — convention-aligned columns until then fork on first edit.
