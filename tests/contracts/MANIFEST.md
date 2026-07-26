# [CONTRACTS_MANIFEST]

Corpus entries bind each contract to its class: an `infrastructure` entry names every branch anchor that mints the shape, and a `domain` entry names the one producer that emits it. Content-addressed fixtures key on the `docs/laws/patterns.md` `[CONTENT_KEY]` law; pin state records whether the byte-deriving input is frozen.

## [01]-[LEDGER]

| [INDEX] | [FIXTURE]               | [SEAM]                  | [CLASS]        | [PAYLOAD]                       | [PIN]      |
| :-----: | :---------------------- | :---------------------- | :------------- | :------------------------------ | :--------- |
|  [01]   | CANONICAL_BYTE_IDENTITY | `content-identity`      | infrastructure | `wire-bytes` + `digest`         | DESIGN-PIN |
|  [02]   | MESH_ADJACENCY_GOLDEN   | `mesh-adjacency`        | domain         | `wire-bytes` + `digest`         | REAL       |
|  [03]   | MATERIAL_LAYER_GOLDEN   | `material-layer`        | domain         | `wire-bytes` + `digest`         | DESIGN-PIN |
|  [04]   | FAULT_TRIPLES           | `fault-triples`         | infrastructure | `wire-bytes` + `canonical-json` | DESIGN-PIN |
|  [05]   | CRDT_OP_SET             | `crdt-op-set`           | infrastructure | `wire-bytes`                    | DESIGN-PIN |
|  [06]   | GLB_BY_KEY              | `glb-by-key`            | domain         | `wire-bytes` + `digest`         | DESIGN-PIN |
|  [07]   | HLC_TWO_HALF            | `hlc-two-half`          | infrastructure | `wire-bytes`                    | DESIGN-PIN |
|  [08]   | IFC_WIRE                | `ifc-wire`              | domain         | `wire-bytes` + `digest`         | DESIGN-PIN |
|  [09]   | DESCRIPTOR_DRIFT        | `descriptor-drift`      | infrastructure | `descriptor-set`                | DESIGN-PIN |
|  [10]   | CONSUMPTION_PROFILE     | `consumption-profile`   | infrastructure | `canonical-json`                | DESIGN-PIN |
|  [11]   | BACKEND_CONTRACT        | `backend-contract`      | infrastructure | `canonical-json` + `digest`     | DESIGN-PIN |
|  [12]   | CAPABILITY_DESCRIPTOR   | `capability-descriptor` | domain         | `canonical-json` + `digest`     | DESIGN-PIN |

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
- Minters: `csharp:Rasm.AppHost/Runtime/ports#PORT_RECORDS`; `python:runtime/clock/clock#CLOCK`; `typescript:core/value/clock#TWO_HALF_LAYOUT`
- Consumers: `python:runtime/transport/serve#SERVE` decodes the halves with `tenant`; `python:runtime/evidence/reproduction#SEED_REPRODUCTION` reproduces every mint in the parity suite.
- Payload: `wire-bytes`
- Pin: DESIGN-PIN
- Blocker: the C# minter's ports page lacks the indexed `HLC_FANIN` two-half vectors the fixture freezes.
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
- Minters: `csharp:Rasm.Element/Graph/wire#WIRE_CODEC` (`Graph/element.proto`, the `rasm.element.v1` descriptor source); `csharp:Rasm.Compute/Runtime/wire#CONTRACT_EVOLUTION` (the suite proto vocabulary); `python:runtime/transport/shapes#REGISTRY_AND_DRIFT`; `typescript:core/interchange/contract#DRIFT_VERDICT`
- Consumers: each minter's gate reads the snapshot pair it owns; no branch reads a peer's verdict.
- Payload: `descriptor-set`
- Pin: DESIGN-PIN
- Blocker: repository lacks a `.proto`; its first source lands with a `FileDescriptorSet` snapshot, and `buf breaking` gates FILE against `main`.
- Shape: each proto source emits a `FileDescriptorSet`; the gate gives `Identical`/`Additive`/`Breaking`; numbers only append, and removals reserve name and number. Each descriptor source is one mint unit, so a branch owning two sources carries two snapshot rows under one gate law and the forked-parity defect is two snapshots of one source.
- Regenerate when: any owning `.proto` contract changes.

### [02.10]-[CONSUMPTION_PROFILE]

- Seam: `consumption-profile`
- Class: infrastructure
- Minters: `csharp:Rasm.AppHost/Runtime/profiles#PROFILE_AXIS`; `python:runtime/execution/admission#CONTEXT`; `typescript:runtime/proc/config#ADMISSION_ROWS`
- Consumers: every package reading a supplied profile row instead of assuming a deployment shape; each branch resolves the row at its own admission owner.
- Payload: `canonical-json`
- Pin: DESIGN-PIN
- Blocker: no minter has frozen a canonical-json vector, so three landed rosters carry no proved parity.
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
- Blocker: the producer has pinned no descriptor set, so the schema digest carries no frozen preimage.
- Shape: one descriptor per brokered op — name, argument and result schema references, and the effect, idempotency, and cost-unit keys as the broker's own vocabulary — with cross-language shape identity the JSON Schema the producer exports and every SDK target binds unchanged. Brokered capability is the producer's domain capability, so peers decode the descriptor and re-author no capability shape; schema evolution is additive, and a generated SDK is a build artifact rather than a second mint.
- Regenerate when: the descriptor vocabulary, the exported schema layout, or the effect and cost key sets change.
