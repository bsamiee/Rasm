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
|  [14]   | BENCHMARK_CLAIM           | `benchmark-claim`           | infrastructure | `canonical-json`                | DESIGN-PIN |
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
|  [31]   | ENTITY_EDIT_WIRE          | `entity-edit`               | domain         | `canonical-json` + `digest`     | DESIGN-PIN |
|  [32]   | ORGANIZATION_WIRE         | `organization-wire`         | domain         | `wire-bytes` + `digest`         | DESIGN-PIN |
|  [33]   | SCENE_DESCRIPTOR          | `scene-descriptor`          | domain         | `wire-bytes` + `digest`         | DESIGN-PIN |
|  [34]   | OPLOG_ENTRY               | `oplog-entry`               | infrastructure | `wire-bytes` + `digest`         | DESIGN-PIN |
|  [35]   | TOLERANCE_WIRE            | `tolerance-wire`            | domain         | `wire-bytes` + `digest`         | REAL       |
|  [36]   | CHANGEFEED_ENVELOPE       | `changefeed-envelope`       | domain         | `wire-bytes` + `digest`         | DESIGN-PIN |
|  [37]   | JOURNAL_RELAY             | `journal-relay`             | domain         | `canonical-json` + `digest`     | DESIGN-PIN |
|  [38]   | TRANSMITTAL_NOTICE        | `transmittal-notice`        | domain         | `wire-bytes` + `digest`         | DESIGN-PIN |
|  [39]   | CESQL_CONFORMANCE         | `cesql-conformance`         | infrastructure | `wire-bytes` + `digest`         | REAL       |
|  [40]   | ARCHIVE_3DM               | `archive-3dm`               | domain         | `wire-bytes` + `digest`         | DESIGN-PIN |

Two appearance entries — `[02.17]` and `[02.18]` — conform to one shared definition, `appearance-vocabulary.schema.json`: the channel roster with its per-channel transfer, neutral, unit, mip policy, and minting branches; the ingest alias table; the transfer, normal-convention, alpha-mode, container, pack, plane-format, mip-policy, and KTX2-payload vocabularies; the three hex spellings; the level-ordered plane address; the egress grammar; and the spherical-harmonic band order with its golden vectors. Neither seam restates a row of it, and a document-local re-spelling is the fork the shared definition forecloses.

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
- Minters: `csharp:Rasm.Persistence/Version/commits#CRDT_ALGEBRA`; `python:runtime/transport/wire#CRDT_CODEC`; `typescript:core/interchange/codec#LANDING_EVIDENCE`
- Consumers: `typescript:core/interchange/format#MSGPACK_ENGINE` carries the message envelope into `typescript:core/state/merge`; `python:runtime/evidence/reproduction#SEED_REPRODUCTION` reproduces every mint in the parity suite.
- Payload: `wire-bytes`
- Pin: DESIGN-PIN
- Blocker: the message envelope, the `Beat` encoding, and the ascending-by-origin vector-slot order the `write`/`maintain` arms carry are pinned at three minters — a flat array with slot 0 the integer union tag and every arm leading with `Field`, `Beat` carrying `(field, origin, state, physical_ticks, logical)` — so the C# flat-framing mint is the remaining pin and no frozen vector stands against the three.
- Shape: a `CrdtOpWire` MessagePack op multiset over the `Set | Write | Add | Remove | Increment | InsertAfter | Delete | Maintain | Beat | Leave` union with the `Hlc` 16-byte cell; the convergence law folds divergent-delivery permutations of the same op multiset to byte-identical state under the join-semilattice `Merge`. Every branch authors ops as well as merging them, so each mints the union in its own types.
- Regenerate when: the `CrdtOp` union, the `Merge` algebra, or the message-envelope framing changes.

### [02.6]-[GLB_BY_KEY]

- Seam: `glb-by-key`
- Class: domain
- Producer: `csharp:Rasm.Compute/Runtime/tiles#TILE_PARTITION` over the GLB tessellation result, content-keyed through `csharp:Rasm.Compute/Runtime/codecs#CONTENT_ADDRESSING`.
- Consumers: `typescript:ui/viewer/scene` through `typescript:core/interchange/frame`; `python:geometry/mesh/daemon#DAEMON`.
- Payload: `wire-bytes` + `digest`
- Pin: DESIGN-PIN
- Blocker: producer must pin source geometry with `TessellationPolicy`; Bim tile-emit codec admission gates leaf-tile content emission.
- Shape: one GLB keyed by the `ContentIdentity` seed; geometry identity uses the seed-zero content key, not a policy-seeded interchange cache key. Tessellation is the producer's domain capability, and peers consume the keyed tile.
- Regenerate when: the tessellation policy, the content-key derivation, or the pinned source geometry changes.

### [02.7]-[HLC_TWO_HALF]

- Seam: `hlc-two-half`
- Class: infrastructure
- Minters: `csharp:Rasm/Domain/frame#RECEIPT_PORT`; `python:runtime/evidence/clock#CLOCK`; `typescript:core/value/clock#TWO_HALF_LAYOUT`
- Consumers: `python:runtime/transport/serve#SERVE` decodes the halves with `tenant`; `python:runtime/evidence/reproduction#SEED_REPRODUCTION` reproduces every mint in the parity suite.
- Payload: `wire-bytes`
- Pin: DESIGN-PIN
- Blocker: the C# minter's capsule lacks the indexed two-half vectors the fixture freezes.
- Shape: two-64-bit-half stamps in the `ReceiptEnvelope` order — physical half first as the `Instant` Unix-epoch tick `int64`-LE where ONE TICK IS 100 NANOSECONDS, logical half second as the monotone `ulong`-LE, the exact order `InterchangeIdentity.Compose` seals — with vectors chosen so a logical-half-first composition corrupts by folding a fresh op as stale. Layout fixes the UNIT: a minter whose platform reads milliseconds scales onto the tick axis rather than transcribing, and the physical half's mint domain is the signed-64-bit range inside its unsigned cell, reaching 31197-CE against the year-292-million ceiling a millisecond half carries. Every branch stamps its own causal frames, so each mints the layout rather than reading a peer's.
- Regenerate when: the two-half compose order or the `ReceiptEnvelope` stamp layout changes.

### [02.8]-[IFC_WIRE]

- Seam: `ifc-wire`
- Class: domain
- Producer: `csharp:Rasm.Bim/Exchange/wire#WIRE_PROJECTION`
- Consumers: `typescript:core/interchange/codec`; `python:geometry` IfcOpenShell — each projects its own graph and reproduces the GraphKey.
- Payload: `wire-bytes` + `digest`
- Pin: DESIGN-PIN
- Blocker: producer lacks the canonical IFC payload the `WireParity` row freezes.
- Shape: `IfcWire` is one raw artifact `{format, bytes, schema, content, at}`.
- Shape: Format stores `InterchangeFormat.Key`, schema projects `ReleaseVersion.Key`, and bytes remain the IFC payload.
- Shape: At projects the producer mint `Instant`.
- Shape: Content is `ContentAddress.OfGraph`, whose GraphKey proves cross-runtime semantic parity.
- Shape: Byte equality proves C# host-local re-seal only.
- Shape: GeometryGym-backed IFC exchange remains the C# producer's domain capability.
- Shape: Python's IfcOpenShell peer decodes the raw bytes and projects its own graph.
- Regenerate when: the canonical authoring order, the `ContentAddress.OfGraph` law, or the pinned corpus payload changes.

### [02.9]-[DESCRIPTOR_DRIFT]

- Seam: `descriptor-drift`
- Class: infrastructure
- Minters: `csharp:Rasm.Materials/Appearance/interchange#TEXTURE_EGRESS` (`rasm/channels/v1/channels.proto`, the `rasm.channels.v1` descriptor source, `python:runtime/transport/shapes#VOCABULARY` its co-transcribed roster); `csharp:Rasm.Element/Graph/wire#WIRE_CODEC` (`rasm/element/v1/element.proto`, the `rasm.element.v1` graph source); `csharp:Rasm.Compute/Runtime/wire#CONTRACT_EVOLUTION` (`rasm/compute/v1/compute.proto`, the `rasm.compute.v1` suite service source); `csharp:Rasm.Rhino/Document/layers#ORGANIZATION_PROJECTION` (`rasm/organization/v1/organization.proto`, the `rasm.organization.v1` host-free organization source); `csharp:Rasm.Rhino/Render/settings#SUN_ASTRONOMY` (`rasm/scene/v1/scene.proto`, the `rasm.scene.v1` captured-scene source); `python:runtime/transport/shapes#REGISTRY_AND_DRIFT`; `typescript:core/interchange/contract#DRIFT_VERDICT`
- Consumers: each minter's gate reads the snapshot pair it owns; no branch reads a peer's verdict.
- Payload: `descriptor-set`
- Pin: DESIGN-PIN
- Blocker: every source this entry rosters carries a landed `.proto` beside a frozen `FileDescriptorSet` snapshot minted at one parity setting — single file in the set, no `include_imports`, no `source_code_info`: each source stamping the `option csharp_namespace` its own package derives — `rasm.<family>.v1` spells `Rasm.<Family>` — so a family owns its generated identity and a later source stamps its own without re-deciding, where one namespace shared across packages demands globally-unique message names estate-wide. Reachability closes with it: every peer decode of `rasm.element.v1` now resolves in a pool `typescript:core/interchange/format#PROTO_ENGINE`, `codec`, and `python:runtime/transport/shapes#REGISTRY_AND_DRIFT` each compile the same source into. EXECUTION is the residual gap: `buf` mints all three snapshots at the corpus, while no branch source tree exists yet to run `Grpc.Tools`, `grpc_tools.protoc`, or `protoc-gen-es` over those sources and compare its own emission byte-for-byte, so the three-minter parity this pin graduates on holds by construction and stays unrun at every peer.
- Blocker: the gate's rule half is CLOSED. Root `buf.yaml` declares `tests/contracts` as buf's one module under the v2 schema, `lint` holds `STANDARD` less the two rpc-naming rules `csharp:Rasm.Compute/Runtime/wire#PROTO_VOCABULARY` displaces and one path-scoped `RPC_REQUEST_RESPONSE_UNIQUE` waiver for the two types the suite deliberately shares, and `breaking` holds `FILE`. Every verb runs from `node_modules/.bin/buf` exactly as each workspace tool does, and `tests/contracts/.api/bufbuild-buf.md` owns the command, config, and rule surface.
- Shape: each proto source emits a `FileDescriptorSet`; the gate gives `Identical`/`Additive`/`Breaking`; numbers only append, and removals reserve name and number. Each descriptor source is one mint unit, so a branch owning two sources carries two snapshot rows under one gate law and the forked-parity defect is two snapshots of one source. Each source's snapshot freezes beside it as `rasm/<family>/v1/<family>.descriptor.binpb`, minted by `buf build --path <source> --as-file-descriptor-set --exclude-imports --exclude-source-info`, so a later source lands into a spelled home rather than re-deciding the layout. That snapshot is the PARITY digest and never the breaking baseline: excluding imports is what makes three minters agree byte-for-byte without their protoc bundles' well-known-type descriptors entering the comparison, and the same exclusion leaves the set unresolvable as an image, so `buf breaking --against <snapshot>` refuses any importing source. FILE gating reads a git ref instead — `buf breaking --against '.git#branch=main' --against-config buf.yaml`, the config flag evaluating the baseline under the current rules so a rule edit takes effect on the same run.
- Regenerate when: any owning `.proto` contract changes.

