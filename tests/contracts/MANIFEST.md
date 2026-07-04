# [CONTRACTS_MANIFEST]

The corpus registry: one entry per committed cross-language fixture, instantiating the schema in [README.md](README.md) `[03]-[MANIFEST]`. Every fixture keys on the one C#-minted `XxHash128` identity (seed zero, two-64-bit-half order); a per-fixture digest function or a per-runtime fixture mint is the named drift defect. Pin states are honest: a `REAL` entry records the frozen expectation the producer emit must reproduce, and a `DESIGN-PIN` entry records the producer gap that blocks byte derivation — never a fabricated stand-in.

## [01]-[LEDGER]

| [INDEX] | [FIXTURE]               | [SEAM]              | [PRODUCER]                                              | [PAYLOAD]                    | [PIN]      |
| :-----: | :---------------------- | :------------------ | :------------------------------------------------------ | :--------------------------- | :--------- |
|  [01]   | CANONICAL_BYTE_IDENTITY | `content-identity`  | `csharp:Rasm/Geometry/Spatial/reconciliation`           | `wire-bytes` + `digest`      | REAL       |
|  [02]   | MATERIAL_LAYER_GOLDEN   | `content-identity`  | `csharp:Rasm.Element/Projection/address`                | `wire-bytes` + `digest`      | DESIGN-PIN |
|  [03]   | CLASH_GOLDEN            | `clash-golden`      | `csharp:Rasm/Geometry/Spatial/index`                    | `wire-bytes`                 | REAL       |
|  [04]   | FAULT_TRIPLES           | `fault-triples`     | `csharp:Rasm.Compute/Runtime/channels`                  | `wire-bytes` + `canonical-json` | DESIGN-PIN |
|  [05]   | CRDT_OP_SET             | `crdt-op-set`       | `csharp:Rasm.Persistence/Version/commits`               | `wire-bytes`                 | DESIGN-PIN |
|  [06]   | GLB_BY_KEY              | `glb-by-key`        | `csharp:Rasm.Compute/Runtime/codecs`                    | `wire-bytes` + `digest`      | DESIGN-PIN |
|  [07]   | HLC_TWO_HALF            | `hlc-two-half`      | `csharp:Rasm.AppHost/Runtime/ports`                     | `wire-bytes`                 | DESIGN-PIN |
|  [08]   | IFC_WIRE                | `ifc-wire`          | `csharp:Rasm.Bim/Exchange/wire`                         | `wire-bytes` + `digest`      | DESIGN-PIN |
|  [09]   | DESCRIPTOR_DRIFT        | `descriptor-drift`  | two descriptor sources, named in the entry              | `descriptor-set`             | DESIGN-PIN |

## [02]-[ENTRIES]

### [02.1]-[CANONICAL_BYTE_IDENTITY]

- Seam: `content-identity`
- Producer: `csharp:Rasm/Geometry/Spatial/reconciliation#CANONICAL_BYTE_IDENTITY`
- Consumers: `python:runtime/evidence/identity#SEED_REPRODUCTION`; `typescript:core/value/contentKey` (delegating sites `core/interchange/frame`, `runtime/browser/fetch`, `data/object/store`; readers in `tests/typescript/_testkit`); the C# shared-corpus harness under `tests/csharp`.
- Payload: `wire-bytes` + `digest`
- Pin: REAL
- Shape: the canonical-adjacency byte stream — `int32`-LE `VertexCount`, `int32`-LE `EdgeCount`, `(int32-LE Min, int32-LE Max)` per sorted edge pair, `int32`-LE `FaceCount`, per lowest-vertex-rotated face cycle `(int32-LE CycleLength, int32-LE Vertex…)` — contiguous, no padding, hashed by `XxHash128.HashToUInt128` at seed zero. Discriminating laws: a morph (moved control points, same adjacency) re-hashes identically; a topology break re-hashes distinctly.
- Expectation: the single-triangle topology (`VertexCount=3`; edges `(0,1),(0,2),(1,2)`; face cycle `[0,1,2]`) whose 52-byte stream is `03 00 00 00 03 00 00 00 00 00 00 00 01 00 00 00 00 00 00 00 02 00 00 00 01 00 00 00 02 00 00 00 01 00 00 00 03 00 00 00 00 00 00 00 01 00 00 00 02 00 00 00`, digest `0x9462A71A5DD13DCFA3B1D6D225FCBE70`, 16-byte LE memory `70 be fc 25 d2 d6 b1 a3 cf 3d d1 5d 1a a7 62 94` — host-validated against the native `Mesh` topology surface and frozen on the producer page.
- Regenerate when: the frozen canonical-adjacency field order or the kernel `Domain/ContentHash` seed law changes.

### [02.2]-[MATERIAL_LAYER_GOLDEN]

