# [UI_SCENE]

`Glb` owns generation-scoped scene residency, backend acquisition, environment lighting, OpenPBR binding, and GPU teardown.

## [01]-[INDEX]

- [02]-[VIEWPORT_PORT]: verified scene inputs; `GlbViewport`.
- [03]-[FAULT_FAMILY]: closed scene failures; `GlbFault`.
- [04]-[BACKEND_SELECT]: renderer acquisition and lifecycle; `Glb`.
- [05]-[RESIDENCY_GRAFT]: generation-safe graft and release; `Glb`.
- [06]-[ENVIRONMENT_FOLD]: environment decode and prefilter; `Glb`.
- [07]-[DRAW_COLLAPSE]: draw and visibility policy; `Glb`.
- [08]-[APPEARANCE_BIND]: OpenPBR and texture seating; `Pbr`.
- [09]-[INSTANCED_ROWS]: georeferenced instances; `Instanced`.
- [10]-[EMBED_ROW]: model-viewer backend; `Glb`.

## [02]-[VIEWPORT_PORT]

- Owner: `GlbViewport` carries generation-scoped residency, verified arrivals, dome data, appearances, and cache ports.
- Law: only verified whole-buffer octets enter the viewer; every geometry arrival carries its manifest generation.
- Boundary: runtime owns hauling and cache policy; UI owns decoding and GPU residency.

```typescript signature
import { Digest, Frame, Wire } from "@rasm/ts/core"
import { Context, type Effect, type Stream, type Subscribable } from "effect"

type AppearanceSummary = InstanceType<typeof Wire.AppearanceSummary>
type Material = InstanceType<typeof Wire.Material>
type PbrGroups = typeof Wire.PbrGroups.Type
type TextureSet = InstanceType<typeof Wire.TextureSet>

declare namespace GlbViewport {
  // generic arguments carry the whole-buffer contract: a shared backing store reaches no `three` decode entry
  type Arrival = { readonly key: Digest.Key<"content">; readonly generation: number; readonly octets: Uint8Array<ArrayBuffer> }
  type Environment = Arrival & {
    readonly intensity: number
    readonly rotation: number
    readonly sh9: ReadonlyArray<number>
  }
  // ONE appearance document: the summary roster is the appearance-key preimage census AND the flat pre-bind preview,
  // `sets` join it on `TextureSet.appearanceKey`, `worn` carries the element-side pairing no appearance wire holds,
  // and `planes` resolves one addressed leaf — `[setKey, file]`, the pair `Glb.assetPath` already derives from.
  // `materials` carries each override BESIDE the appearance key it was fetched under, because the wire nests the
  // whole OpenPBR vector inline (`Material.openPbr`) and carries NO key column — `Material.id` is the seam
  // `family.name` identity string, never a digest — the payload rides BEHIND the key at transport, so the fetch
  // coordinate is the pairing fact and only the carrier can state it
  type Appearance = {
    readonly summaries: ReadonlyArray<AppearanceSummary>
    readonly materials: ReadonlyArray<readonly [appearance: Digest.Key<"content">, override: Material]>
    readonly sets: ReadonlyArray<TextureSet>
    readonly worn: ReadonlyArray<readonly [mesh: Digest.Key<"content">, appearance: Digest.Key<"content">]>
    readonly planes: (address: readonly [Digest.Key<"content">, string]) => Effect.Effect<Uint8Array<ArrayBuffer>, GlbFault>
  }
  // the acceleration band: DERIVED data under the same content key its geometry arrived on, so a snapshot can
  // only ever describe the parse it was built from and a miss costs a rebuild, never correctness. Entries are
  // ordered by the residency walk's own traversal, which a content-addressed parse fixes.
  type Snapshots = {
    readonly read: (key: Digest.Key<"content">) => Effect.Effect<Option.Option<ReadonlyArray<SerializedBVH>>, GlbFault>
    readonly write: (key: Digest.Key<"content">, snapshot: ReadonlyArray<SerializedBVH>) => Effect.Effect<void, GlbFault>
  }
  // the served coordinate of a planned payload — the growth row the prefetch hints earned. It is a total read,
  // not a rail: a key the hauling side has not planned yet is lawfully absent, and a hint is never a fetch.
  type Addressed = (key: Digest.Key<"content">) => Option.Option<string>
}

class GlbViewport extends Context.Tag("ui/viewer/GlbViewport")<GlbViewport, {
  readonly ledger: Subscribable.Subscribable<Frame.ResidencyLedger>
  readonly arrivals: Stream.Stream<GlbViewport.Arrival, GlbFault>
  readonly environments: Stream.Stream<GlbViewport.Environment, GlbFault>
  readonly appearances: Stream.Stream<GlbViewport.Appearance, GlbFault>
  readonly snapshots: GlbViewport.Snapshots
  readonly addressed: GlbViewport.Addressed
}>() {}
```

## [03]-[FAULT_FAMILY]

- Owner: `GlbFault` closes scene failure policy over `Fault.Class` and private eviction and plane columns.
- Law: `_faultPolicy.at(reason)` is the only local policy read, and `_family.classOf(reason)` supplies the core class.

```typescript signature
import { Fault, Shape } from "@rasm/ts/core"
import { Record, Schema } from "effect"

// One row per reason: the core kind the class getter projects, plus the TWO scene-local axes. Severity, retry,
// blame, and quarantine are the core Fault.Class row table's — a rank or retry literal here would fork them.
// `arm` is the lifecycle guard's own column: `codec-absent` and `backend-lost` share the `unavailable` class, so a
// reason literal in the guard would move the phase on an unserved decoder.
const _reasons = ["manifest-skew", "key-mismatch", "decode-refused", "codec-absent", "plane-unbound", "backend-lost"] as const
const _faultRows = {
    "manifest-skew": { class: "conflicted", evict: true, arm: "asset" },
    "key-mismatch": { class: "malformed", evict: true, arm: "asset" },
    "decode-refused": { class: "invalid", evict: true, arm: "asset" },
    "codec-absent": { class: "unavailable", evict: false, arm: "asset" },
    // the mesh keeps its GLB-embedded slot: an unseatable plane is evidence over a drawable subtree, never an eviction
    "plane-unbound": { class: "invalid", evict: false, arm: "asset" },
    "backend-lost": { class: "unavailable", evict: false, arm: "backend" },
} as const
const _faultPolicy = Shape.vocabulary(_reasons, _faultRows)
const _family = Fault.Class.family(_reasons, Record.map(_faultRows, ({ class: kind }) => ({ class: kind })))

declare namespace GlbFault {
  type Reason = (typeof _family.reasons)[number]
  // the indicted plane: an asset refusal is contained by the lane that raised it, a backend refusal is a lifecycle event
  type Arm = ReturnType<typeof _faultPolicy.at>["arm"]
}

class GlbFault extends Schema.TaggedError<GlbFault>()("GlbFault", {
  reason: _family.schema,
  mesh: Schema.String,
  detail: Schema.String,
}) {
  static readonly roster: typeof _family.reasons = _family.reasons // the census anchor: the family's own ordered non-empty tuple
  get class(): Fault.Class.Kind {
    return _family.classOf(this.reason)
  }
  get evict(): boolean {
    return _faultPolicy.at(this.reason).evict
  }
  get arm(): GlbFault.Arm {
    return _faultPolicy.at(this.reason).arm
  }
  override get message(): string {
    return `<glb:${this.reason}> ${this.mesh}: ${this.detail}`
  }
}
```

## [04]-[BACKEND_SELECT]

- Owner: `Glb` acquires one WebGPU or WebGL renderer, output policy, sampling cap, prefilter, and lifecycle.
- Law: one scoped backplane preserves renderer identity across all decode, upload, dome, and device-loss paths.

