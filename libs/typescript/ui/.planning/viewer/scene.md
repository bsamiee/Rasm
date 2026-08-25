# [UI_SCENE]

`Glb` owns epoch-scoped scene residency, backend acquisition, environment lighting, OpenPBR binding, and GPU teardown.

## [01]-[INDEX]

- [02]-[VIEWPORT_PORT]: verified scene inputs; `GlbViewport`.
- [03]-[FAULT_FAMILY]: closed scene failures; `GlbFault`.
- [04]-[BACKEND_SELECT]: renderer acquisition and lifecycle; `Glb`.
- [05]-[RESIDENCY_GRAFT]: epoch-safe graft and release; `Glb`.
- [06]-[ENVIRONMENT_FOLD]: environment decode and prefilter; `Glb`.
- [07]-[DRAW_COLLAPSE]: draw, visibility, splat order, and scene-tool queries; `Glb`.
- [08]-[APPEARANCE_BIND]: OpenPBR seating and the standard-to-physical upgrade; `Pbr`.
- [09]-[INSTANCED_ROWS]: georeferenced instances folded off the residency ledger; `Instanced`.
- [10]-[EMBED_ROW]: model-viewer backend; `Glb`.

## [02]-[VIEWPORT_PORT]

- Owner: `GlbViewport` carries the admitted residency manifest, verified arrivals, dome data, byte-backed appearances, and cache ports.
- Law: only verified whole-buffer octets enter the viewer; every geometry arrival carries the manifest epoch it was planned under.
- Boundary: runtime owns hauling and cache policy; UI owns decoding and GPU residency.

```typescript signature
import { Digest, Frame, Wire } from "@rasm/core"
import { Context, type Effect, type HashMap, type Option, type Stream, type Subscribable } from "effect"

type AppearanceRow = Extract<Wire.Decoded<"NodeWire">["payload"], { readonly case: "appearance" }>["value"]

declare namespace GlbViewport {
  type Arrival = { readonly key: Digest.Key<"content">; readonly epoch: number; readonly octets: Uint8Array<ArrayBuffer> }
  type Environment = Wire.Set
  type Appearance = {
    readonly census: ReadonlyArray<AppearanceRow>
    readonly materials: ReadonlyArray<readonly [appearance: Digest.Key<"content">, octets: Uint8Array]>
    readonly sets: ReadonlyArray<readonly [appearance: Digest.Key<"content">, set: Wire.Set]>
    readonly worn: ReadonlyArray<readonly [mesh: Digest.Key<"content">, appearance: Digest.Key<"content">]>
  }
  type Snapshots = {
    readonly read: (key: Digest.Key<"content">) => Effect.Effect<Option.Option<ReadonlyArray<SerializedBVH>>, GlbFault>
    readonly write: (key: Digest.Key<"content">, snapshot: ReadonlyArray<SerializedBVH>) => Effect.Effect<void, GlbFault>
  }
  type Addressed = (key: Digest.Key<"content">) => Option.Option<string>
  type Residency = {
    readonly view: Frame.ResidencyView
    readonly index: HashMap.HashMap<Digest.Key<"content">, Frame.ResidencyTile>
    readonly epoch: number
  }
}

class GlbViewport extends Context.Tag("ui/viewer/GlbViewport")<GlbViewport, {
  readonly manifest: Subscribable.Subscribable<Option.Option<GlbViewport.Residency>>
  readonly arrivals: Stream.Stream<GlbViewport.Arrival, GlbFault>
  readonly environments: Stream.Stream<GlbViewport.Environment, GlbFault>
  readonly appearances: Stream.Stream<GlbViewport.Appearance, GlbFault>
  readonly planes: (artifact: Wire.Artifact.Reference) => Effect.Effect<Uint8Array<ArrayBuffer>, GlbFault>
  readonly snapshots: GlbViewport.Snapshots
  readonly addressed: GlbViewport.Addressed
}>() {}
```

## [03]-[FAULT_FAMILY]

- Owner: `GlbFault` closes scene failure policy over `Fault.Class` and private eviction and plane columns.
- Law: `_faultPolicy.at(reason)` reads the one axis the family does not carry — eviction; `_family.classOf(reason)` supplies the core class and `_family.legOf(reason)` the indicted arm, so the lifecycle guard and the raiser read one roster.
- Law: seating accumulates on ONE carrier — `GlbFault.seating` is the family's own census, so a partially-bound set names every offending plane in a single refusal fact while every other lane refuses singly and keeps the plain carrier.
- Law: the refusal census and its error-rail fold ride ONE declaration beside the family — the mount and the tracking operator are two reads of one row, and the census is the family's own published vocabulary rather than a tuple a call site assembled.

```typescript signature
import { Convention, Fault, Shape } from "@rasm/core"
import { Schema } from "effect"

const _reasons = ["manifest-skew", "decode-refused", "codec-absent", "plane-unbound", "backend-lost"] as const

const _unserved = ["asset-row", "meshopt-codec", "decoder-module"] as const

const _faultRows = {
    "manifest-skew": { evict: true },
    "decode-refused": { evict: true },
    "codec-absent": { evict: false },
    "plane-unbound": { evict: false },
    "backend-lost": { evict: false },
} as const
const _faultPolicy = Shape.vocabulary(_reasons, _faultRows)

const _family = Fault.Class.family(_reasons, {
  "manifest-skew": Fault.Class.row({
    class: "conflicted",
    leg: "asset",
    detail: Schema.Struct({ mesh: Schema.String, cause: Schema.String }),
    render: ({ cause, mesh }) => `${mesh} disagrees with the manifest: ${cause}`,
  }),
  "decode-refused": Fault.Class.row({
    class: "invalid",
    leg: "asset",
    detail: Schema.Struct({ mesh: Schema.String, cause: Schema.String }),
    render: ({ cause, mesh }) => `${mesh} would not decode: ${cause}`,
  }),
  "codec-absent": Fault.Class.row({
    class: "unavailable",
    leg: "asset",
    detail: Schema.Struct({ mesh: Schema.String, absent: Schema.Literal(..._unserved) }),
    render: ({ absent, mesh }) => `${mesh} reaches no ${absent}`,
  }),
  "plane-unbound": Fault.Class.row({
    class: "invalid",
    leg: "asset",
    detail: Schema.Struct({ mesh: Schema.String, cause: Schema.String }),
    render: ({ cause, mesh }) => `${mesh} seats no plane: ${cause}`,
  }),
  "backend-lost": Fault.Class.row({
    class: "unavailable",
    leg: "backend",
    detail: Schema.Struct({ stage: Schema.Literal("device", "init") }),
    render: ({ stage }) => `webgpu backend refused at ${stage}`,
  }),
})

declare namespace GlbFault {
  type Case = typeof _family.payload.Type
  type Reason = (typeof _family.kinds)[number]
  type Arm = ReturnType<typeof _family.legOf<Reason>>
}

class GlbFault extends Schema.TaggedError<GlbFault>()("GlbFault", {
  case: _family.payload,
}) {
  get class(): Fault.Class.Kind {
    return _family.classOf(this.case.reason)
  }
  get evict(): boolean {
    return _faultPolicy.at(this.case.reason).evict
  }
  get arm(): GlbFault.Arm {
    return _family.legOf(this.case.reason)
  }
  override get message(): string {
    return _family.render(this.case)
  }
}

const GlbSeating = _family.census("GlbFault.seating")
type GlbSeating = InstanceType<typeof GlbSeating>

const _refused = Convention.tracked(Convention.metric.sceneRefusals, _faultPolicy, (fault: GlbFault) => fault.case.reason)
```

## [04]-[BACKEND_SELECT]

- Owner: `Glb` acquires one WebGPU or WebGL renderer, output policy, sampling cap, prefilter, and lifecycle.
- Law: one scoped backplane preserves renderer identity across all decode, upload, dome, and device-loss paths.

