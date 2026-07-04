# [UI_PROJECT]

`viewer/geo/project.ts` is the camera and projection plane: one `CameraState` vocabulary spans every render backend — the maplibre `Camera` is the single authority under the interleaved overlay, the free `three` scene and the `<model-viewer>` element adapt through per-backend rows — and all camera motion flows one way: interaction writes intents into the camera atom, the atom drives the owning backend, the backend's settled state folds back on `moveend`. Screen↔world math is pure and derived-atom-safe (`project`/`unproject`, `WebMercatorViewport`, turf's `toMercator`/`toWgs84`), so overlay anchors and BCF pins compute without touching a live instance.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]       | [OWNS]                                                                        |
| :-----: | :-------------- | :------------------------------------------------------------------------------ |
|   [1]   | `CAMERA_FOLD`   | the `CameraState` vocabulary, the intent family, and the one-authority sync law  |
|   [2]   | `PROJECT_SEAM`  | pure screen↔world math — project/unproject, viewport fit, mercator crossings     |
|   [3]   | `BACKEND_ROWS`  | the per-backend adapters — maplibre drive, three frame, model-viewer orbit       |

## [2]-[CAMERA_FOLD]

- Owner: `Camera` — the camera vocabulary: `Camera.State` (center `[lng, lat]`, `zoom`, `bearing`, `pitch` — the shape both the maplibre getters and deck's `MapViewState` speak), the intent family `Camera.Intent` as a closed `Data.taggedEnum` (`JumpTo` instant, `EaseTo` animated, `FlyTo` curved, `FitBounds` extent-driven, `LookAt` eye/target — the 3D viewpoint carriage BCF restores and scene framings mint), and the fold pair: `Camera.drive(map, intent)` dispatches an intent onto the maplibre `Camera` verbs, `Camera.settled(map)` reads the getters into a `State` — the `moveend` subscription writes it to the atom so the store always holds the authority's last settled truth.
- Law: intent payloads speak canonical shapes only — `FitBounds` carries the wire `GeoFeature.Extent` quadruple, never a maplibre bounds dialect, so the closed family stays backend-agnostic and every backend arm respells at its own adapter; a foreign camera type inside the intent vocabulary is the named defect.
- Packages: `maplibre-gl` (`Camera` verbs — `jumpTo`/`easeTo`/`flyTo`/`fitBounds`/`calculateCameraOptionsFromTo`, getters), `#vocab` (`GeoFeature.Extent` as the bounds carriage), `effect` (`Data`, `Match`), `@effect-atom/atom-react` (the camera atom rides `atom/binding`'s store).
- Law: one authority per surface — under `MapboxOverlay` the map owns pan/zoom/pitch and deck's view state syncs automatically; hand-syncing deck's camera under an overlay is the named defect; the free-`Deck` surface (map-less) instead drives `viewState` from the same atom with `FlyToInterpolator`/`LinearInterpolator` as the transition rows.
- Law: intents are the only write path — a gesture (`act/gesture`'s `Gesture.useCanvas`), a BCF viewpoint restore, and a fit-to-selection all mint `Camera.Intent` values on every surface class; nothing calls a map verb outside `Camera.drive`, so camera motion is replayable and the undo stack (`History` over the camera atom) works by construction.
- Law: `LookAt` grounds on the map through the map's own solve — `calculateCameraOptionsFromTo(eye, eyeAltitude, target, targetAltitude)` derives center, zoom, bearing, AND pitch in the map's camera model, the camera landing at the eye because zoom derives from the eye→target distance against metre altitudes, and the transform's own pitch constraint clamps an eye below its target; the arm spreads the solved `CameraOptions` into `easeTo`, so the restore is exact on every surface class and a `LookAt` payload is consume-only viewpoint carriage, never re-derived camera truth — a hand tangent-plane fold beside this member is the named reimplementation defect.
- Growth: a new motion kind (an orbit-around) is one intent case plus one dispatch arm per backend — consumers break loudly at the missing arm.

```typescript
import type { GeoFeature } from "#vocab"
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
    FitBounds: { readonly bounds: GeoFeature.Extent; readonly padding: number }
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
    FitBounds: ({ bounds, padding }) => void map.fitBounds([bounds[0], bounds[1], bounds[2], bounds[3]], { padding }), // BOUNDARY ADAPTER: the readonly wire quadruple respells into the map's mutable bounds at the one maplibre arm
    LookAt: ({ eye, millis, target }) =>
      void map.easeTo({
        ...map.calculateCameraOptionsFromTo([eye[0], eye[1]], eye[2], [target[0], target[1]], target[2]), // the map's own solve: center, zoom, bearing, and pitch from eye→target — the camera lands AT the eye
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
```

## [3]-[PROJECT_SEAM]

- Law: screen↔world is pure math — `map.project(lnglat)`/`map.unproject(point)` for live-surface reads; `WebMercatorViewport` (constructed from a `Camera.State` snapshot plus surface extent) for derived-atom anchor math — `project`/`unproject`/`fitBounds` on the immutable viewport compute BCF pin positions and marquee extents with no live instance in the derivation.
- Law: mercator crossings are turf rows — `toMercator`/`toWgs84` convert whole geometries at the boundary where planar compute meets the geographic camera; a hand-rolled projection formula anywhere is the named defect.
- Law: fit intents derive from geometry — `bbox(featureOrCollection)` (turf) feeds `Camera.Intent.FitBounds`; centroid targets feed `EaseTo` — geometry-to-camera is a fold from decoded features to intent values.
- Law: the wire extent is fit material as-is — `Camera.Intent.FitBounds` carries the `GeoFeature.Extent` `[west, south, east, north]` tuple published through `wire` `#vocab`, so an extent-carrying payload mints the intent with zero adaptation; the maplibre arm alone respells the readonly quadruple into the map's mutable bounds at the drive boundary, an antimeridian crossing (`west > east`, the wire's own law) survives the fit because `cameraForBounds` applies `LngLatBounds.adjustAntiMeridian` — the east limb unwraps by +360 — before the camera solve, and tile-to-extent conversion happens here (`WebMercatorViewport`), never in `wire`.

```typescript
import { WebMercatorViewport } from "@deck.gl/core"

declare namespace Anchor {
  type Extent = { readonly width: number; readonly height: number }
}

const _anchor = (state: Camera.State, extent: Anchor.Extent, lnglat: readonly [number, number]): readonly [number, number] =>
  pipe(
    new WebMercatorViewport({
      longitude: state.center[0],
      latitude: state.center[1],
      zoom: state.zoom,
      bearing: state.bearing,
      pitch: state.pitch,
      width: extent.width,
      height: extent.height,
    }).project([lnglat[0], lnglat[1]]),
    (projected) => [projected[0] ?? 0, projected[1] ?? 0] as const,
  )
```

## [4]-[BACKEND_ROWS]

- Law: the three scene frames by bounds — `Box3.setFromObject(root)` measures the residency graph, and the framing row positions the `PerspectiveCamera` to enclose it (distance from the bounding sphere radius and the camera's fov); `OrbitControls` (constructed and disposed under the same scope as the renderer) owns interactive orbit, and its `change` events fold into the camera atom exactly like `moveend` — the vocabulary stays `Camera.State` with `center` carrying scene coordinates on non-geo surfaces.
- Law: the model-viewer element adapts through its triad — `getCameraOrbit(): SphericalPosition` and `getCameraTarget()` read on the `camera-change` event into the atom; writes stamp the `cameraOrbit`/`cameraTarget` attributes; `jumpCameraToGoal()` is the settle verb — the element's own interpolation is respected, never fought with per-frame writes.
- Law: adapters translate, never own — each backend row converts between its native camera speech and `Camera.State`/`Camera.Intent`; policy (bounds clamps, zoom limits) lives in the intent fold once, so every backend inherits it.
- Law: `LookAt` is native on the scene backends — the three arm sets the camera position from `eye` and aims through `Object3D.lookAt` at `target` (OrbitControls' `target` follows so orbit resumes around the looked-at point); the model-viewer arm stamps `cameraTarget` from `target` and `cameraOrbit` from the eye−target spherical offset, letting the element interpolate — so a BCF viewpoint restores exactly on every surface class: the scene consumes eye and target natively, the map solves them through its own camera model.
- Boundary: gesture recognition is `act/gesture`'s; the surfaces themselves are `viewer/geo/layers` (map) and `viewer/scene/glb` (scene/embed); BCF viewpoint camera carriage is `viewer/mark/bcf`'s consumption of `Camera.Intent`.

```typescript
const Camera: {
  readonly Intent: typeof _Intent
  readonly drive: typeof _drive
  readonly settled: typeof _settled
  readonly anchor: typeof _anchor
} = {
  Intent: _Intent,
  drive: _drive,
  settled: _settled,
  anchor: _anchor,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Camera }
```
