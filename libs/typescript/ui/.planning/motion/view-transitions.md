# [UI_VIEW_TRANSITIONS]

The route and state transition owner over the native View Transitions API and the React `<ViewTransition>` component, plus the React `<Activity>` state-preserving hidden-surface binding. A route or layout change is one declarative, interruptible, GPU-composited transition with zero manual FLIP code; a backgrounded heavy surface stays mounted-but-hidden through `<Activity>`, preserving atom subscriptions and GPU resources while effects unmount. The owner holds no domain state; transition triggers ride the `binding/atom-binding.md` `AtomBinding` route cell.

## [1]-[INDEX]

One cluster: `VIEW_TRANSITIONS` owns the route-transition binding and the activity-preserved surface.

## [2]-[VIEW_TRANSITIONS]

- Owner: `RouteTransition`, the React `<ViewTransition>` binding over the native browser View Transitions API; and `ActivitySurface`, the React `<Activity>` binding that keeps a state-bearing surface mounted-but-hidden. A route or layout change is one declarative transition; a backgrounded surface is one `<Activity mode>` toggle.
- Cases: `RouteTransition` wraps a route or layout swap in `<ViewTransition>` so the browser captures the before/after and composites an interruptible, GPU-driven transition with no manual FLIP; the transition class is keyed by the route axis so enter/exit/update animations are declarative. `ActivitySurface` keeps the `viewport/glb-viewport.md` GL context, the tabbed `observation/observation-routes.md` routes, and the background `cartography/geo-series-layer.md` layers mounted with `mode="hidden"` — preserving the atom subscriptions and GPU buffers while effects unmount and render defers — so a re-show is instant and the `acquireRelease` GL resource is held across visibility toggles rather than churned.
- Entry: a route change reads the route cell through the `binding/atom-binding.md` `AtomBinding` and the swap is wrapped in `<ViewTransition>`; a surface backgrounds by toggling its `<Activity>` mode from the same route or tab cell; the non-reactive transition-callback logic is extracted through `useEffectEvent` so the effect dependency set stays minimal.
- Packages: `react`, `react-dom`, `effect`.
- Growth: a new transition kind lands as one `<ViewTransition>` class keyed by the route axis; a new preserved surface lands as one `<Activity>` toggle keyed by the route or tab cell, never a teardown/rebuild path.
- Boundary: a manual FLIP or a hand-rolled animation-frame transition beside `<ViewTransition>` is the named defect; a teardown/rebuild of a heavy surface on route or tab switch instead of an `<Activity>` toggle is the named defect; the gesture algebra is the `motion/gesture-algebra.md` owner, not this page, and a pointer handler here is the named defect.

```ts contract
const RouteTransition: React.FC<{
  readonly route: string;
  readonly children: React.ReactNode;
}> = ({ route, children }) =>
  React.createElement(ViewTransition, { name: route }, children);

const ActivitySurface: React.FC<{
  readonly visible: boolean;
  readonly children: React.ReactNode;
}> = ({ visible, children }) =>
  React.createElement(Activity, { mode: visible ? "visible" : "hidden" }, children);
```

RESEARCH [VIEW_TRANSITION_ACTIVITY]: the React `<ViewTransition>` and `<Activity>` component import paths and props (`name`/`mode`/`enter`/`exit`/`update`), the `useEffectEvent` hook signature, and the native `document.startViewTransition` contract are unverified; the component props and the View Transitions API binding stay RESEARCH until the folder `.api/` catalogue carries the `<Activity>`/`<ViewTransition>`/`useEffectEvent` rows.