```typescript signature
import { Machine } from "@effect/experimental"
import { Convention, Shape } from "@rasm/core"
import { Duration, Effect, Metric, Option, pipe, Schedule, Schema, type Scope, ScopedRef, type Stream, type SubscriptionRef } from "effect"
import {
  ACESFilmicToneMapping, AgXToneMapping, AmbientLight, DirectionalLight, HemisphereLight, LightProbe,
  NeutralToneMapping, PMREMGenerator as GlPrefilter, RectAreaLight, SRGBColorSpace, WebGLRenderer,
} from "three"
import type { Light, PerspectiveCamera, Scene, Texture } from "three"
import { PMREMGenerator as GpuPrefilter, WebGPURenderer } from "three/webgpu"
import type { Clock } from "./geo.ts"

declare namespace Glb {
  type Prefilter = {
    fromScene(scene: Scene, sigma?: number): { readonly texture: Texture; dispose(): void }
    fromEquirectangular(equirect: Texture): { readonly texture: Texture; dispose(): void }
    compileEquirectangularShader(): void | Promise<void>
    dispose(): void
  }
  type Acquired = {
    readonly renderer: WebGLRenderer | WebGPURenderer
    readonly device: Option.Option<GPUDevice>
    readonly prefilter: Glb.Prefilter
    readonly anisotropy: number
    readonly codecs: Glb.Codecs
  }
  type Backplane = ScopedRef.ScopedRef<Glb.Acquired>
  type Phase = (typeof _phases)[number]
  type Signal = (typeof _signals)[number] | typeof _GUARDED
  type Act = (typeof _acts)[number]
  type Row = { readonly next: Glb.Phase; readonly act: Glb.Act }
  type Plane = {
    readonly canvas: HTMLCanvasElement
    readonly served: Glb.Served
    readonly backplane: Glb.Backplane
    readonly loop: Glb.Loop
    readonly root: Scene
    readonly camera: PerspectiveCamera
    readonly frames: SubscriptionRef.SubscriptionRef<Clock.Frame>
    readonly shown: Stream.Stream<boolean>
  }
  type Turn = {
    readonly plane: Glb.Plane
    readonly cool: Duration.Duration
    readonly forkOne: (effect: Effect.Effect<void>, id: string) => Effect.Effect<void>
    readonly forkReplace: (effect: Effect.Effect<void>, id: string) => Effect.Effect<void>
    readonly send: (request: Advance | Refuse) => Effect.Effect<void>
  }
  type Lifecycle = ReturnType<typeof _lifecycle>
  type Stance = Machine.SerializableActor<Glb.Lifecycle>
}

const _backends = ["three", "model-viewer"] as const

const _Backend = Schema.Literal(..._backends)

const _TONE = { aces: ACESFilmicToneMapping, agx: AgXToneMapping, neutral: NeutralToneMapping } as const

const _OUTPUT = { colorSpace: SRGBColorSpace, tone: "agx", exposure: 1 } as const satisfies {
  colorSpace: typeof SRGBColorSpace
  tone: keyof typeof _TONE
  exposure: number
}

const _SAMPLING = { anisotropy: 16 } as const satisfies { anisotropy: number }

const _RIG = {
  ambient: { color: 0xffffff, intensity: 0.3 },
  hemisphere: { sky: 0xffffff, ground: 0x444444, intensity: 0.6 },
  directional: { color: 0xffffff, intensity: 1.2, position: [4, 8, 4] },
  rect: { color: 0xffffff, intensity: 2.4, width: 4, height: 2 },
  probe: { intensity: 1 },
} as const

const _lit = (root: Scene): Scene => {
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

const _renderer = (canvas: HTMLCanvasElement, served: Glb.Served): Effect.Effect<Glb.Acquired, never, Scope.Scope> =>
  Effect.acquireRelease(
    Effect.gen(function* () {
      const acquired = globalThis.navigator.gpu === undefined
        ? Option.none<GPUDevice>()
        : yield* Effect.tryPromise({
            try: async () => {
              const adapter = await globalThis.navigator.gpu.requestAdapter()
              return adapter === null ? Option.none<GPUDevice>() : Option.some(await adapter.requestDevice())
            },
            catch: () => new GlbFault({ case: { reason: "backend-lost", stage: "device" } }),
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
            catch: () => new GlbFault({ case: { reason: "backend-lost", stage: "init" } }),
          }).pipe(Effect.orElseSucceed(() => new WebGLRenderer({ canvas, antialias: true }))),
      })
      built.outputColorSpace = _OUTPUT.colorSpace
      built.toneMapping = _TONE[_OUTPUT.tone]
      built.toneMappingExposure = _OUTPUT.exposure
      const legacy = built instanceof WebGLRenderer
      const prefilter: Glb.Prefilter = legacy ? new GlPrefilter(built) : new GpuPrefilter(built)
      const codecs = yield* _codecs(served, built)
      return {
        renderer: built,
        device: legacy ? Option.none<GPUDevice>() : acquired,
        prefilter,
        anisotropy: Math.min(
          _SAMPLING.anisotropy,
          legacy ? built.capabilities.getMaxAnisotropy() : built.getMaxAnisotropy(),
        ),
        codecs,
      } satisfies Glb.Acquired
    }),
    ({ codecs, prefilter, renderer }) => Effect.sync(() => {
      codecs.ktx2.dispose()
      prefilter.dispose()
      renderer.dispose()
    }),
  )

const _backplane = (canvas: HTMLCanvasElement, served: Glb.Served): Effect.Effect<Glb.Backplane, never, Scope.Scope> =>
  ScopedRef.fromAcquire(_renderer(canvas, served))

const _phases = ["booting", "ready", "degraded", "lost", "reviving"] as const

const _signals = ["settled", "floored", "revive"] as const

const _GUARDED = "refused" as const

const _acts = ["arm", "park", "chase", "hold"] as const

const _FIBERS = { draw: "draw", revive: "revive", watch: "watch" } as const

const _Signal = Schema.Literal(..._signals)

const _Boot = Schema.Struct({ cool: Schema.Duration })

const _turn = {
  settled: { next: "ready", act: "arm" },
  floored: { next: "degraded", act: "arm" },
  refused: { next: "lost", act: "park" },
  revive: { next: "reviving", act: "chase" },
} as const satisfies { readonly [S in Glb.Signal]: Glb.Row }

const _lifecycleRows = {
  booting: _turn,
  ready: _turn,
  degraded: _turn,
  lost: { ..._turn, refused: { next: "lost", act: "hold" } },
  reviving: { ..._turn, revive: { next: "reviving", act: "hold" } },
} as const satisfies { readonly [P in Glb.Phase]: { readonly [S in Glb.Signal]: Glb.Row } }

const _lifecycle = Shape.vocabulary(_phases, _lifecycleRows)

const _PHASED = Convention.mount(Convention.metric.sceneBackend, _lifecycle)

class Advance extends Schema.TaggedRequest<Advance>()("Advance", {
  failure: Schema.Never,
  success: _lifecycle.schema,
  payload: { signal: _Signal },
}) {}

class Refuse extends Schema.TaggedRequest<Refuse>()("Refuse", {
  failure: Schema.Never,
  success: _lifecycle.schema,
  payload: { reason: _family.schema, detail: Schema.String },
}) {}

const _watched = (device: GPUDevice, send: Glb.Turn["send"]): Effect.Effect<void> =>
  Effect.flatMap(Effect.promise(() => device.lost), (info) =>
    info.reason === "destroyed"
      ? Effect.void
      : send(new Refuse({ reason: "backend-lost", detail: info.message })))

const _ACTS: { readonly [K in Glb.Act]: (turn: Glb.Turn) => Effect.Effect<void> } = {
  arm: ({ forkReplace, plane, send }) =>
    Effect.gen(function* () {
      const acquired = yield* ScopedRef.get(plane.backplane)
      yield* plane.loop.rebind(acquired)
      yield* forkReplace(
        Effect.scoped(
          Effect.andThen(_loop(acquired.renderer, plane.root, plane.camera, plane.loop, plane.frames, plane.shown), Effect.never),
        ),
        _FIBERS.draw,
      )
      yield* Option.match(acquired.device, {
        onNone: () => Effect.void,
        onSome: (device) => forkReplace(_watched(device, send), _FIBERS.watch),
      })
    }),
  park: ({ cool, forkOne, forkReplace, send }) =>
    Effect.zipRight(
      forkReplace(Effect.void, _FIBERS.draw),
      forkOne(Effect.delay(send(new Advance({ signal: "revive" })), cool), _FIBERS.revive),
    ),
  chase: ({ forkOne, plane, send }) =>
    forkOne(
      Effect.gen(function* () {
        yield* ScopedRef.set(plane.backplane, _renderer(plane.canvas, plane.served))
        const fresh = yield* ScopedRef.get(plane.backplane)
        yield* send(new Advance({ signal: Option.isSome(fresh.device) ? "settled" : "floored" }))
      }),
      _FIBERS.revive,
    ),
  hold: () => Effect.void,
}

const _turned = (turn: Glb.Turn, phase: Glb.Phase, signal: Glb.Signal): Effect.Effect<readonly [Glb.Phase, Glb.Phase]> =>
  pipe(_lifecycle.at(phase)[signal], (row) =>
    Effect.as(Effect.zipRight(_ACTS[row.act](turn), Metric.update(_PHASED, row.next)), [row.next, row.next] as const))

const _lifecycle = (plane: Glb.Plane) =>
  Machine.makeSerializable({ state: _lifecycle.schema, input: _Boot }, (input, previous) =>
    Effect.succeed(Machine.serializable.make(previous ?? "booting").pipe(
      Machine.serializable.add(Advance, ({ forkOne, forkReplace, request, send, state }) =>
        _turned({ plane, cool: input.cool, forkOne, forkReplace, send }, state, request.signal)),
      Machine.serializable.add(Refuse, ({ forkOne, forkReplace, request, send, state }) =>
        _family.legOf(request.reason) === "backend"
          ? Effect.zipRight(
              Effect.annotateCurrentSpan("glb.backend.loss", request.detail),
              _turned({ plane, cool: input.cool, forkOne, forkReplace, send }, state, _GUARDED),
            )
          : Effect.succeed([state, state] as const)),
    )),
  ).pipe(
    Machine.retry(Schedule.exponential("250 millis").pipe(Schedule.jittered, Schedule.intersect(Schedule.recurs(4)))),
  )

const _booted = (plane: Glb.Plane, cool: Duration.DurationInput): Effect.Effect<Glb.Stance, never, Scope.Scope> =>
  Effect.gen(function* () {
    const actor = yield* Machine.boot(_lifecycle(plane), { cool: Duration.decode(cool) })
    const acquired = yield* ScopedRef.get(plane.backplane)
    yield* actor.send(new Advance({ signal: Option.isSome(acquired.device) ? "settled" : "floored" }))
    return actor
  })
```

## [05]-[RESIDENCY_GRAFT]

- Owner: `Glb.graft` validates the manifest epoch, decodes verified GLB bytes, and owns subtree resources.
- Law: manifest replacement evicts old-epoch grafts even when artifact keys repeat.
- Law: eviction IS absence from the successor's tile set — the manifest names the whole resident set for one viewpoint, so a key it omits is a key the producer released and no row state is owed.
- Law: one traversal owns upload, acceleration, dress, visibility, and release coverage.
- Law: decode capability splits by binding — `Glb.Served` resolves once per viewport, `Glb.Codecs` re-mints per backend generation, and every lane reads the bundle off the live acquisition rather than capturing it.
- Law: a standard-material graft whose appearance demands a physical-only lobe upgrades in place; `<material-unphysical>` names only a material outside that pair.