```typescript signature
import { Machine } from "@effect/experimental"
import { Convention } from "@rasm/ts/core"
import { Duration, Effect, Metric, Option, pipe, Schedule, Schema, type Scope, ScopedRef, type Stream, type SubscriptionRef } from "effect"
import {
  ACESFilmicToneMapping, AgXToneMapping, AmbientLight, DirectionalLight, HemisphereLight, LightProbe,
  NeutralToneMapping, PMREMGenerator as GlPrefilter, RectAreaLight, SRGBColorSpace, WebGLRenderer,
} from "three"
import type { Light, PerspectiveCamera, Scene, Texture } from "three"
import { PMREMGenerator as GpuPrefilter, WebGPURenderer } from "three/webgpu"
import type { Clock } from "./geo.ts"

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
  // one acquisition record serves every downstream lane — backend, device seam, prefilter, codecs, and the settled
  // sampling cap all read off this shape, so no lane re-probes `navigator.gpu`, no second prefilter is ever
  // constructed, and no texture stamp re-asks a maximum the two backend classes spell under two different member
  // paths. Codecs ride the ACQUISITION because the transcoder is renderer-bound: `detectSupport` cannot rebind a
  // live pool — spawned workers hold their first config forever and the parse cache keys on buffer alone — so a
  // backend swap rebuilds the codec record with the renderer and the displaced pool dies with it.
  type Acquired = {
    readonly renderer: WebGLRenderer | WebGPURenderer
    readonly device: Option.Option<GPUDevice>
    readonly prefilter: Glb.Prefilter
    readonly anisotropy: number
    readonly codecs: Glb.Codecs
  }
  // the acquisition held as a resource cell rather than a value: a re-init SETS it, so the successor acquires
  // before the displaced renderer releases and no lane ever reads a disposed handle out of a captured record
  type Backplane = ScopedRef.ScopedRef<Glb.Acquired>
  type Phase = (typeof _phases)[number]
  // the OPEN turn vocabulary any lane may send, kept apart from the one guarded signal at the anchor: an
  // `Advance` schema spread from a derived subset would demote to the widened literal overload, and a payload
  // admitting `refused` would route around the policy guard the fault family owns
  type Signal = (typeof _signals)[number] | typeof _GUARDED
  type Act = (typeof _acts)[number]
  type Row = { readonly next: Glb.Phase; readonly act: Glb.Act }
  // the renderer-BOUND plane, gathered once so the acts re-derive it in one place instead of four
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
  // one handler-context projection the act rows consume: the keyed fork pair, the self-send, and the cooldown
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

// the sampling ceiling this viewport asks for; the acquisition meets it against the backend's own maximum, so the
// stamped value is `min(policy, hardware)` and no consumer holds a second opinion about grazing-angle quality
const _SAMPLING = { anisotropy: 16 } as const satisfies { anisotropy: number }

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
      // the ONE backend discrimination in this module: prefilter class and anisotropy member path both hang off it,
      // so every later lane reads the settled record and never asks which renderer it holds
      const legacy = built instanceof WebGLRenderer
      const prefilter: Glb.Prefilter = legacy ? new GlPrefilter(built) : new GpuPrefilter(built)
      return {
        renderer: built,
        device: legacy ? Option.none<GPUDevice>() : acquired,
        prefilter,
        anisotropy: Math.min(
          _SAMPLING.anisotropy,
          legacy ? built.capabilities.getMaxAnisotropy() : built.getMaxAnisotropy(),
        ),
        codecs: _codecs(served, built),
      } satisfies Glb.Acquired
    }),
    ({ codecs, prefilter, renderer }) => Effect.sync(() => {
      // BOUNDARY ADAPTER — the transcoder pool and the prefilter release before the renderer that backs them
      codecs.ktx2.dispose()
      prefilter.dispose()
      renderer.dispose()
    }),
  )

// the swap-in-place cell: `set` acquires the successor and releases the displaced renderer only after the swap
// commits, so a lane reading between the two never observes a torn backend and no consumer holds a dead handle
const _backplane = (canvas: HTMLCanvasElement, served: Glb.Served): Effect.Effect<Glb.Backplane, never, Scope.Scope> =>
  ScopedRef.fromAcquire(_renderer(canvas, served))

const _phases = ["booting", "ready", "degraded", "lost", "reviving"] as const

// the lifecycle word census: one frequency over the closed phase roster, so every landed turn is a bounded-tag
// series and the backend-health board reads transitions without a second subscription on the actor
const _PHASED = Convention.mount(Convention.metric.sceneBackend, _phases)

const _signals = ["settled", "floored", "revive"] as const

const _GUARDED = "refused" as const // minted only where the fault family's `arm` column reads `backend`

const _acts = ["arm", "park", "chase", "hold"] as const

const _FIBERS = { draw: "draw", revive: "revive", watch: "watch" } as const // keyed children, never a free string at a fork site

const _Phase = Schema.Literal(..._phases) // the tuple spread holds the non-empty overload on both wire schemas

const _Signal = Schema.Literal(..._signals)

const _Boot = Schema.Struct({ cool: Schema.Duration })

// the uniform turn: an acquisition outcome always arms and a revive always chases, so the table states four cells
// once and the two phases that diverge state only their divergence
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
  // already parked with a cooldown ticking: a second backend fault must not re-park a dead loop nor reset the
  // window the first one opened, which is exactly the cell a signal-keyed table would need a phase guard to spell
  lost: { ..._turn, refused: { next: "lost", act: "hold" } },
  // the keyed chase fiber already holds the id, so the row STATES the absorption instead of leaning on forkOne's
  reviving: { ..._turn, revive: { next: "reviving", act: "hold" } },
} as const satisfies { readonly [P in Glb.Phase]: { readonly [S in Glb.Signal]: Glb.Row } }

class Advance extends Schema.TaggedRequest<Advance>()("Advance", {
  failure: Schema.Never,
  success: _Phase,
  payload: { signal: _Signal },
}) {}

// the guarded arm: the reason travels so the guard reads the policy table's own column, and the detail rides the
// span rather than a message field the fields already prove
class Refuse extends Schema.TaggedRequest<Refuse>()("Refuse", {
  failure: Schema.Never,
  success: _Phase,
  payload: { reason: _family.schema, detail: Schema.String },
}) {}

// `GPUDevice.lost` resolves once per device, so the watch re-arms per acquisition under its own keyed fiber;
// `destroyed` is this module's own finalizer calling `dispose()`, so only an unknown loss mints the fault and a
// scope close can never chase a renderer it just retired
const _watched = (device: GPUDevice, send: Glb.Turn["send"]): Effect.Effect<void> =>
  Effect.flatMap(Effect.promise(() => device.lost), (info) =>
    info.reason === "destroyed"
      ? Effect.void
      : send(new Refuse({ reason: "backend-lost", detail: info.message })))

// One act per row value: every renderer-bound consequence lives here and nowhere else, so a new phase or signal
// is a table row and a new consequence is an act row — never a branch inside a procedure body.
const _ACTS: { readonly [K in Glb.Act]: (turn: Glb.Turn) => Effect.Effect<void> } = {
  arm: ({ forkReplace, plane, send }) =>
    Effect.gen(function* () {
      // the fresh acquisition constructed its own transcoder against the live backend, so the arm re-reads
      // nothing: the prefiltered target died with its renderer, the decoded source did not, the dome re-derives
      const acquired = yield* ScopedRef.get(plane.backplane)
      yield* plane.loop.rebind(acquired)
      // forkReplace interrupts the incumbent draw fiber, and ITS scope finalizer is the park — one park path
      // serves the hidden stance and the dead renderer alike, and `Effect.never` is what keeps the scope open
      yield* forkReplace(
        Effect.scoped(
          Effect.andThen(_loop(acquired.renderer, plane.root, plane.camera, plane.loop, plane.frames, plane.shown), Effect.never),
        ),
        _FIBERS.draw,
      )
      yield* Option.match(acquired.device, {
        onNone: () => Effect.void, // the WebGL floor has no device to lose: the watch simply has nothing to arm
        onSome: (device) => forkReplace(_watched(device, send), _FIBERS.watch),
      })
    }),
  park: ({ cool, forkOne, forkReplace, send }) =>
    Effect.zipRight(
      forkReplace(Effect.void, _FIBERS.draw), // the interrupt IS the park: the fiber's finalizer clears the callback
      forkOne(Effect.delay(send(new Advance({ signal: "revive" })), cool), _FIBERS.revive),
    ),
  // one acquisition path serves boot and re-init alike; forkOne means a stacked loss cannot stack acquisitions
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

// ONE step both requests execute: the row decides the phase, its act column decides the consequence, and the
// reply IS the landed phase because the actor's own Subscribable is what every view reads
const _turned = (turn: Glb.Turn, phase: Glb.Phase, signal: Glb.Signal): Effect.Effect<readonly [Glb.Phase, Glb.Phase]> =>
  pipe(_lifecycleRows[phase][signal], (row) =>
    Effect.as(Effect.zipRight(_ACTS[row.act](turn), Metric.update(_PHASED, row.next)), [row.next, row.next] as const))

const _lifecycle = (plane: Glb.Plane) =>
  Machine.makeSerializable({ state: _Phase, input: _Boot }, (input, previous) =>
    // reboot and retry resume through the same slot: `previousState` carries the last live phase across a remount
    Machine.serializable.make(previous ?? "booting").pipe(
      Machine.serializable.add(Advance, ({ forkOne, forkReplace, request, send, state }) =>
        _turned({ plane, cool: input.cool, forkOne, forkReplace, send }, state, request.signal)),
      Machine.serializable.add(Refuse, ({ forkOne, forkReplace, request, send, state }) =>
        // the guard is the policy table's own column, never a reason literal: `codec-absent` shares
        // `backend-lost`'s `unavailable` class and must still leave the phase exactly where it stands
        _faultPolicy.at(request.reason).arm === "backend"
          ? Effect.zipRight(
              // the device's own message is per-occurrence evidence, so it rides the request span and never a tag
              Effect.annotateCurrentSpan("glb.backend.loss", request.detail),
              _turned({ plane, cool: input.cool, forkOne, forkReplace, send }, state, _GUARDED),
            )
          : Effect.succeed([state, state] as const)),
    ),
  ).pipe(
    // an initialization defect re-drives as a policy value; the jitter keeps a fleet of tabs off one adapter
    Machine.retry(Schedule.exponential("250 millis").pipe(Schedule.jittered, Schedule.intersect(Schedule.recurs(4)))),
  )

// the cell acquired eagerly, so the first turn READS the landed device instead of acquiring a second one — boot
// and re-init differ only in which signal opens the turn, and `chase` therefore has exactly one caller shape
const _booted = (plane: Glb.Plane, cool: Duration.DurationInput): Effect.Effect<Glb.Stance, never, Scope.Scope> =>
  Effect.gen(function* () {
    const actor = yield* Machine.boot(_lifecycle(plane), { cool: Duration.decode(cool) })
    const acquired = yield* ScopedRef.get(plane.backplane)
    yield* actor.send(new Advance({ signal: Option.isSome(acquired.device) ? "settled" : "floored" }))
    return actor
  })
```

## [05]-[RESIDENCY_GRAFT]

- Owner: `Glb.graft` validates ledger generation, decodes verified GLB bytes, and owns subtree resources.
- Law: manifest replacement evicts old-generation grafts even when artifact keys repeat.
- Law: one traversal owns upload, acceleration, dress, visibility, and release coverage.

