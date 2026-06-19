# [UI_VIEW_TRANSITIONS]

The route-driven surface-lifecycle owner over the native View Transitions API, the React `<ViewTransition>` component, and the React `<Activity>` state-preserving binding. One `SurfaceTransition` `Data.TaggedEnum` discriminates the two lifecycle modes a route or tab swap drives — a `swap` capture transition and a `preserve` mounted-but-hidden surface — under one `$match` that mounts the matching React primitive, so a route change is one declarative, interruptible, GPU-composited transition with zero manual FLIP and a backgrounded heavy surface stays mounted with its atom subscriptions and GPU resources held while effects unmount. The `swap` case carries a reduced-motion gate as one payload field — a `matchMedia("(prefers-reduced-motion: reduce)")` read folded onto the `mountSurface` swap arm suppresses the `<ViewTransition>` enter/exit class through the `theming/theme-tokens.md#THEME_TOKENS` `tw-animate-css` `prefers-reduced-motion` collapse, never a JS animation controller — so a motion-suppressed user sees an instant swap while the GPU-composited transition rides for everyone else. The transition fold is one scoped resource the `platform` navigation pipeline wraps around the guard-admitted location commit; the `ui` side owns only the `<ViewTransition>`/`<Activity>` wrapper and the motion gate, declares the navigation-commit requirement, and never imports `platform`. The owner holds no domain state; transition triggers ride the `binding/atom-binding.md` `AtomBinding` route cell.

## [1]-[INDEX]

One cluster: `VIEW_TRANSITIONS` owns the `SurfaceTransition` lifecycle union and its single mount fold.

## [2]-[VIEW_TRANSITIONS]

- Owner: `SurfaceTransition`, the one `Data.TaggedEnum` over the two route-driven surface lifecycles — `swap` (a captured `<ViewTransition>` route/layout exchange) and `preserve` (a `<Activity mode>` mounted-but-hidden surface); and `mountSurface`, the total `$match` fold that mounts the matching React primitive. A route or layout change is one `swap` case; a backgrounded surface is one `preserve` case — both are arms of the one fold, never two sibling components.
- Cases: the `swap` case wraps a route or layout exchange in `<ViewTransition>` keyed by the route axis so the browser captures the before/after and composites an interruptible, GPU-driven transition with no manual FLIP and the enter/exit/update class is declarative; the gate folds a `matchMedia("(prefers-reduced-motion: reduce)")` read onto the swap arm so a motion-suppressed render passes no `<ViewTransition>` `name` (suppressing the capture) and lets the `theme-tokens.md#THEME_TOKENS` `tw-animate-css` `prefers-reduced-motion` keyframe collapse own the suppression declaratively — never a JS animation controller, never a hand-rolled frame loop. The `preserve` case keeps the `viewport/glb-viewport.md` GL context, the tabbed `observation/observation-routes.md` routes, and the background `cartography/geo-series-layer.md` layers mounted with `mode="hidden"` — preserving the atom subscriptions and GPU buffers while effects unmount and render defers — so a re-show is instant and the `acquireRelease` GL resource is held across visibility toggles rather than churned. `mountSurface` is total over the union under `Data.taggedEnum().$match`, so a new lifecycle mode without a mount arm is a compile error.
- Entry: a route change reads the route cell through the `binding/atom-binding.md` `AtomBinding` and dispatches one `SurfaceTransition` through `mountSurface`; the reduced-motion gate reads `prefersReducedMotion()` once at the swap arm; a surface backgrounds by the `preserve` case's `visible` payload toggled from the same route or tab cell; the non-reactive transition-callback logic is extracted through `useEffectEvent` so the effect dependency set stays minimal; the navigation-commit seam is the `platform` router's — the transition fold is one scoped resource the `platform` navigation pipeline wraps around the guard-admitted location commit, consumed by capability, and `ui/**` never imports `platform/**`.
- Packages: `react`, `react-dom`, `tw-animate-css`, `effect`.
- Growth: a new surface lifecycle mode lands as one `SurfaceTransition` case carrying its payload and one `mountSurface` arm, never a parallel component beside the union; a new transition class lands as one `swap`-payload field; a new motion-policy gate lands as one read folded onto the swap arm, never a sibling component; a new preserved surface lands as one `preserve` dispatch keyed by the route or tab cell, never a teardown/rebuild path.
- Boundary: a `RouteTransition`/`ActivitySurface` pair of sibling components beside the one `SurfaceTransition` union is the named defect the fold deletes; a manual FLIP or a hand-rolled animation-frame transition beside the `swap` case is the named defect — extended to motion suppression, a JS animation controller or imperative class toggle beside the declarative `tw-animate-css` `prefers-reduced-motion` layer is the named defect; a `ui`-side router beside the `platform` navigation pipeline is the named one-way-direction defect; a teardown/rebuild of a heavy surface on route or tab switch instead of the `preserve` case is the named defect; the gesture algebra is the `motion/gesture-algebra.md` owner, not this page, and a pointer handler here is the named defect.

```ts contract
import * as React from "react";
import { Data } from "effect";
// `ViewTransition`/`Activity` are the React 19.2 exotics absent from the branch `react.md`
// catalogue — admitted-on-precondition, named below as the target import, not yet verified.

type SurfaceTransition = Data.TaggedEnum<{
  readonly swap: { readonly route: string; readonly children: React.ReactNode; readonly reduceMotion: boolean };
  readonly preserve: { readonly visible: boolean; readonly children: React.ReactNode };
}>;
const SurfaceTransition = Data.taggedEnum<SurfaceTransition>();

// One `matchMedia` read for the reduced-motion policy; the `tw-animate-css`
// `prefers-reduced-motion` layer owns the actual keyframe collapse, so the gate only chooses
// whether the `<ViewTransition>` capture runs at all.
const prefersReducedMotion = (): boolean =>
  typeof window !== "undefined" && window.matchMedia("(prefers-reduced-motion: reduce)").matches;

const mountSurface = (transition: SurfaceTransition): React.ReactElement =>
  SurfaceTransition.$match(transition, {
    swap: ({ route, children, reduceMotion }) =>
      reduceMotion
        ? React.createElement(React.Fragment, null, children)
        : React.createElement(ViewTransition, { name: route }, children),
    preserve: ({ visible, children }) =>
      React.createElement(Activity, { mode: visible ? "visible" : "hidden" }, children),
  });
```

The `useEffectEvent<T extends Function>(callback)` non-reactive event hook is verified on the branch `react.md` catalogue (entrypoint row [18]) — the transition callback rides it so the effect dependency set stays minimal.

RESEARCH [VIEW_TRANSITION_ACTIVITY]: the React `<ViewTransition>` and `<Activity>` component exotics and their props (`name`/`mode`/`enter`/`exit`/`update`) and the `react-dom` `addTransitionType`/native `document.startViewTransition` binding are absent from the branch `react.md`/`react-dom.md` catalogues; the component symbols stay RESEARCH until the catalogue carries the `<Activity>`/`<ViewTransition>` rows — the React 19.2 exotics are admitted-on-precondition and the fence above names them as the target shape, not a verified import. The `mountSurface` `$match` fold and the `SurfaceTransition` union are the settled owner regardless of the final component import path.
