# [UI_GESTURE_ALGEBRA]

The one pointer-gesture algebra shared by the viewport camera and any draggable surface. `GestureFold` folds drag/pinch/wheel pointer intents through the `@use-gesture/react` recognizer into one `CameraGesture` `Data.TaggedEnum`, and `applyGesture` folds an intent forward onto `CameraState` under one `Match.tagsExhaustive`. The viewport camera composes this fold rather than owning an orphaned per-surface pointer handler. The fold is pure; its result drives state through the `binding/atom-binding.md` `AtomBinding`.

## [1]-[INDEX]

One cluster: `GESTURE_ALGEBRA` owns the shared pointer-gesture fold and the camera-state fold.

## [2]-[GESTURE_ALGEBRA]

- Owner: `CameraGesture`, the one `Data.TaggedEnum` over the pointer intents (`orbit`/`pan`/`dolly`/`frame`); `applyGesture`, the total fold from an intent onto `CameraState`; and `GestureFold`, the `@use-gesture/react` recognizer binding that maps raw drag/pinch/wheel pointer events onto the `CameraGesture` tags. The algebra is shared, not per-surface.
- Cases: `GestureFold` maps a drag onto `orbit` or `pan` by modifier, a pinch or wheel onto `dolly`, and a frame action onto `frame`; `applyGesture` folds each tag onto `CameraState` total under `Match.tagsExhaustive` — `orbit` advances azimuth/elevation, `pan` shifts the target, `dolly` scales the distance with a floor, `frame` recomputes the camera from the mesh centroid and radius. A draggable surface composes the same `GestureFold` keyed to its own intent set.
- Entry: a surface binds the `GestureFold` recognizer to its pointer events; each recognized `CameraGesture` tag folds through `applyGesture` and the result drives the surface state through the `binding/atom-binding.md` `AtomBinding`, closing the gesture→state→binding loop; the viewport camera `RoleBehavior` row composes this fold by reference.
- Packages: `@use-gesture/react`, `effect`.
- Growth: a new pointer intent lands as one `CameraGesture` tag carrying its payload and one `applyGesture` arm; a new draggable surface composes the same `GestureFold`, never a parallel pointer-handler type.
- Boundary: a per-surface pointer handler beside the shared `GestureFold` is the named defect; the camera fold is owned here, not on the `viewport/glb-viewport.md` leaf, and a `CameraGesture`/`applyGesture` re-declaration on the viewport is the named defect; the fold is pure and a state mutation inside `applyGesture` is the named defect.

```ts contract
type CameraGesture = Data.TaggedEnum<{
  readonly orbit: { readonly dx: number; readonly dy: number };
  readonly pan: { readonly dx: number; readonly dy: number };
  readonly dolly: { readonly delta: number };
  readonly frame: { readonly bounds: MeshView };
}>;
const CameraGesture = Data.taggedEnum<CameraGesture>();

interface CameraState {
  readonly azimuth: number;
  readonly elevation: number;
  readonly distance: number;
  readonly target: readonly [number, number, number];
}

const centroidOf = (bounds: MeshView): readonly [number, number, number] =>
  Array.range(0, bounds.vertexCount - 1).pipe(
    Array.reduce([0, 0, 0] as const, (acc, i) => [
      acc[0] + bounds.vertices[i * 3],
      acc[1] + bounds.vertices[i * 3 + 1],
      acc[2] + bounds.vertices[i * 3 + 2],
    ] as const),
    (sum) => [sum[0] / bounds.vertexCount, sum[1] / bounds.vertexCount, sum[2] / bounds.vertexCount] as const,
  );

const radiusOf = (bounds: MeshView): number => {
  const c = centroidOf(bounds);
  return Array.range(0, bounds.vertexCount - 1).pipe(
    Array.reduce(0, (max, i) =>
      Math.max(max, Math.hypot(
        bounds.vertices[i * 3] - c[0],
        bounds.vertices[i * 3 + 1] - c[1],
        bounds.vertices[i * 3 + 2] - c[2],
      ))),
  );
};

const applyGesture = (state: CameraState, gesture: CameraGesture): CameraState =>
  Match.value(gesture).pipe(
    Match.tagsExhaustive({
      orbit: (g) => ({ ...state, azimuth: state.azimuth + g.dx, elevation: state.elevation + g.dy }),
      pan: (g) => ({ ...state, target: [state.target[0] + g.dx, state.target[1] + g.dy, state.target[2]] as const }),
      dolly: (g) => ({ ...state, distance: Math.max(0.01, state.distance * (1 + g.delta)) }),
      frame: (g) => ({ azimuth: 0, elevation: 0.6, distance: radiusOf(g.bounds) * 2.5, target: centroidOf(g.bounds) }),
    }),
  );
```

RESEARCH [USE_GESTURE]: the `@use-gesture/react` `useGesture`/`useDrag`/`usePinch`/`useWheel` recognizer hooks and the gesture-state shape are unverified; the recognizer-binding member spellings stay RESEARCH until the folder `.api/` catalogue carries the `@use-gesture/react` rows.
