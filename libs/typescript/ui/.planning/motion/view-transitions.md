# [UI_VIEW_TRANSITIONS]

The route-driven surface-lifecycle owner over the native View Transitions API, the React `<ViewTransition>` component, and the React `<Activity>` state-preserving binding. One `SurfaceTransition` `Data.TaggedEnum` discriminates the two lifecycle modes a route or tab swap drives — a `swap` capture transition and a `preserve` mounted-but-hidden surface — under one `$match` that mounts the matching React primitive, so a route change is one declarative, interruptible, GPU-composited transition with zero manual FLIP and a backgrounded heavy surface stays mounted with its atom subscriptions and GPU resources held while effects unmount. The owner holds no domain state; transition triggers ride the `binding/atom-binding.md` `AtomBinding` route cell.

## [1]-[INDEX]

One cluster: `VIEW_TRANSITIONS` owns the `SurfaceTransition` lifecycle union and its single mount fold.

## [2]-[VIEW_TRANSITIONS]

- Owner: `SurfaceTransition`, the one `Data.TaggedEnum` over the two route-driven surface lifecycles — `swap` (a captured `<ViewTransition>` route/layout exchange) and `preserve` (a `<Activity mode>` mounted-but-hidden surface); and `mountSurface`, the total `$match` fold that mounts the matching React primitive. A route or layout change is one `swap` case; a backgrounded surface is one `preserve` case — both are arms of the one fold, never two sibling components.
- Cases: the `swap` case wraps a route or layout exchange in `<ViewTransition>` keyed by the route axis so the browser captures the before/after and composites an interruptible, GPU-driven transition with no manual FLIP and the enter/exit/update class is declarative; the `preserve` case keeps the `viewport/glb-viewport.md` GL context, the tabbed `observation/observation-routes.md` routes, and the background `cartography/geo-series-layer.md` layers mounted with `mode="hidden"` — preserving the atom subscriptions and GPU buffers while effects unmount and render defers — so a re-show is instant and the `acquireRelease` GL resource is held across visibility toggles rather than churned. `mountSurface` is total over the union under `Data.taggedEnum().$match`, so a new lifecycle mode without a mount arm is a compile error.
- Entry: a route change reads the route cell through the `binding/atom-binding.md` `AtomBinding` and dispatches one `SurfaceTransition` through `mountSurface`; a surface backgrounds by the `preserve` case's `visible` payload toggled from the same route or tab cell; the non-reactive transition-callback logic is extracted through `useEffectEvent` so the effect dependency set stays minimal.
- Packages: `react`, `react-dom`, `effect`.
- Growth: a new surface lifecycle mode lands as one `SurfaceTransition` case carrying its payload and one `mountSurface` arm, never a parallel component beside the union; a new transition class lands as one `swap`-payload field; a new preserved surface lands as one `preserve` dispatch keyed by the route or tab cell, never a teardown/rebuild path.
- Boundary: a `RouteTransition`/`ActivitySurface` pair of sibling components beside the one `SurfaceTransition` union is the named defect the fold deletes; a manual FLIP or a hand-rolled animation-frame transition beside the `swap` case is the named defect; a teardown/rebuild of a heavy surface on route or tab switch instead of the `preserve` case is the named defect; the gesture algebra is the `motion/gesture-algebra.md` owner, not this page, and a pointer handler here is the named defect.

```ts contract
type SurfaceTransition = Data.TaggedEnum<{
  readonly swap: { readonly route: string; readonly children: React.ReactNode };
  readonly preserve: { readonly visible: boolean; readonly children: React.ReactNode };
}>;
const SurfaceTransition = Data.taggedEnum<SurfaceTransition>();

const mountSurface = (transition: SurfaceTransition): React.ReactElement =>
  SurfaceTransition.$match(transition, {
    swap: ({ route, children }) => React.createElement(ViewTransition, { name: route }, children),
    preserve: ({ visible, children }) =>
      React.createElement(Activity, { mode: visible ? "visible" : "hidden" }, children),
  });
```

RESEARCH [VIEW_TRANSITION_ACTIVITY]: the React `<ViewTransition>` and `<Activity>` component import paths and props (`name`/`mode`/`enter`/`exit`/`update`), the `useEffectEvent` hook signature, and the native `document.startViewTransition` contract are unverified; the component props and the View Transitions API binding stay RESEARCH until the folder `.api/` catalogue carries the `<Activity>`/`<ViewTransition>`/`useEffectEvent` rows.