```typescript signature
import { Convention } from "@rasm/core"
import { Context, Effect, HashMap, Option, Queue, Record, Ref, Schema, Scope, ScopedRef, Stream, SubscriptionRef } from "effect"
import { preinit, preinitModule, preload, type PreloadOptions } from "react-dom"
import {
  AnimationMixer, BufferGeometry, LoadingManager, LoopRepeat, Mesh, MeshPhysicalMaterial, MeshStandardMaterial,
  Points, Scene, Texture,
} from "three"
import type { Material, Object3D, PerspectiveCamera } from "three"
import type { MeshoptDecoder } from "three/addons/libs/meshopt_decoder.module.js"
import { DRACOLoader } from "three/addons/loaders/DRACOLoader.js"
import { GLTFLoader, type GLTFLoaderPlugin, type GLTFParser } from "three/addons/loaders/GLTFLoader.js"
import { KTX2Loader } from "three/addons/loaders/KTX2Loader.js"
import { vertexColor } from "three/tsl"
import { PointsNodeMaterial, WebGPURenderer } from "three/webgpu"
import { acceleratedRaycast, computeBoundsTree, disposeBoundsTree, MeshBVH, SAH } from "three-mesh-bvh"
import type { BVHOptions, SerializedBVH, SplitStrategy } from "three-mesh-bvh"
import { ParallelMeshBVHWorker } from "three-mesh-bvh/worker"
import type { Clock } from "./geo.ts"
import { Hook } from "../../src/system/hook.ts"

declare namespace Glb {
  type Backend = typeof _Backend.Type
  type AssetIdentity = typeof _AssetIdentity.Type
  type AssetIdentityWire = typeof _AssetIdentity.Encoded
  type AssetRoster = typeof _AssetRoster.Type
  type AssetRosterWire = typeof _AssetRoster.Encoded
  type Drawable = Mesh | Points
  type Graft = {
    readonly node: Object3D
    readonly mixer: Option.Option<AnimationMixer>
    readonly epoch: number
    readonly kind: Frame.ResidencyKind
    readonly drawn: ReadonlyArray<Glb.Drawable>
  }
  type Ledger = HashMap.HashMap<Digest.Key<"content">, Glb.Graft>
  type Codecs = { readonly gltf: GLTFLoader; readonly ktx2: KTX2Loader }
  type Served = {
    readonly manager: LoadingManager
    readonly draco: DRACOLoader
    readonly ktx2Dir: string
    readonly meshopt: Option.Option<typeof MeshoptDecoder>
  }
  type Codec = (typeof _codecSlugs)[number]
  type Address = (typeof _addresses)[number]
  type Warm = (typeof _warms)[number]
  type CodecRow = { readonly address: Glb.Address; readonly warm: Glb.Warm }
  type ResidencyFact =
    | { readonly _tag: "Admitted"; readonly view: Frame.ResidencyView; readonly epoch: number }
    | { readonly _tag: "Arrived"; readonly arrival: GlbViewport.Arrival }
    | { readonly _tag: "Environment"; readonly key: Digest.Key<"content"> }
    | { readonly _tag: "Refused"; readonly refusal: GlbFault | GlbSeating }
  type Resident = { readonly tree: MeshBVH; readonly node: Object3D }
  type Trees = {
    readonly stamp: number
    readonly held: HashMap.HashMap<string, Glb.Resident>
    readonly keyOf: (node: Object3D) => Option.Option<string>
  }
  type Loop = {
    readonly root: Scene
    readonly advance: (delta: number) => void
    readonly facts: Stream.Stream<Glb.ResidencyFact>
    readonly dome: Effect.Effect<Glb.Dome>
    readonly trees: Effect.Effect<Glb.Trees>
    readonly rebind: (acquired: Glb.Acquired) => Effect.Effect<void>
  }
}

const _SEGMENT = Schema.String.pipe(Schema.pattern(/^(?!\.{1,2}$)[A-Za-z0-9._-]+$/))

const _AssetIdentity = Schema.Struct({
  slug: _SEGMENT,
  digest: Schema.String.pipe(Schema.pattern(/^[a-z0-9_-]+$/)),
  file: _SEGMENT,
})

const _AssetRoster = Schema.Array(_AssetIdentity).pipe(
  Schema.filter(
    (rows) => new Set(rows.map((row) => row.slug)).size === rows.length,
    { identifier: "UniqueAssetSlugs" },
  ),
)

const _asset = (roster: Glb.AssetRoster, slug: string): Effect.Effect<Glb.AssetIdentity, GlbFault> =>
  Option.match(Option.fromNullable(roster.find((row) => row.slug === slug)), {
    onNone: () => Effect.fail(new GlbFault({ case: { reason: "codec-absent", mesh: slug, absent: "asset-row" } })),
    onSome: Effect.succeed,
  })

const _assetPath = (asset: Glb.AssetIdentity): string => `assets/${asset.digest}/${asset.file}`

const _assetDir = (asset: Glb.AssetIdentity): string => `assets/${asset.digest}/`

declare module "../../src/system/hook.ts" {
  interface Points {
    readonly "rasm.ui.scene.residency": { readonly modality: "observe"; readonly payload: Glb.ResidencyFact }
  }
}

const _GRAFTED = Convention.mount(Convention.metric.sceneGrafts)

const _walk = (
  node: Object3D,
  visit: (drawn: Glb.Drawable, materials: ReadonlyArray<Material>) => void,
): ReadonlyArray<Glb.Drawable> => {
  const seen: Array<Glb.Drawable> = []
  node.traverse((child) => {
    if (child instanceof Mesh || child instanceof Points) {
      seen.push(child)
      visit(child, Array.isArray(child.material) ? child.material : [child.material])
    }
  })
  return seen
}

const _slots = (material: Material): ReadonlyArray<Texture> => [
  ...new Set(Object.values(material).filter((slot): slot is Texture => slot instanceof Texture)),
]

const _retire = (materials: ReadonlyArray<Material>): void =>
  materials.forEach((material) => {
    _slots(material).forEach((slot) => slot.dispose())
    material.dispose()
  })

const _release = (drawn: Glb.Drawable, materials: ReadonlyArray<Material>): void => {
  drawn.geometry.disposeBoundsTree()
  drawn.geometry.dispose()
  _retire(materials)
}

const _upload =
  (renderer: WebGLRenderer | WebGPURenderer) => (_drawn: Glb.Drawable, materials: ReadonlyArray<Material>): void =>
    materials.forEach((material) => _slots(material).forEach((slot) => renderer.initTexture(slot)))

const _BVH = { strategy: SAH, indirect: true, setBoundingBox: true, targetLeafSize: 10, shared: true } as const satisfies {
  strategy: SplitStrategy
  indirect: boolean
  setBoundingBox: boolean
  targetLeafSize: number
  shared: boolean
}

const _options = (onProgress: (fraction: number) => void): BVHOptions => ({
  strategy: _BVH.strategy,
  indirect: _BVH.indirect,
  setBoundingBox: _BVH.setBoundingBox,
  targetLeafSize: _BVH.targetLeafSize,
  useSharedArrayBuffer: _BVH.shared && globalThis.crossOriginIsolated,
  onProgress,
})

let _patch = false

const _patched: Effect.Effect<void> = Effect.sync(() => {
  if (_patch) return
  _patch = true
  BufferGeometry.prototype.computeBoundsTree = computeBoundsTree
  BufferGeometry.prototype.disposeBoundsTree = disposeBoundsTree
  Mesh.prototype.raycast = acceleratedRaycast
})

const _pool: Effect.Effect<ParallelMeshBVHWorker, never, Scope.Scope> = Effect.acquireRelease(
  Effect.sync(() => new ParallelMeshBVHWorker()),
  (pool) => Effect.sync(() => pool.dispose()),
)

const _built = (
  key: Digest.Key<"content">,
  mesh: Mesh,
  pool: ParallelMeshBVHWorker,
  onProgress: (fraction: number) => void,
): Effect.Effect<MeshBVH, GlbFault> =>
  Effect.tryPromise({
    try: () => pool.generate(mesh.geometry, _options(onProgress)),
    catch: (defect) => new GlbFault({ case: { reason: "decode-refused", mesh: `${key}`, cause: String(defect) } }),
  })

const _trees = (stamp: number, held: HashMap.HashMap<string, Glb.Resident>): Glb.Trees => ({
  stamp,
  held,
  keyOf: (node) => (HashMap.has(held, node.uuid) ? Option.some(node.uuid) : Option.none()),
})

const _accelerate = (
  key: Digest.Key<"content">,
  meshes: ReadonlyArray<Mesh>,
  snapshots: GlbViewport.Snapshots,
  pool: ParallelMeshBVHWorker,
  onProgress: (fraction: number) => void,
): Effect.Effect<ReadonlyArray<Glb.Resident>, GlbFault> =>
  Effect.gen(function* () {
    const cached = Option.filter(yield* snapshots.read(key), (snapshot) => snapshot.length === meshes.length)
    const built = yield* Option.match(cached, {
      onSome: (snapshot) =>
        Effect.sync(() => meshes.map((mesh, at) => MeshBVH.deserialize(snapshot[at]!, mesh.geometry, { setIndex: !_BVH.indirect }))),
      onNone: () => Effect.forEach(meshes, (mesh) => _built(key, mesh, pool, onProgress), { concurrency: 1 }),
    })
    yield* Option.isNone(cached) ? snapshots.write(key, built.map((tree) => MeshBVH.serialize(tree))) : Effect.void
    return yield* Effect.sync(() =>
      meshes.map((mesh, at) => {
        const tree = built[at]!
        mesh.geometry.boundsTree = tree
        return { tree, node: mesh } satisfies Glb.Resident
      }))
  })

const _splat = (graft: Glb.Graft, acquired: Glb.Acquired): Effect.Effect<void> =>
  acquired.renderer instanceof WebGPURenderer
    ? Effect.sync(() => {
        graft.drawn.forEach((drawn) => {
          if (!(drawn instanceof Points)) return
          const bound = new PointsNodeMaterial({ transparent: true, depthWrite: false })
          bound.colorNode = vertexColor()
          const worn = Array.isArray(drawn.material) ? drawn.material : [drawn.material]
          drawn.material = bound
          _retire(worn)
        })
      })
    : Effect.void

const _slotted = (drawn: Glb.Drawable): ReadonlyArray<readonly [at: number, material: Material]> =>
  Array.isArray(drawn.material) ? drawn.material.map((row, at) => [at, row] as const) : [[-1, drawn.material] as const]

const _repoint = (drawn: Glb.Drawable, at: number, bound: MeshPhysicalMaterial): void => {
  if (at < 0) drawn.material = bound
  else if (Array.isArray(drawn.material)) drawn.material[at] = bound
}

const _dressed = (
  key: Digest.Key<"content">,
  drawn: Glb.Drawable,
  at: number,
  material: Material,
  bound: Pbr.Bound,
  planes: Context.Tag.Service<GlbViewport>["planes"],
  acquired: Glb.Acquired,
): Effect.Effect<Option.Option<GlbSeating>> =>
  material instanceof MeshPhysicalMaterial
    ? Pbr.seat(material, bound, planes, acquired)
    : material instanceof MeshStandardMaterial && Pbr.demands(bound)
      ? Effect.flatMap(
          Effect.sync(() => {
            const upgraded = Pbr.upgrade(material)
            _repoint(drawn, at, upgraded)
            material.dispose()
            return upgraded
          }),
          (upgraded) => Pbr.seat(upgraded, bound, planes, acquired),
        )
      : Effect.succeed(Option.some(
        new GlbSeating({ issues: [{ reason: "plane-unbound", mesh: `${key}`, cause: "source material is not physical" }] }),
      ))

const _dress = (
  key: Digest.Key<"content">,
  graft: Glb.Graft,
  index: Pbr.Index,
  planes: Context.Tag.Service<GlbViewport>["planes"],
  acquired: Glb.Acquired,
  facts: Queue.Queue<Glb.ResidencyFact>,
): Effect.Effect<void> =>
  Frame.Residency.kind[graft.kind].splatBorne
    ? _splat(graft, acquired)
    : Option.match(Pbr.resolve(index, key), {
        onNone: () => Effect.void,
        onSome: (bound) =>
          Effect.flatMap(
            Effect.forEach(
              graft.drawn.flatMap((drawn) => _slotted(drawn).map(([at, material]) => [drawn, at, material] as const)),
              ([drawn, at, material]) => _dressed(key, drawn, at, material, bound, planes, acquired),
            ),
            (refusals) =>
              Effect.forEach(
                refusals,
                Option.match({
                  onNone: () => Effect.void,
                  onSome: (refusal: GlbSeating) =>
                    Effect.asVoid(Queue.offer(facts, { _tag: "Refused", refusal } as const)),
                }),
                { discard: true },
              ),
          ),
      })

const _graft = (
  root: Scene,
  backplane: Glb.Backplane,
  port: Context.Tag.Service<GlbViewport>,
  progress: (fraction: number) => void,
): Effect.Effect<Glb.Loop, never, Scope.Scope> =>
  Effect.gen(function* () {
    yield* _patched
    const pool = yield* _pool
    const held = yield* Ref.make(HashMap.empty<Digest.Key<"content">, Glb.Graft>())
    const facts = yield* Queue.bounded<Glb.ResidencyFact>(32)
    const dressed = yield* Ref.make(Option.none<Pbr.Index>())
    const accel = yield* Ref.make(_trees(0, HashMap.empty<string, Glb.Resident>()))
    const insert = Stream.runForEach(port.arrivals, (arrival) =>
      Effect.gen(function* () {
        const live = yield* port.manifest.get
        const tile = yield* Option.match(
          Option.flatMap(live, (held) =>
            arrival.epoch === held.epoch ? HashMap.get(held.index, arrival.key) : Option.none()),
          {
          onNone: () =>
            Effect.fail(new GlbFault({ case: { reason: "manifest-skew", mesh: `${arrival.key}`, cause: "the live index censuses no such arrival" } })),
          onSome: Effect.succeed,
          },
        )
        const acquired = yield* ScopedRef.get(backplane)
        const gltf = yield* Effect.tryPromise({
          try: () => acquired.codecs.gltf.parseAsync(arrival.octets.buffer, ""),
          catch: (defect) =>
            defect instanceof GlbFault
              ? defect
              : new GlbFault({ case: { reason: "decode-refused", mesh: `${arrival.key}`, cause: String(defect) } }),
        })
        const mixer = gltf.animations.length === 0
          ? Option.none<AnimationMixer>()
          : Option.some(
              gltf.animations.reduce((bound, clip) => {
                bound.clipAction(clip).setLoop(LoopRepeat, Number.POSITIVE_INFINITY).play()
                return bound
              }, new AnimationMixer(gltf.scene)),
            )
        const drawn = yield* Effect.sync(() => {
          const seen = _walk(gltf.scene, _upload(acquired.renderer))
          root.add(gltf.scene)
          return seen
        })
        const residents = yield* _accelerate(
          arrival.key,
          drawn.filter((node): node is Mesh => node instanceof Mesh),
          port.snapshots,
          pool,
          progress,
        )
        const graft = { node: gltf.scene, mixer, epoch: arrival.epoch, kind: tile.kind, drawn } satisfies Glb.Graft
        yield* Ref.update(held, HashMap.set(arrival.key, graft))
        yield* Ref.update(accel, (live) =>
          _trees(live.stamp + 1, residents.reduce((map, row) => HashMap.set(map, row.node.uuid, row), live.held)))
        yield* Option.match(yield* Ref.get(dressed), {
          onNone: () => Effect.void,
          onSome: (index) => _dress(arrival.key, graft, index, port.planes, acquired, facts),
        })
        yield* Effect.asVoid(Effect.withMetric(Effect.succeed(1), _GRAFTED))
        yield* Queue.offer(facts, { _tag: "Arrived", arrival } as const)
      }).pipe(
        _refused,
        Effect.withSpan("rasm.ui.scene.residency", { attributes: { "glb.bytes": arrival.octets.byteLength } }),
        Effect.annotateLogs({ mesh: `${arrival.key}` }),
        Effect.catchAll((refusal) => Queue.offer(facts, { _tag: "Refused", refusal } as const)),
      )))
    const evict = Stream.runForEach(port.manifest.changes, (admitted) =>
      Effect.gen(function* () {
        const grafts = yield* Ref.get(held)
        const gone = HashMap.filter(grafts, (graft, key) =>
          Option.match(admitted, {
            onNone: () => true,
            onSome: (residency) => graft.epoch !== residency.epoch || !HashMap.has(residency.index, key),
          }))
        yield* Effect.forEach(gone, ([key, graft]) =>
          Effect.zipRight(
            Effect.zipRight(
              Ref.update(held, HashMap.remove(key)),
              Ref.update(accel, (live) =>
                _trees(live.stamp + 1, graft.drawn.reduce((map, node) => HashMap.remove(map, node.uuid), live.held))),
            ),
            Effect.sync(() => {
              Option.map(graft.mixer, (live) => {
                live.stopAllAction()
                live.uncacheRoot(graft.node)
              })
              root.remove(graft.node)
              _walk(graft.node, _release)
            }),
          ))
        yield* Option.match(admitted, {
          onNone: () => Effect.void,
          onSome: (residency) =>
            Queue.offer(facts, { _tag: "Admitted", view: residency.view, epoch: residency.epoch } as const),
        })
      }))
    const wear = Stream.runForEach(port.appearances, (document) =>
      Effect.gen(function* () {
        const index = yield* Pbr.index(document)
        const acquired = yield* ScopedRef.get(backplane)
        yield* Ref.set(dressed, Option.some(index))
        yield* Effect.forEach(
          yield* Ref.get(held),
          ([key, graft]) => _dress(key, graft, index, port.planes, acquired, facts),
          { discard: true },
        )
      }).pipe(
        _refused,
        Effect.withSpan("rasm.ui.scene.residency"),
        Effect.catchAll((refusal) => Queue.offer(facts, { _tag: "Refused", refusal } as const)),
      ))
    const dome = yield* _dome(root, backplane, port.environments, port.planes, facts)
    yield* Effect.forkScoped(Effect.all([insert, evict, wear], { concurrency: 3, discard: true }))
    return {
      root,
      advance: (delta) => {
        HashMap.forEach(Effect.runSync(Ref.get(held)), (graft) =>
          Option.map(graft.mixer, (live) => live.update(delta)))
      },
      facts: Stream.fromQueue(facts),
      dome: Ref.get(dome.slot),
      trees: Ref.get(accel),
      rebind: dome.rebind,
    }
  })

const _residencyHook = (loop: Glb.Loop): Hook.Row<"rasm.ui.scene.residency"> => ({
  modality: "observe",
  depth: 32,
  source: Option.some(loop.facts),
})

const _MESHOPT = "EXT_meshopt_compression"

const _Required = Schema.Struct({ extensionsRequired: Schema.optional(Schema.Array(Schema.String)) })

const _gate = (admitted: boolean) => (parser: GLTFParser): GLTFLoaderPlugin => ({
  name: "rasm.codec.gate",
  beforeRoot: () =>
    admitted ||
      !Option.match(Schema.decodeUnknownOption(_Required)(parser.json), {
        onNone: () => false,
        onSome: (row) => (row.extensionsRequired ?? []).includes(_MESHOPT),
      })
      ? null
      : Promise.reject(new GlbFault({ case: { reason: "codec-absent", mesh: _MESHOPT, absent: "meshopt-codec" } })),
})

const _codecSlugs = ["draco", "ktx2", "meshopt"] as const

const _addresses = ["dir", "file"] as const

const _warms = ["fetch", "module", "script"] as const

const _ADDRESS = { dir: _assetDir, file: _assetPath } as const satisfies {
  readonly [K in Glb.Address]: (asset: Glb.AssetIdentity) => string
}

const _CODECS = {
  draco: { address: "dir", warm: "fetch" },
  ktx2: { address: "dir", warm: "fetch" },
  meshopt: { address: "file", warm: "module" },
} as const satisfies { readonly [K in Glb.Codec]: Glb.CodecRow }

const _WARM = { crossOrigin: "anonymous", priority: "high" } as const satisfies {
  crossOrigin: NonNullable<PreloadOptions["crossOrigin"]>
  priority: NonNullable<PreloadOptions["fetchPriority"]>
}

const _HINTS: { readonly [K in Glb.Warm]: (href: string) => void } = {
  fetch: (href) => preload(href, { as: "fetch", crossOrigin: _WARM.crossOrigin, fetchPriority: _WARM.priority }),
  module: (href) => preinitModule(href, { as: "script", crossOrigin: _WARM.crossOrigin }),
  script: (href) => preinit(href, { as: "script", crossOrigin: _WARM.crossOrigin }),
}

const _warm = (
  roster: Glb.AssetRoster,
  residency: GlbViewport.Residency,
  addressed: GlbViewport.Addressed,
): Effect.Effect<void> =>
  Effect.zipRight(
    Effect.forEach(
      Record.toEntries(_CODECS),
      ([slug, row]) =>
        Effect.flatMap(Effect.option(_asset(roster, slug)), (found) =>
          Effect.sync(() => void Option.map(found, (asset) => _HINTS[row.warm](_ADDRESS[row.address](asset))))),
      { discard: true },
    ),
    Effect.sync(() =>
      void Array.fromIterable(HashMap.keys(residency.index)).forEach((key) => Option.map(addressed(key), _HINTS.fetch))),
  )

const _served = (
  roster: Glb.AssetRoster,
  taps: {
    readonly progress: (url: string, loaded: number, total: number) => void
    readonly error: (url: string) => void
  },
): Effect.Effect<Glb.Served, GlbFault, Scope.Scope> =>
  Effect.gen(function* () {
    const draco = yield* _asset(roster, "draco")
    const ktx2 = yield* _asset(roster, "ktx2")
    const decoder = yield* Option.match(yield* Effect.option(_asset(roster, "meshopt")), {
      onNone: () => Effect.succeedNone,
      onSome: (row) =>
        Effect.map(
          Effect.tryPromise({
            try: (): Promise<{ readonly MeshoptDecoder: typeof MeshoptDecoder }> =>
              import(_ADDRESS[_CODECS.meshopt.address](row)),
            catch: () => new GlbFault({ case: { reason: "codec-absent", mesh: row.slug, absent: "decoder-module" } }),
          }),
          (module) => Option.some(module.MeshoptDecoder),
        ),
    })
    return yield* Effect.acquireRelease(
      Effect.sync(() => {
        const manager = new LoadingManager()
        manager.onProgress = taps.progress
        manager.onError = taps.error
        return {
          manager,
          draco: new DRACOLoader(manager).setDecoderPath(_ADDRESS[_CODECS.draco.address](draco)).preload(),
          ktx2Dir: _ADDRESS[_CODECS.ktx2.address](ktx2),
          meshopt: decoder,
        } satisfies Glb.Served
      }),
      (row) => Effect.sync(() => row.draco.dispose()),
    )
  })

const _codecs = (served: Glb.Served, renderer: WebGLRenderer | WebGPURenderer): Effect.Effect<Glb.Codecs> =>
  Effect.sync(() => {
    const basis = new KTX2Loader(served.manager).setTranscoderPath(served.ktx2Dir).detectSupport(renderer)
    const built = new GLTFLoader(served.manager)
      .setDRACOLoader(served.draco)
      .setKTX2Loader(basis)
      .register(_gate(Option.isSome(served.meshopt)))
    return {
      gltf: Option.match(served.meshopt, { onNone: () => built, onSome: (row) => built.setMeshoptDecoder(row) }),
      ktx2: basis,
    } satisfies Glb.Codecs
  })

const _loop = (
  renderer: WebGLRenderer | WebGPURenderer,
  scene: Scene,
  camera: PerspectiveCamera,
  live: Glb.Loop,
  frames: SubscriptionRef.SubscriptionRef<Clock.Frame>,
  shown: Stream.Stream<boolean>,
): Effect.Effect<void, never, Scope.Scope> =>
  Effect.gen(function* () {
    const draw = (): void => {
      live.advance(Effect.runSync(SubscriptionRef.get(frames)).delta)
      renderer.render(scene, camera)
    }
    yield* Effect.addFinalizer(() => Effect.sync(() => void renderer.setAnimationLoop(null)))
    yield* Effect.forkScoped(
      Stream.runForEach(shown, (visible) => Effect.sync(() => void renderer.setAnimationLoop(visible ? draw : null))),
    )
  })
```

