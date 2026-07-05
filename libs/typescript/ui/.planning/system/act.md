# [UI_ACT]

The one motion-and-interaction owner: `react-aria` owns every DISCRETE accessible interaction — press, hover-intent, focus, keyboard, cross-input-normalized events, focus scoping — `@use-gesture/react` owns every CONTINUOUS analog gesture — drag deltas, pinch scale/rotate, wheel zoom, swipe momentum — the `Motion` vocabulary owns element enter/exit motion as named class-row compositions over `tw-animate-css`'s one keyframe mechanism, and `Transition` owns document-level motion over the native View Transitions API with a total degrade chain to bare `flushSync`. Four planes, one page, so motion authority never fractures: a discrete press routed through a pointer handler, a raw DOM listener where a hook exists, a second gesture hook stacked on one element, a bespoke `@keyframes` an axis row expresses, or a per-element JS lifecycle where an `entering:`/`exiting:` variant suffices — each is the named defect. The module is `ui/src/system/act.ts`.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]          | [OWNS]                                                                              | [PUBLIC]     |
| :-----: | :----------------- | :------------------------------------------------------------------------------------ | :----------- |
|  [01]   | `CLASS_DIVISION`   | the discrete/continuous ownership law and the composition rules on shared elements     | —            |
|  [02]   | `DISCRETE_ROWS`    | the react-aria interaction/focus hook composition every view row binds through         | `Gesture`    |
|  [03]   | `CONTINUOUS_OWNER` | the tree-shaken camera/free-drag recognizer factory over `@use-gesture`                | `Gesture`    |
|  [04]   | `MOTION_ROWS`      | the named enter/exit composition vocabulary over the tw-animate axis mechanism         | `Motion`     |
|  [05]   | `DOCUMENT_RAIL`    | the `startViewTransition` + `flushSync` rail, the `<Activity>` row, the gated upgrade  | `Transition` |

## [2]-[CLASS_DIVISION]

