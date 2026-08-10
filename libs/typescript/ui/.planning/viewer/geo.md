# [UI_GEO]

The one geospatial surface-and-camera owner: one maplibre `Map` owns the WebGL context, camera, and declarative style; one `MapboxOverlay` interleaves deck.gl layers into that same context through the `IControl` rail; the layer tree is a pure value derived from the atom fold and pushed at the single `setProps` sink; and camera authority is this page's `Camera` vocabulary — one `Camera.State` across every render backend, a closed intent family as the only write path, pure screen↔world math for derived anchors. One rAF-fed `Clock` owner publishes the viewer stratum's single time coordinate, so every animated row and every renderer's draw cadence read one `{now, delta}` frame. GeoArrow layers stream `apache-arrow` columns zero-copy from the explicit IPC decode seam — the decoded `Table` doubling as the multi-surface bus the chart owner consumes — tile streaming rides one engine with vector, terrain, 3D-tile, and LAS point-cloud payload rows behind a resilient TTL cache, the discrete-global-grid cell family is one scheme-keyed table, the extension pack is an eight-capability roster on any layer, 3D relief/sky/globe are scene-config rows on the map, position capability enters through the folder-declared `Position`/`Grant` ports, and `@turf/turf` runs planar ops as the NTS-equivalent browser peer over already-decoded GeoJSON through one bounded op algebra — WKB decode stays behind `core/interchange/codec`'s `WkbParser` port, and this module never parses a geometry byte. The module is `ui/viewer/src/geo.ts`.

## [01]-[INDEX]

- [02]-[SURFACE]: scoped map + interleaved overlay, relief/sky/globe rows, the closed chrome rail, position ports; `Geo`.
- [03]-[FRAME_CLOCK]: the one rAF-fed time coordinate, its drive family, the atom bridge; `Clock`.
- [04]-[CAMERA]: the `Camera.State` vocabulary, the closed intent family, backend adapter rows; `Camera`.
- [05]-[PROJECT]: pure screen↔world math — anchors, mercator crossings, geometry-to-intent folds; `Camera`.
- [06]-[LAYER_ROWS]: the atom-derived layer vocabulary — GeoJSON, arrow fan, tiles, clouds, cells, trips, WMS; `Geo`.
- [07]-[EXTENSION_ROWS]: both injection planes — the `LayerExtension` roster and the screen-space depth pass; `Geo`.
- [08]-[PLANAR_OPS]: the turf peer algebra — relation, overlay, projection, traversal, measurement, and the CRS admission; `Geo`.
- [09]-[STYLE_DATA]: the style sub-owner, the source-data sink, the feature-state echo, the glyph registry, DOM pins; `Geo`.

## [02]-[SURFACE]

[SURFACE]:
- Owner: `Geo.surface` — one scoped acquisition: `new MapLibreMap(options)` over the app-provided container, `new MapboxOverlay({ interleaved: true })` added through `map.addControl` (deck registers a `CustomLayerInterface` per layer into the shared context and depth buffer, so 3D deck geometry occludes against basemap layers); release removes the control — deck's full teardown rides the `IControl.onRemove` hook — then `map.remove()`: one context, one camera, one teardown order.
- Law: relief, sky, and globe are scene-config rows on the one map — a `raster-dem` source through the `addSource` rail feeds `setTerrain({ source, exaggeration })`, `setSky` and `setLight` take `*Specification` data, and `setProjection({ type: "globe" })` swaps the projection without touching a layer; each row is a live re-config, never a map rebuild.
- Law: chrome is a CLOSED family over one `addControl(control, position)` rail — `Geo.chrome` folds `Chrome.Rail` cases (`Navigate`, `Scale`, `Locate`, `Relief`, `Globe`) into their shipped classes and adds each at its row's `ControlPosition`, exactly as the overlay joins; the arms carry their own construction options because the shipped classes disagree on arity (`NavigationControl`/`ScaleControl` take an optional bag, `GeolocateControl`/`TerrainControl` a required one, `GlobeControl` none), so the discriminant is what keeps the rail uniform. A hand-built DOM widget over a shipped row is the named defect, and the add is `Effect.acquireRelease` because a control outliving its surface's scope holds a DOM node and an event subscription the map's own teardown never reaches.
- Law: `Geo.Fix` carries the WHOLE platform coordinate, never a two-field slice — `lnglat` is the pair the camera vocabulary already speaks, `accuracy` is total, and `altitude`/`altitudeAccuracy`/`heading`/`speed` are `Option` because the platform reports each as absent on a device that cannot measure it, while `at` is the fix instant a survey trace joins on; a port narrower than the satisfying value forces the composition to author the lossy projection, and altitude with heading is exactly what a camera-follow atom folding `Camera.Intent.LookAt` reads.
- Law: `Grant.query` is generic in the platform's own name axis — `<Name extends PermissionName>` returning the whole status record minus the re-narrowed `name`, so its `state` is the live verdict rather than a flattened literal and a new capability grant (the OPFS residency budget, an AR arm's camera) is a call site instead of a page edit; a hand-listed name union re-narrows what the platform already closes and re-forks on every addition.
- Law: `Grant.changes` is the port's second modality and revocation is a live fact — the platform status record extends `EventTarget` and fires `change` on the same object `query` returned, so a viewer row gates on the current verdict rather than the one it read at mount; the ui-declared port carries the stream and the substrate satisfies it at the app root by lifting those events, because the platform ships the observable status and the query alone strands every revoke. Only `state` moves on a change, so the stream carries `PermissionState` and takes the name un-parameterized — a type argument the return never mentions is an unwitnessed generic.
- Law: the grant port's fault channel is `never` by declaration — a refused permission is a `state` value on the record, while the substrate's own refusal reasons name a caller-side or document-state defect no viewer arm can act on, so the composing layer escalates them at the satisfying seam and `E` stays total over actionable faults.
- Law: clipboard grant custody is NOT here and the name axis never reached it — the platform's `PermissionName` closes at nine members (`camera`, `geolocation`, `microphone`, `midi`, `notifications`, `persistent-storage`, `push`, `screen-wake-lock`, `storage-access`) with no clipboard entry, so a clipboard query cannot type against this port at all; `system/primitive`'s `Clipboard` port owns the affordance, the sanitize gate, its fault, and whatever descriptor widening its own read demands. The generic name axis is exactly as wide as the platform union and never wider, so a grant this port cannot spell is evidence of a second owner rather than a missing arm.
- Law: events fold into atoms — `map.on(...)` returns `Subscription`s registered as scope finalizers, and each handler body rides `useEffectEvent` where it closes over changing values so the subscription never re-binds per render; React owns only mount/unmount and the imperative map lifecycle never leaks into render. Module-level worker policy (`prewarm`, `addProtocol` for authed tile transport) is app-composition material set before the first `Map`; `transformRequest` routes tile URLs through the app's auth boundary.
- Growth: a second viewport is a second `surface` call with its own scope; the module never holds a singleton map.