## [06]-[ENVIRONMENT_FOLD]

- Owner: `Glb.dome` decodes and prefilters one content-keyed environment under the active backplane.
- Law: repeated keys update policy without duplicating GPU resources; backend replacement rebinds the same source.

```typescript signature
const TextureVocab = Wire.Texture
import * as appearance from "@rasm\/contracts/rasm/contracts/appearance/appearance_pb"
import { Effect, Match, Option, Queue, Ref, Scope, ScopedRef, Stream } from "effect"
import {
  EquirectangularReflectionMapping, Euler, HalfFloatType, LinearFilter, LinearMipmapLinearFilter,
  LinearSRGBColorSpace, NoColorSpace, Quaternion, SphericalHarmonics3, SRGBColorSpace, Texture, Vector3,
} from "three"
import type {
  ColorSpace, DataTexture, MagnificationTextureFilter, MinificationTextureFilter, Scene, TextureDataType,
} from "three"
import { RoomEnvironment } from "three/addons/environments/RoomEnvironment.js"
import { EXRLoader } from "three/addons/loaders/EXRLoader.js"
import { HDRLoader } from "three/addons/loaders/HDRLoader.js"

declare namespace Glb {
  type Dome = {
    readonly key: Option.Option<Digest.Key<"content">>
    readonly source: Option.Option<Texture>
    readonly target: { readonly texture: Texture; dispose(): void }
    readonly sh: SphericalHarmonics3
    readonly intensity: number
    readonly rotation: number
    readonly basis: Quaternion
  }
  type Container = {
    readonly probe: Option.Option<(octets: Uint8Array<ArrayBuffer>) => boolean>
    readonly decode: Option.Option<(payload: Glb.Payload, acquired: Glb.Acquired) => Effect.Effect<Texture, GlbFault>>
  }
  type Payload = Pick<GlbViewport.Arrival, "key" | "octets">
  type Slot = {
    readonly slot: Ref.Ref<Glb.Dome>
    readonly rebind: (acquired: Glb.Acquired) => Effect.Effect<void>
  }
}

type _SceneTransfer = (typeof TextureVocab.rows.channel)[TextureVocab.Role]["transfer"]
const _TRANSFER = {
  [appearance.Transfer.SRGB]: SRGBColorSpace,
  [appearance.Transfer.LINEAR]: LinearSRGBColorSpace,
  [appearance.Transfer.RAW]: NoColorSpace,
} as const satisfies { readonly [K in _SceneTransfer]: ColorSpace }
const _sceneTransfer = (transfer: TextureVocab.Transfer): transfer is keyof typeof _TRANSFER =>
  Object.hasOwn(_TRANSFER, transfer)

const _ENV = {
  type: HalfFloatType,
  transfer: appearance.Transfer.LINEAR,
  backdrop: true,
  blur: 0,
  floorBlur: 0.04,
  bands: 27,
  filter: { mipped: LinearMipmapLinearFilter, flat: LinearFilter },
} as const satisfies {
  type: TextureDataType
  transfer: keyof typeof _TRANSFER
  backdrop: boolean
  blur: number
  floorBlur: number
  bands: number
  filter: { mipped: MinificationTextureFilter; flat: MagnificationTextureFilter }
}

const _filtered = (texture: Texture, transfer: keyof typeof _TRANSFER): Texture => {
  texture.minFilter = texture.mipmaps.length > 1 ? _ENV.filter.mipped : _ENV.filter.flat
  texture.magFilter = _ENV.filter.flat
  texture.colorSpace = _TRANSFER[transfer]
  return texture
}

const _KTX2 = [0xab, 0x4b, 0x54, 0x58, 0x20, 0x32, 0x30, 0xbb, 0x0d, 0x0a, 0x1a, 0x0a] as const

const _plane =
  (open: () => { createDataTexture(buffer: ArrayBuffer): DataTexture }) =>
  (payload: Glb.Payload): Effect.Effect<Texture, GlbFault> =>
    Effect.try({
      try: () => open().createDataTexture(payload.octets.buffer),
      catch: (defect) => new GlbFault({ case: { reason: "decode-refused", mesh: `${payload.key}`, cause: String(defect) } }),
    })

const _bitmap =
  (mime: string) =>
  (payload: Glb.Payload): Effect.Effect<Texture, GlbFault> =>
    Effect.map(
      Effect.tryPromise({
        try: () =>
          globalThis.createImageBitmap(new Blob([payload.octets], { type: mime }), {
            colorSpaceConversion: "none",
            premultiplyAlpha: "none",
            imageOrientation: "none",
          }),
        catch: (defect) => new GlbFault({ case: { reason: "decode-refused", mesh: `${payload.key}`, cause: String(defect) } }),
      }),
      (bitmap) => {
        const texture = new Texture(bitmap)
        texture.flipY = false
        texture.needsUpdate = true
        return texture
      },
    )

const _deep = (payload: Glb.Payload, acquired: Glb.Acquired): Effect.Effect<Texture, GlbFault> =>
  Effect.async<Texture, GlbFault>((resume) =>
    acquired.codecs.ktx2.parse(
      payload.octets.buffer,
      (texture) => resume(Effect.succeed(texture)),
      (defect) =>
        resume(Effect.fail(new GlbFault({ case: { reason: "decode-refused", mesh: `${payload.key}`, cause: String(defect) } }))),
    ))

const _CONTAINERS: { readonly [K in TextureVocab.Container]: Glb.Container } = {
  [appearance.Container.EXR]: {
    probe: Option.some((o: Uint8Array<ArrayBuffer>): boolean =>
      o.byteLength >= 4 && new DataView(o.buffer, o.byteOffset, 4).getUint32(0, true) === 20000630),
    decode: Option.some(_plane(() => new EXRLoader().setDataType(_ENV.type))),
  },
  [appearance.Container.HDR]: {
    probe: Option.some((o: Uint8Array<ArrayBuffer>): boolean => o.byteLength >= 2 && o[0] === 0x23 && o[1] === 0x3f),
    decode: Option.some(_plane(() => new HDRLoader().setDataType(_ENV.type))),
  },
  [appearance.Container.KTX2]: {
    probe: Option.some((o: Uint8Array<ArrayBuffer>): boolean =>
      o.byteLength >= _KTX2.length && _KTX2.every((byte, at) => o[at] === byte)),
    decode: Option.some(_deep),
  },
  [appearance.Container.PNG16]: { probe: Option.none(), decode: Option.some(_bitmap("image/png")) },
  [appearance.Container.WEBP]: { probe: Option.none(), decode: Option.some(_bitmap("image/webp")) },
  [appearance.Container.AVIF12]: { probe: Option.none(), decode: Option.some(_bitmap("image/avif")) },
  [appearance.Container.TIFF16]: { probe: Option.none(), decode: Option.none() },
  [appearance.Container.TIFF_F32]: { probe: Option.none(), decode: Option.none() },
  [appearance.Container.EXR_DEEP]: { probe: Option.none(), decode: Option.none() },
  [appearance.Container.JXL]: { probe: Option.none(), decode: Option.none() },
  [appearance.Container.JXL_F16]: { probe: Option.none(), decode: Option.none() },
  [appearance.Container.QOI]: { probe: Option.none(), decode: Option.none() },
}

const _sniff = (octets: Uint8Array<ArrayBuffer>): Option.Option<Glb.Container> =>
  Option.fromNullable(
    Object.values<Glb.Container>(_CONTAINERS).find((row) => Option.exists(row.probe, (probe) => probe(octets))),
  )

const _decoded = (
  row: Glb.Container,
  payload: Glb.Payload,
  acquired: Glb.Acquired,
  transfer: keyof typeof _TRANSFER,
  reason: Extract<GlbFault.Reason, "decode-refused" | "plane-unbound">,
): Effect.Effect<Texture, GlbFault> =>
  Option.match(row.decode, {
    onNone: () => Effect.fail(new GlbFault({ case: { reason, mesh: `${payload.key}`, cause: "the container row declares no decoder" } })),
    onSome: (decode) => Effect.map(decode(payload, acquired), (texture) => _filtered(texture, transfer)),
  })

const _orient = (rotation: number): Euler => new Euler(-Math.PI / 2, rotation, 0, "YXZ")

const _harmonics = (sh9: ReadonlyArray<number>): Option.Option<SphericalHarmonics3> =>
  sh9.length === _ENV.bands ? Option.some(new SphericalHarmonics3().fromArray(sh9)) : Option.none()

const _irradiance = (dome: Glb.Dome, normal: Vector3, target: Vector3): Vector3 =>
  dome.sh.getIrradianceAt(target.copy(normal).applyQuaternion(dome.basis), target).multiplyScalar(dome.intensity)

const _floor = (prefilter: Glb.Prefilter): Effect.Effect<{ readonly texture: Texture; dispose(): void }> =>
  Effect.sync(() => {
    const room = new RoomEnvironment()
    const baked = prefilter.fromScene(room, _ENV.floorBlur)
    room.dispose()
    return baked
  })

const _bind = (
  root: Scene,
  acquired: Glb.Acquired,
  held: Option.Option<Glb.Dome>,
  next: Omit<Glb.Dome, "basis">,
): Effect.Effect<Glb.Dome> =>
  Effect.sync(() => {
    const orientation = _orient(next.rotation)
    Option.map(next.source, (plane) => {
      plane.mapping = EquirectangularReflectionMapping
      acquired.renderer.initTexture(plane)
    })
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

type _EnvironmentSet = Extract<Wire.Set["product"], { readonly case: "environment" }>["value"]
type _EnvironmentIbl = Extract<_EnvironmentSet["product"], { readonly case: "ibl" }>["value"]
type _EnvironmentSource = _EnvironmentIbl["source"]
type _EnvironmentPlane = _EnvironmentSource["equirect"]

const _environment = (set: Wire.Set): Effect.Effect<{
  readonly source: _EnvironmentSource
  readonly products: ReadonlyArray<_EnvironmentPlane>
}, GlbFault> =>
  Match.value(set.product).pipe(Match.discriminatorsExhaustive("case")({
    pbr: () => Effect.fail(new GlbFault({ case: { reason: "manifest-skew", mesh: "environment", cause: "pbr set reached the dome stream" } })),
    baked: () => Effect.fail(new GlbFault({ case: { reason: "manifest-skew", mesh: "environment", cause: "baked set reached the dome stream" } })),
    environment: ({ value }) => Match.value(value.product).pipe(Match.discriminatorsExhaustive("case")({
      hdri: ({ value: hdri }) => Effect.succeed({
        source: hdri.source,
        products: [hdri.source.equirect, ...(hdri.source.cubemap === undefined ? [] : [hdri.source.cubemap]), ...(hdri.source.preview === undefined ? [] : [hdri.source.preview])],
      }),
      ibl: ({ value: ibl }) => Effect.succeed({
        source: ibl.source,
        products: [
          ibl.source.equirect,
          ...(ibl.source.cubemap === undefined ? [] : [ibl.source.cubemap]),
          ...(ibl.source.preview === undefined ? [] : [ibl.source.preview]),
          ...ibl.specular,
          ibl.brdfLut,
          ...(ibl.luminanceCdf === undefined ? [] : [ibl.luminanceCdf]),
        ],
      }),
    })),
  }))

const _dome = (
  root: Scene,
  backplane: Glb.Backplane,
  arrivals: Stream.Stream<GlbViewport.Environment, GlbFault>,
  planes: Context.Tag.Service<GlbViewport>["planes"],
  facts: Queue.Queue<Glb.ResidencyFact>,
): Effect.Effect<Glb.Slot, never, Scope.Scope> =>
  Effect.gen(function* () {
    const booted = yield* ScopedRef.get(backplane)
    const baked = yield* _floor(booted.prefilter)
    const floor = yield* _bind(root, booted, Option.none(), {
      key: Option.none<Digest.Key<"content">>(),
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
          dome.target.dispose()
          Option.map(dome.source, (source) => source.dispose())
        })))
    yield* Effect.forkScoped(Effect.promise(async () => await booted.prefilter.compileEquirectangularShader()))
    yield* Effect.forkScoped(Stream.runForEach(arrivals, (arrival) =>
      Effect.gen(function* () {
        const held = yield* Ref.get(slot)
        const acquired = yield* ScopedRef.get(backplane)
        const document = yield* _environment(arrival)
        const setKey = yield* _content(arrival.key, "<environment-set>")
        const references = yield* Effect.forEach(document.products, (product) =>
          Schema.decode(TextureVocab.reference)(product.plane).pipe(
            Effect.mapError((issue) => new GlbFault({ case: { reason: "manifest-skew", mesh: product.plane.file, cause: issue.message } })),
            Effect.flatMap((reference) => Effect.map(planes(reference.artifact), (octets) => ({ product, reference, octets }))),
          )), { concurrency: 1 })
        const equirect = yield* Option.match(Array.head(references), {
          onNone: () => Effect.fail(new GlbFault({ case: { reason: "manifest-skew", mesh: `${setKey}`, cause: "environment carries no equirect product" } })),
          onSome: Effect.succeed,
        })
        const sh = yield* Option.match(_harmonics(document.source.sh9), {
          onNone: () =>
            Effect.fail(new GlbFault({ case: { reason: "decode-refused", mesh: `${setKey}`, cause: "sh9 carries the wrong coefficient count" } })),
          onSome: Effect.succeed,
        })
        const handles = yield* (Option.exists(held.key, (key) => key === setKey)
          ? Effect.succeed({ source: held.source, target: held.target })
          : Effect.gen(function* () {
              const transfer = yield* _sceneTransfer(equirect.product.transfer)
                ? Effect.succeed(equirect.product.transfer)
                : Effect.fail(new GlbFault({ case: { reason: "decode-refused", mesh: `${setKey}`, cause: "environment transfer reaches no renderer space" } }))
              const source = yield* _decoded(
                _CONTAINERS[equirect.product.container],
                { key: equirect.reference.artifact.artifactId, octets: equirect.octets },
                acquired,
                transfer,
                "decode-refused",
              )
              return yield* Effect.sync(() => ({
                source: Option.some(source),
                target: acquired.prefilter.fromEquirectangular(source),
              }))
            }))
        const dome = yield* _bind(root, acquired, Option.some(held), {
          key: Option.some(setKey),
          source: handles.source,
          target: handles.target,
          sh,
          intensity: document.source.intensity,
          rotation: document.source.rotation,
        })
        yield* Ref.set(slot, dome)
        yield* Queue.offer(facts, { _tag: "Environment", key: setKey } as const)
      }).pipe(
        _refused,
        Effect.withSpan("rasm.ui.scene.residency"),
        Effect.annotateLogs({ mesh: "environment" }),
        Effect.catchAll((refusal) => Queue.offer(facts, { _tag: "Refused", refusal } as const)),
      )))
    return { slot, rebind: _rebind(root, slot) }
  })
```