### [02.10]-[CONSUMPTION_PROFILE]

- Seam: `consumption-profile`
- Class: infrastructure
- Minters: `csharp:Rasm.AppHost/Runtime/profiles#PROFILE_AXIS`; `python:runtime/execution/admission#CONTEXT`; `typescript:runtime/proc/config#ADMISSION_ROWS`
- Consumers: every package reading a supplied profile row instead of assuming a deployment shape; each branch resolves the row at its own admission owner.
- Payload: `canonical-json`
- Pin: DESIGN-PIN
- Vector: the frozen preimage is the unhosted cli row — `tenancy` none, `topology` cli, `host` absent, `lifecycle` caller-owned, `isolation` in-proc, `providers` empty — rendering `{"tenancy":"none","topology":"cli","host":"none","lifecycle":"caller-owned","isolation":"in-proc","providers":""}` at all three mints (`csharp:Rasm.AppHost/Runtime/profiles#PROFILE_AXIS` `ConsumptionProfile.CanonicalJson()`, `python:runtime/execution/admission#CONTEXT` `ConsumptionProfile.canonical_json()`, `typescript:runtime/proc/config#ADMISSION_ROWS` `Profile.canonicalJson`). It selects the absent-host arm on purpose: `host` renders the `none` key each branch substitutes for an absent descriptor, and the empty `providers` join is the one cell where the three sort comparators — `StringComparer.Ordinal`, Python `sorted`, and the default JavaScript comparator — cannot disagree, so the vector pins the ROSTER order and the escape posture without also pinning a comparator the printable-ASCII bound below already reconciles.
- Blocker: EXECUTION alone — the vector above is derived from the three renderers on disk and no branch has yet run its own encoder against it, so parity holds by construction and stays unproven at every peer.
- Shape: one canonical row over the axis roster `libs/.planning/ARCHITECTURE.md` `[10]-[CONSUMPTION_MODEL]` fixes — a closed axis carrying its fixed vocabulary, an open axis carrying consumption-descriptor rows its supplying branch names — the composition root supplying the row and each minter spelling the same axes in its own types; that section owns the axis set, each axis's form, and the refusal contract. Each branch spells the four closed axes as exhaustive vocabularies in its own type system and the two open axes as one common descriptor shape — a key beside the columns `[CONSUMPTION_DESCRIPTOR]` fixes, each family's own extension columns riding beside them — whose rows and capability vocabulary stay branch-owned, and no row restates a closed axis — an unservable value refuses at admission naming its axis rather than riding a descriptor column. Every preimage cell is bounded to printable ASCII, since the three branches escape a JSON string literal through three different encoders whose non-ASCII postures split — one emits `\uXXXX` where two emit raw UTF-8 — so a descriptor key reaching outside that alphabet forks the preimage the vector pins. Refusal is one grammar everywhere: axis, value, and reason.
- Regenerate when: the Tier-0 roster gains an axis, or a closed axis gains a value.

### [02.11]-[BACKEND_CONTRACT]

- Seam: `backend-contract`
- Class: infrastructure
- Minters: `csharp:Rasm.Persistence/Store/schema#IDENTITY`; `typescript:data/lane/capability#CONTRACT`; `python:runtime/execution/admission#BACKEND_CONTRACT`
- Consumers: `typescript:iac/operate/converge#PROJECTION` realizes the merged generation onto the deployment plane; each branch's runtime admission verifies the generation it observes.
- Payload: `canonical-json` + `digest`
- Pin: DESIGN-PIN
- Blocker: no minter has frozen its canonical artifact set, so the generation digest carries no pinned preimage.
- Shape: one branch contribution is the artifact set sorted by artifact key under one ordinal total order over a key alphabet bounded to printable ASCII, since the branch comparators split above it — `StringComparer.Ordinal` and the default JavaScript comparator order by UTF-16 code unit where Python `sorted` orders by codepoint, inverting every key pair mixing an astral character with `U+E000`-`U+FFFF` — so the bounded alphabet is what makes the three orders one order, and a key admitting a non-ASCII character forks the generation. Each artifact carries key, role, canonical bytes, its provider set, and its dependency key set, every collection deduplicated and ordinally sorted before framing, with generation identity the seed-zero `XxHash128` digest over the framed canonical artifact stream. Minters digest the stream they frame and verifiers digest the bytes they receive, so no path re-encodes a decoded document and compares — `[02.1]-[CANONICAL_BYTE_IDENTITY]` fixes why a re-encode never reproduces foreign bytes, and a branch digesting its own re-encode mints a generation its peers cannot match from the same artifacts. Artifact key order is the whole order, so a dependency-depth or topological rank inside the stream mints a second generation from one artifact set. Dependency keys are digest-bearing payload the projection funnel proves closed and acyclic, so every mint path carries the proof and no path validates by sorting. Capability rows carry key, lane, requirement, and value beside a `failureRank` of `required | degradable | observational` and a `restartClass` of `session | reload | restart`, whose rank order is load-bearing so an aggregated repair reports the worst disruption across its gap set rather than the least; the extension roster filling those rows is branch-owned deployment state, never a corpus set. Polyglot merge unions branch contributions by artifact key under that same order and refuses any key two branches claim with differing content, artifact and capability rows alike, so neither first-wins nor last-wins resolves a collision.
- Regenerate when: the artifact ordering, the preimage framing, or the generation-identity derivation changes.

### [02.12]-[CAPABILITY_DESCRIPTOR]

- Seam: `capability-descriptor`
- Class: domain
- Producer: `csharp:Rasm.AppHost/Agent/capability#SDK_CODEGEN` pins the broker registry through `SuiteContracts.Schema`.
- Consumers: `typescript:core/interchange/invoke#CAPABILITY_BIND` decodes and grades the producer document without transport coordinates.
- Payload: `canonical-json` + `digest`
- Pin: DESIGN-PIN
- Blocker: DISCHARGED — `DescriptorPin.Of(registry, wire)` addresses one ordinal, fixed-field JSON document.
- Shape: each row is `descriptor`, `surface`, `effect`, `idempotency`, `scope`, `units`, `input`, then `output`.
- Shape: `units` is the descriptor's complete fixed-plus-variable `CostModel` roster in ordinal key order.
- Shape: `input` is the `CommandArguments` schema, and `output` is the `CommandReceipt` schema from `SuiteContracts.Schema`.
- Shape: live `DiscoveryResult` carries the same `units` roster beside its fixed-cost `estimated` vector.
- Shape: the pin owns no Connect service or method coordinate; a consumer decodes the producer catalog and re-authors none.
- Regenerate when: the descriptor vocabulary, the exported schema layout, or the effect and cost key sets change.

### [02.13]-[TELEMETRY_CONVENTION]

- Seam: `telemetry-convention`
- Class: infrastructure
- Minters: `csharp:Rasm.AppHost/Observability/telemetry#CONFORMANCE_PROJECTION`; `python:runtime/observability/telemetry#TELEMETRY`; `typescript:core/observe/convention#IDENTITY_PROJECTION`
- Consumers: `typescript:iac/operate/observe#STORE_ROWS` reads the translation strategy the wire row pins and `#CHART_ROWS` the collector receiver and exporter rows compiled from it; each branch's composition root realizes its own mint and asserts it through that branch's telemetry harness.
- Payload: `canonical-json` + `digest`
- Pin: DESIGN-PIN
- Blocker: the C# minter projects its rows as a document — `Conformance.Of(TelemetryComposition)` at `csharp:Rasm.AppHost/Observability/telemetry#CONFORMANCE_PROJECTION` returns the `ConformanceDocument` carrying the branch's role, schema url, and ordinally-joined `ConformanceRow` set, each row a case whose disposition and payload move together and whose `Owner` column names the member the value projects off — while the python telemetry owner and the typescript convention rows still hold their values as composition code with no canonical projection member, so two of three branches carry no byte-deriving input and the parity this entry proves has nothing to compare against.
- Shape: the closed conformance row set `libs/.planning/ARCHITECTURE.md` `[08]-[OBSERVABILITY_CONFORMANCE]` legislates, projected as one document per minter — the resource triple and its detector-merge precedence, the scope coordinate (emitting package id, version stamp, semconv schema url) shared by tracer, meter, and logger, the metric-name grammar with its UCUM unit vocabulary and the minter's own domain roster of segment beside admitted subject, wire temporality and default histogram aggregation, exemplar posture, the propagation dialect set with its one global composite registration and the inbound parent-adoption posture, the tenant key with its promotion allowlist and view cap, the absent-entry posture every sometimes-absent dimension shares — tenant, level-family, and substrate keys read as the untagged whole when absent, stay rostered on the allow-list while sometimes absent, and a key that is its cell's own identity never constructs the absent state, egress protocol and compression beside the metrics-receiver translation strategy, buffer and loss posture, and lifecycle drain bounds. Role-aware absence is part of the shape rather than an exception to it: each row carries the disposition its minter's role admits over `process` | `browser` | `deploy`, so a browser minter marks the process profiler, the process health probe, and the process support archive `absent` while carrying logs, metrics, traces, vitals, and crash evidence, and a reader distinguishes a role that cannot carry a signal from a branch that dropped one. Capability absence rides the same column under its own disposition and never under role absence: a row whose plane exposes no seat at the minter's pinned distribution — the exemplar filter and the sender encoding knob are the standing pair — projects the value the plane carries beside the pin that withholds the seat, so the digest reads a stated ceiling rather than a dropped row and a pin bump that opens the seat fails here until the row re-values. Every branch mints from its own composition inputs and parity across the three mints IS the conformance; transcription remains how each branch spells the rows, and this entry is what proves the three spellings agree. Domain rows carry that proof furthest: two minters claiming one segment project one subject spelling byte-identical or fail the digest here, so a vocabulary each branch declares locally still resolves to one estate namespace.
- Regenerate when: Tier-0 `[08]-[OBSERVABILITY_CONFORMANCE]` gains, drops, or re-values a row, or the semconv schema pin bumps.

### [02.14]-[BENCHMARK_CLAIM]

