# [WIRE_BCF]

`codec/bcf.ts` decodes the issue-collaboration plane from `Rasm.Bim` — `BcfTopicWire` and `BcfViewpointWire`. BCF vocabulary is fenced to `wire` codecs and `ui/viewer` marks (invariant 7): topics carry the issue lifecycle over closed status/priority vocabularies, viewpoints carry the camera and the GlobalId anchor sets that re-locate an issue in the scene, and `ui/viewer` `mark/bcf` consumes both through `#vocab`. GlobalIds cross as brand-refined fields — the IFC 22-character identity — and no other folder ever sees a BCF shape.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]         | [OWNS]                                                       |
| :-----: | :---------------- | :------------------------------------------------------------------ |
|   [1]   | `TOPIC_VIEWPOINT` | the `Bcf` owner: topic and viewpoint classes, vocabularies, decode   |

## [2]-[TOPIC_VIEWPOINT]

- Owner: `Bcf` — one assembled owner over two `Schema.Class` families: `Topic` (guid, lifecycle vocabularies, comment rows at full depth) and `Viewpoint` (camera block, GlobalId selection set, clipping planes); the GlobalId brand is one interior field schema composed into both, exported by neither.
- Entry: `Bcf.topic`/`Bcf.viewpoint` the per-family byte schemas; `Bcf.decode(family, octets)` the keyed one-shot whose return follows the family.
- Receipt: a viewpoint is a re-location proof — camera plus anchors; the mark UI restores the view and highlights the selection set, and a missing anchor (a GlobalId absent from the loaded model) is the VIEWER's absence case, not a decode fault.
- Growth: a new topic lifecycle field, comment axis, or viewpoint component (a bitmap block, a section-box) is one field mirroring the C# emit; a new status or priority is one literal row.
- Law: vocabularies are closed at the seam — `status` (`open`/`in-progress`/`resolved`/`closed`) and `priority` (`low`/`normal`/`high`/`critical`) decode through literal rows; free-string lifecycle fields are the rejected shape.
- Law: the GlobalId is a brand-in-field refinement — 22 characters over the IFC base64 alphabet — inside the owning classes; a standalone exported GlobalId brand is the named defect, and consumers reach it as `Bcf.Viewpoint["selection"][number]`.
- Law: the camera block is consume-only carriage — position, direction, up, field-of-view land verbatim for the viewer's camera restore; TS computes no view transform here (invariant 7).
- Boundary: mark rendering and anchor resolution are `ui/viewer` `mark/bcf`; BIM exchange golden bytes are `codec/bim.ts`; the census fences both wire families to this page.

```typescript
import { Effect, type ParseResult, Schema } from "effect"
import { ProtoCodec } from "./proto.ts"

const _statuses = ["open", "in-progress", "resolved", "closed"] as const
const _priorities = ["low", "normal", "high", "critical"] as const

const _GlobalId = Schema.String.pipe(
  Schema.length(22),
  Schema.pattern(/^[0-9A-Za-z_$]{22}$/),
  Schema.brand("GlobalId"),
)

const _Comment = Schema.Struct({
  author: Schema.NonEmptyString,
  at: Schema.DateTimeUtc,
  body: Schema.NonEmptyString,
  viewpoint: Schema.optionalWith(Schema.UUID, { as: "Option" }),
})

class Topic extends Schema.Class<Topic>("Topic")({
  guid: Schema.UUID,
  title: Schema.NonEmptyString,
  status: Schema.Literal(..._statuses),
  priority: Schema.Literal(..._priorities),
  labels: Schema.Array(Schema.NonEmptyString),
  assignee: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
  due: Schema.optionalWith(Schema.DateTimeUtc, { as: "Option" }),
  comments: Schema.Array(_Comment),
}) {}

const _Camera = Schema.Struct({
  position: Schema.Tuple(Schema.Number, Schema.Number, Schema.Number),
  direction: Schema.Tuple(Schema.Number, Schema.Number, Schema.Number),
  up: Schema.Tuple(Schema.Number, Schema.Number, Schema.Number),
  fieldOfView: Schema.Number.pipe(Schema.positive()),
})

const _Plane = Schema.Struct({
  origin: Schema.Tuple(Schema.Number, Schema.Number, Schema.Number),
  normal: Schema.Tuple(Schema.Number, Schema.Number, Schema.Number),
})

class Viewpoint extends Schema.Class<Viewpoint>("Viewpoint")({
  guid: Schema.UUID,
  topic: Schema.UUID,
  camera: _Camera,
  selection: Schema.Array(_GlobalId),
  clipping: Schema.Array(_Plane),
}) {}

type _Landing = {
  readonly BcfTopicWire: Topic
  readonly BcfViewpointWire: Viewpoint
}

const _rows: { readonly [K in keyof _Landing]: Schema.Schema<_Landing[K], Uint8Array> } = {
  BcfTopicWire: ProtoCodec.family(ProtoCodec.suite.BcfTopicWire, Topic),
  BcfViewpointWire: ProtoCodec.family(ProtoCodec.suite.BcfViewpointWire, Viewpoint),
}

declare namespace Bcf {
  type Family = keyof _Landing
  type Decoded<K extends Family> = _Landing[K]
}

const Bcf: {
  readonly Topic: typeof Topic
  readonly Viewpoint: typeof Viewpoint
  readonly topic: (typeof _rows)["BcfTopicWire"]
  readonly viewpoint: (typeof _rows)["BcfViewpointWire"]
  readonly decode: <K extends Bcf.Family>(family: K, octets: Uint8Array) => Effect.Effect<Bcf.Decoded<K>, ParseResult.ParseError>
} = {
  Topic,
  Viewpoint,
  topic: _rows.BcfTopicWire,
  viewpoint: _rows.BcfViewpointWire,
  decode: (family, octets) => Schema.decodeUnknown(_rows[family])(octets),
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Bcf }
```
