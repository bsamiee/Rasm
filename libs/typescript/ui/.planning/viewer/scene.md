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

- Owner: `GlbViewport` carries the admitted residency manifest, verified arrivals, dome data, appearances, and cache ports.
- Law: only verified whole-buffer octets enter the viewer; every geometry arrival carries the manifest epoch it was planned under.
- Boundary: runtime owns hauling and cache policy; UI owns decoding and GPU residency.

```typescript signature
import { Digest, Frame, Wire } from "@rasm/ts/core"
import { Context, type Effect, type HashMap, type Option, type Stream, type Subscribable } from "effect"

type AppearanceSummary = InstanceType<typeof Wire.AppearanceSummary>
type Material = InstanceType<typeof Wire.Material>
type PbrGroups = typeof Wire.PbrGroups.Type
type TextureSet = InstanceType<typeof Wire.TextureSet>

declare namespace GlbViewport {
  // generic arguments carry the whole-buffer contract: a shared backing store reaches no `three` decode entry.
  // `epoch` is the hauling side's REPLACEMENT counter, not a wire column — the residency manifest carries none, and
  // `Frame.ResidencyManifest.version` is the producer's schema pin, equal on every lawful emission, so keying skew
  // on it would pin every arrival identical and pass the guard below on a manifest two replacements out of date
  type Arrival = { readonly key: Digest.Key<"content">; readonly epoch: number; readonly octets: Uint8Array<ArrayBuffer> }
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
  // The admitted manifest as this viewer consumes it. The VIEW crosses rather than the bare manifest, because core's
  // admission already folded the per-kind `census` and the `resident` byte total in the same pass it graded the
  // budget — re-summing either here would fork one fold into two. `index` is built ONCE per replacement, which the
  // manifest's own duplicate-key filter proves injective, so the insert lane's membership test costs a hash rather
  // than a scan of every tile per arrival. `epoch` is the hauling side's replacement counter, the one ordinal a
  // graft's lifetime is keyed on.
  type Residency = {
    readonly view: Frame.ResidencyView
    readonly index: HashMap.HashMap<Digest.Key<"content">, Frame.ResidencyTile>
    readonly epoch: number
  }
}

class GlbViewport extends Context.Tag("ui/viewer/GlbViewport")<GlbViewport, {
  // absence is a real state and rides the option carrier: a viewport before its first admitted manifest holds no
  // resident set, and a fabricated empty one would read exactly as a producer that emitted an empty viewpoint
  readonly manifest: Subscribable.Subscribable<Option.Option<GlbViewport.Residency>>
  readonly arrivals: Stream.Stream<GlbViewport.Arrival, GlbFault>
  readonly environments: Stream.Stream<GlbViewport.Environment, GlbFault>
  readonly appearances: Stream.Stream<GlbViewport.Appearance, GlbFault>
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
import { Convention, Fault, Shape } from "@rasm/ts/core"
import { Schema } from "effect"

// One row per reason: the core kind, the owning leg, and the subject that reason alone renders. Severity, retry,
// blame, and quarantine are the core Fault.Class row table's — a rank or retry literal here would fork them. The
// LEG is the lifecycle guard's own column: `codec-absent` and `backend-lost` share the `unavailable` class, so a
// reason literal in the guard would move the phase on an unserved decoder, and reading the leg back keeps one
// authority where an `arm` column beside it was a second name for the same fact.
const _reasons = ["manifest-skew", "decode-refused", "codec-absent", "plane-unbound", "backend-lost"] as const

// What a codec refusal names is a CLOSED set — the roster row, the wire codec, the loadable module — so an operator
// reads which of the three the deployment failed to serve rather than a bracketed token parsed back out of prose.
const _unserved = ["asset-row", "meshopt-codec", "decoder-module"] as const

// Eviction is the one axis neither the class nor the leg decides: a skewed manifest and a refused decode each
// condemn the held bytes, while an unserved codec and an unseatable plane leave them sound. Content-key
// verification is NOT this page's — `system/cache` re-mints every leaf through `Digest.mint("content", octets)`
// and raises its own `key-mismatch` there, so bytes reaching this page already answered that gate.
const _faultRows = {
    "manifest-skew": { evict: true },
    "decode-refused": { evict: true },
    "codec-absent": { evict: false },
    // the mesh keeps its GLB-embedded slot: an unseatable plane is evidence over a drawable subtree, never an eviction
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
  // A boot-time backend refusal names no mesh — the `<boot>` placeholder it once carried was a coordinate no reader
  // could resolve — so the row carries the acquisition stage the renderer died at instead.
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
  // the indicted plane: an asset refusal is contained by the lane that raised it, a backend refusal is a lifecycle
  // event — and it derives off the family's own leg column, so the guard and the raiser read one roster
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

// Seating admits every plane INDEPENDENTLY — a store format that decodes nowhere decides nothing about a sibling's
// pack order — so the seat's whole damage rides ONE carrier off this same family rather than an array of faults a
// consumer has to fold itself. Every other lane refuses singly and keeps the plain carrier.
const GlbSeating = _family.census("GlbFault.seating")
type GlbSeating = InstanceType<typeof GlbSeating>

// The frequency aspect and the error-rail fold in ONE declaration, seated beside the family that raises it: the
// mount and the tracking operator were two spellings of one row repeated at four sites, and the census argument is
// the policy vocabulary itself — a flat roster a call site assembles can drift from the reasons the raiser can
// actually mint, while the vocabulary already refused a duplicate word at its own construction.
const _refused = Convention.tracked(Convention.metric.sceneRefusals, _faultPolicy, (fault: GlbFault) => fault.case.reason)
```

## [04]-[BACKEND_SELECT]

- Owner: `Glb` acquires one WebGPU or WebGL renderer, output policy, sampling cap, prefilter, and lifecycle.
- Law: one scoped backplane preserves renderer identity across all decode, upload, dome, and device-loss paths.