```typescript signature
import { Convention } from "@rasm/ts/core"
import { Context, Effect, HashMap, Metric, Option, Queue, Record, Ref, Schema, Scope, ScopedRef, Stream, SubscriptionRef } from "effect"
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
// importing the package is what merges `boundsTree`/`computeBoundsTree`/`disposeBoundsTree` onto `BufferGeometry`
// and `firstHitOnly` onto `Raycaster`; the RUNTIME halves of that promise are the pin below
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
  // a splat-borne payload parses to `Points`, which is NOT a `Mesh`, so the walk's set is the drawable UNION
  // both classes inhabit — a visitor bounded to `Mesh` reaches nothing a point cloud owns, and the release arm
  // would then leak every buffer and every slot a splat graft holds
  type Drawable = Mesh | Points
  // the walk's own drawable set rides the record: a re-dress on a fresh appearance document reaches exactly the
  // nodes the graft covered, and `kind` carries the ledger row's payload columns for the subtree's lifetime
  type Graft = {
    readonly node: Object3D
    readonly mixer: Option.Option<AnimationMixer>
    readonly generation: number
    readonly kind: Frame.ResidencyKind
    readonly drawn: ReadonlyArray<Glb.Drawable>
  }
  type Ledger = HashMap.HashMap<Digest.Key<"content">, Glb.Graft>
  // one codec record publishes the transcoder beside the loader consuming it, because the dome's deep-store
  // container decodes through that same configured instance and a second one owns a second worker pool
  type Codecs = { readonly gltf: GLTFLoader; readonly ktx2: KTX2Loader }
  // renderer-INDEPENDENT decode capability resolved once per viewport: the tapped manager, the warmed draco pool,
  // and the meshopt module are backend-blind, while the KTX2 transcoder is renderer-BOUND and rides `Acquired`
  type Served = {
    readonly manager: LoadingManager
    readonly draco: DRACOLoader
    readonly ktx2Dir: string
    readonly meshopt: Option.Option<typeof MeshoptDecoder>
  }
  type Codec = (typeof _codecSlugs)[number]
  type Address = (typeof _addresses)[number]
  type Warm = (typeof _warms)[number]
  // the two facts a served decoder leaf carries beyond its roster row: which address form its consumer joins
  // against, and which hint modality its own consumption earns
  type CodecRow = { readonly address: Glb.Address; readonly warm: Glb.Warm }
  type ResidencyFact =
    | { readonly _tag: "Arrived"; readonly arrival: GlbViewport.Arrival }
    | { readonly _tag: "Environment"; readonly key: Digest.Key<"content"> }
    | { readonly _tag: "Refused"; readonly refusal: GlbFault }
  // the pick plane's substrate, published as a STAMPED snapshot: `boundsTree` types as the `GeometryBVH` base and
  // every triangle query lives on the leaf, so the graft publishes `MeshBVH` directly rather than handing a
  // consumer a widened handle it would have to narrow. `stamp` advances on every residency mutation, so a held
  // descent re-reads instead of answering against a hierarchy the ledger already replaced, and `keyOf` answers
  // off THIS snapshot's own roster.
  type Resident = { readonly tree: MeshBVH; readonly node: Object3D }
  type Trees = {
    readonly stamp: number
    readonly held: HashMap.HashMap<string, Glb.Resident>
    readonly keyOf: (node: Object3D) => Option.Option<string>
  }
  type Loop = {
    // the residency root the graft owns: the app shell hands the `Scene` in exactly as it hands the `<canvas>`,
    // and republishing it here is what gives a scenegraph consumer — a glTF export pass, a debug walk — the one
    // parseable node without re-reaching the construction site or minting a second scene surface
    readonly root: Scene
    readonly advance: (delta: number) => void
    readonly facts: Stream.Stream<Glb.ResidencyFact, GlbFault>
    readonly dome: Effect.Effect<Glb.Dome>
    readonly trees: Effect.Effect<Glb.Trees>
    readonly rebind: (acquired: Glb.Acquired) => Effect.Effect<void>
  }
}

// the iac `_Asset` admission alphabet verbatim on BOTH leaf columns: no separator, no traversal, no dot-only form.
// The roster crosses as WIRE data — `AssetRosterWire` is its encoded form — so the CONSUMING end is the one that must
// refuse: a publish-end admission proves nothing about a roster a deploy hands the browser, and `../secrets/key`
// admitted here composes straight into `setDecoderPath`/`import()` off the derived path.
const _SEGMENT = Schema.String.pipe(Schema.pattern(/^(?!\.{1,2}$)[A-Za-z0-9._-]+$/))

const _AssetIdentity = Schema.Struct({
  slug: _SEGMENT,
  // the digest segment is LOWERCASE at admission on BOTH ends of the `assets/<digest>/` derivation, so a mis-cased
  // key refuses here instead of deriving a directory the publish never wrote — uppercasing or re-lowering at the
  // join is the deleted direction
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

// ONE traverse kernel carries every per-drawable direction: a resource one visitor reaches the other reaches, so
// eager upload never covers a texture the teardown then leaks. The predicate is the drawable UNION rather than
// `Mesh`, because a splat-borne graft's payload is `Points` — narrowing to `Mesh` would hand every splat subtree
// an empty visitor set and leak its geometry and slots whole. The kernel also RETURNS the visited set in traversal
// order, because the acceleration direction is asynchronous and cannot ride a synchronous visitor — reading the
// walk's own coverage is what keeps it from opening the second private walk this law forbids.
const _walk = (
  node: Object3D,
  visit: (drawn: Glb.Drawable, materials: ReadonlyArray<Material>) => void,
): ReadonlyArray<Glb.Drawable> => {
  // BOUNDARY ADAPTER
  const seen: Array<Glb.Drawable> = []
  node.traverse((child) => {
    if (child instanceof Mesh || child instanceof Points) {
      seen.push(child)
      visit(child, Array.isArray(child.material) ? child.material : [child.material])
    }
  })
  return seen
}

// `Object.values` enumerates OWN properties: every `MeshPhysicalMaterial` map field is a plain own property while
// its weight lobes are prototype accessors carrying no texture, so this reaches every declared slot and nothing in
// `userData` or a node graph. The set REFERENCE-dedupes because a pack plane seats one handle in three slots — a
// texture is a GPU handle, never a domain value, so identity is the invariant and both visitors see it once.
const _slots = (material: Material): ReadonlyArray<Texture> => [
  // BOUNDARY ADAPTER
  ...new Set(Object.values(material).filter((slot): slot is Texture => slot instanceof Texture)),
]

// material.dispose() frees the program and never its textures, so the slots release ahead of their material; the
// teardown visitor and the splat arm's material swap both retire through this ONE reader
const _retire = (materials: ReadonlyArray<Material>): void =>
  materials.forEach((material) => {
    _slots(material).forEach((slot) => slot.dispose())
    material.dispose()
  })

// the bounds tree frees ahead of the geometry that parks it. The tree call is uniform across the union because the
// prototype merge lands on `BufferGeometry` itself, so a point geometry that never carried one clears a null
const _release = (drawn: Glb.Drawable, materials: ReadonlyArray<Material>): void => {
  drawn.geometry.disposeBoundsTree()
  drawn.geometry.dispose()
  _retire(materials)
}

const _upload =
  (renderer: WebGLRenderer | WebGPURenderer) => (_drawn: Glb.Drawable, materials: ReadonlyArray<Material>): void =>
    materials.forEach((material) => _slots(material).forEach((slot) => renderer.initTexture(slot)))

// Build policy beside the output and sampling rows: `SAH` buys the tightest tree for the pick, section, and
// measure queries this viewport actually runs; `indirect` leaves the source index buffer untouched, which matters
// precisely because the release visitor's own `geometry.dispose()` is that buffer's one owner; `shared` is the
// zero-copy transfer INTENT, met at build time because `SharedArrayBuffer` exists only under cross-origin
// isolation and an un-isolated document would throw inside the worker.
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

// three's prototypes are HOST-WIDE, so the patch is one process-global pin taken inside the viewport's own scoped
// construction rather than at module load, and never restored — un-patching under a second live viewport would
// break its picks. This module is the estate's single viewport owner and therefore its one legal patch owner, and
// the pin is all-or-nothing: the package's `declare module` merge promises all three members, so patching a subset
// leaves a declared method absent at runtime.
let _patch = false

const _patched: Effect.Effect<void> = Effect.sync(() => {
  // BOUNDARY ADAPTER
  if (_patch) return
  _patch = true
  BufferGeometry.prototype.computeBoundsTree = computeBoundsTree
  BufferGeometry.prototype.disposeBoundsTree = disposeBoundsTree
  Mesh.prototype.raycast = acceleratedRaycast
})

// the pool is scoped and its façade owns the fallback: where `SharedArrayBuffer` is absent it constructs a
// single-threaded builder instead, so no caller branches on isolation and no second acquisition path exists
const _pool: Effect.Effect<ParallelMeshBVHWorker, never, Scope.Scope> = Effect.acquireRelease(
  Effect.sync(() => new ParallelMeshBVHWorker()),
  (pool) => Effect.sync(() => pool.dispose()),
)

// the tree is constructed as a `MeshBVH` on BOTH paths — the prototype's own builder answers the `GeometryBVH`
// base, and both the snapshot band and the query surface need the leaf — so one type crosses the whole band
const _built = (
  key: Digest.Key<"content">,
  mesh: Mesh,
  pool: ParallelMeshBVHWorker,
  onProgress: (fraction: number) => void,
): Effect.Effect<MeshBVH, GlbFault> =>
  Effect.tryPromise({
    try: () => pool.generate(mesh.geometry, _options(onProgress)),
    catch: (defect) => new GlbFault({ reason: "decode-refused", mesh: `${key}`, detail: String(defect) }),
  })

// The acceleration direction, keyed by the SAME content key the octets are: a snapshot can only ever describe the
// geometry it was built from, so a hit deserializes and no validity probe exists to write, while a cardinality
// disagreement (a snapshot older than a re-cut parse) falls back to a build rather than under-accelerating a tail.
// The builder refuses a second concurrent `generate`, so the fold is single-flight by declaration, and the build
// runs off the render thread so a large-assembly graft never stalls a frame.
// one stamped snapshot: `keyOf` closes over THIS roster, so a consumer holding a structure can never resolve a hit
// against a map the ledger has already replaced — the stamp tells it to re-read and the closure keeps it honest
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
        // BOUNDARY ADAPTER — the cardinality check above is the index evidence the checker cannot carry
        Effect.sync(() => meshes.map((mesh, at) => MeshBVH.deserialize(snapshot[at]!, mesh.geometry, { setIndex: !_BVH.indirect }))),
      onNone: () => Effect.forEach(meshes, (mesh) => _built(key, mesh, pool, onProgress), { concurrency: 1 }),
    })
    yield* Option.isNone(cached) ? snapshots.write(key, built.map((tree) => MeshBVH.serialize(tree))) : Effect.void
    return yield* Effect.sync(() =>
      // BOUNDARY ADAPTER — the tree parks on the geometry the release visitor already owns, so its lifetime is
      // the subtree's and `disposeBoundsTree` frees it under the same arm that frees the buffers; the same leaf
      // handle publishes to the pick plane, which the widened `boundsTree` declaration could not serve
      meshes.map((mesh, at) => {
        const tree = built[at]!
        mesh.geometry.boundsTree = tree
        return { tree, node: mesh } satisfies Glb.Resident
      }))
  })

// The splat draw ARM, selected by the same `Frame.Residency.kind` read that skips the lobe bind: a splat carries
// per-point radiance and no material, so the node graph reads the parse's own colour attribute and nothing
// repaints it as a dielectric. `vertexColor()` is a `vec4`, so the alpha channel is live and the material
// declares `transparent` rather than discarding it; no size node is bound because the shipped declaration states
// point primitives are fixed at one pixel under WebGPU and `sizeNode` is honoured only on `Sprite`, so setting it
// would be a knob the backend ignores. The displaced handle retires through the release visitor's own reader,
// because that visitor only ever sees what the drawable currently holds.
const _splat = (graft: Glb.Graft, acquired: Glb.Acquired): Effect.Effect<void> =>
  // the CLASS probe, not the device Option: node materials and `computeAsync` live on the unified renderer alone,
  // and only the class carries that fact into the type plane, while `Acquired.device` stays the DEVICE seam a
  // `tgpu.initFromDevice` adopter reads — two facts, two reads, neither re-deriving the acquisition's own answer.
  // The WebGL floor therefore keeps the material the parse gave it: one arm, never a second material family.
  acquired.renderer instanceof WebGPURenderer
    ? Effect.sync(() => {
        // BOUNDARY ADAPTER — the node material is a mutable platform record and the swap is one assignment.
        // Compositing is straight-alpha over-blend: the wire crosses sigmoid STRAIGHT alpha (`StreamFilter.None`,
        // the renderer's direct input) with no baked order, and the encoding's draw obligation is back-to-front
        // per view — `depthWrite: false` keeps the composite from self-occluding, and the fixed-pixel Points
        // floor composites UNSORTED as its declared forfeit, never a premultiplied or additive re-read.
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

// ONE dressing entry over a graft, run at commit and re-run on every fresh appearance document because the seat is
// idempotent pure assignment: `worn` names which meshes carry an appearance and every other mesh keeps the
// material the parse gave it. The `kind` column selects the ARM — a splat-borne row takes the point-radiance bind
// above and never reaches the lobe seat. A material the GLB minted without its PBR extensions has no coat, sheen,
// or transmission slot to seat, so it surfaces as evidence rather than taking a partial bind, and every refusal
// folds into the one bounded fact queue the lanes already share.
const _dress = (
  key: Digest.Key<"content">,
  graft: Glb.Graft,
  index: Pbr.Index,
  document: GlbViewport.Appearance,
  codecs: Glb.Codecs,
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
              graft.drawn.flatMap((drawn) => (Array.isArray(drawn.material) ? drawn.material : [drawn.material])),
              (material) =>
                material instanceof MeshPhysicalMaterial
                  ? Pbr.seat(material, bound, document, codecs, acquired)
                  : Effect.succeed([
                    new GlbFault({ reason: "plane-unbound", mesh: `${key}`, detail: "<material-unphysical>" }),
                  ]),
            ),
            (refusals) =>
              Effect.forEach(
                refusals.flat(),
                (refusal) => Queue.offer(facts, { _tag: "Refused", refusal } as const),
                { discard: true },
              ),
          ),
      })

const _graft = (
  root: Scene,
  codecs: Glb.Codecs,
  // the CELL, never a captured record: a re-init swaps the acquisition beneath these lanes, so every lane reads
  // the live renderer at the moment it uploads, seats, or prefilters rather than holding a disposed handle
  backplane: Glb.Backplane,
  port: Context.Tag.Service<GlbViewport>,
  // the same dependency tap `_codecs` hands its LoadingManager: an off-thread tree build reports its own fraction
  // into the residency telemetry rather than minting a second progress channel
  progress: (fraction: number) => void,
  // every lane forks with its refusals contained, so construction succeeds or dies; `_codecs` alone carries
  // `codec-absent`, and a caller inheriting an unraisable channel writes a recovery arm nothing reaches
): Effect.Effect<Glb.Loop, never, Scope.Scope> =>
  Effect.gen(function* () {
    yield* _patched
    const pool = yield* _pool
    const held = yield* Ref.make(HashMap.empty<Digest.Key<"content">, Glb.Graft>())
    const facts = yield* Queue.bounded<Glb.ResidencyFact>(32)
    // the appearance ledger the graft lane reads at commit and the appearance lane refreshes; both dress through
    // ONE entry, so a mesh arriving before its document and a document arriving before its mesh converge
    const dressed = yield* Ref.make(Option.none<readonly [Pbr.Index, GlbViewport.Appearance]>())
    // the pick plane's substrate: one stamped roster the graft mutates and `mark` reads, never a second walk
    const accel = yield* Ref.make(_trees(0, HashMap.empty<string, Glb.Resident>()))
    const insert = Stream.runForEach(port.arrivals, (arrival) =>
      Effect.gen(function* () {
        // the published ledger IS the manifest: an arrival it never named refuses BEFORE any parse, which is the
        // one raiser `manifest-skew` declares, and the row it does find rides the graft for the subtree's lifetime
        const ledger = yield* port.ledger.get
        const row = yield* Option.match(
          arrival.generation === ledger.generation ? HashMap.get(ledger.rows, arrival.key) : Option.none(),
          {
          onNone: () =>
            Effect.fail(new GlbFault({ reason: "manifest-skew", mesh: `${arrival.key}`, detail: "<unledgered-arrival>" })),
          onSome: Effect.succeed,
          },
        )
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
        const acquired = yield* ScopedRef.get(backplane) // read per arrival: a re-init between two arrivals is invisible here
        const drawn = yield* Effect.sync(() => {
          const seen = _walk(gltf.scene, _upload(acquired.renderer))
          root.add(gltf.scene)
          return seen
        })
        // only triangle payloads index: the snapshot band's cardinality is therefore the MESH count in the walk's
        // own traversal order, so a splat graft files no snapshot and re-opens none
        const residents = yield* _accelerate(
          arrival.key,
          drawn.filter((node): node is Mesh => node instanceof Mesh),
          port.snapshots,
          pool,
          progress,
        )
        const graft = { node: gltf.scene, mixer, generation: arrival.generation, kind: row.kind, drawn } satisfies Glb.Graft
        yield* Ref.update(held, HashMap.set(arrival.key, graft))
        yield* Ref.update(accel, (live) =>
          _trees(live.stamp + 1, residents.reduce((map, row) => HashMap.set(map, row.node.uuid, row), live.held)))
        yield* Option.match(yield* Ref.get(dressed), {
          onNone: () => Effect.void,
          onSome: ([index, document]) => _dress(arrival.key, graft, index, document, codecs, acquired, facts),
        })
        yield* Effect.asVoid(Effect.withMetric(Effect.succeed(1), _GRAFTED))
        yield* Queue.offer(facts, { _tag: "Arrived", arrival } as const)
      }).pipe(
        Metric.trackErrorWith(_REFUSED, (fault: GlbFault) => fault.reason),
        Effect.withSpan("rasm.ui.scene.residency", { attributes: { "glb.bytes": arrival.octets.byteLength } }),
        Effect.annotateLogs({ mesh: `${arrival.key}` }),
        Effect.catchAll((refusal) => Queue.offer(facts, { _tag: "Refused", refusal } as const)),
      )))
    const evict = Stream.runForEach(port.ledger.changes, (ledger) =>
      Effect.gen(function* () {
        const grafts = yield* Ref.get(held)
        const gone = HashMap.filter(grafts, (graft, key) =>
          graft.generation !== ledger.generation || Option.match(HashMap.get(ledger.rows, key), {
            onNone: () => true,
            onSome: (row) => row.state === "evicted",
          }))
        yield* Effect.forEach(gone, ([key, graft]) =>
          Effect.zipRight(
            Effect.zipRight(
              Ref.update(held, HashMap.remove(key)),
              // removal advances the stamp, so a descent held across an eviction re-reads rather than
              // answering against a tree whose geometry the release visitor is about to free
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
      }))
    // this third residency lane indexes one document ONCE and re-dresses every held graft, because the seat is
    // idempotent pure assignment over carried handles and a re-issued appearance is exactly the edit that has no
    // digest change left to carry it — the same law the dome's policy re-read already holds
    const wear = Stream.runForEach(port.appearances, (document) =>
      Effect.gen(function* () {
        const index = yield* Pbr.index(document)
        const acquired = yield* ScopedRef.get(backplane) // the re-dress stamps against the LIVE backend's sampling cap
        yield* Ref.set(dressed, Option.some([index, document] as const))
        yield* Effect.forEach(
          yield* Ref.get(held),
          ([key, graft]) => _dress(key, graft, index, document, codecs, acquired, facts),
          { discard: true },
        )
      }).pipe(
        Metric.trackErrorWith(_REFUSED, (fault: GlbFault) => fault.reason),
        Effect.withSpan("rasm.ui.scene.residency"),
        Effect.catchAll((refusal) => Queue.offer(facts, { _tag: "Refused", refusal } as const)),
      ))
    const dome = yield* _dome(root, backplane, codecs, port.environments, facts)
    yield* Effect.forkScoped(Effect.all([insert, evict, wear], { concurrency: 3, discard: true }))
    return {
      root,
      advance: (delta) => {
        // BOUNDARY ADAPTER
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

const _codecSlugs = ["draco", "ktx2", "meshopt"] as const

const _addresses = ["dir", "file"] as const

const _warms = ["fetch", "module", "script"] as const

const _ADDRESS = { dir: _assetDir, file: _assetPath } as const satisfies {
  readonly [K in Glb.Address]: (asset: Glb.AssetIdentity) => string
}

// ONE row per served decoder. `address` is the form its consumer joins against — the two multi-leaf transcoders
// take the DIRECTORY and append their own leaf names, the single-leaf decoder module takes the FILE — and `warm`
// is the hint modality its own consumption earns. Both facts were spelled at three sites (this construction,
// `[10]`'s process-global pin, and the hint family), so a fourth decoder cost three edits; it is now one row.
const _CODECS = {
  draco: { address: "dir", warm: "fetch" },
  ktx2: { address: "dir", warm: "fetch" },
  // this construction reaches the leaf through `import()`, so the module GRAPH is what warms here; `[10]`'s embed
  // arm points its process-global static at the SAME address and the element executes it as a classic script,
  // which is why the modality is a column read at each consumption rather than a property of the leaf
  meshopt: { address: "file", warm: "module" },
} as const satisfies { readonly [K in Glb.Codec]: Glb.CodecRow }

// The hint must match the eventual request's CORS mode or the browser discards the warmed entry and re-fetches,
// so the mode is stated once beside the output and sampling rows rather than guessed per call site; every
// addressed leaf is served same-origin under `assets/` and every loader fetches it without credentials.
const _WARM = { crossOrigin: "anonymous", priority: "high" } as const satisfies {
  crossOrigin: NonNullable<PreloadOptions["crossOrigin"]>
  priority: NonNullable<PreloadOptions["fetchPriority"]>
}

// One call per modality: `PreloadAs` carries NO wasm member, so every wasm and worker leaf a loader fetches for
// itself rides `as: "fetch"` and a guessed `as` is the defect this table forecloses; `module` warms the graph
// `import()` pulls, and `script` evaluates the one leaf the document itself executes.
const _HINTS: { readonly [K in Glb.Warm]: (href: string) => void } = {
  // BOUNDARY ADAPTER — the react-dom hint family is a void-returning document-level surface
  fetch: (href) => preload(href, { as: "fetch", crossOrigin: _WARM.crossOrigin, fetchPriority: _WARM.priority }),
  module: (href) => preinitModule(href, { as: "script", crossOrigin: _WARM.crossOrigin }),
  script: (href) => preinit(href, { as: "script", crossOrigin: _WARM.crossOrigin }),
}

// One pass ahead of first frame over the roster and the ledger's OWN `pending` census: the decoder leaves the
// first parse will demand and the payloads the transport has already planned. Per-mesh hinting at draw time is
// the named defect, because a hint landing beside the fetch it warms saves nothing. The payload address comes
// from the port, never from a key: the hauling side owns the served coordinate and this module addresses only
// what `Glb.assetPath` derives, so a viewer minting a GLB URL out of a content key would forge one.
const _warm = (
  roster: Glb.AssetRoster,
  ledger: Frame.ResidencyLedger,
  addressed: GlbViewport.Addressed,
): Effect.Effect<void> =>
  Effect.zipRight(
    // the owner's ONE roster resolver with its refusal reified: an unserved decoder is lawfully absent to a hint
    Effect.forEach(
      Record.toEntries(_CODECS),
      ([slug, row]) =>
        Effect.flatMap(Effect.option(_asset(roster, slug)), (found) =>
          Effect.sync(() => void Option.map(found, (asset) => _HINTS[row.warm](_ADDRESS[row.address](asset))))),
      { discard: true },
    ),
    Effect.sync(() => void Frame.Residency.pending(ledger).forEach((row) => Option.map(addressed(row.mesh), _HINTS.fetch))),
  )

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
            try: (): Promise<{ readonly MeshoptDecoder: typeof MeshoptDecoder }> =>
              import(_ADDRESS[_CODECS.meshopt.address](row)),
            catch: () => new GlbFault({ reason: "codec-absent", mesh: row.slug, detail: "<decoder-module>" }),
          }),
          (module) => Option.some(module.MeshoptDecoder),
        ),
    })
    return yield* Effect.sync(() => {
      // BOUNDARY ADAPTER — the address form is the row's own column; no site re-decides dir versus file
      const manager = new LoadingManager()
      manager.onProgress = taps.progress
      manager.onError = taps.error
      // transcoders bind ONCE and publish, never swallowed: [6]'s deep-store dome decodes through this one, and
      // `[4]`'s re-init arm re-runs `detectSupport` on THIS instance rather than minting a second worker pool
      const basis = new KTX2Loader(manager).setTranscoderPath(_ADDRESS[_CODECS.ktx2.address](ktx2)).detectSupport(renderer)
      const built = new GLTFLoader(manager)
        .setDRACOLoader(new DRACOLoader(manager).setDecoderPath(_ADDRESS[_CODECS.draco.address](draco)).preload())
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
  // geo's rAF clock is the viewer stratum's ONE time coordinate; this renderer contributes its DRAW cadence alone
  frames: SubscriptionRef.SubscriptionRef<Clock.Frame>,
  // the <Activity> stance as a feed: it carries the live value on subscribe, so the drain IS the arm
  shown: Stream.Stream<boolean>,
): Effect.Effect<void, never, Scope.Scope> =>
  Effect.gen(function* () {
    const draw = (): void => {
      // BOUNDARY ADAPTER — the published frame is read, never a timer sampled: a scrubbed coordinate and a wall
      // clock disagree the moment either is held, and the mixer would then land off the sought pose
      live.advance(Effect.runSync(SubscriptionRef.get(frames)).delta)
      renderer.render(scene, camera)
    }
    // the finalizer is the park BOTH paths reach: `dispose()` frees GPU handles and leaves the frame callback armed
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
// core's frozen vocabulary and three's texture class both spell `Texture`; the wire anchor takes the alias because
// three's class is the type this whole fold speaks, and the campaign's cross-module spelling for it is `TextureVocab`
const TextureVocab = Wire.Texture
import { Effect, Metric, Option, Queue, Ref, Scope, ScopedRef, Stream } from "effect"
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
  // one shape carries the analytic floor AND every keyed dome: the floor is the row with no key and no source,
  // and it holds read policy beside the GPU handles so an irradiance query never re-reads the scene fields;
  // `basis` is the read-time un-rotation, derived at commit because a dome's orientation is fixed for its life
  type Dome = {
    readonly key: Option.Option<Digest.Key<"content">>
    readonly source: Option.Option<Texture>
    readonly target: { readonly texture: Texture; dispose(): void }
    readonly sh: SphericalHarmonics3
    readonly intensity: number
    readonly rotation: number
    readonly basis: Quaternion
  }
  // ONE row per frozen container, TWO lookup modalities: `probe` is the dome lane's magic sniff and is absent on
  // every container that cannot hold radiance, `decode` is the set-bind's keyed lookup and its absence IS the
  // browser's refusal. The decode owns its shape whole — synchronous for the two DataTextureLoader legs,
  // callback-shaped for the deep store, promise-shaped for the bitmap legs — so no lane branches on decoder shape.
  type Container = {
    readonly probe: Option.Option<(octets: Uint8Array<ArrayBuffer>) => boolean>
    readonly decode: Option.Option<(arrival: GlbViewport.Arrival, codecs: Glb.Codecs) => Effect.Effect<Texture, GlbFault>>
  }
  type Slot = {
    readonly slot: Ref.Ref<Glb.Dome>
    readonly rebind: (acquired: Glb.Acquired) => Effect.Effect<void>
  }
}

// one projection carries the transfer tag onto three's colour-space constants; `[8]`'s per-plane stamp reads the wire's
// own `transfer` column through it and the dome reads `_ENV.transfer`, so the estate's ACEScc-primaried linear
// planes land under one declaration instead of two literals that drift apart
const _TRANSFER = {
  srgb: SRGBColorSpace,
  linear: LinearSRGBColorSpace,
  raw: NoColorSpace,
  // this domain reads the DECODED row's own column, not the five-tag anchor: a channel plane carries a scene transfer
  // by construction, so the two display tags cannot enter and a fourth scene tag breaks this table loudly
} as const satisfies { readonly [K in TextureSet["channels"][number]["transfer"]]: ColorSpace }

const _ENV = {
  type: HalfFloatType,
  transfer: "linear",
  backdrop: true,
  blur: 0,
  floorBlur: 0.04,
  bands: 27,
  // this row repairs the loader's uncompressed branch: linear where a pyramid exists, linear where it does not
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

// KTX2Loader hands every UNCOMPRESSED store NearestFilter and, past its three DFD primary arms, NoColorSpace;
// both are stamped back from policy after `parse`, and both consumers — dome source and set plane — call this one
const _filtered = (texture: Texture, transfer: keyof typeof _TRANSFER): Texture => {
  // BOUNDARY ADAPTER
  texture.minFilter = texture.mipmaps.length > 1 ? _ENV.filter.mipped : _ENV.filter.flat
  texture.magFilter = _ENV.filter.flat
  texture.colorSpace = _TRANSFER[transfer]
  return texture
}

// KTX 2.0 spells its file identifier in twelve bytes — the deep store's only self-description before its header parses
const _KTX2 = [0xab, 0x4b, 0x54, 0x58, 0x20, 0x32, 0x30, 0xbb, 0x0d, 0x0a, 0x1a, 0x0a] as const

// both DataTextureLoader legs differ only by their loader, so the decode parameterizes over the constructor
const _plane =
  (open: () => { createDataTexture(buffer: ArrayBuffer): DataTexture }) =>
  (arrival: GlbViewport.Arrival): Effect.Effect<Texture, GlbFault> =>
    Effect.try({
      try: () => open().createDataTexture(arrival.octets.buffer),
      catch: (defect) => new GlbFault({ reason: "decode-refused", mesh: `${arrival.key}`, detail: String(defect) }),
    })

// every eight-bit row the browser reaches differs only by media type, so the decode parameterizes over it and
// declares every implicit conversion OFF — colour management, premultiplication, and EXIF orientation each rewrite
// a channel plane's stored value, and the frozen top-left origin survives only where the browser applies none
const _bitmap =
  (mime: string) =>
  (arrival: GlbViewport.Arrival): Effect.Effect<Texture, GlbFault> =>
    Effect.map(
      Effect.tryPromise({
        try: () =>
          globalThis.createImageBitmap(new Blob([arrival.octets], { type: mime }), {
            colorSpaceConversion: "none",
            premultiplyAlpha: "none",
            imageOrientation: "none",
          }),
        catch: (defect) => new GlbFault({ reason: "decode-refused", mesh: `${arrival.key}`, detail: String(defect) }),
      }),
      (bitmap) => {
        // BOUNDARY ADAPTER — a bitmap-sourced texture uploads only once `needsUpdate` marks it
        const texture = new Texture(bitmap)
        texture.needsUpdate = true
        return texture
      },
    )

// dual-callback surfaces are the seam Effect.async owns; this instance is [5]'s configured transcoder, its declared
// CompressedTexture is narrower than the DataTexture an uncompressed store yields, and the uncompressed branch
// hands back NearestFilter and NoColorSpace, so the policy stamp closes the decode
const _deep = (arrival: GlbViewport.Arrival, codecs: Glb.Codecs): Effect.Effect<Texture, GlbFault> =>
  Effect.async<Texture, GlbFault>((resume) =>
    codecs.ktx2.parse(
      arrival.octets.buffer,
      (texture) => resume(Effect.succeed(texture)),
      (defect) =>
        resume(Effect.fail(new GlbFault({ reason: "decode-refused", mesh: `${arrival.key}`, detail: String(defect) }))),
    ))

// This roster closes the frozen twelve: every container the wire can name has a row, and the browser answers in a column.
// `probe` (container magic, never filename or producer) is present only where the container holds radiance, so the
// dome sniff structurally cannot land on a preview store; `decode` absent IS the refusal the set-bind raises.
const _CONTAINERS: { readonly [K in TextureVocab.Container]: Glb.Container } = {
  // OpenEXR spells 20000630 little-endian at byte 0; Radiance opens "#?"; KTX 2.0 its twelve-byte identifier
  exr: {
    probe: Option.some((o: Uint8Array<ArrayBuffer>): boolean =>
      o.byteLength >= 4 && new DataView(o.buffer, o.byteOffset, 4).getUint32(0, true) === 20000630),
    decode: Option.some(_plane(() => new EXRLoader().setDataType(_ENV.type))),
  },
  hdr: {
    probe: Option.some((o: Uint8Array<ArrayBuffer>): boolean => o.byteLength >= 2 && o[0] === 0x23 && o[1] === 0x3f),
    decode: Option.some(_plane(() => new HDRLoader().setDataType(_ENV.type))),
  },
  ktx2: {
    probe: Option.some((o: Uint8Array<ArrayBuffer>): boolean =>
      o.byteLength >= _KTX2.length && _KTX2.every((byte, at) => o[at] === byte)),
    decode: Option.some(_deep),
  },
  // eight-bit stores the browser decodes and a dome may never sniff: no probe, so radiance never lands here
  png16: { probe: Option.none(), decode: Option.some(_bitmap("image/png")) },
  webp: { probe: Option.none(), decode: Option.some(_bitmap("image/webp")) },
  avif12: { probe: Option.none(), decode: Option.some(_bitmap("image/avif")) },
  // no browser path exists: the two TIFF stores and both JPEG XL stores have no decoder, `EXRLoader` reads
  // scanline and tiled EXR alone, and the eight-bit lossless preview its own producer refuses for any plane
  tiff16: { probe: Option.none(), decode: Option.none() },
  tiff_f32: { probe: Option.none(), decode: Option.none() },
  exr_deep: { probe: Option.none(), decode: Option.none() },
  jxl: { probe: Option.none(), decode: Option.none() },
  jxl_f16: { probe: Option.none(), decode: Option.none() },
  qoi: { probe: Option.none(), decode: Option.none() },
}

const _sniff = (octets: Uint8Array<ArrayBuffer>): Option.Option<Glb.Container> =>
  Option.fromNullable(
    Object.values<Glb.Container>(_CONTAINERS).find((row) => Option.exists(row.probe, (probe) => probe(octets))),
  )

// ONE decode entry over a roster row, whichever modality found it: the absent column IS the refusal and the caller
// names the reason its plane deserves, while the policy stamp closes every leg so no consumer repairs filter or
// colour space at its own site
const _decoded = (
  row: Glb.Container,
  arrival: GlbViewport.Arrival,
  codecs: Glb.Codecs,
  transfer: keyof typeof _TRANSFER,
  reason: GlbFault.Reason,
): Effect.Effect<Texture, GlbFault> =>
  Option.match(row.decode, {
    onNone: () => Effect.fail(new GlbFault({ reason, mesh: `${arrival.key}`, detail: "<container-undecodable>" })),
    onSome: (decode) => Effect.map(decode(arrival, codecs), (texture) => _filtered(texture, transfer)),
  })

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
  // the CELL: the boot rows read it once and the arrival lane reads it per arrival, so a dome landing after a
  // re-init prefilters through the live generator instead of one its renderer already released
  backplane: Glb.Backplane,
  codecs: Glb.Codecs,
  arrivals: Stream.Stream<GlbViewport.Environment, GlbFault>,
  facts: Queue.Queue<Glb.ResidencyFact>,
  // every refusal is contained inside the forked lane, so the constructor itself carries no error channel
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
          // BOUNDARY ADAPTER
          dome.target.dispose()
          Option.map(dome.source, (source) => source.dispose())
        })))
    // floor bakes compile the CUBEMAP path alone; warming the equirect path off the critical path keeps the
    // first arrival free of a shader compile, and the await spans both backend spellings of the return
    yield* Effect.forkScoped(Effect.promise(async () => await booted.prefilter.compileEquirectangularShader()))
    yield* Effect.forkScoped(Stream.runForEach(arrivals, (arrival) =>
      Effect.gen(function* () {
        const held = yield* Ref.get(slot)
        const acquired = yield* ScopedRef.get(backplane) // per arrival: the prefilter a re-init retired is never reached
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
              const source = yield* _decoded(row, arrival, codecs, _ENV.transfer, "decode-refused")
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

- Owner: `Glb.draw` collapses merge, instancing, batching, visibility, tint, and culling into keyed rows.
- Law: `Frame.Residency.kind` selects splat and culling posture once per graft.

```typescript signature
import { Array, Effect, Function, Option, type Scope } from "effect"
import { BatchedMesh, Color, Frustum, InstancedMesh, LinearSRGBColorSpace, Matrix4, Sphere, Vector4 } from "three"
import type { BufferGeometry, Material, Object3D, PerspectiveCamera } from "three"
import { mergeGeometries } from "three/addons/utils/BufferGeometryUtils.js"
import { bool, Fn, instancedArray, instanceIndex, select, uint, uniform } from "three/tsl"
import { WebGPURenderer } from "three/webgpu"
import type { StorageBufferNode, UniformNode } from "three/webgpu"
import { MeshBVH, StaticGeometryGenerator } from "three-mesh-bvh"

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

