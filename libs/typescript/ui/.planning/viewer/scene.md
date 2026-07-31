# [UI_SCENE]

Glb owns content-keyed scene residency behind a `three` or `<model-viewer>` backend discriminant. One `GlbViewport` port supplies verified bytes, ledger facts, dome policy, appearance documents, and the derived-tree cache band; one renderer acquisition shares its `GPUDevice`, seats the one prefilter, and answers the one sampling cap; one codec record seats the one transcoder; one graft ledger owns subtrees, mixers, acceleration trees — built off-thread, parked on the geometry they index, and published as one stamped `MeshBVH` roster the pick plane descends — GPU residency, and typed evidence. One environment dome rides the same port — container-sniffed HDR, Radiance, and deep-KTX2 octets folded through ONE commit that decodes and prefilters on a key change, re-reads policy on a repeat, and re-derives its target on backend loss, with nine-band irradiance carried verbatim behind one wire-frame read seam. OpenPBR binding carries the C# algebra verbatim across BOTH halves — the scalar lobes and the baked plane set that hangs behind the same appearance key — every plane seated into a declared material slot by one role table. Georeferenced instances remain deck layer values. Scoped GPU resources, a parked hidden loop, one lifecycle machine, and one `GlbFault` family close the plane. Module: `ui/viewer/src/scene.ts`.

## [01]-[INDEX]

- [02]-[VIEWPORT_PORT]: the `GlbViewport` port — ledger read, verified arrivals, dome policy, appearance documents, tree snapshots; `GlbViewport`.
- [03]-[FAULT_FAMILY]: `GlbFault` — the closed reason vocabulary and its policy table; `GlbFault`.
- [04]-[BACKEND_SELECT]: the backend literal, the acquisition cell, sampling cap, output policy, light rig, lifecycle statechart; `Glb`.
- [05]-[RESIDENCY_GRAFT]: the asset roster, codec rows, prefetch hints, and the graft/accelerate/dress/splat/release fold; `Glb`.
- [06]-[ENVIRONMENT_FOLD]: the closed container roster, the dome fold — one commit, keyed handles, the SH9 read seam; `Glb`.
- [07]-[DRAW_COLLAPSE]: merge/instanced/batched rows, the per-slot visibility and tint writes, the compute cull, the bracketed assembly bake; `Glb`.
- [08]-[APPEARANCE_BIND]: the OpenPBR lobe assignment, the role-slot plane seat, color contract, census resolve; `Pbr`.
- [09]-[INSTANCED_ROWS]: `@deck.gl/mesh-layers` — georeferenced element instances as deck layer values; `Instanced`.
- [10]-[EMBED_ROW]: the `<model-viewer>` backend row — decoder statics, object-URL bracket, animation; `Glb`.

## [02]-[VIEWPORT_PORT]

[VIEWPORT_PORT]:
- Owner: `GlbViewport` — the runtime-capability port this folder declares and NEVER implements: `ledger` is the residency read (`core/interchange/frame`'s `Residency.Ledger` published `Subscribable`, the same cell `Depot` owns), and `arrivals` is the verified-octet ingress — each element pairs a `ContentKey` with whole-buffer GLB bytes the hauling side already reassembled and re-proved through the one `Parity` delegate, so bytes reaching this module are proof-carrying by construction. `environments` is the dome ingress on the same proof rail — an `Arrival` widened with the read policy its decoded set manifest carries (`intensity`, `rotation` in radians about the producer frame's `+Z`, and `sh9`, the producer's irradiance projection as 27 band-major RGB-interleaved values). `appearances` is the appearance ingress: one document carrying the summary roster, the key-paired material overrides (each nesting its whole OpenPBR vector), the baked plane sets, the mesh-to-appearance pairing composition resolved off the decoded element graph, and `planes`, the keyed plane-octet resolver. App composition satisfies the Tag by forwarding `Depot.haul` arrival pairs, the `Depot` ledger cell, `Depot.dome` answers, and the decoded appearance landings.
- Packages: `effect` (`Context.Tag`, `Effect`, `Stream`, `Subscribable`); `@rasm/ts/core` (`AppearanceSummary`, `ContentKey`, `Material`, `PbrGroups`, `Residency`, `TextureSet`).
- Law: the port is the ONLY byte ingress — a fetch, a worker message, or an `ArrayBuffer` reaching this module by any other path bypasses content-key verification and is the named defect. `planes` is a RESOLVER on that same rail, not an exemption: it answers a `[setKey, file]` pair with verified octets, so the plane leg reads addressed bytes exactly as the arrival stream reads hauled ones.
- Law: arrival octets are whole-buffer and the TYPE carries it — `Uint8Array<ArrayBuffer>` fixes a non-shared backing store, so `octets.buffer` satisfies the `ArrayBuffer` parameter every decode entry declares (`parseAsync`, `createDataTexture`, `KTX2Loader.parse`) at all three sites with no narrowing guard, while the bare `Uint8Array` widens its buffer to `ArrayBufferLike` and refuses each of them; the hauling side reassembles at `byteOffset` zero, and a sliced view smuggling a neighbor's bytes is the provider's defect, never the graft's guard.
- Law: `planes` is keyed and idempotent, never a stream — a set's channel list is known whole at commit, every leaf address is `[setKey, file]`, and that pair is what `Glb.assetPath` derives `assets/<digest>/<file>` from, so the ui end of the served-asset join has ONE spelling for decoder assets and set planes alike. Repeating a pull on one pair answers the same octets; a per-plane subscription re-keys what the document already addresses.
- Law: the mesh-to-appearance pairing is the ONE coordinate no appearance wire carries — the C# seam content-keys a `Node.Appearance` on the ELEMENT side and `MaterialWire` carries its `family.name` identity alone, so the composing root reads each override's appearance key off the decoded element graph and pairs it here; a viewport re-deriving the key from the lobe scalars re-mints the seam's own `XxHash128` preimage and is the named cross-language drift defect.
- Law: the wire shapes reach this module as the settled core vocabulary — `Residency.Ledger` rows, `ContentKey`, `AppearanceSummary`, `Material`, `PbrGroups`, and `TextureSet` compose directly; a parallel manifest shape, row twin, or local key notion is unspellable because the core owner is the only declaration.
- Law: `snapshots` is the ONE member carrying derived data rather than payload, and its posture states why that is safe — a bounds-tree snapshot is addressed by the SAME `ContentKey` the octets arrived under, so it can only ever describe the parse it was built from, a cardinality disagreement falls back to a build, and a cold or refused read costs a rebuild instead of correctness. It never becomes a byte ingress: nothing the scene renders comes out of it.
- Law: `addressed` answers the served coordinate of a PLANNED payload and nothing else — the hauling side owns that coordinate, so `[5]`'s hint family warms exactly what the transport already intends to fetch, and a viewer deriving a payload URL out of a `ContentKey` would forge an address the served tree never wrote (`Glb.assetPath` derives the decoder and plane addresses because their `[digest, file]` pair IS the wire's own, and no arrival carries one). Absence is lawful and total — a key the plan has not reached yet answers `Option.none()` — because a hint is a warm, never a fetch: a miss costs a cold request, exactly as a snapshot miss costs a rebuild.
- Growth: eviction acknowledgement and priority lanes land as members HERE when earned — consumers already hold the Tag, so growth is a member row, never a second port; `environments`, `appearances`, `snapshots`, and `addressed` are that law realized four times.
- Boundary: haul scheduling, cache warmth, worker verify, and byte budgets are `runtime/browser/fetch`'s; the residency protocol and its fold law are `core/interchange/frame`'s. WHICH set manifest named a dome — the C#-baked set document or the python-assembled one — is composition's concern behind the decoded core landings; the port carries bytes, read policy, and appearance documents alone.

```typescript signature
import type { AppearanceSummary, ContentKey, Material, PbrGroups, Residency, TextureSet } from "@rasm/ts/core"
import { Context, type Effect, type Stream, type Subscribable } from "effect"

declare namespace GlbViewport {
  // generic arguments carry the whole-buffer contract: a shared backing store reaches no `three` decode entry
  type Arrival = { readonly key: ContentKey; readonly octets: Uint8Array<ArrayBuffer> }
  type Environment = Arrival & {
    readonly intensity: number
    readonly rotation: number
    readonly sh9: ReadonlyArray<number>
  }
  // ONE appearance document: the summary roster is the appearance-key preimage census AND the flat pre-bind preview,
  // `sets` join it on `TextureSet.appearanceKey`, `worn` carries the element-side pairing no appearance wire holds,
  // and `planes` resolves one addressed leaf — `[setKey, file]`, the pair `Glb.assetPath` already derives from.
  // `materials` carries each override BESIDE the appearance key it was fetched under, because the wire nests the
  // whole OpenPBR vector inline (`Material.openPbr`) and carries NO key column — the payload rides BEHIND the key
  // at transport, so the fetch coordinate is the pairing fact and only the carrier can state it
  type Appearance = {
    readonly summaries: ReadonlyArray<AppearanceSummary>
    readonly materials: ReadonlyArray<readonly [appearance: ContentKey, override: Material]>
    readonly sets: ReadonlyArray<TextureSet>
    readonly worn: ReadonlyArray<readonly [mesh: ContentKey, appearance: ContentKey]>
    readonly planes: (address: readonly [ContentKey, string]) => Effect.Effect<Uint8Array<ArrayBuffer>, GlbFault>
  }
  // the acceleration band: DERIVED data under the same content key its geometry arrived on, so a snapshot can
  // only ever describe the parse it was built from and a miss costs a rebuild, never correctness. Entries are
  // ordered by the residency walk's own traversal, which a content-addressed parse fixes.
  type Snapshots = {
    readonly read: (key: ContentKey) => Effect.Effect<Option.Option<ReadonlyArray<SerializedBVH>>, GlbFault>
    readonly write: (key: ContentKey, snapshot: ReadonlyArray<SerializedBVH>) => Effect.Effect<void, GlbFault>
  }
  // the served coordinate of a planned payload — the growth row the prefetch hints earned. It is a total read,
  // not a rail: a key the hauling side has not planned yet is lawfully absent, and a hint is never a fetch.
  type Addressed = (key: ContentKey) => Option.Option<string>
}

class GlbViewport extends Context.Tag("ui/viewer/GlbViewport")<GlbViewport, {
  readonly ledger: Subscribable.Subscribable<Residency.Ledger>
  readonly arrivals: Stream.Stream<GlbViewport.Arrival, GlbFault>
  readonly environments: Stream.Stream<GlbViewport.Environment, GlbFault>
  readonly appearances: Stream.Stream<GlbViewport.Appearance, GlbFault>
  readonly snapshots: GlbViewport.Snapshots
  readonly addressed: GlbViewport.Addressed
}>() {}
```