```typescript signature
import { Machine } from "@effect/experimental"
import { Convention, Shape } from "@rasm/ts/core"
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
  // paths. Codecs ride the ACQUISITION because the transcoder is renderer-bound and UNREBINDABLE: `detectSupport`
  // only writes a config field, which each worker bakes at CREATION out of a lazily-filled cache, so a re-call
  // after the first parse leaves a split-brain pool; and `dispose()` revokes the worker blob URL while leaving
  // `transcoderPending` resolved, so a disposed instance handed another buffer spawns workers on a dead URL. A
  // backend swap therefore mints a FRESH bundle beside the renderer and the displaced pool dies with it — the
  // one-loader-per-viewport ruling reads per backend GENERATION, and every lane reads the codecs off this record
  // rather than capturing them, so no lane can reach the retired pool.
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
      // the ONE backend discrimination in this module: prefilter class and anisotropy member path both hang off it,
      // so every later lane reads the settled record and never asks which renderer it holds
      const legacy = built instanceof WebGLRenderer
      const prefilter: Glb.Prefilter = legacy ? new GlPrefilter(built) : new GpuPrefilter(built)
      // the codec bundle is SEQUENCED, never assigned: `_codecs` answers an Effect because the transcode target it
      // probes is the backend's own, and the served half it composes was resolved once for the viewport
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
      // BOUNDARY ADAPTER — the transcoder pool and the prefilter release before the renderer that backs them; the
      // SERVED half outlives this scope by construction, so a swap re-pays neither the draco preload nor the
      // meshopt import, and the retired pool is unreachable because nothing captured it
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

const _signals = ["settled", "floored", "revive"] as const

const _GUARDED = "refused" as const // minted only where the fault family's `arm` column reads `backend`

const _acts = ["arm", "park", "chase", "hold"] as const

const _FIBERS = { draw: "draw", revive: "revive", watch: "watch" } as const // keyed children, never a free string at a fork site

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

// ONE owner for the closed phase family: the tuple ranks it, the transition table IS its row payload, and both the
// wire literal and the metric census derive HERE rather than standing as two more spellings of the same five words.
// The tuple survives above because `Glb.Phase` and the table's own `satisfies` guard read it before this mint can.
const _lifecycle = Shape.vocabulary(_phases, _lifecycleRows)

// the lifecycle word census: one frequency over that same roster, so every landed turn is a bounded-tag series and
// the backend-health board reads transitions without a second subscription on the actor
const _PHASED = Convention.mount(Convention.metric.sceneBackend, _lifecycle)

class Advance extends Schema.TaggedRequest<Advance>()("Advance", {
  failure: Schema.Never,
  success: _lifecycle.schema,
  payload: { signal: _Signal },
}) {}

// the guarded arm: the reason travels so the guard reads the policy table's own column, and the detail rides the
// span rather than a message field the fields already prove
class Refuse extends Schema.TaggedRequest<Refuse>()("Refuse", {
  failure: Schema.Never,
  success: _lifecycle.schema,
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
  pipe(_lifecycle.at(phase)[signal], (row) =>
    Effect.as(Effect.zipRight(_ACTS[row.act](turn), Metric.update(_PHASED, row.next)), [row.next, row.next] as const))

const _lifecycle = (plane: Glb.Plane) =>
  Machine.makeSerializable({ state: _lifecycle.schema, input: _Boot }, (input, previous) =>
    // reboot and retry resume through the same slot: `previousState` carries the last live phase across a remount.
    // The initializer answers an EFFECT because that is the shape `InitializeSerializable` declares — a bare
    // procedure list is the input-carrying overload's one refusal, and this list needs no acquisition to build
    Effect.succeed(Machine.serializable.make(previous ?? "booting").pipe(
      Machine.serializable.add(Advance, ({ forkOne, forkReplace, request, send, state }) =>
        _turned({ plane, cool: input.cool, forkOne, forkReplace, send }, state, request.signal)),
      Machine.serializable.add(Refuse, ({ forkOne, forkReplace, request, send, state }) =>
        // the guard is the family's own leg, never a reason literal: `codec-absent` shares `backend-lost`'s
        // `unavailable` class and must still leave the phase exactly where it stands
        _family.legOf(request.reason) === "backend"
          ? Effect.zipRight(
              // the device's own message is per-occurrence evidence, so it rides the request span and never a tag
              Effect.annotateCurrentSpan("glb.backend.loss", request.detail),
              _turned({ plane, cool: input.cool, forkOne, forkReplace, send }, state, _GUARDED),
            )
          : Effect.succeed([state, state] as const)),
    )),
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

- Owner: `Glb.graft` validates the manifest epoch, decodes verified GLB bytes, and owns subtree resources.
- Law: manifest replacement evicts old-epoch grafts even when artifact keys repeat.
- Law: eviction IS absence from the successor's tile set — the manifest names the whole resident set for one viewpoint, so a key it omits is a key the producer released and no row state is owed.
- Law: one traversal owns upload, acceleration, dress, visibility, and release coverage.
- Law: decode capability splits by binding — `Glb.Served` resolves once per viewport, `Glb.Codecs` re-mints per backend generation, and every lane reads the bundle off the live acquisition rather than capturing it.
- Law: a standard-material graft whose appearance demands a physical-only lobe upgrades in place; `<material-unphysical>` names only a material outside that pair.

```typescript signature
import { Convention } from "@rasm/ts/core"
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
  // nodes the graft covered, and `kind` carries the manifest tile's payload axis for the subtree's lifetime
  type Graft = {
    readonly node: Object3D
    readonly mixer: Option.Option<AnimationMixer>
    readonly epoch: number
    readonly kind: Frame.ResidencyKind
    readonly drawn: ReadonlyArray<Glb.Drawable>
  }
  type Ledger = HashMap.HashMap<Digest.Key<"content">, Glb.Graft>
  // one codec record publishes the transcoder beside the loader consuming it, because the dome's deep-store
  // container decodes through that same configured instance and a second one owns a second worker pool. The
  // record is per backend GENERATION and reached only through `Acquired` — a lane capturing it survives the swap
  // that disposes it, which is the split-brain the transcoder's unrebindable worker config makes unrecoverable.
  type Codecs = { readonly gltf: GLTFLoader; readonly ktx2: KTX2Loader }
  // renderer-INDEPENDENT decode capability resolved ONCE per viewport and threaded into every acquisition: the
  // tapped manager, the warmed draco pool, the transcoder directory, and the meshopt module are backend-blind, so
  // they survive every re-init, while the KTX2 transcoder and the loader holding it are renderer-BOUND and
  // re-mint on `Acquired`. This split is what keeps a backend swap off the draco preload and the module import.
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
  // `Admitted` carries the replacement itself, and it carries the VIEW: core's admission already folded the per-kind
  // census and the resident byte total against the budget it graded, so the observe point publishes the producer's
  // own figures instead of a second accounting this module would have to keep in step with the tile set.
  type ResidencyFact =
    | { readonly _tag: "Admitted"; readonly view: Frame.ResidencyView; readonly epoch: number }
    | { readonly _tag: "Arrived"; readonly arrival: GlbViewport.Arrival }
    | { readonly _tag: "Environment"; readonly key: Digest.Key<"content"> }
    // a lane refuses singly, a seat refuses in census — both are one refusal fact, so the case admits either whole
    | { readonly _tag: "Refused"; readonly refusal: GlbFault | GlbSeating }
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
    // the fact feed carries NO error channel: every lane catches its refusal into the queue as the `Refused` case,
    // so the drain is `Stream.fromQueue`'s own unfailing stream — which is also the shape `Hook.Row`'s adopted
    // `source` demands, and a declared `GlbFault` here would refuse the hook row while promising a raise no
    // subscriber can ever observe
    readonly facts: Stream.Stream<Glb.ResidencyFact>
    readonly dome: Effect.Effect<Glb.Dome>
    readonly trees: Effect.Effect<Glb.Trees>
    // the re-init entry: the successor's prefilter re-derives the dome target off the SURVIVING decoded source.
    // It carries no codec direction because there is none to carry — the fresh acquisition already holds the
    // fresh bundle and every lane re-reads the cell, so a rebind that handed codecs forward would be the capture
    // this record's generation law forbids
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
    onNone: () => Effect.fail(new GlbFault({ case: { reason: "codec-absent", mesh: slug, absent: "asset-row" } })),
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
    catch: (defect) => new GlbFault({ case: { reason: "decode-refused", mesh: `${key}`, cause: String(defect) } }),
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
        // Compositing is straight-alpha OVER: the producer wire carries positions, scales, rotations, harmonics,
        // and sigmoid-activated STRAIGHT alphas — no ordering key and no blend equation ride it — so the decode
        // end fixes the equation (`transparent` with `depthWrite: false`, never a premultiplied or additive
        // re-read) and the ORDER stays the consumer's: `[07]`'s per-view radix-depth fold, which the caller
        // composes as a `Glb.Pass` beside the cull. The WebGL floor keeps the parse's own material and therefore
        // composites unsorted as its declared forfeit.
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