## [07]-[DRAW_COLLAPSE]

- Owner: `Glb.draw` collapses merge, instancing, batching, visibility, tint, culling, splat order, and the scene-tool queries into keyed rows.
- Law: `Frame.Residency.kind` selects splat and culling posture once per graft.
- Law: every per-view fold is a `Glb.Pass` over the camera — the cluster cull and the splat sort share one shape, so a frame driver folds one roster and no surface runs a private loop.
- Law: ordering a splat composite is the CONSUMER's — the producer wire carries no ordering key, so back-to-front is re-derived on the camera epoch and never read off decode order.
- Law: the tool rows carry no fault channel — a plane cutting nothing and a probe reaching no surface are absences the return spells, and the closed `GlbFault` roster names no query reason a tool miss could take without forking the family.
- Boundary: `panel#CONTROL_SINKS` routes its `section` and `measure` sinks onto these rows; the world-space bake they descend belongs to the asking scope, never the residency manifest.

```typescript signature
import { Array, Effect, Function, Option, type Scope } from "effect"
import {
  BatchedMesh, BufferAttribute, Color, DynamicDrawUsage, Frustum, InstancedMesh, Line3, LinearSRGBColorSpace,
  Matrix4, Plane as CutPlane, Sphere, Vector3, Vector4,
} from "three"
import type {
  BufferGeometry, InterleavedBufferAttribute, Material, Object3D, PerspectiveCamera, Points,
} from "three"
import { mergeGeometries } from "three/addons/utils/BufferGeometryUtils.js"
import { bool, Fn, instancedArray, instanceIndex, select, uint, uniform } from "three/tsl"
import { WebGPURenderer } from "three/webgpu"
import type { StorageBufferNode, UniformNode } from "three/webgpu"
import { INTERSECTED, MeshBVH, NOT_INTERSECTED, StaticGeometryGenerator } from "three-mesh-bvh"
import type { HitPointInfo } from "three-mesh-bvh"

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

const _visible = (batch: BatchedMesh, slots: ReadonlyArray<readonly [instance: number, shown: boolean]>): void => {
  Array.forEach(slots, ([instance, shown]) => void batch.setVisibleAt(instance, shown))
}

const _tinted = (
  batch: BatchedMesh,
  slots: ReadonlyArray<readonly [instance: number, linear: readonly [number, number, number]]>,
): void => {
  const ink = new Color()
  Array.forEach(slots, ([instance, linear]) =>
    void batch.setColorAt(instance, ink.setRGB(linear[0], linear[1], linear[2], LinearSRGBColorSpace)))
}

const _PLANES = 6

const _CULL = { workgroup: [64, 1, 1] } as const satisfies { workgroup: ReadonlyArray<number> }

declare namespace Glb {
  type Pass = (camera: PerspectiveCamera) => Effect.Effect<void>
  type Cluster = {
    readonly count: number
    readonly bounds: StorageBufferNode<"vec4">
    readonly verdict: StorageBufferNode<"uint">
    readonly planes: ReadonlyArray<UniformNode<"vec4", Vector4>>
  }
  type Hit = HitPointInfo
  type Cut = ReadonlyArray<Line3>
  type Span = { readonly from: Glb.Hit; readonly to: Glb.Hit; readonly length: number }
}

const _clustered = (batch: BatchedMesh, draft: Float32Array): void => {
  const sphere = new Sphere()
  const matrix = new Matrix4()
  for (let slot = 0; slot < batch.instanceCount; slot += 1) {
    batch.getBoundingSphereAt(batch.getGeometryIdAt(slot), sphere)
    batch.getMatrixAt(slot, matrix)
    sphere.center.applyMatrix4(matrix)
    draft.set([sphere.center.x, sphere.center.y, sphere.center.z, sphere.radius * matrix.getMaxScaleOnAxis()], slot * 4)
  }
}

const _framed = (
  camera: PerspectiveCamera,
  renderer: WebGPURenderer,
  frustum: Frustum,
  view: Matrix4,
  planes: Glb.Cluster["planes"],
): void => {
  frustum.setFromProjectionMatrix(view.multiplyMatrices(camera.projectionMatrix, camera.matrixWorldInverse), renderer.coordinateSystem)
  planes.forEach((slot, at) => {
    const plane = frustum.planes[at]!
    slot.value.set(plane.normal.x, plane.normal.y, plane.normal.z, plane.constant)
  })
}

const _verdicts = (batch: BatchedMesh, verdict: Uint32Array): ReadonlyArray<readonly [instance: number, shown: boolean]> => {
  const rows: Array<readonly [number, boolean]> = []
  for (let slot = 0; slot < batch.instanceCount; slot += 1) rows.push([slot, verdict[slot] === 1] as const)
  return rows
}

const _kernel = (cluster: Glb.Cluster) =>
  Fn(() => {
    const sphere = cluster.bounds.element(instanceIndex).toVar()
    const inside = cluster.planes.reduce(
      (held, plane) => held.and(plane.xyz.dot(sphere.xyz).add(plane.w).greaterThanEqual(sphere.w.negate())),
      bool(true),
    )
    cluster.verdict.element(instanceIndex).assign(select(inside, uint(1), uint(0)))
  })().compute(cluster.count, [..._CULL.workgroup])

const _cull = (batch: BatchedMesh, acquired: Glb.Acquired): Effect.Effect<Glb.Pass, never, Scope.Scope> =>
  Option.match(
    Option.liftPredicate(acquired.renderer, (renderer): renderer is WebGPURenderer => renderer instanceof WebGPURenderer),
    {
      onNone: () => Effect.succeed<Glb.Pass>(() => Effect.void),
      onSome: (renderer) =>
        Effect.map(
          Effect.acquireRelease(
            Effect.sync(() => {
              const count = batch.maxInstanceCount
              const draft = new Float32Array(count * 4)
              const store = new ArrayBuffer(count * Uint32Array.BYTES_PER_ELEMENT)
              const cluster: Glb.Cluster = {
                count,
                bounds: instancedArray(draft, "vec4"),
                verdict: instancedArray(new Uint32Array(store), "uint"),
                planes: Array.makeBy(_PLANES, () => uniform(new Vector4())),
              }
              return { cluster, draft, store, kernel: _kernel(cluster), frustum: new Frustum(), view: new Matrix4() }
            }),
            ({ cluster }) =>
              Effect.sync(() => void [cluster.bounds.value, cluster.verdict.value].forEach((held) => held.dispose())),
          ),
          ({ cluster, draft, frustum, kernel, store, view }) => (camera) =>
            Effect.gen(function* () {
              yield* Effect.sync(() => {
                _framed(camera, renderer, frustum, view, cluster.planes)
                _clustered(batch, draft)
                cluster.bounds.value.needsUpdate = true
              })
              yield* Effect.promise(() => renderer.computeAsync(kernel, cluster.count))
              const read = yield* Effect.promise(() => renderer.getArrayBufferAsync(cluster.verdict.value, store))
              yield* Effect.sync(() => _visible(batch, _verdicts(batch, new Uint32Array(read))))
            }),
        ),
    },
  )

const _baked = (subtree: Object3D): Effect.Effect<{ readonly geometry: BufferGeometry; readonly tree: MeshBVH }, never, Scope.Scope> =>
  Effect.acquireRelease(
    Effect.sync(() => {
      const generator = new StaticGeometryGenerator(subtree)
      generator.applyWorldTransforms = true
      const geometry = generator.generate()
      return { geometry, tree: new MeshBVH(geometry, _options(Function.constVoid)) }
    }),
    (baked) => Effect.sync(() => void baked.geometry.dispose()),
  )

// --- [SPLAT_ORDER]

const _SPLAT = { buckets: 1 << 16, epoch: 0.9999 } as const satisfies { buckets: number; epoch: number }

const _depths = (
  position: BufferAttribute | InterleavedBufferAttribute,
  axis: Vector3,
  depths: Float32Array,
): void => {
  for (let point = 0; point < depths.length; point += 1) {
    depths[point] = position.getX(point) * axis.x + position.getY(point) * axis.y + position.getZ(point) * axis.z
  }
}

const _ordered = (depths: Float32Array, keys: Uint16Array, census: Uint32Array, order: Uint32Array): void => {
  let low = depths[0] ?? 0
  let high = low
  for (let at = 0; at < depths.length; at += 1) {
    const depth = depths[at] ?? 0
    low = depth < low ? depth : low
    high = depth > high ? depth : high
  }
  const scale = high > low ? (_SPLAT.buckets - 1) / (high - low) : 0
  census.fill(0)
  for (let at = 0; at < depths.length; at += 1) {
    const key = Math.round(((depths[at] ?? 0) - low) * scale)
    keys[at] = key
    census[key] = (census[key] ?? 0) + 1
  }
  let cursor = 0
  for (let key = _SPLAT.buckets - 1; key >= 0; key -= 1) {
    const held = census[key] ?? 0
    census[key] = cursor
    cursor += held
  }
  for (let at = 0; at < depths.length; at += 1) {
    const key = keys[at] ?? 0
    order[census[key] ?? 0] = at
    census[key] = (census[key] ?? 0) + 1
  }
}

const _sorted = (points: Points): Effect.Effect<Glb.Pass> =>
  Effect.sync(() => {
    const position = points.geometry.getAttribute("position")
    const depths = new Float32Array(position.count)
    const keys = new Uint16Array(position.count)
    const census = new Uint32Array(_SPLAT.buckets)
    const order = new Uint32Array(position.count)
    const index = new BufferAttribute(order, 1)
    index.setUsage(DynamicDrawUsage)
    points.geometry.setIndex(index)
    const axis = new Vector3()
    const held = new Vector3()
    return (camera) =>
      Effect.sync(() => {
        camera.getWorldDirection(axis)
        if (held.dot(axis) > _SPLAT.epoch) return
        held.copy(axis)
        _depths(position, axis, depths)
        _ordered(depths, keys, census, order)
        index.needsUpdate = true
      })
  })

// --- [TOOL_QUERY]

const _section = (tree: MeshBVH, plane: CutPlane): Effect.Effect<Glb.Cut> =>
  Effect.sync(() => {
    const cut: Array<Line3> = []
    const edge = new Line3()
    const crossed = new Vector3()
    tree.shapecast({
      intersectsBounds: (box) => (plane.intersectsBox(box) ? INTERSECTED : NOT_INTERSECTED),
      intersectsTriangle: (triangle) => {
        const corners = [triangle.a, triangle.b, triangle.c] as const
        const met: Array<Vector3> = []
        corners.forEach((corner, at) => {
          edge.start.copy(corner)
          edge.end.copy(corners[(at + 1) % corners.length]!)
          if (plane.intersectLine(edge, crossed) !== null) met.push(crossed.clone())
        })
        if (met.length === 2 && !met[0]!.equals(met[1]!)) cut.push(new Line3(met[0]!, met[1]!))
      },
    })
    return cut
  })

const _nearest = (tree: MeshBVH, point: Vector3): Effect.Effect<Option.Option<Glb.Hit>> =>
  Effect.sync(() => Option.fromNullable(tree.closestPointToPoint(point)))

const _measure = (tree: MeshBVH, from: Vector3, to: Vector3): Effect.Effect<Option.Option<Glb.Span>> =>
  Effect.map(Effect.all([_nearest(tree, from), _nearest(tree, to)]), ([start, end]) =>
    Option.map(Option.all([start, end]), ([head, tail]) =>
      ({ from: head, to: tail, length: head.point.distanceTo(tail.point) }) satisfies Glb.Span))
```

