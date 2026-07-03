# [WIRE_GRAPH]

`codec/graph.ts` decodes the shared `rasm.element.v1` contract — `ElementGraphWire`, `NodeWire`, `RelationshipWire`, C#-owned by `Rasm.Element/Graph` — under the descriptor drift gate: content keys cross verbatim as the node identity, decode admission requires the gate's verdict in the requirement channel, and the decoded `ElementGraph` is a wire-owned keyed value (`ui/viewer` and `state` folds consume it through `#vocab`) because TS owns no element semantics and no geometry. The golden-byte parity fixtures for this family ride the `tests/contracts` corpus; the content-key cell projection that feeds them is a reflection read over the same descriptors the gate diffed.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]      | [OWNS]                                                                        |
| :-----: | :------------- | :------------------------------------------------------------------------------ |
|   [1]   | `GRAPH_DECODE` | the gated `ElementGraph` owner: node/relation shapes, keyed projection, decode   |
|   [2]   | `PARITY_WALK`  | the `Parity` reflect/path projection over key fields — parity evidence, no re-mint |

## [2]-[GRAPH_DECODE]

- Owner: `ElementGraph` — one `Schema.Class`: wire-true node and relation rows as embedded case classes at full depth, the keyed lookup as a derived getter, and every decode surface as a static; `Node` and `Relation` ride the owner's name.
- Entry: `ElementGraph.admitted(octets)` — the one decode rail: yields `DescriptorGate.admitted("ElementGraphWire")` first, so a `breaking` live contract refuses with `WireFault` reason `drift` before any byte decodes; the gate requirement rides `R`, satisfied at the app root, and an ungated decode spelling does not exist on this surface.
- Receipt: the decoded graph is keyed evidence — every node addressable by its verbatim content key through `byKey`, every relation a typed edge between keys; `ui/viewer` scene lookups and `state` folds read it structurally.
- Growth: a new node payload axis or relation kind is a C# contract change — the gate reports it (`additive` decodes on, `breaking` refuses), and the field lands here mirroring the emit in the same wave as the regenerated suite; consumers of `byKey` are untouched.
- Law: keys cross verbatim — node and relation keys decode through the kernel `ContentKey` cell admission with zero re-mint, zero re-hash; the graph's own `key` is the C#-minted identity of its canonical bytes.
- Law: the field record is wire-true — `nodes` decodes as the repeated row list the proto emits; the keyed view is `byKey`, a derived projection consumers hold, because a `HashMap` field would demand an entries wire shape the C# mint never writes.
- Law: payload fields are semantics-free carriage — `payload` is a string-keyed record the viewer projects; element MEANING is C#'s and Python's, and a TS fold that interprets payload semantics has crossed the geometry fence (invariant 7).
- Boundary: the drift verdict vocabulary is `contract/drift.ts`; the gate service is `contract/descriptor.ts`; mesh payloads ride `frame/geometry.ts` — the graph carries keys and carriage, never geometry.

```typescript
import { ContentKey } from "@rasm/ts/kernel"
import { Array, Effect, HashMap, type ParseResult, Schema } from "effect"
import { DescriptorGate } from "../contract/descriptor.ts"
import type { WireFault } from "../fault/quarantine.ts"
import { ProtoCodec } from "./proto.ts"

class Node extends Schema.Class<Node>("Node")({
  key: ContentKey.FromCell,
  kind: Schema.NonEmptyString,
  payload: Schema.Record({ key: Schema.String, value: Schema.Unknown }),
}) {}

class Relation extends Schema.Class<Relation>("Relation")({
  key: ContentKey.FromCell,
  kind: Schema.NonEmptyString,
  source: ContentKey.FromCell,
  target: ContentKey.FromCell,
}) {}

class ElementGraph extends Schema.Class<ElementGraph>("ElementGraph")({
  key: ContentKey.FromCell,
  nodes: Schema.Array(Node),
  relations: Schema.Array(Relation),
}) {
  static readonly Node: typeof Node = Node
  static readonly Relation: typeof Relation = Relation
  static readonly FromBytes: Schema.Schema<ElementGraph, Uint8Array> = ProtoCodec.family(ProtoCodec.suite.ElementGraphWire, ElementGraph)
  static readonly admitted = (octets: Uint8Array): Effect.Effect<ElementGraph, ParseResult.ParseError | WireFault, DescriptorGate> =>
    DescriptorGate.admitted("ElementGraphWire").pipe(
      Effect.andThen(Schema.decodeUnknown(ElementGraph.FromBytes)(octets)),
    )
  get byKey(): HashMap.HashMap<ContentKey, Node> {
    return Array.reduce(this.nodes, HashMap.empty<ContentKey, Node>(), (acc, node) => HashMap.set(acc, node.key, node))
  }
}
```

## [3]-[PARITY_WALK]

- Owner: `Parity` — the key-cell projection surface: `paths`, the typed field-mask addresses of the key-bearing fields resolved by name from the descriptor at module evaluation, and `read`, the reflection projection of raw key cells without constructing the domain value.
- Entry: `Parity.read(octets)` — the verbatim graph-key cells in wire form, byte-comparable against the `tests/contracts` fixture; the corpus driver asserts equality, this module only projects.
- Growth: a new key-bearing field is one name in `_KEY_FIELDS` — the path resolution, the projection, and the corpus fixture move together; an unresolvable name dies at module load through the resolve's own throw, because a census-guarded descriptor missing its key field is a build defect, never a runtime state.
- Law: parity is evidence, never repair — cells read verbatim through `reflect`; a production-flow mismatch is a `parity` `WireFault` minted by the verifying caller holding both cells as evidence.
- Law: reflection here is parity-scoped — the generated schema exists, so ordinary decode never reflects; the walk exists solely because parity must read cells positionally without trusting the decode it checks.
- Boundary: `buildPath(schema).field(desc).toPath()`, `reflect(desc, message).get(field)`, and `pathToString` are the `@bufbuild/protobuf` reflection surface; corpus fixtures and their TS reader live in `tests/contracts` and `tests/typescript/_testkit`.

```typescript
import { buildPath, reflect, type DescField, type Path } from "@bufbuild/protobuf"
import { Either, Option } from "effect"

const _KEY_FIELDS = ["key"] as const

const _resolved: ReadonlyArray<DescField> = Array.map(_KEY_FIELDS, (name) =>
  Option.getOrThrow(Array.findFirst(ProtoCodec.suite.ElementGraphWire.fields, (field) => field.name === name)),
)

const Parity: {
  readonly paths: ReadonlyArray<Path>
  readonly read: (octets: Uint8Array) => Either.Either<ReadonlyArray<Uint8Array>, ParseResult.ParseError>
} = {
  paths: Array.map(_resolved, (field) => buildPath(ProtoCodec.suite.ElementGraphWire).field(field).toPath()),
  read: (octets) =>
    Either.map(
      Schema.decodeUnknownEither(ProtoCodec.frame(ProtoCodec.suite.ElementGraphWire))(octets),
      (message) =>
        Array.filterMap(_resolved, (field) =>
          Option.filter(
            Option.some(reflect(ProtoCodec.suite.ElementGraphWire, message).get(field)),
            (cell): cell is Uint8Array => cell instanceof Uint8Array,
          ),
        ),
    ),
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { ElementGraph, Parity }
```