- Seam: `content-identity`
- Producer: `csharp:Rasm.Element/Projection/address#CONTENT_ADDRESS`
- Consumers: `python:runtime/evidence/identity#SEED_REPRODUCTION` (`_CORPUS` row, `planned`-phase obligation until pinned); `typescript:core/value/contentKey` (the `hash-wasm` bit-parity gate); `csharp:Rasm.Element/Graph/wire#WIRE_CODEC` (the `MaterialLayerWire` three-runtime round-trip).
- Payload: `wire-bytes` + `digest`
- Pin: DESIGN-PIN
- Blocker: the producer has not frozen the concrete `MaterialComposition.LayerSet` node and its digest — the `CanonicalWriter` counted-bag pin on `csharp:Rasm.Element/Projection/address`.
- Shape: the float-bearing `IfcMaterialLayer`-shaped node's `CanonicalWriter` bytes — case ordinal `1`, layer count, then per layer the length-prefixed material-id UTF-8, the `ThicknessMm` `Measure` (length-prefixed `QuantityType` token first, IEEE-754-LE `Si` magnitude with `-0.0` collapsed to `0.0` and every NaN mapped to one quiet pattern quantized to `Header.Tolerance`, then the 7 SI `Dimension` exponent ordinals), and the layer-name UTF-8 — hashed seed-zero. The only corpus fixture exercising the float canon the integer-topology fixture cannot reach.
- Regenerate when: the `CanonicalWriter` layout or the pinned layer node changes.

### [02.3]-[CLASH_GOLDEN]

- Seam: `clash-golden`
- Producer: `csharp:Rasm/Geometry/Spatial/index#CLASH_GOLDEN`
- Consumers: `csharp:Rasm.Compute/Solver/clash#CLASH_GOLDEN` (the `ClashScale.NodeLinkPairs` descent decoding the pinned pair set); the two-sided byte-identity harness both pages assert.
- Payload: `wire-bytes`
- Pin: REAL
- Shape: the frozen node-link wire `NodeLinkProjection` emits — `Bounds` as `6·NodeCount` `float32`-LE per-node `[minX,minY,minZ,maxX,maxY,maxZ]`, then `Nodes` as `NodeCount + primitiveCount` `int64`-LE descriptors packing `(FirstChild << 21) | ChildCount` for internal nodes and the negated leaf form with a primitive-id tail.
- Expectation: the 8-primitive `BoundingBox(min,max)` set — two X-separated clusters of four unit cubes at `(0,0,0)→(1,1,1)`, `(0.5,0,0)→(1.5,1,1)`, `(2,0,0)→(3,1,1)`, `(2.5,0,0)→(3.5,1,1)`, `(10,0,0)→(11,1,1)`, `(10.5,0,0)→(11.5,1,1)`, `(12,0,0)→(13,1,1)`, `(12.5,0,0)→(13.5,1,1)` — built with `BuildPolicy.Canonical` (`LeafSize: 4`) through `SpatialIndex.Build(SpatialKind.Bvh, …)` yields `NodeCount == 3`, the identity `Order` permutation, descriptors `Nodes[0] = 2097154`, `Nodes[1] = -5`, `Nodes[2] = -8388613`, tail `[0,1,2,3,4,5,6,7]`, and the 160-byte `Bounds`-then-`Nodes` stream frozen hex-complete on the producer page; the decode side reproduces the clash-pair set `{(0,1),(2,3),(4,5),(6,7)}`.
- Regenerate when: the node-link layout, `BuildPolicy.Canonical`, or the pinned 8-box input changes.

### [02.4]-[FAULT_TRIPLES]

- Seam: `fault-triples`
- Producer: `csharp:Rasm.Compute/Runtime/channels#FAULT_PROJECTION`
- Consumers: `typescript:core/interchange/codec` (`faultTagOf`/`FAULT_CTOR` reconstruction; unmapped packages fold to `Quarantine`). Python mints `FaultDetail` outbound but is not a package-keyed decoder, so the round-trip scope is C#-to-TypeScript.
- Payload: `wire-bytes` + `canonical-json`
- Pin: DESIGN-PIN
- Blocker: the producer has not pinned the concrete `(package, code, case)` triple set spanning the disjoint bands.
- Shape: `FaultDetail` triples spanning ComputeFault band 2200, HopFault band 4500, the `WireFault` sub-band 4520-4532, and store/config bands at their app roots; the round-trip law reconstructs the identical literal-discriminated union from pack to decode, and neither `package` nor `code` alone is a total key.
- Regenerate when: the band allocation or the `WireFault` case roster changes.

### [02.5]-[CRDT_OP_SET]

