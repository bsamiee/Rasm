# [UI_SCENE]

Glb owns content-keyed scene residency behind a `three` or `<model-viewer>` backend discriminant. One `GlbViewport` port supplies verified bytes and ledger facts; one renderer acquisition shares its `GPUDevice` and seats the one prefilter; one codec record seats the one transcoder; one graft ledger owns subtrees, mixers, GPU residency, and typed evidence. One environment dome rides the same port — container-sniffed HDR, Radiance, and deep-KTX2 octets folded through ONE commit that decodes and prefilters on a key change, re-reads policy on a repeat, and re-derives its target on backend loss, with nine-band irradiance carried verbatim behind one wire-frame read seam. OpenPBR binding carries the C# algebra verbatim, and georeferenced instances remain deck layer values. Scoped GPU resources, a parked hidden loop, one lifecycle machine, and one `GlbFault` family close the plane. Module: `ui/viewer/src/scene.ts`.

## [01]-[INDEX]

- [02]-[VIEWPORT_PORT]: the `GlbViewport` port — residency ledger read + verified-arrival ingress as a Tag; `GlbViewport`.
- [03]-[FAULT_FAMILY]: `GlbFault` — the closed reason vocabulary and its policy table; `GlbFault`.
- [04]-[BACKEND_SELECT]: the backend literal, renderer + prefilter acquisition, output policy, light rig; `Glb`.
- [05]-[RESIDENCY_GRAFT]: the asset-identity roster, the one codec record, and the graft/upload/release fold; `Glb`.
- [06]-[ENVIRONMENT_FOLD]: the container-sniffed dome fold — one commit, keyed handles, the SH9 read seam; `Glb`.
- [07]-[DRAW_COLLAPSE]: merge/instanced/batched rows — repeated element geometry as bounded draw calls; `Glb`.
- [08]-[APPEARANCE_BIND]: the OpenPBR lobe assignment, color contract, census resolve, node-material mirror; `Pbr`.
- [09]-[INSTANCED_ROWS]: `@deck.gl/mesh-layers` — georeferenced element instances as deck layer values; `Instanced`.
- [10]-[EMBED_ROW]: the `<model-viewer>` backend row — decoder statics, object-URL bracket, animation; `Glb`.

## [02]-[VIEWPORT_PORT]

