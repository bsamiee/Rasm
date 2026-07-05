# [UI_SCENE]

The GLB scene owner: content-key-keyed mesh residency rendered through one of two backends behind one `Schema.Literal` discriminant — the imperative `three` scene (custom materials, instancing, animation, compute, receipts) or the declarative `<model-viewer>` embed (zero GL handle) — with every mesh byte arriving through the `GlbViewport` port this module declares and the app composition satisfies by forwarding `runtime/browser/fetch#DEPOT_SCHEDULER`'s `Depot.haul` arrivals and ledger. The graft fold keeps GLB animation alive (one mixer per animated subtree under one `Clock`), collapses draw calls through merge/instanced/batched rows, and binds the C#-owned OpenPBR algebra field-for-field onto `MeshPhysicalMaterial` lobes with color crossing the linear seam through the one `system/token` authority. Georeferenced element instancing rides the `@deck.gl/mesh-layers` pair as layer values `geo` sinks. GPU resources are `Scope`-bracketed without exception, the frame loop parks under `system/act#DOCUMENT_RAIL`'s hidden `<Activity>` row, faults ride one `GlbFault` family over a closed reason vocabulary, and meshopt decode stays gated `[R23]`. The module is `ui/viewer/src/scene.ts`.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]         | [OWNS]                                                                          | [PUBLIC]      |
| :-----: | :---------------- | :--------------------------------------------------------------------------------- | :------------ |
|  [01]   | `VIEWPORT_PORT`   | the `GlbViewport` port — residency ledger read + verified-arrival ingress as a Tag  | `GlbViewport` |
|  [02]   | `FAULT_FAMILY`    | `GlbFault` — the closed reason vocabulary and its policy table                      | `GlbFault`    |
|  [03]   | `BACKEND_SELECT`  | the backend literal, renderer acquisition, output policy, light rig, compute lane   | `Glb`         |
|  [04]   | `RESIDENCY_GRAFT` | the one-`Scene` graft/dispose/animate fold and the codec-injected loader            | `Glb`         |
|  [05]   | `DRAW_COLLAPSE`   | merge/instanced/batched rows — repeated element geometry as bounded draw calls      | `Glb`         |
|  [06]   | `APPEARANCE_BIND` | the OpenPBR lobe assignment, color contract, census resolve, node-material mirror   | `Pbr`         |
|  [07]   | `INSTANCED_ROWS`  | `@deck.gl/mesh-layers` — georeferenced element instances as deck layer values       | `Instanced`   |
|  [08]   | `EMBED_ROW`       | the `<model-viewer>` backend row — decoder statics, object-URL bracket, animation   | `Glb`         |

## [2]-[VIEWPORT_PORT]