// same-frame submeshes only: this fold applies NO transform, so it collapses draw calls and never bakes a scene
const _merged = (parts: ReadonlyArray<BufferGeometry>): BufferGeometry => mergeGeometries([...parts])

// the ONE per-slot visibility write both drivers reach: the selection echo and the cluster cull over the rows
// `Frame.Residency.kind[kind].coneCullable` admits — a second path would let one driver's hide survive the other's show
const _visible = (batch: BatchedMesh, slots: ReadonlyArray<readonly [instance: number, shown: boolean]>): void => {
  // BOUNDARY ADAPTER — the batched slot plane is an imperative per-instance surface
  Array.forEach(slots, ([instance, shown]) => void batch.setVisibleAt(instance, shown))
}

// the tint rides `[8]`'s colour contract, not a second one: the triple arrives from `Theme.linear` and ingests
// under three's ColorManagement exactly as the lobe bind's own `_tint` does
const _tinted = (
  batch: BatchedMesh,
  slots: ReadonlyArray<readonly [instance: number, linear: readonly [number, number, number]]>,
): void => {
  // BOUNDARY ADAPTER — one scratch Color serves the whole sweep; setColorAt copies, so no handle escapes
  const ink = new Color()
  Array.forEach(slots, ([instance, linear]) =>
    void batch.setColorAt(instance, ink.setRGB(linear[0], linear[1], linear[2], LinearSRGBColorSpace)))
}