## [08]-[APPEARANCE_BIND]

- Owner: `Pbr.index` admits Material bytes through `Wire.decode("Material", bytes)`; `Pbr` then binds decoded OpenPBR values and addressed texture planes to declared Three.js slots.
- Law: one role table owns channel seating, color transfer, packing, refusal evidence, and which slots exist only on the physical class.
- Law: the widening upgrade is a NARROW copy through the standard prototype — the two idioms three appears to offer both destroy the target.
- Law: alpha association is a blend fact the material owns; `Texture.premultiplyAlpha` reaches the browser-decoded legs alone — where the decode already demanded straight alpha and the flag stays at its false default — and is inert on every ArrayBufferView upload, so no leg multiplies and association lands on the material.

```typescript signature
import { Array, Effect, HashMap, Match, Option, Schema } from "effect"
import {
  ClampToEdgeWrapping, DoubleSide, FrontSide, LinearSRGBColorSpace, MeshPhysicalMaterial, MeshStandardMaterial,
  RepeatWrapping, type Color, type Texture as Plane,
} from "three"

declare namespace Pbr {
  type Surface =
    | Extract<Wire.Set["product"], { readonly case: "pbr" }>["value"]
    | Extract<Wire.Set["product"], { readonly case: "baked" }>["value"]["surface"]
  type Bound = {
    readonly appearance: Digest.Key<"content">
    readonly row: AppearanceRow
    readonly override: Option.Option<Wire.Material>
    readonly set: Option.Option<Surface>
  }
  type Index = {
    readonly seats: HashMap.HashMap<Digest.Key<"content">, Pbr.Seat>
    readonly overrides: HashMap.HashMap<Digest.Key<"content">, Wire.Material>
    readonly worn: HashMap.HashMap<Digest.Key<"content">, Digest.Key<"content">>
  }
  type Seat = { readonly row: AppearanceRow; readonly set: Option.Option<Surface> }
  type RoleSlot = {
    readonly slot: keyof MeshPhysicalMaterial | null
    readonly component: 1 | 2 | 3 | 4 | null
    readonly scalar: keyof MeshPhysicalMaterial | null
    readonly physical: boolean
  }
  type Seating = {
    readonly plane: Surface["planes"][number]["levels"][number]
    readonly container: TextureVocab.Container
    readonly format: TextureVocab.PlaneFormat
    readonly transfer: Option.Option<keyof typeof _TRANSFER>
    readonly pack: Option.Option<TextureVocab.Pack>
    readonly present: ReadonlyArray<TextureVocab.Role>
    readonly absent: ReadonlyArray<TextureVocab.Role>
  }
  type Shape = {
    readonly roles: typeof _ROLE_SLOT
    readonly index: typeof _index
    readonly resolve: typeof _resolve
    readonly demands: typeof _demands
    readonly upgrade: typeof _upgraded
    readonly seat: typeof _seat
  }
}

const _ROLE_SLOT: { readonly [K in TextureVocab.Role]: Pbr.RoleSlot } = {
  [appearance.Role.BASE_WEIGHT]: { slot: null, component: null, scalar: null, physical: false },
  [appearance.Role.BASE_COLOR]: { slot: "map", component: 4, scalar: "color", physical: false },
  [appearance.Role.BASE_METALNESS]: { slot: "metalnessMap", component: 3, scalar: "metalness", physical: false },
  [appearance.Role.BASE_DIFFUSE_ROUGHNESS]: { slot: null, component: null, scalar: null, physical: false },
  [appearance.Role.BASE_SPECULAR_TINT]: { slot: null, component: null, scalar: null, physical: false },
  [appearance.Role.SPECULAR_WEIGHT]: { slot: "specularIntensityMap", component: 4, scalar: "specularIntensity", physical: true },
  [appearance.Role.SPECULAR_COLOR]: { slot: "specularColorMap", component: 3, scalar: "specularColor", physical: true },
  [appearance.Role.SPECULAR_ROUGHNESS]: { slot: "roughnessMap", component: 2, scalar: "roughness", physical: false },
  [appearance.Role.SPECULAR_ROUGHNESS_ANISOTROPY]: { slot: "anisotropyMap", component: 3, scalar: "anisotropy", physical: true },
  [appearance.Role.SPECULAR_ROUGHNESS_ANISOTROPY_ROTATION]: { slot: null, component: null, scalar: "anisotropyRotation", physical: true },
  [appearance.Role.SPECULAR_IOR]: { slot: null, component: null, scalar: "ior", physical: true },
  [appearance.Role.TRANSMISSION_WEIGHT]: { slot: "transmissionMap", component: 1, scalar: "transmission", physical: true },
  [appearance.Role.TRANSMISSION_ROUGHNESS]: { slot: null, component: null, scalar: null, physical: false },
  [appearance.Role.SUBSURFACE_WEIGHT]: { slot: null, component: null, scalar: null, physical: false },
  [appearance.Role.SUBSURFACE_RADIUS]: { slot: null, component: null, scalar: null, physical: false },
  [appearance.Role.COAT_WEIGHT]: { slot: "clearcoatMap", component: 1, scalar: "clearcoat", physical: true },
  [appearance.Role.COAT_COLOR]: { slot: null, component: null, scalar: null, physical: false },
  [appearance.Role.COAT_ROUGHNESS]: { slot: "clearcoatRoughnessMap", component: 2, scalar: "clearcoatRoughness", physical: true },
  [appearance.Role.COAT_IOR]: { slot: null, component: null, scalar: null, physical: false },
  [appearance.Role.FUZZ_WEIGHT]: { slot: null, component: null, scalar: "sheen", physical: true },
  [appearance.Role.FUZZ_COLOR]: { slot: "sheenColorMap", component: 3, scalar: "sheenColor", physical: true },
  [appearance.Role.FUZZ_ROUGHNESS]: { slot: "sheenRoughnessMap", component: 4, scalar: "sheenRoughness", physical: true },
  [appearance.Role.THIN_FILM_WEIGHT]: { slot: "iridescenceMap", component: 1, scalar: "iridescence", physical: true },
  [appearance.Role.THIN_FILM_THICKNESS]: { slot: "iridescenceThicknessMap", component: 2, scalar: "iridescenceThicknessRange", physical: true },
  [appearance.Role.THIN_FILM_IOR]: { slot: null, component: null, scalar: "iridescenceIOR", physical: true },
  [appearance.Role.EMISSION_COLOR]: { slot: "emissiveMap", component: 3, scalar: "emissive", physical: false },
  [appearance.Role.EMISSION_LUMINANCE]: { slot: null, component: null, scalar: "emissiveIntensity", physical: false },
  [appearance.Role.GEOMETRY_OPACITY]: { slot: "alphaMap", component: 2, scalar: "opacity", physical: false },
  [appearance.Role.GEOMETRY_NORMAL]: { slot: "normalMap", component: 3, scalar: "normalScale", physical: false },
  [appearance.Role.GEOMETRY_COAT_NORMAL]: { slot: "clearcoatNormalMap", component: 3, scalar: "clearcoatNormalScale", physical: true },
  [appearance.Role.GEOMETRY_TANGENT]: { slot: null, component: null, scalar: null, physical: false },
  [appearance.Role.GEOMETRY_COAT_TANGENT]: { slot: null, component: null, scalar: null, physical: false },
  [appearance.Role.HEIGHT]: { slot: "displacementMap", component: 1, scalar: "displacementScale", physical: false },
  [appearance.Role.OCCLUSION]: { slot: "aoMap", component: 1, scalar: "aoMapIntensity", physical: false },
  [appearance.Role.CURVATURE]: { slot: null, component: null, scalar: null, physical: false },
}

const _WORLD = { metrePerMillimetre: 1e-3 } as const satisfies { metrePerMillimetre: number }

const _write = (material: MeshPhysicalMaterial, slot: keyof MeshPhysicalMaterial, value: unknown): void => {
  ;(material as unknown as Record<keyof MeshPhysicalMaterial, unknown>)[slot] = value
}

const _tint = (target: Color, triple: readonly [number, number, number]): Color =>
  target.setRGB(triple[0], triple[1], triple[2], LinearSRGBColorSpace)

const _lobes = (material: MeshPhysicalMaterial, groups: Wire.Material["openPbr"]): MeshPhysicalMaterial => {
  _tint(material.color, groups.base.color.rgb)
  material.metalness = groups.base.metalness
  material.roughness = groups.specular.roughness
  _tint(material.specularColor, groups.specular.color.rgb)
  material.specularIntensity = groups.specular.weight
  material.ior = groups.specular.ior
  material.anisotropy = groups.specular.anisotropy
  material.anisotropyRotation = groups.specular.rotation * Math.PI
  material.transmission = groups.transmission.weight
  material.clearcoat = groups.coat.weight
  material.clearcoatRoughness = groups.coat.roughness
  material.sheen = groups.fuzz.weight
  _tint(material.sheenColor, groups.fuzz.color.rgb)
  material.sheenRoughness = groups.fuzz.roughness
  material.iridescence = groups.thinFilm.weight
  material.iridescenceIOR = groups.thinFilm.ior
  material.iridescenceThicknessRange = [groups.thinFilm.thickness, groups.thinFilm.thickness]
  _tint(material.emissive, groups.emission.color.rgb)
  material.emissiveIntensity = groups.emission.luminance
  material.opacity = groups.geometry.opacity
  material.transparent = groups.geometry.opacity < 1
  material.side = groups.geometry.thinWalled ? DoubleSide : FrontSide
  return material
}

const _stamped = (plane: Plane, set: Pbr.Surface, acquired: Glb.Acquired): Plane => {
  plane.wrapS = set.tiled ? RepeatWrapping : ClampToEdgeWrapping
  plane.wrapT = plane.wrapS
  plane.anisotropy = acquired.anisotropy
  plane.channel = 0
  acquired.renderer.initTexture(plane)
  return plane
}

const _seatings = (set: Pbr.Surface): ReadonlyArray<Pbr.Seating> => [
  ...Array.filterMap(set.planes, (row) => Option.map(Array.head(row.levels), (plane): Pbr.Seating => ({
    plane,
    container: row.container,
    format: row.format,
    transfer: Option.match(Option.fromNullable(row.transfer), {
      onNone: () => Option.some(TextureVocab.authored(row.role, TextureVocab.rows.plane[row.format].depth)),
      onSome: (transfer) => _sceneTransfer(transfer) ? Option.some(transfer) : Option.none(),
    }),
    pack: Option.none(),
    present: [row.role],
    absent: [],
  }))),
  ...Array.filterMap(set.packs, (row) => Option.map(Array.head(row.levels), (plane): Pbr.Seating => {
      const slots = TextureVocab.rows.pack[row.pack].slots
      return {
        plane,
        container: row.container,
        format: row.format,
        transfer: Option.some(TextureVocab.rows.channel[slots[0]].transfer),
        pack: Option.some(row.pack),
        present: row.present,
        absent: Array.filter(slots, (role) => !Array.contains(row.present, role)),
      }
    })),
]

const _demands = (bound: Pbr.Bound): boolean =>
  Option.isSome(bound.override) ||
  Option.exists(bound.set, (set) =>
    Array.some(_seatings(set), (seating) => Array.some(seating.present, (role) => _ROLE_SLOT[role].physical)))

const _upgraded = (source: MeshStandardMaterial): MeshPhysicalMaterial => {
  const target = new MeshPhysicalMaterial()
  MeshStandardMaterial.prototype.copy.call(target, source)
  target.defines = { STANDARD: "", PHYSICAL: "" }
  return target
}

const _packRolesLawful = (seating: Pbr.Seating): boolean =>
  Option.match(seating.pack, {
    onNone: () => true,
    onSome: (pack) => {
      const ordered = Array.filter(TextureVocab.rows.pack[pack].slots, (role) => Array.contains(seating.present, role))
      return seating.present.length === ordered.length
        && Array.every(seating.present, (role, at) => role === ordered[at])
    },
  })

const _unseatable = (set: Pbr.Surface, seating: Pbr.Seating, mesh: string): ReadonlyArray<GlbFault.Case> => {
  const refuse = (reason: Extract<GlbFault.Reason, "manifest-skew" | "plane-unbound">, cause: string): GlbFault.Case => ({
    reason,
    mesh,
    cause,
  })
  return [
    ...(TextureVocab.rows.plane[seating.format].web ? [] : [refuse("plane-unbound", "the store format decodes nowhere on the web")]),
    ...(TextureVocab.rows.layer[set.layerLaw].gltf ? [] : [refuse("plane-unbound", "the layer law seats nowhere in gltf")]),
    ...(set.udimTiles.length === 0 ? [] : [refuse("plane-unbound", "udim tiles reach no sampler")]),
    ...(Option.isNone(seating.transfer) ? [refuse("manifest-skew", "the plane carries no transfer")] : []),
    ...(Array.contains(seating.present, appearance.Role.HEIGHT) && set.heightScaleMm === undefined
      ? [refuse("manifest-skew", "the height plane carries no millimetre span")]
      : []),
    ...(_packRolesLawful(seating) ? [] : [refuse("manifest-skew", "the packed roles escape or reorder the pack slots")]),
    ...(Option.exists(seating.pack, (pack) => !TextureVocab.rows.pack[pack].gltf)
      ? [refuse("manifest-skew", "the pack read order is inverted")]
      : []),
    ...Array.flatMap(seating.present, (role) =>
      _ROLE_SLOT[role].slot === null
        ? [refuse("plane-unbound", `role ${role} names no slot`)]
        : TextureVocab.rows.plane[seating.format].width < (_ROLE_SLOT[role].component ?? 0)
          ? [refuse("manifest-skew", `role ${role} names a component the plane never stored`)]
          : []),
  ]
}

const _seated = (
  material: MeshPhysicalMaterial,
  set: Pbr.Surface,
  seating: Pbr.Seating,
  planes: Context.Tag.Service<GlbViewport>["planes"],
  acquired: Glb.Acquired,
): Effect.Effect<ReadonlyArray<GlbFault.Case>> =>
  Effect.gen(function* () {
    const reference = yield* Schema.decode(TextureVocab.reference)(seating.plane).pipe(
      Effect.mapError((issue) => new GlbFault({
        case: { reason: "manifest-skew", mesh: seating.plane.file, cause: issue.message },
      })),
    )
    return yield* Array.match(_unseatable(set, seating, `${reference.artifact.artifactId}`), {
      onNonEmpty: (refused) => Effect.succeed<ReadonlyArray<GlbFault.Case>>(refused),
      onEmpty: () =>
        Effect.gen(function* () {
          const key = reference.artifact.artifactId
          const transfer = yield* Option.match(seating.transfer, {
            onNone: () => Effect.fail(new GlbFault({
              case: { reason: "manifest-skew", mesh: `${key}`, cause: "the plane carries no transfer" },
            })),
            onSome: Effect.succeed,
          })
          const octets = yield* planes(reference.artifact)
          const plane = yield* _decoded(
            _CONTAINERS[seating.container],
            { key, octets },
            acquired,
            transfer,
            "plane-unbound",
          )
          return yield* Effect.sync(() => {
            const stamped = _stamped(plane, set, acquired)
            seating.present.forEach((role) => {
              const row = _ROLE_SLOT[role]
              if (row.slot !== null) _write(material, row.slot, stamped)
              if (role === appearance.Role.HEIGHT && row.scalar !== null && set.heightScaleMm !== undefined) {
                _write(material, row.scalar, set.heightScaleMm * _WORLD.metrePerMillimetre)
                _write(
                  material,
                  "displacementBias",
                  -set.heightScaleMm * _WORLD.metrePerMillimetre
                    * TextureVocab.rows.channel[appearance.Role.HEIGHT].neutral[0],
                )
              }
            })
            seating.absent.forEach((role) => {
              const row = _ROLE_SLOT[role]
              if (row.scalar !== null) _write(material, row.scalar, TextureVocab.rows.channel[role].neutral[0])
            })
            return []
          })
        }),
    })
  }).pipe(Effect.catchAll((refusal: GlbFault) => Effect.succeed<ReadonlyArray<GlbFault.Case>>([refusal.case])))

const _seat = (
  material: MeshPhysicalMaterial,
  bound: Pbr.Bound,
  planes: Context.Tag.Service<GlbViewport>["planes"],
  acquired: Glb.Acquired,
): Effect.Effect<Option.Option<GlbSeating>> =>
  Effect.map(
    Option.match(bound.set, {
      onNone: () => Effect.succeed<ReadonlyArray<ReadonlyArray<GlbFault.Case>>>([]),
      onSome: (set) =>
        Effect.forEach(_seatings(set), (seating) => _seated(material, set, seating, planes, acquired)),
    }),
    (refusals) => {
      Option.map(bound.override, (row) => _lobes(material, row.openPbr))
      Option.map(bound.set, (set) =>
        void (material.premultipliedAlpha = set.alphaMode === appearance.AlphaMode.ASSOCIATED))
      material.needsUpdate = true
      return Option.map(
        Array.match(Array.flatten(refusals), { onEmpty: Option.none, onNonEmpty: Option.some }),
        (issues) => new GlbSeating({ issues }),
      )
    },
  )

const _manifest = (mesh: string, cause: string): GlbFault =>
  new GlbFault({ case: { reason: "manifest-skew", mesh, cause } })

const _content = (bytes: Uint8Array, mesh: string): Effect.Effect<Digest.Key<"content">, GlbFault> =>
  Schema.decode(Digest.codecs.content.bytes)(bytes).pipe(Effect.mapError((issue) => _manifest(mesh, issue.message)))

const _requiredContent = (
  key: Uint8Array | undefined,
  mesh: string,
): Effect.Effect<Digest.Key<"content">, GlbFault> =>
  Option.match(Option.fromNullable(key), {
    onNone: () => Effect.fail(_manifest(mesh, "the appearance key is absent")),
    onSome: (held) => _content(held, mesh),
  })

const _unique = <V>(
  rows: ReadonlyArray<readonly [Digest.Key<"content">, V]>,
  subject: string,
): Effect.Effect<HashMap.HashMap<Digest.Key<"content">, V>, GlbFault> =>
  Array.reduce(
    rows,
    Effect.succeed(HashMap.empty<Digest.Key<"content">, V>()),
    (held, [key, value]) => Effect.flatMap(held, (index) =>
      HashMap.has(index, key)
        ? Effect.fail(_manifest(`${key}`, `duplicate ${subject}`))
        : Effect.succeed(HashMap.set(index, key, value))),
  )

const _index = (document: GlbViewport.Appearance): Effect.Effect<Pbr.Index, GlbFault> =>
  Effect.gen(function* () {
    const censusRows = yield* Effect.forEach(document.census, (row) =>
      Effect.map(_content(row.appearanceKey, "<appearance>"), (key) => [key, row] as const))
    const census = yield* _unique(censusRows, "appearance")

    const setDocuments = yield* Effect.forEach(document.sets, ([, set]) =>
      Effect.map(_content(set.key, "<texture-set>"), (identity) => [identity, set] as const))
    yield* _unique(setDocuments, "set document")
    const setRows = yield* Effect.forEach(document.sets, ([appearanceKey, set]) =>
      Match.value(set.product).pipe(Match.discriminatorsExhaustive("case")({
        baked: ({ value }) => Effect.flatMap(
          _requiredContent(value.appearanceKey, "<texture-set-appearance>"),
          (embedded) => embedded === appearanceKey
            ? Effect.succeed([appearanceKey, value.surface] as const)
            : Effect.fail(_manifest(`${appearanceKey}`, "the baked set embeds another appearance key")),
        ),
        pbr: ({ value }) => Effect.succeed([appearanceKey, value] as const),
        environment: () => Effect.fail(_manifest(`${appearanceKey}`, "an environment set cannot occupy an appearance seat")),
      })))
    const sets = yield* _unique(setRows, "appearance set")
    const materialRows = yield* Effect.forEach(document.materials, ([appearance, octets]) =>
      Effect.map(
        Wire.decode("Material", octets).pipe(
          Effect.mapError((fault) => new GlbFault({
            case: { reason: "decode-refused", mesh: `${appearance}`, cause: fault.message },
          })),
        ),
        (override) => [appearance, override] as const,
      ))
    const overrides = yield* _unique(materialRows, "material override")
    const worn = yield* _unique(document.worn, "mesh appearance")

    yield* Effect.forEach(HashMap.keys(sets), (appearance) =>
      HashMap.has(census, appearance)
        ? Effect.void
        : Effect.fail(_manifest(`${appearance}`, "the seat census holds no such set")))
    yield* Effect.forEach(HashMap.keys(overrides), (appearance) =>
      HashMap.has(census, appearance)
        ? Effect.void
        : Effect.fail(_manifest(`${appearance}`, "the seat census holds no such override")))
    yield* Effect.forEach(HashMap.toEntries(worn), ([mesh, appearance]) =>
      HashMap.has(census, appearance)
        ? Effect.void
        : Effect.fail(_manifest(`${mesh}`, "the seat census holds no such appearance")))

    return {
      seats: HashMap.map(census, (row, appearance) => ({ row, set: HashMap.get(sets, appearance) })),
      overrides,
      worn,
    }
  })

const _resolve = (index: Pbr.Index, mesh: Digest.Key<"content">): Option.Option<Pbr.Bound> =>
  Option.flatMap(HashMap.get(index.worn, mesh), (appearance) =>
    Option.map(HashMap.get(index.seats, appearance), (seat) => {
      return {
        appearance,
        row: seat.row,
        override: HashMap.get(index.overrides, appearance),
        set: seat.set,
      }
    }))

const Pbr: Pbr.Shape = {
  roles: _ROLE_SLOT,
  index: _index,
  resolve: _resolve,
  demands: _demands,
  upgrade: _upgraded,
  seat: _seat,
}
```