## [03]-[FAULT_FAMILY]

[FAULT_FAMILY]:
- Owner: `GlbFault` — the page's one reason-discriminated `Schema.TaggedError`, its rows closed through the core `FaultClass.family` seam with a closed `reason` vocabulary: `manifest-skew` (an arrival references a mesh the ledger never named, or a wire column the bind reads declares an order this consumer cannot honour), `key-mismatch` (verification refused the octets at the port boundary), `decode-refused` (loader or codec rejected the payload — the GLB parse and the dome container/decode legs share it), `codec-absent` (the asset demands a codec the viewport did not wire — the meshopt gate's refusal spelling), `plane-unbound` (a set plane the viewer cannot SEAT: a role `MeshPhysicalMaterial` declares no slot for, a sampled component the plane's stored width does not carry, or a container the browser cannot decode), `backend-lost` (GPU device loss). Each row carries its core `FaultClass` kind beside the two scene-local axes no core column holds — `evict`, whether the graft ledger drops the mesh, and `arm`, which plane the reason indicts — and the `class`/`evict`/`arm` getters project the row, so severity, retryability, blame, and quarantine derive from the core row table while the two viewer axes stay columns — a local `rank` or `retry` column beside `class` is the per-folder taxonomy fork the branch fault ruling deletes. `GlbFault.roster` republishes `_family.reasons` — the ordered non-empty tuple the refusal instrument's word census pre-registers — and the family seam closes membership by construction, so no local guard pair exists. Code-keyed free-string fault construction is the named discard — reasons are a closed vocabulary.
- Packages: `effect` (`Schema`); `@rasm/ts/core` (`FaultClass`).
- Law: recovery policy reads the core lattice off `class` — `codec-absent` and `backend-lost` classify `unavailable` (system, retryable: the decoder row landing and the device re-init each heal them), `key-mismatch` classifies `malformed` while `decode-refused` and `plane-unbound` classify `invalid` (caller-blamed, quarantined, never retried), `manifest-skew` classifies `conflicted` (a refreshed ledger resolves the disagreement) — while `evict` and `arm` stay the genuine viewer axes no core column carries: whether the graft ledger drops the mesh's residency, and whether the reason indicts an ASSET the raising lane contains or the BACKEND `[4]`'s lifecycle statechart owns. `arm` is what keeps an asset refusal from moving the lifecycle when any lane may send the machine's `Refuse` request: the guard reads this column, never a reason literal, so a `codec-absent` sharing `backend-lost`'s `unavailable` class still leaves the phase untouched.
- Law: `plane-unbound` is EVIDENCE, never a repaint — a refused plane leaves the material's GLB-embedded slot exactly as the graft parsed it, so a partially-bound set renders the authored asset with the seatable planes overlaid and the rest untouched; `evict: false` is that law's column, because dropping the mesh over one unseatable role discards a subtree the wire proved. Silent default maps, black textures, and dropped elements are the shapes this reason replaces.
- Law: `backend-lost` is the re-init row and the acquisition's own device watcher is its ONE raiser — `[4]`'s `arm` act forks `GPUDevice.lost` under the keyed watch fiber and re-arms it per acquisition, `reason: "unknown"` (a driver reset or an evicted tab) minting the fault while `reason: "destroyed"` is the finalizer's own `renderer.dispose()` and mints nothing; the probe's degrade-to-WebGL path never touches this row, so a reason with no raiser cannot survive here. Recovery re-runs backend selection under the same `Scope` through the backplane swap, and residency state survives because the graft ledger is renderer-independent.
- Law: a fault no consumer arm can act on escalates — a torn invariant inside the graft fold dies through `Effect.die`, keeping the channel total over actionable faults.
- Law: the reason table is scene-local vocabulary — the transport-altitude `Hops` table is `core/interchange/codec`'s and never restates here; a scene reason names a residency/render condition, never a wire condition.
- Growth: a new failure condition (a compute-pass refusal, an embed mount loss) is one family row with its kind, `evict`, and `arm` columns — every policy consumer inherits it through the core lattice and the getters, and an `arm: "backend"` row joins the lifecycle's guarded signal with no transition-table edit.

```typescript signature
import { Schema } from "effect"
import { FaultClass } from "@rasm/ts/core"

// One row per reason: the core kind the class getter projects, plus the TWO scene-local axes. Severity, retry,
// blame, and quarantine are the core FaultClass row table's — a rank or retry literal here would fork them.
// `arm` is the lifecycle guard's own column: `codec-absent` and `backend-lost` share the `unavailable` class, so a
// reason literal in the guard would move the phase on an unserved decoder.
const _family = FaultClass.family(
  ["manifest-skew", "key-mismatch", "decode-refused", "codec-absent", "plane-unbound", "backend-lost"] as const,
  {
    "manifest-skew": { class: "conflicted", evict: true, arm: "asset" },
    "key-mismatch": { class: "malformed", evict: true, arm: "asset" },
    "decode-refused": { class: "invalid", evict: true, arm: "asset" },
    "codec-absent": { class: "unavailable", evict: false, arm: "asset" },
    // the mesh keeps its GLB-embedded slot: an unseatable plane is evidence over a drawable subtree, never an eviction
    "plane-unbound": { class: "invalid", evict: false, arm: "asset" },
    "backend-lost": { class: "unavailable", evict: false, arm: "backend" },
  },
)

declare namespace GlbFault {
  type Reason = (typeof _family.reasons)[number]
  // the indicted plane: an asset refusal is contained by the lane that raised it, a backend refusal is a lifecycle event
  type Arm = (typeof _family.rows)[Reason]["arm"]
}

class GlbFault extends Schema.TaggedError<GlbFault>()("GlbFault", {
  reason: _family.schema,
  mesh: Schema.String,
  detail: Schema.String,
}) {
  static readonly roster: typeof _family.reasons = _family.reasons // the census anchor: the family's own ordered non-empty tuple
  get class(): FaultClass.Kind {
    return _family.classOf(this.reason)
  }
  get evict(): boolean {
    return _family.rows[this.reason].evict
  }
  get arm(): GlbFault.Arm {
    return _family.rows[this.reason].arm
  }
  override get message(): string {
    return `<glb:${this.reason}> ${this.mesh}: ${this.detail}`
  }
}
```

## [04]-[BACKEND_SELECT]

[BACKEND_SELECT]:
- Owner: the closed backend vocabulary (`three` | `model-viewer`) spread from one `as const` tuple into `Schema.Literal`, and the `three` arm's renderer acquisition under `Effect.acquireRelease`: probe `navigator.gpu`, acquire the ONE `GPUDevice` (`requestAdapter` → `requestDevice`), construct `WebGPURenderer({ canvas, device })` and await `init()` (feature-gating through `renderer.hasFeature` post-init), else `WebGLRenderer` — a refused acquisition on a present-but-broken adapter degrades to the same WebGL floor, so `backend-lost` stays reserved for a lost LIVE device and the probe never faults the channel; the acquisition returns ONE `Glb.Acquired` record — `device` as an `Option`, the published seam the compute lane, the probe hash kernel, and any `tgpu.initFromDevice` adopter share, one device, one memory space, zero readback round-trips; `prefilter` is the ONE `Glb.Prefilter` — the backend-matched PMREM class behind one structural seam — that `[6]`'s dome fold consumes and `Glb.Loop.rebind` re-derives against, released before its renderer; `renderer` is what `[5]` wires into `KTX2Loader.detectSupport` and every eager texture upload, so the codec transcode target, the residency walk, and the dome's own source upload read the same live backend; the output policy row (`outputColorSpace`, tone map selected from the `_TONE` vocabulary, `toneMappingExposure`) stamps at construction; `anisotropy` is the ONE sampling cap — the `_SAMPLING` policy ceiling met against the backend's own maximum, which the two backends spell differently (`capabilities.getMaxAnisotropy()` on the WebGL class, `getMaxAnisotropy()` on the unified one) and the acquisition therefore answers ONCE for every texture stamp downstream; release disposes — three has no GPU garbage collection, so the finalizer IS memory correctness. `Glb.Backplane` is that acquisition held as a `ScopedRef` rather than a value, because a re-init must hand every downstream lane a LIVE record: `ScopedRef.set` acquires the successor and releases the displaced renderer only after the swap commits, so no lane observes a torn backend and no consumer holds a disposed handle. `Glb.lifecycle` is the statechart over that cell — the phase/signal row table, its `GlbFault`-guarded refusal arm, its keyed re-acquisition and device-loss watch fibers, and the acts that re-fix the transcode target, rebind the dome, and re-arm the draw loop — booted as a serializable `Machine` whose actor is the `Subscribable` the view binds.
- Packages: `three` (`WebGLRenderer`, `PMREMGenerator`, `SRGBColorSpace`, `ACESFilmicToneMapping`/`AgXToneMapping`/`NeutralToneMapping`, the light classes incl. `RectAreaLight`/`LightProbe`); `three/webgpu` (`WebGPURenderer`, `PMREMGenerator` — the unified-renderer prefilter class); `effect` (`Duration`, `Effect`, `Option`, `Schedule`, `Schema`, `ScopedRef`, `Scope`); `@effect/experimental` (`Machine` — `makeSerializable`, `serializable.make`/`add`, `retry`, `boot`, `snapshot`, `restore`); `@webgpu/types` (ambient, a viewer-tsconfig `types` entry, never an import).
- Law: one usage contract over both renderer backends — scene, camera, and loop code are backend-agnostic after construction; the WebGPU upgrade is a construction-site swap, never a scene rewrite; the `scope:viewer` tier itself is `lazy(() => import(…))` behind `Suspense` so the non-spatial majority never downloads three.
- Law: tone mapping is a vocabulary row, never a constant — `_TONE` carries `aces`/`agx`/`neutral`, the output policy names one, and a per-scene override is one policy value; hardcoding a tone-map enum at a construction site is the named defect.
- Law: anisotropic sampling is VIEWER POLICY, never a wire column — no set document, dome manifest, or appearance block declares it, so `_SAMPLING` states the ceiling beside `_OUTPUT`/`_ENV` and the acquisition meets it against the live backend's maximum; every texture the dome fold and `[8]`'s set-bind stamp reads `Glb.Acquired.anisotropy`, so one number governs the whole viewport. Per-texture literals, bind-time re-probes, and backend `instanceof` ladders at a stamp site each re-derive a VALUE the acquisition already answered, while an uncapped value silently clamps per driver. A class refinement is the one thing that is not that re-derivation: `PointsNodeMaterial` and `computeAsync` exist on the unified renderer alone, and no column on the record carries that into the type plane, so `[5]`'s splat arm and `[7]`'s cull lift `renderer instanceof WebGPURenderer` to an `Option` at their own gate while `Acquired.device` stays the DEVICE seam the `tgpu` altitude adopts — two distinct facts, never two answers to one.
- Law: lighting is a rig of rows — the analytic light vocabulary (`ambient`, `hemisphere`, `directional`, `rect`, `probe`) is one `as const` table materialized by one fold; the image-based half is `[6]`'s dome fold — `fromScene(RoomEnvironment())` as the floor, the content-keyed equirect dome on arrival; `rect` is the analytic area source for interior shots, `probe` the analytic low-frequency seat the rig holds before any dome; a hand-placed light outside the rig table, or per-frame IBL work, is the named defect.
- Law: the compute lane is WebGPU-gated and altitude-split — scene-resident kernels (per-instance culling, skinning) are `three/tsl` node graphs over `StorageBufferNode`/`instancedArray` dispatched through `computeAsync`, and `[7]`'s `Glb.cull` is that lane realized: it drives the `setVisibleAt` rows `Glb.visible` writes; compute WITHOUT a scene consumer (probe hashing, mark lasso folds) adopts the published device through `tgpu.initFromDevice({ device })` with `root.unwrap(buffer)` at the seam — two altitudes of one concern split by scene residency, never two engines on one kernel; the WebGL arm renders the same scene without the pass, and a compute result feeding appearance is `[8]`'s debug-view boundary, never OpenPBR algebra.
- Law: the backend lifecycle is a statechart, not scattered arms — `Glb.lifecycle` is one `Machine.makeSerializable` over the `_PHASES` × `_SIGNALS` row table (`booting` → `ready` | `degraded` (the WebGL floor) → `lost` → `reviving` and back), its actor state binding through `system/atom#LIVE_BRIDGE`'s `Atom.subscribable` row because the booted `Machine.Actor` IS a `Subscribable` of its phase, and `Machine.snapshot`/`Machine.restore` carrying that phase across a remount. The `GlbFault` policy table supplies the guard: `Refuse` fires the `refused` row only where the reason's `arm` column reads `backend`, so any lane may send it and an asset refusal moves nothing. Every phase carries its ACT — `arm`, `park`, `chase`, `hold` — and one act table owns the whole consequence, so a new phase or signal is a row and never a branch in a handler body.
- Law: re-init is one path and boot takes it too — the `chase` act is `ScopedRef.set(backplane, _renderer(canvas))` under `forkOne`, so at most one re-acquisition is ever in flight and a stacked loss cannot stack acquisitions; the swap disposes the displaced renderer only after the successor lands. The `arm` act then re-fixes every renderer-bound handle in one place: `detectSupport` re-reads the fresh backend on the ONE transcoder `[5]` publishes (a second instance is the ruling's named defect, so re-configuring the held one is the only spelling), `Glb.Loop.rebind` re-derives the dome target from the decoded source it still holds, and `forkReplace` re-arms the draw loop so the incumbent fiber's scope finalizer parks the dead renderer's callback. Domes are NOT renderer-independent — a prefiltered target dies with the renderer that backed it — so a re-init leaving the dome un-rebound serves the scene a disposed environment texture, and a re-init leaving the transcoder un-probed transcodes to a format the fresh backend cannot sample.
- Law: the loop is a SCOPED subscription over the activity stance, never a bare arm — `Glb.loop` drains the `<Activity>` mode cell's own boolean feed (`system/act#DOCUMENT_RAIL` consumed as settled, bound through `system/atom#LIVE_BRIDGE` so it emits the live stance on subscribe and the loop needs no second arm), `setAnimationLoop(null)` on hide, the callback on visible, and the scope's own finalizer parks it — because `renderer.dispose()` frees GPU handles and leaves the registered frame callback running, so an unbracketed arm burns a rAF against a disposed renderer for the document's life. A loop burning under a hidden viewport is the named defect and so is a park that only the hide path reaches. The lifecycle is the ONE caller: it holds the arm inside `Effect.scoped(… , Effect.never)` under the keyed draw fiber, so an interrupt IS the park and both the hidden path and the backend-lost path reach the same finalizer.
- Boundary: camera drive is `geo`'s; receipt readback and `compileAsync` settle discipline are `probe`'s; the `<canvas>` element and the `Scene` root both arrive as parameters from the app shell, and the graft republishes the root on `Glb.Loop` so a scenegraph consumer reaches one parseable node without a second scene surface.

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
  // one acquisition record serves every downstream lane — backend, device seam, prefilter, and the settled sampling
  // cap all read off this shape, so no lane re-probes `navigator.gpu`, no second prefilter is ever constructed, and
  // no texture stamp re-asks a maximum the two backend classes spell under two different member paths
  type Acquired = {
    readonly renderer: WebGLRenderer | WebGPURenderer
    readonly device: Option.Option<GPUDevice>
    readonly prefilter: Glb.Prefilter
    readonly anisotropy: number
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
    readonly backplane: Glb.Backplane
    readonly codecs: Glb.Codecs
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
      } satisfies Glb.Acquired
    }),
    ({ prefilter, renderer }) => Effect.sync(() => {
      // BOUNDARY ADAPTER — the prefilter releases before the renderer that backs it
      prefilter.dispose()
      renderer.dispose()
    }),
  )