[VIEWPORT_PORT]:
- Owner: `GlbViewport` — the runtime-capability port this folder declares and NEVER implements: `ledger` is the residency read (`core/interchange/frame`'s `Residency.Ledger` published `Subscribable`, the same cell `Depot` owns), and `arrivals` is the verified-octet ingress — each element pairs a `ContentKey` with whole-buffer GLB bytes the hauling side already reassembled and re-proved through the one `Parity` delegate, so bytes reaching this module are proof-carrying by construction. The browser composition root satisfies the Tag by forwarding `Depot.haul` arrival pairs and the `Depot` ledger cell.
- Packages: `effect` (`Context.Tag`, `Stream`, `Subscribable`); `@rasm/ts/core` (`ContentKey`, `Residency`).
- Law: the port is the ONLY byte ingress — a fetch, a worker message, or an `ArrayBuffer` reaching this module by any other path bypasses content-key verification and is the named defect.
- Law: arrival octets are whole-buffer — the hauling side reassembles into a fresh buffer with `byteOffset` zero, so `parseAsync(octets.buffer, "")` is exact by contract; a sliced view smuggling a neighbor's bytes is the provider's defect, never the graft's guard.
- Law: the wire shapes reach this module as the settled core vocabulary — `Residency.Ledger` rows and `ContentKey` compose directly; a parallel manifest shape, row twin, or local key notion is unspellable because the core owner is the only declaration.
- Growth: prefetch hints, eviction acknowledgement, and priority lanes land as members HERE when earned — consumers already hold the Tag, so growth is a member row, never a second port.
- Boundary: haul scheduling, cache warmth, worker verify, and byte budgets are `runtime/browser/fetch`'s; the residency protocol and its fold law are `core/interchange/frame`'s.

```typescript
import type { ContentKey, Residency } from "@rasm/ts/core"
import { Context, type Stream, type Subscribable } from "effect"

declare namespace GlbViewport {
  type Arrival = { readonly key: ContentKey; readonly octets: Uint8Array }
}

class GlbViewport extends Context.Tag("ui/viewer/GlbViewport")<GlbViewport, {
  readonly ledger: Subscribable.Subscribable<Residency.Ledger>
  readonly arrivals: Stream.Stream<GlbViewport.Arrival, GlbFault>
}>() {}
```

## [3]-[FAULT_FAMILY]

[FAULT_FAMILY]:
- Owner: `GlbFault` — one `Data.TaggedError` sized by routing with a closed `reason` vocabulary: `manifest-skew` (an arrival references a mesh the ledger never named), `key-mismatch` (verification refused the octets at the port boundary), `decode-refused` (loader or codec rejected the payload), `codec-absent` (the asset demands a codec the viewport did not wire — the meshopt gate's refusal spelling), `backend-lost` (GPU device loss). The policy table carries `rank`/`retry`/`evict` per reason and the class getter projects it, so recovery reads policy rows, never re-derives them per arm. Code-keyed free-string fault construction is the named discard — reasons are a closed vocabulary.
- Packages: `effect` (`Data`).
- Law: `backend-lost` is the re-init row — `GPUDevice.lost` resolves into a fault whose recovery re-runs backend selection under the same `Scope`; residency state survives because the graft ledger is renderer-independent.
- Law: a fault no consumer arm can act on escalates — a torn invariant inside the graft fold dies through `Effect.die`, keeping the channel total over actionable faults.
- Law: the reason table is scene-local vocabulary — the transport-altitude `Hops` table is `core/interchange/codec`'s and never restates here; a scene reason names a residency/render condition, never a wire condition.
- Growth: a new failure condition (a compute-pass refusal, an embed mount loss) is one reason row plus its policy columns — every policy consumer inherits it through the getter.

```typescript
import { Data } from "effect"

const _reasons = {
  "manifest-skew": { rank: 3, retry: false, evict: true },
  "key-mismatch": { rank: 5, retry: false, evict: true },
  "decode-refused": { rank: 4, retry: false, evict: true },
  "codec-absent": { rank: 2, retry: false, evict: false },
  "backend-lost": { rank: 4, retry: true, evict: false },
} as const

declare namespace GlbFault {
  type Reason = keyof typeof _reasons
  type Row = (typeof _reasons)[Reason]
}

class GlbFault extends Data.TaggedError("GlbFault")<{
  readonly reason: GlbFault.Reason
  readonly mesh: string
  readonly detail: string
}> {
  static readonly reasons: typeof _reasons = _reasons
  get policy(): GlbFault.Row {
    return _reasons[this.reason]
  }
}
```

## [4]-[BACKEND_SELECT]

[BACKEND_SELECT]:
- Owner: the closed backend vocabulary (`three` | `model-viewer`) spread from one `as const` tuple into `Schema.Literal`, and the `three` arm's renderer acquisition under `Effect.acquireRelease`: probe `navigator.gpu`, construct `WebGPURenderer` and await `init()` when present (feature-gating through `renderer.hasFeature` post-init), else `WebGLRenderer` — a refused `init()` on a present-but-broken adapter degrades to the same WebGL floor at the acquisition, so `backend-lost` stays reserved for a lost LIVE device and the probe never faults the channel; the output policy row (`outputColorSpace`, `toneMapping`, `toneMappingExposure`) stamps at construction; release disposes — three has no GPU garbage collection, so the finalizer IS memory correctness.
- Packages: `three` (`WebGLRenderer`, `SRGBColorSpace`, `ACESFilmicToneMapping`, the light classes); `three/webgpu` (`WebGPURenderer`); `effect` (`Effect`, `Schema`); `@webgpu/types` (ambient, a viewer-tsconfig `types` entry, never an import).
- Law: one usage contract over both renderer backends — scene, camera, and loop code are backend-agnostic after construction; the WebGPU upgrade is a construction-site swap, never a scene rewrite.
- Law: lighting is a rig of rows — the analytic light vocabulary (`ambient`, `hemisphere`, `directional`) is one `as const` table materialized by one fold beside the IBL bake (`PMREMGenerator.fromScene(RoomEnvironment())` serving `scene.environment` once); a hand-placed light outside the rig table, or per-frame IBL work, is the named defect.
- Law: texture upload is eager — `renderer.initTexture(texture)` runs at graft time for every decoded texture so the first frame that samples it never hitches; lazy first-use upload is the named defect on large residency sets.
- Law: the compute lane is WebGPU-gated — per-instance culling and skinning land as `computeAsync` passes over `StorageBufferNode` buffers when the selected backend carries the feature, authored through `three/tsl` node graphs; the WebGL arm renders the same scene without the pass, and a compute result feeding appearance is `[6]`'s debug-view boundary, never OpenPBR algebra.
- Law: the loop parks under `<Activity>` hidden — `setAnimationLoop(null)` on hide, re-arm on visible (`system/act#DOCUMENT_RAIL` consumed as settled); a loop burning under a hidden viewport is the named defect.
- Boundary: camera drive is `geo`'s; receipt readback and `compileAsync` settle discipline are `probe`'s; the `<canvas>` element arrives as a parameter from the app shell.

```typescript
import { Effect, Schema } from "effect"
import {
  ACESFilmicToneMapping, AmbientLight, DirectionalLight, HemisphereLight, SRGBColorSpace, WebGLRenderer,
} from "three"
import type { Light, Scene } from "three"
import { WebGPURenderer } from "three/webgpu"

const _backends = ["three", "model-viewer"] as const

const _Backend = Schema.Literal(..._backends)

const _OUTPUT = { colorSpace: SRGBColorSpace, toneMapping: ACESFilmicToneMapping, exposure: 1 } as const

const _RIG = {
  ambient: { color: 0xffffff, intensity: 0.3 },
  hemisphere: { sky: 0xffffff, ground: 0x444444, intensity: 0.6 },
  directional: { color: 0xffffff, intensity: 1.2, position: [4, 8, 4] },
} as const

const _lit = (root: Scene): Scene => {
  const sun = new DirectionalLight(_RIG.directional.color, _RIG.directional.intensity)
  sun.position.set(_RIG.directional.position[0], _RIG.directional.position[1], _RIG.directional.position[2])
  const rows: ReadonlyArray<Light> = [
    new AmbientLight(_RIG.ambient.color, _RIG.ambient.intensity),
    new HemisphereLight(_RIG.hemisphere.sky, _RIG.hemisphere.ground, _RIG.hemisphere.intensity),
    sun,
  ]
  return rows.reduce((held, light) => held.add(light), root)
}

const _renderer = (canvas: HTMLCanvasElement) =>
  Effect.acquireRelease(
    Effect.gen(function* () {
      const built = globalThis.navigator.gpu === undefined
        ? new WebGLRenderer({ canvas, antialias: true })
        : yield* Effect.tryPromise({
            try: async () => {
              const gpu = new WebGPURenderer({ canvas, antialias: true })
              await gpu.init()
              return gpu
            },
            catch: () => new GlbFault({ reason: "backend-lost", mesh: "<boot>", detail: "<webgpu-init>" }),
          }).pipe(Effect.orElseSucceed(() => new WebGLRenderer({ canvas, antialias: true })))
      built.outputColorSpace = _OUTPUT.colorSpace
      built.toneMapping = _OUTPUT.toneMapping
      built.toneMappingExposure = _OUTPUT.exposure
      return built
    }),
    (renderer) => Effect.sync(() => renderer.dispose()),
  )
```

## [5]-[RESIDENCY_GRAFT]

[RESIDENCY_GRAFT]:
- Owner: `Glb.graft` — the residency fold over the port: one `Scene` roots the graph, an interior ledger (`HashMap<ContentKey, Glb.Graft>`) tracks grafted subtrees, and two arms drive it concurrently — the arrival arm parses verified octets through the ONE codec-injected `GLTFLoader` (`parseAsync` over the whole buffer), mints the animation half (a per-subtree `AnimationMixer` whose every `gltf.animations` clip binds through `clipAction(...).setLoop(LoopRepeat, Infinity).play()` — the loader result's animation array is retained, never discarded), grafts `gltf.scene` under the root, and registers the mixer on the loop roster; the eviction arm diffs the port ledger's `evicted` rows against held grafts, removes the subtree, tears the mixer down (`stopAllAction` then `uncacheRoot`) and walks it through the disposal kernel before the interior ledger drops the key. The returned `Glb.Loop.advance(delta)` is the one tick the frame loop calls — every live mixer advances under one `Clock` delta.
- Packages: `three` (`Scene`, `Mesh`, `AnimationMixer`, `Clock`, `LoopRepeat`); `three/addons` (`GLTFLoader`); `effect` (`Effect`, `HashMap`, `Option`, `Ref`, `Stream`, `Scope`); `@rasm/ts/core` (`ContentKey`).
- Law: codec injection is capability wiring — `setDRACOLoader`/`setKTX2Loader` attach at loader construction with transcoder paths pinned self-hosted (`setDecoderPath`/`setTranscoderPath`, `detectSupport(renderer)` reading the compressed-format capability); `setMeshoptDecoder` attaches ONLY when the asset flags `EXT_meshopt_compression` and the `[R23]` gate has admitted a decoder identity — until then such an asset refuses with `codec-absent`.
- Law: the disposal kernel is total over GPU handles — every visited `Mesh` releases its geometry, every texture slot its materials hold, then the materials themselves, because `material.dispose()` frees the program and never its textures; three's `traverse` callback is the kernel's platform-forced statement seam and no reference escapes it.
- Law: `preload`/`preinit` hints warm decoder wasm and imminent GLB fetches ahead of first frame (`react-dom` hint family), issued from the ledger's `pending` census, never per-mesh at draw time.
- Exemption: the graft kernel's mixer roster is operation-local mutable state read synchronously by the frame loop — the loop callback is the platform-forced statement seam, and only the immutable ledger snapshot leaves the fold.
- Growth: a new residency policy (priority lanes, partial LOD) is a fold arm over new ledger rows minted at `core/interchange/frame` — the graft signature never changes; a new animation policy (clip selection, cross-fade) is one action-policy row applied at mint.

```typescript
import type { ContentKey } from "@rasm/ts/core"
import { Context, Effect, HashMap, Option, Ref, Scope, Stream } from "effect"
import { AnimationMixer, Clock, LoopRepeat, Mesh, Scene, Texture } from "three"
import type { Object3D, PerspectiveCamera } from "three"
import { GLTFLoader } from "three/addons/loaders/GLTFLoader.js"

declare namespace Glb {
  type Backend = typeof _Backend.Type
  type Graft = { readonly node: Object3D; readonly mixer: Option.Option<AnimationMixer> }
  type Ledger = HashMap.HashMap<ContentKey, Glb.Graft>
  type Loop = { readonly advance: (delta: number) => void }
}

const _dispose = (node: Object3D): void => {
  node.traverse((child) => {
    if (child instanceof Mesh) {
      child.geometry.dispose()
      const materials = Array.isArray(child.material) ? child.material : [child.material]
      materials.forEach((material) => {
        Object.values(material).forEach((slot) => slot instanceof Texture && slot.dispose())
        material.dispose()
      })
    }
  })
}

const _graft = (
  root: Scene,
  loader: GLTFLoader,
  port: Context.Tag.Service<GlbViewport>,
): Effect.Effect<Glb.Loop, GlbFault, Scope.Scope> =>
  Effect.gen(function* () {
    const held = yield* Ref.make(HashMap.empty<ContentKey, Glb.Graft>())
    const roster: Array<AnimationMixer> = []
    const insert = Stream.runForEach(port.arrivals, (arrival) =>
      Effect.gen(function* () {
        const gltf = yield* Effect.tryPromise({
          try: () => loader.parseAsync(arrival.octets.buffer, ""),
          catch: (defect) =>
            new GlbFault({ reason: "decode-refused", mesh: `${arrival.key}`, detail: String(defect) }),
        })
        const mixer = gltf.animations.length === 0
          ? Option.none<AnimationMixer>()
          : Option.some(
              gltf.animations.reduce((bound, clip) => {
                bound.clipAction(clip).setLoop(LoopRepeat, Number.POSITIVE_INFINITY).play()
                return bound
              }, new AnimationMixer(gltf.scene)),
            )
        yield* Effect.sync(() => {
          root.add(gltf.scene)
          Option.map(mixer, (live) => roster.push(live))
        })
        yield* Ref.update(held, HashMap.set(arrival.key, { node: gltf.scene, mixer }))
      }))
    const evict = Stream.runForEach(port.ledger.changes, (rows) =>
      Effect.gen(function* () {
        const grafts = yield* Ref.get(held)
        const gone = HashMap.filter(grafts, (_, key) =>
          Option.match(HashMap.get(rows, key), {
            onNone: () => true,
            onSome: (row) => row.state === "evicted",
          }))
        yield* Effect.forEach(gone, ([key, graft]) =>
          Effect.zipRight(
            Effect.sync(() => {
              Option.map(graft.mixer, (live) => {
                live.stopAllAction()
                live.uncacheRoot(graft.node)
                roster.splice(roster.indexOf(live), 1)
              })
              root.remove(graft.node)
              _dispose(graft.node)
            }),
            Ref.update(held, HashMap.remove(key)),
          ))
      }))
    yield* Effect.forkScoped(Effect.all([insert, evict], { concurrency: 2, discard: true }))
    return { advance: (delta) => roster.forEach((mixer) => mixer.update(delta)) }
  })

const _loop = (
  renderer: WebGLRenderer | WebGPURenderer,
  scene: Scene,
  camera: PerspectiveCamera,
  live: Glb.Loop,
): Effect.Effect<void> =>
  Effect.sync(() => {
    const clock = new Clock()
    renderer.setAnimationLoop(() => {
      live.advance(clock.getDelta())
      renderer.render(scene, camera)
    })
  })
```

## [6]-[DRAW_COLLAPSE]

[DRAW_COLLAPSE]:
- Owner: the collapse rows the graft applies at parse time, keyed by what repeats: same-material submeshes within one graft merge through `BufferGeometryUtils.mergeGeometries`; N identical geometries collapse into one `InstancedMesh` whose per-instance transform stamps through `setMatrixAt` and re-bounds through `computeBoundingSphere`; distinct same-material geometries batch into one `BatchedMesh` — `addGeometry` per unique geometry, `addInstance` per placement, `setVisibleAt` as the per-instance visibility toggle the selection echo and the compute-lane cull both drive.
- Packages: `three` (`InstancedMesh`, `BatchedMesh`, `Matrix4`); `three/addons` (`BufferGeometryUtils`).
- Law: collapse never alters appearance identity — a merged or batched node keeps its source meshes' material keys so `[7]`'s keyed override still targets exactly the meshes the wire names; a collapse that widens an override's blast radius is the named defect.
- Law: visibility is a flag, never a rebuild — hiding an instanced element flips `setVisibleAt`; removing and re-adding geometry for a visibility change is the named defect.
- Growth: a new repetition signal (an element-graph repetition census) is one detection row feeding the same three collapse arms — the arms never multiply.

```typescript
import { BatchedMesh, InstancedMesh, Matrix4 } from "three"
import type { BufferGeometry, Material } from "three"
import { mergeGeometries } from "three/addons/utils/BufferGeometryUtils.js"

const _instanced = (geometry: BufferGeometry, material: Material, placements: ReadonlyArray<Matrix4>): InstancedMesh => {
  const built = new InstancedMesh(geometry, material, placements.length)
  placements.forEach((matrix, rank) => built.setMatrixAt(rank, matrix))
  built.computeBoundingSphere()
  return built
}

const _batched = (
  material: Material,
  parts: ReadonlyArray<{ readonly geometry: BufferGeometry; readonly placements: ReadonlyArray<Matrix4> }>,
  budget: { readonly instances: number; readonly vertices: number; readonly indices: number },
): BatchedMesh =>
  parts.reduce((batch, part) => {
    const geoId = batch.addGeometry(part.geometry)
    part.placements.forEach((matrix) => batch.setMatrixAt(batch.addInstance(geoId), matrix))
    return batch
  }, new BatchedMesh(budget.instances, budget.vertices, budget.indices, material))

const _merged = (parts: ReadonlyArray<BufferGeometry>): BufferGeometry => mergeGeometries([...parts])
```

## [7]-[APPEARANCE_BIND]

[APPEARANCE_BIND]:
- Owner: `Pbr` — the field-for-field bind of the C# OpenPBR algebra: `Material`/`PbrGroups`/`AppearanceSummary` arrive DECODED through `core/interchange/codec#LANDING_WIRE` (the verbatim mirror of the `csharp:Rasm.Materials/Appearance` projection), and `Pbr.bind(material, bound)` lands the five wire blocks onto `MeshPhysicalMaterial` lobes exactly as projected — base → `color`/`metalness`/`roughness`, specular → `specularColor`/`specularIntensity`/`ior`, transmission → `transmission`/`attenuationColor`/`attenuationDistance` (the wire's `depth` IS the attenuation depth), emission → `emissive`/`emissiveIntensity` (the wire's `luminance`), geometry → `opacity`/`transparent`/`side` — with `needsUpdate` stamped once at the fold's tail. Every numeric is carriage: a TS-side derivation, regrouping, or convenience-merge of any OpenPBR parameter is the cross-language drift defect.
- Packages: `three` (`MeshPhysicalMaterial` lobes, `Color`, `LinearSRGBColorSpace`, `FrontSide`/`DoubleSide` — members verified against the shipped runtime; `@types/three` supplies compile-time declarations only, never member truth); `@rasm/ts/core` (`Material`, `PbrGroups`, `AppearanceSummary`, `ContentKey`); `effect` (`HashMap`, `Option`).
- Law: assignments mirror the projection's grouping — the fold's arm order IS the wire's block order, so a C# projection change lands here as the same-shaped field wave; a flattened group or renamed field breaks the mirror and the golden fixtures upstream.
- Law: unit semantics are C#-owned — weights arrive unit-interval, distances in the projection's units; a clamp, remap, or correction in the fold is the drift defect, and an out-of-range value is upstream evidence. `transparent` and `side` are the two render-representation toggles three demands (`opacity < 1` raises the blend flag, `thinWalled` selects `DoubleSide`) — structural consequences of carriage, and no other computed value exists in the fold.
- Law: color triples are linear-space carriage — `Color.setRGB(r, g, b, LinearSRGBColorSpace)` ingests under three's `ColorManagement`, the renderer's `outputColorSpace` owns display transform, and a THEME color reaching the scene (selection tint, highlight) crosses through `Theme.linear` (`system/token`'s OKLCH → srgb-linear projection) into the same `setRGB` seam — one color contract, drift structurally impossible; no gamma math exists in this module.
- Law: `AppearanceSummary` is the preload census — read BEFORE any bind so the scene resolves every `Material.groups` reference once into the interior `HashMap<ContentKey, PbrGroups>`; a dangling reference is upstream evidence surfaced as-is, never a silent default material. The override is keyed — a `Material` row targets a mesh content key and the bind applies ONLY to targeted meshes; GLB-embedded materials on untargeted meshes ride untouched, so appearance is an overlay, never a repaint. Rebinding is idempotent pure assignment driven by the same keyed ledger the graft holds.
- Law: the WebGPU path swaps the material class, never the fold — `MeshPhysicalNodeMaterial` (`three/webgpu`) carries the same lobe fields, so the assignments land unchanged; TSL node-graph authoring is reached only where a lobe becomes a computed node (a probe-driven debug view), and such a graph is render-side presentation, never OpenPBR algebra.
- Growth: a new OpenPBR block (coat, sheen, iridescence, anisotropy — `MeshPhysicalMaterial` already carries the target lobes) is one wire mirror field wave plus one assignment arm here, landed in the same wave as the C# projection change — the fold signature never changes and TS never emits a block ahead of the wire.

```typescript
import type { Material as MaterialRow, PbrGroups } from "@rasm/ts/core"
import { HashMap, Option } from "effect"
import { DoubleSide, FrontSide, LinearSRGBColorSpace, type Color, type MeshPhysicalMaterial } from "three"

declare namespace Pbr {
  type Bound = { readonly material: MaterialRow; readonly groups: PbrGroups }
  type Index = HashMap.HashMap<ContentKey, PbrGroups>
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

const _index = (groups: ReadonlyArray<PbrGroups>): Pbr.Index =>
  groups.reduce((held, row) => HashMap.set(held, row.key, row), HashMap.empty<ContentKey, PbrGroups>())

const _resolve = (index: Pbr.Index, material: MaterialRow): Option.Option<Pbr.Bound> =>
  Option.map(HashMap.get(index, material.groups), (groups) => ({ material, groups }))

const Pbr: {
  readonly bind: typeof _bind
  readonly index: typeof _index
  readonly resolve: typeof _resolve
} = { bind: _bind, index: _index, resolve: _resolve }
```

## [8]-[INSTANCED_ROWS]

[INSTANCED_ROWS]:
- Owner: `Instanced` — the georeferenced instancing rows over `@deck.gl/mesh-layers`: `Instanced.mesh` places ONE arbitrary mesh at N element anchors through `SimpleMeshLayer`, `Instanced.scene` places a COMPLETE glTF scenegraph through `ScenegraphLayer` with per-node animation under the shared atom clock; both ride the one instance-transform axis — `getPosition` anchor plus the `getOrientation`/`getScale`/`getTranslation` triple — and every row is a declarative deck layer VALUE `geo`'s one `setProps` sink consumes; this module renders nothing itself.
- Packages: `@deck.gl/mesh-layers` (`SimpleMeshLayer`, `ScenegraphLayer`); `@deck.gl/core` (`Position`, `Color`, the accessor plane).
- Law: the transform family is one shared axis — `getTransformMatrix` OVERRIDES the orientation/scale/translation triple; a row supplies the matrix OR the triple, never both, and a third transform vocabulary beside the shared axis is the named defect.
- Law: an anchor row is decoded material — element positions arrive from the decoded geo plane keyed by `GlobalId`, so an instance pick's `PickingInfo.index` resolves through `mark`'s pipes into the one selection set; `pickable` is the row's toggle.
- Law: scenegraph animation rides `_animations` under the overlay's `_animate` flag driven by the same rAF-fed atom clock as `geo`'s trips row — one animation clock across the geo surface, never a second timer.
- Law: this pair is the `Tile3DLayer` mesh peer — `geo`'s 3D-tiles row renders b3dm/glTF tile content THROUGH these classes, and `ScenegraphLayer.onFirstDraw` is the transition-safety signal that row reads.
- Growth: a new instanced-asset need selects one of the two payload rows over the shared axis — never a third prop vocabulary.

```typescript
import type { Color, Position } from "@deck.gl/core"
import { ScenegraphLayer, SimpleMeshLayer } from "@deck.gl/mesh-layers"

declare namespace Instanced {
  type Anchor = {
    readonly id: string
    readonly position: Position
    readonly yaw: number
    readonly scale: readonly [number, number, number]
    readonly tint: Color
  }
}

const _mesh = (id: string, mesh: SimpleMeshLayer["props"]["mesh"], anchors: ReadonlyArray<Instanced.Anchor>) =>
  new SimpleMeshLayer<Instanced.Anchor>({
    id,
    data: anchors,
    mesh,
    pickable: true,
    getPosition: (row) => row.position,
    getOrientation: (row) => [0, row.yaw, 0],
    getScale: (row) => [row.scale[0], row.scale[1], row.scale[2]],
    getColor: (row) => row.tint,
  })

const _scene = (id: string, scenegraph: string, anchors: ReadonlyArray<Instanced.Anchor>) =>
  new ScenegraphLayer<Instanced.Anchor>({
    id,
    data: anchors,
    scenegraph,
    pickable: true,
    _lighting: "pbr",
    _animations: { "*": { speed: 1 } },
    getPosition: (row) => row.position,
    getOrientation: (row) => [0, row.yaw, 0],
    sizeScale: 1,
  })

const Instanced: {
  readonly mesh: typeof _mesh
  readonly scene: typeof _scene
} = { mesh: _mesh, scene: _scene }
```

## [9]-[EMBED_ROW]

[EMBED_ROW]:
- Owner: the `model-viewer` backend row — the zero-GL-handle embed: `.src` takes a `model/gltf-binary` object URL minted over a port arrival's bytes, `camera-controls` enables orbit, and the element owns decode, upload, camera, and dispose internally; decoder statics (`dracoDecoderLocation`, `ktx2TranscoderLocation`, `meshoptDecoderLocation`) pin to self-hosted paths BEFORE the first model or the element side-loads from a foreign CDN — a CSP breach. The animation surface is native: `play`/`pause` with `PlayAnimationOptions`, `appendAnimation` for additive blend, `availableAnimations` as the clip census — the embed arm's mirror of `[5]`'s mixer family.
- Packages: `@google/model-viewer` (`ModelViewerElement` — the const IS the statics owner and the instance type).
- Law: the element and the three arm share ONE physical `three` module (peer-deduped) but never a renderer, canvas, or GL context — sibling backends the backend literal selects per viewport.
- Law: the object URL lifecycle is bracketed — `URL.createObjectURL` acquires, `URL.revokeObjectURL` releases with the viewport scope; a leaked object URL pins the blob.
- Boundary: camera read/write on the element (`getCameraOrbit`/`getCameraTarget`/`jumpCameraToGoal`, the `camera-change` event) is `geo#CAMERA`'s adapter row; hotspot ray-casting (`positionAndNormalFromPoint`, `updateHotspot`) is `mark`'s anchor row.

```typescript
import { ModelViewerElement } from "@google/model-viewer"

const _pinned: Effect.Effect<void> = Effect.sync(() => {
  ModelViewerElement.dracoDecoderLocation = "<self-hosted>/draco/"
  ModelViewerElement.ktx2TranscoderLocation = "<self-hosted>/basis/"
  ModelViewerElement.meshoptDecoderLocation = "<self-hosted>/meshopt/"
})

const _embed = (element: ModelViewerElement, octets: Uint8Array) =>
  Effect.acquireRelease(
    Effect.sync(() => {
      const url = URL.createObjectURL(new Blob([octets], { type: "model/gltf-binary" }))
      element.src = url
      return url
    }),
    (url) => Effect.sync(() => URL.revokeObjectURL(url)),
  )

const Glb: {
  readonly Backend: typeof _Backend
  readonly backends: typeof _backends
  readonly output: typeof _OUTPUT
  readonly rig: typeof _RIG
  readonly lit: typeof _lit
  readonly renderer: typeof _renderer
  readonly graft: typeof _graft
  readonly loop: typeof _loop
  readonly instanced: typeof _instanced
  readonly batched: typeof _batched
  readonly merged: typeof _merged
  readonly pinned: typeof _pinned
  readonly embed: typeof _embed
} = {
  Backend: _Backend,
  backends: _backends,
  output: _OUTPUT,
  rig: _RIG,
  lit: _lit,
  renderer: _renderer,
  graft: _graft,
  loop: _loop,
  instanced: _instanced,
  batched: _batched,
  merged: _merged,
  pinned: _pinned,
  embed: _embed,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Glb, GlbFault, GlbViewport, Instanced, Pbr }
```