- Seam: `benchmark-claim`
- Class: infrastructure
- Minters: `csharp:Rasm.Compute/Runtime/claims#TS_PROJECTION`; `typescript:core/interchange/codec#LANDING_WIRE`
- Consumers: `typescript:core/observe/board#BENCH` grades a decoded claim against its baseline and trends the bench pack; `typescript:security/crypt/sign#CALIBRATION` and `typescript:runtime/proc/exec#MEASURED_RUN` mint through that landing class and grade each row against its own ceiling; `csharp:Rasm.Compute/Runtime/claims#CLAIM_ROW` admits the claim the projection carries and `csharp:Rasm.Compute/Runtime/channels#TRANSPORT_AXIS` folds it into the winning wire encoding, both resolving through the durable summary at `csharp:Rasm.Persistence/Query/cache#BENCHMARK_INDEX`.
- Payload: `canonical-json`
- Pin: DESIGN-PIN
- Blocker: neither minter freezes a claim document — the C# equivalence sweep pins no host fingerprint and the typescript harness pins no suite, so the shape carries no byte-deriving input on either branch.
- Shape: `BenchmarkClaimWire` is `{suite, host, minted, metrics}`, and metrics is non-empty.
- Shape: Host and minted live once per measurement document rather than repeating on each metric.
- Shape: Each metric carries polarity, subject, band, and cost columns.
- Shape: Polarity is `minimize | maximize`, projected from admitted `BenchmarkClaim.Polarity.Key`.
- Shape: Duration rows minimize, while throughput and score rows can maximize through the same mint.
- Shape: Bare subjects are `{label, unit, modality}`.
- Shape: Kernel subjects add tensor input, substrate, family, case, route, provider, corpus key, and artifact key.
- Shape: Kernel subjects also carry equivalence deviation, tolerance class, and profile artifacts.
- Shape: Bare and kernel subjects are distinct arms, so neither widens the other into optional fields.
- Shape: Sampling bands carry the raw vector, tick count, and full rung ladder.
- Shape: Equivalence summaries carry median, p95, and deviation.
- Shape: Grading policy names the rung it reads and refuses a pair missing that rung on either side.
- Shape: Enrichment bands the executing runtime cannot fill remain absent.
- Shape: Each harness owns its non-empty unit vocabulary, and grading compares unit verbatim as an equality axis.
- Shape: Timing, render, and C# sweep units remain independent from the telemetry instrument census.
- Shape: Claims whose host differs from the executing identity refuse before comparison.
- Shape: Consumers never compare measurements across fingerprints or grade a peer runtime.
- Shape: Each runtime branch mints its own claims, and parity across those mints is conformance.
- Shape: Branches whose benchmark evidence stays local mint no claim.
- Regenerate when: the subject union, the band ladder, or the host-admission gate changes.

### [02.15]-[HOST_FINGERPRINT]

- Seam: `host-fingerprint`
- Class: infrastructure
- Minters: `csharp:Rasm.AppHost/Runtime/determinism#DETERMINISM_KERNEL` (`EnvFingerprint` composing the record and `EnvFingerprint.Digest` minting its print through the kernel content-hash entry); `typescript:ui/viewer/probe#HOST_MIRROR` (`Probe.host`, the browser-role capture)
- Consumers: `csharp:Rasm.Compute/Runtime/claims#CLAIM_ROW` reads the record as the claim's `host` column and refuses a claim whose print differs from the executing identity; `typescript:core/interchange/codec#LANDING_WIRE` decodes the wire into `Claim.Host`; `typescript:ui/viewer/probe#CLAIM_BOARD` joins the local capture against an admitted claim's own host to display divergence context
- Payload: `canonical-json` + `digest`
- Pin: DESIGN-PIN
- Blocker: `typescript:ui/viewer/probe#HOST_MIRROR` pins no adapter capture, so the browser minter carries no frozen document and the two-role parity this entry proves stands on one side alone. Both minter forks this entry once carried are CLOSED: the role-unreachable sentinel is `unreachable` at BOTH minters (the TS `<unavailable>` spelling retired), and `stamps` crosses as ORDERED `[key, value]` pairs at both ends — `HostFingerprintWire.Canonical()` derives bytes from container order, which a keyed object cannot publish.
- Shape: one environment-identity record — `print` the digest identifying the environment, `machine`, `os`, `arch`, `processors`, `runtime`, and `stamps` as ordered `[key, value]` pairs carrying the dimensions a role reaches beyond the fixed columns. Role-aware absence is part of the shape rather than an exception to it: a process minter composes every column from its own host and derives `print` over them, while a browser minter takes `print` as the identity it was handed, reads `machine` and `arch` off the graphics adapter, and marks the operating-system name with the `unreachable` sentinel because no stable browser surface exposes it — so a reader distinguishes a role that cannot reach a fact from a branch that dropped one, and a fabricated value is unspellable. Registration lands here rather than resolving as `[02.14]`'s column alone because the record crosses on its own seam edge as well as inside a benchmark claim: the viewer mirrors the field set with no claim in hand, so the two spellings agree even where no claim exists to carry them. Every branch reaching a host mints its own record and parity across the mints IS the conformance — a branch that only decodes carries no fingerprint of its own.
- Regenerate when: the record's column set changes, the sentinel spelling for a role-unreachable fact changes, or the print derivation moves.

### [02.16]-[BOARD_PACK]

- Seam: `board-pack`
- Class: infrastructure
- Minters: `csharp:Rasm.Compute/Runtime/board#TS_PROJECTION`; `typescript:core/observe/board#PACKS`
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
- Consumers: `typescript:core/interchange/codec#WIRE_CENSUS` carries the family and `#LANDING_WIRE` lands it as `TextureSet`; `typescript:data/object/asset#ASSET_GATE` projects each `ktx2` channel row onto the ktx declaration and re-proves it against the fetched container; `typescript:ui/viewer/scene#APPEARANCE_BIND` seats it through `Pbr.seat`/`Pbr.index` over the served asset directory; `python:runtime/transport/shapes#VOCABULARY` decodes it on the `texture_set_wire` row, decode-only.
- Payload: `wire-bytes` + `digest`
- Pin: DESIGN-PIN
- Blocker: the producer has pressed no concrete set — `Rasm.Materials` is a fences-only planning surface whose `Raster/` press estate holds no realized source, so no runtime mints the channel-ordered plane-digest preimage the set key streams over. Its descriptor half is closed: `rasm/channels/v1/channels.proto` carries the texture messages both decode legs bind.
- Shape: conforms to `appearance-vocabulary.schema.json` — the appearance-coupled BAKED set document, riding BEHIND the `AppearanceKey` the seam `AppearanceSummary` freezes over its seven-value preimage — the set key is a payload field, never a summary column, because widening `AppearanceSummary` forks the Bim dedup key and re-ids every appearance node. It carries NO kind discriminant: its one kind is the baked PBR set, and an environment or IBL product rides `[02.18]` instead. Field names are `camelCase` — the mechanical projection of the producer's own members under `JsonSerializerDefaults.Web`, never a third spelling — and every hex field carries the uppercase `ContentAddress` spelling the shared definition's `keySpelling` fixes. Each document declares the set extent, its layer law and layer count, the ingest-source normal convention, the set-level alpha mode, the millimetre span the normalized `height` plane resolves against, the tiling coherence a synthesis gate proved, the Mari tile indices when the set is UDIM, the material and conductor identities, the capture provenance the appearance wire already owns, and one press receipt. Channel and pack rows each carry a LEVEL-ORDERED list of address triples under the shared `planeLevels` law: a self-pyramiding container holds one entry whatever the declared depth, and a container holding no pyramid of its own holds one entry per level, so a pyramid is addressed whole and every level is digested. Each row also carries its container beside its storage format, because the alpha-association conversion the shared definition fixes selects on the container and no other column recovers it; the payload column admits only the wire-legal KTX2 classes, and a desktop-native block payload refuses at decode. Baked bytes are ALWAYS CPU-minted: the press receipt's backend column reads `cpu` on every receipt reaching the wire, the GPU lane is an accelerator whose products carry no set and therefore no key, and its divergence column stays absent until a parity run measures it so a zero never reads as a perfect match. Plane bytes cross as content-addressed blobs in the write-once object store; the tessellation entry `[02.6]` stays untouched, because planes never embed in tiles and a texture edit never re-keys a tessellation. Baked appearance is the producer's domain capability, so peers decode the document, join its leaves to the served asset directory, and re-derive no address and no key.
- Regenerate when: `appearance-vocabulary.schema.json` gains, drops, or re-values a row; the document's field roster changes; or the channel-ordered key preimage moves.

### [02.18]-[ASSET_SET_MANIFEST]

- Seam: `asset-set-manifest`
- Class: domain
- Producer: `python:artifacts/graphic/texture/set#TEXTURE_SET`, its egress leaves joined at `python:artifacts/graphic/texture/set#EGRESS`
- Consumers: `typescript:core/interchange/codec#WIRE_CENSUS` carries the family and `#LANDING_WIRE` lands it; `typescript:runtime/browser/fetch#RUNNER_ENTRY` decodes it on the worker `Survey` arm and `#DEPOT_SCHEDULER` folds the dome the `typescript:ui/viewer/scene#ENVIRONMENT_FOLD` port composes; `csharp:Rasm.Materials/Raster/set#SET_INGEST` decodes it as a classification input through `SetIngest.Peer` and refuses a non-`pbr_set` kind.
- Payload: `wire-bytes` + `digest`
- Pin: REAL
- Shape: conforms to `appearance-vocabulary.schema.json` — the ingest-assembled and environment-assembled set manifest, the SECOND appearance producer, distinct from `[02.17]` by capability rather than by language rank: it discriminates `pbr_set`, `hdri`, and `ibl` on a kind column, records the ingest root or generator id, and carries the classification residue no alias claimed, none of which the baked document holds. Environment and IBL products ride HERE and never the baked document, which carries no environment kind. Field names are `snake_case` because the vocabulary row binds each declaration to its generated message under preserved proto field names — the declaration IS the wire contract, with no rename layer — and every digest carries the lowercase `ContentKey` spelling the shared definition's `keySpelling` fixes, so a consumer joining a key across the two documents lowers and never uppercases. Map and pack rows carry the same LEVEL-ORDERED address list, container column, and payload legality the baked document carries, beside the producing tool and its recorded version PER MAP, because one set legitimately mixes the spawned encode floor with an in-process acceleration leg and a set-level tool column erases which leaf came from which. Its environment leg carries the irradiance harmonics under the shared band order and layout, the equirect source, the roughness-indexed prefilter pyramid as address triples rather than a bare file roster, the split-sum BRDF table, the importance-sampling guide, and the read-side intensity and rotation — read-side because re-orienting or re-exposing a dome re-keys no blob and triggers no re-prefilter, and a producer baking either into the coefficients forks every consumer reading the same digest at another orientation. Ingest classification and environment assembly are the producer's domain capability; the C# consumer folds a decoded manifest into its own ingest intent and classifies nothing the manifest already resolved.
- Expectation: the frozen assets ride `asset-set-manifest/` — `asset_set_manifest.bin` (476 wire bytes, deterministic proto serialization, seed-zero digest `87aa8c48b73c71fc6d9d131a57331a77`), `asset_set_manifest.json` (the `preserving_proto_field_name` canonical projection), and the three byte-deriving planes under `planes/`. Byte input is settled-design-determined: 8×8 planes over `idx = y·8 + x` row-major — `base_color` u16 `((idx·1021) mod 65536, (idx·2039) mod 65536, (idx·4093) mod 65536)` as `png16`, `geometry_normal` the constant u16 neutral `(32768, 32768, 65535)` as `png16`, `height` float32 `idx/63` as `exr` — encoded through the pinned `imagecodecs` legs whose versions each map row's `tool_version` records. Frozen values the producer emit must reproduce: plane digests (policy-folded `texture-plane` namespace, lowercase `ContentKey` wire spelling) `base_color = 72b07d26416e03d501713d3781dd99c2` (134 B), `geometry_normal = 1cb09264272c513b6a78a6f83566ce47` (82 B), `height = 40f4dd4b6fbdf17d18f3d04bbac4e31b` (578 B); `manifest_key = adf06145b592fc08fedd963c5170f974`, the `texture-set` merkle over the three 16-byte little-endian digests in roster order; document facts `kind = pbr_set`, `normal_convention = gl`, `alpha_mode = none`, `height_scale = 10.0` mm, `license_class = permissive`, `ktx_payload = none` and `mips = 1` on every row. That landed instance schema-validates against `asset-set-manifest/contract.schema.json`, round-trips byte-identically through the generated message, and re-derives every digest and the manifest key from the landed plane bytes.
- Regenerate when: `appearance-vocabulary.schema.json` gains, drops, or re-values a row; the document's field roster changes; or the roster-ordered merkle fold moves.