[CLASS_DIVISION]:
- Law: react-aria owns discrete — `usePress`/`useHover`/`useLongPress`/`useKeyboard`/`useMove` emit `PressEvent`/`HoverEvent`/`MoveEvent` normalized across mouse, touch, pen, keyboard, and virtual cursors; this page never binds a raw `onClick`/`onPointerDown` where a hook covers the interaction, and `on*` prop TYPES stay the `@types/react` `EventHandler` aliases while behavior sources from the hooks.
- Law: `@use-gesture` owns continuous — cumulative `offset`, per-gesture `movement`, `velocity`/`direction`/`swipe` classification; a drag delta computed from raw pointer events or a wheel-zoom hand-rolled from `onWheel` restates the recognizer.
- Law: third-party drag surfaces keep their own physics — `vaul` sheets drag through vaul (`view/overlay`), RAC collections drag through `useDragAndDrop` (`system/primitive`'s roster); layering `useDrag` over either is the double-bind defect.
- Law: composition on one element is `mergeProps` — a draggable that is also keyboard-operable spreads the react-aria bundle and the gesture `bind()` through one `mergeProps` fold; handler chains, ids, and aria attributes merge, and declaration-order spreading is rejected.
- Boundary: which widget owns which state is `system/primitive`'s spine; the camera atoms a gesture writes are the viewer projection plane's.

## [3]-[DISCRETE_ROWS]

[DISCRETE_ROWS]:
- Owner: `Gesture.useDiscrete(options)` — the composed discrete bundle: `usePress` + `useHover` + `useKeyboard` + `useFocusRing` merged through one `mergeProps` fold into a single spreadable prop record plus the state flags a recipe styles; the `use` prefix is load-bearing — the member composes hooks, so rules-of-hooks and the compiler's inference both key on it.
- Packages: `react-aria` (`usePress`, `useHover`, `useKeyboard`, `useFocusRing`, `mergeProps`); the bundle's prop-record type derives from `usePress`'s own return (`ReturnType<typeof usePress>["pressProps"]`) — the react-aria barrel re-exports no attribute base, so a `@react-types/shared` import is the unadmitted-package defect.
- Law: focus is scoped, never managed by hand — `FocusScope` traps/restores for overlays, `useFocusRing` styles keyboard-only focus (its `isFocusVisible` reaches CSS as the `focus-visible:` variant), `useFocusWithin` tracks containment, and `useFocusManager` walks programmatically; a `tabindex` ladder or a `document.activeElement` read in a row marks a missing hook.
- Law: hover carries intent — `useHover` suppresses touch-emulated hover and pairs with floating-ui's `safePolygon` only at the overlay seam (`view/overlay`); `useInteractOutside` owns outside-press dismissal where a full overlay stack is not mounted.
- Law: every bundle spreads through `mergeProps`, refs reconcile through `useObjectRef`/`mergeRefs`, and `useId` supplies SSR-stable identity — three mechanisms, no local variants.

```typescript
import { mergeProps, useFocusRing, useHover, useKeyboard, usePress } from "react-aria"

const _useDiscrete = (options: Gesture.DiscreteOptions): Gesture.DiscreteBundle => {
  const disabled = options.disabled ?? false
  const press = usePress({ isDisabled: disabled, onPress: (event) => options.onPress?.(event.pointerType) })
  const hover = useHover({ isDisabled: disabled, onHoverChange: (hovering) => options.onHoverChange?.(hovering) })
  const keyboard = useKeyboard({ onKeyDown: (event) => options.onKey?.(event.key) })
  const ring = useFocusRing()
  return {
    props: mergeProps(press.pressProps, hover.hoverProps, keyboard.keyboardProps, ring.focusProps),
    focusVisible: ring.isFocusVisible,
    pressed: press.isPressed,
    hovered: hover.isHovered,
  }
}
```

## [4]-[CONTINUOUS_OWNER]

[CONTINUOUS_OWNER]:
- Owner: `Gesture.useCanvas(options)` — the continuous recognizer factory: composes ONE tree-shaken `useGesture` variant (`createUseGesture([dragAction, pinchAction, wheelAction])` — only the engines the viewer uses bundle) bound imperatively to the canvas ref with `eventOptions: { passive: false }` and `preventDefault: true`, the ONLY binding that can block page scroll on wheel/touch; drag pans, pinch zooms-and-rotates around `origin`, wheel zooms with `pinchOnWheel` folding ctrl+wheel into the pinch engine.
- Packages: `@use-gesture/react` (`createUseGesture`, `dragAction`/`pinchAction`/`wheelAction`, the config/state algebra); `react` (`startTransition`).
- Entry: one `Gesture.useCanvas` per interactive surface — a new gesture on that surface is a handler key or sub-config on the same call, never a second hook on the element.
- Law: start state rides `memo`, origin rides `from` — the handler captures the origin on `first`, applies `movement` against it, and returns the memo; `from: () => read(camera)` binds the offset origin to the live atom so consecutive gestures accumulate; an external mutable ref for gesture accumulation is the named defect.
- Law: the wheel arm integrates per-event `delta` — wheel `offset` accumulates for the surface lifetime with no `from`-bound origin, so offset math against the live atom double-integrates every event; `delta` applies each tick exactly once against the current read.
- Law: the handler stays in domain coordinates — `transform` maps the raw screen `Vector2` into world/canvas space before the handler sees `movement`/`offset`; `bounds` + `rubberband` clamp with elastic overflow; `axis: "lock"` locks the dominant axis past `threshold`.
- Law: one bounds policy clamps every zoom write path — the pinch engine clamps through `scaleBounds`, the wheel arm clamps against the SAME policy row before the write; a zoom path escaping the bounds is the named defect.
- Law: high-frequency writes commit non-urgently — the camera atom write wraps in `startTransition` so the pointer stream stays responsive while the non-urgent camera commit deprioritizes; the write itself is `useAtomSet(camera)` with `"value"` mode.
- Boundary: the camera state shape and its per-backend adapters are the viewer projection plane's; `Gesture.useCanvas` only recognizes and writes intents.
- Growth: a new recognizer class (a two-finger rotate row, a keyboard-displacement drag) is one handler key plus its sub-config; a new surface is one `Gesture.useCanvas` call.

```typescript
import { createUseGesture, dragAction, pinchAction, wheelAction } from "@use-gesture/react"
import type { Vector2 } from "@use-gesture/react"
import { startTransition } from "react"
import type { RefObject } from "react"

const _ZOOM = { min: 0.1, max: 64 } as const

const _useCanvasGesture = createUseGesture([dragAction, pinchAction, wheelAction])

declare namespace Gesture {
  type DiscreteOptions = {
    readonly onPress?: (kind: "keyboard" | "mouse" | "pen" | "touch" | "virtual") => void
    readonly onHoverChange?: (hovering: boolean) => void
    readonly onKey?: (key: string) => void
    readonly disabled?: boolean
  }
  type DiscreteBundle = { readonly props: ReturnType<typeof usePress>["pressProps"]; readonly focusVisible: boolean; readonly pressed: boolean; readonly hovered: boolean }
  type Camera = { readonly center: Vector2; readonly zoom: number; readonly bearing: number }
  type CanvasOptions = {
    readonly target: RefObject<HTMLElement | null>
    readonly read: () => Camera
    readonly write: (camera: Camera) => void
    readonly transform?: (screen: Vector2) => Vector2
    readonly zoomBounds?: { readonly min: number; readonly max: number }
  }
}

const Gesture: {
  readonly useDiscrete: typeof _useDiscrete
  readonly useCanvas: (options: Gesture.CanvasOptions) => void
} = {
  useDiscrete: _useDiscrete,
  useCanvas: (options) => {
    const bounds = options.zoomBounds ?? _ZOOM
    _useCanvasGesture(
      {
        onDrag: ({ offset: [x, y] }) =>
          startTransition(() => options.write({ ...options.read(), center: [x, y] })),
        onPinch: ({ offset: [scale, angle] }) =>
          startTransition(() => options.write({ ...options.read(), zoom: scale, bearing: angle })),
        onWheel: ({ delta: [, dy] }) =>
          startTransition(() =>
            options.write({
              ...options.read(),
              zoom: Math.min(bounds.max, Math.max(bounds.min, options.read().zoom - dy / 500)),
            })),
      },
      {
        target: options.target,
        eventOptions: { passive: false },
        ...(options.transform !== undefined && { transform: options.transform }),
        drag: { from: () => options.read().center, preventDefault: true, filterTaps: true },
        pinch: { from: () => [options.read().zoom, options.read().bearing], scaleBounds: bounds, pinchOnWheel: true },
        wheel: { preventDefault: true },
      },
    )
  },
}
```

## [5]-[MOTION_ROWS]

[MOTION_ROWS]:
- Owner: `Motion` — the named motion vocabulary: one `as const satisfies Record<string, Motion.Row>` table whose rows compose `tw-animate-css`'s single mechanism — `animate-in`/`animate-out` trigger + axis setters (`fade-*`, `zoom-*`, `slide-*`, `blur-*`, `spin-*`) + timing modifiers (`animation-duration-*`, `delay-*`, `fill-mode-*`) — into enter/exit class pairs keyed by surface concept (`overlay`, `sheet`, `palette`, `toast`, `panel`). Every row leads with `motion-reduce:animate-none` so reduced motion is a construction fact.
- Packages: `tw-animate-css` (imported once in the token stylesheet as `@import "tw-animate-css";` after the tailwind entry — pure CSS, zero runtime); `tailwindcss` core `motion-reduce:` variant.
- Law: a motion is trigger plus at least one axis setter — a bare `animate-in` animates nothing; the row table makes the pairing structural because every row string carries both.
- Law: never author a `@keyframes` for an enter/exit effect the six axes express; the named component animations (`animate-accordion-down/up`, `animate-collapsible-down/up`, `animate-caret-blink`) are the only sanctioned self-contained keyframes and ride rows here, not bespoke CSS.
- Law: the RAC transition phases bind these rows through variants — `entering:` and `exiting:` (the `tailwindcss-react-aria-components` mappings of `data-entering`/`data-exiting`) scope the enter/exit halves, so overlay motion is one `cn(Motion.overlay.enter, Motion.overlay.exit)` class string with zero JS lifecycle code.
- Law: the row strings participate in `cn` conflict resolution — the motion class groups are taught to the one merge instance at `system/token#CLASS_RAIL`, so a caller override of `delay-*` or `fade-in-*` wins deterministically.
- Boundary: floating-ui `useTransitionStyles` phases consume `Scale.ease` values where an overlay needs style-object motion (`view/overlay`); the sheet's drag physics are `vaul`'s own and take no Motion row.
- Growth: a new surface motion is one row composing existing setters; a new axis is upstream (`tw-animate-css`), never a local keyframe.

```typescript
const _kinds = ["overlay", "sheet", "palette", "toast", "panel"] as const

declare namespace Motion {
  type Row = { readonly enter: string; readonly exit: string }
  type Kind = (typeof _kinds)[number]
}

const _rows = {
  overlay: {
    enter: "motion-reduce:animate-none entering:animate-in entering:fade-in-0 entering:zoom-in-95 entering:animation-duration-150",
    exit: "motion-reduce:animate-none exiting:animate-out exiting:fade-out-0 exiting:zoom-out-95 exiting:animation-duration-100",
  },
  sheet: {
    enter: "motion-reduce:animate-none entering:animate-in entering:slide-in-from-bottom entering:animation-duration-300",
    exit: "motion-reduce:animate-none exiting:animate-out exiting:slide-out-to-bottom exiting:animation-duration-200",
  },
  palette: {
    enter: "motion-reduce:animate-none entering:animate-in entering:fade-in-0 entering:slide-in-from-top-2 entering:animation-duration-150",
    exit: "motion-reduce:animate-none exiting:animate-out exiting:fade-out-0 exiting:animation-duration-100",
  },
  toast: {
    enter: "motion-reduce:animate-none entering:animate-in entering:slide-in-from-right entering:fade-in-0 entering:animation-duration-200",
    exit: "motion-reduce:animate-none exiting:animate-out exiting:slide-out-to-right exiting:fade-out-0 exiting:animation-duration-150",
  },
  panel: {
    enter: "motion-reduce:animate-none entering:animate-in entering:fade-in-0 entering:animation-duration-100",
    exit: "motion-reduce:animate-none exiting:animate-out exiting:fade-out-0 exiting:animation-duration-100",
  },
} as const satisfies Record<Motion.Kind, Motion.Row>

const Motion: typeof _rows & { readonly kinds: typeof _kinds } = {
  ..._rows,
  kinds: _kinds,
}
```

## [6]-[DOCUMENT_RAIL]

[DOCUMENT_RAIL]:
- Owner: `Transition` — one entrypoint: `Transition.run(commit, options?)` gates on capability (`document.startViewTransition` present) and motion budget (`prefers-reduced-motion`), wraps the state commit in `flushSync` inside the transition callback so the DOM the browser snapshots is the post-commit DOM, and returns the transition's `finished` promise lifted onto the rail; absent the API or under reduced motion the commit runs bare — the caller never branches.
- Packages: `react-dom` (`flushSync` — the synchronous commit the snapshot requires); the platform View Transitions API (lib.dom); `effect` (`Effect.tryPromise` lifting `finished`).
- Entry: route changes, panel-set swaps, theme flips — any whole-surface state change whose old/new crossfade earns a document snapshot; per-element motion stays on `Motion` rows.
- Law: the commit inside `run` is synchronous by construction — an async commit leaves the snapshot pair torn; awaited work completes BEFORE `run` and the transition wraps only the final atom write.
- Law: named transition regions are CSS data — `view-transition-name` styles assign region identity in the stylesheet (through `cn` where dynamic), and `::view-transition-*` pseudo-element animation is authored beside the token stylesheet; this module never touches per-region JS.
- Law: reduced motion degrades to instant — the gate reads `matchMedia("(prefers-reduced-motion: reduce)")` at call time, mirroring `Motion`'s `motion-reduce:` law at the document tier.
- Law: `<Activity mode="hidden">` is the stable pre-render/hide row — a subtree keeps its state and defers its effects while hidden, and pre-renders a cold route before navigation; the mode value is the whole knob and rides an atom-derived string; hidden means paused — a viewer frame loop reads the activity state and parks while its viewport subtree is hidden, and the wake path re-arms the loop on `mode` flipping visible; `Activity` composes with `Suspense` so a hidden pre-render suspends and resolves in the background and the fallback never flashes for a pre-rendered route.
- Law: the degrade chain is total — `<ViewTransition>` (gated) → native `startViewTransition` (current) → bare `flushSync` commit (floor); every tier preserves the commit semantics, so callers are transition-agnostic by construction and no public tier probe exists — a caller branching on the tier re-opens the modality `Transition.run` already owns.
- Law: `<ViewTransition>` and `addTransitionType` are `[R16]` — typed in `@types/react` `./canary`, absent from the stable runtime, so no shipping row imports them; a `./canary` import on the stable path is the named defect; the gate closing rewrites `Transition.run`'s interior to the component form and deletes zero call sites because the entrypoint signature already owns the modality. RAC `SharedElementTransition` is the pre-stable pairing row on the same gate.
- Boundary: `flushSync` also serves `FocusScope` restoration (`system/primitive`); the atom write being committed is `system/atom` material; which routes pre-render is app routing policy; interrupted-transition policy (a second `run` while one is live skips the old — the platform's own `skipTransition` semantics) rides the returned handle.

```typescript
import { Effect } from "effect"
import { flushSync } from "react-dom"

declare namespace Transition {
  type Options = { readonly force?: boolean }
}

const _eligible = (force: boolean): boolean =>
  typeof globalThis.document.startViewTransition === "function"
  && (force || !globalThis.matchMedia("(prefers-reduced-motion: reduce)").matches)

const Transition: {
  readonly run: (commit: () => void, options?: Transition.Options) => Effect.Effect<void>
} = {
  run: (commit, options) =>
    _eligible(options?.force ?? false)
      ? Effect.tryPromise({
          try: () => globalThis.document.startViewTransition(() => flushSync(commit)).finished,
          catch: () => undefined,
        }).pipe(Effect.ignore)
      : Effect.sync(() => flushSync(commit)),
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Gesture, Motion, Transition }
```