// the closed frustum: six planes, so the test is a graph-build fold and the per-frame CPU cost is six vec4 writes
const _PLANES = 6

// the dispatch policy beside the collapse rows: one workgroup width the kernel declares and the pass reuses
const _CULL = { workgroup: [64, 1, 1] } as const satisfies { workgroup: ReadonlyArray<number> }

declare namespace Glb {
  // the pass holds its own GPU buffers for the batch's whole life, so the frame call carries only the camera
  type Pass = (camera: PerspectiveCamera) => Effect.Effect<void>
  type Cluster = {
    readonly count: number
    readonly bounds: StorageBufferNode<"vec4">
    readonly verdict: StorageBufferNode<"uint">
    readonly planes: ReadonlyArray<UniformNode<"vec4", Vector4>>
  }
}

// BOUNDARY ADAPTER — the batched slot plane is an imperative per-instance surface and its readers write into
// caller-owned scratch, so one scratch pair serves the whole sweep and the packed draft detaches at the return.
// The world sphere is the geometry sphere placed by the instance matrix, its radius scaled by the largest axis so
// a non-uniformly scaled part is bounded conservatively rather than culled while still on screen.
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

// BOUNDARY ADAPTER — three's frustum writes into its own plane array, and the coordinate system is the renderer's
// own read because WebGPU's clip volume is not WebGL's; the six uniforms take the planes as (normal, constant)
const _framed = (
  camera: PerspectiveCamera,
  renderer: WebGPURenderer,
  frustum: Frustum,
  view: Matrix4,
  planes: Glb.Cluster["planes"],
): void => {
  frustum.setFromProjectionMatrix(view.multiplyMatrices(camera.projectionMatrix, camera.matrixWorldInverse), renderer.coordinateSystem)
  planes.forEach((slot, at) => {
    const plane = frustum.planes[at]! // sanctioned assertion: the uniform roster is minted at the frustum's own arity
    slot.value.set(plane.normal.x, plane.normal.y, plane.normal.z, plane.constant)
  })
}