### [02.19]-[MATERIAL_WIRE]

- Seam: `material-wire`
- Class: domain
- Producer: `csharp:Rasm.Materials/Appearance/interchange#MATERIAL_WIRE`
- Consumers: `typescript:core/interchange/codec#LANDING_WIRE` lands `Material` and `PbrGroups` for `typescript:ui/viewer/scene#APPEARANCE_BIND`; `python:runtime/transport/shapes#VOCABULARY` decodes both on its appearance mirror rows, decode-only. Seam `AppearanceSummary` is NOT a shape of this crossing: no producer emits a standalone summary document — it crosses as the `rasm.element.v1` `AppearanceWire` payload inside `NodeWire` (`csharp:Rasm.Element/Graph/wire#WIRE_CODEC`), and each peer's `AppearanceSummary` landing shape seats that payload, never a document from this codec.
- Payload: `wire-bytes` + `digest`
- Pin: DESIGN-PIN
- Blocker: the producer has pinned no concrete material, so the appearance content hash carries no byte-deriving input.
- Shape: the OpenPBR parameter algebra as it crosses — the layered surface projection, its group wire over the OpenPBR Surface 1.1 inputs, the conductor key, the capture receipt, and the appearance content hash the seam summary keys. Production is SOLE: peers mirror the projection field for field and a peer-side lowering, conductor table, colour conversion, or key derivation is the cross-language drift defect registered here — a decoded group carries verbatim onto a consumer's own material, and deriving one field from another is the standing defect the single-producer law forecloses. Colour crosses as the scene-linear triple the graph's working space fixes, never a display-encoded byte triple. Registration covers a PRE-EXISTING crossing rather than a new one: the typescript branch already decodes both shapes and the python branch already rosters them, so until this entry landed a shape crossed three branches with no contract binding it — convention-aligned interop that forks on first edit. Map and texture-transform fields stay OFF the group wire by ruling: a baked plane set is `[02.17]`, and widening the group wire with map digests seats a second appearance producer behind one shape. Schema authority is the producer's MessagePack integer-keyed record roster — appended keys past the frozen block, mirrored structurally at each peer under the mirror-census law — and the family holds NO descriptor source under `[02.9]` because the wire is not proto-shaped.
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
- Producer: `csharp:Rasm.AppHost/Runtime/ports#WIRE_LAW` — the source-generated System.Text.Json roster, one `[JsonSerializable]` row per family, carried by TWO package contexts that `SuiteContracts.Wire(...)` merges into the one options identity built on `JsonSerializerOptions.Strict` with an explicit `PropertyNamingPolicy = JsonNamingPolicy.CamelCase`. `AppHostWireContext` carries `ReceiptEnvelopeWire`, `TenantContextWire`, `CommandAvailabilityWire`, `CredentialPemWire`, `FlagVerdictWire`, `SupportCaptureWire`, `HopReceiptWire`, `DeliveryReceiptWire`, `DropReceiptWire`, `OutboxRowWire`, `DeadLetterRowWire`, `ReplayTallyWire`, `OutboxLaneWire`, and `OutboxSweepWire`; `csharp:Rasm.AppHost/Wire/livewire#TS_PROJECTION`'s `LiveWireContext` carries `BindingStatusWire`, `CoercedValueWire`, `WriteReceiptWire`, and their two union lowerings `WriteBackWire` and `QualityWire`, beside the withdrawn `MachineObservationWire`.
- Consumers: `typescript:core/interchange/codec#WIRE_CENSUS` census rows sourced `Rasm.AppHost` decode each family under the census `json` arm into its landing class; `typescript:ui` dashboard surfaces read the decoded `ui`-consumer rows.
- Payload: `canonical-json` + `digest`
- Pin: DESIGN-PIN
- Blocker: the producer freezes no family vector — every family crosses as source-generated JSON with no pinned instance, so no family carries a byte-deriving input on either branch.
- Shape: the AppHost runtime-evidence family set as ONE registration — `ReceiptEnvelopeWire`, `TenantContextWire`, `CommandAvailabilityWire`, `CredentialPemWire`, `BindingStatusWire`, `CoercedValueWire`, `WriteReceiptWire`, `FlagVerdictWire`, `SupportCaptureWire`, `HopReceiptWire`, `DeliveryReceiptWire`, `DropReceiptWire`, `OutboxRowWire`, `DeadLetterRowWire`, `ReplayTallyWire`, `OutboxLaneWire`, `OutboxSweepWire` — each family one `[JsonSerializable]` row on the producer roster and one census row at the consumer, field names the mechanical camelCase projection of the producer's own members, whose one author is the explicit `PropertyNamingPolicy = JsonNamingPolicy.CamelCase` the `Strict`-based merge sets: `Strict` carries no naming policy of its own, so the camelCase field-name law is that assignment rather than a preset default. Membership in this roster IS the cross-language discriminant: a family here holds a decoder row on the consumer census, while a C#-only receipt (`SandboxReceipt`, `UpdateReceipt`, `FenceReceipt<LeaseKey>`, the companion `VerbReceipt`/`CascadeReceipt`/`BindReceipt`/`Delivery`) rides `ReceiptEnvelopeWire` through the same merged context as a `[JsonSerializable]` row alone and carries no family row here. Two family names FORK across the boundary: `ReceiptEnvelopeWire` and `TenantContextWire` name the TS faces of the C# records `ReceiptEnvelope` and `TenantContext` that `csharp:Rasm.AppHost/Runtime/ports#WIRE_LAW` registers, the `Wire` suffix being minted at the TS projection, so a census join resolves the pair as one family under two spellings and neither end renames. Eight message-plane families project through generated Mapperly seams (`csharp:Rasm.AppHost/Wire/outbound#TS_PROJECTION` `OutboundMap`, `Wire/outbox#TS_PROJECTION` `OutboxMap`, `Wire/topics#TS_PROJECTION` `TopicsMap`) rather than hand transcriptions, so a producer column rename breaks the projection at build rather than at a peer decode. Two crossings are DECLARED narrowings the census must honor: the HLC ordinal crosses as a decimal STRING on `OutboxRowWire`, `DeadLetterRowWire`, and `OutboxSweepWire` because a `ulong` past 2^53 loses precision in a JSON number and exact comparison is the ordinal's whole purpose, and the hop and delivery measurement pair (`attempts`, `elapsedSeconds`) crosses ABSENT together on any leg that reached no pipeline, so a decoder reading either as zero reports a dial that never happened. Both `LiveWireContext` union lowerings carry no row of their own, and they carry none for OPPOSITE reasons: `WriteBackWire` crosses INSIDE `WriteReceiptWire.disposition` as a `kind`-discriminated literal union, so `WriteReceiptWire`'s census row decodes both, while `QualityWire` crosses INSIDE `MachineObservationWire.quality` and is therefore withdrawn with the carrier below — a census row for a quality union no decoded family reaches strands a decoder against nothing. `MachineObservationWire` is WITHDRAWN from this seam — its producer (`Wire/livewire#MACHINE_LANE`), its decoder (`Observability/instruments#RECEIPT_PROJECTION` `ReceiptKind.Observation`), and its readers (the `Rasm.Fabrication` wear, fleet, and engagement consumers) are all C#, so it is an in-process receipt payload and a registration asserting a peer decoder no branch declares is the stranded state this manifest exists to refuse. Carriage is JSON by the producer's landed wire law, so the consumer census carries these rows under its `json` arm, never `proto` — no descriptor source exists or is owed under `[02.9]`. Families a sibling entry already owns register THERE and not here: `HlcStampWire` byte layout is `[02.7]`'s shape (this producer carries it as a message-envelope field), `CapabilityDescriptorWire` is `[02.12]`, `HostFingerprintWire` is `[02.15]`, and `BenchmarkClaimWire` is `[02.14]` with `csharp:Rasm.Compute` the minter — a census row sourcing it to AppHost mis-names the producer. Host runtime evidence is the producer's domain capability, so peers decode the families and re-author none.
- Regenerate when: a family's field roster changes, either producer context gains or retires a family, or the merge's `JsonSerializerOptions.Strict` base or its explicit `JsonNamingPolicy.CamelCase` assignment moves.

### [02.22]-[APPUI_WIRE]