[VIEWPORT_PORT]:
- Owner: `GlbViewport` — the runtime-capability port this folder declares and NEVER implements: `ledger` is the residency read (`core/interchange/frame`'s `Residency.Ledger` published `Subscribable`, the same cell `Depot` owns), and `arrivals` is the verified-octet ingress — each element pairs a `ContentKey` with whole-buffer GLB bytes the hauling side already reassembled and re-proved through the one `Parity` delegate, so bytes reaching this module are proof-carrying by construction. `environments` is the dome ingress on the same proof rail — an `Arrival` widened with the read policy its decoded set manifest carries (`intensity`, `rotation` in radians about the producer frame's `+Z`, and `sh9`, the producer's irradiance projection as 27 band-major RGB-interleaved values). App composition satisfies the Tag by forwarding `Depot.haul` arrival pairs and the `Depot` ledger cell.
- Packages: `effect` (`Context.Tag`, `Stream`, `Subscribable`); `@rasm/ts/core` (`ContentKey`, `Residency`).
- Law: the port is the ONLY byte ingress — a fetch, a worker message, or an `ArrayBuffer` reaching this module by any other path bypasses content-key verification and is the named defect.
- Law: arrival octets are whole-buffer and the TYPE carries it — `Uint8Array<ArrayBuffer>` fixes a non-shared backing store, so `octets.buffer` satisfies the `ArrayBuffer` parameter every decode entry declares (`parseAsync`, `createDataTexture`, `KTX2Loader.parse`) at all three sites with no narrowing guard, while the bare `Uint8Array` widens its buffer to `ArrayBufferLike` and refuses each of them; the hauling side reassembles at `byteOffset` zero, and a sliced view smuggling a neighbor's bytes is the provider's defect, never the graft's guard.
- Law: the wire shapes reach this module as the settled core vocabulary — `Residency.Ledger` rows and `ContentKey` compose directly; a parallel manifest shape, row twin, or local key notion is unspellable because the core owner is the only declaration.
- Growth: prefetch hints, eviction acknowledgement, and priority lanes land as members HERE when earned — consumers already hold the Tag, so growth is a member row, never a second port; `environments` is that law realized.
- Boundary: haul scheduling, cache warmth, worker verify, and byte budgets are `runtime/browser/fetch`'s; the residency protocol and its fold law are `core/interchange/frame`'s. WHICH set manifest named a dome — the C#-baked set document or the python-assembled one — is composition's concern behind the decoded core landings; the port carries bytes and read policy alone.

```typescript signature
import type { ContentKey, Residency } from "@rasm/ts/core"
import { Context, type Stream, type Subscribable } from "effect"

declare namespace GlbViewport {
  // generic arguments carry the whole-buffer contract: a shared backing store reaches no `three` decode entry
  type Arrival = { readonly key: ContentKey; readonly octets: Uint8Array<ArrayBuffer> }
  type Environment = Arrival & {
    readonly intensity: number
    readonly rotation: number
    readonly sh9: ReadonlyArray<number>
  }
}

class GlbViewport extends Context.Tag("ui/viewer/GlbViewport")<GlbViewport, {
  readonly ledger: Subscribable.Subscribable<Residency.Ledger>
  readonly arrivals: Stream.Stream<GlbViewport.Arrival, GlbFault>
  readonly environments: Stream.Stream<GlbViewport.Environment, GlbFault>
}>() {}
```

## [03]-[FAULT_FAMILY]

[FAULT_FAMILY]:
- Owner: `GlbFault` — one `Data.TaggedError` sized by routing with a closed `reason` vocabulary: `manifest-skew` (an arrival references a mesh the ledger never named), `key-mismatch` (verification refused the octets at the port boundary), `decode-refused` (loader or codec rejected the payload — the GLB parse and the dome container/decode legs share it), `codec-absent` (the asset demands a codec the viewport did not wire — the meshopt gate's refusal spelling), `backend-lost` (GPU device loss). Its policy table carries `rank`/`retry`/`evict` per reason and the class getter projects it, so recovery reads policy rows, never re-derives them per arm, while `GlbFault.roster` publishes the same vocabulary as the ordered non-empty tuple the refusal instrument's word census pre-registers — the guard pair closes the two anchors against each other, so a reason landing in one alone refuses at the declaration. Code-keyed free-string fault construction is the named discard — reasons are a closed vocabulary.
- Packages: `effect` (`Data`).
- Law: `backend-lost` is the re-init row — `GPUDevice.lost` resolves into a fault whose recovery re-runs backend selection under the same `Scope`; residency state survives because the graft ledger is renderer-independent.
- Law: a fault no consumer arm can act on escalates — a torn invariant inside the graft fold dies through `Effect.die`, keeping the channel total over actionable faults.
- Law: the reason table is scene-local vocabulary — the transport-altitude `Hops` table is `core/interchange/codec`'s and never restates here; a scene reason names a residency/render condition, never a wire condition.
- Growth: a new failure condition (a compute-pass refusal, an embed mount loss) is one reason row with its policy columns — every policy consumer inherits it through the getter.

```typescript signature
import { Data } from "effect"

const _reasons = {
  "manifest-skew": { rank: 3, retry: false, evict: true },
  "key-mismatch": { rank: 5, retry: false, evict: true },
  "decode-refused": { rank: 4, retry: false, evict: true },
  "codec-absent": { rank: 2, retry: false, evict: false },
  "backend-lost": { rank: 4, retry: true, evict: false },
} as const

// Ordered non-empty roster beside the policy table: the word census the refusal instrument pre-registers takes a
// proven-non-empty tuple, and a key walk over the table returns a plain array no such tuple admits. The guard pair
// closes membership both directions, so a reason landing in one anchor alone refuses here.
const _ROSTER = ["manifest-skew", "key-mismatch", "decode-refused", "codec-absent", "backend-lost"] as const

declare namespace GlbFault {
  type Reason = keyof typeof _reasons
  type Row = (typeof _reasons)[Reason]
  type _Roster<Missing extends never = Exclude<Reason, (typeof _ROSTER)[number]>, Excess extends never = Exclude<(typeof _ROSTER)[number], Reason>> = [Missing, Excess]
}

class GlbFault extends Data.TaggedError("GlbFault")<{
  readonly reason: GlbFault.Reason
  readonly mesh: string
  readonly detail: string
}> {
  static readonly reasons: typeof _reasons = _reasons
  static readonly roster: typeof _ROSTER = _ROSTER
  get policy(): GlbFault.Row {
    return _reasons[this.reason]
  }
}
```

## [04]-[BACKEND_SELECT]

[BACKEND_SELECT]:
- Owner: the closed backend vocabulary (`three` | `model-viewer`) spread from one `as const` tuple into `Schema.Literal`, and the `three` arm's renderer acquisition under `Effect.acquireRelease`: probe `navigator.gpu`, acquire the ONE `GPUDevice` (`requestAdapter` → `requestDevice`), construct `WebGPURenderer({ canvas, device })` and await `init()` (feature-gating through `renderer.hasFeature` post-init), else `WebGLRenderer` — a refused acquisition on a present-but-broken adapter degrades to the same WebGL floor, so `backend-lost` stays reserved for a lost LIVE device and the probe never faults the channel; the acquisition returns ONE `Glb.Acquired` record — `device` as an `Option`, the published seam the compute lane, the probe hash kernel, and any `tgpu.initFromDevice` adopter share, one device, one memory space, zero readback round-trips; `prefilter` is the ONE `Glb.Prefilter` — the backend-matched PMREM class behind one structural seam — that `[6]`'s dome fold consumes and `Glb.Loop.rebind` re-derives against, released before its renderer; `renderer` is what `[5]` wires into `KTX2Loader.detectSupport` and every eager texture upload, so the codec transcode target, the residency walk, and the dome's own source upload read the same live backend; the output policy row (`outputColorSpace`, tone map selected from the `_TONE` vocabulary, `toneMappingExposure`) stamps at construction; release disposes — three has no GPU garbage collection, so the finalizer IS memory correctness.
- Packages: `three` (`WebGLRenderer`, `PMREMGenerator`, `SRGBColorSpace`, `ACESFilmicToneMapping`/`AgXToneMapping`/`NeutralToneMapping`, the light classes incl. `RectAreaLight`/`LightProbe`); `three/webgpu` (`WebGPURenderer`, `PMREMGenerator` — the unified-renderer prefilter class); `effect` (`Effect`, `Option`, `Schema`); `@webgpu/types` (ambient, a viewer-tsconfig `types` entry, never an import).
- Law: one usage contract over both renderer backends — scene, camera, and loop code are backend-agnostic after construction; the WebGPU upgrade is a construction-site swap, never a scene rewrite; the `scope:viewer` tier itself is `lazy(() => import(…))` behind `Suspense` so the non-spatial majority never downloads three.
- Law: tone mapping is a vocabulary row, never a constant — `_TONE` carries `aces`/`agx`/`neutral`, the output policy names one, and a per-scene override is one policy value; hardcoding a tone-map enum at a construction site is the named defect.
- Law: lighting is a rig of rows — the analytic light vocabulary (`ambient`, `hemisphere`, `directional`, `rect`, `probe`) is one `as const` table materialized by one fold; the image-based half is `[6]`'s dome fold — `fromScene(RoomEnvironment())` as the floor, the content-keyed equirect dome on arrival; `rect` is the analytic area source for interior shots, `probe` the analytic low-frequency seat the rig holds before any dome; a hand-placed light outside the rig table, or per-frame IBL work, is the named defect.
- Law: the compute lane is WebGPU-gated and altitude-split — scene-resident kernels (per-instance culling, skinning) are `three/tsl` node graphs over `StorageBufferNode`/`instancedArray` dispatched through `computeAsync`, driving `[7]`'s `setVisibleAt` rows; compute WITHOUT a scene consumer (probe hashing, mark lasso folds) adopts the published device through `tgpu.initFromDevice({ device })` with `root.unwrap(buffer)` at the seam — two altitudes of one concern split by scene residency, never two engines on one kernel; the WebGL arm renders the same scene without the pass, and a compute result feeding appearance is `[8]`'s debug-view boundary, never OpenPBR algebra.
- Law: the backend lifecycle is a statechart, not scattered arms — boot → ready → degraded (WebGL floor) → backend-lost → re-init is a serializable `Machine` whose actor state binds through `system/atom#LIVE_BRIDGE`'s `Atom.subscribable` row and snapshots across remounts; the `GlbFault` policy table supplies the transition guards, and residency state survives re-init because the graft ledger is renderer-independent. Domes are NOT — a prefiltered target dies with the renderer that backed it — so the re-init arm drives `Glb.Loop.rebind` with the fresh acquisition and the dome re-derives from the decoded source it still holds; a re-init leaving the dome un-rebound serves the scene a disposed environment texture.
- Law: the loop parks under `<Activity>` hidden — `setAnimationLoop(null)` on hide, re-arm on visible (`system/act#DOCUMENT_RAIL` consumed as settled); a loop burning under a hidden viewport is the named defect.
- Boundary: camera drive is `geo`'s; receipt readback and `compileAsync` settle discipline are `probe`'s; the `<canvas>` element arrives as a parameter from the app shell.

```typescript signature
import { Effect, Option, Schema, type Scope } from "effect"
import {
  ACESFilmicToneMapping, AgXToneMapping, AmbientLight, DirectionalLight, HemisphereLight, LightProbe,
  NeutralToneMapping, PMREMGenerator as GlPrefilter, RectAreaLight, SRGBColorSpace, WebGLRenderer,
} from "three"
import type { Light, Scene, Texture } from "three"
import { PMREMGenerator as GpuPrefilter, WebGPURenderer } from "three/webgpu"

declare namespace Glb {
  // one prefilter concept, two backend classes under one exported name — the aliased imports are language
  // disambiguation, this structural seam is the published shape both classes satisfy; the compile member is the
  // one column where they diverge (core returns void, the unified class a promise), so the seam declares both
  type Prefilter = {
    fromScene(scene: Scene, sigma?: number): { readonly texture: Texture; dispose(): void }
    fromEquirectangular(equirect: Texture): { readonly texture: Texture; dispose(): void }
    compileEquirectangularShader(): void | Promise<void>
    dispose(): void
  }
  // one acquisition record serves every downstream lane — backend, device seam, and prefilter all read off
  // this shape, so no lane re-probes `navigator.gpu` and no second prefilter is ever constructed
  type Acquired = {
    readonly renderer: WebGLRenderer | WebGPURenderer
    readonly device: Option.Option<GPUDevice>
    readonly prefilter: Glb.Prefilter
  }
}

const _backends = ["three", "model-viewer"] as const

const _Backend = Schema.Literal(..._backends)

const _TONE = { aces: ACESFilmicToneMapping, agx: AgXToneMapping, neutral: NeutralToneMapping } as const

const _OUTPUT = { colorSpace: SRGBColorSpace, tone: "agx", exposure: 1 } as const satisfies {
  colorSpace: typeof SRGBColorSpace
  tone: keyof typeof _TONE
  exposure: number
}

const _RIG = {
  ambient: { color: 0xffffff, intensity: 0.3 },
  hemisphere: { sky: 0xffffff, ground: 0x444444, intensity: 0.6 },
  directional: { color: 0xffffff, intensity: 1.2, position: [4, 8, 4] },
  rect: { color: 0xffffff, intensity: 2.4, width: 4, height: 2 },
  probe: { intensity: 1 },
} as const

const _lit = (root: Scene): Scene => {
  // BOUNDARY ADAPTER
  const sun = new DirectionalLight(_RIG.directional.color, _RIG.directional.intensity)
  sun.position.set(_RIG.directional.position[0], _RIG.directional.position[1], _RIG.directional.position[2])
  const rows: ReadonlyArray<Light> = [
    new AmbientLight(_RIG.ambient.color, _RIG.ambient.intensity),
    new HemisphereLight(_RIG.hemisphere.sky, _RIG.hemisphere.ground, _RIG.hemisphere.intensity),
    sun,
    new RectAreaLight(_RIG.rect.color, _RIG.rect.intensity, _RIG.rect.width, _RIG.rect.height),
    new LightProbe(undefined, _RIG.probe.intensity),
  ]
  return rows.reduce((held, light) => held.add(light), root)
}

const _renderer = (canvas: HTMLCanvasElement): Effect.Effect<Glb.Acquired, never, Scope.Scope> =>
  Effect.acquireRelease(
    Effect.gen(function* () {
      const acquired = globalThis.navigator.gpu === undefined
        ? Option.none<GPUDevice>()
        : yield* Effect.tryPromise({
            try: async () => {
              const adapter = await globalThis.navigator.gpu.requestAdapter()
              return adapter === null ? Option.none<GPUDevice>() : Option.some(await adapter.requestDevice())
            },
            catch: () => new GlbFault({ reason: "backend-lost", mesh: "<boot>", detail: "<device-refused>" }),
          }).pipe(Effect.orElseSucceed(() => Option.none<GPUDevice>()))
      const built = yield* Option.match(acquired, {
        onNone: () => Effect.succeed(new WebGLRenderer({ canvas, antialias: true })),
        onSome: (device) =>
          Effect.tryPromise({
            try: async () => {
              const gpu = new WebGPURenderer({ canvas, antialias: true, device })
              await gpu.init()
              return gpu
            },
            catch: () => new GlbFault({ reason: "backend-lost", mesh: "<boot>", detail: "<webgpu-init>" }),
          }).pipe(Effect.orElseSucceed(() => new WebGLRenderer({ canvas, antialias: true }))),
      })
      built.outputColorSpace = _OUTPUT.colorSpace
      built.toneMapping = _TONE[_OUTPUT.tone]
      built.toneMappingExposure = _OUTPUT.exposure
      const prefilter: Glb.Prefilter = built instanceof WebGLRenderer ? new GlPrefilter(built) : new GpuPrefilter(built)
      return {
        renderer: built,
        device: built instanceof WebGLRenderer ? Option.none<GPUDevice>() : acquired,
        prefilter,
      } satisfies Glb.Acquired
    }),
    ({ prefilter, renderer }) => Effect.sync(() => {
      // BOUNDARY ADAPTER — the prefilter releases before the renderer that backs it
      prefilter.dispose()
      renderer.dispose()
    }),
  )
```

## [05]-[RESIDENCY_GRAFT]

[RESIDENCY_GRAFT]:
- Owner: `Glb.AssetRoster` — the encoded, slug-unique `{ slug, digest, file }` identity roster for every self-hosted decoder and wasm asset; `Glb.asset` resolves one row with `codec-absent` on absence, `Glb.assetPath` derives the immutable `assets/<digest>/<file>` file address, and `Glb.assetDir` the sibling `assets/<digest>/` directory form — one digest carrying every leaf of a multi-file asset, the trailing slash part of the spelling. `Glb.graft` owns the residency fold over the port: one `Scene` roots the graph, ONE `Ref`-held ledger (`HashMap<ContentKey, Glb.Graft>`) is the single truth for grafted subtrees AND their mixers, and the port's arrival stream enters one graft lane — the lane parses verified octets through `Glb.Codecs.gltf`, the ONE codec-injected `GLTFLoader` (`parseAsync` over the whole buffer), mints the animation half (a per-subtree `AnimationMixer` whose every `gltf.animations` clip binds through `clipAction(...).setLoop(LoopRepeat, Infinity).play()` — the loader result's animation array is retained, never discarded), uploads every decoded texture through the one residency walk, grafts `gltf.scene` under the root, and commits the ledger atomically. One bounded fact queue receives `Arrived` only after that commit, `Environment` from `[6]`'s dome commit, and `Refused` from the contained failure legs; `Glb.hook(loop)` adopts the queue stream for probe and telemetry taps, so one source feeds every consumer without parallel subscriptions or speculative arrival facts. Its eviction arm diffs the port ledger's `evicted` rows against held grafts, removes the subtree, tears the mixer down (`stopAllAction` then `uncacheRoot`) and walks it through the SAME residency walk under the release visitor before the ledger drops the key — both arms mutate ONLY the `Ref`, so the fold is race-free by construction. `Glb.Loop.advance(delta)` derives live mixers from the same ledger inside the marked frame-loop kernel — no second residency roster exists — while `Glb.Loop.dome` republishes `[6]`'s slot read so the irradiance seam reaches consumers without a second subscription and `Glb.Loop.rebind` hands the backend statechart's re-init arm the one entry that re-derives the dome against a fresh acquisition.
- Packages: `three` (`Scene`, `Mesh`, `Material`, `Texture`, `AnimationMixer`, `Clock`, `LoadingManager`, `LoopRepeat`); `three/addons` (`GLTFLoader` + its `GLTFParser`/`GLTFLoaderPlugin` extension seam, `DRACOLoader`, `KTX2Loader`, the `MeshoptDecoder` module shape); `effect` (`Effect`, `HashMap`, `Option`, `Queue`, `Ref`, `Schema`, `Stream`, `Scope`); `@rasm/ts/core` (`ContentKey`).
- Law: codec injection is capability wiring — the loaders construct over one `LoadingManager` whose `onProgress`/`onError` fold per-graft dependency progress into the residency telemetry tap; `setDRACOLoader`/`setKTX2Loader` attach at loader construction with `setDecoderPath`/`setTranscoderPath` handed the `Glb.assetDir` DIRECTORY form — each loader joins its own leaf names (`basis_transcoder.js` beside its wasm) onto the handed directory, so a multi-leaf decoder rides ONE digest exactly as the iac `_addressedAll` publish spells it — and `detectSupport(renderer)` reads the acquisition's live backend once, fixing the Basis transcode target before the first parse. Perspective wasm reads the same roster through its own slug, and no consumer accepts a free-form asset path.
- Law: ONE transcoder instance serves the viewport, which is why construction publishes `Glb.Codecs` rather than a bare loader — `KTX2Loader` owns a `WorkerPool` and its `parse` refuses outright until `detectSupport` fills the worker config, so the instance the GLB pipeline receives through `setKTX2Loader` is the same instance `[6]`'s deep-store container row decodes a `.ktx2` dome through; a second instance either doubles the transcoder wasm download and the worker set or throws on its first dome.
- Law: the meshopt leg is roster-gated at BOTH ends — `Glb.asset(roster, "meshopt")` resolves the served decoder module and `setMeshoptDecoder` attaches only on that hit, while one registered `GLTFLoaderPlugin` reads `extensionsRequired` off the parsed JSON chunk at `beforeRoot` and refuses `codec-absent` for an `EXT_meshopt_compression` asset the roster never admitted. Gating fires BEFORE any buffer decode, so an unserved decoder reads as the declared refusal instead of an opaque parse defect, and draco/ktx2 stay hard requirements while meshopt stays the arming row iac's serve completes.
- Law: the parser's JSON chunk crosses as unknown — `GLTFParser.json` is an untyped platform value, so the gate decodes it through one `Schema` before reading a field; a property read straight off the parser is the erased-type defect this decode forecloses.
- Law: ONE residency walk owns GPU handles in both directions — the traverse kernel visits each `Mesh` with its materials and two visitors ride it: upload runs `renderer.initTexture` over every decoded texture slot at graft time so the first sampling frame never hitches, release frees geometry, then the texture slots, then the materials, because `material.dispose()` frees the program and never its textures. Coverage cannot fork between the arms — a slot one visitor reaches the other reaches — and three's `traverse` callback is the kernel's platform-forced statement seam, marked on its first line, with no reference escaping it. Lazy first-use upload and a second private walk are the named defects.
- Law: the graft lane outlives any refusal — a per-arrival `GlbFault` (`decode-refused`, `codec-absent`) folds into the bounded fact queue and the lane keeps consuming; an arrival stream that dies on one bad mesh is the named defect, the policy table's `evict` column governs the ledger consequence, and success and refusal join only after their respective settlement edges. Containment is the signature's own truth — both residency lanes and the dome lane fork with their refusals caught, so construction carries NO error channel and the roster resolve's `codec-absent` is the one failure a composing root ever handles; an unraisable channel buys every caller a recovery arm nothing reaches.
- Law: the graft lane is woven — each arrival's graft effect carries `Effect.withSpan("rasm.ui.scene.residency")` with the content key and byte count as log-and-span material, successful grafts feed `1` through `Effect.withMetric` into `_GRAFTED`, and `_REFUSED` folds refusal reasons through `Metric.trackErrorWith` over the closed `GlbFault.Reason` vocabulary (bounded tags by construction — the key never a tag); `Glb.hook` adopts the one fact stream behind the observe point, so app and probe taps never wrap the fold.
- Law: `preload`/`preinit` hints warm decoder wasm and imminent GLB fetches ahead of first frame (`react-dom` hint family), issued from the ledger's `pending` census, never per-mesh at draw time.
- Exemption: the frame-loop tick is the platform-forced synchronous seam — `advance` reads the ledger through `Effect.runSync(Ref.get(held))` inside the marked kernel (a pure sync read, total by construction) and only immutable snapshots leave the fold.
- Growth: a new residency policy (priority lanes, partial LOD) is a fold arm over new ledger rows minted at `core/interchange/frame` — the graft signature never changes; a new animation policy (clip selection, cross-fade) is one action-policy row applied at mint; a new arrival consumer is one hook tap over the adopted fact stream, never a second port subscription; a new codec or wasm is one `Glb.AssetRoster` row consumed through `Glb.asset`, never a parallel identity or path surface.

```typescript signature
import { Convention, type ContentKey } from "@rasm/ts/core"
import { Context, Effect, HashMap, Metric, Option, Queue, Ref, Schema, Scope, Stream } from "effect"
import { AnimationMixer, Clock, LoadingManager, LoopRepeat, Mesh, Scene, Texture } from "three"
import type { Material, Object3D, PerspectiveCamera } from "three"
import type { MeshoptDecoder } from "three/addons/libs/meshopt_decoder.module.js"
import { DRACOLoader } from "three/addons/loaders/DRACOLoader.js"
import { GLTFLoader, type GLTFLoaderPlugin, type GLTFParser } from "three/addons/loaders/GLTFLoader.js"
import { KTX2Loader } from "three/addons/loaders/KTX2Loader.js"
import { Hook } from "../../src/system/hook.ts"

declare namespace Glb {
  type Backend = typeof _Backend.Type
  type AssetIdentity = typeof _AssetIdentity.Type
  type AssetIdentityWire = typeof _AssetIdentity.Encoded
  type AssetRoster = typeof _AssetRoster.Type
  type AssetRosterWire = typeof _AssetRoster.Encoded
  type Graft = { readonly node: Object3D; readonly mixer: Option.Option<AnimationMixer> }
  type Ledger = HashMap.HashMap<ContentKey, Glb.Graft>
  // one codec record publishes the transcoder beside the loader consuming it, because the dome's deep-store
  // container decodes through that same configured instance and a second one owns a second worker pool
  type Codecs = { readonly gltf: GLTFLoader; readonly ktx2: KTX2Loader }
  type ResidencyFact =
    | { readonly _tag: "Arrived"; readonly arrival: GlbViewport.Arrival }
    | { readonly _tag: "Environment"; readonly key: ContentKey }
    | { readonly _tag: "Refused"; readonly refusal: GlbFault }
  type Loop = {
    readonly advance: (delta: number) => void
    readonly facts: Stream.Stream<Glb.ResidencyFact, GlbFault>
    readonly dome: Effect.Effect<Glb.Dome>
    readonly rebind: (acquired: Glb.Acquired) => Effect.Effect<void>
  }
}

const _AssetIdentity = Schema.Struct({
  slug: Schema.NonEmptyString,
  // the iac `_Asset` admission alphabet verbatim: the digest segment is LOWERCASE at admission on BOTH ends of
  // the `assets/<digest>/` derivation, so a mis-cased key refuses here instead of deriving a directory the
  // publish never wrote — uppercasing or re-lowering at the join is the deleted direction
  digest: Schema.String.pipe(Schema.pattern(/^[a-z0-9_-]+$/)),
  file: Schema.NonEmptyString,
})

const _AssetRoster = Schema.Array(_AssetIdentity).pipe(
  Schema.filter(
    (rows) => new Set(rows.map((row) => row.slug)).size === rows.length,
    { identifier: "UniqueAssetSlugs" },
  ),
)

const _asset = (roster: Glb.AssetRoster, slug: string): Effect.Effect<Glb.AssetIdentity, GlbFault> =>
  Option.match(Option.fromNullable(roster.find((row) => row.slug === slug)), {
    onNone: () => Effect.fail(new GlbFault({ reason: "codec-absent", mesh: slug, detail: "<asset-identity>" })),
    onSome: Effect.succeed,
  })

const _assetPath = (asset: Glb.AssetIdentity): string => `assets/${asset.digest}/${asset.file}`

// trailing slashes are part of the spelling: a directory-consuming decoder concatenates its own leaf names verbatim
const _assetDir = (asset: Glb.AssetIdentity): string => `assets/${asset.digest}/`

declare module "../../src/system/hook.ts" {
  interface Points {
    readonly "rasm.ui.scene.residency": { readonly modality: "observe"; readonly payload: Glb.ResidencyFact }
  }
}

const _GRAFTED = Convention.mount(Convention.metric.sceneGrafts)

// closed GlbFault reason rows preregister, so a refusal nothing has raised yet reads zero rather than absent
const _REFUSED = Convention.mount(Convention.metric.sceneRefusals, GlbFault.roster) // the ordered roster, not a key walk: the word census demands a proven-non-empty tuple

// ONE traverse kernel carries both GPU-residency directions: a visitor reaching a slot on the upload arm reaches
// same slot on the release arm, so eager upload never covers a texture the teardown then leaks
const _walk = (node: Object3D, visit: (mesh: Mesh, materials: ReadonlyArray<Material>) => void): void => {
  // BOUNDARY ADAPTER
  node.traverse((child) => {
    if (child instanceof Mesh) {
      visit(child, Array.isArray(child.material) ? child.material : [child.material])
    }
  })
}

const _slots = (material: Material): ReadonlyArray<Texture> =>
  Object.values(material).filter((slot): slot is Texture => slot instanceof Texture)

// material.dispose() frees the program and never its textures, so the slots release ahead of their material
const _release = (mesh: Mesh, materials: ReadonlyArray<Material>): void => {
  mesh.geometry.dispose()
  materials.forEach((material) => {
    _slots(material).forEach((slot) => slot.dispose())
    material.dispose()
  })
}

const _upload =
  (renderer: WebGLRenderer | WebGPURenderer) => (_mesh: Mesh, materials: ReadonlyArray<Material>): void =>
    materials.forEach((material) => _slots(material).forEach((slot) => renderer.initTexture(slot)))

const _graft = (
  root: Scene,
  codecs: Glb.Codecs,
  acquired: Glb.Acquired,
  port: Context.Tag.Service<GlbViewport>,
  // both residency lanes fork with their refusals contained, so construction succeeds or dies; `_codecs` alone
  // carries `codec-absent`, and a caller inheriting an unraisable channel writes a recovery arm nothing reaches
): Effect.Effect<Glb.Loop, never, Scope.Scope> =>
  Effect.gen(function* () {
    const held = yield* Ref.make(HashMap.empty<ContentKey, Glb.Graft>())
    const facts = yield* Queue.bounded<Glb.ResidencyFact>(32)
    const insert = Stream.runForEach(port.arrivals, (arrival) =>
      Effect.gen(function* () {
        const gltf = yield* Effect.tryPromise({
          try: () => codecs.gltf.parseAsync(arrival.octets.buffer, ""),
          // registered gates reject with their own typed refusal, so codec-absent survives the promise edge
          catch: (defect) =>
            defect instanceof GlbFault
              ? defect
              : new GlbFault({ reason: "decode-refused", mesh: `${arrival.key}`, detail: String(defect) }),
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
          _walk(gltf.scene, _upload(acquired.renderer))
          root.add(gltf.scene)
        })
        yield* Ref.update(held, HashMap.set(arrival.key, { node: gltf.scene, mixer }))
        yield* Effect.asVoid(Effect.withMetric(Effect.succeed(1), _GRAFTED))
        yield* Queue.offer(facts, { _tag: "Arrived", arrival } as const)
      }).pipe(
        Metric.trackErrorWith(_REFUSED, (fault: GlbFault) => fault.reason),
        Effect.withSpan("rasm.ui.scene.residency", { attributes: { "glb.bytes": arrival.octets.byteLength } }),
        Effect.annotateLogs({ mesh: `${arrival.key}` }),
        Effect.catchAll((refusal) => Queue.offer(facts, { _tag: "Refused", refusal } as const)),
      )))
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
            Ref.update(held, HashMap.remove(key)),
            Effect.sync(() => {
              Option.map(graft.mixer, (live) => {
                live.stopAllAction()
                live.uncacheRoot(graft.node)
              })
              root.remove(graft.node)
              _walk(graft.node, _release)
            }),
          ))
      }))
    const dome = yield* _dome(root, acquired, codecs, port.environments, facts)
    yield* Effect.forkScoped(Effect.all([insert, evict], { concurrency: 2, discard: true }))
    return {
      advance: (delta) => {
        // BOUNDARY ADAPTER
        HashMap.forEach(Effect.runSync(Ref.get(held)), (graft) =>
          Option.map(graft.mixer, (live) => live.update(delta)))
      },
      facts: Stream.fromQueue(facts),
      dome: Ref.get(dome.slot),
      rebind: dome.rebind,
    }
  })

const _residencyHook = (loop: Glb.Loop): Hook.Row<"rasm.ui.scene.residency"> => ({
  modality: "observe",
  depth: 32,
  source: Option.some(loop.facts),
})

const _MESHOPT = "EXT_meshopt_compression"

// GLTFParser.json is an untyped platform value: the gate decodes the one field it reads and never touches the rest
const _Required = Schema.Struct({ extensionsRequired: Schema.optional(Schema.Array(Schema.String)) })

const _gate = (admitted: boolean) => (parser: GLTFParser): GLTFLoaderPlugin => ({
  name: "rasm.codec.gate",
  // beforeRoot runs after the JSON chunk parses and before any buffer view decodes, so an unserved decoder
  // refuses as the declared reason instead of dying inside three's decompression arm
  beforeRoot: () =>
    admitted ||
      !Option.match(Schema.decodeUnknownOption(_Required)(parser.json), {
        onNone: () => false,
        onSome: (row) => (row.extensionsRequired ?? []).includes(_MESHOPT),
      })
      ? null
      : Promise.reject(new GlbFault({ reason: "codec-absent", mesh: _MESHOPT, detail: "<meshopt-unserved>" })),
})

const _codecs = (
  roster: Glb.AssetRoster,
  renderer: WebGLRenderer | WebGPURenderer,
  taps: {
    readonly progress: (url: string, loaded: number, total: number) => void
    readonly error: (url: string) => void
  },
): Effect.Effect<Glb.Codecs, GlbFault> =>
  Effect.gen(function* () {
    const draco = yield* _asset(roster, "draco")
    const ktx2 = yield* _asset(roster, "ktx2")
    const decoder = yield* Option.match(yield* Effect.option(_asset(roster, "meshopt")), {
      onNone: () => Effect.succeedNone,
      onSome: (row) =>
        Effect.map(
          Effect.tryPromise({
            try: (): Promise<{ readonly MeshoptDecoder: typeof MeshoptDecoder }> => import(_assetPath(row)),
            catch: () => new GlbFault({ reason: "codec-absent", mesh: row.slug, detail: "<decoder-module>" }),
          }),
          (module) => Option.some(module.MeshoptDecoder),
        ),
    })
    return yield* Effect.sync(() => {
      // BOUNDARY ADAPTER — the decoder setters take the DIRECTORY form; each loader joins its own leaf names
      const manager = new LoadingManager()
      manager.onProgress = taps.progress
      manager.onError = taps.error
      // transcoders bind ONCE and publish, never swallowed: [6]'s deep-store dome decodes through this one
      const basis = new KTX2Loader(manager).setTranscoderPath(_assetDir(ktx2)).detectSupport(renderer)
      const built = new GLTFLoader(manager)
        .setDRACOLoader(new DRACOLoader(manager).setDecoderPath(_assetDir(draco)).preload())
        .setKTX2Loader(basis)
        .register(_gate(Option.isSome(decoder)))
      return {
        gltf: Option.match(decoder, { onNone: () => built, onSome: (row) => built.setMeshoptDecoder(row) }),
        ktx2: basis,
      } satisfies Glb.Codecs
    })
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

## [06]-[ENVIRONMENT_FOLD]

[ENVIRONMENT_FOLD]:
- Owner: the content-keyed dome fold — one environment per scene: the port's `environments` stream enters one lane, each arrival container-sniffed by magic (`OpenEXR` little-endian `20000630` at byte 0, Radiance `#?`, the KTX 2.0 twelve-byte identifier) — never a filename, format knob, or producer identity, so the C# press estate and the python ingest/IBL estate serve the same fold — and decoded through the matching `_CONTAINERS` row. ONE commit owns every dome write: it stamps the scene read-policy fields (`environmentIntensity` the wire `intensity`, `environmentRotation` the composed orientation, the `_ENV` backdrop row mirroring both onto `background` beside `backgroundBlurriness`), uploads a fresh source through the acquisition, mints the dome's inverse orientation once, retires only the handles its successor supersedes, and returns the row one `Ref`-held slot carries and the graft republishes as `Glb.Loop.dome`. Three callers reach that commit and nothing else writes the scene — a NEW key decodes and prefilters ONCE through `Glb.Prefilter.fromEquirectangular`, a SAME-key arrival carries the held source and target forward and re-reads policy alone, and `Glb.Loop.rebind` re-derives the target from the held source against a fresh acquisition. Each arrival's `sh9` lands as one `SphericalHarmonics3` beside that basis, so `Glb.irradiance` answers a directional query without re-reading the scene and without allocating.
- Packages: `three` (`HDRLoader` — the live spelling whose deprecated `RGBELoader` alias is never authored, `EXRLoader`, `RoomEnvironment`, `DataTexture`, `Euler`, `Quaternion`, `Vector3`, `SphericalHarmonics3`, `EquirectangularReflectionMapping`, `HalfFloatType`); `effect` (`Effect`, `Metric`, `Option`, `Queue`, `Ref`, `Stream`).
- Law: one commit owns read policy, orientation, upload, and retirement together, and retirement is IDENTITY-keyed — a handle the successor carries forward never releases under it, the scene fields always stamp before a superseded handle disposes, and a torn dome is therefore unreachable rather than guarded against. Every entry reaching it supplies its own handle pair; a second write path to `scene.environment` is the fork this owner forecloses.
- Law: repeating the held key is a POLICY re-read, never a no-op — intensity and rotation are stored-frame reads, so re-exposing or re-orienting a dome is exactly the arrival the wire's own law blesses, arriving under the digest it never re-keys; dropping it strands the new policy behind an unchanged blob with nothing left to carry it, and re-decoding it burns a prefilter pass the stored frame already made unnecessary.
- Law: the producer frame is `+Z`-up and three samples `+Y`-up, so BOTH reads remap the BASIS and neither rewrites a stored value — the equirect stamps `R = Ry(rotation) · Rx(−π/2)` as one `YXZ` Euler on the environment fields, and an irradiance query un-rotates its world normal through the INVERSE of that same orientation, minted once at commit because a dome's rotation is fixed for its lifetime; the wire bands are never permuted, negated, or re-projected and the plane bytes never re-encode. Landing proves against the producer's axis fixture: a dome whose only non-zero band is `sh_2` answers `2π/3` at the producer's `+Z` pole and zero at its equator — a band-permuting runtime answers at the wrong pole, and a producer-side re-encode to Y-up is the fork the frozen mapping forecloses.
- Law: the prefiltered target is renderer-bound and the decoded source is not — `Glb.Loop.rebind` is the realized consequence, re-folding the held source through the fresh prefilter and retiring the dead target under the same identity-keyed arm, which is why the source outlives its prefilter pass and disposes only on replacement or scope close; a target surviving renderer identity is the leak, a source disposed at prefilter time is the re-init refetch, and a re-init that never calls `rebind` leaves the scene sampling a disposed texture.
- Law: the analytic floor is the slot's boot row — `fromScene(RoomEnvironment(), _ENV.floorBlur)` bakes once, the room scene disposes at the bake, and the keyless `Glb.Dome` row serves `scene.environment` until the first arrival supersedes it; the same bake answers `rebind` while no arrival has landed, and the floor's harmonics are the zero set so an irradiance query before any arrival reads black rather than a fabricated dome. `compileEquirectangularShader` warms beside it because the floor bake compiles the CUBEMAP path alone — without the warm, the first dome pays a shader compile mid-session — and the core class returns `void` where the unified class returns a promise, so the warm awaits a union rather than assuming one spelling.
- Law: the nine bands carry VERBATIM — the wire ships 27 values band-major with RGB interleaved at `i·3 + c`, which is exactly the stride `SphericalHarmonics3.fromArray` reads into `coefficients[i]`, and the member admits any `ArrayLike`, so the frozen carrier lands with neither a re-ordering site nor a defensive copy; a length other than 27 refuses `decode-refused` at the fold, and `getIrradianceAt` already reconstructs under the producer's own `π`, `2π/3`, `π/4` convolution constants, so no rescale exists here either. It reads its normal into locals before it writes its target, so a query aliases the two and allocates nothing.
- Law: the harmonics are a CPU read, NEVER a second scene light — `scene.environment` already carries the prefiltered diffuse, so seating the same coefficients on a `LightProbe` beside a live dome adds a second irradiance term and doubles the diffuse; the rig's `probe` row stays the pre-dome analytic seat, and `Glb.irradiance` serves probe receipts, mark tinting, and any analysis read off the GPU path.
- Law: every decoded source is scene-resident, so it crosses `renderer.initTexture` at commit exactly as a grafted texture slot does — the backdrop samples it every frame and the eager-upload law admits no lazy corner. `_ENV.type` is the declared read depth on the two `DataTextureLoader` rows alone, narrowing the wire's `rgba32f`/`rgba16f` planes once as a policy row; the deep-store row carries the store's own type out of its `vkFormat` and takes no read-depth declaration.
- Law: the deep-store row rides `[5]`'s ONE transcoder — `KTX2Loader.parse` is callback-shaped and refuses until `detectSupport` fills its worker config, an uncompressed KTX2 yields a `DataTexture` where the shipped declaration promises a `CompressedTexture` so the row binds `Texture` and never the declared subtype, and the transcoding leg TRANSFERS the arrival buffer to its worker. That transfer is unreachable here by wire law: an environment plane is float or half, block compression admits an 8-bit store alone, so a dome always carries the uncompressed payload and always reads `needs_transcoding` false.
- Law: the lane weaves exactly as the graft lane — span, `_REFUSED` over the closed reason vocabulary, refusal into the one bounded fact queue — and an `Environment` fact commits only after the scene fields do.
- Boundary: the producer's GGX pyramid, split-sum LUT, and luminance CDF serve renderers evaluating their own IBL — `PMREMGenerator` fixes its mip layout and roughness mapping internally, so this fold re-derives the radiance chain from the equirect and consumes the producer's harmonics alone; binding a foreign pyramid into `scene.environment` forks the roughness-to-mip relation the renderer already owns.
- Growth: a new container is one `_CONTAINERS` row carrying its probe beside a decode it owns end to end, so a synchronous, callback-shaped, or promise-shaped decoder needs no second lane and no caller knob; a second dome (per-viewport split lighting) is a slot-per-scene instantiation, the fold already scene-parameterized.

```typescript signature
import { Effect, Metric, Option, Queue, Ref, Scope, Stream } from "effect"
import { EquirectangularReflectionMapping, Euler, HalfFloatType, Quaternion, SphericalHarmonics3, Vector3 } from "three"
import type { DataTexture, Scene, Texture, TextureDataType } from "three"
import { RoomEnvironment } from "three/addons/environments/RoomEnvironment.js"
import { EXRLoader } from "three/addons/loaders/EXRLoader.js"
import { HDRLoader } from "three/addons/loaders/HDRLoader.js"

declare namespace Glb {
  // one shape carries the analytic floor AND every keyed dome: the floor is the row with no key and no source,
  // and it holds read policy beside the GPU handles so an irradiance query never re-reads the scene fields;
  // `basis` is the read-time un-rotation, derived at commit because a dome's orientation is fixed for its life
  type Dome = {
    readonly key: Option.Option<ContentKey>
    readonly source: Option.Option<Texture>
    readonly target: { readonly texture: Texture; dispose(): void }
    readonly sh: SphericalHarmonics3
    readonly intensity: number
    readonly rotation: number
    readonly basis: Quaternion
  }
  // probe beside a decode the row owns WHOLE: the two DataTextureLoader legs decode synchronously and the
  // deep-store leg is callback-shaped, so the row carries the shape difference and the lane never branches on it
  type Container = {
    readonly probe: (octets: Uint8Array<ArrayBuffer>) => boolean
    readonly decode: (arrival: GlbViewport.Environment, codecs: Glb.Codecs) => Effect.Effect<Texture, GlbFault>
  }
  type Slot = {
    readonly slot: Ref.Ref<Glb.Dome>
    readonly rebind: (acquired: Glb.Acquired) => Effect.Effect<void>
  }
}

const _ENV = { type: HalfFloatType, backdrop: true, blur: 0, floorBlur: 0.04, bands: 27 } as const satisfies {
  type: TextureDataType
  backdrop: boolean
  blur: number
  floorBlur: number
  bands: number
}

// KTX 2.0 spells its file identifier in twelve bytes — the deep store's only self-description before its header parses
const _KTX2 = [0xab, 0x4b, 0x54, 0x58, 0x20, 0x32, 0x30, 0xbb, 0x0d, 0x0a, 0x1a, 0x0a] as const

// both DataTextureLoader legs differ only by their loader, so the decode parameterizes over the constructor
const _plane =
  (open: () => { createDataTexture(buffer: ArrayBuffer): DataTexture }) =>
  (arrival: GlbViewport.Environment): Effect.Effect<Texture, GlbFault> =>
    Effect.try({
      try: () => open().createDataTexture(arrival.octets.buffer),
      catch: (defect) => new GlbFault({ reason: "decode-refused", mesh: `${arrival.key}`, detail: String(defect) }),
    })

// container magic, never filename or producer: OpenEXR spells 20000630 little-endian at byte 0, Radiance "#?"
const _CONTAINERS = {
  exr: {
    probe: (o: Uint8Array<ArrayBuffer>): boolean =>
      o.byteLength >= 4 && new DataView(o.buffer, o.byteOffset, 4).getUint32(0, true) === 20000630,
    decode: _plane(() => new EXRLoader().setDataType(_ENV.type)),
  },
  hdr: {
    probe: (o: Uint8Array<ArrayBuffer>): boolean => o.byteLength >= 2 && o[0] === 0x23 && o[1] === 0x3f,
    decode: _plane(() => new HDRLoader().setDataType(_ENV.type)),
  },
  ktx2: {
    probe: (o: Uint8Array<ArrayBuffer>): boolean =>
      o.byteLength >= _KTX2.length && _KTX2.every((byte, at) => o[at] === byte),
    // dual-callback surfaces are the seam Effect.async owns; this instance is [5]'s configured transcoder,
    // and its declared CompressedTexture is narrower than the DataTexture an uncompressed store yields
    decode: (arrival: GlbViewport.Environment, codecs: Glb.Codecs): Effect.Effect<Texture, GlbFault> =>
      Effect.async<Texture, GlbFault>((resume) =>
        codecs.ktx2.parse(
          arrival.octets.buffer,
          (texture) => resume(Effect.succeed(texture)),
          (defect) =>
            resume(Effect.fail(new GlbFault({ reason: "decode-refused", mesh: `${arrival.key}`, detail: String(defect) }))),
        )),
  },
} as const satisfies Record<string, Glb.Container>

const _sniff = (octets: Uint8Array<ArrayBuffer>): Option.Option<Glb.Container> =>
  Option.fromNullable(Object.values<Glb.Container>(_CONTAINERS).find((row) => row.probe(octets)))

// wire equirect is +Z-up, three samples +Y-up: remap the read basis, then rotate about world up — R = Ry(rotation) · Rx(-π/2);
// plane bytes never rewrite, so one blob serves every orientation
const _orient = (rotation: number): Euler => new Euler(-Math.PI / 2, rotation, 0, "YXZ")

// wire layout IS three's own `fromArray` stride — band-major, RGB interleaved at i*3+c — and the member
// reads any ArrayLike, so the carrier transcribes with no permutation site and no copy; any other
// cardinality is a decode refusal, never a padded read
const _harmonics = (sh9: ReadonlyArray<number>): Option.Option<SphericalHarmonics3> =>
  sh9.length === _ENV.bands ? Option.some(new SphericalHarmonics3().fromArray(sh9)) : Option.none()

// caller targets double as the scratch: getIrradianceAt reads its normal into locals before it writes,
// so un-rotating into the target and reconstructing in place is exact and a directional query allocates nothing
const _irradiance = (dome: Glb.Dome, normal: Vector3, target: Vector3): Vector3 =>
  dome.sh.getIrradianceAt(target.copy(normal).applyQuaternion(dome.basis), target).multiplyScalar(dome.intensity)

const _floor = (prefilter: Glb.Prefilter): Effect.Effect<{ readonly texture: Texture; dispose(): void }> =>
  Effect.sync(() => {
    // BOUNDARY ADAPTER — the analytic bake owns its room scene end to end and releases it at the bake
    const room = new RoomEnvironment()
    const baked = prefilter.fromScene(room, _ENV.floorBlur)
    room.dispose()
    return baked
  })

// ONE dome commit stamps scene fields, uploads a fresh source, and retires a handle ONLY where its successor
// supersedes it — so a policy re-read over carried handles disposes nothing and no frame samples a torn dome
const _bind = (
  root: Scene,
  acquired: Glb.Acquired,
  held: Option.Option<Glb.Dome>,
  next: Omit<Glb.Dome, "basis">,
): Effect.Effect<Glb.Dome> =>
  Effect.sync(() => {
    // BOUNDARY ADAPTER
    const orientation = _orient(next.rotation)
    Option.map(next.source, (plane) => {
      plane.mapping = EquirectangularReflectionMapping
      acquired.renderer.initTexture(plane)
    })
    // backdrops derive from the SUCCESSOR's source in every case, so a keyless row never strands a retired plane
    root.background = _ENV.backdrop ? Option.getOrNull(next.source) : null
    root.backgroundIntensity = next.intensity
    root.backgroundBlurriness = _ENV.blur
    root.backgroundRotation.copy(orientation)
    root.environment = next.target.texture
    root.environmentIntensity = next.intensity
    root.environmentRotation.copy(orientation)
    Option.map(held, (gone) => {
      Option.map(Option.filter(gone.source, (row) => !Option.exists(next.source, (live) => live === row)), (row) => row.dispose())
      if (gone.target !== next.target) gone.target.dispose()
    })
    return { ...next, basis: new Quaternion().setFromEuler(orientation).invert() }
  })

const _rebind = (root: Scene, slot: Ref.Ref<Glb.Dome>) => (acquired: Glb.Acquired): Effect.Effect<void> =>
  Effect.gen(function* () {
    const held = yield* Ref.get(slot)
    const target = yield* Option.match(held.source, {
      onNone: () => _floor(acquired.prefilter),
      onSome: (source) => Effect.sync(() => acquired.prefilter.fromEquirectangular(source)),
    })
    yield* Effect.flatMap(
      _bind(root, acquired, Option.some(held), {
        key: held.key,
        source: held.source,
        target,
        sh: held.sh,
        intensity: held.intensity,
        rotation: held.rotation,
      }),
      (dome) => Ref.set(slot, dome),
    )
  })

const _dome = (
  root: Scene,
  acquired: Glb.Acquired,
  codecs: Glb.Codecs,
  arrivals: Stream.Stream<GlbViewport.Environment, GlbFault>,
  facts: Queue.Queue<Glb.ResidencyFact>,
  // every refusal is contained inside the forked lane, so the constructor itself carries no error channel
): Effect.Effect<Glb.Slot, never, Scope.Scope> =>
  Effect.gen(function* () {
    const baked = yield* _floor(acquired.prefilter)
    const floor = yield* _bind(root, acquired, Option.none(), {
      key: Option.none<ContentKey>(),
      source: Option.none<Texture>(),
      target: baked,
      sh: new SphericalHarmonics3(),
      intensity: 1,
      rotation: 0,
    })
    const slot = yield* Ref.make(floor)
    yield* Effect.addFinalizer(() =>
      Effect.flatMap(Ref.get(slot), (dome) =>
        Effect.sync(() => {
          // BOUNDARY ADAPTER
          dome.target.dispose()
          Option.map(dome.source, (source) => source.dispose())
        })))
    // floor bakes compile the CUBEMAP path alone; warming the equirect path off the critical path keeps the
    // first arrival free of a shader compile, and the await spans both backend spellings of the return
    yield* Effect.forkScoped(Effect.promise(async () => await acquired.prefilter.compileEquirectangularShader()))
    yield* Effect.forkScoped(Stream.runForEach(arrivals, (arrival) =>
      Effect.gen(function* () {
        const held = yield* Ref.get(slot)
        const sh = yield* Option.match(_harmonics(arrival.sh9), {
          onNone: () =>
            Effect.fail(new GlbFault({ reason: "decode-refused", mesh: `${arrival.key}`, detail: "<sh9-cardinality>" })),
          onSome: Effect.succeed,
        })
        // ONE branch governs the lane: a held key carries its handles forward, anything else decodes and prefilters
        const handles = yield* (Option.exists(held.key, (key) => key === arrival.key)
          ? Effect.succeed({ source: held.source, target: held.target })
          : Effect.gen(function* () {
              const row = yield* Option.match(_sniff(arrival.octets), {
                onNone: () =>
                  Effect.fail(new GlbFault({ reason: "decode-refused", mesh: `${arrival.key}`, detail: "<container-magic>" })),
                onSome: Effect.succeed,
              })
              const source = yield* row.decode(arrival, codecs)
              return yield* Effect.sync(() => ({
                source: Option.some(source),
                target: acquired.prefilter.fromEquirectangular(source),
              }))
            }))
        const dome = yield* _bind(root, acquired, Option.some(held), {
          key: Option.some(arrival.key),
          source: handles.source,
          target: handles.target,
          sh,
          intensity: arrival.intensity,
          rotation: arrival.rotation,
        })
        yield* Ref.set(slot, dome)
        yield* Queue.offer(facts, { _tag: "Environment", key: arrival.key } as const)
      }).pipe(
        Metric.trackErrorWith(_REFUSED, (fault: GlbFault) => fault.reason),
        Effect.withSpan("rasm.ui.scene.residency", { attributes: { "glb.bytes": arrival.octets.byteLength } }),
        Effect.annotateLogs({ mesh: `${arrival.key}` }),
        Effect.catchAll((refusal) => Queue.offer(facts, { _tag: "Refused", refusal } as const)),
      )))
    return { slot, rebind: _rebind(root, slot) }
  })
```

## [07]-[DRAW_COLLAPSE]

[DRAW_COLLAPSE]:
- Owner: the collapse rows the graft applies at parse time, keyed by what repeats: same-material submeshes within one graft merge through `BufferGeometryUtils.mergeGeometries`; N identical geometries collapse into one `InstancedMesh` whose per-instance transform stamps through `setMatrixAt` and re-bounds through `computeBoundingSphere`; distinct same-material geometries batch into one `BatchedMesh` — `addGeometry` per unique geometry, `addInstance` per placement, `setVisibleAt` as the per-instance visibility toggle the selection echo and the compute-lane cull both drive.
- Packages: `three` (`InstancedMesh`, `BatchedMesh`, `Matrix4`); `three/addons` (`BufferGeometryUtils`).
- Law: collapse never alters appearance identity — a merged or batched node keeps its source meshes' material keys so `[8]`'s keyed override still targets exactly the meshes the wire names; a collapse that widens an override's blast radius is the named defect.
- Law: visibility is a flag, never a rebuild — hiding an instanced element flips `setVisibleAt`; removing and re-adding geometry for a visibility change is the named defect.
- Growth: a new repetition signal (an element-graph repetition census) is one detection row feeding the same three collapse arms — the arms never multiply.

```typescript signature
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

## [08]-[APPEARANCE_BIND]

[APPEARANCE_BIND]:
- Owner: `Pbr` — the field-for-field bind of the C# OpenPBR algebra: `Material`/`PbrGroups`/`AppearanceSummary` arrive DECODED through `core/interchange/codec#LANDING_WIRE` (the verbatim mirror of the `csharp:Rasm.Materials/Appearance` projection), and `Pbr.bind(material, bound)` lands the five wire blocks onto `MeshPhysicalMaterial` lobes exactly as projected — base → `color`/`metalness`/`roughness`, specular → `specularColor`/`specularIntensity`/`ior`, transmission → `transmission`/`attenuationColor`/`attenuationDistance` (the wire's `depth` IS the attenuation depth), emission → `emissive`/`emissiveIntensity` (the wire's `luminance`), geometry → `opacity`/`transparent`/`side` — with `needsUpdate` stamped once at the fold's tail. Every numeric is carriage: a TS-side derivation, regrouping, or convenience-merge of any OpenPBR parameter is the cross-language drift defect.
- Packages: `three` (`MeshPhysicalMaterial` lobes, `Color`, `LinearSRGBColorSpace`, `FrontSide`/`DoubleSide` — members verified against the shipped runtime; `@types/three` supplies compile-time declarations only, never member truth); `@rasm/ts/core` (`Material`, `PbrGroups`, `AppearanceSummary`, `ContentKey`); `effect` (`HashMap`, `Option`).
- Law: assignments mirror the projection's grouping — the fold's arm order IS the wire's block order, so a C# projection change lands here as the same-shaped field wave; a flattened group or renamed field breaks the mirror and the golden fixtures upstream.
- Law: unit semantics ride the SI quantity contract — weights arrive unit-interval, distances in the projection's units; a clamp, remap, or correction in the fold is the drift defect, and an out-of-range value is upstream evidence. `transparent` and `side` are the two render-representation toggles three demands (`opacity < 1` raises the blend flag, `thinWalled` selects `DoubleSide`) — structural consequences of carriage, and no other computed value exists in the fold.
- Law: color triples are linear-space carriage — `Color.setRGB(r, g, b, LinearSRGBColorSpace)` ingests under three's `ColorManagement`, the renderer's `outputColorSpace` owns display transform, and a THEME color reaching the scene (selection tint, highlight) crosses through `Theme.linear` (`system/token`'s OKLCH → srgb-linear projection) into the same `setRGB` seam — one color contract, drift structurally impossible; no gamma math exists in this module.
- Law: `AppearanceSummary` is the preload census — read BEFORE any bind so the scene resolves every `Material.groups` reference once into the interior `HashMap<ContentKey, PbrGroups>`; a dangling reference is upstream evidence surfaced as-is, never a silent default material. Overrides are keyed — a `Material` row targets a mesh content key and the bind applies ONLY to targeted meshes; GLB-embedded materials on untargeted meshes ride untouched, so appearance is an overlay, never a repaint. Rebinding is idempotent pure assignment driven by the same keyed ledger the graft holds.
- Law: the WebGPU path swaps the material class, never the fold — `MeshPhysicalNodeMaterial` (`three/webgpu`) carries the same lobe fields, so the assignments land unchanged; TSL node-graph authoring is reached only where a lobe becomes a computed node (a probe-driven debug view), and such a graph is render-side presentation, never OpenPBR algebra.
- Growth: a new OpenPBR block (coat, sheen, iridescence, anisotropy — `MeshPhysicalMaterial` already carries the target lobes) is one wire mirror field wave with one assignment arm here, landed in the same wave as the C# projection change — the fold signature never changes and TS never emits a block ahead of the wire.

```typescript signature
import type { Material as MaterialRow, PbrGroups } from "@rasm/ts/core"
import { HashMap, Option } from "effect"
import { DoubleSide, FrontSide, LinearSRGBColorSpace, type Color, type MeshPhysicalMaterial } from "three"

declare namespace Pbr {
  type Bound = { readonly material: MaterialRow; readonly groups: PbrGroups }
  type Index = HashMap.HashMap<ContentKey, PbrGroups>
  type Shape = {
    readonly bind: typeof _bind
    readonly index: typeof _index
    readonly resolve: typeof _resolve
  }
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

const Pbr: Pbr.Shape = { bind: _bind, index: _index, resolve: _resolve }
```

## [09]-[INSTANCED_ROWS]

[INSTANCED_ROWS]:
- Owner: `Instanced` — the georeferenced instancing rows over `@deck.gl/mesh-layers`: `Instanced.mesh` places ONE arbitrary mesh at N element anchors through `SimpleMeshLayer`, `Instanced.scene` places a COMPLETE glTF scenegraph through `ScenegraphLayer` with per-node animation under the shared atom clock; both ride the one instance-transform axis — the `getPosition` anchor with the `getOrientation`/`getScale`/`getTranslation` triple — and every row is a declarative deck layer VALUE `geo`'s one `setProps` sink consumes; this module renders nothing itself.
- Packages: `@deck.gl/mesh-layers` (`SimpleMeshLayer`, `ScenegraphLayer`); `@deck.gl/core` (`Position`, `Color`, the accessor plane).
- Law: the transform family is one shared axis — `getTransformMatrix` OVERRIDES the orientation/scale/translation triple; a row supplies the matrix OR the triple, never both, and a third transform vocabulary beside the shared axis is the named defect.
- Law: an anchor row is decoded material — element positions arrive from the decoded geo plane keyed by `GlobalId`, so an instance pick's `PickingInfo.index` resolves through `mark`'s pipes into the one selection set; `pickable` is the row's toggle.
- Law: scenegraph animation rides `_animations` under the overlay's `_animate` flag driven by the same rAF-fed atom clock as `geo`'s trips row — one animation clock across the geo surface, never a second timer.
- Law: this pair is the `Tile3DLayer` mesh peer — `geo`'s 3D-tiles row renders b3dm/glTF tile content THROUGH these classes, and `ScenegraphLayer.onFirstDraw` is the transition-safety signal that row reads.
- Growth: a new instanced-asset need selects one of the two payload rows over the shared axis — never a third prop vocabulary.

```typescript signature
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
  type Shape = {
    readonly mesh: typeof _mesh
    readonly scene: typeof _scene
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

const Instanced: Instanced.Shape = { mesh: _mesh, scene: _scene }
```

## [10]-[EMBED_ROW]

[EMBED_ROW]:
- Owner: the `model-viewer` backend row — the zero-GL-handle embed: `.src` takes a `model/gltf-binary` object URL minted over a port arrival's bytes, `camera-controls` owns orbit interaction, and the element owns decode, upload, camera, and dispose internally; decoder statics (`dracoDecoderLocation`, `ktx2TranscoderLocation`, `meshoptDecoderLocation`) resolve the matching `Glb.AssetRoster` rows BEFORE the first model or the element side-loads from a foreign CDN — a CSP breach; the multi-leaf pair takes the `Glb.assetDir` directory form (the element joins its own leaf names), `meshoptDecoderLocation` the `Glb.assetPath` file form — one classic-script leaf; the statics are process-global upstream, so `Glb.pinned` is token-gated — the first roster fixes the process decoder set, an identical re-pin no-ops, and a divergent roster refuses `manifest-skew` instead of re-pointing decoders under a live element. Its animation surface is native: `play`/`pause` with `PlayAnimationOptions`, `appendAnimation` for additive blend, `availableAnimations` as the clip census — the embed arm's mirror of `[5]`'s mixer family.
- Packages: `@google/model-viewer` (`ModelViewerElement` — the const IS the statics owner and the instance type).
- Law: the element and the three arm share ONE physical `three` module (peer-deduped) but never a renderer, canvas, or GL context — sibling backends the backend literal selects per viewport.
- Law: the object URL lifecycle is bracketed — `URL.createObjectURL` acquires, `URL.revokeObjectURL` releases with the viewport scope; a leaked object URL pins the blob.
- Boundary: camera read/write on the element (`getCameraOrbit`/`getCameraTarget`/`jumpCameraToGoal`, the `camera-change` event) is `geo#CAMERA`'s adapter row; hotspot ray-casting (`positionAndNormalFromPoint`, `updateHotspot`) is `mark`'s anchor row.

```typescript signature
import { ModelViewerElement } from "@google/model-viewer"

// decoder statics are process-global upstream, so the pin is token-gated: the first roster fixes
// process-wide decoder paths, an identical re-pin no-ops, and a divergent roster refuses as skew — no
// per-roster re-point ever races a live element
let _pin: Option.Option<{ readonly draco: string; readonly ktx2: string; readonly meshopt: string }> = Option.none()

const _pinned = (roster: Glb.AssetRoster): Effect.Effect<void, GlbFault> =>
  Effect.gen(function* () {
    const assets = yield* Effect.all({
      draco: _asset(roster, "draco"),
      ktx2: _asset(roster, "ktx2"),
      meshopt: _asset(roster, "meshopt"),
    })
    const paths = { draco: _assetDir(assets.draco), ktx2: _assetDir(assets.ktx2), meshopt: _assetPath(assets.meshopt) }
    yield* Effect.suspend(() =>
      Option.match(_pin, {
        onNone: () =>
          Effect.sync(() => {
            _pin = Option.some(paths)
            ModelViewerElement.dracoDecoderLocation = paths.draco
            ModelViewerElement.ktx2TranscoderLocation = paths.ktx2
            ModelViewerElement.meshoptDecoderLocation = paths.meshopt
          }),
        onSome: (held) =>
          held.draco === paths.draco && held.ktx2 === paths.ktx2 && held.meshopt === paths.meshopt
            ? Effect.void
            : Effect.fail(new GlbFault({ reason: "manifest-skew", mesh: "<decoder-pin>", detail: "<divergent-roster>" })),
      }),
    )
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

declare namespace Glb {
  type Shape = {
    readonly Backend: typeof _Backend
    readonly AssetIdentity: typeof _AssetIdentity
    readonly AssetRoster: typeof _AssetRoster
    readonly asset: typeof _asset
    readonly assetPath: typeof _assetPath
    readonly assetDir: typeof _assetDir
    readonly backends: typeof _backends
    readonly tone: typeof _TONE
    readonly output: typeof _OUTPUT
    readonly env: typeof _ENV
    readonly rig: typeof _RIG
    readonly lit: typeof _lit
    readonly renderer: typeof _renderer
    readonly codecs: typeof _codecs
    readonly graft: typeof _graft
    readonly irradiance: typeof _irradiance
    readonly hook: typeof _residencyHook
    readonly loop: typeof _loop
    readonly instanced: typeof _instanced
    readonly batched: typeof _batched
    readonly merged: typeof _merged
    readonly pinned: typeof _pinned
    readonly embed: typeof _embed
  }
}

const Glb: Glb.Shape = {
  Backend: _Backend,
  AssetIdentity: _AssetIdentity,
  AssetRoster: _AssetRoster,
  asset: _asset,
  assetPath: _assetPath,
  assetDir: _assetDir,
  backends: _backends,
  tone: _TONE,
  output: _OUTPUT,
  env: _ENV,
  rig: _RIG,
  lit: _lit,
  renderer: _renderer,
  codecs: _codecs,
  graft: _graft,
  irradiance: _irradiance,
  hook: _residencyHook,
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

## [11]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