// BOUNDARY ADAPTER — the readback is a flat typed array and the sink takes slot rows, so the fold runs here and
// the row set detaches immutable at the return; a live batch shorter than its own capacity leaves the tail unread
const _verdicts = (batch: BatchedMesh, verdict: Uint32Array): ReadonlyArray<readonly [instance: number, shown: boolean]> => {
  const rows: Array<readonly [number, boolean]> = []
  for (let slot = 0; slot < batch.instanceCount; slot += 1) rows.push([slot, verdict[slot] === 1] as const)
  return rows
}

// the kernel: one invocation per instance, the six-plane test folded into one boolean chain at graph-build time
// because the frustum is closed — a signed distance below the negated radius rejects, everything else survives
const _kernel = (cluster: Glb.Cluster) =>
  Fn(() => {
    const sphere = cluster.bounds.element(instanceIndex).toVar()
    const inside = cluster.planes.reduce(
      (held, plane) => held.and(plane.xyz.dot(sphere.xyz).add(plane.w).greaterThanEqual(sphere.w.negate())),
      bool(true),
    )
    cluster.verdict.element(instanceIndex).assign(select(inside, uint(1), uint(0)))
  })().compute(cluster.count, [..._CULL.workgroup])

// ONE scene-resident compute lane over ONE sink: the buffers live for the batch's whole life under the asking
// scope, the frame call refreshes the six planes and the cluster bounds, dispatches, reads the verdict back into
// its own held store, and folds it into `Glb.visible` — the same per-slot write the selection echo reaches, so a
// cull and a hide can never disagree about which path owns visibility.
const _cull = (batch: BatchedMesh, acquired: Glb.Acquired): Effect.Effect<Glb.Pass, never, Scope.Scope> =>
  Option.match(
    // the class probe lifted to a value: `computeAsync` and the node materials live on the unified renderer alone,
    // and the refinement is what carries the acquisition's own invariant into the type plane with no cast at the
    // dispatch site, while `Acquired.device` stays the DEVICE seam the `tgpu` altitude adopts
    Option.liftPredicate(acquired.renderer, (renderer): renderer is WebGPURenderer => renderer instanceof WebGPURenderer),
    {
      onNone: () => Effect.succeed<Glb.Pass>(() => Effect.void), // the WebGL floor draws the same scene, uncullled
      onSome: (renderer) =>
        Effect.map(
          Effect.acquireRelease(
            Effect.sync(() => {
              // BOUNDARY ADAPTER — the storage attributes are GPU resources, so the bracket is the folder's law,
              // and both the upload draft and the readback store are held once rather than reallocated per frame
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
                cluster.bounds.value.needsUpdate = true // the attribute wraps the draft, so the write IS the upload
              })
              yield* Effect.promise(() => renderer.computeAsync(kernel, cluster.count))
              const read = yield* Effect.promise(() => renderer.getArrayBufferAsync(cluster.verdict.value, store))
              yield* Effect.sync(() => _visible(batch, _verdicts(batch, new Uint32Array(read))))
            }),
        ),
    },
  )

// this world-space peer bakes one assembly through its own transforms and indexes it by ONE tree, so section-plane
// overlap and point-to-surface measure descend once instead of iterating parts. The build is synchronous because
// it answers a query already in flight, not a graft — a residency tree is `[5]`'s and rides the worker — and it is
// bracketed on the ASKING query's scope, because a bake outside the ledger has no release visitor to free it.
const _baked = (subtree: Object3D): Effect.Effect<{ readonly geometry: BufferGeometry; readonly tree: MeshBVH }, never, Scope.Scope> =>
  Effect.acquireRelease(
    Effect.sync(() => {
      // BOUNDARY ADAPTER — the generator is configure-then-generate; the pair detaches as one immutable query structure
      const generator = new StaticGeometryGenerator(subtree)
      generator.applyWorldTransforms = true
      const geometry = generator.generate()
      return { geometry, tree: new MeshBVH(geometry, _options(Function.constVoid)) }
    }),
    // the bake's own index frees ahead of the buffers it indexes, mirroring `[5]`'s release order exactly
    (baked) => Effect.sync(() => void baked.geometry.dispose()),
  )
```

## [08]-[APPEARANCE_BIND]

- Owner: `Pbr` binds decoded OpenPBR values and addressed texture planes to declared Three.js slots.
- Law: one role table owns channel seating, color transfer, packing, and refusal evidence.

```typescript signature
type MaterialRow = Material
import { Array, Effect, HashMap, Option } from "effect"
import {
  ClampToEdgeWrapping, DoubleSide, FrontSide, LinearSRGBColorSpace, RepeatWrapping,
  type Color, type MeshPhysicalMaterial, type Texture as Plane,
} from "three"