- Seam: `appui-wire`
- Class: domain
- Producer: `csharp:Rasm.AppUi/Shell/commands#TS_PROJECTION` mints the command payload and gate families.
- Producer: `csharp:Rasm.AppUi/Shell/controls#TS_PROJECTION` mints the control-intent family.
- Producer: `csharp:Rasm.AppUi/Shell/solver#TS_PROJECTION` mints the layout-constraint family.
- Producer: `csharp:Rasm.AppUi/Render/pipeline#TS_PROJECTION` mints the geometry-residency family.
- Producer: `csharp:Rasm.AppUi/Diagnostics/evidence#TS_PROJECTION` mints the evidence-timeline family.
- Consumers: TypeScript core decodes AppUi families into `ControlIntent`, `LayoutProgram`, `CommandGate`, and `EvidenceTimeline`.
- Consumers: TypeScript UI materializes shell families and reads timeline render evidence.
- Consumers: TypeScript runtime consumes the residency manifest on its frame home.
- Payload: `canonical-json` + `digest`
- Pin: DESIGN-PIN
- Blocker: the producer freezes no family vector — every family crosses as source-generated JSON with no pinned instance, so no family carries a byte-deriving input on either branch.
- Shape: `CommandPayloadWire`, `CommandGateWire`, `ControlIntentWire`, and `LayoutConstraintWire` are AppUi families.
- Shape: `GeometryResidencyWire` and `EvidenceTimelineWire` complete the AppUi family registration.
- Shape: Each AppUi family has one producer roster row and one consumer census row.
- Shape: `CommandPayloadWire` is `none | single(id) | many(ids) | text(value) | fields(values)`.
- Shape: `CommandInvocationWire` nests `{key,payload}` as a command-family member.
- Shape: Render evidence is the `EvidenceTimelineWire` render arm, never a standalone `RenderReceiptWire`.
- Shape: Render evidence retains `frameHash`, optional `drawHash`, and optional `pixels`.
- Shape: Pixel identity is `{version:"rgba8-srgb-straight-top-left-v2",width,height,hash}`.
- Shape: Pixel preimage is the kernel canonical framing — the length-framed version string, each extent as a little-endian ordinal, then the tight top-left canonical RGBA plane as the trailing raw leaf. The v1 literal named the same pixels under an unframed version prefix, so the two versions key one plane differently and a decoder pinning the older literal admits a digest from a preimage law no producer writes.
- Shape: `ControlIntentWire` closes THIRTY-ONE arms, the `banner` condition strip and the `overview` minimap strip included, and both ends now carry all thirty-one — the wire stays OPEN, so a peer decoding a subset remains lawful and a peer re-authoring the vocabulary does not.
- Shape: `banner` carries `{key,headlineKey,bodyKey,severity,placement,actions,evidence,binding}` with `severity` the `information|success|warning|error` roster and `placement` the `page|section` roster crossing as their own keys; severity dismissibility is a producer column that never crosses, so each head resolves it from the literal.
- Shape: `overview` carries `{key,axis,sourceKey,jumpCommand,binding}` with `axis` the `vertical|horizontal|plane` roster; the strip names its frame producer and its jump verb by key and mounts no child intent.
- Shape: Command intent, invocation, outcome, and receipt wires are nested family members.
- Shape: Intent binding and control receipt wires are nested family members.
- Shape: `ControlReceiptWire` is `{kind:"control",intentKey,controlType,command,emphasis,at}` — a cold materialization and a pool re-entry seal the same row, so a recycling regression reads as a receipt whose `controlType` disagrees with its `intentKey`.
- Shape: Layout variable, term, and program wires are nested family members.
- Shape: `GeometryResidencyWire` is `{version,viewpoint,tiles,vramBudget}` — a VIEWPORT MANIFEST, where `version` is the producer's pinned schema and pins the cluster roster as much as the envelope, so a decoder reading a column set one row short of the producer's stops at the wrong offset on every cluster past the first.
- Shape: A residency tile is `{kind,contentKey,blobKey,bytes,residentCount,harmonicDegree,bounds,streams,meshlets}`, `kind` closes `meshlet-cluster | quantized-vertex | point-splat | gaussian-splat`, and `bounds` packs `[x,y,z,radius]`.
- Shape: A meshlet row carries `parent` and `parentError` ABSENT at the LOD subtree root and terminus rather than sentinel-valued, and carries the producer's realized `cut` and measured `curvature` so no consumer re-derives either off decoded positions.
- Shape: The residency manifest REPLACES whole — the producer mints the entire resident tile set for one viewpoint per emission, so no scene key, generation column, row state, or delta arm crosses this seam and a consumer-side residency ledger keyed on those columns names no producer.
- Shape: Viewpoint, residency tile, Meshopt stream, and meshlet wires are nested family members.
- Shape: `RedlineDelta` is C#-and-Persistence-local and mints no TypeScript family — the markup payload crosses as the `EditIntent.Annotation` JSON blob onto the Persistence op-log projection, so it holds no consumer census row and a registration asserting a peer decoder no branch declares is the stranded state this manifest refuses.
- Shape: Nested AppUi family members never register as standalone entries.
- Shape: AppUi families use producer camelCase Strict JSON and the consumer `json` census arm.
- Shape: No AppUi descriptor source exists, so no AppUi family registers under `proto`.
- Shape: `LayoutConstraintWire` binds introduction order, edit variables, and authored and resolved measurement suggestions.
- Shape: Both tableaus re-solve the ordered program instead of trusting solved positions.
- Shape: AppUi product-shell vocabulary remains producer-owned, and peers only decode it.
- Shape: The drag/clipboard formats are C#-local registrations: `application/x-rasm-table-rows+json` carries `TableRowsWire {version:1, keys, tsv}` and refuses a non-current version by name, while `application/x-rasm-asset-key` and `application/x-rasm-host-objects` carry opaque single-value payloads — none mints a peer family, so a decoder claim for any of the three names a consumer no branch declares.
- Regenerate when: a family's field roster changes, the producer roster gains or retires a family, the residency schema pin or the pixel-identity version bumps, a `x-rasm-*` clipboard format changes its payload, or the camelCase Strict emission law moves.

### [02.23]-[BIM_WIRE]

- Seam: `bim-wire`
- Class: domain
- Producer: `csharp:Rasm.Bim/Review/issues#TS_PROJECTION` mints `BcfTopicWire` and its nested BCF family.
- Producer: `csharp:Rasm.Bim/Review/diff#MODEL_DIFF` mints the typed `ModelDiff` change set.
- Producer: `csharp:Rasm.Bim/Model/query#PREDICATE_WIRE` mints `PredicateWire` through `PredicateCodec.Seal` and `Admit`.
- Producer: `csharp:Rasm.Bim/Exchange/events#EVENT_PROJECTION` announces each `BimFact` as one message envelope through the kernel mint.
- Consumers: TypeScript core decodes the Bim coordination families.
- Consumers: TypeScript UI materializes the BCF board, viewpoint, and diff.
- Consumers: Python and TypeScript geospatial peers consume the separate raw GeoJSON text projection.
- Payload: `canonical-json` + `digest`
- Pin: DESIGN-PIN
- Blocker: the producer freezes no family vector — every family crosses as source-generated or codec-sealed JSON with no pinned instance, so no family carries a byte-deriving input on either branch; the announcement half carries none either, because `BimEventing.Observe` has no fire site until a source tree runs the hook rail.
- Shape: Bim typed families are `BcfTopicWire`, `BcfViewpointWire`, `ModelDiff`, and `PredicateWire`.
- Shape: `GeoFeatureWire` is absent because the producer emits raw GeoJSON text.
- Shape: `ModelDiff` carries baseline, revision, changes, and unchangedCount.
- Shape: `ElementChange` closes `added | removed | modified | moved | split | merged`.
- Shape: the announcement crosses as a CloudEvents message envelope under `[02.34]`'s payload-agnostic law, never a registry proto family.
- Shape: `BimAnnounce` closes the announced roster and each row carries the kernel `EventType` and the hook point it observes.
- Shape: `BimEventWire` is the flat camelCase payload half, one record per announced case under one source-generated `BimEventContext`.
- Shape: `BimEventing.Mint` is total over `BimFact` — an announced case projects one `EventMint` and every other answers `None`.
- Shape: `BimEventing.Admit` is the inverse a consuming ingress reaches after decode, re-proving `subject` against the admitted fact's own derivation.
- Shape: the message envelope ANNOUNCES the fact and gains no authority — the owning rail's receipt stays the evidence truth and no event ledger lands beside it.
- Shape: VerdictIssued carries specification, spec, model, tri-state outcome, severity, findings, and GlobalIds.
- Shape: BCF comment, camera, and coloring wires are nested family members.
- Shape: Value match, measure, bound, and node match wires are nested predicate members.
- Shape: `PredicateWire` is a CLOSED face over a wider producer algebra — `Seal` refuses a term the roster carries no discriminator for and mints no arm outside it, so the consumer union stays exhaustive.
- Shape: Per-arm diff records are nested `ModelDiff` family members.
- Shape: Nested Bim family members never register as standalone entries.
- Shape: Browsers compose the same closed predicate algebra the model owner evaluates.
- Shape: Unrostered predicate arms and matches refuse at both ends.
- Shape: Pattern compilability, vocabulary membership, and measure dimension remain producer gates.
- Shape: Raw IFC artifacts remain owned by `[02.8]`.
- Shape: `IdsAudit` is C# host-local and mints no TypeScript family.
- Shape: `IdsVerdict` is the Bim-owned companion-oracle row and mints no TypeScript family.
- Regenerate when: a family's field roster changes, the producer roster gains or retires a family, either dialect register's emission law moves, or the `BimAnnounce` roster gains or retires an announced fact.

### [02.24]-[ELEMENT_WIRE]