// the swap-in-place cell: `set` acquires the successor and releases the displaced renderer only after the swap
// commits, so a lane reading between the two never observes a torn backend and no consumer holds a dead handle
const _backplane = (canvas: HTMLCanvasElement): Effect.Effect<Glb.Backplane, never, Scope.Scope> =>
  ScopedRef.fromAcquire(_renderer(canvas))

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
      const acquired = yield* ScopedRef.get(plane.backplane)
      // the ONE transcoder re-reads the fresh backend: a second instance is the ruling's named defect, so
      // re-configuring the held one is the only spelling a re-init has for the transcode target
      yield* Effect.sync(() => void plane.codecs.ktx2.detectSupport(acquired.renderer))
      // the prefiltered target died with its renderer; the decoded source did not, so the dome re-derives
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
        yield* ScopedRef.set(plane.backplane, _renderer(plane.canvas))
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
        _family.rows[request.reason].arm === "backend"
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

[RESIDENCY_GRAFT]:
- Owner: `Glb.AssetRoster` — the encoded, slug-unique `{ slug, digest, file }` identity roster for every self-hosted decoder and wasm asset; `Glb.asset` resolves one row with `codec-absent` on absence, `Glb.assetPath` derives the immutable `assets/<digest>/<file>` file address, and `Glb.assetDir` the sibling `assets/<digest>/` directory form — one digest carrying every leaf of a multi-file asset, the trailing slash part of the spelling. `Glb.graft` owns the residency fold over the port: one `Scene` roots the graph, ONE `Ref`-held ledger (`HashMap<ContentKey, Glb.Graft>`) is the single truth for grafted subtrees AND their mixers, and the port's arrival stream enters one graft lane — the lane parses verified octets through `Glb.Codecs.gltf`, the ONE codec-injected `GLTFLoader` (`parseAsync` over the whole buffer), mints the animation half (a per-subtree `AnimationMixer` whose every `gltf.animations` clip binds through `clipAction(...).setLoop(LoopRepeat, Infinity).play()` — the loader result's animation array is retained, never discarded), uploads every decoded texture through the one residency walk, grafts `gltf.scene` under the root, and commits the ledger atomically. One bounded fact queue receives `Arrived` only after that commit, `Environment` from `[6]`'s dome commit, and `Refused` from the contained failure legs; `Glb.hook(loop)` adopts the queue stream for probe and telemetry taps, so one source feeds every consumer without parallel subscriptions or speculative arrival facts. Its eviction arm diffs the port ledger's `evicted` rows against held grafts, removes the subtree, tears the mixer down (`stopAllAction` then `uncacheRoot`) and walks it through the SAME residency walk under the release visitor before the ledger drops the key — both arms mutate ONLY the `Ref`, so the fold is race-free by construction. `Glb.Loop.advance(delta)` derives live mixers from the same ledger inside the marked frame-loop kernel — no second residency roster exists — while `Glb.Loop.dome` republishes `[6]`'s slot read so the irradiance seam reaches consumers without a second subscription, `Glb.Loop.trees` publishes the stamped `MeshBVH` roster `mark`'s pick, marquee, and snap descents read, and `Glb.Loop.rebind` hands the backend statechart's re-init arm the one entry that re-derives the dome against a fresh acquisition. `_CODECS` is the served-decoder row table both this construction and `[10]`'s pin read, and `Glb.warm` is the prefetch hint family it feeds together with the ledger's own `pending` census.
- Packages: `three` (`Scene`, `Mesh`, `Points`, `Material`, `MeshPhysicalMaterial`, `BufferGeometry`, `Texture`, `AnimationMixer`, `LoadingManager`, `LoopRepeat`); `three/webgpu` (`PointsNodeMaterial`, `WebGPURenderer`); `three/tsl` (`vertexColor`); `three/addons` (`GLTFLoader` + its `GLTFParser`/`GLTFLoaderPlugin` extension seam, `DRACOLoader`, `KTX2Loader`, the `MeshoptDecoder` module shape); `three-mesh-bvh` (`MeshBVH` with its `serialize`/`deserialize` statics, the `computeBoundsTree`/`disposeBoundsTree`/`acceleratedRaycast` prototype extensions and their `declare module` merge, `BVHOptions`, `SAH`, `SerializedBVH`) and `three-mesh-bvh/worker` (`ParallelMeshBVHWorker`); `react-dom` (`preload`, `preinit`, `preinitModule`, `PreloadOptions`); `effect` (`Effect`, `HashMap`, `Option`, `Queue`, `Record`, `Ref`, `Schema`, `Stream`, `Scope`, `ScopedRef`, `SubscriptionRef`); `@rasm/ts/core` (`ContentKey`, `Residency`).
- Law: codec injection is capability wiring — the loaders construct over one `LoadingManager` whose `onProgress`/`onError` fold per-graft dependency progress into the residency telemetry tap; `setDRACOLoader`/`setKTX2Loader` attach at loader construction with `setDecoderPath`/`setTranscoderPath` handed the `Glb.assetDir` DIRECTORY form — each loader joins its own leaf names (`basis_transcoder.js` beside its wasm) onto the handed directory, so a multi-leaf decoder rides ONE digest exactly as the iac `_addressedAll` publish spells it — and `detectSupport(renderer)` reads the acquisition's live backend once, fixing the Basis transcode target before the first parse. Perspective wasm reads the same roster through its own slug, and no consumer accepts a free-form asset path.
- Law: which address form a decoder takes is a COLUMN, never a per-site choice — `_CODECS` carries `address` beside `warm` and `_ADDRESS` is the one reader over `Glb.assetDir`/`Glb.assetPath`, so this construction, `[10]`'s process-global pin, and the hint family all resolve one row; the same three facts spelled at three sites made a fourth decoder a three-edit change, and a site re-deciding dir-versus-file is how a directory reaches a file-form consumer as `assets/<digest>/` with nothing after the slash.
- Law: ONE transcoder instance serves the viewport, which is why construction publishes `Glb.Codecs` rather than a bare loader — `KTX2Loader` owns a `WorkerPool` and its `parse` refuses outright until `detectSupport` fills the worker config, so the instance the GLB pipeline receives through `setKTX2Loader` is the same instance `[6]`'s deep-store container row decodes a `.ktx2` dome through; a second instance either doubles the transcoder wasm download and the worker set or throws on its first dome.
- Law: the meshopt leg is roster-gated at BOTH ends — `Glb.asset(roster, "meshopt")` resolves the served decoder module and `setMeshoptDecoder` attaches only on that hit, while one registered `GLTFLoaderPlugin` reads `extensionsRequired` off the parsed JSON chunk at `beforeRoot` and refuses `codec-absent` for an `EXT_meshopt_compression` asset the roster never admitted. Gating fires BEFORE any buffer decode, so an unserved decoder reads as the declared refusal instead of an opaque parse defect. Draco and ktx2 are HARD requirements — the roster resolve fails construction without them; meshopt is roster-CONDITIONAL because the served decoder is a classic-script leaf a composition may omit, and the `beforeRoot` gate refuses `codec-absent` rather than dying inside three's decompression arm.
- Law: the asset roster is a WIRE value and refuses at the consumer — `_SEGMENT` admits `slug` and `file` under the publishing plane's own alphabet (no separator, no traversal, no dot-only form) and the digest under the lowercase directory alphabet, so a row carrying a traversal leaf refuses at decode rather than composing `assets/<digest>/../<path>` into `setDecoderPath` and `import()`; the two ends spell one alphabet and neither re-derives the other's.
- Law: the parser's JSON chunk crosses as unknown — `GLTFParser.json` is an untyped platform value, so the gate decodes it through one `Schema` before reading a field; a property read straight off the parser is the erased-type defect this decode forecloses.
- Law: ONE residency walk owns every per-drawable resource in both directions — the traverse kernel visits each `Mesh` OR `Points` with its materials and THREE visitors ride it: upload runs `renderer.initTexture` over every decoded texture slot at graft time so the first sampling frame never hitches, accelerate builds `geometry.boundsTree` over the triangle-bearing subset alone, and release frees the bounds tree, then the geometry, then the texture slots, then the materials, because `material.dispose()` frees the program and never its textures. Coverage cannot fork between the arms — a resource one visitor reaches the other reaches, so an accelerated geometry cannot outlive its subtree and an eagerly uploaded texture cannot leak past teardown — and three's `traverse` callback is the kernel's platform-forced statement seam, marked on its first line, with no reference escaping it. Lazy first-use upload, a second private walk, and a predicate narrowed past the drawable union are the named defects.
- Law: the walk's coverage is BOUNDED and the bound is law, not luck — `_slots` reads `Object.values(material)`, which enumerates OWN properties, so it reaches every declared map field (`MeshPhysicalMaterial` holds its maps as plain own properties while `clearcoat`, `iridescence`, `sheen`, `transmission`, and `anisotropy` are prototype accessors carrying no texture) and reaches NOTHING held in `Material.userData` or inside a `MeshPhysicalNodeMaterial` TSL graph. That bound obligates this module: every texture it assigns lands in a DECLARED slot — `[8]`'s set-bind holds no private roster for exactly this reason — and the WebGPU node-material arm's debug views bind no texture of their own, so the kernel is provably total rather than incidentally total.
- Law: the graft lane outlives any refusal — a per-arrival `GlbFault` (`decode-refused`, `codec-absent`) folds into the bounded fact queue and the lane keeps consuming; an arrival stream that dies on one bad mesh is the named defect, the policy table's `evict` column governs the ledger consequence, and success and refusal join only after their respective settlement edges. Containment is the signature's own truth — both residency lanes and the dome lane fork with their refusals caught, so construction carries NO error channel and the roster resolve's `codec-absent` is the one failure a composing root ever handles; an unraisable channel buys every caller a recovery arm nothing reaches.
- Law: the graft lane is woven — each arrival's graft effect carries `Effect.withSpan("rasm.ui.scene.residency")` with the content key and byte count as log-and-span material, successful grafts feed `1` through `Effect.withMetric` into `_GRAFTED`, and `_REFUSED` folds refusal reasons through `Metric.trackErrorWith` over the closed `GlbFault.Reason` vocabulary (bounded tags by construction — the key never a tag); `Glb.hook` adopts the one fact stream behind the observe point, so app and probe taps never wrap the fold.
- Law: the bounds tree is residency, not a query cache — it is built at graft, parked on the geometry the release visitor already owns, and freed under the same arm, so tree lifetime cannot diverge from subtree lifetime. `indirect` is what makes that safe: the build leaves the source index buffer untouched, so `geometry.dispose()` stays that buffer's one owner. Building lazily at first pick pays a stall inside a pointer event, and holding a tree in a roster beside the ledger outlives the geometry it indexes.
- Law: three's prototypes are HOST-WIDE and this module is their ONE legal patch owner — the estate has a single viewport owner, so the pin is taken inside the viewport's own scoped construction rather than at module load, is idempotent under a repeat, and is never restored because un-patching under a second live viewport breaks its picks. This pin is all-or-nothing: the package's `declare module` merge promises `boundsTree`, `computeBoundsTree`, `disposeBoundsTree`, and `firstHitOnly`, so patching a subset leaves a declared member absent at runtime. `mark`'s pick reads the same pin — `Raycaster.firstHitOnly` routes through the accelerated prototype and nothing there re-patches.
- Law: the build runs OFF the render thread and single-flight — the pool refuses a second concurrent build outright, so the fold declares `concurrency: 1` rather than discovering the refusal; `useSharedArrayBuffer` is policy met against `globalThis.crossOriginIsolated` because that buffer class does not exist otherwise, and the pool's own façade degrades to a single-threaded builder on the same condition, so no caller branches on isolation. Build progress folds into the SAME dependency tap the codec `LoadingManager` feeds, never a second progress channel.
- Law: the snapshot band is rebuild avoidance and nothing more — `MeshBVH.serialize` yields transferable buffers the composing store parks beside the GLB octets under the same `ContentKey`, `deserialize` re-opens them against the geometry that key produced, and `setIndex` follows the `indirect` policy. Misses, refusals, and cardinality disagreements each rebuild; correctness never depends on the band.
- Law: `Glb.warm` is the hint family and it issues ONCE ahead of first frame — the `react-dom` document surface (`preload`, `preinitModule`, `preinit`) over the `_CODECS` roster join and the ledger's own `Residency.pending` census, addressed through the port's `addressed` read. The modality is a column, never a guess: `PreloadAs` carries no `wasm` member, so every wasm and worker leaf a loader fetches for itself rides `as: "fetch"`, the decoder this module reaches through `import()` rides `preinitModule`, and the classic-script location `[10]`'s embed arm hands the element rides `preinit` beside that pin. A hint whose CORS mode does not match the eventual request is discarded and re-fetched, so `_WARM` states the mode once; per-mesh hinting at draw time lands beside the fetch it was meant to precede and is the named defect.
- Law: the appearance lane is the graft's THIRD lane and dressing has ONE entry — a fresh document indexes once and re-dresses every held graft, a fresh graft dresses off the held document, and both call the same seat, so a mesh arriving before its appearance and an appearance arriving before its mesh converge on identical state. Re-dressing is safe because the seat is idempotent pure assignment over carried handles: it is exactly the dome's policy-re-read law applied to appearance, and a lane dressing only at graft strands every re-issued document behind an unchanged digest.
- Law: the ledger row is READ at graft, so `manifest-skew` has its raiser — an arrival whose key the published ledger never named refuses before any parse, which is exactly the reason the fault family declares, and the row it does find rides `Glb.Graft.kind` for the lifetime of the subtree. That column is consumed, never carried: `Residency.kind[kind].splatBorne` selects the draw arm — `_dress` reads it ONCE and routes a splat-borne graft to `_splat`, which binds the unified backend's `PointsNodeMaterial` with `vertexColor()` as its radiance node and retires the parse's own material through the same reader the release visitor uses, so a splat never enters `[8]`'s lobe bind and repainting one as a dielectric is the named defect — and `Residency.kind[kind].coneCullable` admits the row to `[7]`'s `Glb.cull` pass driving `Glb.visible`, so a non-cullable row is skipped by the pass rather than tested per frame.
- Law: a splat-borne payload parses to `Points`, so the drawable set the walk returns is the UNION both classes inhabit and the acceleration direction alone narrows back to `Mesh` — a bounds tree indexes triangles, so a point cloud files no snapshot and re-opens none, while upload and release stay total over the union. A walk narrowed to `Mesh` hands every splat subtree an empty visitor set: nothing uploads eagerly, nothing frees at eviction, and the leak is invisible because the ledger still drops the key.
- Law: TIME is `geo`'s one coordinate and DRAW is this renderer's own cadence — `setAnimationLoop` pumps the frame and reads `delta` off the published `Clock.Frame`, so a scrub, a hold, and a seek move mixers exactly as they move the geo surface's animated rows; a `Clock` minted here samples wall time a held or sought coordinate has already left, and the two then disagree the moment either is scrubbed.
- Exemption: the frame-loop tick is the platform-forced synchronous seam — `advance` reads the ledger through `Effect.runSync(Ref.get(held))` and the loop callback reads the frame through `Effect.runSync(SubscriptionRef.get(frames))`, both inside the marked kernel (pure sync reads, total by construction), and only immutable snapshots leave the fold.
- Growth: a new residency policy (priority lanes, partial LOD) is a fold arm over new ledger rows minted at `core/interchange/frame` — the graft signature never changes; a new animation policy (clip selection, cross-fade) is one action-policy row applied at mint; a new arrival consumer is one hook tap over the adopted fact stream, never a second port subscription; a new codec or wasm is one `Glb.AssetRoster` row plus one `_CODECS` row carrying its address form and hint modality, consumed through `Glb.asset`, never a parallel identity or path surface; a new draw arm is one `Residency.kind` column read beside `splatBorne`, never a second dressing entry.