```typescript
import { MapboxOverlay } from "@deck.gl/mapbox"
import { Fault } from "@rasm/ts/core"
import { Array, Context, Data, type DateTime, Effect, type Option, Schema, type Scope, type Stream } from "effect"
import {
  GeolocateControl, GlobeControl, Map as MapLibreMap, NavigationControl, ScaleControl, TerrainControl,
} from "maplibre-gl"
import type {
  ControlPosition, IControl, LightSpecification, MapOptions, SkySpecification, TerrainSpecification, Unit,
} from "maplibre-gl"

declare namespace Geo {
  type Surface = { readonly map: MapLibreMap; readonly overlay: MapboxOverlay }
  type Fix = {
    // the whole platform coordinate: the four measurable-or-absent facts ride Option, the fix instant rides the scalar owner
    readonly lnglat: readonly [number, number]
    readonly accuracy: number
    readonly altitude: Option.Option<number>
    readonly altitudeAccuracy: Option.Option<number>
    readonly heading: Option.Option<number>
    readonly speed: Option.Option<number>
    readonly at: DateTime.Utc
  }
}

declare namespace Grant {
  // the platform record whole: `state` is the live verdict, and re-narrowing `name` recovers the literal the DOM widens to `string`
  type Status<Name extends PermissionName> = Omit<PermissionStatus, "name"> & { readonly name: Name }
}

const _positionFamily = Fault.Class.family(
  ["denied", "unavailable", "timeout"] as const,
  {
    denied: { class: "denied" },
    unavailable: { class: "unavailable" },
    timeout: { class: "expired" },
  },
)

class PositionFault extends Schema.TaggedError<PositionFault>()("PositionFault", {
  reason: _positionFamily.schema,
}) {
  get class(): Fault.Class.Kind {
    return _positionFamily.classOf(this.reason)
  }
  override get message(): string {
    return `<position:${this.reason}>`
  }
}

class Position extends Context.Tag("ui/viewer/Position")<Position, {
  readonly current: Effect.Effect<Geo.Fix, PositionFault>
  readonly watch: Stream.Stream<Geo.Fix, PositionFault>
}>() {}

class Grant extends Context.Tag("ui/viewer/Grant")<Grant, {
  readonly query: <Name extends PermissionName>(name: Name) => Effect.Effect<Grant.Status<Name>>
  readonly changes: (name: PermissionName) => Stream.Stream<PermissionState> // revocation is a live fact: the record's own change signal, never a mount-time read — only `state` moves, so no name parameter flows to the return
}>() {}

// ONE terrain row: `_relief` applies it and the `Relief` chrome arm toggles exactly it, so the control and the
// scene config cannot disagree about which DEM is on — a per-arm spec would fork the same terrain into two answers
const _RELIEF = { source: "relief-dem", exaggeration: 1.2 } as const satisfies TerrainSpecification

const _SKY = { "atmosphere-blend": 0.8, "sky-horizon-blend": 0.5 } as const satisfies SkySpecification

const _LIGHT = { anchor: "viewport", intensity: 0.4 } as const satisfies LightSpecification

const _surface = (options: MapOptions) =>
  Effect.acquireRelease(
    Effect.sync(() => {
      const map = new MapLibreMap(options)
      const overlay = new MapboxOverlay({ interleaved: true, layers: [] })
      map.addControl(overlay)
      return { map, overlay } satisfies Geo.Surface
    }),
    (surface) =>
      Effect.sync(() => {
        surface.map.removeControl(surface.overlay)
        surface.map.remove()
      }),
  )

const _relief = (surface: Geo.Surface, demTiles: string): Effect.Effect<void> =>
  Effect.sync(() => {
    surface.map.addSource(_RELIEF.source, { type: "raster-dem", url: demTiles })
    surface.map.setTerrain({ source: _RELIEF.source, exaggeration: _RELIEF.exaggeration })
  })

const _globe = (surface: Geo.Surface): Effect.Effect<void> =>
  Effect.sync(() => void surface.map.setProjection({ type: "globe" }))

const _sky = (surface: Geo.Surface): Effect.Effect<void> =>
  Effect.sync(() => {
    surface.map.setSky(_SKY)
    surface.map.setLight(_LIGHT) // one atmosphere row: sky and light are the same live re-config, never two calls a caller sequences
  })

declare namespace Chrome {
  // the closed chrome family: each arm owns its own construction options because the shipped classes disagree on
  // arity, so the discriminant is what keeps the one `addControl` rail uniform across five different constructors
  type Rail = Data.TaggedEnum<{
    Navigate: { readonly compass: boolean; readonly pitch: boolean }
    Scale: { readonly unit: Unit }
    Locate: { readonly track: boolean }
    Relief: {} // toggles the ONE `_RELIEF` row `_relief` applies: a per-arm spec would fork one terrain into two answers
    Globe: {}
  }>
  type Row = { readonly rail: Chrome.Rail; readonly at: ControlPosition }
}

const _Rail = Data.taggedEnum<Chrome.Rail>()

const _control = (rail: Chrome.Rail): IControl =>
  _Rail.$match(rail, {
    Navigate: ({ compass, pitch }) => new NavigationControl({ showCompass: compass, visualizePitch: pitch }),
    Scale: ({ unit }) => new ScaleControl({ unit }),
    Locate: ({ track }) => new GeolocateControl({ trackUserLocation: track, showAccuracyCircle: true }),
    Relief: () => new TerrainControl(_RELIEF),
    Globe: () => new GlobeControl(),
  })

// the chrome set is scope-bracketed like the overlay: a control outliving its surface holds a DOM node and an event
// subscription `map.remove()` never reaches, so acquisition and removal ride the same scope the map does
const _chrome = (surface: Geo.Surface, rows: ReadonlyArray<Chrome.Row>): Effect.Effect<void, never, Scope.Scope> =>
  Effect.asVoid(Effect.acquireRelease(
    Effect.sync(() =>
      // BOUNDARY ADAPTER — the mint-then-add pair is the platform's own registration seam; the handles detach immutable
      Array.map(rows, (row) => {
        const control = _control(row.rail)
        surface.map.addControl(control, row.at)
        return control
      })),
    (added) => Effect.sync(() => Array.forEach(added, (control) => void surface.map.removeControl(control))),
  ))
```

## [03]-[FRAME_CLOCK]

[FRAME_CLOCK]:
- Owner: `Clock` — the viewer stratum's ONE time authority, exported beside `Camera` because `scene`'s mixer advance reads the same coordinate this page publishes: `Clock.live(drive)` is a scoped acquisition whose interior is one `requestAnimationFrame` registration lifted through `Stream.asyncScoped`, whose fold is one Mealy step advancing a carried `{frame, stamp}` against the live drive row, and whose published surface is a `SubscriptionRef<Clock.Frame>` the atom plane binds read-only through `Atom.subscribable`; `Clock.Frame` is `{now, delta}` — `now` the scene time coordinate every animated row reads, `delta` the per-frame advance every mixer consumes.
- Law: the boot posture is a drive value, not a scalar — `Clock.live` opens on the SAME closed family every later write speaks, so a replay that boots held or boots at a scrub position is one constructor call and no arm carries a start-rate parameter beside the vocabulary that already expresses it.
- Packages: `effect` (`Data`, `Effect`, `Ref`, `Scope`, `Stream`, `SubscriptionRef`).
- Boundary: the frame atom binds at `system/atom#LIVE_BRIDGE` and the scene mixer's delta read is `scene#RESIDENCY_GRAFT`'s; this page owns the coordinate, never its bindings.
- Law: the clock owns the TIME coordinate and each renderer owns its DRAW cadence — a deck `Deck`/`MapboxOverlay` pumps its own `AnimationLoop` and a three renderer its own `setAnimationLoop`, and both read `delta`/`now` off this one frame instead of sampling a timer of their own; a second time producer beside this owner un-reconciles construction-sequence scrub, because a scrubbed coordinate and a wall-clock coordinate disagree the moment either is held.
- Law: the drive is a closed family, never a boolean — `Run` advances by real elapsed scaled by `rate` (a rate of `0` is not `Hold`: a held clock reports `delta: 0` while a zero-rate run still re-stamps), `Hold` freezes `now` and reports `delta: 0`, `Seek` jumps `now` to a scrub position and reports the signed jump as that frame's `delta` so a mixer lands on the sought pose in one step. `Seek` self-settles: the frame after a seek measures no distance and holds, so resuming is one `Run` write and no arm carries a one-shot flag.
- Law: the stamp is the platform's own argument — the rAF callback receives its `DOMHighResTimeStamp` and every arm re-stamps the carried state, so no arm reads an ambient clock and the first advance after any drive change measures a real interval rather than the epoch.
- Law: the frame bridge is freshest-wins — the stamp stream carries one slot under the sliding strategy, because a stalled consumer must never backpressure the browser's own frame callback and a dropped stamp costs a coarser `delta`, never a lost coordinate; frames are published by a scope-forked drain, so the clock never runs an effect from inside a callback.
- Growth: a new drive temperament (a ping-pong scrub, a fixed-step replay) is one case plus one arm; a new consumer is one read of the published frame, never a second registration.

```typescript
import { Data, Effect, Ref, type Scope, Stream, SubscriptionRef } from "effect"

declare namespace Clock {
  type Frame = { readonly now: number; readonly delta: number }
  type Drive = Data.TaggedEnum<{
    Run: { readonly rate: number }
    Hold: {}
    Seek: { readonly at: number }
  }>
  type Held = { readonly frame: Clock.Frame; readonly stamp: number }
  type Live = {
    readonly frames: SubscriptionRef.SubscriptionRef<Clock.Frame>
    readonly drive: (drive: Clock.Drive) => Effect.Effect<void>
  }
}

const _Drive = Data.taggedEnum<Clock.Drive>()

const _ORIGIN: Clock.Held = { frame: { now: 0, delta: 0 }, stamp: 0 }

const _advanced = (held: Clock.Held, drive: Clock.Drive, stamp: number): Clock.Held =>
  _Drive.$match(drive, {
    // every arm re-stamps, so the first advance after any drive change measures a real interval
    Run: ({ rate }) => ({ frame: { now: held.frame.now + (stamp - held.stamp) * rate, delta: (stamp - held.stamp) * rate }, stamp }),
    Hold: () => ({ frame: { now: held.frame.now, delta: 0 }, stamp }),
    Seek: ({ at }) => ({ frame: { now: at, delta: at - held.frame.now }, stamp }), // the signed jump lands a mixer on the sought pose in one step; the next frame measures nothing and holds
  })

const _stamps: Stream.Stream<number> = Stream.asyncScoped<number>(
  (emit) =>
    Effect.acquireRelease(
      Effect.sync(() => {
        // BOUNDARY ADAPTER: the platform frame callback is the one push seam — emissions are void-discarded and the handle never escapes
        const pump = { handle: 0 }
        const step = (stamp: number): void => {
          pump.handle = globalThis.requestAnimationFrame(step)
          void emit.single(stamp)
        }
        pump.handle = globalThis.requestAnimationFrame(step)
        return pump
      }),
      (pump) => Effect.sync(() => globalThis.cancelAnimationFrame(pump.handle)),
    ),
  { bufferSize: 1, strategy: "sliding" }, // freshest-wins: a stalled consumer coarsens delta, never backpressures the browser
)

const _live = (opening: Clock.Drive): Effect.Effect<Clock.Live, never, Scope.Scope> =>
  Effect.gen(function* () {
    const drive = yield* Ref.make<Clock.Drive>(opening) // the boot posture is a drive value: a held or scrubbed start needs no second parameter
    const frames = yield* SubscriptionRef.make<Clock.Frame>(_ORIGIN.frame)
    yield* Effect.forkScoped(
      Stream.runForEach(
        Stream.mapAccumEffect(_stamps, _ORIGIN, (held, stamp) =>
          Effect.map(Ref.get(drive), (row) => {
            const next = _advanced(held, row, stamp)
            return [next, next.frame] as const
          })),
        (frame) => SubscriptionRef.set(frames, frame), // the drain is scope-forked: no run call fires inside the callback
      ),
    )
    return { frames, drive: (row) => Ref.set(drive, row) }
  })

declare namespace Clock {
  type Shape = {
    readonly Drive: typeof _Drive
    readonly live: typeof _live
  }
}

const Clock: Clock.Shape = {
  Drive: _Drive,
  live: _live,
}
```

## [04]-[CAMERA]

