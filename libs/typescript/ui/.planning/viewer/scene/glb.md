# [UI_GLB]

`viewer/scene/glb.ts` is the GLB_VIEWPORT owner: content-key-keyed mesh residency (the `Residency.Manifest`/`Residency.Delta` vocabulary decoded at `wire`, AU:63) rendered through one of two backends behind one `Schema.Literal` discriminant — the imperative `three` scene (full control: custom materials, picking, compute, receipts) or the declarative `<model-viewer>` embed (zero GL handle) — with every mesh byte arriving through the `GlbViewport` decode-worker port `ui` declares and `browser` provides at app composition. GPU resources are `Scope`-bracketed without exception, the frame loop parks under a hidden `<Activity>`, faults ride one `GlbFault` family over the closed `HopReason` vocabulary, and meshopt decode stays gated `[R23]`.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]         | [OWNS]                                                                     |
| :-----: | :---------------- | :--------------------------------------------------------------------------- |
|   [1]   | `VIEWPORT_PORT`   | the `GlbViewport` port record — decode-worker residency as a capability Tag   |
|   [2]   | `FAULT_FAMILY`    | `GlbFault` — the closed `HopReason` vocabulary and its policy table           |
|   [3]   | `BACKEND_SELECT`  | the `RendererBackend` literal, the WebGPU probe, and renderer acquisition     |
|   [4]   | `RESIDENCY_GRAFT` | the one-`Scene` graft/dispose fold over deltas and the codec-injected loader  |
|   [5]   | `EMBED_ROW`       | the `<model-viewer>` read-only backend row                                    |

## [2]-[VIEWPORT_PORT]

- Owner: `GlbViewport` — the runtime-capability port record this folder declares and NEVER implements: `manifest` (the current `Residency.Manifest` as an atom-bridgeable `Subscribable`), `deltas` (the `Residency.Delta` feed as a `Stream`), and `bytes(mesh)` (verified GLB octets per content key — the decode worker already reassembled frames and verified the `ContentKey` against the kernel mint, so bytes arriving here are proof-carrying). `browser/transport/pool` provides the Layer at app composition; `ui` and `browser` never import each other.
- Packages: `effect` (`Context.Tag`, `Stream`, `Subscribable`), `@rasm/ts/kernel` (`ContentKey`), `#vocab` (`Residency` — the manifest/delta/state vocabulary decoded once at `wire`).
- Law: the port is the ONLY byte ingress — a fetch, a worker message, or an `ArrayBuffer` reaching this module by any other path bypasses content-key verification and is the named defect.
- Law: `bytes` yields whole-buffer octets — the worker reassembles into a fresh buffer with `byteOffset` zero, so `parseAsync(octets.buffer, "")` is exact by contract; a sliced view smuggling a neighbor's bytes is the port implementation's defect, never the graft's guard.
- Law: port shape is sized for the family — prefetch hints, eviction acknowledgement, and priority lanes land as members HERE when earned; consumers already hold the Tag, so growth is a member row, never a second port.
- Law: the wire classes reach this module as derived types — `Schema.Schema.Type<typeof Residency.Manifest>` computes the decoded instance type off the `#vocab` value, so no parallel manifest shape is ever declared here.
- Boundary: worker mechanics, frame reassembly, and the content-key delegate are `browser`'s; the manifest wire decode is `wire/frame/residency`'s.

```typescript
import type { ContentKey } from "@rasm/ts/kernel"
import type { Residency } from "#vocab"
import { Context, type Effect, type Schema, type Stream, type Subscribable } from "effect"

type _Manifest = Schema.Schema.Type<typeof Residency.Manifest>
type _Delta = Schema.Schema.Type<typeof Residency.Delta>

class GlbViewport extends Context.Tag("ui/GlbViewport")<GlbViewport, {
  readonly manifest: Subscribable.Subscribable<_Manifest>
  readonly deltas: Stream.Stream<_Delta, GlbFault>
  readonly bytes: (mesh: ContentKey) => Effect.Effect<Uint8Array, GlbFault>
}>() {}
```

## [3]-[FAULT_FAMILY]