```typescript signature
import { Convention, Residency, type ContentKey } from "@rasm/ts/core"
import { Context, Effect, HashMap, Metric, Option, Queue, Record, Ref, Schema, Scope, ScopedRef, Stream, SubscriptionRef } from "effect"
import { preinit, preinitModule, preload, type PreloadOptions } from "react-dom"
import {
  AnimationMixer, BufferGeometry, LoadingManager, LoopRepeat, Mesh, MeshPhysicalMaterial, Points, Scene, Texture,
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
    readonly kind: Residency.Kind
    readonly drawn: ReadonlyArray<Glb.Drawable>
  }
  type Ledger = HashMap.HashMap<ContentKey, Glb.Graft>
  // one codec record publishes the transcoder beside the loader consuming it, because the dome's deep-store
  // container decodes through that same configured instance and a second one owns a second worker pool
  type Codecs = { readonly gltf: GLTFLoader; readonly ktx2: KTX2Loader }
  type Codec = (typeof _codecSlugs)[number]
  type Address = (typeof _addresses)[number]
  type Warm = (typeof _warms)[number]
  // the two facts a served decoder leaf carries beyond its roster row: which address form its consumer joins
  // against, and which hint modality its own consumption earns
  type CodecRow = { readonly address: Glb.Address; readonly warm: Glb.Warm }
  type ResidencyFact =
    | { readonly _tag: "Arrived"; readonly arrival: GlbViewport.Arrival }
    | { readonly _tag: "Environment"; readonly key: ContentKey }
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
  key: ContentKey,
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
  key: ContentKey,
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

// The splat draw ARM, selected by the same `Residency.kind` read that skips the lobe bind: a splat carries
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
        // BOUNDARY ADAPTER — the node material is a mutable platform record and the swap is one assignment
        graft.drawn.forEach((drawn) => {
          if (!(drawn instanceof Points)) return
          const bound = new PointsNodeMaterial({ transparent: true })
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
  key: ContentKey,
  graft: Glb.Graft,
  index: Pbr.Index,
  document: GlbViewport.Appearance,
  codecs: Glb.Codecs,
  acquired: Glb.Acquired,
  facts: Queue.Queue<Glb.ResidencyFact>,
): Effect.Effect<void> =>
  Residency.kind[graft.kind].splatBorne
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
    const held = yield* Ref.make(HashMap.empty<ContentKey, Glb.Graft>())
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
        const row = yield* Option.match(HashMap.get(yield* port.ledger.get, arrival.key), {
          onNone: () =>
            Effect.fail(new GlbFault({ reason: "manifest-skew", mesh: `${arrival.key}`, detail: "<unledgered-arrival>" })),
          onSome: Effect.succeed,
        })
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
        const graft = { node: gltf.scene, mixer, kind: row.kind, drawn } satisfies Glb.Graft
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
  ledger: Residency.Ledger,
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
    Effect.sync(() => void Residency.pending(ledger).forEach((row) => Option.map(addressed(row.mesh), _HINTS.fetch))),
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

[ENVIRONMENT_FOLD]:
- Owner: the content-keyed dome fold — one environment per scene: the port's `environments` stream enters one lane, each arrival container-sniffed by magic (`OpenEXR` little-endian `20000630` at byte 0, Radiance `#?`, the KTX 2.0 twelve-byte identifier) — never a filename, format knob, or producer identity, so the C# press estate and the python ingest/IBL estate serve the same fold — and decoded through the matching `_CONTAINERS` row. ONE commit owns every dome write: it stamps the scene read-policy fields (`environmentIntensity` the wire `intensity`, `environmentRotation` the composed orientation, the `_ENV` backdrop row mirroring both onto `background` beside `backgroundBlurriness`), uploads a fresh source through the acquisition, mints the dome's inverse orientation once, retires only the handles its successor supersedes, and returns the row one `Ref`-held slot carries and the graft republishes as `Glb.Loop.dome`. Three callers reach that commit and nothing else writes the scene — a NEW key decodes and prefilters ONCE through `Glb.Prefilter.fromEquirectangular`, a SAME-key arrival carries the held source and target forward and re-reads policy alone, and `Glb.Loop.rebind` re-derives the target from the held source against a fresh acquisition. Each arrival's `sh9` lands as one `SphericalHarmonics3` beside that basis, so `Glb.irradiance` answers a directional query without re-reading the scene and without allocating.
- Packages: `three` (`HDRLoader` — the live spelling whose deprecated `RGBELoader` alias is never authored, `EXRLoader`, `RoomEnvironment`, `DataTexture`, `Texture`, `Euler`, `Quaternion`, `Vector3`, `SphericalHarmonics3`, `EquirectangularReflectionMapping`, `HalfFloatType`, `LinearFilter`/`LinearMipmapLinearFilter`, `SRGBColorSpace`/`LinearSRGBColorSpace`/`NoColorSpace`); `effect` (`Effect`, `Metric`, `Option`, `Queue`, `Ref`, `ScopedRef`, `Stream`); `@rasm/ts/core` (`Texture` — the frozen container roster this fold closes over).
- Law: one commit owns read policy, orientation, upload, and retirement together, and retirement is IDENTITY-keyed — a handle the successor carries forward never releases under it, the scene fields always stamp before a superseded handle disposes, and a torn dome is therefore unreachable rather than guarded against. Every entry reaching it supplies its own handle pair; a second write path to `scene.environment` is the fork this owner forecloses.
- Law: repeating the held key is a POLICY re-read, never a no-op — intensity and rotation are stored-frame reads, so re-exposing or re-orienting a dome is exactly the arrival the wire's own law blesses, arriving under the digest it never re-keys; dropping it strands the new policy behind an unchanged blob with nothing left to carry it, and re-decoding it burns a prefilter pass the stored frame already made unnecessary.
- Law: the producer frame is `+Z`-up and three samples `+Y`-up, so BOTH reads remap the BASIS and neither rewrites a stored value — the equirect stamps `R = Ry(rotation) · Rx(−π/2)` as one `YXZ` Euler on the environment fields, and an irradiance query un-rotates its world normal through the INVERSE of that same orientation, minted once at commit because a dome's rotation is fixed for its lifetime; the wire bands are never permuted, negated, or re-projected and the plane bytes never re-encode. Landing proves against the producer's axis fixture: a dome whose only non-zero band is `sh_2` answers `2π/3` at the producer's `+Z` pole and zero at its equator — a band-permuting runtime answers at the wrong pole, and a producer-side re-encode to Y-up is the fork the frozen mapping forecloses.
- Law: the prefiltered target is renderer-bound and the decoded source is not — `Glb.Loop.rebind` is the realized consequence, re-folding the held source through the fresh prefilter and retiring the dead target under the same identity-keyed arm, which is why the source outlives its prefilter pass and disposes only on replacement or scope close; a target surviving renderer identity is the leak, a source disposed at prefilter time is the re-init refetch, and a re-init that never calls `rebind` leaves the scene sampling a disposed texture.
- Law: the analytic floor is the slot's boot row — `fromScene(RoomEnvironment(), _ENV.floorBlur)` bakes once, the room scene disposes at the bake, and the keyless `Glb.Dome` row serves `scene.environment` until the first arrival supersedes it; the same bake answers `rebind` while no arrival has landed, and the floor's harmonics are the zero set so an irradiance query before any arrival reads black rather than a fabricated dome. `compileEquirectangularShader` warms beside it because the floor bake compiles the CUBEMAP path alone — without the warm, the first dome pays a shader compile mid-session — and the core class returns `void` where the unified class returns a promise, so the warm awaits a union rather than assuming one spelling.
- Law: the nine bands carry VERBATIM — the wire ships 27 values band-major with RGB interleaved at `i·3 + c`, which is exactly the stride `SphericalHarmonics3.fromArray` reads into `coefficients[i]`, and the member admits any `ArrayLike`, so the frozen carrier lands with neither a re-ordering site nor a defensive copy; a length other than 27 refuses `decode-refused` at the fold, and `getIrradianceAt` already reconstructs under the producer's own `π`, `2π/3`, `π/4` convolution constants, so no rescale exists here either. It reads its normal into locals before it writes its target, so a query aliases the two and allocates nothing.
- Law: the harmonics are a CPU read, NEVER a second scene light — `scene.environment` already carries the prefiltered diffuse, so seating the same coefficients on a `LightProbe` beside a live dome adds a second irradiance term and doubles the diffuse; the rig's `probe` row stays the pre-dome analytic seat, and `Glb.irradiance` serves probe receipts, mark tinting, and any analysis read off the GPU path.
- Law: every decoded source is scene-resident, so it crosses `renderer.initTexture` at commit exactly as a grafted texture slot does — the backdrop samples it every frame and the eager-upload law admits no lazy corner. `_ENV.type` is the declared read depth on the two `DataTextureLoader` rows alone, narrowing the wire's `rgba32f`/`rgba16f` planes once as a policy row; the deep-store row carries the store's own type out of its `vkFormat` and takes no read-depth declaration.
- Law: the deep-store row rides `[5]`'s ONE transcoder — `KTX2Loader.parse` is callback-shaped and refuses until `detectSupport` fills its worker config, an uncompressed KTX2 yields a `DataTexture` where the shipped declaration promises a `CompressedTexture` so the row binds `Texture` and never the declared subtype, and the transcoding leg TRANSFERS the arrival buffer to its worker. That transfer is unreachable here by wire law: an environment plane is float or half, block compression admits an 8-bit store alone, so a dome always carries the uncompressed payload and always reads `needs_transcoding` false.
- Law: an uncompressed KTX2 arrives POINT-SAMPLED and the row repairs it — the loader hands the block-compressed branch `LinearMipmapLinearFilter`/`LinearFilter` and the uncompressed branch `NearestMipmapNearestFilter`/`NearestFilter`, so exactly the store every channel plane and every deep dome uses is the one that samples nearest; `_ENV.filter` is the one policy row and `_filtered` stamps it after `parse`, keyed on whether the store carried a pyramid. Unrepaired, a KTX2 dome prefilters from a point-sampled equirect where the HDR and EXR rows prefilter from a linear-filtered one — one dome, two qualities, keyed only on the container the producer chose — and every KTX2 channel plane samples blocky at every mip transition.
- Law: an uncompressed KTX2 also arrives COLORLESS — the loader's DFD read has arms for BT.709, Display-P3, and UNSPECIFIED alone and returns `NoColorSpace` with a console warning for anything else, while this estate mints every scene-linear plane on ACEScc primaries and the producer's gate PROVES the DFD carries them. `_TRANSFER` therefore stamps the wire's own `transfer` column after `parse` — transcription of a column the producer already proved, never a fork of the loader's read, which stays authoritative for `srgb` and `raw` where its arms cover the primaries.
- Law: the lane weaves exactly as the graft lane — span, `_REFUSED` over the closed reason vocabulary, refusal into the one bounded fact queue — and an `Environment` fact commits only after the scene fields do.
- Boundary: the producer's GGX pyramid, split-sum LUT, and luminance CDF serve renderers evaluating their own IBL — `PMREMGenerator` fixes its mip layout and roughness mapping internally, so this fold re-derives the radiance chain from the equirect and consumes the producer's harmonics alone; binding a foreign pyramid into `scene.environment` forks the roughness-to-mip relation the renderer already owns.
- Law: `_CONTAINERS` is the CLOSED mirror of the frozen twelve — `{ readonly [K in Texture.Container]: Glb.Container }` — so a container the wire can name always has a row and the browser's answer to it is a column value, never an absent key. Each `Option` column carries one lookup modality over ONE roster: `probe` is the dome lane's magic-byte sniff and is present only where the container can hold radiance (`Option.none()` is a row the sniff never reaches, which is why an 8-bit preview container can never win a dome), while `decode` is the set-bind's DIRECT `_CONTAINERS[row.container]` lookup because the wire declares its own container and `Option.none()` IS the refusal — the browser has no path to `tiff16`, `tiff_f32`, `jxl`, `jxl_f16`, or deep EXR, and `qoi` its producer already refuses for planes. Second container tables, hand exclusion lists, and `switch` ladders beside the roster are the forks this closure forecloses.
- Law: the bitmap leg declares its own conversions OFF — `createImageBitmap` applies color management, premultiplication, and EXIF orientation by default, each of which rewrites a channel plane's stored value, so the three reachable eight-bit rows pass `colorSpaceConversion`/`premultiplyAlpha`/`imageOrientation` as `"none"` and the frozen top-left storage origin survives with `flipY` never set. Sixteen-bit PNG still truncates to eight bits in every shipping browser, so store width carries the refusal `[8]`'s bind raises off `TextureVocab.rows.plane[format].web`, never a silent precision loss inside this decode.
- Growth: a new container is one `_CONTAINERS` column edit — a probe where the dome can sniff it, a decode where the browser can reach it — so a synchronous, callback-shaped, or promise-shaped decoder needs no second lane and no caller knob; a second dome (per-viewport split lighting) is a slot-per-scene instantiation, the fold already scene-parameterized.