- Seam: `crdt-op-set`
- Producer: `csharp:Rasm.Persistence/Version/commits#CRDT_ALGEBRA`
- Consumers: `typescript:core/interchange/format` feeding `typescript:core/state/merge`; `python:runtime/transport/serve#CRDT_DECODE`.
- Payload: `wire-bytes`
- Pin: DESIGN-PIN
- Blocker: the op-set input is unpinned, and the producer must settle the MessagePack envelope — `MessagePackCompression.None` for the companion lane or one published `Lz4BlockArray` framing spec — plus the `Beat` `state` byte-encoding.
- Shape: a `CrdtOpWire` MessagePack op multiset over the `Set | Write | Add | Remove | Increment | InsertAfter | Delete | Maintain | Beat | Leave` union with the `Hlc` 16-byte cell; the convergence law folds divergent-delivery permutations of the same op multiset to byte-identical state under the join-semilattice `Merge`.
- Regenerate when: the `CrdtOp` union, the `Merge` algebra, or the envelope framing changes.

### [02.6]-[GLB_BY_KEY]

- Seam: `glb-by-key`
- Producer: `csharp:Rasm.Compute/Runtime/codecs#TILE_PARTITION` over the GLB tessellation result, content-keyed through `csharp:Rasm.Compute/Runtime/codecs#CONTENT_ADDRESSING`.
- Consumers: `typescript:ui/viewer/scene` fetching by `ContentKey` through `typescript:core/interchange/frame`; `python:geometry/mesh/daemon#TESSELLATE` (the tessellation companion whose output the key addresses).
- Payload: `wire-bytes` + `digest`
- Pin: DESIGN-PIN
- Blocker: the producer has not pinned the byte-deriving input — the sample source geometry plus `TessellationPolicy` — and the leaf-tile content emit is gated on the Bim tile-emit codec admission.
- Shape: one content-keyed GLB sample keyed by the `ContentIdentity` seed; the GLB geometry-content identity is the kernel seed-zero `XxHash128`, never the policy-seeded interchange cache key.
- Regenerate when: the tessellation policy, the content-key derivation, or the pinned source geometry changes.

### [02.7]-[HLC_TWO_HALF]

- Seam: `hlc-two-half`
- Producer: `csharp:Rasm.AppHost/Runtime/ports#PORT_RECORDS`
- Consumers: `typescript:core/value/clock` (readers in `tests/typescript/_testkit`; `core/state/causal` orders by the decoded band); `python:runtime/clock` and `python:runtime/transport/serve#SERVE` (decoding `hlc_physical`/`hlc_logical` plus `tenant` from the receipt slot).
- Payload: `wire-bytes`
- Pin: DESIGN-PIN
- Blocker: the producer page carries no frozen two-half stamp vectors — the corpus index names an `HLC_FANIN` fixture cluster the ports page has not authored; the fixture landing is the upstream blocker and is not resolvable consumer-side.
- Shape: two-64-bit-half stamps in the `ReceiptEnvelope` order — physical half first as the `Instant` Unix-tick `int64`-LE, logical half second as the monotone `ulong`-LE, the exact order `InterchangeIdentity.Compose` seals — with vectors chosen so a logical-half-first composition corrupts by folding a fresh op as stale.
- Regenerate when: the two-half compose order or the `ReceiptEnvelope` stamp layout changes.

### [02.8]-[IFC_WIRE]

- Seam: `ifc-wire`
- Producer: `csharp:Rasm.Bim/Exchange/wire#WIRE_PROJECTION`
- Consumers: `typescript:core/interchange/codec`; the `python:geometry` ifcopenshell companion — each decodes the same bytes, projects its own graph, and reproduces the seam `ContentAddress` GraphKey (`WireParity.Agrees`).
- Payload: `wire-bytes` + `digest`
- Pin: DESIGN-PIN
- Blocker: the producer has not pinned the canonical corpus IFC payload the `WireParity` row freezes.
- Shape: the `IfcWire` serialization bytes plus the `ContentAddress.OfGraph` GraphKey. The byte golden proves host-local re-seal determinism only (`WireParity.Reproduces`, the GeometryGym regression catch); cross-runtime parity is GraphKey equality alone, because peer serializers emit divergent byte layouts for one graph — a cross-runtime byte compare is the deleted form.
- Regenerate when: the canonical authoring order, the `ContentAddress.OfGraph` law, or the pinned corpus payload changes.

### [02.9]-[DESCRIPTOR_DRIFT]

- Seam: `descriptor-drift`
- Producer: `csharp:Rasm.Element/Graph/wire#WIRE_CODEC` (`Graph/element.proto`, the `rasm.element.v1` descriptor source) and `csharp:Rasm.Compute/Runtime/channels#CONTRACT_EVOLUTION` (the suite proto vocabulary) — one snapshot per descriptor source.
- Consumers: `typescript:core/interchange/contract` (the `Identical`/`Additive`/`Breaking` verdict at the dial).
- Payload: `descriptor-set`
- Pin: DESIGN-PIN
- Blocker: no `.proto` exists on disk; the snapshot lands the day the first `.proto` lands, when `buf breaking` (FILE category, against `main`) becomes the required gate.
- Shape: the emitted `FileDescriptorSet` snapshot per proto source; the gate classifies every contract change `Identical`, `Additive`, or `Breaking` — field numbers are append-only, and a removal reserves its number and name.
- Regenerate when: any owning `.proto` contract changes.