declare namespace Pbr {
  // one seat per mesh: the census row always, the override and its lobe graph where the roster carried them, and
  // plus the plane set where one hangs behind the same appearance key
  type Bound = {
    readonly appearance: Digest.Key<"content">
    readonly summary: AppearanceSummary
    readonly override: Option.Option<MaterialRow>
    readonly set: Option.Option<TextureSet>
  }
  // this resolved appearance ledger holds one map per join the wire actually proves, each under the key it is
  // addressed by, so no lookup re-derives a coordinate and no second roster exists; the lobe graph is not a map
  // because the wire nests it inline — `override.openPbr` IS the lobe read, and a separate lobes roster would be
  // a second truth a dangling digest could fork
  type Index = {
    readonly seats: HashMap.HashMap<Digest.Key<"content">, Pbr.Seat>
    readonly overrides: HashMap.HashMap<Digest.Key<"content">, MaterialRow>
    readonly worn: HashMap.HashMap<Digest.Key<"content">, Digest.Key<"content">>
  }
  type Seat = { readonly summary: AppearanceSummary; readonly set: Option.Option<TextureSet> }
  // `slot` is null for the fifteen roles `MeshPhysicalMaterial` declares nothing for; `component` is the LOWEST
  // stored texel width carrying the swizzle three's own shader chunk samples, so the width proof is arithmetic
  // against `TextureVocab.rows.plane[format].width` rather than a per-role exception; `scalar` is the lobe field a false pack
  // slot neutralizes and the companion a debug view reads
  type RoleSlot = {
    readonly slot: keyof MeshPhysicalMaterial | null
    readonly component: 1 | 2 | 3 | 4 | null
    readonly scalar: keyof MeshPhysicalMaterial | null
  }
  // ONE seating request whichever row minted it: a channel row seats one role from its own plane and a pack row
  // seats its present slots from a shared one, so decode, stamp, assignment, and refusal have a single owner
  type Seating = {
    readonly address: readonly [Digest.Key<"content">, string]
    readonly container: TextureVocab.Container
    readonly format: TextureVocab.PlaneFormat
    readonly transfer: TextureSet["channels"][number]["transfer"]
    readonly pack: Option.Option<TextureVocab.Pack>
    readonly present: ReadonlyArray<TextureVocab.Role>
    readonly absent: ReadonlyArray<TextureVocab.Role>
  }
  type Shape = {
    readonly roles: typeof _ROLE_SLOT
    readonly index: typeof _index
    readonly resolve: typeof _resolve
    readonly seat: typeof _seat
  }
}

// This table projects the whole role roster: the slotted rows reach a `MeshPhysicalMaterial` slot and the
// rest reach none. Component widths follow the swizzle three's shader chunks sample — `map_fragment` `.rgba`,
// `aomap_fragment`/`transmission_fragment` and the clearcoat/iridescence weight reads `.r`/`.x`,
// `roughnessmap_fragment`/`alphamap_fragment` and the clearcoat/iridescence-thickness reads `.g`/`.y`,
// `metalnessmap_fragment` `.b`, the normal and colour reads `.xyz`/`.rgb`, and `lights_physical_fragment`'s
// specular-intensity and sheen-roughness reads `.a`. Every unslotted row is POLICY: tangent frames are the
// `TANGENT` accessor's, curvature and the subsurface pair are probe analysis planes, the rest are scalar lobes.
const _ROLE_SLOT: { readonly [K in TextureVocab.Role]: Pbr.RoleSlot } = {
  base_weight: { slot: null, component: null, scalar: null },
  base_color: { slot: "map", component: 4, scalar: "color" },
  base_metalness: { slot: "metalnessMap", component: 3, scalar: "metalness" },
  base_diffuse_roughness: { slot: null, component: null, scalar: null },
  base_specular_tint: { slot: null, component: null, scalar: null },
  specular_weight: { slot: "specularIntensityMap", component: 4, scalar: "specularIntensity" },
  specular_color: { slot: "specularColorMap", component: 3, scalar: "specularColor" },
  specular_roughness: { slot: "roughnessMap", component: 2, scalar: "roughness" },
  specular_roughness_anisotropy: { slot: "anisotropyMap", component: 3, scalar: "anisotropy" },
  // three exposes no rotation MAP — the direction rides anisotropyMap's RG encode; the plane stays a scalar lobe
  specular_roughness_anisotropy_rotation: { slot: null, component: null, scalar: "anisotropyRotation" },
  specular_ior: { slot: null, component: null, scalar: "ior" },
  transmission_weight: { slot: "transmissionMap", component: 1, scalar: "transmission" },
  transmission_roughness: { slot: null, component: null, scalar: null },
  subsurface_weight: { slot: null, component: null, scalar: null },
  subsurface_radius: { slot: null, component: null, scalar: null },
  coat_weight: { slot: "clearcoatMap", component: 1, scalar: "clearcoat" },
  coat_color: { slot: null, component: null, scalar: null },
  coat_roughness: { slot: "clearcoatRoughnessMap", component: 2, scalar: "clearcoatRoughness" },
  coat_ior: { slot: null, component: null, scalar: null },
  fuzz_weight: { slot: null, component: null, scalar: "sheen" },
  fuzz_color: { slot: "sheenColorMap", component: 3, scalar: "sheenColor" },
  fuzz_roughness: { slot: "sheenRoughnessMap", component: 4, scalar: "sheenRoughness" },
  thin_film_weight: { slot: "iridescenceMap", component: 1, scalar: "iridescence" },
  thin_film_thickness: { slot: "iridescenceThicknessMap", component: 2, scalar: "iridescenceThicknessRange" },
  thin_film_ior: { slot: null, component: null, scalar: "iridescenceIOR" },
  emission_color: { slot: "emissiveMap", component: 3, scalar: "emissive" },
  emission_luminance: { slot: null, component: null, scalar: "emissiveIntensity" },
  geometry_opacity: { slot: "alphaMap", component: 2, scalar: "opacity" },
  geometry_normal: { slot: "normalMap", component: 3, scalar: "normalScale" },
  geometry_coat_normal: { slot: "clearcoatNormalMap", component: 3, scalar: "clearcoatNormalScale" },
  geometry_tangent: { slot: null, component: null, scalar: null },
  geometry_coat_tangent: { slot: null, component: null, scalar: null },
  height: { slot: "displacementMap", component: 1, scalar: "displacementScale" },
  occlusion: { slot: "aoMap", component: 1, scalar: "aoMapIntensity" },
  curvature: { slot: null, component: null, scalar: null },
}

// glTF declares its scene in metres and the set declares its height span in millimetres: this is the ONE unit
// crossing the seat performs, because every other slot three exposes is dimensionless or already carries the
// wire's own unit (`iridescenceThicknessRange` is nanometres on both sides of the seam)
const _WORLD = { metrePerMillimetre: 1e-3 } as const satisfies { metrePerMillimetre: number }

// BOUNDARY ADAPTER — three's material is a mutable platform record and the slot name is proven against
// `keyof MeshPhysicalMaterial` at its row, so this pins the platform's value plane at ONE site and nowhere else
const _write = (material: MeshPhysicalMaterial, slot: keyof MeshPhysicalMaterial, value: unknown): void => {
  ;(material as unknown as Record<keyof MeshPhysicalMaterial, unknown>)[slot] = value
}

const _tint = (target: Color, triple: readonly [number, number, number]): Color =>
  target.setRGB(triple[0], triple[1], triple[2], LinearSRGBColorSpace)

const _lobes = (material: MeshPhysicalMaterial, groups: PbrGroups): MeshPhysicalMaterial => {
  // BOUNDARY ADAPTER — the arm order IS the wire's key order, so a projection change lands as one field wave.
  // Slots the wire carries and three declares no channel for stay render-unbound by law, never forged from a
  // neighbouring band: `base.diffuseRoughness`, `base.specularTint`, `coat.color`, `coat.ior`,
  // `transmission.roughness`, and the whole `subsurface` band; attenuation likewise stays at three's defaults
  // because the wire carries no transmission colour or depth column.
  _tint(material.color, groups.base.color.rgb)
  material.metalness = groups.base.metalness
  // three carries ONE microfacet roughness and it is the wire's SPECULAR roughness — the diffuse band's
  // Oren-Nayar roughness is a different lobe fact and never lands here
  material.roughness = groups.specular.roughness
  _tint(material.specularColor, groups.specular.color.rgb)
  material.specularIntensity = groups.specular.weight
  material.ior = groups.specular.ior
  material.anisotropy = groups.specular.anisotropy
  material.anisotropyRotation = groups.specular.rotation * Math.PI // the wire's half-turn convention: 1 IS π radians
  material.transmission = groups.transmission.weight
  material.clearcoat = groups.coat.weight
  material.clearcoatRoughness = groups.coat.roughness
  material.sheen = groups.fuzz.weight
  _tint(material.sheenColor, groups.fuzz.color.rgb)
  material.sheenRoughness = groups.fuzz.roughness
  material.iridescence = groups.thinFilm.weight
  material.iridescenceIOR = groups.thinFilm.ior
  material.iridescenceThicknessRange = [groups.thinFilm.thickness, groups.thinFilm.thickness] // one film, one nm thickness — both ends of three's range
  _tint(material.emissive, groups.emission.color.rgb)
  material.emissiveIntensity = groups.emission.luminance
  material.opacity = groups.geometry.opacity
  material.transparent = groups.geometry.opacity < 1
  material.side = groups.geometry.thinWalled ? DoubleSide : FrontSide
  return material
}

// this set-level stamp reads four sources and nothing else. `flipY` is never written — the frozen storage origin is
// top-left and every decode leg already defaults it false — and the KTX2 leg stamps `premultiplyAlpha` itself
// off the DFD alpha flag, so this write is the authority for the bitmap and data-texture legs alone.
const _stamped = (plane: Plane, set: TextureSet, acquired: Glb.Acquired): Plane => {
  // BOUNDARY ADAPTER
  plane.wrapS = set.tiled ? RepeatWrapping : ClampToEdgeWrapping
  plane.wrapT = plane.wrapS
  plane.anisotropy = acquired.anisotropy
  plane.channel = 0
  plane.premultiplyAlpha = set.alphaMode === "associated"
  acquired.renderer.initTexture(plane)
  return plane
}