## [09]-[INSTANCED_ROWS]

- Owner: `Instanced` folds resident grafts into georeferenced deck-layer values over one placement axis.
- Law: the anchor roster is a fold over the residency ledger — a repeat outlives neither the graft it repeats nor the key that graft arrived under.
- Law: placement is the `Matrix4` `[07]` already speaks, and deck's own matrix-XOR-triple law keeps it the only transform vocabulary this pair carries.
- Law: colour crosses through `[08]`'s ingest — the linear triple seats under `LinearSRGBColorSpace` and reads back out under `SRGBColorSpace`, because deck instances take eight-bit display bytes.
- Boundary: these rows mint deck layer DESCRIPTORS, not GPU handles — deck's reconciler allocates and finalizes at mount, so a `Scope` bracket here would fight it and free nothing, and the mesh and scenegraph payloads stay the caller's.
- Boundary: the map coordinate is `geo#PROJECT`'s, the redraw clock is the overlay's, and `Tile3DLayer` hands its mesh and scenegraph tile content in through this pair.

```typescript signature
import type { Color as Paint, Position } from "@deck.gl/core"
import { ScenegraphLayer, SimpleMeshLayer } from "@deck.gl/mesh-layers"
import { Array, HashMap, Option } from "effect"
import { Color as Ink, type Matrix4, SRGBColorSpace } from "three"

declare namespace Instanced {
  type Anchor = {
    readonly key: Digest.Key<"content">
    readonly anchor: Position
    readonly placement: Matrix4
    readonly tint: Paint
  }
  type Placed = (key: Digest.Key<"content">) => Option.Option<{
    readonly anchor: Position
    readonly placement: Matrix4
  }>
  type Shape = {
    readonly anchors: typeof _anchors
    readonly mesh: typeof _mesh
    readonly scene: typeof _scene
  }
}

const _INK = { full: 255 } as const satisfies { full: number }

const _NEUTRAL: Paint = [_INK.full, _INK.full, _INK.full, _INK.full]

const _paint = (linear: readonly [number, number, number], scratch: Ink, display: Ink): Paint => {
  _tint(scratch, linear).getRGB(display, SRGBColorSpace)
  return [display.r * _INK.full, display.g * _INK.full, display.b * _INK.full, _INK.full]
}

const _anchors = (
  ledger: Glb.Ledger,
  index: Pbr.Index,
  placed: Instanced.Placed,
): ReadonlyArray<Instanced.Anchor> => {
  const scratch = new Ink()
  const display = new Ink()
  return Array.filterMap(HashMap.keys(ledger), (key) =>
    Option.map(placed(key), (seat) => ({
      key,
      anchor: seat.anchor,
      placement: seat.placement,
      tint: Option.match(Option.flatMap(Pbr.resolve(index, key), (bound) => bound.override), {
        onNone: () => _NEUTRAL,
        onSome: (row) => _paint(row.openPbr.base.color.rgb, scratch, display),
      }),
    } satisfies Instanced.Anchor)))
}

const _DECK = { sizeScale: 1, speed: 1 } as const satisfies { sizeScale: number; speed: number }

const _mesh = (id: string, mesh: SimpleMeshLayer["props"]["mesh"], anchors: ReadonlyArray<Instanced.Anchor>) =>
  new SimpleMeshLayer<Instanced.Anchor>({
    id,
    data: anchors,
    mesh,
    pickable: true,
    getPosition: (row) => row.anchor,
    getTransformMatrix: (row) => row.placement.elements,
    getColor: (row) => row.tint,
    sizeScale: _DECK.sizeScale,
  })

const _scene = (id: string, scenegraph: string, anchors: ReadonlyArray<Instanced.Anchor>) =>
  new ScenegraphLayer<Instanced.Anchor>({
    id,
    data: anchors,
    scenegraph,
    pickable: true,
    _lighting: "pbr",
    _animations: { "*": { speed: _DECK.speed } },
    getPosition: (row) => row.anchor,
    getTransformMatrix: (row) => row.placement.elements,
    sizeScale: _DECK.sizeScale,
  })

const Instanced: Instanced.Shape = { anchors: _anchors, mesh: _mesh, scene: _scene }
```