[CAMERA]:
- Owner: `Camera` — the camera vocabulary spanning every backend: `Camera.State` (center `[lng, lat]`, `zoom`, `bearing`, `pitch` — the shape both the maplibre getters and deck's `MapViewState` speak), the intent family `Camera.Intent` as a closed `Data.taggedEnum` (`JumpTo` instant, `EaseTo` animated, `FlyTo` curved, `FitBounds` extent-driven, `LookAt` eye/target — the 3D viewpoint carriage `mark`'s restore mints), and the fold pair: `Camera.drive(map, intent)` dispatches onto the maplibre `Camera` verbs, `Camera.settled(map)` reads the getters into a `State` — the `moveend` subscription writes it to the atom so the store always holds the authority's last settled truth.
- Packages: `maplibre-gl` (`jumpTo`/`easeTo`/`flyTo`/`fitBounds`/`calculateCameraOptionsFromTo`, the getters); `@rasm/ts/core` (`Wire.GeoFeature.Extent` as the bounds carriage); `effect` (`Data`, `pipe`).
- Boundary: the camera atom rides `system/atom#STORE_ROOT` and the gesture that mints an intent is `system/act#CONTINUOUS_OWNER`'s; this page owns the vocabulary and the folds, never the binding or the recognizer.
- Law: one authority per surface — under `MapboxOverlay` the map owns pan/zoom/pitch and deck's view state syncs automatically; hand-syncing deck's camera under an overlay is the named defect; a map-less free `Deck` drives `viewState` from the same atom.
- Law: intents are the only write path — a gesture (`system/act#CONTINUOUS_OWNER`), a viewpoint restore, and a fit-to-selection all mint `Camera.Intent` values on every surface class; nothing calls a map verb outside `Camera.drive`, so camera motion is replayable and undo is `system/atom#HISTORY_FOLD` over the camera atom by construction.
- Law: the gesture owner is an intent PRODUCER because this page supplies both halves of its read/write pair — the floor's `Gesture.useCanvas` takes `read: () => <the three gesture axes off Camera.settled>` and `emit: (reading) => Camera.gestured({ ...held, ...reading })`, so the recognizer speaks only its own three-axis `Gesture.Reading` while `pitch` and every other geo-owned axis rides through the settled state untouched, and `Camera.gestured` folds the merged `State` into `JumpTo` because a continuous drag or pinch is already at its destination and an eased arm would fight the pointer; the intent family and the camera shape stay this page's, so a gesture, a restore, and a control intent all reach `Camera.drive` through the same closed family and the replay journal cannot tell them apart.
- Law: intent payloads speak canonical shapes only — `FitBounds` carries the `Wire.GeoFeature.Extent` quadruple, never a maplibre bounds dialect; the maplibre arm alone respells the readonly quadruple into the map's mutable bounds at the drive boundary — the one boundary adaptation this fold carries.
- Law: `LookAt` grounds on the map through the map's own solve — `calculateCameraOptionsFromTo(eye, eyeAltitude, target, targetAltitude)` derives center, zoom, bearing, AND pitch in the map's camera model, the camera landing at the eye because zoom derives from the eye→target distance against metre altitudes; the arm spreads the solved options into `easeTo`, and a hand tangent-plane fold beside this member is the named reimplementation defect.
- Law: backend adapters translate, never own — the three arm folds control state into the atom on the `change` dispatch of the ONE control class the surface earns (`OrbitControls` for object inspection, `ArcballControls` for trackball-precision review, `MapControls` for plan-view pan-first navigation — one row each, `controls.target` follows a `LookAt` so orbit resumes around the looked-at point, position sets from `eye` through `Object3D.lookAt`); the model-viewer arm reads `getCameraOrbit()`/`getCameraTarget()` on `camera-change`, writes `cameraOrbit`/`cameraTarget`, and `jumpCameraToGoal()` settles — the element's own interpolation is respected, never fought per frame. Policy (bounds clamps, zoom limits) lives in the intent fold once, so every backend inherits it; `center` carries scene coordinates on non-geo surfaces under the same `State` shape.
- Law: the settled state publishes for cross-app taps — the camera atom is the one truth, and a non-atom consumer (a wire egress, a sibling app's probe) observes it through `Atom.toStream(camera)` — never a second `moveend` subscription and never a mirrored cell; per-app soundness holds because each app's registry scopes its own camera stream.
- Growth: a new motion kind (an orbit-around) is one intent case plus one dispatch arm per backend — consumers break loudly at the missing arm; a new control temperament is one adapter row, never a fourth camera vocabulary.

```typescript
import type { Wire } from "@rasm/ts/core"
import { Data, pipe } from "effect"
import type { Map as MapLibreMap } from "maplibre-gl"

declare namespace Camera {
  type State = {
    readonly center: readonly [number, number]
    readonly zoom: number
    readonly bearing: number
    readonly pitch: number
  }
  type Eye = readonly [number, number, number]
  type Intent = Data.TaggedEnum<{
    JumpTo: { readonly state: Partial<Camera.State> }
    EaseTo: { readonly state: Partial<Camera.State>; readonly millis: number }
    FlyTo: { readonly state: Partial<Camera.State>; readonly speed: number }
    FitBounds: { readonly bounds: Wire.GeoFeature.Extent; readonly padding: number }
    LookAt: { readonly eye: Camera.Eye; readonly target: Camera.Eye; readonly millis: number }
  }>
}

const _Intent = Data.taggedEnum<Camera.Intent>()

const _payload = (state: Partial<Camera.State>) => ({
  ...(state.center !== undefined && { center: [state.center[0], state.center[1]] satisfies [number, number] }),
  ...(state.zoom !== undefined && { zoom: state.zoom }),
  ...(state.bearing !== undefined && { bearing: state.bearing }),
  ...(state.pitch !== undefined && { pitch: state.pitch }),
})

const _drive = (map: MapLibreMap, intent: Camera.Intent): void =>
  _Intent.$match(intent, {
    JumpTo: ({ state }) => void map.jumpTo(_payload(state)),
    EaseTo: ({ state, millis }) => void map.easeTo({ ..._payload(state), duration: millis }),
    FlyTo: ({ state, speed }) => void map.flyTo({ ..._payload(state), speed }),
    FitBounds: ({ bounds, padding }) => void map.fitBounds([bounds[0], bounds[1], bounds[2], bounds[3]], { padding }),
    LookAt: ({ eye, millis, target }) =>
      void map.easeTo({
        ...map.calculateCameraOptionsFromTo([eye[0], eye[1]], eye[2], [target[0], target[1]], target[2]),
        duration: millis,
      }),
  })

const _settled = (map: MapLibreMap): Camera.State =>
  pipe(map.getCenter(), (center) => ({
    center: [center.lng, center.lat] as const,
    zoom: map.getZoom(),
    bearing: map.getBearing(),
    pitch: map.getPitch(),
  }))

const _gestured = (state: Camera.State): Camera.Intent => _Intent.JumpTo({ state }) // the gesture owner's write half: a live drag is already at its destination, so easing would fight the pointer
```

## [05]-[PROJECT]

[PROJECT]:
- Law: screen↔world is pure math — `map.project(lnglat)`/`map.unproject(point)` for live-surface reads; `WebMercatorViewport` (constructed from a `Camera.State` snapshot plus surface extent) for derived-atom anchor math — `project`/`unproject`/`fitBounds` on the immutable viewport compute pin positions and marquee extents with no live instance in the derivation; a projected point is a fixed 2-tuple, so the marked adapter asserts the bound rather than fabricating a fallback coordinate.
- Law: mercator crossings are turf rows the planar owner holds — `[08]-[PLANAR_OPS]`' projection pair converts whole geometries at the boundary where planar compute meets the geographic camera, and a hand-rolled projection formula anywhere is the named defect.
- Law: fit intents derive from geometry — `_fitFrom` folds `bbox` into `Camera.Intent.FitBounds` and centroid targets feed `EaseTo`; geometry-to-camera is a fold from decoded features to intent values. Tile-to-extent conversion happens here, never in `core`.
- Law: the bbox fold reads its own arity — `bbox` returns the 4-tuple or, over altitude-bearing coordinates, the 6-tuple `[west, south, minAltitude, east, north, maxAltitude]`, so the fold selects the horizontal quadruple by length and the wire `Extent` shape survives without a cast; a positional read that assumes four members fits a 3D feature to its altitude band.
- Law: a geometry-derived fit cannot express an antimeridian crossing — `bbox` is a coordinate extremum, so a feature straddling the seam flattens to a globe-wide box; a crossing survives only where the wire extent is fit material as-is (`west > east`, the wire's own law), and `cameraForBounds` adjusts the east limb by +360 before the camera solve. The two paths therefore stay distinct rows on one intent, never one derivation.
- Packages: `@deck.gl/core` (`WebMercatorViewport`); `@turf/turf` (`bbox`, the `AllGeoJSON` input union); `effect` (`pipe`).

```typescript
import { WebMercatorViewport } from "@deck.gl/core"
import { type AllGeoJSON, bbox } from "@turf/turf"

declare namespace Camera {
  type Extent = { readonly width: number; readonly height: number } // the surface extent the viewport solve needs; it rides the camera hub, never a second namespace with no owner
}

const _anchor = (state: Camera.State, extent: Camera.Extent, lnglat: readonly [number, number]): readonly [number, number] => {
  // BOUNDARY ADAPTER
  const projected = new WebMercatorViewport({
    longitude: state.center[0],
    latitude: state.center[1],
    zoom: state.zoom,
    bearing: state.bearing,
    pitch: state.pitch,
    width: extent.width,
    height: extent.height,
  }).project([lnglat[0], lnglat[1]]) as [number, number]
  return [projected[0], projected[1]] as const
}

const _fitFrom = (geometry: AllGeoJSON, padding: number): Camera.Intent =>
  pipe(bbox(geometry), (box) =>
    _Intent.FitBounds({
      // the tuple arity is the discriminant: the 6-form interleaves the altitude band, so the horizontal quadruple reads by position without a cast
      bounds: box.length === 6 ? ([box[0], box[1], box[3], box[4]] as const) : ([box[0], box[1], box[2], box[3]] as const),
      padding,
    }))

declare namespace Camera {
  type Shape = {
    readonly Intent: typeof _Intent
    readonly drive: typeof _drive
    readonly settled: typeof _settled
    readonly gestured: typeof _gestured
    readonly anchor: typeof _anchor
    readonly fitFrom: typeof _fitFrom
  }
}

const Camera: Camera.Shape = {
  Intent: _Intent,
  drive: _drive,
  settled: _settled,
  gestured: _gestured,
  anchor: _anchor,
  fitFrom: _fitFrom,
}
```

## [06]-[LAYER_ROWS]

[LAYER_ROWS]:
- Owner: `Geo.push` — the one imperative sink: the layer tree is an atom-derived `LayersList`, the compositing passes are an atom-derived effect array (deck layer and effect instances are alike declarative descriptors), and every change lands as ONE `overlay.setProps({ layers, effects })`; the overlay diffs and touches only what changed. Both arrays cross the same seam because both are pure values the fold mints — a second sink for the screen-space plane would diff against a different prior-prop snapshot and drop frames the layer sink kept. Two memoization planes stay orthogonal: react-compiler memoizes the tree, deck's `updateTriggers` memoizes GPU attributes — an accessor closing over an atom value names its `updateTriggers` key.
- Law: the effect array's element type derives from the sink itself — reading it off `setProps`' own parameter keeps the screen-space vocabulary honest against the pinned overlay and needs no second import of a deck type this page would otherwise alias against `effect`'s own `Effect`.
- Law: transport truth and payload truth are separate reasons — a tile, tileset, or scan the transport cannot deliver is `tile-unreachable`, retryable and system-blamed, so the resilient lookup's own `Schedule` re-drives it off the class column; a payload that arrived and would not decode is `frame-refused`, caller-blamed and quarantined, so a retry re-fetches identical bytes and the schedule must not touch it. One overloaded reason forces every fetch failure to answer the decode's non-retryable policy.
- Law: columnar geometry rides the GeoArrow fan — `data` per GeoArrow layer is ONE `RecordBatch`, so a chunked `Table` fans through `Table.batches` into per-batch layers; `initEarcutPool` hoists ONE pool shared by every polygon layer via `earcutWorkerPool`; picking returns the zero-copy row proxy `mark` resolves to `GlobalId`. Decoded GeoJSON features render through `GeoJsonLayer` — `pointType` and the fill/stroke/3D accessor sub-groups fan one feature stream to the whole mark vocabulary.
- Law: the decoded `Table` is a multi-surface bus — the SAME frame `Geo.decoded` mints feeds the GeoArrow layers here, the pivot engine, and the aligned-series projection at `view/chart#REGIME_LAW`; a second IPC decode of one frame is the named defect.
- Law: tile streaming is one engine with payload rows — `TileLayer.getTileData({ index, signal })` speaks the wire `Wire.GeoFeature.Tile` coordinate, the fetch rides the app's authed transport honoring the abort signal, and `renderSubLayers` projects each tile into ordinary rows bounded by the tile header's `boundingBox` (a proven `[[west,south],[east,north]]` pair — the marked adapter asserts the tuple); `MVTLayer` is the vector specialization (`binary: true`, cross-tile highlight by `uniqueIdProperty`), `TerrainLayer` reconstructs relief from an `elevationDecoder` row, `Tile3DLayer` streams 3D-tile hierarchies rendering mesh content through `scene#INSTANCED_ROWS`' pair, `PointCloudLayer` renders LAS scans, and `_WMSLayer` binds OGC image services — payload rows on one engine, never a second tiling machine; cache and throttle (`maxCacheByteSize`, `maxRequests`, `refinementStrategy`) are policy values.
- Law: tile fetches ride a resilient TTL cache above deck's byte cache — `Cache.make({ capacity, timeToLive, lookup })` fronts the authed transport so a pan-return re-renders from the decoded cache instead of re-fetching, retry/backoff policy composes on the lookup rail as one `Schedule` value, and deck's `maxCacheByteSize` remains the GPU-side budget — two caches, two altitudes, one lookup path; cache keys cross as `Data.struct` values so lookup identity is structural — a plain tile literal hashes referentially and turns the cache into a permanent miss.
- Law: the 3D-tile transport is a closed discriminant, never a knob — `_tiles3d`'s `transport` selects `Tiles3DLoader` for an open tileset href, `CesiumIonLoader` for an Ion asset (whose token rides the loader's OWN `cesium-ion` option bag, the asset identity staying in the href), and the `Tiles3DArchiveFileLoader`-then-`Tiles3DLoader` pair for a `.3tz` archive (the archive loader unwraps one member to bytes at its `3d-tiles-archive` `path` and the tileset loader parses them, so the archive case is two loaders on one row, never a second layer).
- Law: loaders arrive through the layer's `loaders` prop, never a host registry — `registerLoaders` mutates a process-global roster this library shares with every other owner on the page and ships deprecated in the pinned release, so a reusable surface passes its descriptor list per layer; the deprecated singular `loader` prop is likewise never spelled.
- Law: the up-axis reconciliation is a LOADER option, not a layer prop — `assetGltfUpAxis` lives inside each loader's own option bag (`3d-tiles` for the open and archive rows, `cesium-ion` for the Ion row) and reaches the layer only through `loadOptions`, its `"z"` value re-basing a Z-up tile payload onto the Y-up frame three samples at the `scene#INSTANCED_ROWS` handoff; the pinned default is `null`, so a row that omits it hands the mesh peer an unrotated payload and every instance lies on its side. The tileset's own `asset.gltfUpAxis` field is decoded evidence, never the input knob.
- Law: the DGGS cell family is ONE scheme-keyed table — `S2Layer.getS2Token`, `QuadkeyLayer.getQuadkey`, `GeohashLayer.getGeohash`, `A5Layer.getPentagon`, `H3ClusterLayer.getHexagons` specialize one `GeoCellLayer` pattern by index accessor, with `H3HexagonLayer` the high-precision GPU sibling; a new grid is one table row, and the GeoArrow cell mirrors take over when the index column is an Arrow batch.
- Law: the LAS descriptor is picked by point-record format, never by extension — `LASLoader` is the package's own alias for `LAZPerfLoader`, the sync-capable common path whose header declines LAS 1.4, and `LAZRsLoader` reads the 1.4 extended formats; the grade is therefore a COLUMN on the scan policy rather than a parameter beside it, because the format that picks the descriptor also picks the decimation and precision the same row carries, and a caller holding a policy holds the whole decision.
- Law: the LAS option bag is four policy rows under one `las` key — `skip` is the LOD decimation the zoom row supplies, `fp64` and `colorDepth` are precision rows, and `workerUrl` resolves through the `Glb.assetDir` roster (`scene#RESIDENCY_GRAFT`) because the shipped default fetches its worker from unpkg and every CSP the estate serves refuses it; an unset `workerUrl` is the named defect, not a default. `las.shape` is never spelled — the descriptor already fixes the output, and the option is inert.
- Law: the scan's Arrow egress lands the bus frame on the envelope's `data` member — `LASArrowLoader.parse` answers the loaders.gl `ArrowTable` `{ shape: "arrow-table", schema?, data }` whose `data` column IS the `apache-arrow` `Table` (a hard dependency of the schema package, never a structural stand-in), so `_scanned` projects `.data` and joins the `Geo.decoded` bus type with no cast; the loader parses on the main thread (`worker: false`), its spread-inherited `parseSync` is never spelled because it returns the mesh shape its name does not promise, and `LASLoader`'s declared `las.shape: "arrow-table"` option is DEAD in the shipped release — the conversion call is commented out, so the option route silently answers a mesh and is never spelled; `parse` itself takes no option bag — the `las` rows reach it through `load`, never a second argument.
- Law: the tile-content vocabulary is a bounded row table, never a render branch this page owns — `TILE3D_TYPE` discriminates a decoded payload across composite, point-cloud, batched, instanced, geometry, vector, and glTF, and each row names which peer surfaces it: the batched and instanced rows hand off at `scene#INSTANCED_ROWS`, the point-cloud row lands on the same binary attribute seam the LAS scan takes, and the composite row is a container whose members re-enter the table. Deck's own traversal picks the sublayer; this table is what `onTileLoad` reads to route evidence and appearance, so a hand-written render switch beside it restates the tileset engine.
- Law: motion is an animated row reading the ONE clock — `TripsLayer` binds `getTimestamps` against `currentTime` taken from `Clock.Frame.now` (`[03]-[FRAME_CLOCK]`) with `_animate` set on the overlay; `trailLength`/`fadeTrail` are the decay policy, and `scene#INSTANCED_ROWS`' `_animations` reads the same frame — one time coordinate across every animated surface, so a construction-sequence scrub moves trips and mixers together.
- Law: layer assembly admits through `Planar.admit` (`[08]`) — a `geographic` collection feeds a layer directly, a `projected` one crosses `toWgs84` exactly once at that boundary, and an SRID `Wire.GeoFeature.Crs.of` cannot resolve refuses `crs-unresolved` so the layer renders nothing and the refusal surfaces as evidence; per-feature projection inside an accessor is the named defect because an accessor re-runs the crossing every draw.
- Growth: a new payload format is one `getTileData`/`renderSubLayers` pair; a new 3D-tile transport is one discriminant case with its loader list; a new grid is one cell-table row; a new mark shape is one accessor sub-group on the owning row.

```typescript
import type { LayersList } from "@deck.gl/core"
import {
  A5Layer, GeohashLayer, H3ClusterLayer, H3HexagonLayer, MVTLayer, QuadkeyLayer, S2Layer,
  TerrainLayer, Tile3DLayer, TileLayer, TripsLayer, _WMSLayer,
} from "@deck.gl/geo-layers"
import { BitmapLayer, GeoJsonLayer, PointCloudLayer } from "@deck.gl/layers"
import { GeoArrowPolygonLayer, type initEarcutPool } from "@geoarrow/deck.gl-geoarrow"
import { CesiumIonLoader, TILE3D_TYPE, Tiles3DArchiveFileLoader, Tiles3DLoader } from "@loaders.gl/3d-tiles"
import { FetchError, type Loader, load } from "@loaders.gl/core"
import { LASArrowLoader, LASLoader, LAZRsLoader } from "@loaders.gl/las"
import { Fault, type Wire } from "@rasm/ts/core"
import { tableFromIPC, type RecordBatch, type Table } from "apache-arrow"
import { Array, Cache, Data, type Duration, Effect, Match, pipe, type Schedule, Schema } from "effect"
import type { FeatureCollection } from "geojson"

const _geoFamily = Fault.Class.family(
  ["frame-refused", "crs-unresolved", "style-unbound", "tile-unreachable"] as const,
  {
    "frame-refused": { class: "malformed" }, // arrived and would not decode: caller-blamed, quarantined, never re-driven
    "crs-unresolved": { class: "absent" },
    // a source, layer, or glyph the live style never declared: the write names a coordinate the style has no slot
    // for, so a retry against the same style answers identically and the refusal is the operator's evidence
    "style-unbound": { class: "absent" },
    "tile-unreachable": { class: "unavailable" }, // never arrived: system-blamed and retryable, so the lookup's Schedule re-drives it off the class column
  },
)

class GeoFault extends Schema.TaggedError<GeoFault>()("GeoFault", {
  reason: _geoFamily.schema,
  detail: Schema.String,
}) {
  get class(): Fault.Class.Kind {
    return _geoFamily.classOf(this.reason)
  }
  override get message(): string {
    return `<geo:${this.reason}> ${this.detail}`
  }
}

type _EarcutPool = Awaited<ReturnType<typeof initEarcutPool>>

const _decoded = (frame: Uint8Array): Effect.Effect<Table, GeoFault> =>
  Effect.try({
    try: () => tableFromIPC(frame),
    catch: (defect) => new GeoFault({ reason: "frame-refused", detail: String(defect) }),
  })

const _POINT = { radius: 4, units: "pixels" } as const

const _DECAY = { trail: 300, fade: true } as const

const _features = (id: string, collection: FeatureCollection): GeoJsonLayer =>
  new GeoJsonLayer({
    id,
    data: collection,
    pickable: true,
    stroked: true,
    filled: true,
    pointType: "circle",
    getPointRadius: _POINT.radius,
    pointRadiusUnits: _POINT.units,
  })

const _arrowFan = (id: string, table: Table, pool: _EarcutPool): LayersList =>
  Array.map(table.batches, (batch: RecordBatch, rank: number) =>
    new GeoArrowPolygonLayer({
      id: `${id}/${rank}`,
      data: batch,
      pickable: true,
      earcutWorkerPool: pool,
    }))

const _tileCache = (
  fetched: (tile: Wire.GeoFeature.Tile, signal?: AbortSignal) => Promise<ImageBitmap>,
  policy: { readonly capacity: number; readonly ttl: Duration.DurationInput; readonly retry: Schedule.Schedule<unknown, GeoFault> },
): Effect.Effect<Cache.Cache<Wire.GeoFeature.Tile, ImageBitmap, GeoFault>> =>
  Cache.make({
    capacity: policy.capacity,
    timeToLive: policy.ttl,
    lookup: (tile: Wire.GeoFeature.Tile) =>
      Effect.tryPromise({
        try: (signal) => fetched(tile, signal),
        catch: (defect) => new GeoFault({ reason: "tile-unreachable", detail: String(defect) }), // the fetch leg is transport truth: retryable and system-blamed, so the schedule re-drives it
      }).pipe(Effect.retry(policy.retry)),
  })

const _rasterTiles = (
  id: string,
  cache: Cache.Cache<Wire.GeoFeature.Tile, ImageBitmap, GeoFault>,
  run: <A>(effect: Effect.Effect<A, GeoFault>) => Promise<A>,
): TileLayer<ImageBitmap> =>
  new TileLayer<ImageBitmap>({
    id,
    getTileData: ({ index }) => run(cache.get(Data.struct({ zoom: index.z, x: index.x, y: index.y }))),
    renderSubLayers: (props) => {
      // BOUNDARY ADAPTER
      const box = props.tile.boundingBox as [[number, number], [number, number]]
      return new BitmapLayer({
        id: `${props.id}/frame`,
        image: props.data,
        bounds: [box[0][0], box[0][1], box[1][0], box[1][1]],
      })
    },
  })

const _vectorTiles = (id: string, template: string, idProperty: string): MVTLayer =>
  new MVTLayer({ id, data: template, binary: true, pickable: true, uniqueIdProperty: idProperty })

declare namespace Tiles3d {
  type Basis = "x" | "y" | "z" // the loader's own up-axis vocabulary; "z" is the re-basing every Z-up producer needs
  type Transport = Data.TaggedEnum<{
    Open: {}
    Ion: { readonly token: string }
    Archive: { readonly path: string }
  }>
  type Policy = { readonly transport: Tiles3d.Transport; readonly basis: Tiles3d.Basis }
  type Peer = keyof typeof _peers
}

const _Transport = Data.taggedEnum<Tiles3d.Transport>()

// the decoded-payload vocabulary keyed by the loader's own type constant: each row names the peer that surfaces it, so
// `onTileLoad` routes evidence and appearance off one table and no render switch restates deck's own traversal
const _peers = { mesh: "scene#INSTANCED_ROWS", points: "PointCloudLayer", container: "_peers", none: "<unsurfaced>" } as const

const _tileContent = {
  [TILE3D_TYPE.COMPOSITE]: "container",
  [TILE3D_TYPE.POINT_CLOUD]: "points",
  [TILE3D_TYPE.BATCHED_3D_MODEL]: "mesh",
  [TILE3D_TYPE.INSTANCED_3D_MODEL]: "mesh",
  [TILE3D_TYPE.GEOMETRY]: "mesh",
  [TILE3D_TYPE.VECTOR]: "none",
  [TILE3D_TYPE.GLTF]: "mesh",
} as const satisfies Record<(typeof TILE3D_TYPE)[keyof typeof TILE3D_TYPE], Tiles3d.Peer>

// the up-axis is a LOADER option keyed by each loader's own bag, never a layer prop; the pinned default is null, so an omitted row hands the mesh peer an unrotated payload
const _transports = {
  Open: (basis: Tiles3d.Basis) => ({ loaders: [Tiles3DLoader], loadOptions: { "3d-tiles": { assetGltfUpAxis: basis } } }),
  Ion: (basis: Tiles3d.Basis, token: string) => ({
    loaders: [CesiumIonLoader],
    loadOptions: { "cesium-ion": { accessToken: token, assetGltfUpAxis: basis } }, // the Ion bag is its own key; the asset identity stays in the href
  }),
  Archive: (basis: Tiles3d.Basis, path: string) => ({
    loaders: [Tiles3DArchiveFileLoader, Tiles3DLoader], // two loaders, one row: the archive unwraps one member to bytes, the tileset loader parses them
    loadOptions: { "3d-tiles-archive": { path }, "3d-tiles": { assetGltfUpAxis: basis } },
  }),
} as const satisfies { readonly [K in Tiles3d.Transport["_tag"]]: (basis: Tiles3d.Basis, ...rest: never) => unknown }

const _tiles3d = (id: string, source: string, policy: Tiles3d.Policy): Tile3DLayer =>
  new Tile3DLayer({
    id,
    data: source,
    pickable: true,
    ..._Transport.$match(policy.transport, {
      Open: () => _transports.Open(policy.basis),
      Ion: ({ token }) => _transports.Ion(policy.basis, token),
      Archive: ({ path }) => _transports.Archive(policy.basis, path),
    }),
  })

declare namespace Scan {
  type Grade = "common" | "extended" // point-record format, never a file extension
  // one policy value carries the whole scan decision: the grade that picks the descriptor sits beside the decimation and precision the same format governs
  type Policy = {
    readonly grade: Scan.Grade
    readonly skip: number
    readonly colorDepth: number
    readonly fp64: boolean
    readonly workerUrl: string
    readonly size: number
  }
}

const _scanDescriptors = { common: LASLoader, extended: LAZRsLoader } as const satisfies Record<Scan.Grade, Loader>

const _scanOptions = (policy: Scan.Policy) => ({
  // `shape` is never spelled: the descriptor already fixes the output and the option is inert
  las: { skip: policy.skip, colorDepth: policy.colorDepth, fp64: policy.fp64, workerUrl: policy.workerUrl },
})

const _scan = (id: string, href: string, policy: Scan.Policy): PointCloudLayer =>
  new PointCloudLayer({
    id,
    data: load(href, _scanDescriptors[policy.grade], _scanOptions(policy)),
    pickable: true,
    pointSize: policy.size,
    sizeUnits: "pixels",
    material: true,
  })

// the loaders.gl envelope: `data` IS the apache-arrow `Table` — arrow is a hard dependency of the schema package —
// and the `shape` discriminant goes unread here because the loader's own `dataType` already fixes it
type _ArrowEgress = Awaited<ReturnType<typeof LASArrowLoader.parse>>

// the fused fetch-and-decode call can fail on either truth, so the caught value is triaged rather than assigned one reason:
// the loaders.gl fetch failure is transport (retryable, system-blamed), every other defect is payload (quarantined)
const _scanFault: (defect: unknown) => GeoFault = pipe(
  Match.type<unknown>(),
  Match.when(Match.instanceOf(FetchError), (fault) => new GeoFault({ reason: "tile-unreachable", detail: fault.message })),
  Match.orElse((defect) => new GeoFault({ reason: "frame-refused", detail: String(defect) })),
)

const _scanned = (href: string, policy: Scan.Policy): Effect.Effect<Table, GeoFault> =>
  Effect.map(
    Effect.tryPromise({
      // the loader's OWN envelope projected at this seam: `parse` takes no option bag, so the las rows ride `load`
      try: (signal) => load(href, LASArrowLoader, { ..._scanOptions(policy), fetch: { signal } }),
      catch: _scanFault,
    }),
    (egress: _ArrowEgress) => egress.data,
  )

declare namespace Cell {
  type Scheme = keyof typeof _cells
  type Row<DataT> = { readonly id: string; readonly data: ReadonlyArray<DataT>; readonly index: (row: DataT) => string }
}

const _cells = {
  s2: <D>(row: Cell.Row<D>) => new S2Layer<D>({ id: row.id, data: row.data, getS2Token: row.index, pickable: true }),
  quadkey: <D>(row: Cell.Row<D>) => new QuadkeyLayer<D>({ id: row.id, data: row.data, getQuadkey: row.index, pickable: true }),
  geohash: <D>(row: Cell.Row<D>) => new GeohashLayer<D>({ id: row.id, data: row.data, getGeohash: row.index, pickable: true }),
  a5: <D>(row: Cell.Row<D>) => new A5Layer<D>({ id: row.id, data: row.data, getPentagon: row.index, pickable: true }),
  h3: <D>(row: Cell.Row<D>) => new H3HexagonLayer<D>({ id: row.id, data: row.data, getHexagon: row.index, pickable: true }),
  h3Cluster: <D>(row: Cell.Row<D> & { readonly indexes: (r: D) => ReadonlyArray<string> }) =>
    new H3ClusterLayer<D>({ id: row.id, data: row.data, getHexagons: (r) => [...row.indexes(r)], pickable: true }),
} as const

const _trips = (
  id: string,
  paths: ReadonlyArray<{ readonly path: ReadonlyArray<readonly [number, number]>; readonly stamps: ReadonlyArray<number> }>,
  now: number,
): TripsLayer =>
  new TripsLayer({
    id,
    data: paths,
    getPath: (row) => row.path.map((point): [number, number] => [point[0], point[1]]),
    getTimestamps: (row) => [...row.stamps],
    currentTime: now,
    trailLength: _DECAY.trail,
    fadeTrail: _DECAY.fade,
  })

const _imagery = (id: string, endpoint: string, layers: ReadonlyArray<string>): _WMSLayer =>
  new _WMSLayer({ id, data: endpoint, serviceType: "wms", layers: [...layers] })

// the effect element type derives from the sink's own parameter, so no deck `Effect` import collides with the rail's
type _Passes = NonNullable<Parameters<MapboxOverlay["setProps"]>[0]["effects"]>

const _push = (surface: Geo.Surface, layers: LayersList, effects: _Passes): void =>
  surface.overlay.setProps({ layers, effects })
```

## [07]-[EXTENSION_ROWS]

[EXTENSION_ROWS]:
- Owner: `Geo.extensions` — the per-layer capability roster: one `LayerExtension` instance per GPU capability joins any layer's `extensions` array — `DataFilterExtension` (time-window/range filtering through `filterRange` driven by the atom clock), `BrushingExtension` (`brushingRadius` pointer reveal), `PathStyleExtension` (`getDashArray`/`getOffset` dash and offset), `FillStyleExtension` (`getFillPattern` pattern fill), `CollisionFilterExtension` (label declutter by `getCollisionPriority`), `MaskExtension` (geofence keyed by `maskId` to a layer carrying `operation: "mask"`), `ClipExtension` (`clipBounds` rectangular clip), `_TerrainExtension` (`terrainDrawMode` drape onto the relief surface).
- Owner: `Geo.depth` — the screen-space half of the same injection concern: a `PostProcessEffect` wraps one shader pass and joins the surface's `effects` array, and eye-dome shading is the row a dense scan earns — a depth-derived darkening at every discontinuity, which is what makes an untextured cloud legible where flat point colour reads as noise.
- Packages: `@deck.gl/extensions` (the full roster); `@deck.gl/core` (`PostProcessEffect`).
- Law: the two injection planes are disjoint and neither substitutes — a `LayerExtension` injects a shader module INTO one layer's own draw and rides that layer's `extensions` array, while a `PostProcessEffect` runs a full-screen pass over the composited frame and rides the surface's `effects` array; both reach the GPU through the one `setProps` sink, so the sink carries both arrays and a screen-space capability spelled as an extension has no layer to attach to.
- Law: options versus props is the discriminant — constructor options are static shader-compilation switches set once at construction; the injected props are per-frame runtime values pushed through the same `setProps` sink, and an extension accessor closing over an atom value names its `updateTriggers` key exactly like a layer's own accessor.
- Law: a cross-layer capability is an extension instance in the array, NEVER a layer subclass or a forked prop; extensions stack — each owns a disjoint shader-module injection.
- Law: the pass module is viewer-authored and its CONSTRAINT derives from deck — the pinned `@luma.gl/shadertools` ships no post-process pass and `@luma.gl/effects` is unadmitted, so no shipped eye-dome exists to compose; the pass type therefore reads off `PostProcessEffect`'s own constructor rather than a direct luma import, which the deck substrate rule forbids, and the authored module is admitted through exactly the surface deck already publishes.
- Law: the module lands FOUR members — `name`, `fs`, `uniformTypes`, `passes` — on luma's `ShaderPass` shape: `passes` is the sub-pass array deck's pass builder maps UNGUARDED, so its absence throws at mount, and `uniformTypes` order matches the `layout(std140)` block declaration by the shadertools position contract. Deck SYNTHESIZES `main()` from the sub-pass flag — `filter: true` calls `<name>_filterColor_ext(vec4, vec2, vec2)`, `sampler: true` calls `<name>_sampleColor(sampler2D, vec2, vec2)` — while the declared `action` field goes unread; a module `vs` is ignored because the clip-space model owns the geometry, `texSrc` is template-declared and never redeclared in the module `fs`, runtime props arrive keyed under the module NAME merged over each sub-pass's static `uniforms`, and `screen` is a reserved name the auto-mounted screen-uniforms module already holds.
- Law: the pass samples the composited COLOUR frame alone — `texSrc` is the template's one bound sampler and no depth attachment reaches a screen pass — so the discontinuity measure is a luminance ring over composited colour, and a depth-buffer EDL is unspellable on this surface rather than a stronger variant this row declined.
- Law: the sampling ring is a device-pixel distance — the policy carries a CSS-pixel radius and the row scales it by the surface's pixel ratio, because an unscaled radius shades a retina surface at half its intended reach and reads as a weaker effect rather than a wrong one.
- Growth: a new per-layer capability is one factory row and every layer inherits it by concatenation; a new screen-space pass is one policy shape plus one row on the same sink.

```typescript
import { PostProcessEffect } from "@deck.gl/core"
import {
  BrushingExtension, ClipExtension, CollisionFilterExtension, DataFilterExtension,
  FillStyleExtension, MaskExtension, PathStyleExtension, _TerrainExtension,
} from "@deck.gl/extensions"

const _extensions = {
  filter: () => new DataFilterExtension({ filterSize: 1 }),
  brush: () => new BrushingExtension(),
  dash: () => new PathStyleExtension({ dash: true }),
  pattern: () => new FillStyleExtension({ pattern: true }),
  declutter: () => new CollisionFilterExtension(),
  mask: () => new MaskExtension(),
  clip: () => new ClipExtension(),
  drape: () => new _TerrainExtension(),
} as const

declare namespace Screen {
  type Pass = ConstructorParameters<typeof PostProcessEffect>[0] // deck's constructor IS the admission surface; the luma substrate is never imported directly
  type Depth = { readonly strength: number; readonly radius: number } // eye-dome: shading weight, and the neighbour sampling ring in CSS pixels
}

const _DEPTH: Screen.Depth = { strength: 1.2, radius: 2 }

// the authored pass: the sub-pass is `sampler` because the ring taps its own neighbours, so deck's generated
// main() calls `eyedome_sampleColor(texSrc, screen.texSize, coordinate)`; the std140 block order IS the
// uniformTypes order, the shade folds the luminance drop against the brightest ring neighbour — the colour-frame
// reading of the eye-dome discontinuity, since no depth attachment reaches a screen pass — and `radius` arrives
// already scaled to device pixels so the shader divides by texSize alone
const _EYEDOME = {
  name: "eyedome",
  fs: `\
layout(std140) uniform eyedomeUniforms {
  float strength;
  float radius;
} eyedome;

const vec2 eyedome_ring[4] = vec2[4](vec2(1.0, 0.0), vec2(-1.0, 0.0), vec2(0.0, 1.0), vec2(0.0, -1.0));

float eyedome_luma(sampler2D source, vec2 texCoord) {
  return dot(texture(source, texCoord).rgb, vec3(0.2126, 0.7152, 0.0722));
}

vec4 eyedome_sampleColor(sampler2D source, vec2 texSize, vec2 texCoord) {
  vec4 centre = texture(source, texCoord);
  float here = dot(centre.rgb, vec3(0.2126, 0.7152, 0.0722));
  float peak = here;
  for (int at = 0; at < 4; at++) {
    peak = max(peak, eyedome_luma(source, texCoord + eyedome_ring[at] * eyedome.radius / texSize));
  }
  float shade = exp(-eyedome.strength * max(peak - here, 0.0));
  return vec4(centre.rgb * shade, centre.a);
}`,
  uniformTypes: { strength: "f32", radius: "f32" },
  passes: [{ sampler: true }],
} as const satisfies Screen.Pass

const _depth = (policy: Screen.Depth, pixelRatio: number): PostProcessEffect<typeof _EYEDOME> =>
  new PostProcessEffect(_EYEDOME, { strength: policy.strength, radius: policy.radius * pixelRatio })
```

## [08]-[PLANAR_OPS]

[PLANAR_OPS]:
- Owner: `Geo.planar` — the turf algebra as bounded op-row tables plus the ops whose own signatures are the contract: `relation` is the DE-9IM predicate matrix (seven uniform `(a, b) => boolean` rows mirroring the NetTopologySuite relationship set), `overlay` is the boolean-overlay triple, `project` is the WGS84↔Mercator pair, and `derive`/`gauge` are the geometry-to-geometry and geometry-to-scalar ops each carrying its own option row. Every member is a pure synchronous turf function; the algebra holds no state, no effect, and no DOM.
- Packages: `@turf/turf` (`union`, `intersect`, `difference`, `buffer`, `simplify`, `convex`, `dissolve`, `bboxClip`, `mask`, `area`, `length`, `convertArea`, `centroid`, `truncate`, the `boolean*` predicate set, `coordEach`/`geomEach`/`featureEach` traversal, `getCoord`/`getGeom`/`getType` accessors, `toMercator`, `toWgs84`, the `AllGeoJSON`/`Units`/`AreaUnits` vocabularies); `@types/geojson` (the `Feature`/`FeatureCollection`/`Polygon`/`Geometry` value types turf itself imports); `@rasm/ts/core` (`Wire.GeoFeature.Crs` — the SRID row table the admission resolves against); `effect` (`Effect`, `Option`).
- Law: the overlay arms take ONE collection, never two features — the pinned release moved `union`/`intersect` onto a `FeatureCollection<Polygon | MultiPolygon>` input answering one feature or `null`, so the call shape is the collection and a two-argument sibling has no spelling; `difference` takes the same collection and the empty result is `null`, a real answer the caller folds rather than an error.
- Law: turf is the planar compute peer, render surfaces are the sink — derived polygons feed a `GeoJsonLayer` row or a `GeoJSONSource.setData`, and the hit-test rows (`booleanPointInPolygon`, `geojsonRbush`) are `mark`'s consumption of this same algebra under its own selection law.
- Law: planar ONLY — turf never re-derives a spatial relation the C# side owns as authority; the two meet at the WKB/GeoJSON wire behind `WkbParser`, and a relation computed on both sides that diverges is the cross-language drift defect.
- Law: the mercator crossing is a row on this owner, not a formula anywhere — `toMercator`/`toWgs84` convert whole geometries at the boundary where planar compute meets the geographic camera, so a hand-rolled projection constant is the named defect.
- Law: `Planar.admit` IS the `[06]` assembly law's realization and the ONE raiser `crs-unresolved` has — it resolves the wire SRID through the core `Wire.GeoFeature.Crs.of` row table, passes a `geographic` collection through untouched, crosses a `projected` one exactly once through `toWgs84`, and refuses an SRID the table cannot resolve; the crossing happens at this boundary and never inside a layer accessor, which would re-project every feature per draw.
- Law: traversal rides the substrate — `coordEach`/`geomEach`/`featureEach` folds and the `getCoord`/`getGeom` accessors replace every hand coordinate loop, so a coordinate walk this owner does not already carry is a substrate call and never a `for` over `coordinates`.
- Law: measurement egress is ONE unit policy, never a per-call option — `Planar.measure` answers area and span together under the `_MEASURE` row, because `area` is fixed at square metres by the package while `length` takes `{ units }`, so the two axes would otherwise disagree per site; `convertArea` is the area crossing, `{ units }` the span's, and a `lengthKm`/`lengthMi` sibling is the suffix family the bounded option deletes.
- Law: `truncate` is the re-encode gate — coordinate precision trims once before a derived feature crosses back to a wire or a source, so a buffer's float tail never inflates a payload nor forks a content key against the same geometry rounded elsewhere.
- Growth: a new predicate is one row on `relation`, a new overlay arm one row on `overlay`; an op whose signature is genuinely its own joins as an owner member rather than distorting a table's uniform contract.

```typescript
import {
  type AllGeoJSON, area, type AreaUnits, bboxClip, booleanContains, booleanCrosses, booleanDisjoint,
  booleanIntersects, booleanOverlap, booleanTouches, booleanWithin, buffer, centroid, convertArea, convex,
  coordEach, difference, dissolve, featureEach, geomEach, getCoord, getGeom, getType, intersect, length, mask,
  simplify, toMercator, toWgs84, truncate, union, type Units,
} from "@turf/turf"
import { Wire } from "@rasm/ts/core"
import { Effect, Option } from "effect"
import type { Feature, FeatureCollection, Geometry, MultiPolygon, Polygon } from "geojson"

declare namespace Planar {
  type Areal = FeatureCollection<Polygon | MultiPolygon>
  type Shaped = Feature<Polygon | MultiPolygon> | null
  type Subject = Feature | Geometry
  type Relation = keyof typeof _relation
  type Overlay = keyof typeof _overlay
  type Projection = keyof typeof _project
  type _Relations<T extends Record<Relation, (a: Planar.Subject, b: Planar.Subject) => boolean> = typeof _relation> = T // row guard: a non-uniform predicate fails here, never at a call site
  type _Overlays<T extends Record<Overlay, (areal: Planar.Areal) => Planar.Shaped> = typeof _overlay> = T
}

// the NTS relationship matrix, member for member: one uniform arity means a relation is selected as data and the
// cross-language pair is checkable by name — a divergence between this row and its C# twin is the drift defect
const _relation = {
  contains: booleanContains,
  within: booleanWithin,
  crosses: booleanCrosses,
  overlap: booleanOverlap,
  disjoint: booleanDisjoint,
  intersects: booleanIntersects,
  touches: booleanTouches,
} as const

// the boolean-overlay triple: collection in, one feature or null out — the empty overlay is an answer, never a fault
const _overlay = { union, intersect, difference } as const

const _project = { mercator: toMercator, wgs84: toWgs84 } as const

const _PRECISION = { coordinates: 2, precision: 7 } as const // the re-encode gate: planar precision trims once, so a derived feature cannot fork a content key against the same geometry rounded elsewhere

// the coordinate substrate every fold rides: a walk this table does not carry is a substrate call, never a loop
const _walk = { coords: coordEach, geoms: geomEach, features: featureEach } as const

// the flexible-input normalizers: turf takes a `Coord` union and these collapse it once, so no op re-probes shape
const _read = { coord: getCoord, geom: getGeom, kind: getType } as const

// ONE measurement policy: `area` is fixed at square metres by the package while `length` takes `{ units }`, so
// stating both axes here is what keeps a report's two numbers on one declared scale
const _MEASURE = { area: "hectares", span: "meters" } as const satisfies { area: AreaUnits; span: Units }

const _measured = (geojson: Feature | FeatureCollection): { readonly area: number; readonly span: number } => ({
  area: convertArea(area(geojson), "meters", _MEASURE.area), // the area crossing: the package answers m², the policy names the report's unit
  span: length(geojson, { units: _MEASURE.span }),
})

// the ONE raiser `crs-unresolved` has, and the `[06]` assembly law realized: the wire SRID resolves against the
// core row table, a projected collection crosses to WGS84 exactly once here, and an unresolvable SRID refuses —
// re-projecting inside a layer accessor would re-run this crossing per feature per draw
const _admitted = <T extends AllGeoJSON>(geojson: T, srid: number): Effect.Effect<T, GeoFault> =>
  Option.match(Wire.GeoFeature.Crs.of(srid), {
    onNone: () => Effect.fail(new GeoFault({ reason: "crs-unresolved", detail: `<srid:${srid}>` })),
    onSome: (crs) => Effect.succeed(crs.kind === "projected" ? toWgs84(geojson) : geojson),
  })

declare namespace Planar {
  type Shape = {
    readonly relation: typeof _relation
    readonly overlay: typeof _overlay
    readonly project: typeof _project
    readonly walk: typeof _walk
    readonly read: typeof _read
    readonly units: typeof _MEASURE
    readonly buffer: typeof buffer
    readonly simplify: typeof simplify
    readonly convex: typeof convex
    readonly dissolve: typeof dissolve
    readonly clip: typeof bboxClip
    readonly mask: typeof mask
    readonly measure: typeof _measured
    readonly centroid: typeof centroid
    readonly admit: typeof _admitted
    readonly trimmed: <T extends AllGeoJSON>(geojson: T) => T
  }
}

const _planar: Planar.Shape = {
  relation: _relation,
  overlay: _overlay,
  project: _project,
  walk: _walk,
  read: _read,
  units: _MEASURE,
  buffer,
  simplify,
  convex,
  dissolve,
  clip: bboxClip,
  mask,
  measure: _measured, // the two metric axes answer together on one declared scale; a bare `length` forward would leave its unit at the call site
  centroid,
  admit: _admitted,
  trimmed: (geojson) => truncate(geojson, _PRECISION),
}
```

## [09]-[STYLE_DATA]

[STYLE_DATA]:
- Owner: `Geo.style` — the assembled style sub-owner over the one live map: `layer` adds an `AddLayerObject` at its `beforeId` slot, `paint` and `layout` write one correlated property each, `filter` swaps a `FilterSpecification`, and `data` re-feeds a `GeoJSONSource`. The three property writes stay separate members rather than one input union because the package correlates each name to its own value space (`AllPaintProperties[K]` against `AllLayoutProperties[K]`) and a union payload erases exactly that correlation — the discriminant the entrypoint law asks for does not exist at the value.
- Packages: `maplibre-gl` (`addLayer`/`setPaintProperty`/`setLayoutProperty`/`setFilter`, the `AddLayerObject`/`FilterSpecification`/`AllPaintProperties`/`AllLayoutProperties`/`StyleImageMetadata` data types, `getSource`, `GeoJSONSource.setData`, `setFeatureState`/`removeFeatureState` with `FeatureIdentifier`, `addImage`/`hasImage`/`loadImage`/`addSprite`, `Marker.setLngLat`/`setPopup`/`addTo`/`remove`, `Popup.setDOMContent`); `effect` (`Array`, `Data`, `Effect`, `Option`, `Scope`); `geojson` (`GeoJSON` — the `setData` payload union).
- Law: basemap styling is `*Specification` data — `addLayer(AddLayerObject)`, `setPaintProperty`, `setLayoutProperty`, and `setFilter` consume expression data authored as values; style edits are live re-paints, never style-swap rebuilds, and no render code hand-evaluates an expression.
- Law: a write against a coordinate the live style never declared refuses as `style-unbound` — `getSource` answers `undefined` for an undeclared id and every property write is silently dropped for an unknown layer, so the source sink lifts the miss into the family rather than resolving a promise nothing will settle; the class is `absent`, so a retry against the same style answers identically and the refusal reads as operator evidence.
- Law: hover/select echo is feature-state and its INPUT is `mark`'s own diff — `Geo.echo` takes the `{ entered, left }` pair `mark#ECHO_ROWS`' `Selection.diff` already computes, stamps `setFeatureState(target, { selected: true })` per entered id, and `removeFeatureState(target, "selected")` per left id, so data-driven paint tracks the one selection atom with no source re-add and no second diff; the echo writes exactly the key it removes, because a whole-state clear would drop a hover or a review tint some other row owns.
- Law: symbol glyphs are registered material and registration is IDEMPOTENT — `Geo.glyphs` folds a closed `Glyph` family (`Image` over a fetched bitmap with its `pixelRatio`/`sdf` metadata, `Sprite` over a published sheet), guards each image on `hasImage` because a repeat `addImage` on a live id is refused upstream, and lifts a `loadImage` rejection as `tile-unreachable` since an unfetchable glyph is transport truth the schedule may re-drive; an inline data-URI glyph beside the registry is the named defect.
- Law: DOM anchors survive only for HTML-bearing overlays (`mark`'s pins) and the PAIR is one bracket — `Geo.pinned` acquires the `Marker` at its coordinate, binds an `Option`-carried `Popup` through `setPopup` so the marker owns its detail node's lifetime, and removes the marker on release; a popup bracketed apart from its marker outlives the anchor it belongs to, and a pin outliving the surface keeps a detached element and its listeners alive. GPU marks belong to deck rows, and the sanitize gate (`system/primitive#SANITIZE_GATE`) is what any wire-borne popup body crosses before it reaches a node — `setDOMContent` takes the gated node, never a raw HTML string.
- Growth: a new style write is one member on the same sub-owner; a new glyph shape is one `Glyph` case with its arm; a new echo key is one column on the state record the same pair writes and removes.

```typescript
import { Array, Data, Effect, Option, type Scope } from "effect"
import type { GeoJSON } from "geojson"
import { Marker, Popup } from "maplibre-gl"
import type {
  AddLayerObject, AllLayoutProperties, AllPaintProperties, FeatureIdentifier, FilterSpecification,
  GeoJSONSource, StyleImageMetadata,
} from "maplibre-gl"

declare namespace Style {
  // the property writes stay three correlated members: `AllPaintProperties[K]` and `AllLayoutProperties[K]` are
  // disjoint key spaces the package correlates per name, so a union payload would erase the very correlation
  type Shape = {
    readonly layer: (surface: Geo.Surface, layer: AddLayerObject, beforeId?: string) => Effect.Effect<void>
    readonly paint: <K extends keyof AllPaintProperties>(
      surface: Geo.Surface,
      layerId: string,
      name: K,
      value: AllPaintProperties[K],
    ) => Effect.Effect<void>
    readonly layout: <K extends keyof AllLayoutProperties>(
      surface: Geo.Surface,
      layerId: string,
      name: K,
      value: AllLayoutProperties[K],
    ) => Effect.Effect<void>
    readonly filter: (surface: Geo.Surface, layerId: string, filter: FilterSpecification) => Effect.Effect<void>
    readonly data: (surface: Geo.Surface, sourceId: string, payload: GeoJSON) => Effect.Effect<void, GeoFault>
  }
}

const _style: Style.Shape = {
  layer: (surface, layer, beforeId) => Effect.sync(() => void surface.map.addLayer(layer, beforeId)),
  paint: (surface, layerId, name, value) => Effect.sync(() => void surface.map.setPaintProperty(layerId, name, value)),
  layout: (surface, layerId, name, value) => Effect.sync(() => void surface.map.setLayoutProperty(layerId, name, value)),
  filter: (surface, layerId, filter) => Effect.sync(() => void surface.map.setFilter(layerId, filter)),
  data: (surface, sourceId, payload) =>
    // the undeclared source is the one refusal this sink can raise: `getSource` answers undefined and there is no
    // promise to await, so the miss lifts here rather than resolving into a write the style silently dropped
    Option.match(Option.fromNullable(surface.map.getSource<GeoJSONSource>(sourceId)), {
      onNone: () => Effect.fail(new GeoFault({ reason: "style-unbound", detail: `<source:${sourceId}>` })),
      onSome: (source) =>
        Effect.tryPromise({
          try: () => source.setData(payload),
          catch: (defect) => new GeoFault({ reason: "frame-refused", detail: String(defect) }),
        }),
    }),
}

// the selection echo consumes `mark`'s own diff pair: entered ids take the key, left ids give it back, and the
// removal names the SAME key the stamp wrote so a hover or a review tint riding another key survives untouched
const _SELECTED = "selected" as const

const _echo = (
  surface: Geo.Surface,
  target: Omit<FeatureIdentifier, "id">,
  diff: { readonly entered: ReadonlyArray<string>; readonly left: ReadonlyArray<string> },
): Effect.Effect<void> =>
  Effect.sync(() => {
    // BOUNDARY ADAPTER — the feature-state plane is an imperative host surface keyed by identifier
    Array.forEach(diff.entered, (id) => void surface.map.setFeatureState({ ...target, id }, { [_SELECTED]: true }))
    Array.forEach(diff.left, (id) => void surface.map.removeFeatureState({ ...target, id }, _SELECTED))
  })

declare namespace Glyph {
  type Row = Data.TaggedEnum<{
    Image: { readonly id: string; readonly href: string; readonly pixelRatio: number; readonly sdf: boolean }
    Sprite: { readonly id: string; readonly href: string }
  }>
}

const _Glyph = Data.taggedEnum<Glyph.Row>()

const _glyph = (surface: Geo.Surface, row: Glyph.Row): Effect.Effect<void, GeoFault> =>
  _Glyph.$match(row, {
    // a repeat `addImage` on a live id is refused upstream, so registration reads the registry before it writes
    Image: ({ href, id, pixelRatio, sdf }) =>
      surface.map.hasImage(id)
        ? Effect.void
        : Effect.flatMap(
            Effect.tryPromise({
              try: () => surface.map.loadImage(href),
              // an unfetchable glyph is transport truth, so the class column lets a schedule re-drive it
              catch: (defect) => new GeoFault({ reason: "tile-unreachable", detail: String(defect) }),
            }),
            (response) =>
              Effect.sync(() =>
                void surface.map.addImage(id, response.data, { pixelRatio, sdf } satisfies Partial<StyleImageMetadata>)),
          ),
    Sprite: ({ href, id }) => Effect.sync(() => void surface.map.addSprite(id, href)),
  })

const _glyphs = (surface: Geo.Surface, rows: ReadonlyArray<Glyph.Row>): Effect.Effect<void, GeoFault> =>
  Effect.forEach(rows, (row) => _glyph(surface, row), { discard: true })

// the DOM anchor is scope-bracketed like every other surface resource: a pin outliving its map keeps a detached
// element and its listeners alive, and `mark#ANCHOR_PINS` mints the pin VALUE while this row owns the element
const _pinned = (
  surface: Geo.Surface,
  lnglat: readonly [number, number],
  element: HTMLElement,
  // the detail node arrives already through `system/primitive`'s sanitize gate: this row takes a node, never HTML
  detail: Option.Option<Node>,
): Effect.Effect<Marker, never, Scope.Scope> =>
  Effect.acquireRelease(
    Effect.sync(() =>
      // BOUNDARY ADAPTER — the fluent DOM-overlay builder; binding the popup to the marker is what makes the pair one lifetime
      new Marker({ element })
        .setLngLat([lnglat[0], lnglat[1]])
        .setPopup(Option.getOrNull(Option.map(detail, (node) => new Popup().setDOMContent(node))))
        .addTo(surface.map)),
    (marker) => Effect.sync(() => void marker.remove()),
  )
```

```typescript
declare namespace Geo {
  type Shape = {
    readonly surface: typeof _surface
    readonly relief: typeof _relief
    readonly globe: typeof _globe
    readonly sky: typeof _sky
    readonly chrome: typeof _chrome
    readonly decoded: typeof _decoded
    readonly features: typeof _features
    readonly arrowFan: typeof _arrowFan
    readonly tileCache: typeof _tileCache
    readonly rasterTiles: typeof _rasterTiles
    readonly vectorTiles: typeof _vectorTiles
    readonly tiles3d: typeof _tiles3d
    readonly tileContent: typeof _tileContent
    readonly scan: typeof _scan
    readonly scanned: typeof _scanned
    readonly cells: typeof _cells
    readonly trips: typeof _trips
    readonly imagery: typeof _imagery
    readonly extensions: typeof _extensions
    readonly depth: typeof _depth
    readonly planar: typeof _planar
    readonly push: typeof _push
    readonly Chrome: typeof _Rail
    readonly Glyph: typeof _Glyph
    readonly style: typeof _style
    readonly echo: typeof _echo
    readonly glyphs: typeof _glyphs
    readonly pinned: typeof _pinned
  }
}

const Geo: Geo.Shape = {
  surface: _surface,
  relief: _relief,
  globe: _globe,
  sky: _sky,
  chrome: _chrome,
  decoded: _decoded,
  features: _features,
  arrowFan: _arrowFan,
  tileCache: _tileCache,
  rasterTiles: _rasterTiles,
  vectorTiles: _vectorTiles,
  tiles3d: _tiles3d,
  tileContent: _tileContent,
  scan: _scan,
  scanned: _scanned,
  cells: _cells,
  trips: _trips,
  imagery: _imagery,
  extensions: _extensions,
  depth: _depth,
  planar: _planar,
  push: _push,
  Chrome: _Rail,
  Glyph: _Glyph,
  style: _style,
  echo: _echo,
  glyphs: _glyphs,
  pinned: _pinned,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Camera, Clock, Geo, GeoFault, Grant, Position, PositionFault }
```

## [10]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

[LAS_ARROW_EGRESS]-[OPEN]: does the loaders.gl `ArrowTable` that `LASArrowLoader.parse` answers carry the `apache-arrow` `Table` on a member the bus can take, and under which key; read `@loaders.gl/schema`'s table-category declarations in the pinned tree.
[EYE_DOME_PASS]-[OPEN]: which `ShaderModule` fields a viewer-authored eye-dome `ShaderPass` fills for `PostProcessEffect` to mount it; read `@luma.gl/shadertools`' shader-module declaration in the pinned tree.