- Owner: `GlbFault` — one `Data.TaggedError` sized by routing with a closed `reason` vocabulary (`HopReason`): `manifest-skew` (delta references a mesh the manifest never named), `key-mismatch` (worker verification refused the octets), `decode-refused` (loader/codec rejected the payload), `codec-absent` (asset demands a codec the viewport did not wire — the meshopt gate's refusal spelling), `backend-lost` (GPU device loss); the policy table carries `rank`/`retry`/`evict` per reason and the class getter projects it, so recovery reads policy rows, never re-derives them per arm. The salvage corpus's code-keyed `HopFault` construction is the named discard — reasons are a closed vocabulary, not free strings.
- Law: `backend-lost` is the re-init row — `GPUDevice.lost` resolves into a fault whose recovery re-runs backend selection under the same `Scope`; residency state survives because the manifest fold is renderer-independent.
- Law: a fault no consumer arm can act on escalates — a torn invariant inside the graft fold dies (`Effect.die`), keeping the channel total over actionable faults.

```typescript
import { Data } from "effect"

const HopReason = {
  "manifest-skew": { rank: 3, retry: false, evict: true },
  "key-mismatch": { rank: 5, retry: false, evict: true },
  "decode-refused": { rank: 4, retry: false, evict: true },
  "codec-absent": { rank: 2, retry: false, evict: false },
  "backend-lost": { rank: 4, retry: true, evict: false },
} as const

declare namespace HopReason {
  type Kind = keyof typeof HopReason
  type Row = (typeof HopReason)[Kind]
}

class GlbFault extends Data.TaggedError("GlbFault")<{
  readonly reason: HopReason.Kind
  readonly mesh: string
  readonly detail: string
}> {
  get policy(): HopReason.Row {
    return HopReason[this.reason]
  }
}
```

## [4]-[BACKEND_SELECT]

- Owner: `Glb.Backend` — the closed `RendererBackend` vocabulary (`three` | `model-viewer`) spread from one `as const` tuple into `Schema.Literal`; the `three` arm acquires a renderer under `Effect.acquireRelease`: probe `navigator.gpu`, construct `WebGPURenderer` and `await init()` when present (feature-gating through `renderer.hasFeature` post-init), else `WebGLRenderer`; output policy (`outputColorSpace`, `toneMapping`, exposure) stamps at construction; release disposes — three has no GPU garbage collection, so the finalizer IS memory correctness.
- Law: one usage contract over both renderer backends — scene, camera, and loop code are backend-agnostic after construction; the WebGPU upgrade is a construction-site swap, never a scene rewrite.
- Law: the loop parks under `<Activity>` hidden — `setAnimationLoop(null)` on hide, re-arm on visible (`act/transition`'s activity row consumed as settled); a loop burning under a hidden viewport is the named defect.
- Law: `@webgpu/types` is a tsconfig `types` entry of the viewer project only — ambient globals, never an import.
- Boundary: camera drive is `viewer/geo/project`'s; receipt readback is `viewer/probe/receipt`'s; the `<canvas>` element arrives as a parameter from the app shell.

```typescript
import { Effect, Schema } from "effect"
import { ACESFilmicToneMapping, SRGBColorSpace, WebGLRenderer } from "three"
import { WebGPURenderer } from "three/webgpu"

const _backends = ["three", "model-viewer"] as const

const _Backend = Schema.Literal(..._backends)

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
          })
      built.outputColorSpace = SRGBColorSpace
      built.toneMapping = ACESFilmicToneMapping
      return built
    }),
    (renderer) => Effect.sync(() => renderer.dispose()),
  )
```

## [5]-[RESIDENCY_GRAFT]

- Owner: `Glb.graft` — the residency fold: one `Scene` roots the graph, an interior ledger (`HashMap<ContentKey, Object3D>`) tracks grafted subtrees, and the `deltas` stream drives the fold — an arriving mesh pulls `bytes(mesh)` through the port, parses via the ONE codec-injected `GLTFLoader` (`parseAsync` over the verified buffer), grafts `gltf.scene` under the root keyed by content key; an evicting delta removes the subtree AND walks it through `_dispose` — the disposal kernel narrowing each visited node to `Mesh` and releasing geometry and material handles — before the ledger drops the key; three's `traverse` callback is the kernel's platform-forced statement seam and no reference escapes it.
- Law: codec injection is capability wiring — `setDRACOLoader`/`setKTX2Loader` attach at loader construction with transcoder paths pinned self-hosted (their wasm runs behind the `browser` worker, never the render thread); `setMeshoptDecoder` attaches ONLY when the asset flags `EXT_meshopt_compression` and the `[R23]` gate has admitted a decoder identity — until then such an asset refuses with `codec-absent`.
- Law: environment is a prefilter fact — one `PMREMGenerator.fromScene(RoomEnvironment())` bake serves `scene.environment`; per-frame IBL work is the named defect.
- Law: draw-call collapse is a graft-time fold — same-material submeshes merge through `BufferGeometryUtils.mergeGeometries`; repeated element geometry rides `InstancedMesh`/`BatchedMesh` rows when the manifest marks repetition.
- Law: `preload`/`preinit` hints warm decoder wasm and imminent GLB fetches ahead of first frame (`react-dom` hint family) — issued from the manifest census, not per-mesh at draw time.
- Growth: a new residency policy (priority lanes, partial LOD) is a fold arm over new delta rows minted at `wire` — the graft signature never changes.

```typescript
import type { ContentKey } from "@rasm/ts/kernel"
import { Effect, HashMap, Option, Ref, Stream } from "effect"
import { Mesh, Scene } from "three"
import type { Object3D } from "three"
import { GLTFLoader } from "three/addons/loaders/GLTFLoader.js"

declare namespace Glb {
  type Backend = Schema.Schema.Type<typeof _Backend>
  type Ledger = HashMap.HashMap<ContentKey, Object3D>
}

const _dispose = (node: Object3D): void => {
  node.traverse((child) => {
    if (child instanceof Mesh) {
      child.geometry.dispose()
      const materials = Array.isArray(child.material) ? child.material : [child.material]
      materials.forEach((material) => material.dispose())
    }
  })
}

const _graft = (root: Scene, loader: GLTFLoader, port: Context.Tag.Service<GlbViewport>) =>
  Effect.gen(function* () {
    const ledger = yield* Ref.make(HashMap.empty<ContentKey, Object3D>())
    yield* Stream.runForEach(port.deltas, (delta) =>
      delta.state === "evicted"
        ? Effect.gen(function* () {
            const held = yield* Ref.get(ledger)
            yield* Option.match(HashMap.get(held, delta.mesh), {
              onNone: () => Effect.void,
              onSome: (node) =>
                Effect.zipRight(
                  Effect.sync(() => {
                    root.remove(node)
                    _dispose(node)
                  }),
                  Ref.update(ledger, HashMap.remove(delta.mesh)),
                ),
            })
          })
        : Effect.gen(function* () {
            const octets = yield* port.bytes(delta.mesh)
            const gltf = yield* Effect.tryPromise({
              try: () => loader.parseAsync(octets.buffer, ""),
              catch: (defect) => new GlbFault({ reason: "decode-refused", mesh: `${delta.mesh}`, detail: String(defect) }),
            })
            yield* Effect.sync(() => root.add(gltf.scene))
            yield* Ref.update(ledger, HashMap.set(delta.mesh, gltf.scene))
          }))
  })
```

## [6]-[EMBED_ROW]

- Law: the `model-viewer` backend is the zero-GL-handle embed — `.src` takes a `model/gltf-binary` object URL minted over the port's bytes, `camera-controls` enables orbit, and the element owns decode/upload/dispose internally; decoder statics (`dracoDecoderLocation`, `ktx2TranscoderLocation`, `meshoptDecoderLocation`) pin to self-hosted paths BEFORE the first model or the element side-loads from the Google CDN — a CSP breach.
- Law: the element and the three row share ONE physical `three` module (peer-deduped) but never a renderer, canvas, or GL context — they are sibling backends the `Glb.Backend` literal selects per viewport.
- Law: the object URL lifecycle is bracketed — `URL.createObjectURL` acquires, `URL.revokeObjectURL` releases with the viewport scope; a leaked object URL pins the blob.
- Boundary: camera read/write on the element (`getCameraOrbit`/`SphericalPosition`, `camera-change`) is `viewer/geo/project`'s adapter row; hotspot ray-casting is `viewer/mark/bcf`'s anchor row.

```typescript
const Glb: {
  readonly Backend: typeof _Backend
  readonly backends: typeof _backends
  readonly Fault: typeof GlbFault
  readonly reasons: typeof HopReason
  readonly renderer: typeof _renderer
  readonly graft: typeof _graft
} = {
  Backend: _Backend,
  backends: _backends,
  Fault: GlbFault,
  reasons: HopReason,
  renderer: _renderer,
  graft: _graft,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Glb, GlbViewport }
```