// the per-SLOT projection the upgrade arm demands: three's material member is a handle OR an array, and a re-point
// must land in the exact slot the displaced material sat in so a multi-material drawable keeps its group order.
// `-1` is the bare-handle spelling, kept as a slot value rather than a second shape, because both forms then take
// one writer. The ledger needs no edit either way: `Glb.Graft.drawn` holds NODES, and the release visitor reads
// `child.material` at teardown, so it always frees whatever the drawable currently wears.
const _slotted = (drawn: Glb.Drawable): ReadonlyArray<readonly [at: number, material: Material]> =>
  Array.isArray(drawn.material) ? drawn.material.map((row, at) => [at, row] as const) : [[-1, drawn.material] as const]

const _repoint = (drawn: Glb.Drawable, at: number, bound: MeshPhysicalMaterial): void => {
  // BOUNDARY ADAPTER
  if (at < 0) drawn.material = bound
  else if (Array.isArray(drawn.material)) drawn.material[at] = bound
}

// ONE slot decision, three outcomes. A physical material seats directly. A STANDARD material whose resolved
// appearance demands a physical-only lobe is upgraded in place — minted, narrow-copied, re-pointed, and seated —
// because the alternative is a silent drop: three declares no coat, sheen, transmission, iridescence, or
// anisotropy member on the standard class, so `_lobes` would write fields the render never reads. Anything else
// is genuinely unseatable and surfaces as evidence. The physical probe runs FIRST and must: `MeshPhysicalMaterial`
// extends `MeshStandardMaterial`, so the standard arm would otherwise swallow every already-physical seat and
// re-mint a target over a material that needed none.
const _dressed = (
  key: Digest.Key<"content">,
  drawn: Glb.Drawable,
  at: number,
  material: Material,
  bound: Pbr.Bound,
  document: GlbViewport.Appearance,
  acquired: Glb.Acquired,
): Effect.Effect<Option.Option<GlbSeating>> =>
  material instanceof MeshPhysicalMaterial
    ? Pbr.seat(material, bound, document, acquired)
    : material instanceof MeshStandardMaterial && Pbr.demands(bound)
      ? Effect.flatMap(
          Effect.sync(() => {
            // BOUNDARY ADAPTER — the copy TRANSFERRED every texture handle to the successor, so `_retire` is the
            // wrong reader here: its slot sweep would free planes the target now samples. Only the displaced
            // program retires, and the release visitor frees the shared handles once, through the live material.
            const upgraded = Pbr.upgrade(material)
            _repoint(drawn, at, upgraded)
            material.dispose()
            return upgraded
          }),
          (upgraded) => Pbr.seat(upgraded, bound, document, acquired),
        )
      : Effect.succeed(Option.some(
        new GlbSeating({ issues: [{ reason: "plane-unbound", mesh: `${key}`, cause: "source material is not physical" }] }),
      ))