```typescript signature
// core's frozen vocabulary and three's texture class both spell `Texture`; the wire anchor takes the alias because
// three's class is the type this whole fold speaks, and the campaign's cross-module spelling for it is `TextureVocab`
import { type TextureSet, Texture as TextureVocab } from "@rasm/ts/core"
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
    readonly key: Option.Option<ContentKey>
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

[DRAW_COLLAPSE]:
- Owner: the collapse rows the graft applies at parse time, keyed by what repeats: same-material submeshes within one graft merge through `BufferGeometryUtils.mergeGeometries`; N identical geometries collapse into one `InstancedMesh` whose per-instance transform stamps through `setMatrixAt` and re-bounds through `computeBoundingSphere`; distinct same-material geometries batch into one `BatchedMesh` — `addGeometry` per unique geometry, `addInstance` per placement, `setVisibleAt` as the per-instance visibility toggle the selection echo and the compute-lane cull both drive. `Glb.cull` is that compute lane: one scoped pass per batch holding its storage buffers, its six frustum uniforms, and its readback store, dispatching `[4]`'s WebGPU-gated kernel and folding the verdict into the one visibility write.
- Packages: `three` (`InstancedMesh`, `BatchedMesh`, `Color`, `Frustum`, `LinearSRGBColorSpace`, `Matrix4`, `Sphere`, `Vector4`); `three/addons` (`BufferGeometryUtils`); `three/webgpu` (`WebGPURenderer`, `StorageBufferNode`); `three/tsl` (`Fn`, `bool`, `instancedArray`, `instanceIndex`, `select`, `uint`, `uniform`); `three-mesh-bvh` (`StaticGeometryGenerator` with its `applyWorldTransforms` column, `MeshBVH`); `effect` (`Array`, `Effect`, `Function`, `Option`, `Scope`).
- Law: collapse never alters appearance identity — a merged or batched node keeps its source meshes' material keys so `[8]`'s keyed override still targets exactly the meshes the wire names; a collapse that widens an override's blast radius is the named defect.
- Law: visibility is a flag, never a rebuild — `Glb.visible` flips `setVisibleAt` per slot and is the ONE write both drivers reach: the selection echo (`mark#ECHO_ROWS`) and `Glb.cull`'s compute pass over the `Residency.kind[kind].coneCullable` rows, so a non-cullable row is never tested per frame and neither driver mints a second visibility path; removing and re-adding geometry for a visibility change is the named defect.
- Law: `Glb.cull` is `[4]`'s scene-resident compute lane realized against that one sink — a `three/tsl` `Fn` graph over two `instancedArray` storage buffers (world-space `(center, radius)` in, a per-instance verdict out) dispatched through `renderer.computeAsync`, its verdict read back through `renderer.getArrayBufferAsync` off the verdict node's own attribute, and folded into `Glb.visible`. The pass is a SCOPED constructor holding its buffers, never a per-frame build: the storage attributes are GPU resources the folder's bracket law owns, and re-allocating them each frame would re-upload the whole cluster to save nothing.
- Law: the frustum is a CLOSED six-plane set, so the test folds at graph-build time into one boolean chain over six `uniform` vec4 rows — no loop node, no uniform array, no dispatch-time branch — and the per-frame CPU work is six `Vector4` writes; the planes derive through `Frustum.setFromProjectionMatrix` under `renderer.coordinateSystem`, because WebGPU's clip volume is not WebGL's and a hardcoded convention culls the near band on one backend and not the other.
- Law: the pass is backend-gated by the RENDERER CLASS, not by the device `Option` — `computeAsync` lives on the unified renderer alone and only the class probe carries that into the type plane, so the WebGL arm receives a pass that is `Effect.void` and renders the same scene uncullled; a caller branching on backend restates the gate the pass already absorbed.
- Law: a cluster bound is derived, never carried — `getBoundingSphereAt` answers the GEOMETRY's sphere and `getMatrixAt` the instance placement, so the world sphere is the transformed centre against the radius scaled by `Matrix4.getMaxScaleOnAxis`, which bounds a non-uniform instance conservatively rather than culling a stretched part out of frame. The ledger carries no bounds by its own law, so a viewer reading one off a residency row would read a column that does not exist.
- Law: the batched tint is the SAME color contract the lobe bind carries — `Glb.tinted` writes `setColorAt` through `Color.setRGB(r, g, b, LinearSRGBColorSpace)` over a `Theme.linear` triple, exactly as `[8]`'s `_tint` does, so a selection highlight and an authored base colour cannot land in two colour spaces; a per-instance colour written from a CSS string is the drift this shared seam forecloses.
- Law: the two geometry folds split on FRAME, not on size — `mergeGeometries` folds submeshes that already share a parent frame and applies no transform, so it is the draw-call collapse alone, while a whole assembly spans transforms and its world-space fold is `StaticGeometryGenerator`'s `applyWorldTransforms`, the exact column merging has no answer for. Feeding `mergeGeometries` an assembly stacks every part at the origin, and the bake is therefore a distinct owner rather than a bigger merge.
- Law: the assembly measure is ONE tree over ONE baked geometry — section-plane overlap and point-to-surface distance answer across the whole assembly through a single `MeshBVH` descent with no per-mesh iteration and no reuse of `[5]`'s per-graft trees, which index parts in their own local frames. That bake is a query structure, never scene residency: it never enters the graft ledger and never grafts under the root, so it is `Effect.acquireRelease` over the asking query's own scope rather than the graft's — the folder's every-GPU-resource-bracketed law reaching the one owner the residency walk provably cannot see, because a geometry outside the ledger has no release visitor to free it.
- Growth: a new repetition signal (an element-graph repetition census) is one detection row feeding the same three collapse arms — the arms never multiply; a new whole-assembly query is one descent over the same bake; a second cluster test (occlusion, screen-extent) is one more node in the same boolean chain over one more storage row, never a second pass.

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
// `Residency.kind[kind].coneCullable` admits — a second path would let one driver's hide survive the other's show
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

