# [UI_APPEARANCE]

`viewer/scene/appearance.ts` binds the C#-owned OpenPBR algebra onto the render material: `Appearance.Material`/`Appearance.PbrGroups`/`Appearance.Summary` arrive DECODED through `wire/vocab` (MA:65 — a field-for-field mirror of the `Rasm.Materials` projection), and this module's one fold, `Pbr.bind`, lands those values onto `MeshPhysicalMaterial`'s lobes — base, specular, transmission, coat, sheen, emission — verbatim, with color triples crossing the linear-space seam through the one `token/theme` color authority. Every numeric is carriage: a TS-side derivation, regrouping, or convenience-merge of any OpenPBR parameter is the `CROSS_LANGUAGE_WIRE` drift defect.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]        | [OWNS]                                                                     |
| :-----: | :--------------- | :---------------------------------------------------------------------------- |
|   [1]   | `BIND_FOLD`      | `Pbr.bind` — the field-for-field lobe assignment onto `MeshPhysicalMaterial`   |
|   [2]   | `COLOR_CONTRACT` | the linear-space crossing and the one color authority shared with the tokens   |
|   [3]   | `CENSUS_OVERRIDE`| the `Summary` preload census and the mesh-key override law                     |
|   [4]   | `NODE_MIRROR`    | the WebGPU `MeshPhysicalNodeMaterial` mirror row                               |

## [2]-[BIND_FOLD]