// ONE dressing entry over a graft, run at commit and re-run on every fresh appearance document because the seat is
// idempotent pure assignment: `worn` names which meshes carry an appearance and every other mesh keeps the
// material the parse gave it. The `kind` column selects the ARM — a splat-borne row takes the point-radiance bind
// above and never reaches the lobe seat. Every refusal folds into the one bounded fact queue the lanes share.
const _dress = (
  key: Digest.Key<"content">,
  graft: Glb.Graft,
  index: Pbr.Index,
  document: GlbViewport.Appearance,
  // the acquisition carries its own codec bundle, so the dress lane reads ONE renderer-bound carrier and can never
  // pair a live renderer with the transcoder its predecessor disposed
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
              ([drawn, at, material]) => _dressed(key, drawn, at, material, bound, document, acquired),
            ),
            // a seat that bound everything answers no carrier at all, so the offer arm runs only where damage exists
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
  // the CELL, never a captured record: a re-init swaps the acquisition beneath these lanes, so every lane reads
  // the live renderer AND its live codec bundle at the moment it parses, uploads, seats, or prefilters rather
  // than holding a disposed handle or a transcoder whose worker blob URL the swap already revoked
  backplane: Glb.Backplane,
  port: Context.Tag.Service<GlbViewport>,
  // the same dependency tap `_served` hands its LoadingManager: an off-thread tree build reports its own fraction
  // into the residency telemetry rather than minting a second progress channel
  progress: (fraction: number) => void,
  // every lane forks with its refusals contained, so construction succeeds or dies; `_served` alone carries
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
        // the published manifest is the whole authority: an arrival it never named refuses BEFORE any parse, which
        // is the one raiser `manifest-skew` declares, and the tile it does find rides the graft for the subtree's
        // lifetime. The epoch guard runs first, so an arrival from a superseded plan never consults the successor's
        // index and a key repeating across two viewpoints cannot graft under the wrong one.
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
        // read per arrival and BEFORE the parse: a re-init between two arrivals is invisible here, and the loader
        // that decodes these octets must be the one whose transcoder the live backend still owns
        const acquired = yield* ScopedRef.get(backplane)
        const gltf = yield* Effect.tryPromise({
          try: () => acquired.codecs.gltf.parseAsync(arrival.octets.buffer, ""),
          // registered gates reject with their own typed refusal, so codec-absent survives the promise edge
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
        // only triangle payloads index: the snapshot band's cardinality is therefore the MESH count in the walk's
        // own traversal order, so a splat graft files no snapshot and re-opens none
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
          onSome: ([index, document]) => _dress(arrival.key, graft, index, document, acquired, facts),
        })
        yield* Effect.asVoid(Effect.withMetric(Effect.succeed(1), _GRAFTED))
        yield* Queue.offer(facts, { _tag: "Arrived", arrival } as const)
      }).pipe(
        _refused,
        Effect.withSpan("rasm.ui.scene.residency", { attributes: { "glb.bytes": arrival.octets.byteLength } }),
        Effect.annotateLogs({ mesh: `${arrival.key}` }),
        Effect.catchAll((refusal) => Queue.offer(facts, { _tag: "Refused", refusal } as const)),
      )))
    // the replacement lane: one published manifest decides BOTH what survives and what the observe point reports,
    // so the census the fact carries and the eviction set are read off the same value in the same pass
    const evict = Stream.runForEach(port.manifest.changes, (admitted) =>
      Effect.gen(function* () {
        const grafts = yield* Ref.get(held)
        // absence from the successor's tile set IS the eviction — the manifest replaces whole, so a key it omits is
        // a key the producer released, and that answers strictly more than the row state it replaces: a state column
        // could only ever say "evicted" about a row the producer still chose to name
        const gone = HashMap.filter(grafts, (graft, key) =>
          Option.match(admitted, {
            onNone: () => true, // no manifest names anything resident, so nothing may stay grafted
            onSome: (residency) => graft.epoch !== residency.epoch || !HashMap.has(residency.index, key),
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
        // the replacement publishes AFTER its releases, so a subscriber reading the census reads it beside a scene
        // that already dropped what the successor no longer names; `vramBudget` needs no arm here because core's
        // `Frame.Residency.admit` refuses an over-budget manifest before this port ever sees one
        yield* Option.match(admitted, {
          onNone: () => Effect.void,
          onSome: (residency) =>
            Queue.offer(facts, { _tag: "Admitted", view: residency.view, epoch: residency.epoch } as const),
        })
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
          ([key, graft]) => _dress(key, graft, index, document, acquired, facts),
          { discard: true },
        )
      }).pipe(
        _refused,
        Effect.withSpan("rasm.ui.scene.residency"),
        Effect.catchAll((refusal) => Queue.offer(facts, { _tag: "Refused", refusal } as const)),
      ))
    const dome = yield* _dome(root, backplane, port.environments, facts)
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
      : Promise.reject(new GlbFault({ case: { reason: "codec-absent", mesh: _MESHOPT, absent: "meshopt-codec" } })),
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

// One pass ahead of first frame over the roster and the manifest's WHOLE tile set: the decoder leaves the first
// parse will demand and every payload the viewpoint declares resident. No pending filter is owed — the manifest
// names the resident set rather than a work queue, and this pass runs with nothing grafted yet, so every tile is a
// warm candidate. Per-mesh hinting at draw time is the named defect, because a hint landing beside the fetch it
// warms saves nothing. The payload address comes from the port, never from a key: the hauling side owns the served
// coordinate — `blobKey` is ITS column — and this module addresses only what `Glb.assetPath` derives, so a viewer
// minting a GLB URL out of a content key would forge one.
const _warm = (
  roster: Glb.AssetRoster,
  residency: GlbViewport.Residency,
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
    Effect.sync(() =>
      void residency.view.manifest.tiles.forEach((tile) => Option.map(addressed(tile.contentKey), _HINTS.fetch))),
  )

// The renderer-BLIND half, resolved once for the viewport and threaded into every acquisition: the tapped manager
// carries the progress and error channel the whole decode estate reports through, the draco pool warms its wasm
// here so a backend swap never re-pays it, the meshopt module resolves through the module graph its own row's
// `warm` column already hinted, and the transcoder DIRECTORY crosses as a string because the loader consuming it
// is per generation. `codec-absent` raises HERE and nowhere downstream, which is why the graft lane inherits no
// error channel. The draco pool is bracketed because it owns worker threads for the viewport's whole life.
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
        // BOUNDARY ADAPTER — the address form is the row's own column; no site re-decides dir versus file
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

// The renderer-BOUND half, re-minted with every acquisition. A transcoder cannot be re-targeted after the fact —
// `detectSupport` writes a config field that each worker bakes at CREATION out of a lazily-filled pool — so a
// backend swap gets a FRESH loader here rather than a second probe on the incumbent, and the displaced instance
// dies under `_renderer`'s release. Its `dispose()` leaves `transcoderPending` resolved, so re-parsing through a
// retired instance would spawn workers on a revoked blob URL; nothing captures the bundle, which forecloses it.
// The loader publishes beside the transcoder because `[6]`'s deep-store dome decodes through that same instance.
const _codecs = (served: Glb.Served, renderer: WebGLRenderer | WebGPURenderer): Effect.Effect<Glb.Codecs> =>
  Effect.sync(() => {
    // BOUNDARY ADAPTER
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
import { Effect, Option, Queue, Ref, Scope, ScopedRef, Stream } from "effect"
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
    // the decode takes the ACQUISITION, not a codec record: the deep-store leg rides the renderer-bound
    // transcoder, so the one carrier that can never be stale is the cell's own live value
    readonly decode: Option.Option<(payload: Glb.Payload, acquired: Glb.Acquired) => Effect.Effect<Texture, GlbFault>>
  }
  // the two columns a decode reads — the content key its refusal names and the whole-buffer octets. `[8]`'s plane
  // arrives under a SET key belonging to no residency manifest, so the decode takes this pair rather than the
  // arrival record; an epoch column here would be a field only one of the two callers could fill.
  type Payload = Pick<GlbViewport.Arrival, "key" | "octets">
  type Slot = {
    readonly slot: Ref.Ref<Glb.Dome>
    readonly rebind: (acquired: Glb.Acquired) => Effect.Effect<void>
  }
}

// One projection carries the transfer tag onto three's colour-space constants; `[8]`'s per-plane stamp reads the
// wire's own `transfer` column through it and the dome reads `_ENV.transfer`, so every linear plane the estate
// decodes lands under one declaration instead of two literals that drift apart.
// PRIMARIES are not this consumer's to convert. three registers exactly two working spaces — `srgb-linear` and
// `srgb` — and while `ColorManagement.define()` accepts a custom space unvalidated (it is a bare `Object.assign`
// onto the roster), a texture payload TAGGED with a custom linear non-Rec.709 space hard-`error()`s in the WebGL
// backend's upload verification and shader-converts never at all on WebGPU, while float and half-float uploads
// receive no conversion on either. An AP1-primaried plane is therefore converted to Rec.709 by its PRODUCER
// before upload, and this table's `linear` row tags the decoded HDR and float payloads `LinearSRGBColorSpace`
// because that is what they now are. A custom space is admissible only for `Color`-object math and an output
// transform, never as a texture tag.
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
  (payload: Glb.Payload): Effect.Effect<Texture, GlbFault> =>
    Effect.try({
      try: () => open().createDataTexture(payload.octets.buffer),
      catch: (defect) => new GlbFault({ case: { reason: "decode-refused", mesh: `${payload.key}`, cause: String(defect) } }),
    })

// every eight-bit row the browser reaches differs only by media type, so the decode parameterizes over it and
// declares every implicit conversion OFF — colour management, premultiplication, and EXIF orientation each rewrite
// a channel plane's stored value, and the frozen top-left origin survives only where the browser applies none
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
        // BOUNDARY ADAPTER — a bitmap-sourced texture uploads only once `needsUpdate` marks it, and `flipY` is
        // WRITTEN here because the base `Texture` class alone defaults it TRUE: the two DataTextureLoader legs and
        // the deep store construct `DataTexture`/`CompressedTexture`, whose own constructors close it false. Left
        // standing, three would flip the upload on both backends — `UNPACK_FLIP_Y_WEBGL` on WebGL, the copy's own
        // flip on WebGPU — and undo the `imageOrientation: "none"` the decode just asked for.
        const texture = new Texture(bitmap)
        texture.flipY = false
        texture.needsUpdate = true
        return texture
      },
    )