[APPEARANCE_BIND]:
- Owner: `Pbr` — ONE seat for an appearance on a material, both halves of the same C# projection: the SCALAR lobes and the BAKED plane set that hangs behind the same appearance key. `Material`/`PbrGroups`/`AppearanceSummary`/`TextureSet` arrive DECODED through `core/interchange/codec#LANDING_WIRE` (the verbatim mirror of the `csharp:Rasm.Materials/Appearance` projection), `Pbr.index` folds the whole appearance document into the one resolved ledger — appearance seats, keyed overrides carrying their inline lobe vectors, and the element-side `worn` pairing — `Pbr.resolve` answers one mesh, and `Pbr.seat` applies what it answered: the wire's nine parameter blocks onto `MeshPhysicalMaterial` lobes exactly as projected (base → `color`/`metalness`, specular → `roughness`/`specularColor`/`specularIntensity`/`ior`/`anisotropy`/`anisotropyRotation` — three's one microfacet roughness IS the wire's specular roughness, transmission → `transmission`, coat → `clearcoat`/`clearcoatRoughness`, fuzz → `sheen`/`sheenColor`/`sheenRoughness`, thin-film → `iridescence`/`iridescenceIOR`/`iridescenceThicknessRange`, emission → `emissive`/`emissiveIntensity` from the wire's `luminance`, geometry → `opacity`/`transparent`/`side`; the slots three declares no channel for — the diffuse-roughness and specular-tint scalars, coat colour and ior, transmission roughness, the subsurface band — stay render-unbound, never forged), then every seatable plane into the map slot `_ROLE_SLOT` names for its role, with `needsUpdate` stamped once at the tail. Every numeric is carriage: a TS-side derivation, regrouping, or convenience-merge of any OpenPBR parameter is the cross-language drift defect. Refusals ACCUMULATE and never fail the seat — a partially-bound set renders the authored asset with its seatable planes overlaid.
- Packages: `three` (`MeshPhysicalMaterial` lobes and its nineteen reachable map slots, `Color`, `Vector2`, `LinearSRGBColorSpace`, `RepeatWrapping`/`ClampToEdgeWrapping`, `FrontSide`/`DoubleSide` — members verified against the shipped runtime; `@types/three` supplies compile-time declarations only, never member truth); `@rasm/ts/core` (`AppearanceSummary`, `ContentKey`, `Material`, `PbrGroups`, `Texture`, `TextureSet`); `effect` (`Array`, `Effect`, `HashMap`, `Option`).
- Law: assignments mirror the projection's grouping — the fold's arm order IS the wire's block order, so a C# projection change lands here as the same-shaped field wave; a flattened group or renamed field breaks the mirror and the golden fixtures upstream.
- Law: unit semantics ride the SI quantity contract — weights arrive unit-interval, distances in the projection's units; a clamp, remap, or correction in the fold is the drift defect, and an out-of-range value is upstream evidence. `transparent` and `side` are the two render-representation toggles three demands (`opacity < 1` raises the blend flag, `thinWalled` selects `DoubleSide`) — structural consequences of carriage, and no other computed value exists in the lobe fold.
- Law: color triples are linear-space carriage — `Color.setRGB(r, g, b, LinearSRGBColorSpace)` ingests under three's `ColorManagement`, the renderer's `outputColorSpace` owns display transform, and a THEME color reaching the scene (selection tint, highlight) crosses through `Theme.linear` (`system/token`'s OKLCH → srgb-linear projection) into the same `setRGB` seam — one color contract, drift structurally impossible; no gamma math exists in this module.
- Law: `AppearanceSummary` is the preload census and `Pbr.index` is where it is READ — the summary roster fixes the appearance key space, `sets` join it on `TextureSet.appearanceKey`, `materials` join on the appearance key each override was FETCHED under (the wire carries no key column — the payload rides behind the key at transport, so the carrier states the pairing and `Material.openPbr` nests the whole lobe vector inline), and `worn` carries the mesh pairing no appearance wire holds. A `worn` mesh naming an appearance the census never listed, an override fetched under an uncensused key, or a set hanging behind an unlisted key each refuse `manifest-skew` at the index — which is what makes the "dangling reference is upstream evidence, never a silent default material" clause reachable instead of aspirational.
- Law: appearance is an OVERLAY, never a repaint — the bind applies only to meshes `worn` names, GLB-embedded materials on unpaired meshes ride untouched, and every write is idempotent pure assignment driven by the same keyed ledger the graft holds, so a re-seat over carried handles disposes nothing.
- Law: the set-bind holds NO private texture roster — every decoded plane is assigned into a DECLARED material slot, which is the only reason `[5]`'s one residency walk reaches it; a texture parked in a local map, a `userData` bag, or a closure leaks past `_release` with nothing left to free it. Retirement is IDENTITY-keyed exactly as the dome's is: a re-seat carrying a texture forward disposes nothing, and a superseded handle releases only where its successor supersedes it. Upload rides the same `_upload` visitor — a set bound after graft calls `renderer.initTexture` at the seat, exactly as the dome source does at its commit.
- Law: a role binds only where three declares a slot AND the plane stores the component that slot samples — `_ROLE_SLOT` carries both facts, and the width proof is arithmetic over the wire's own column: `.r`/`.x` needs one stored component, `.g`/`.y` two, `.b`/`.rgb`/`.xyz` three, `.a`/`.rgba` four, so `TextureVocab.rows.plane[format].width` decides. The consequence is structural and generative rather than a list: `specular_weight` and `fuzz_roughness` read `.a`, `base_metalness` reads `.b`, `geometry_opacity` and `specular_roughness` read `.g`, and every one of them has roster width floor one — so each is correct ONLY inside a four-wide pack and refuses `manifest-skew` as a standalone plane. The same arithmetic refuses a two-component direction store bound to `normalMap`, because three samples `.xyz` and reconstructs nothing.
- Law: fifteen of the thirty-four roles have no `MeshPhysicalMaterial` slot and their arm is a POLICY, never an omission — `geometry_tangent` and `geometry_coat_tangent` are the `TANGENT` accessor's business, `curvature` and the `subsurface_*` pair are analysis planes a `probe` debug view reads, and the remainder are scalar-only lobes the lobe fold already carries. Each surfaces `plane-unbound` as evidence on the one fact queue, so an unslotted role is visible rather than silently dropped.
- Law: a pack plane seats ONE texture in several slots and its FALSE slots are the neutral stamp — `TextureVocab.rows.pack[pack].slots` is the read order in slot order, so a present slot takes the shared plane and an absent one takes `TextureVocab.rows.channel[role].neutral` on its scalar companion, which is exactly what that column exists to answer. `TextureVocab.rows.pack[pack].gltf` is the legality: the occlusion-first order IS glTF's read order and binds to `.r`/`.g`/`.b`, while the inverted order swaps R and B — so an `mra` pack refuses `manifest-skew` rather than rendering occlusion as metalness.
- Law: only a browser-reachable store binds — `TextureVocab.rows.plane[format].web` is false exactly for the one- and two-component sixteen-bit integer stores, which have no Vulkan format row in the KTX2 read path, and `_widthFloor` routes every direction plane to width two, so the natural high-precision normal store is the undecodable one; the refusal reads off the column and the producer's re-route is upstream work, never a viewer transcode.
- Law: the SET carries no uv transform and this bind mints none — `Texture.offset`/`repeat`/`center`/`rotation`/`matrix` exist, and their only wire source in this estate is `KHR_texture_transform` on the GLB, which the graft's parse already applied to the mesh's own material. A set plane therefore OVERLAYS a mesh whose uv transform is already settled, `channel` stays `0` because no set document declares a second uv set, and a set-level transform column is a producer decision that has not been made.
- Law: the per-plane stamp reads four sources and nothing else — the wire row's `transfer` through `_TRANSFER`, the set's `tiled` column (the producer's own TileGate-proven coherence, never a caller assertion) selecting `RepeatWrapping` or `ClampToEdgeWrapping`, the set's `alphaMode` selecting `premultiplyAlpha`, and the acquisition's settled `anisotropy`; `flipY` is never written because the frozen storage origin is top-left and every leg already defaults it false. The KTX2 leg stamps `premultiplyAlpha` itself off the DFD alpha flag, so the set stamp is the authority for the bitmap and data-texture legs alone.
- Law: a set whose extent three cannot seat refuses rather than binds a slice — a `layerLaw` other than `none` names a cube, array, volume, or frame set no 2D map slot carries, and a non-empty `udimTiles` names a Mari tile grid three has no sampler for; both surface `plane-unbound` so the authored GLB material rides, and a new layer family is one seating row rather than a silent first-tile bind.
- Law: the WebGPU path swaps the material class, never the fold — `MeshPhysicalNodeMaterial` (`three/webgpu`) carries the same lobe fields AND the same map slot names, so both halves land unchanged; TSL node-graph authoring is reached only where a lobe becomes a computed node (a probe-driven debug view), and such a graph is render-side presentation holding no texture of its own, which is what keeps `[5]`'s walk total.
- Growth: a new OpenPBR block is one wire mirror field wave with one assignment arm here, landed in the same wave as the C# projection change; a new ROLE is one `_ROLE_SLOT` row whose `slot`/`component`/`scalar` columns decide seating, refusal, and neutral stamping with no predicate widening; a new pack order is one `TextureVocab.rows.pack` row its own `gltf` column governs — the seat signature never changes and TS never emits a block ahead of the wire.

```typescript signature
import type { AppearanceSummary, Material as MaterialRow, PbrGroups, TextureSet } from "@rasm/ts/core"
import { Array, Effect, HashMap, Option } from "effect"
import {
  ClampToEdgeWrapping, DoubleSide, FrontSide, LinearSRGBColorSpace, RepeatWrapping,
  type Color, type MeshPhysicalMaterial, type Texture as Plane,
} from "three"

declare namespace Pbr {
  // one seat per mesh: the census row always, the override and its lobe graph where the roster carried them, and
  // plus the plane set where one hangs behind the same appearance key
  type Bound = {
    readonly appearance: ContentKey
    readonly summary: AppearanceSummary
    readonly override: Option.Option<MaterialRow>
    readonly set: Option.Option<TextureSet>
  }
  // this resolved appearance ledger holds one map per join the wire actually proves, each under the key it is
  // addressed by, so no lookup re-derives a coordinate and no second roster exists; the lobe graph is not a map
  // because the wire nests it inline — `override.openPbr` IS the lobe read, and a separate lobes roster would be
  // a second truth a dangling digest could fork
  type Index = {
    readonly seats: HashMap.HashMap<ContentKey, Pbr.Seat>
    readonly overrides: HashMap.HashMap<ContentKey, MaterialRow>
    readonly worn: HashMap.HashMap<ContentKey, ContentKey>
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
    readonly address: readonly [ContentKey, string]
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
    ...(set.layerLaw === "none" ? [] : [refuse("plane-unbound", "<layer-law-unseated>")]),
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
      HashMap.empty<ContentKey, Pbr.Seat>(),
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
      overrides: Array.reduce(document.materials, HashMap.empty<ContentKey, MaterialRow>(), (held, [appearance, override]) =>
        HashMap.set(held, appearance, override)),
      worn: Array.reduce(document.worn, HashMap.empty<ContentKey, ContentKey>(), (held, [mesh, appearance]) =>
        HashMap.set(held, mesh, appearance)),
    }
  })

// one mesh in, its whole appearance out: an unpaired mesh answers none and keeps the GLB material the graft parsed
const _resolve = (index: Pbr.Index, mesh: ContentKey): Option.Option<Pbr.Bound> =>
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
- Owner: the `model-viewer` backend row — the zero-GL-handle embed: `.src` takes a `model/gltf-binary` object URL minted over a port arrival's bytes, `camera-controls` owns orbit interaction, and the element owns decode, upload, camera, and dispose internally; decoder statics (`dracoDecoderLocation`, `ktx2TranscoderLocation`, `meshoptDecoderLocation`) resolve the matching `Glb.AssetRoster` rows BEFORE the first model or the element side-loads from a foreign CDN — a CSP breach; each location reads its address form off `[5]`'s `_CODECS` column rather than re-choosing it — the multi-leaf pair takes the directory (the element joins its own leaf names) and `meshoptDecoderLocation` the file, one classic-script leaf whose `preinit` hint rides beside this pin because the DOCUMENT is what executes it; the statics are process-global upstream, so `Glb.pinned` is token-gated — the first roster fixes the process decoder set, an identical re-pin no-ops, and a divergent roster refuses `manifest-skew` instead of re-pointing decoders under a live element. Its animation surface is native: `play`/`pause` with `PlayAnimationOptions`, `appendAnimation` for additive blend, `availableAnimations` as the clip census — the embed arm's mirror of `[5]`'s mixer family.
- Packages: `@google/model-viewer` (`ModelViewerElement` — the const IS the statics owner and the instance type).
- Law: the element and the three arm share ONE physical `three` module (peer-deduped) but never a renderer, canvas, or GL context — sibling backends the backend literal selects per viewport.
- Law: the object URL lifecycle is bracketed — `URL.createObjectURL` acquires, `URL.revokeObjectURL` releases with the viewport scope; a leaked object URL pins the blob.
- Boundary: camera read/write on the element (`getCameraOrbit`/`getCameraTarget`/`jumpCameraToGoal`, the `camera-change` event) is `geo#CAMERA`'s adapter row; the hotspot seam is `mark`'s embed adapter family — `Mark.ray` mints anchors from the element's own ray, `Mark.mounted` is the declarative slot + `data-*` record that IS a pin's existence on the element, and `Mark.moved`/`Mark.queried` carry the imperative move and the placement read — this page hands the element over and spells none of those members itself.

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

- [APPEARANCE_KEY_SPELLING]-[OPEN]: does `MaterialWire.key` carry the `Node.Appearance` content key the summary roster and the set both hang on, or the material family's own key; the seat joins overrides on the appearance key and a family key there strands every override; verify against the `csharp:Rasm.Materials/Appearance` projection that mints the field.
- [ACESCC_WORKING_SPACE]-[OPEN]: scene-linear planes carry ACEScc primaries while three's working space is `srgb-linear`, so a stamped `LinearSRGBColorSpace` transcribes the transfer and leaves the gamut unconverted; verify whether the estate lands a primaries conversion at the producer, a working-space change through `ColorManagement`, or accepts the AP1-in-Rec.709 read.
- [PREMULTIPLY_ON_DATA_TEXTURE]-[OPEN]: `Texture.premultiplyAlpha` is documented against the image-upload path, so its effect on a `DataTexture` or a transcoded store is unproven; verify against three's texture upload source before the set stamp is trusted on those two legs.
- [TRANSCODE_TARGET_REBIND]-[OPEN]: does a second `KTX2Loader.detectSupport(renderer)` re-fill an already-populated `workerConfig`, or is the first call terminal; the re-init arm re-probes the held instance because the one-transcoder ruling forbids a second, so a terminal first call would leave a WebGPU-selected target sampling under the degraded WebGL floor; verify against `KTX2Loader`'s shipped source in `three/examples/jsm/loaders`.
- [STORAGE_READBACK_SHAPE]-[OPEN]: `getArrayBufferAsync` declares `BufferAttribute`, and `instancedArray` mints a `StorageInstancedBufferAttribute`; whether the backend's readback path serves the instanced subclass or only the plain `StorageBufferAttribute` decides whether the cull's verdict buffer swaps to `attributeArray`; verify against the WebGPU backend's readback implementation.
- [SPLAT_BLEND_ORDER]-[OPEN]: a gaussian splat's per-point radiance may demand premultiplied blending and back-to-front ordering that `transparent` alone does not supply; the arm binds radiance and declares transparency because `vertexColor()` is a `vec4`, and the ordering question is upstream of it; verify against the producer's own splat encoding before a blend or sort policy row lands.