// both row shapes project into ONE request: a channel row carries its single role present, a pack row splits its
// frozen slot triple on the `present` flags so the shared plane seats the true slots and the false ones fall to
// their channel neutral. Level entry zero is the base level; a self-pyramiding container holds only that entry.
const _seatings = (set: TextureSet): ReadonlyArray<Pbr.Seating> => [
  ...Array.map(set.channels, (row): Pbr.Seating => ({
    address: [set.setKey, row.levels[0].file] as const,
    container: row.container,
    format: row.format,
    transfer: row.transfer,
    pack: Option.none(),
    present: [row.role],
    absent: [],
  })),
  ...Array.map(set.packs, (row): Pbr.Seating => {
    const slots = TextureVocab.rows.pack[row.pack].slots
    const present = Array.filterMap(slots, (role, at) => (row.present[at] ? Option.some(role) : Option.none()))
    return {
      address: [set.setKey, row.levels[0].file] as const,
      container: row.container,
      format: row.format,
      // a pack's slots are linear-authored by roster construction, so the shared plane's transfer reads off the
      // first slot's own channel column rather than a literal the table would have to keep in step
      transfer: TextureVocab.rows.channel[slots[0]].transfer,
      pack: Option.some(row.pack),
      present,
      absent: Array.filterMap(slots, (role, at) => (row.present[at] ? Option.none() : Option.some(role))),
    }
  }),
]

// every refusal a seating can carry, read off wire columns alone — no predicate widens as roles or packs grow
const _unseatable = (set: TextureSet, seating: Pbr.Seating, mesh: string): ReadonlyArray<GlbFault> => {
  const refuse = (reason: GlbFault.Reason, detail: string): GlbFault => new GlbFault({ reason, mesh, detail })
  return [
    ...(TextureVocab.rows.plane[seating.format].web ? [] : [refuse("plane-unbound", "<store-undecodable>")]),
    ...(TextureVocab.rows.layer[set.layerLaw].gltf ? [] : [refuse("plane-unbound", "<layer-law-unseated>")]),
    ...(set.udimTiles.length === 0 ? [] : [refuse("plane-unbound", "<udim-unsampled>")]),
    // inverting the pack order swaps R and B, so a consumer binding it to three's slots reads occlusion as
    // metalness — a refusal only the column can declare, and the column is the wire's own
    ...(Option.exists(seating.pack, (pack) => !TextureVocab.rows.pack[pack].gltf)
      ? [refuse("manifest-skew", "<pack-read-order>")]
      : []),
    ...Array.flatMap(seating.present, (role) =>
      _ROLE_SLOT[role].slot === null
        ? [refuse("plane-unbound", `<role-unslotted:${role}>`)]
        : TextureVocab.rows.plane[seating.format].width < (_ROLE_SLOT[role].component ?? 0)
          ? [refuse("manifest-skew", `<component-unstored:${role}>`)]
          : []),
  ]
}

// one seated plane, one refusal set: the roster row decodes it, the policy stamps it, and every present slot
// takes the SAME handle so a pack costs one upload and `[5]`'s deduped walk frees it once
const _seated = (
  material: MeshPhysicalMaterial,
  set: TextureSet,
  seating: Pbr.Seating,
  port: GlbViewport.Appearance,
  codecs: Glb.Codecs,
  acquired: Glb.Acquired,
): Effect.Effect<ReadonlyArray<GlbFault>> =>
  Array.match(_unseatable(set, seating, `${set.setKey}/${seating.address[1]}`), {
    // a refused seating never reaches the network: the wire columns already decided it, so no plane is pulled
    onNonEmpty: (refused) => Effect.succeed<ReadonlyArray<GlbFault>>(refused),
    onEmpty: () =>
      Effect.gen(function* () {
        const octets = yield* port.planes(seating.address)
        const plane = yield* _decoded(
          _CONTAINERS[seating.container],
          { key: set.setKey, octets },
          codecs,
          seating.transfer,
          "plane-unbound",
        )
        return yield* Effect.sync(() => {
          // BOUNDARY ADAPTER — the shared handle lands in every present slot; a false pack slot takes its channel
          // neutral on the scalar companion, which is the only reader that column has
          const stamped = _stamped(plane, set, acquired)
          seating.present.forEach((role) => {
            const row = _ROLE_SLOT[role]
            if (row.slot !== null) _write(material, row.slot, stamped)
            if (role === "height" && row.scalar !== null) {
              _write(material, row.scalar, set.heightScale * _WORLD.metrePerMillimetre)
              _write(
                material,
                "displacementBias",
                -set.heightScale * _WORLD.metrePerMillimetre * TextureVocab.rows.channel.height.neutral[0],
              )
            }
          })
          seating.absent.forEach((role) => {
            const row = _ROLE_SLOT[role]
            if (row.scalar !== null) _write(material, row.scalar, TextureVocab.rows.channel[role].neutral[0])
          })
          return []
        })
        // a plane refusal is EVIDENCE on the one queue, never a failed seat: the material keeps the slot the
        // graft gave it and the remaining seatings still run
      }).pipe(Effect.catchAll((refusal: GlbFault) => Effect.succeed<ReadonlyArray<GlbFault>>([refusal]))),
  })

// ONE seat: lobes then planes, refusals accumulated and never raised — a partially-bound set renders the
// authored asset with its seatable planes overlaid, which is exactly what `plane-unbound` declares
const _seat = (
  material: MeshPhysicalMaterial,
  bound: Pbr.Bound,
  port: GlbViewport.Appearance,
  codecs: Glb.Codecs,
  acquired: Glb.Acquired,
): Effect.Effect<ReadonlyArray<GlbFault>> =>
  Effect.map(
    Option.match(bound.set, {
      onNone: () => Effect.succeed<ReadonlyArray<ReadonlyArray<GlbFault>>>([]),
      onSome: (set) =>
        Effect.forEach(_seatings(set), (seating) => _seated(material, set, seating, port, codecs, acquired)),
    }),
    (refusals) => {
      // BOUNDARY ADAPTER — the lobe fold runs first so a seated plane always modulates a carried scalar, and
      // `needsUpdate` closes the whole seat once: assigning a map where there was none demands the recompile
      Option.map(bound.override, (row) => _lobes(material, row.openPbr))
      material.needsUpdate = true
      return Array.flatten(refusals)
    },
  )

// this census IS the index: the summary roster fixes the appearance key space and every other roster joins onto it
const _index = (document: GlbViewport.Appearance): Effect.Effect<Pbr.Index, GlbFault> =>
  Effect.gen(function* () {
    const seats = Array.reduce(
      document.summaries,
      HashMap.empty<Digest.Key<"content">, Pbr.Seat>(),
      (held, summary) =>
        HashMap.set(held, summary.appearanceKey, {
          summary,
          set: Array.findFirst(document.sets, (set) => set.appearanceKey === summary.appearanceKey),
        }),
    )
    // these refusals make the dangling-reference clause reachable: an override fetched under an uncensused key, a
    // set hanging behind an unlisted key, or a worn mesh naming an uncensused appearance each refuse HERE; a
    // dangling lobe graph is UNCONSTRUCTIBLE — the wire nests the whole vector inline, so no digest exists to fork
    yield* Effect.forEach(document.materials, ([appearance]) =>
      HashMap.has(seats, appearance)
        ? Effect.void
        : Effect.fail(new GlbFault({ reason: "manifest-skew", mesh: `${appearance}`, detail: "<override-uncensused>" })))
    yield* Effect.forEach(document.sets, (set) =>
      HashMap.has(seats, set.appearanceKey)
        ? Effect.void
        : Effect.fail(new GlbFault({ reason: "manifest-skew", mesh: `${set.appearanceKey}`, detail: "<set-uncensused>" })))
    yield* Effect.forEach(document.worn, ([mesh, appearance]) =>
      HashMap.has(seats, appearance)
        ? Effect.void
        : Effect.fail(new GlbFault({ reason: "manifest-skew", mesh: `${mesh}`, detail: "<appearance-uncensused>" })))
    return {
      seats,
      overrides: Array.reduce(document.materials, HashMap.empty<Digest.Key<"content">, MaterialRow>(), (held, [appearance, override]) =>
        HashMap.set(held, appearance, override)),
      worn: Array.reduce(document.worn, HashMap.empty<Digest.Key<"content">, Digest.Key<"content">>(), (held, [mesh, appearance]) =>
        HashMap.set(held, mesh, appearance)),
    }
  })

// one mesh in, its whole appearance out: an unpaired mesh answers none and keeps the GLB material the graft parsed
const _resolve = (index: Pbr.Index, mesh: Digest.Key<"content">): Option.Option<Pbr.Bound> =>
  Option.flatMap(HashMap.get(index.worn, mesh), (appearance) =>
    Option.map(HashMap.get(index.seats, appearance), (seat) => {
      return {
        appearance,
        summary: seat.summary,
        override: HashMap.get(index.overrides, appearance),
        set: seat.set,
      }
    }))

const Pbr: Pbr.Shape = { roles: _ROLE_SLOT, index: _index, resolve: _resolve, seat: _seat }
```

## [09]-[INSTANCED_ROWS]

- Owner: `Instanced` projects georeferenced instances into deck-layer values.
- Boundary: the shared scene port supplies verified identities and appearance data.

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

- Owner: the embed row brackets object URLs and seats decoder assets from the same roster.
- Law: embed is a backend arm of `Glb`, not a second residency owner.

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
    // the address form is `_CODECS`' own column, so this pin and `[5]`'s construction can never disagree about
    // which leaf a directory-joining decoder receives
    const paths = Record.map(assets, (asset, slug) => _ADDRESS[_CODECS[slug].address](asset))
    yield* Effect.suspend(() =>
      Option.match(_pin, {
        onNone: () =>
          Effect.sync(() => {
            _pin = Option.some(paths)
            ModelViewerElement.dracoDecoderLocation = paths.draco
            ModelViewerElement.ktx2TranscoderLocation = paths.ktx2
            ModelViewerElement.meshoptDecoderLocation = paths.meshopt
            // the embed arm's own hint beside its own pin: this leaf is the one the DOCUMENT executes as a
            // classic script, which is the `script` modality no other consumption of the same address earns
            _HINTS.script(paths.meshopt)
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
    readonly sampling: typeof _SAMPLING
    readonly env: typeof _ENV
    readonly rig: typeof _RIG
    readonly bvh: typeof _BVH
    readonly codecRows: typeof _CODECS
    readonly lit: typeof _lit
    readonly renderer: typeof _renderer
    readonly backplane: typeof _backplane
    readonly lifecycle: typeof _booted
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
    readonly baked: typeof _baked
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
  baked: _baked,
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

- [APPEARANCE_KEY_SPELLING]-[OPEN]: which identity does `MaterialWire.key` carry; verify against the C# appearance projection.
- [ACESCC_WORKING_SPACE]-[OPEN]: where does AP1-to-Rec.709 conversion occur; verify producer and three color management.
- [PREMULTIPLY_ON_DATA_TEXTURE]-[OPEN]: does `premultiplyAlpha` affect data textures; verify three's upload implementation.
- [TRANSCODE_TARGET_REBIND]-[OPEN]: can `detectSupport` rebind an existing KTX2 loader; verify the shipped loader source.
- [STORAGE_READBACK_SHAPE]-[OPEN]: does WebGPU readback accept instanced storage attributes; verify the backend implementation.
- [SPLAT_BLEND_ORDER]-[OPEN]: which blend and ordering policy does the splat encoding require; verify the producer contract.