// dual-callback surfaces are the seam Effect.async owns; this instance is [5]'s configured transcoder, its declared
// CompressedTexture is narrower than the DataTexture an uncompressed store yields, and the uncompressed branch
// hands back NearestFilter and NoColorSpace, so the policy stamp closes the decode
const _deep = (payload: Glb.Payload, acquired: Glb.Acquired): Effect.Effect<Texture, GlbFault> =>
  Effect.async<Texture, GlbFault>((resume) =>
    acquired.codecs.ktx2.parse(
      payload.octets.buffer,
      (texture) => resume(Effect.succeed(texture)),
      (defect) =>
        resume(Effect.fail(new GlbFault({ case: { reason: "decode-refused", mesh: `${payload.key}`, cause: String(defect) } }))),
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
  payload: Glb.Payload,
  acquired: Glb.Acquired,
  transfer: keyof typeof _TRANSFER,
  reason: Extract<GlbFault.Reason, "decode-refused" | "plane-unbound">, // the two planes this entry serves, and the two whose subject it can fill
): Effect.Effect<Texture, GlbFault> =>
  Option.match(row.decode, {
    onNone: () => Effect.fail(new GlbFault({ case: { reason, mesh: `${payload.key}`, cause: "the container row declares no decoder" } })),
    onSome: (decode) => Effect.map(decode(payload, acquired), (texture) => _filtered(texture, transfer)),
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
  // re-init prefilters through the live generator and transcodes through the live pool instead of two handles its
  // predecessor's renderer already released
  backplane: Glb.Backplane,
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
            Effect.fail(new GlbFault({ case: { reason: "decode-refused", mesh: `${arrival.key}`, cause: "sh9 carries the wrong coefficient count" } })),
          onSome: Effect.succeed,
        })
        // ONE branch governs the lane: a held key carries its handles forward, anything else decodes and prefilters
        const handles = yield* (Option.exists(held.key, (key) => key === arrival.key)
          ? Effect.succeed({ source: held.source, target: held.target })
          : Effect.gen(function* () {
              const row = yield* Option.match(_sniff(arrival.octets), {
                onNone: () =>
                  Effect.fail(new GlbFault({ case: { reason: "decode-refused", mesh: `${arrival.key}`, cause: "no container row claims these magic bytes" } })),
                onSome: Effect.succeed,
              })
              const source = yield* _decoded(row, arrival, acquired, _ENV.transfer, "decode-refused")
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
        _refused,
        Effect.withSpan("rasm.ui.scene.residency", { attributes: { "glb.bytes": arrival.octets.byteLength } }),
        Effect.annotateLogs({ mesh: `${arrival.key}` }),
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
  // the pass holds its own GPU buffers and scratch for its subject's whole life, so the frame call carries only
  // the camera; the cull and the splat sort are both instances, which is why the frame driver folds one roster
  type Pass = (camera: PerspectiveCamera) => Effect.Effect<void>
  type Cluster = {
    readonly count: number
    readonly bounds: StorageBufferNode<"vec4">
    readonly verdict: StorageBufferNode<"uint">
    readonly planes: ReadonlyArray<UniformNode<"vec4", Vector4>>
  }
  // one snapped surface answer, verbatim from the descent, so no second hit vocabulary stands between the pick
  // plane and the measure
  type Hit = HitPointInfo
  // the cut a section plane contributes, in descent order — the world-space polyline a section overlay draws
  type Cut = ReadonlyArray<Line3>
  // both measure endpoints snapped to the surface, with the span they subtend
  type Span = { readonly from: Glb.Hit; readonly to: Glb.Hit; readonly length: number }
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
              // `getArrayBufferAsync(attribute, target = null, offset = 0, count = -1)` accepts ANY buffer
              // attribute — every attribute path grants `COPY_SRC`, so a storage-instanced attribute needs no
              // second shape — and its only gate is that offset and count stay multiples of four. The three
              // target arms are a real choice: `null` allocates a fresh `ArrayBuffer` per frame, a
              // `ReadbackBuffer` holds a persistent mapped buffer that THROWS unless `.release()` ran since its
              // last read, and a plain `ArrayBuffer` is copied into in place. This pass takes the third, so the
              // per-frame allocation is zero and no release obligation crosses the frame boundary.
              const read = yield* Effect.promise(() => renderer.getArrayBufferAsync(cluster.verdict.value, store))
              yield* Effect.sync(() => _visible(batch, _verdicts(batch, new Uint32Array(read))))
            }),
        ),
    },
  )

// this world-space peer bakes one assembly through its own transforms and indexes it by ONE tree, so the section
// and measure rows below descend once instead of iterating parts. The build is synchronous because
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
    // the buffers are the only RESOURCE the bake holds: a directly constructed tree parks on no geometry slot and
    // owns no GPU handle, so it dies with the record and the release states the one free that is real. `[5]`'s
    // extra `disposeBoundsTree` arm clears a slot the residency walk PARKED, which this bake never fills.
    (baked) => Effect.sync(() => void baked.geometry.dispose()),
  )

// --- [SPLAT_ORDER]

// The splat sort's own policy: sixteen-bit depth buckets, and the cosine below which the view axis counts as a new
// epoch. A splat order is invariant under camera TRANSLATION — every point's depth shifts by the same amount — so
// the axis alone is the epoch, and a still or dollying view re-sorts nothing.
const _SPLAT = { buckets: 1 << 16, epoch: 0.9999 } as const satisfies { buckets: number; epoch: number }

// BOUNDARY ADAPTER — the attribute reader is the one accessor both the interleaved and the flat layout answer, so
// the fold never re-derives a stride the parse already owns
const _depths = (
  position: BufferAttribute | InterleavedBufferAttribute,
  axis: Vector3,
  depths: Float32Array,
): void => {
  for (let point = 0; point < depths.length; point += 1) {
    depths[point] = position.getX(point) * axis.x + position.getY(point) * axis.y + position.getZ(point) * axis.z
  }
}