- Seam: `element-wire`
- Class: domain
- Producer: `csharp:Rasm.Element/Graph/wire#WIRE_CODEC` mints the append-only `rasm.element.v1` protobuf family.
- Producer: `csharp:Rasm.Element/Graph/wire#EVENT_ENVELOPE` announces each crossing as one Protobuf-framed CloudEvents message envelope over that family.
- Consumers: TypeScript core decodes the graph, delta, node, relationship, and nested payload messages.
- Consumers: Python runtime binds the `AppearanceWire` seam payload on its proto registry and leaves the graph message envelopes outside its vocabulary.
- Payload: `wire-bytes` + `digest`
- Pin: DESIGN-PIN
- Blocker: execution route absent, source landed — `rasm/element/v1/element.proto` and its `element.descriptor.binpb` seat the family under `[02.9]`, so every peer decode resolves in a real pool, while no source tree exists to run `ElementWire.Encode` over a corpus model and freeze a byte-deriving instance, nor `GraphCrossing.Mint` over that instance to freeze its message envelope; `[02.25]` `element-corpus` is that route and the DESIGN-PIN law forbids fabricated bytes.
- Shape: the proto SOURCE moved past the frozen `element.descriptor.binpb` (W2: `TimeSlice` retired to `CoverageWire` `reserved 9`; `SeriesStatisticsWire` moments `7..12`, `PropertyEvidenceWire` `grade`/`attested`/`run` `4..6` + `AttestationWire`, `HeaderWire` `axes`/`culture`/`format` `8..10` + `UnitAxisWire` appended) — the first sanctioned regeneration re-freezes the descriptor over these deltas.
- Shape: `NodeWire.content_address` is the 16-byte big-endian address minted from node with active header tolerance.
- Shape: `Encode(GraphDelta,basis)` uses basis tolerance when no reheader exists.
- Shape: Revisions take basis tolerance for their before address and revision tolerance for their after address.
- Shape: `redaction.unstable_node_ids` marks carried node addresses ineligible for edit OCC.
- Shape: Nested messages remain family members and never register as standalone entries.
- Shape: `GraphEventType` closes the crossing roster at `snapshot` and `delta`, each row carrying its own `EventType` and producing `EventSource`.
- Shape: the message envelope carries metadata alone under `[02.34]`'s payload-agnostic law and the protobuf message is its whole body.
- Shape: `subject` renders through the kernel `EventKey` lowercase spelling, never the seam's own uppercase `ContentAddress` rendering.
- Shape: `datacontenttype` derives from the encoded message's own descriptor, so a consumer selects its parser from the attribute rather than from the topic.
- Shape: `dataclassification` derives from the egress scope's cleared-column roster, so a crossing cannot announce a grade its redaction state contradicts.
- Shape: a streaming consumer folds length-prefixed bodies one frame per crossing and dedups on `(source, id)`.
- Regenerate when: the message roster, a field number, node-address minting, the descriptor source, or the `GraphEventType` crossing roster changes.

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
- Shape: one product declaration keyed to a material identity — issuer + registration the duplicate-check pair, declared unit admitted at its own functional unit, issue and expiry dates, and the per-indicator per-module impact map at declaration granularity (the frozen 13-indicator EN 15804+A2 roster over the 15-module EN 15978 roster) where KEY PRESENCE is the coverage census — an absent cell is undeclared absence, never a zero. Discriminating laws: a declared cell is a measured value with negative biogenic/avoided-burden carbon valid; the two resource fractions are optional scenario data, absent when undeclared; consumers band modules as their own seam requires (the C# leg sums declared cells onto its six-band `LifecycleStage` axis and constructs the full matrix only when every core indicator covers every band). Declaration semantics are the python data branch's host-free ingestion capability; peers decode and re-author none.
- Regenerate when: the indicator or module roster, the declared-unit vocabulary, or the canonical key order changes.

### [02.27]-[HDF5_FIELD_CONTAINER]

- Seam: `hdf5-field-container`
- Class: domain
- Producer: `csharp:Rasm.Compute/Runtime/field#FIELD_RESULT_CODEC` — `FieldCodec.Hdf5Encode` emitting the station×component chunk model as an HDF5 1.10 container over the `csharp:Rasm.Compute/Runtime/archive#CHUNK_CURSOR` cursor-guarded writer.
- Consumers: `python:data/gridded/field#CONTAINER` reads the container on the h5py rail (`FieldContainer`, with the `phony_dims` labelled lift) and `python:data/gridded/virtual#MANIFEST` virtualizes it through the `hdf` parser arm; `csharp:Rasm.Compute/Runtime/field#FIELD_RESULT_CODEC` `Hdf5Decode` is the round-trip leg.
- Payload: `wire-bytes` + `digest`
- Pin: DESIGN-PIN
- Blocker: the producer freezes no container vector — `Hdf5Encode` is a landed fence with no realized source, so no field corpus carries a byte-deriving instance on either branch.
- Shape: one field crosses as ONE dataset at the `/field` path — this entry MINTS that layout, because today's convention let the dataset path double as a station label and two producers spelled it two ways — with the field extent leading and the trailing axis the COMPONENT axis of that one dataset; one-dataset-per-component is the refuted sibling layout, forking the chunk address every consumer computes. Chunks are station-outermost through the one `ChunkGrid.Derive` derivation, so `Chunk(ordinal)` and a station-slab `HyperslabSelection` window resolve identically at both ends. Filters run Shuffle (id 2) then Deflate (id 1) — the h5py `compression='gzip', shuffle=True` pipeline — with C# WRITERS constrained to the `DeflateGrade` four-value set `{-1, 0, 1, 9}` while READERS accept any level a foreign producer wrote. Element bytes are little-endian ONLY: the managed rail refuses big-endian at open, so a big-endian corpus re-encodes upstream and never crosses. Writes are create-only and chunk-aligned in index order — no append, no in-place edit — so an accumulating series segments at its producer's cadence edge. Dimension-scale stance is PICKED here: the writer emits RAW HDF5 with NO dimension-scale datasets (netCDF semantics resolve above the rail on both branches), so python consumers read via h5py directly — or `phony_dims` under h5netcdf — and a writer-side coordinate-variable roster is growth this entry re-values, never a consumer workaround. VDS stance is PROVEN: an h5py-authored virtual container READS on the managed rail — `VirtualStorage` layout resolves whole and hyperslab reads across source boundaries, relative source paths resolve beside the containing file, and an unresolvable source region yields the declared fill value, never a fault — so a virtual container is a lawful read-side carrier of this entry, while C# writers never emit VDS because the write model carries no virtual layout, leaving virtual authorship on the h5py side. Field-container emission is the producer's domain capability, so peers decode the container and re-derive no layout.
- Regenerate when: the dataset path or component-axis law, the `ChunkGrid.Derive` derivation, the filter pipeline, or the create-only chunk law changes.

### [02.28]-[HDF5_GRADUATION_ENVELOPE]

- Seam: `hdf5-graduation-envelope`
- Class: domain
- Producer: `python:compute/experiments/model#ENVELOPE` — `GraduationEnvelope.fit`/`write` fitting reference bands from the training population and emitting the create-only h5py container.
- Consumers: `csharp:Rasm.Compute/Model/identity#MODEL_IDENTITY` — `GraduationEnvelope.Admit(HdfHandle)` re-running every Wellformed gate on the read roster.
- Payload: `wire-bytes` + `digest`
- Pin: DESIGN-PIN
- Blocker: no frozen graduation-container vector — the producer freezes one container when the fit lane executes; the DESIGN-PIN law forbids fabricated bytes.
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
- Shape: `schemaVersion` `"1"`; `owners[]` of `{name, fields[]}`; `bundleKey` the bare 32-hex content-key render parsed `NumberStyles.HexNumber`, never a raw integer; the `kind` discriminator literals `scalar|array|nested|mapping|optional|union` and the `FieldScalar` rows `i32|i64|f64|bool|string|key|bytes|decimal` are the decode contract at both ends; the scalar leaf's payload property spells `"scalar"` because CamelCase seats it on the `"kind"` discriminator STJ refuses to double-book.
- Regenerate when: the kind literals, the scalar rows, the bundle columns, or `SchemaVersion` change.

### [02.31]-[ENTITY_EDIT_WIRE]

- Seam: `entity-edit`
- Class: domain
- Producer: `csharp:Rasm.Persistence/Version/merge#STRUCTURAL_DIFF` mints the edit union whole — `StructuralMerge.Patch`, the `Members` diff, `EntityEditWire.Encode`, and the `PatchPolicy` ceiling.
- Consumers: TypeScript core decodes the edit union and applies its closed patch document to exact `NodeWire` ProtoJSON.
- Consumers: TypeScript state decodes the patched ProtoJSON through the existing node landing.
- Payload: `canonical-json` + `digest`
- Pin: DESIGN-PIN
- Blocker: no merge corpus freezes base/target graphs and their edit union, so no branch carries a byte-deriving instance.
- Shape: `StructuralMerge.Patch` mints each base from the held node under the active graph tolerance.
- Shape: `EntityEditWire.Members` deterministically diffs exact before/after `NodeWire` ProtoJSON.
- Shape: `EntityEditWire.Encode` emits the closed camelCase edit and RFC 6902 operation unions.
- Shape: `PatchPolicy.OperationCeiling` is positive, caller-supplied, and shared with the crossing ingress policy.
- Shape: Tombstone is `{kind:"tombstone",key,base}`.
- Shape: Members is `{kind:"members",key,base,patch}`.
- Shape: Patch closes `add | remove | replace | move | copy | test` with exact RFC 6902 fields.
- Shape: Key is `NodeId.Value`; base is the canonical `ContentAddress` string.
- Shape: Both arms require the producer-carried held-node address to equal base.
- Shape: Over-ceiling member diffs collapse to one root replacement of exact successor ProtoJSON.
- Shape: Any node listed in `redaction.unstable_node_ids` is ineligible for edit OCC.
- Shape: Inserts remain on `EditOp.Insert` and `GraphDelta`; they never fabricate a held-node edit base.
- Regenerate when: edit arms, patch operations, patch target, address minting, or canonical address spelling change.

### [02.32]-[ORGANIZATION_WIRE]

- Seam: `organization-wire`
- Class: domain
- Producer: `csharp:Rasm.Rhino/Document/layers#ORGANIZATION_PROJECTION` mints the append-only `rasm.organization.v1` family off one `Layers.Ask` read window.
- Consumers: `python:data/graph/graph#TOPOLOGY` folds it onto the rustworkx containment kernel through `organization_graph`; `typescript:core/interchange/codec#WIRE_CENSUS` carries the family and `#LANDING_WIRE` lands it as `Organization`, which `typescript:data/read/query#ORGANIZATION_ROWS` projects into read-side relations and `read/fold#LANE_SPEC` maintains; `python:runtime/transport/shapes#REGISTRY_AND_DRIFT` registers all four messages, decode-only.
- Payload: `wire-bytes` + `digest`
- Pin: DESIGN-PIN
- Blocker: no source tree runs `Layers.Ask`, so no host read has minted a byte-deriving instance and the DESIGN-PIN law forbids fabricated bytes; the descriptor half is closed, its source and `organization.descriptor.binpb` snapshot frozen at sibling parity under the `[02.9]` gate.
- Shape: model organization as a HOST-FREE document, since the producer is a host-boundary package and `libs/.planning/ARCHITECTURE.md` `[03]-[UNIVERSAL_VS_CAPTURE]` forecloses publishing a host's own vocabulary through a seam. `EntityWire.key` is the seed-zero `XxHash128` content key over the kernel `CanonicalWriter` frame — each ancestor label length-framed UTF-8, the chain count-framed, frame integers LITTLE-ENDIAN per the writer's own law — rendered on the wire as 16 BIG-ENDIAN bytes (`WriteUInt128BigEndian` at the one egress member), so one organizational address keys identically across source documents and a federated read unions them; the source key stays OUT of that preimage, since folding it in re-scopes a federation address to one file. `name` carries the leaf label alone and the ancestor chain is the containment walk, never a separator-joined path a peer re-splits. `ordinal` is the DENSE sibling rank the producer resolved, because a sparse host sort column pushes its own case-insensitive tie-break onto every peer and a codepoint-ordering peer inverts exactly the pairs UTF-16 ordering ranks the other way. `ContainmentWire` discriminates on the target's KEY SPACE through a oneof — `entity` in the record's own address space, `member` in the federation space `authority` names — so a decoder routes each target without a relation column that erases which space resolves it. `ViewOverrideWire` rows land only where the producer PROVED a view carries settings, so row presence is the evidence and an unprobed view reads unmeasured rather than defaulted. Host identity never crosses: no session `Guid`, no table index and its miss sentinel, no joined path, no persistent-visibility column whose host read collapses three write states onto two.
- Regenerate when: the message roster, a field number, the label-chain preimage, or the sibling-rank derivation changes.

### [02.33]-[SCENE_DESCRIPTOR]

- Seam: `scene-descriptor`
- Class: domain
- Producer: `csharp:Rasm.Rhino/Render/settings#SUN_ASTRONOMY` mints the sun band and `csharp:Rasm.Rhino/Objects/lights#ASK_AND_COMMIT` the photometric rows; the lights page's `[Mapper] SceneMap` emitter stacks both beside the shading reference — Objects (S2) composes Render (S1) downward, a lawful strata edge.
- Consumers: `python:geometry/energy/simulate#SIMULATE` decodes the whole descriptor into engine-ready context.
- Payload: `wire-bytes` + `digest`
- Pin: DESIGN-PIN
- Blocker: no capture has frozen a vector — `Objects/lights#ASK_AND_COMMIT` now carries the `[Mapper] SceneMap` emitter and all four producer-side repairs (linear `SceneSpectrum` conversion at the boundary, RFC-4122 byte order stated on the mapper, key-string columns on the closed vocabularies, `uint64` byte length), and no source tree runs `Lights.Capture`, so the DESIGN-PIN law still forbids fabricated bytes.
- Blocker: the consuming end is unlanded — `python:geometry/energy/simulate#SIMULATE` imports `SceneDescriptor`, `SceneSun`, `ScenePhotometry`, `SolarAngles`, and `TessellationFidelity` from `rasm.runtime.shapes`, and `python:runtime/transport/shapes` declares none of the five, so no decoder stands against the producer's field roster.
- Shape: identity crosses as RFC-4122 BIG-endian bytes and content keys as the estate's lowercase 32-hex spelling, because the consumer reads `row.id.hex()` against the canonical text form while the platform's default `Guid` layout writes the first three fields little-endian — the two agree only on the trailing eight bytes, so a byte-order slip renames every row without failing a decode.
- Shape: one `rasm.scene.v1` `SceneDescriptor` — `SceneSun` over the `SitedSun`/`AuthoredSun` discriminant, a `ScenePhotometry` roster, and a `ShadingArtifact` naming the GLB by the `GLB_BY_KEY` seed key with its declared `TessellationFidelity`. Every length is metres and `source_unit` carries the host unit as provenance, so no peer rescales. Solved angles cross as `[0,360)` azimuth east of North beside `[-90,90]` altitude, derived once on the kernel almanac; a consumer re-solving them substitutes a second almanac's answer for one instant. Closed vocabularies — light kind, cone state, attenuation, photometric authority, sky dialect — cross as their key strings, so an unknown key is a decode refusal rather than a schema arm. Irradiance never crosses: sky radiation belongs to the consuming weather owner, and `intensity_scale` is a dimensionless render multiplier. `PhotometricWebRef` crosses by content key alone, since no engine on the consuming rail reads one.
- Regenerate when: the `SceneDescriptor` message set, the solar-angle convention, or the fidelity column set changes.

### [02.34]-[OPLOG_ENTRY]

- Seam: `oplog-entry`
- Class: infrastructure
- Minters: `csharp:Rasm.Persistence/Version/ledger#CHANGEFEED`; `csharp:Rasm/Domain/event#ENVELOPE_MINT`; `python:runtime/transport/wire#CRDT_CODEC`; `typescript:data/journal/append#ATOMIC_PUBLISH`
- Consumers: `csharp:Rasm.Rhino/Document/events#STREAM_OWNER` taps sealed commits into host-vocabulary rows the store maps onto lanes; `typescript:core/state/causal#DELIVERY_BUFFER` orders and dedups the decoded entries.
- Payload: `wire-bytes` + `digest`
- Pin: DESIGN-PIN
- Blocker: the `OperationId` canonical preimage is pinned at three minters — framed lowercase-N origin text, little-endian counter, then the vector's own count-framed slots SORTED ascending by origin text — and no golden vector stands against it until the harness proof freezes concrete inputs.
- Blocker: the announcement half rests on the same gap — no minter has framed a message envelope over a concrete entry, so the attribute map carries no byte-deriving input at any branch.
- Shape: the entry record is THIRTEEN wire slots in the producer's own member declaration order — sequence, operation identity, model, entity key, lane, verb, payload, payload content key, trace slot, closure, actor, and the HLC physical and logical halves as two slots. Slot ORDER is the frozen contract every peer decodes by position, so a transcriber folding the two HLC halves into one column shifts every slot past it and strands the logical half in no slot at all; the count the slots prove is thirteen and a page spelling twelve has already dropped one.
- Shape: operation identity is the `(origin, counter)` dot beside its pre-mint frontier and never the payload digest, so equal payloads stay distinct operations; `sequence` resumes a drain and orders nothing across stores. Replay dedup and compaction admission both read the identity, so a decoder dropping it keeps neither guarantee.
- Shape: every entry crossing a transport ANNOUNCES as a CloudEvents message envelope under the nine-attribute grammar `libs/.planning/ARCHITECTURE.md` `[14]-[EVENT_FABRIC]` fixes — `type` reading `rasm.<domain>.<subject>.<fact>.v<N>`, `source` the producing capability's URI-reference, `subject` the payload content key, `id` the producer's operation identity, `time` the occurrence instant, `recordedtime` the receiver's ingest instant, `dataschema` the registry subject and version, `datacontenttype` the serdes arrow's own row data, and extension names lowercase `[a-z0-9]` within twenty characters. Its fourteen-row extension roster hands at construction and at every decode, since a decoder without it reads a declared extension as an unknown string, and its alphabetical declaration order IS the published DSSE digest order under `docs/laws/scars.md` `[DIGEST_OVER_UNORDERED_CONTAINER]`.
- Shape: the message envelope's `(source, id)` uniqueness composite IS the entry's `(origin, counter)` dot rendered — `id` carries the dot and `subject` the payload content key — so a dedup reading the announcement and a replay reading the feed reach ONE decision, and an announcement keyed on payload bytes discards the second edit of identical bytes the feed keeps.
- Shape: the message envelope ANNOUNCES and gains no authority — the entry and its codec-encoded payload stay the evidence truth, no message-envelope column enters this preimage, and a divergence between the two convicts the projection rather than the ledger.
- Shape: this entry freezes the payload-agnostic announcement law alone; each branch-owned payload instantiating it registers its own `domain` entry at its own seam, and a second message-envelope mint inside one branch is the class's drift defect.
- Regenerate when: the `OperationId` preimage layout, the entry slot set or its order, the `ColumnFamily` lane roster, the attribute grammar, or the extension roster changes.

### [02.35]-[TOLERANCE_WIRE]

- Seam: `tolerance-wire`
- Class: domain
- Producer: `csharp:Rasm.Fabrication/Spec/tolerance#OWNER_FOLD` — `GdtFrameWire`, the one landed `IToleranceEncoder`, over the `CorpusFrame` byte-deriving input the same cluster freezes.
- Consumers: `python:artifacts/drawing/dimension#DIMENSION` — `GdtFrame.decode` is the sole reader, seating the frame onto the `Fcf` case for both the `TOLERANCE`-entity render and the `LAYERED` compartment fold.
- Payload: `wire-bytes` + `digest`
- Pin: REAL
- Shape: one ISO 1101 feature-control frame as FRAMED BINARY, conforming to `tolerance-wire/contract.schema.json`. Every count, collection length, option arity, and token byte-length is a `u32` little-endian cell; every magnitude an IEEE-754 binary64 little-endian cell; every digest the `docs/laws/patterns.md` `[CONTENT_KEY]` law's 16 BIG-endian bytes at the lowercase `:x32` spelling; every token a `u32` byte-length then that many UTF-8 bytes. Field order IS `FeatureControl`'s member declaration order — magic, layout, id, source kind, source digest, characteristic, scope, zone kind, width, the kind-discriminated tail, modifiers, datums, material, composite — so a member the owner gains has one lawful seat and the layout cell bumps only when that order moves. Vocabulary KEYS cross where ordinals never do, so a roster reordered at either end re-maps nothing; `ContentKey` crosses WHOLE with egress kind ahead of digest, since two families over equal bytes mint equal digests and a digest-only join merges two specifications; a datum carries its OWN material condition and its precedence rides POSITION; the zone kind alone selects the second dimension, so no arm tag rides beside it; an `Option` frames as a count of 0 or 1, the same rule every collection follows; the composite lower segment crosses HERE as a second row of the same box, never as a second frame. Magnitudes cross EXACT and decimal presentation belongs to the consuming drawing standard — a producer-rounded string draws a sub-micron zone as an unachievable zero. Symbols never cross: each consumer resolves glyphs through its own standard and font. Geometric frames alone cross this encoder; `fit`, `texture`, `general`, and `chain` refuse by name. Model-space geometry stays OFF: datum targets and basic dimensions need a view transform this wire has no view to apply.
- Expectation: the frozen asset rides `tolerance-wire/feature_control_frame.bin` — 210 wire bytes, seed-zero digest `78030538177c18bdf51d0f317ce6ef88`. `CorpusFrame` fixes that input at the producer: `Id = ContentHash.Of("tolerance-wire:corpus-a:characteristic")` = `7b4eae0da233186203c7e8a2248c73de`, `Source = ContentKey.Of(EgressKind.QualityRecord, "tolerance-wire:corpus-a")` = kind `quality-record` beside digest `5f21b69b4380b5c3b461668ed90d0c3a`, over the `position` characteristic on the `axis` scope with a `diameter` zone of `0.25` mm, modifiers `common-zone` then `free-state` in ordinal-ascending key order, the precedence-ordered datum system `(A, rfs) (B, mmc) (C, rfs)`, frame material `mmc`, and one composite lower segment of `0.08` mm over datum `A`. Frozen values the producer must reproduce: `47 44 54 46` at offset 0, the layout cell `1`, the two identities at offsets 8 and 42, the ordinal modifier order, and the trailing composite block, with the decode consuming all 210 bytes and leaving none.
- Regenerate when: the `FeatureControl` member order, a crossing vocabulary roster, the `ContentKey` canonical form, the framing law, or the pinned `CorpusFrame` changes.

### [02.36]-[CHANGEFEED_ENVELOPE]

- Seam: `changefeed-envelope`
- Class: domain
- Producer: `csharp:Rasm.Persistence/Version/egress#EGRESS_SINK` — `Egress.Envelope` projects one durable `OpLogEntry` into the announcement each subscription's binding lowers.
- Consumers: `python:runtime/transport/binding#BINDING` raises a delivered message back to a `MessageEnvelope`; `typescript:core/interchange/carrier#EVENT_ENVELOPE` decodes the whole roster and reports every peer name it does not hold; `typescript:core/state/causal#DELIVERY_BUFFER` orders and dedups on the announced `(source, id)`.
- Payload: `wire-bytes` + `digest`
- Pin: DESIGN-PIN
- Blocker: no source tree runs the egress pump, so `Egress.Envelope` has projected no concrete entry and `EventEnvelope.Encode` has framed no body — the announcement carries no byte-deriving input and the DESIGN-PIN law forbids fabricated bytes.
- Shape: instantiates `[02.34]`'s payload-agnostic announcement law over the durable changefeed lanes, so the grammar, the extension roster, and the digest order arrive settled and this entry states the branch payload alone.
- Shape: `id` re-renders the entry's dot and `subject` its payload content key, so the drain's dedup and the feed's replay resolve one identity.
- Shape: `datacontenttype` and `dataschema` are row data off the serdes arrow the lane's own `SnapshotCodec` already chose, never a literal beside it.
- Shape: `dataclassification` carries the grade the egress redaction scope resolved, and a binding that grade forbids refuses before delivery rather than after it.
- Shape: a body past the row's `dataref` threshold externalizes to the content-keyed residence and the message envelope carries the reference alone, since every broker in the roster frames below an AEC payload.
- Shape: the announcement projects the entry and holds no authority over it — the entry and its encoded payload stay the evidence truth `[02.34]` freezes, and a divergence convicts the projection.
- Regenerate when: the entry slot set, the attribute grammar, the extension roster, or the per-binding `dataref` threshold derivation changes.

### [02.37]-[JOURNAL_RELAY]

- Seam: `journal-relay`
- Class: domain
- Producer: `typescript:data/journal/append#RELAY_ROWS` — `Journal.envelope` projects one claimed outbox deliverable through the branch's ONE mint entry.
- Consumers: `typescript:core/interchange/carrier#EVENT_ENVELOPE` is the authenticated inverse `Journal.carrier` reaches; `python:runtime/transport/binding#BINDING` raises the delivered message; `csharp:Rasm.Persistence/Version/egress#SUBSCRIPTION_FILTER` evaluates its filter AND-set over the decoded attributes.
- Payload: `canonical-json` + `digest`
- Pin: DESIGN-PIN
- Blocker: no source tree drains the relay, so `Journal.claimBatch` has claimed no deliverable and `Journal.envelope` has projected none — the announcement carries no byte-deriving input and the DESIGN-PIN law forbids fabricated bytes.
- Shape: instantiates `[02.34]`'s law over the outbox — `id` the landed global `sequence`, `source` the stream key spelled as one URI path, `type` the event tag verbatim, `time` the write instant, `subject` the stored content key, `dataschema` the `(tag, event_version)` registry coordinate.
- Shape: the app's event family spells its own tag as the estate grammar, since the tag IS the announced `type` and a tag outside it fails typed at the projection rather than at a subscription keying on it.
- Shape: the landed `sequence` serves both roles it inhabits — `id` as operation identity and the `sequence` extension as the per-source position under the integer sequence domain — and both cross as decimal text, so no consumer arms the package's global JSON swap to move a 64-bit identity.
- Shape: `partitionkey` is the stream triple, so a transport partitioning on it keeps one aggregate's announcements inside one ordering domain.
- Shape: the two version axes stay disjoint — the `type` major moves on a breaking change while `event_version` moves on every generation — so neither consumer re-derives one from the other.
- Shape: the announcement is a projection fold over the claimed deliverable and never a second record of truth; the outbox rows stay the evidence the relay drains.
- Regenerate when: the outbox column set, the attribute grammar, the extension roster, or the registry-coordinate derivation changes.

### [02.38]-[TRANSMITTAL_NOTICE]

- Seam: `transmittal-notice`
- Class: domain
- Producer: `python:artifacts/delivery/notice#NOTICE` — `TransmittalNotice` is the observe subscriber over the `TRANSMITTAL_ISSUED` hook fact that answers one message envelope.
- Consumers: `typescript:core/interchange/carrier#EVENT_ENVELOPE` decodes the roster and reports every dropped peer name; `csharp:Rasm.Persistence/Version/egress#SUBSCRIPTION_FILTER` evaluates its filter AND-set over the decoded attributes.
- Payload: `wire-bytes` + `digest`
- Pin: DESIGN-PIN
- Blocker: no source tree fires `TRANSMITTAL_ISSUED`, so the projection has answered no message envelope and the format owner has framed no body — the notice carries no byte-deriving input and the DESIGN-PIN law forbids fabricated bytes.
- Shape: instantiates `[02.34]`'s law over the settled ISO 19650 issue close — `type` under the `artifact` capability segment the receipt fold already records against, `source` that same producing capability, `id` the transmittal's pre-run aggregate key, `subject` the payload content key minted over the encoded fact bytes.
- Shape: the payload IS the fired fact under one encode, so the announcement cannot disagree with the receipt it projects and no second projection re-spells the evidence.
- Shape: `datacontenttype` stays absent because the format row that encodes names it, and a literal beside that row states a payload encoding the arrow already decided.
- Shape: `partitionkey` is the transmittal id, `sequence` the revision ordinal under the integer domain so a consumer reads a gap as a missed revision, `authcontext` the issuing party, and `correlation` the issue-scope baggage id.
- Shape: `dataclassification` resolves AT this boundary off the folded ISO 19650 confidentiality text, an unspelled or absent one reading the internal grade, since that resolved grade is what makes a binding refuse a broker the issue may not cross.
- Shape: the issued register rides as its own content key rather than a re-spelled copy of its rows, since an unbounded row set defeats every frame budget the binding rows declare.
- Regenerate when: the fired fact's field set, the attribute grammar, the extension roster, or the confidentiality keying changes.

### [02.39]-[CESQL_CONFORMANCE]

- Seam: `cesql-conformance`
- Class: infrastructure
- Minters: `csharp:Rasm.Persistence/Version/egress#SUBSCRIPTION_FILTER`; `python:runtime/transport/filter#CESQL`; `typescript:runtime/work/filter#GRAMMAR_OWNER` — each compiles a vector once at admission and evaluates it per event through its own expression owner, and the 32-bit `Integer` width is branch-owned on every one: the C# guard reads a checked-arithmetic throw and the Python guard an explicit wrap where that runtime silently widens instead.
- Consumers: each minter evaluates the vendored vectors through its own compiled expression owner and no branch reads a peer's verdict.
- Payload: `wire-bytes` + `digest`
- Pin: REAL
- Shape: the CloudEvents CESQL conformance corpus as FROZEN PUBLISHER BYTES — the upstream TCK's own YAML vectors, each carrying an expression, its event context, and the value beside the error list a conforming evaluator must answer. Those bytes carve out of every estate lane that respells them: no formatter, no breaking gate, and no schema of this estate's authorship touches them, because a reformat proves this estate's spelling rather than the publisher's, and the seam therefore carries no `contract.schema.json` at all. Conformance is per-minter and total — every operator, function, and cast answers a value beside an accumulated error list, so a vector expecting a `MathError` proves the evaluator returned a defined value with the fault recorded rather than escaping. `Integer` is 32-bit and `ABS(-2147483648)` is the discriminating vector: the negation of the minimum has no representation, so an unchecked implementation throws where the corpus demands a fault row. Seven specification error types — parse, math, cast, missing attribute, missing function, function evaluation, and generic — close the answer vocabulary every vector grades against, and an evaluator collapsing two of them onto one passes the value half while failing the corpus. Each branch that mints an evaluator registers as a minter row here and parity across those verdict sets IS the conformance; a branch reaching CESQL only as a filter string it forwards mints nothing.
- Expectation: the frozen assets ride `cesql-conformance/` — 18 vendored files totalling 30456 bytes, each digested seed-zero `XxHash128` as 16 big-endian bytes at the `:x32` lowercase spelling: `binary_comparison_operators.yaml` 2990 B `aeb978a214344236a0172f547219dc1d`; `binary_logical_operators.yaml` 1468 B `7b120b7c587f589bf0904c94660f62f8`; `binary_math_operators.yaml` 1705 B `bc67603c9f3e9591c85ea8e81874da4c`; `case_sensitivity.yaml` 444 B `034915a8184d0257c4e30d95ffa3eaed`; `casting_functions.yaml` 1649 B `a6c08f0ba457b55ddf736d7c8d741cd6`; `context_attributes_access.yaml` 1341 B `6728abf39e058f0f825c0332124875f7`; `exists_expression.yaml` 1421 B `fe51329c9c127d3cad40f7f196205314`; `in_expression.yaml` 2139 B `c5dad145320cd53669003c739061cd9d`; `integer_builtin_functions.yaml` 305 B `57fcdf509b14dff7ec14a69ae413a094`; `like_expression.yaml` 3764 B `91154fc6e00178def3f66038e9a9b677`; `literals.yaml` 765 B `68e74a588b4d0788450f11dbae1a7dc1`; `negate_operator.yaml` 471 B `21e6fa0a951c95184f0b33df20e667a8`; `not_operator.yaml` 501 B `f3974d7bdde9d1beb21d761324b10a68`; `parse_errors.yaml` 98 B `8533571af7e8f0feef72bbdf08455291`; `spec_examples.yaml` 2037 B `1d5ad501cce01bb1d9d51b9d03f6e6e7`; `string_builtin_functions.yaml` 3421 B `ebd3b971a64f4da6481effa878b07dc2`; `sub_expression.yaml` 238 B `e27b28752d21c9949d18674d2519597e`; `subscriptions_api_recreations.yaml` 5699 B `aae71619fa690c73fef808680f9ef11a`. Digests move only when the vendored bytes move, so a moved digest is a re-vendoring and never an estate edit.
- Regenerate when: the vendored TCK release moves, or a minter joins the roster.

### [02.40]-[ARCHIVE_3DM]

- Seam: `archive-3dm`
- Class: domain
- Producer: `csharp:Rasm.Rhino/Exchange/archive#TRANSACTION_RAIL` writes the standalone `.3dm` archive over one detached `File3dm` lease with exact-byte identity evidence.
- Consumers: `python:data/spatial/mesh#MESH_PAYLOAD` reads the archive offline through the `rhino3dm` backend row (`File3dm.Read` meshes only, units off `File3dm.Settings.ModelUnitSystem.name`), per the geometry-flow law that no geometry kernel crosses.
- Payload: `wire-bytes` + `digest`
- Pin: DESIGN-PIN
- Blocker: no capture has frozen an archive — the producer rail and the consumer row both stand as design; the first frozen fixture is a minimal mesh-bearing `.3dm` beside its seed-zero `XxHash128` digest.
- Shape: the OpenNURBS `.3dm` container as the host publishes it — the producer never re-frames it and the consumer reads meshes alone, so the archive format's authority is McNeel's and this entry pins the crossing, not the grammar. Units cross as the archive's own `ModelUnitSystem` name, never a peer-side default; a mesh-less archive reads as an empty roster, not a refusal.
- Regenerate when: the produced archive version ceiling moves (`Exchange/archive` `FormatVersion.Of`), or the consumer backend row changes its read scope.

## [03]-[DEBT]

Cross-branch flows the corpus observed crossing WITHOUT a binding entry; each row names the divergence and its resolution owner, and a row retires only by entry mint or recorded negative at both ends.

- `DoeDataset` — declared at BOTH ends (`csharp:Rasm.Compute/Solver/sweep` emitting, `python:data` ingesting) with the branch maps carrying the seam edge, yet no entry binds the bytes; the flow meets at Arrow IPC, which both ends already speak, so the resolution is an Arrow-payload entry minted by the C# producer or a recorded negative declaring the crossing content-key-only — convention-aligned columns until then fork on first edit.