- Owner: `Pbr` — one fold: `Pbr.bind(material, bound)` where `bound` is the resolved pair (a `Material` row plus its `PbrGroups` reference resolved against the census); the wire's five blocks assign onto the physical lobes exactly as projected — base → `color`/`metalness`/`roughness`, specular → `specularColor`/`specularIntensity`/`ior`, transmission → `transmission`/`attenuationColor`/`attenuationDistance` (the wire's `depth` IS the attenuation depth), emission → `emissive`/`emissiveIntensity` (the wire's `luminance`), geometry → `opacity`/`transparent`/`side` — and `needsUpdate` stamps once at the fold's tail.
- Packages: `three` (`MeshPhysicalMaterial` lobes, `Color`, `LinearSRGBColorSpace`, `FrontSide`/`DoubleSide` — members verified against the shipped runtime exports; `@types/three` is not admitted), `@rasm/ts/wire/vocab` (`Appearance` — the decoded types derive as `Schema.Schema.Type` off the `#vocab` class values, never a parallel shape).
- Law: assignments mirror the projection's grouping — the fold's arm order IS the wire's block order (base, specular, transmission, emission, geometry), so a C# projection change lands here as the same-shaped field wave; a flattened group or a renamed field breaks the mirror and the golden fixtures upstream.
- Law: unit semantics are C#-owned — weights arrive unit-interval, distances arrive in the projection's units; a clamp, remap, or "fix" in the fold is the drift defect, and an out-of-range value is upstream evidence.
- Law: `transparent` and `side` are the two render-representation toggles, not derivations — three demands the booleans, so `opacity < 1` raises the blend flag and `thinWalled` selects `DoubleSide`; both are structural consequences of carriage, and no other computed value exists in the fold.
- Growth: a new OpenPBR block (coat, sheen, iridescence, anisotropy — `MeshPhysicalMaterial` already carries the target lobes) is one wire mirror field wave plus one assignment arm here, landed in the same wave as the C# projection change — the fold signature never changes and TS never emits a block ahead of the wire.

```typescript
import type { Appearance } from "@rasm/ts/wire/vocab"
import { DoubleSide, FrontSide, LinearSRGBColorSpace, type Color, type MeshPhysicalMaterial } from "three"
import type { Schema } from "effect"

type _Material = Schema.Schema.Type<typeof Appearance.Material>
type _Groups = Schema.Schema.Type<typeof Appearance.PbrGroups>

declare namespace Pbr {
  type Bound = { readonly material: _Material; readonly groups: _Groups }
}

const _tint = (target: Color, triple: readonly [number, number, number]): Color =>
  target.setRGB(triple[0], triple[1], triple[2], LinearSRGBColorSpace)

const _bind = (material: MeshPhysicalMaterial, bound: Pbr.Bound): MeshPhysicalMaterial => {
  const groups = bound.groups
  _tint(material.color, groups.base.color)
  material.metalness = groups.base.metalness
  material.roughness = groups.base.roughness
  _tint(material.specularColor, groups.specular.color)
  material.specularIntensity = groups.specular.weight
  material.ior = groups.specular.ior
  material.transmission = groups.transmission.weight
  _tint(material.attenuationColor, groups.transmission.color)
  material.attenuationDistance = groups.transmission.depth
  _tint(material.emissive, groups.emission.color)
  material.emissiveIntensity = groups.emission.luminance
  material.opacity = groups.geometry.opacity
  material.transparent = groups.geometry.opacity < 1
  material.side = groups.geometry.thinWalled ? DoubleSide : FrontSide
  material.needsUpdate = true
  return material
}
```

## [3]-[COLOR_CONTRACT]

- Law: wire color triples are linear-space carriage — the projection emits linear values, `Color.setRGB(r, g, b, LinearSRGBColorSpace)` ingests them under three's `ColorManagement`, and the renderer's `outputColorSpace` (stamped at `viewer/scene/glb` construction) owns display transform; no gamma math ever appears in this module.
- Law: one color authority spans tokens and render — where a THEME color reaches the scene (selection tint, highlight overlay), it crosses through `Theme.linear` (`token/theme`'s OKLCH → srgb-linear projection) into the same `setRGB` seam, so a token color and a wire color land through one contract and drift is structurally impossible.
- Boundary: the OKLCH authoring algebra is `token/theme`'s; the display-transform policy is the renderer construction row's.

## [4]-[CENSUS_OVERRIDE]

- Law: `Appearance.Summary` is the preload census — material count, group keys, texture extents — read BEFORE any bind so the scene plans residency and resolves every `Material.groups` reference once into an interior `HashMap<groupKey, PbrGroups>`; a dangling reference is upstream evidence surfaced as-is, never a silent default material.
- Law: the override law is keyed — a `Material` row targets a mesh content key, and the bind fold applies ONLY to targeted meshes; GLB-embedded materials on untargeted meshes ride untouched (the GLB rail owns them), so appearance is an overlay, never a repaint of the world.
- Law: rebinding is idempotent — the fold is pure assignment over a resolved pair; a group edit arriving as a fresh wire value re-runs `bind` on the affected meshes and nothing else, driven by the same keyed ledger `viewer/scene/glb` grafts under.

```typescript
import { HashMap, Option } from "effect"

declare namespace Census {
  type Index = HashMap.HashMap<_Material["groups"], _Groups>
}

const _resolve = (index: Census.Index, material: _Material): Option.Option<Pbr.Bound> =>
  Option.map(HashMap.get(index, material.groups), (groups) => ({ material, groups }))
```

## [5]-[NODE_MIRROR]

- Law: the WebGPU path swaps the material class, never the fold — `MeshPhysicalNodeMaterial` (`three/webgpu`) carries the same lobe fields, so `Pbr.bind`'s assignments land unchanged; TSL node-graph authoring (`three/tsl`) is reached only where a lobe must become a computed node (a probe-driven debug view), and such a node graph is render-side presentation, never OpenPBR algebra.
- Law: the backend discriminant is upstream — `viewer/scene/glb`'s renderer row decides WebGL/WebGPU; this module receives the constructed material and stays backend-agnostic by writing only shared lobe fields.

```typescript
const Pbr: {
  readonly bind: typeof _bind
  readonly resolve: typeof _resolve
} = {
  bind: _bind,
  resolve: _resolve,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Pbr }
```