// The radix pass: one bucket census, a prefix scan, one scatter — linear in the point count where a comparison
// sort is not, which is what makes a per-view re-order affordable on a million-point payload. The scan walks the
// FAR bucket down, so the scatter lands the farthest point first and the index IS the back-to-front draw order.
const _ordered = (depths: Float32Array, keys: Uint16Array, census: Uint32Array, order: Uint32Array): void => {
  // BOUNDARY ADAPTER — three flat passes over caller-held scratch; nothing allocates inside a frame
  let low = depths[0] ?? 0
  let high = low
  for (let at = 0; at < depths.length; at += 1) {
    const depth = depths[at] ?? 0
    low = depth < low ? depth : low
    high = depth > high ? depth : high
  }
  // a degenerate span collapses every point into bucket zero, which leaves the incoming order untouched rather
  // than dividing by nothing — a single-plane scan has no depth order to resolve
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

// ONE per-view fold over a splat payload, composed as a `Glb.Pass` beside the cull. The scratch and the index are
// held for the payload's life because a per-frame allocation at this arity is the named defect, and the pass mints
// no handle the graft does not already own — the index attribute parks on the drawable's own geometry, so `[5]`'s
// release visitor frees it under the same `geometry.dispose()` that frees the positions it orders.
const _sorted = (points: Points): Effect.Effect<Glb.Pass> =>
  Effect.sync(() => {
    // BOUNDARY ADAPTER — the index is minted once at the payload's arity and rewritten in place thereafter
    const position = points.geometry.getAttribute("position")
    const depths = new Float32Array(position.count)
    const keys = new Uint16Array(position.count)
    const census = new Uint32Array(_SPLAT.buckets)
    const order = new Uint32Array(position.count)
    const index = new BufferAttribute(order, 1)
    index.setUsage(DynamicDrawUsage)
    points.geometry.setIndex(index)
    const axis = new Vector3()
    const held = new Vector3() // the zero seed never clears the epoch gate, so the first pass always sorts
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

// The SECTION row `panel#CONTROL_SINKS` routes its `section` sink onto: one descent over the baked assembly,
// pruning every node the plane misses and cutting each surviving triangle into the one segment it contributes.
// `CONTAINED` never arises — a plane bounds nothing — so the bounds column is the two-way prune the descent needs
// and no third verdict is spelled. The bake is the caller's: `_baked` holds the world-space geometry and its tree
// under the asking scope, so a tool session pays one bake and every plane drag descends it.
const _section = (tree: MeshBVH, plane: CutPlane): Effect.Effect<Glb.Cut> =>
  Effect.sync(() => {
    // BOUNDARY ADAPTER — the descent writes into local scratch and the segment set detaches at the return
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
          edge.end.copy(corners[(at + 1) % corners.length]!) // sanctioned assertion: the modulus is the triangle's own arity
          if (plane.intersectLine(edge, crossed) !== null) met.push(crossed.clone())
        })
        // a straddling triangle contributes exactly one segment; a coplanar or vertex-grazing one contributes
        // none, because two coincident crossings carry no direction a section overlay can draw
        if (met.length === 2 && !met[0]!.equals(met[1]!)) cut.push(new Line3(met[0]!, met[1]!))
      },
    })
    return cut
  })

// the point-to-surface leaf both measure endpoints fold through: the descent answers the nearest surface point,
// its distance, and the face it landed on. A probe outside every bound answers NONE rather than a fault, because
// an unreached surface is an absence this return already spells
const _nearest = (tree: MeshBVH, point: Vector3): Effect.Effect<Option.Option<Glb.Hit>> =>
  Effect.sync(() => Option.fromNullable(tree.closestPointToPoint(point)))

// the MEASURE row `panel#CONTROL_SINKS` routes its `measure` sink onto: both raw endpoints snap through the leaf
// before the span is read, so a measurement always spans two points the assembly actually carries and never two
// the pointer happened to land near
const _measure = (tree: MeshBVH, from: Vector3, to: Vector3): Effect.Effect<Option.Option<Glb.Span>> =>
  Effect.map(Effect.all([_nearest(tree, from), _nearest(tree, to)]), ([start, end]) =>
    Option.map(Option.all([start, end]), ([head, tail]) =>
      ({ from: head, to: tail, length: head.point.distanceTo(tail.point) }) satisfies Glb.Span))