## [10]-[EMBED_ROW]

- Owner: the embed row brackets object URLs and seats decoder assets from the same roster.
- Law: embed is a backend arm of `Glb`, not a second residency owner.

```typescript signature
import { ModelViewerElement } from "@google/model-viewer"

let _pin: Option.Option<{ readonly draco: string; readonly ktx2: string; readonly meshopt: string }> = Option.none()

const _pinned = (roster: Glb.AssetRoster): Effect.Effect<void, GlbFault> =>
  Effect.gen(function* () {
    const assets = yield* Effect.all({
      draco: _asset(roster, "draco"),
      ktx2: _asset(roster, "ktx2"),
      meshopt: _asset(roster, "meshopt"),
    })
    const paths = Record.map(assets, (asset, slug) => _ADDRESS[_CODECS[slug].address](asset))
    yield* Effect.suspend(() =>
      Option.match(_pin, {
        onNone: () =>
          Effect.sync(() => {
            _pin = Option.some(paths)
            ModelViewerElement.dracoDecoderLocation = paths.draco
            ModelViewerElement.ktx2TranscoderLocation = paths.ktx2
            ModelViewerElement.meshoptDecoderLocation = paths.meshopt
            _HINTS.script(paths.meshopt)
          }),
        onSome: (held) =>
          held.draco === paths.draco && held.ktx2 === paths.ktx2 && held.meshopt === paths.meshopt
            ? Effect.void
            : Effect.fail(new GlbFault({ case: { reason: "manifest-skew", mesh: "decoder-pin", cause: "the pinned decoder roster diverges" } })),
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
    readonly sampling: typeof _SAMPLING
    readonly env: typeof _ENV
    readonly rig: typeof _RIG
    readonly bvh: typeof _BVH
    readonly codecRows: typeof _CODECS
    readonly lit: typeof _lit
    readonly renderer: typeof _renderer
    readonly backplane: typeof _backplane
    readonly lifecycle: typeof _booted
    readonly served: typeof _served
    readonly codecs: typeof _codecs
    readonly warm: typeof _warm
    readonly graft: typeof _graft
    readonly irradiance: typeof _irradiance
    readonly hook: typeof _residencyHook
    readonly loop: typeof _loop
    readonly instanced: typeof _instanced
    readonly batched: typeof _batched
    readonly merged: typeof _merged
    readonly visible: typeof _visible
    readonly tinted: typeof _tinted
    readonly cull: typeof _cull
    readonly sorted: typeof _sorted
    readonly baked: typeof _baked
    readonly section: typeof _section
    readonly nearest: typeof _nearest
    readonly measure: typeof _measure
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
  sampling: _SAMPLING,
  env: _ENV,
  rig: _RIG,
  bvh: _BVH,
  codecRows: _CODECS,
  lit: _lit,
  renderer: _renderer,
  backplane: _backplane,
  lifecycle: _booted,
  served: _served,
  codecs: _codecs,
  warm: _warm,
  graft: _graft,
  irradiance: _irradiance,
  hook: _residencyHook,
  loop: _loop,
  instanced: _instanced,
  batched: _batched,
  merged: _merged,
  visible: _visible,
  tinted: _tinted,
  cull: _cull,
  sorted: _sorted,
  baked: _baked,
  section: _section,
  nearest: _nearest,
  measure: _measure,
  pinned: _pinned,
  embed: _embed,
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Glb, GlbFault, GlbSeating, GlbViewport, Instanced, Pbr }
```

## [11]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
