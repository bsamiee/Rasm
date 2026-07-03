# [WIRE_APPEARANCE]

`codec/appearance.ts` decodes the material plane from `Rasm.Materials/Appearance` — `MaterialWire`, `OpenPbrGroupsWire`, `AppearanceSummaryWire` — as a decode-only, field-for-field mirror of the C# projection: the OpenPBR algebra (parameter grouping, layering, unit semantics) is C#-owned, so a peer re-mint of any material computation is the named `CROSS_LANGUAGE_WIRE` drift defect (MA:65). TS lands the projected values verbatim and `ui/viewer` `scene/appearance` binds them to viewport materials through `#vocab`; every numeric field is carriage, never a recomputation site.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]           | [OWNS]                                                            |
| :-----: | :------------------ | :----------------------------------------------------------------------- |
|   [1]   | `APPEARANCE_FAMILY` | the three mirror owners and the keyed decode surface                      |

## [2]-[APPEARANCE_FAMILY]

- Owner: `Appearance` — one assembled owner over three `Schema.Class` mirrors: `Material` (the bound material row: key, name, the OpenPBR group reference), `PbrGroups` (the grouped OpenPBR parameter blocks: base, specular, transmission, emission, geometry — each block a struct of named scalar/color rows exactly as projected), `Summary` (the per-model appearance census the scene preloads).
- Entry: `Appearance.material`/`Appearance.groups`/`Appearance.summary` per-family byte schemas; `Appearance.decode(family, octets)` the keyed one-shot whose return follows the family key.
- Receipt: `Summary` is the preload census — material count, group keys, texture extents — the scene's residency planner reads it before any material binds; `Material` + `PbrGroups` are the binding material the viewport consumes as-is.
- Growth: a new OpenPBR parameter block or field is a C# projection change mirrored field-for-field here in the same wave; the viewer's binding table picks it up by name. A TS-side derived parameter is not growth — it is the re-mint defect.
- Law: field-for-field means field-for-field — names, groupings, and value ranges mirror the C# projection exactly; a renamed field, a flattened group, or a convenience-merged block breaks the mirror and the golden fixtures.
- Law: colors and scalars are carriage — values land as projected (linear-space triples, unit-interval weights); color-space conversion, tone mapping, and parameter derivation are the viewer's render-side concerns over the delivered values.
- Law: the group reference is a key, not an embed — `Material.groups` references `PbrGroups` by key so shared parameter sets deduplicate on the wire and rebind cheaply; the viewer resolves the reference against its census.
- Boundary: viewport material binding is `ui/viewer` `scene/appearance`; the census fences the three families to this page; GLB-embedded materials ride the GLB rail untouched (`frame/geometry.ts`) and this plane OVERRIDES them only where a `Material` row targets a mesh key.

```typescript
import { ContentKey } from "@rasm/ts/kernel"
import { Effect, type ParseResult, Schema } from "effect"
import { ProtoCodec } from "./proto.ts"

const _Color = Schema.Tuple(Schema.Number, Schema.Number, Schema.Number)
const _Weight = Schema.Number.pipe(Schema.between(0, 1))

const _Base = Schema.Struct({ color: _Color, metalness: _Weight, roughness: _Weight, weight: _Weight })
const _Specular = Schema.Struct({ color: _Color, weight: _Weight, ior: Schema.Number.pipe(Schema.positive()) })
const _Transmission = Schema.Struct({ weight: _Weight, color: _Color, depth: Schema.Number.pipe(Schema.nonNegative()) })
const _Emission = Schema.Struct({ color: _Color, luminance: Schema.Number.pipe(Schema.nonNegative()) })
const _Geometry = Schema.Struct({ opacity: _Weight, thinWalled: Schema.Boolean })

class PbrGroups extends Schema.Class<PbrGroups>("PbrGroups")({
  key: ContentKey.FromCell,
  base: _Base,
  specular: _Specular,
  transmission: _Transmission,
  emission: _Emission,
  geometry: _Geometry,
}) {}

class Material extends Schema.Class<Material>("Material")({
  key: ContentKey.FromCell,
  name: Schema.NonEmptyString,
  groups: ContentKey.FromCell,
}) {}

class Summary extends Schema.Class<Summary>("Summary")({
  model: ContentKey.FromCell,
  materials: Schema.Int.pipe(Schema.nonNegative()),
  groupKeys: Schema.Array(ContentKey.FromCell),
}) {}

const _rows = {
  MaterialWire: ProtoCodec.family(ProtoCodec.suite.MaterialWire, Material),
  OpenPbrGroupsWire: ProtoCodec.family(ProtoCodec.suite.OpenPbrGroupsWire, PbrGroups),
  AppearanceSummaryWire: ProtoCodec.family(ProtoCodec.suite.AppearanceSummaryWire, Summary),
} as const

declare namespace Appearance {
  type Family = keyof typeof _rows
  type Decoded<K extends Family> = Schema.Schema.Type<(typeof _rows)[K]>
}

const Appearance: {
  readonly Material: typeof Material
  readonly PbrGroups: typeof PbrGroups
  readonly Summary: typeof Summary
  readonly material: (typeof _rows)["MaterialWire"]
  readonly groups: (typeof _rows)["OpenPbrGroupsWire"]
  readonly summary: (typeof _rows)["AppearanceSummaryWire"]
  readonly decode: <K extends Appearance.Family>(family: K, octets: Uint8Array) => Effect.Effect<Appearance.Decoded<K>, ParseResult.ParseError>
} = {
  Material,
  PbrGroups,
  Summary,
  material: _rows.MaterialWire,
  groups: _rows.OpenPbrGroupsWire,
  summary: _rows.AppearanceSummaryWire,
  decode: (family, octets) => Schema.decodeUnknown(_rows[family])(octets),
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Appearance }
```