```

## [08]-[APPEARANCE_BIND]

- Owner: `Pbr` binds decoded OpenPBR values and addressed texture planes to declared Three.js slots.
- Law: one role table owns channel seating, color transfer, packing, refusal evidence, and which slots exist only on the physical class.
- Law: the widening upgrade is a NARROW copy through the standard prototype — the two idioms three appears to offer both destroy the target.
- Law: alpha association is a blend fact the material owns; `Texture.premultiplyAlpha` reaches the browser-decoded legs alone — where the decode already demanded straight alpha and the flag stays at its false default — and is inert on every ArrayBufferView upload, so no leg multiplies and association lands on the material.

```typescript signature
type MaterialRow = Material
import { Array, Effect, HashMap, Option } from "effect"
import {
  ClampToEdgeWrapping, DoubleSide, FrontSide, LinearSRGBColorSpace, MeshPhysicalMaterial, MeshStandardMaterial,
  RepeatWrapping, type Color, type Texture as Plane,
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
  // `slot` is null for the roles `MeshPhysicalMaterial` declares nothing for; `component` is the LOWEST
  // stored texel width carrying the swizzle three's own shader chunk samples, so the width proof is arithmetic
  // against `TextureVocab.rows.plane[format].width` rather than a per-role exception; `scalar` is the lobe field a false pack
  // slot neutralizes and the companion a debug view reads; `physical` marks the rows whose slot or scalar exists
  // ONLY on the physical class, which is the one column the standard-graft upgrade gate reads
  type RoleSlot = {
    readonly slot: keyof MeshPhysicalMaterial | null
    readonly component: 1 | 2 | 3 | 4 | null
    readonly scalar: keyof MeshPhysicalMaterial | null
    readonly physical: boolean
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
    readonly demands: typeof _demands
    readonly upgrade: typeof _upgraded
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
// The `physical` column is the standard class's own declaration boundary: the coat, fuzz, transmission, thin-film,
// anisotropy, and specular-tint families are physical-only, so a write against a standard target drops silently.
const _ROLE_SLOT: { readonly [K in TextureVocab.Role]: Pbr.RoleSlot } = {
  base_weight: { slot: null, component: null, scalar: null, physical: false },
  base_color: { slot: "map", component: 4, scalar: "color", physical: false },
  base_metalness: { slot: "metalnessMap", component: 3, scalar: "metalness", physical: false },
  base_diffuse_roughness: { slot: null, component: null, scalar: null, physical: false },
  base_specular_tint: { slot: null, component: null, scalar: null, physical: false },
  specular_weight: { slot: "specularIntensityMap", component: 4, scalar: "specularIntensity", physical: true },
  specular_color: { slot: "specularColorMap", component: 3, scalar: "specularColor", physical: true },
  specular_roughness: { slot: "roughnessMap", component: 2, scalar: "roughness", physical: false },
  specular_roughness_anisotropy: { slot: "anisotropyMap", component: 3, scalar: "anisotropy", physical: true },
  // three exposes no rotation MAP — the direction rides anisotropyMap's RG encode; the plane stays a scalar lobe
  specular_roughness_anisotropy_rotation: { slot: null, component: null, scalar: "anisotropyRotation", physical: true },
  specular_ior: { slot: null, component: null, scalar: "ior", physical: true },
  transmission_weight: { slot: "transmissionMap", component: 1, scalar: "transmission", physical: true },
  transmission_roughness: { slot: null, component: null, scalar: null, physical: false },
  subsurface_weight: { slot: null, component: null, scalar: null, physical: false },
  subsurface_radius: { slot: null, component: null, scalar: null, physical: false },
  coat_weight: { slot: "clearcoatMap", component: 1, scalar: "clearcoat", physical: true },
  coat_color: { slot: null, component: null, scalar: null, physical: false },
  coat_roughness: { slot: "clearcoatRoughnessMap", component: 2, scalar: "clearcoatRoughness", physical: true },
  coat_ior: { slot: null, component: null, scalar: null, physical: false },
  fuzz_weight: { slot: null, component: null, scalar: "sheen", physical: true },
  fuzz_color: { slot: "sheenColorMap", component: 3, scalar: "sheenColor", physical: true },
  fuzz_roughness: { slot: "sheenRoughnessMap", component: 4, scalar: "sheenRoughness", physical: true },
  thin_film_weight: { slot: "iridescenceMap", component: 1, scalar: "iridescence", physical: true },
  thin_film_thickness: { slot: "iridescenceThicknessMap", component: 2, scalar: "iridescenceThicknessRange", physical: true },
  thin_film_ior: { slot: null, component: null, scalar: "iridescenceIOR", physical: true },
  emission_color: { slot: "emissiveMap", component: 3, scalar: "emissive", physical: false },
  emission_luminance: { slot: null, component: null, scalar: "emissiveIntensity", physical: false },
  geometry_opacity: { slot: "alphaMap", component: 2, scalar: "opacity", physical: false },
  geometry_normal: { slot: "normalMap", component: 3, scalar: "normalScale", physical: false },
  geometry_coat_normal: { slot: "clearcoatNormalMap", component: 3, scalar: "clearcoatNormalScale", physical: true },
  geometry_tangent: { slot: null, component: null, scalar: null, physical: false },
  geometry_coat_tangent: { slot: null, component: null, scalar: null, physical: false },
  height: { slot: "displacementMap", component: 1, scalar: "displacementScale", physical: false },
  occlusion: { slot: "aoMap", component: 1, scalar: "aoMapIntensity", physical: false },
  curvature: { slot: null, component: null, scalar: null, physical: false },
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

// This set-level stamp reads the set row and the acquisition's sampling cap and nothing else. `flipY` is never
// written HERE because `[6]`'s decode already closed it at the one leg whose class defaults it true, and both
// backends honour it on every upload path, so a second opinion at the set stamp could only fight the decode.
// `premultiplyAlpha` is never written either, but for the opposite reason: it rides `UNPACK_PREMULTIPLY_ALPHA_WEBGL`
// on WebGL and the external-image copy's own flag on WebGPU, so it reaches the three browser-decoded legs and is
// silently inert on every ArrayBufferView leg — and on the legs it does reach, the decode already asked for
// straight alpha, so the false default is the correct write. The KTX2 leg's own stamp off the DFD alpha flag is
// inert for the same reason. Association therefore lands on the material's blend at `_seat`.
const _stamped = (plane: Plane, set: TextureSet, acquired: Glb.Acquired): Plane => {
  // BOUNDARY ADAPTER
  plane.wrapS = set.tiled ? RepeatWrapping : ClampToEdgeWrapping
  plane.wrapT = plane.wrapS
  plane.anisotropy = acquired.anisotropy
  plane.channel = 0
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

// The upgrade GATE. `_lobes` writes the physical-only scalars unconditionally, so ANY resolved override already
// demands the physical class; a set demands it wherever a present role's own `physical` column reads true. Every
// other appearance seats entirely inside the standard declaration and earns no mint, which is what keeps the
// upgrade off the majority of grafts.
const _demands = (bound: Pbr.Bound): boolean =>
  Option.isSome(bound.override) ||
  Option.exists(bound.set, (set) =>
    Array.some(_seatings(set), (seating) => Array.some(seating.present, (role) => _ROLE_SLOT[role].physical)))

// THE one widening path, and it is a NARROW copy. `MeshStandardMaterial.prototype.copy` carries every shared
// property onto the physical target and touches no physical lobe, so the constructor defaults survive and the
// target keeps its own `uuid`, `type`, and `isMeshPhysicalMaterial`. The re-stamp is mandatory, not tidiness:
// that copy assigns a fresh `{ STANDARD: "" }` and PHYSICAL is what gates `#define IOR` and `USE_SPECULAR` in the
// shipped program.
const _upgraded = (source: MeshStandardMaterial): MeshPhysicalMaterial => {
  // BOUNDARY ADAPTER — two traps are foreclosed here and nowhere else. The apparent idiom
  // `new MeshPhysicalMaterial().copy(standardSource)` THROWS: the widening copy writes `undefined` across the
  // scalar lobes and then dereferences the source's absent `clearcoatNormalScale`, leaving a half-corrupted
  // target behind. `setValues(standardSource)` completes but blind-assigns `type` and `uuid` from the source and
  // ALIASES its `defines` object, so the target reports as standard and program routing reads `type`.
  const target = new MeshPhysicalMaterial()
  MeshStandardMaterial.prototype.copy.call(target, source)
  target.defines = { STANDARD: "", PHYSICAL: "" }
  return target
}

// every refusal a seating can carry, read off wire columns alone — no predicate widens as roles or packs grow;
// the fold answers ISSUES, because the carrier that names them is the seat's own census and not one fault per column
const _unseatable = (set: TextureSet, seating: Pbr.Seating, mesh: string): ReadonlyArray<GlbFault.Case> => {
  const refuse = (reason: Extract<GlbFault.Reason, "manifest-skew" | "plane-unbound">, cause: string): GlbFault.Case => ({
    reason,
    mesh,
    cause,
  })
  return [
    ...(TextureVocab.rows.plane[seating.format].web ? [] : [refuse("plane-unbound", "the store format decodes nowhere on the web")]),
    ...(TextureVocab.rows.layer[set.layerLaw].gltf ? [] : [refuse("plane-unbound", "the layer law seats nowhere in gltf")]),
    ...(set.udimTiles.length === 0 ? [] : [refuse("plane-unbound", "udim tiles reach no sampler")]),
    // inverting the pack order swaps R and B, so a consumer binding it to three's slots reads occlusion as
    // metalness — a refusal only the column can declare, and the column is the wire's own
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

// one seated plane, one refusal set: the roster row decodes it, the policy stamps it, and every present slot
// takes the SAME handle so a pack costs one upload and `[5]`'s deduped walk frees it once
const _seated = (
  material: MeshPhysicalMaterial,
  set: TextureSet,
  seating: Pbr.Seating,
  port: GlbViewport.Appearance,
  acquired: Glb.Acquired,
): Effect.Effect<ReadonlyArray<GlbFault.Case>> =>
  Array.match(_unseatable(set, seating, `${set.setKey}/${seating.address[1]}`), {
    // a refused seating never reaches the network: the wire columns already decided it, so no plane is pulled
    onNonEmpty: (refused) => Effect.succeed<ReadonlyArray<GlbFault.Case>>(refused),
    onEmpty: () =>
      Effect.gen(function* () {
        const octets = yield* port.planes(seating.address)
        const plane = yield* _decoded(
          _CONTAINERS[seating.container],
          { key: set.setKey, octets },
          acquired,
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
      }).pipe(Effect.catchAll((refusal: GlbFault) => Effect.succeed<ReadonlyArray<GlbFault.Case>>([refusal.case]))),
  })

// ONE seat: lobes then planes, refusals accumulated and never raised — a partially-bound set renders the
// authored asset with its seatable planes overlaid, which is exactly what `plane-unbound` declares
const _seat = (
  material: MeshPhysicalMaterial,
  bound: Pbr.Bound,
  port: GlbViewport.Appearance,
  acquired: Glb.Acquired,
): Effect.Effect<Option.Option<GlbSeating>> =>
  Effect.map(
    Option.match(bound.set, {
      onNone: () => Effect.succeed<ReadonlyArray<ReadonlyArray<GlbFault.Case>>>([]),
      onSome: (set) =>
        Effect.forEach(_seatings(set), (seating) => _seated(material, set, seating, port, acquired)),
    }),
    (refusals) => {
      // BOUNDARY ADAPTER — the lobe fold runs first so a seated plane always modulates a carried scalar, and
      // `needsUpdate` closes the whole seat once: assigning a map where there was none demands the recompile
      Option.map(bound.override, (row) => _lobes(material, row.openPbr))
      // the set's alpha mode is a BLEND fact, not an upload one: an associated payload arrives already
      // multiplied, so the consumer's whole obligation is the equation, and asking the upload to multiply again
      // would double it even on the DOM-source legs where the texture flag is not inert
      Option.map(bound.set, (set) => void (material.premultipliedAlpha = set.alphaMode === "associated"))
      material.needsUpdate = true
      // ONE carrier per seat: every offending plane across every seating rides one refusal, so a consumer reads the
      // set's whole damage in one fact instead of folding an array the producer already knew the shape of
      return Option.map(
        Array.match(Array.flatten(refusals), { onEmpty: Option.none, onNonEmpty: Option.some }),
        (issues) => new GlbSeating({ issues }),
      )
    },
  )

// This census IS the index: the summary roster fixes the appearance key space and every other roster joins onto
// it. Both sides of every join carry the appearance key in its DECODED spelling, because `Digest.Key<"content">`
// brands the seed-zero content hash as thirty-two LOWERCASE hex and the two codecs reaching it both land there:
// `summary.appearanceKey` crosses on the bytes codec, whose hex encode is lowercase, and `set.appearanceKey` on
// the wire codec, whose UPPERCASE egress form is lowered at decode and re-raised only at encode. They therefore
// compare like for like and no site re-cases. A key meets a PATH exactly once, at the port's own egress-name
// construction: `planes` takes the key and derives the address, where `_AssetIdentity`'s lowercase digest
// admission has nothing left to lower and refuses an uppercased segment outright. Re-casing at a compare, or
// carrying the wire's uppercase form into a path segment, are both the deleted direction.
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
        : Effect.fail(new GlbFault({ case: { reason: "manifest-skew", mesh: `${appearance}`, cause: "the seat census holds no such override" } })))
    yield* Effect.forEach(document.sets, (set) =>
      HashMap.has(seats, set.appearanceKey)
        ? Effect.void
        : Effect.fail(new GlbFault({ case: { reason: "manifest-skew", mesh: `${set.appearanceKey}`, cause: "the seat census holds no such set" } })))
    yield* Effect.forEach(document.worn, ([mesh, appearance]) =>
      HashMap.has(seats, appearance)
        ? Effect.void
        : Effect.fail(new GlbFault({ case: { reason: "manifest-skew", mesh: `${mesh}`, cause: "the seat census holds no such appearance" } })))
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
  // the georeferenced repeat of ONE resident graft: `key` is the verified content key its geometry arrived under
  // — the ledger's own address, so an instance can be joined back to its residency tile, its appearance, and its
  // tree — while `placement` is `[07]`'s transform vocabulary rather than a second orientation-and-scale pair
  type Anchor = {
    readonly key: Digest.Key<"content">
    readonly anchor: Position
    readonly placement: Matrix4
    readonly tint: Paint
  }
  // the geo counterpart of `GlbViewport.Addressed`, and total for the same reason: a resident graft the map has
  // not placed yet is lawfully absent, and asking for a placement is never a request to compute one
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

// deck reads instance colour as eight-bit DISPLAY bytes while every tint on this page is a working-space linear
// triple, so the crossing runs through `[8]`'s own reader in both halves: `_tint` seats the triple under
// `LinearSRGBColorSpace` and `getRGB` reads it out under `SRGBColorSpace`, which is the transfer three's
// `ColorManagement` owns. A raw triple scaled to bytes skips both and renders the working value as display.
const _INK = { full: 255 } as const satisfies { full: number }

// the untinted floor: deck multiplies the instance colour into the sampled surface, so opaque white is the
// identity a graft with no resolved override deserves
const _NEUTRAL: Paint = [_INK.full, _INK.full, _INK.full, _INK.full]

const _paint = (linear: readonly [number, number, number], scratch: Ink, display: Ink): Paint => {
  // BOUNDARY ADAPTER — the scratch pair is caller-held and the byte tuple detaches at the return
  _tint(scratch, linear).getRGB(display, SRGBColorSpace)
  return [display.r * _INK.full, display.g * _INK.full, display.b * _INK.full, _INK.full]
}

// The anchor roster is a FOLD over the residency ledger, never a second census: a repeat exists only where the
// graft it repeats is resident, so an eviction drops its instances in the same pass the release visitor frees the
// buffers. The tint is the appearance `Pbr` already resolved rather than a second colour authority — a mesh no
// appearance wears keeps the neutral, and a key the map has not placed drops out as lawful absence.
const _anchors = (
  ledger: Glb.Ledger,
  index: Pbr.Index,
  placed: Instanced.Placed,
): ReadonlyArray<Instanced.Anchor> => {
  // BOUNDARY ADAPTER — one scratch pair serves the whole sweep, because `_paint` copies into the tuple it returns
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

// the deck-side policy beside the rows it feeds: `sizeScale` multiplies the placement matrix, so one is the
// identity this page holds, and the animation SPEED is the only time column here — the frame the animator
// advances against is the overlay's, which reads `geo#FRAME_CLOCK` like every other animated surface
const _DECK = { sizeScale: 1, speed: 1 } as const satisfies { sizeScale: number; speed: number }

const _mesh = (id: string, mesh: SimpleMeshLayer["props"]["mesh"], anchors: ReadonlyArray<Instanced.Anchor>) =>
  new SimpleMeshLayer<Instanced.Anchor>({
    id,
    data: anchors,
    mesh,
    pickable: true,
    getPosition: (row) => row.anchor,
    // deck's matrix XOR: supplying this retires the orientation/scale/translation triple whole, which is exactly
    // the second transform vocabulary this row refuses to carry
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

// --- [EXPORTS] --------------------------------------------------------------------------

export { Glb, GlbFault, GlbSeating, GlbViewport, Instanced, Pbr }
```

## [11]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
