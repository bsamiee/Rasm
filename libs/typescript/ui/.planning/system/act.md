# [UI_ACT]

Act seats five motion planes on one owner so motion authority never fractures: `react-aria` normalizes discrete accessible interaction, `@use-gesture/react` recognizes continuous analog gesture, the `Motion` class rows compose enter/exit over `tw-animate-css`, the `Motion` physical plane drives springs, values, scroll linkage, morphs, and presence over the `motion` engine, and `Transition` ladders document-level swaps down to a bare commit. Module: `ui/src/system/act.ts`.

## [01]-[INDEX]

- [02]-[CLASS_DIVISION]: discrete/continuous ownership law and the composition rules on shared elements; —.
- [03]-[DISCRETE_ROWS]: react-aria press/hover/move/keyboard/context-menu hook composition every view row binds through; `Gesture`.
- [04]-[CONTINUOUS_OWNER]: tree-shaken camera/free-drag recognizer factory over `@use-gesture`; `Gesture`.
- [05]-[MOTION_ROWS]: named enter/exit composition vocabulary over the tw-animate axis mechanism; `Motion`.
- [06]-[CONTINUOUS_MOTION]: physical plane — springs, motion values, scroll linkage, morphs, presence, policy; `Motion`.
- [07]-[DOCUMENT_RAIL]: three-tier document-transition ladder, the `<Activity>` row, the degrade chain; `Transition`.

## [02]-[CLASS_DIVISION]

[CLASS_DIVISION]:
- Law: react-aria owns discrete — `usePress`/`useHover`/`useLongPress`/`useKeyboard`/`useMove`/`useContextMenu` emit `PressEvent`/`HoverEvent`/`MoveEvent`/`ContextMenuEvent` normalized across mouse, touch, pen, keyboard, virtual cursors, and the platform's own context-menu gestures; this page never binds a raw `onClick`/`onPointerDown`/`onContextMenu` where a hook covers the interaction, and `on*` prop TYPES stay the `@types/react` `EventHandler` aliases while behavior sources from the hooks.
- Law: `@use-gesture` owns continuous RECOGNITION — cumulative `offset`, per-gesture `movement`, `velocity`/`direction`/`swipe` classification; a drag delta computed from raw pointer events or a wheel-zoom hand-rolled from `onWheel` restates the recognizer. `motion` owns continuous ANIMATION — an element the engine animates takes engine gestures (`whileTap`, `drag`) only when no recognizer binds it; a use-gesture binding and motion `drag` on one node is the double-bind defect.
- Law: third-party drag surfaces keep their own physics — `vaul` sheets drag through vaul (`view/overlay`), RAC collections drag through `useDragAndDrop` (`system/primitive`'s roster); layering `useDrag` or motion's `Reorder.Group` over either is the double-bind defect, and `Reorder` binds only a drag-ordered list RAC models no widget for.
- Law: composition on one element is `mergeProps` — a draggable that is also keyboard-operable spreads the react-aria bundle and the gesture `bind()` through one `mergeProps` fold; handler chains, ids, and aria attributes merge, and declaration-order spreading is rejected.
- Law: one surface, one motion owner — a RAC overlay animating through a `Motion` class row never also mounts `AnimatePresence` around the same element; a surface graduates from class rows to the physical plane when it needs physics, interruption, values, or layout, never both on one property.
- Boundary: which widget owns which state is `system/primitive`'s spine; the camera atoms a gesture writes are the viewer projection plane's.

## [03]-[DISCRETE_ROWS]

[DISCRETE_ROWS]:
- Owner: `Gesture.useDiscrete(options)` — the composed discrete bundle: `usePress` + `useHover` + `useLongPress` + `useMove` + `useKeyboard` + `useContextMenu` + `useFocusRing` merged through one `mergeProps` fold into a single spreadable prop record with the state flags a recipe styles; the `use` prefix is load-bearing — the member composes hooks, so rules-of-hooks and the compiler's inference both key on it.
- Packages: `react-aria` (`usePress`, `useHover`, `useLongPress`, `useMove`, `useKeyboard`, `useContextMenu`, `useFocusRing`, `mergeProps`, `ContextMenuEvent`, `KeyboardShortcutBindings`, `LongPressEvent`, `MoveEvent`); the bundle's prop-record type derives from the composed hooks' OWN returns — the react-aria barrel re-exports no attribute base, so a `@react-types/shared` import is the unadmitted-package defect.
- Law: the bundle's prop type IS the merge, never one arm — `Gesture.DiscreteProps` intersects every composed hook's returned record, so an attribute the fold carries can never go missing from the contract that spreads it; typing the bundle off `pressProps` alone erases the hover, long-press, move, keyboard, and context-menu handlers from the type while shipping every one of them at runtime.
- Law: context-menu invocation is ONE row, not four handlers — `useContextMenu` answers a single `ContextMenuEvent` for a right-click, a Ctrl+click, `Shift`+`F10` or the menu key, a screen-reader activation, and a touch long-press, and its `contextMenuProps` join the same fold; the event's `x`/`y` are RELATIVE TO THE TARGET, so this bundle hands the event on untouched and the viewport conversion happens once at `view/overlay#PALETTE`.
- Law: a row declares long-press OR context-menu, never both — on iOS the context-menu row IS a long press, because the platform fires no `contextmenu` event and the hook falls back to its own `useLongPress`, so a second recognizer on that element is the double-bind `[02]` names; `Gesture.Invocation` makes the exclusion structural rather than a review note.
- Law: a modified key is a DECLARED binding, never a handler branch — `shortcuts` carries a canonical-shortcut-string map to its action and `useKeyboard` matches the combination, resolving modifier state, layout, and platform meta-versus-control itself, so `onKey` narrows to the bare unbound key a surface still wants raw; reading `event.metaKey`/`event.ctrlKey` in a row rebuilds the matcher the map already owns, and the record itself is minted by `view/overlay#PALETTE` so the whole keymap is one value a command surface renders and rebinds.
- Law: auto-repeat and IME composition are bundle knobs, never row branches — `allowRepeats` and `allowComposing` gate the SHORTCUT matcher alone and ride `useKeyboard` beside the record, so a held key bound to fire once and a chord typed mid-composition are declared where the matcher lives; a repeat guard kept in an action's closure re-implements the gate.
- Law: focus is scoped, never managed by hand — `FocusScope` traps/restores for overlays, `useFocusRing` styles keyboard-only focus (its `isFocusVisible` reaches CSS as the `focus-visible:` variant), `useFocusWithin` tracks containment, and `useFocusManager` walks programmatically; a `tabindex` ladder or a `document.activeElement` read in a row marks a missing hook.
- Law: hover carries intent — `useHover` suppresses touch-emulated hover and pairs with floating-ui's `safePolygon` only at the overlay seam (`view/overlay`); `useInteractOutside` owns outside-press dismissal where a full overlay stack is not mounted.
- Law: every bundle spreads through `mergeProps`, refs reconcile through `useObjectRef`/`mergeRefs`, and `useId` supplies SSR-stable identity — three mechanisms, no local variants.
- Growth: a new discrete class is one hook composed into the same fold with its arm on the options record and its return in `DiscreteProps` — never a second bundle beside this one.

```typescript signature
import { mergeProps, useContextMenu, useFocusRing, useHover, useKeyboard, useLongPress, useMove, usePress } from "react-aria"
import type { ContextMenuEvent, KeyboardShortcutBindings, LongPressEvent, MoveEvent } from "react-aria"

declare namespace Gesture {
  type DiscreteBase = {
    readonly onPress?: (kind: "keyboard" | "mouse" | "pen" | "touch" | "virtual") => void
    readonly onHoverChange?: (hovering: boolean) => void
    readonly onMove?: (event: MoveEvent) => void
    readonly onKey?: (key: string) => void
    readonly shortcuts?: KeyboardShortcutBindings
    readonly allowRepeats?: boolean // gates the shortcut matcher only; the raw onKey arm is untouched
    readonly allowComposing?: boolean
    readonly disabled?: boolean
  }
  // iOS answers a context menu THROUGH useLongPress, so one element can carry only one of the two arms
  type Invocation =
    | { readonly onLongPress?: (event: LongPressEvent) => void; readonly onContextMenu?: never }
    | { readonly onContextMenu?: (event: ContextMenuEvent) => void; readonly onLongPress?: never }
  type DiscreteOptions = Gesture.DiscreteBase & Gesture.Invocation
  type DiscreteProps =
    & ReturnType<typeof usePress>["pressProps"]
    & ReturnType<typeof useHover>["hoverProps"]
    & ReturnType<typeof useLongPress>["longPressProps"]
    & ReturnType<typeof useMove>["moveProps"]
    & ReturnType<typeof useKeyboard>["keyboardProps"]
    & ReturnType<typeof useContextMenu>["contextMenuProps"]
    & ReturnType<typeof useFocusRing>["focusProps"]
  type DiscreteBundle = { readonly props: Gesture.DiscreteProps; readonly focusVisible: boolean; readonly pressed: boolean; readonly hovered: boolean }
}

const _useDiscrete = (options: Gesture.DiscreteOptions): Gesture.DiscreteBundle => {
  const disabled = options.disabled ?? false
  const press = usePress({ isDisabled: disabled, onPress: (event) => options.onPress?.(event.pointerType) })
  const hover = useHover({ isDisabled: disabled, onHoverChange: (hovering) => options.onHoverChange?.(hovering) })
  const long = useLongPress(disabled || options.onLongPress === undefined ? {} : { onLongPress: options.onLongPress })
  const move = useMove(disabled || options.onMove === undefined ? {} : { onMove: options.onMove }) // useMove carries no isDisabled knob: the arm withholds instead
  // `shortcuts` matches modifier combinations itself, leaving onKey the raw arm for a bare unbound key
  const keyboard = useKeyboard({
    isDisabled: disabled,
    onKeyDown: (event) => options.onKey?.(event.key),
    ...(options.shortcuts !== undefined && { shortcuts: options.shortcuts }),
    ...(options.allowRepeats !== undefined && { allowRepeats: options.allowRepeats }),
    ...(options.allowComposing !== undefined && { allowComposing: options.allowComposing }),
  })
  // useContextMenu carries two platform traps in its own arms: the macOS Ctrl+Enter keydown path answers the TARGET'S
  // CENTRE rather than a pointer position on a 10ms de-dupe against the real event, and every arm stops
  // propagation, so nested context-menu targets resolve innermost-wins with no ordering knob
  const context = useContextMenu(
    disabled || options.onContextMenu === undefined ? {} : { onContextMenu: options.onContextMenu },
  )
  const ring = useFocusRing()
  return {
    props: mergeProps(
      press.pressProps,
      hover.hoverProps,
      long.longPressProps,
      move.moveProps,
      keyboard.keyboardProps,
      context.contextMenuProps,
      ring.focusProps,
    ),
    focusVisible: ring.isFocusVisible,
    pressed: press.isPressed,
    hovered: hover.isHovered,
  }
}
```

## [04]-[CONTINUOUS_OWNER]

[CONTINUOUS_OWNER]:
- Owner: `Gesture.useCanvas(options)` — the continuous recognizer factory: composes ONE tree-shaken `useGesture` variant (`createUseGesture([dragAction, pinchAction, wheelAction])` — only the engines the viewer uses bundle) bound imperatively to the canvas ref with `eventOptions: { passive: false }` and `preventDefault: true`, the ONLY binding that can block page scroll on wheel/touch; drag pans, pinch zooms-and-rotates around `origin`, wheel zooms with `pinchOnWheel` folding ctrl+wheel into the pinch engine, and every arm settles through ONE `emit`-then-`write` seam so the owning plane's intent mint is the single egress.
- Packages: `@use-gesture/react` (`createUseGesture`, `dragAction`/`pinchAction`/`wheelAction`, the config/state algebra); `react` (`startTransition`, `useEffectEvent`).
- Entry: one `Gesture.useCanvas` per interactive surface — a new gesture on that surface is a handler key or sub-config on the same call, never a second hook on the element.
- Law: start state rides `memo`, origin rides `from` — the handler captures the origin on `first`, applies `movement` against it, and returns the memo; `from: () => read(camera)` binds the offset origin to the live atom so consecutive gestures accumulate; an external mutable ref for gesture accumulation is the named defect.
- Law: the wheel arm integrates per-event `delta` — wheel `offset` accumulates for the surface lifetime with no `from`-bound origin, so offset math against the live atom double-integrates every event; `delta` applies each tick exactly once against one read of the current state, scaled by the `_CANVAS.wheel` policy value — never an inline sensitivity literal.
- Law: the handler stays in domain coordinates — `transform` maps the raw screen `Vector2` into world/canvas space before the handler sees `movement`/`offset`; `bounds` + `rubberband` clamp with elastic overflow; `axis: "lock"` locks the dominant axis past `threshold`.
- Law: one bounds row clamps every zoom write path structurally — the pinch engine clamps through `scaleBounds: bounds` and the wheel arm clamps through `_clamp(bounds, …)` against the SAME row; a zoom path escaping the row is the named defect.
- Law: high-frequency writes commit non-urgently and stably — the intent write wraps in `startTransition`, and the write callback rides `useEffectEvent` so a changing callback identity never re-binds the recognizer; the write itself is `useAtomSet(camera)` with `"value"` mode.
- Law: `Gesture.Reading` is the recognizer's OWN output vocabulary — the three axes a drag, a pinch, and a wheel can physically produce — and never a camera: the owning plane's `emit` folds a reading over its live camera state and mints the intent, so the camera shape, its extra axes, and its intent family all stay that plane's and this floor holds no rival. Writing a camera record straight through, or minting a fourth axis here, re-opens the per-surface camera shape the seam exists to foreclose.
- Law: intents are the only write path — `emit` is the owning plane's intent mint (`viewer/geo#CAMERA`'s `Camera.gestured`, folding a live drag to its already-arrived destination), so a gesture, a viewpoint restore, and a control intent reach one camera driver through one closed family and a replay journal cannot tell them apart.
- Boundary: the camera state shape, its extra axes, its intent family, and the per-backend adapters are the viewer projection plane's; this owner recognizes axes and hands them to that plane's mint.
- Growth: a new recognizer class (a two-finger rotate row, a keyboard-displacement drag) is one handler key with its sub-config; a new camera axis is one field on the owning plane's state, invisible here because a reading folds over it; a new surface is one `Gesture.useCanvas` call.

```typescript signature
import { createUseGesture, dragAction, pinchAction, wheelAction } from "@use-gesture/react"
import type { Vector2 } from "@use-gesture/react"
import type { Types } from "effect"
import { startTransition, useEffectEvent } from "react"
import type { RefObject } from "react"

const _CANVAS = { zoom: { min: 0.1, max: 64 }, wheel: 500 } as const

const _clamp = (bounds: { readonly min: number; readonly max: number }, zoom: number): number =>
  Math.min(bounds.max, Math.max(bounds.min, zoom))

const _useCanvasGesture = createUseGesture([dragAction, pinchAction, wheelAction])

declare namespace Gesture {
  // exactly the axes a drag, a pinch, and a wheel produce — never a camera
  type Reading = { readonly center: readonly [number, number]; readonly zoom: number; readonly bearing: number }
  type CanvasOptions<W> = {
    readonly target: RefObject<HTMLElement | null>
    readonly read: () => Gesture.Reading
    readonly emit: (reading: Gesture.Reading) => W
    readonly write: (intent: W) => void
    readonly transform?: (screen: Vector2) => Vector2
    readonly zoomBounds?: { readonly min: number; readonly max: number }
  }
  type Shape = Types.Simplify<{
    readonly useDiscrete: typeof _useDiscrete
    readonly useCanvas: <W>(options: Gesture.CanvasOptions<W>) => void
  }>
}

const Gesture: Gesture.Shape = {
  useDiscrete: _useDiscrete,
  useCanvas: <W,>(options: Gesture.CanvasOptions<W>) => {
    const bounds = options.zoomBounds ?? _CANVAS.zoom
    // one seam: every arm hands its reading to the owning plane's intent mint, so no arm writes a camera
    const commit = useEffectEvent((reading: Gesture.Reading) => options.write(options.emit(reading)))
    _useCanvasGesture(
      {
        onDrag: ({ offset: [x, y] }) =>
          startTransition(() => commit({ ...options.read(), center: [x, y] })),
        onPinch: ({ offset: [scale, angle] }) =>
          startTransition(() => commit({ ...options.read(), zoom: scale, bearing: angle })),
        onWheel: ({ delta: [, dy] }) => {
          const held = options.read()
          startTransition(() => commit({ ...held, zoom: _clamp(bounds, held.zoom - dy / _CANVAS.wheel) }))
        },
      },
      {
        target: options.target,
        eventOptions: { passive: false },
        ...(options.transform !== undefined && { transform: options.transform }),
        // spread copies the readonly axis tuple into the mutable Vector2 the engine's origin slot takes
        drag: { from: () => [...options.read().center], preventDefault: true, filterTaps: true },
        pinch: { from: () => [options.read().zoom, options.read().bearing], scaleBounds: bounds, pinchOnWheel: true },
        wheel: { preventDefault: true },
      },
    )
  },
}
```

## [05]-[MOTION_ROWS]

[MOTION_ROWS]:
- Owner: the `Motion` class-row vocabulary: one `as const satisfies Record<string, Motion.Row>` table whose rows compose `tw-animate-css`'s single mechanism — `animate-in`/`animate-out` trigger + axis setters (`fade-*`, `zoom-*`, `slide-*`, `blur-*`, `spin-*`) + timing modifiers (`animation-duration-*`, `delay-*`, `fill-mode-*`) — into enter/exit class pairs keyed by surface concept (`overlay`, `sheet`, `palette`, `toast`, `panel`). Every row leads with `motion-reduce:animate-none` so reduced motion is a construction fact.
- Packages: `tw-animate-css` (imported once in the token stylesheet as `@import "tw-animate-css";` after the tailwind entry — pure CSS, zero runtime); `tailwindcss` core `motion-reduce:` variant.
- Law: every row pairs a trigger with at least one axis setter — a bare `animate-in` animates nothing; the row table makes the pairing structural because every row string carries both.
- Law: never author a `@keyframes` for an enter/exit effect the six axes express; the named component animations (`animate-accordion-down/up`, `animate-collapsible-down/up`, `animate-caret-blink`) are the only sanctioned self-contained keyframes and ride rows here, not bespoke CSS.
- Law: the RAC transition phases bind these rows through variants — `entering:` and `exiting:` (the `tailwindcss-react-aria-components` mappings of `data-entering`/`data-exiting`) scope the enter/exit halves, so overlay motion is one `cn(Motion.overlay.enter, Motion.overlay.exit)` class string with zero JS lifecycle code.
- Law: transition and SUSTAINED state are two row families on one owner — `_rows` pairs an enter with an exit and fires once per presence edge, while `Motion.holds` names the looping attention states a surface holds WHILE a condition stands (`pulse` a refusal awaiting repair, `spin` indeterminate work in flight, `ping` an arrival); a phase table keys a hold by name (`viewer/panel`'s refused row) and a hold spelled as an enter row loops nothing, while an enter row spelled as a hold never ends.
- Law: the hold roster is read off the platform, never invented — Tailwind core's sustained animations (`animate-pulse`, `animate-spin`, `animate-ping`, `animate-bounce`) are the whole candidate set and this vocabulary seats the three whose attention semantics a surface here carries; a sustained state outside that set is upstream or it is a bespoke keyframe this vocabulary refuses.
- Law: every hold leads with `motion-reduce:animate-none` under the same construction law as the transition rows, and that guard resolves because ONE merge group at `system/token#CLASS_RAIL` owns every `animate-*` trigger — enter, exit, every platform-sustained animation, the named component animations, and the `animate-none` guard together; splitting any trigger into the default group leaves `motion-reduce:animate-none` in a foreign group where it silently loses.
- Law: the row strings participate in `cn` conflict resolution — the motion class groups are taught to the one merge instance at `system/token#CLASS_RAIL`, so a caller override of `delay-*` or `fade-in-*` wins deterministically.
- Boundary: floating-ui `useTransitionStyles` phases consume `Theme.Scale.ease` values where an overlay needs style-object motion (`view/overlay`); the sheet's drag physics are `vaul`'s own and take no Motion row.
- Growth: a new surface motion is one row composing existing setters; a new axis is upstream (`tw-animate-css`), never a local keyframe.

```typescript signature
const _kinds = ["overlay", "sheet", "palette", "toast", "panel"] as const

declare namespace Motion {
  type Row = { readonly enter: string; readonly exit: string }
  type Kind = (typeof _kinds)[number]
  type Hold = keyof typeof _holds
}

// hold loops while its condition stands; Row fires once per presence edge
const _holds = {
  pulse: "motion-reduce:animate-none animate-pulse",
  spin: "motion-reduce:animate-none animate-spin",
  ping: "motion-reduce:animate-none animate-ping",
} as const satisfies Record<string, string>

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
```

## [06]-[CONTINUOUS_MOTION]

[CONTINUOUS_MOTION]:
- Owner: the `Motion` physical plane riding the same owner as the class rows — springs, motion values, scroll linkage, layout morphs, presence, and subtree policy over the `motion` engine: `Motion.springs` is the spring policy vocabulary (`snap`/`glide`/`bounce` rows — stiffness/damping/mass as data, the physical alternative to `Theme.Scale.ease` cubic-beziers for interruptible motion), `Motion.useFollow(target, kind?)` springs a render-free `MotionValue` toward atom-derived targets (viewer camera-follow readouts, panel drag numerics — bound `style={{ x }}`, React never re-renders per frame), `Motion.useReveal(target)` is the folder's ONE scroll-animation engine (`useScroll` progress + `useTransform` derivation — a second `ScrollTimeline` engine is the named defect), and `Motion.useSettle(value, commit)` is the sanctioned MotionValue→atom bridge (`useMotionValueEvent` on `animationComplete` — a `MotionValue` mirrored into an atom or `useState` is the render-thrash defect).
- Packages: `motion` — the `motion/react` entry only on this plane (`useMotionValue`, `useSpring`, `useTransform`, `useScroll`, `useVelocity`, `useMotionValueEvent`, `AnimatePresence`, `LayoutGroup`, `MotionConfig`, `useReducedMotion`, `motion.*` proxies, `layout`/`layoutId` props); `react` (`useEffect`).
- Law: the entry split is a cost ladder pinned per surface — `motion/react` full hybrid here, `motion/react-mini` (`useAnimate` only, WAAPI) for a single imperative sequence, vanilla `motion` (`animate`/`scroll`/`stagger`) where three/canvas surfaces animate without React; the `delay` unit flips seconds (vanilla) to milliseconds (react), so the entry is pinned before any timing literal.
- Law: exit choreography is `AnimatePresence` (`mode: "sync" | "wait" | "popLayout"`, `onExitComplete`) with `usePresence`/`useIsPresent` for manual completion — reached only where an unmount needs physics or interruption; a surface a `Motion` class row already animates never also mounts it.
- Law: a keyed child surviving a render holds its mount and its index while its siblings exit, so a mixed enter/exit batch keeps identity by construction — `mode` is chosen for the layout the exit leaves behind (`"popLayout"` pulls the leaver out of flow, `"wait"` serializes a full swap), never to serialize a batch into safety.
- Law: the plane animates the engine's accelerated set — `transform`, `opacity`, `filter`, `clipPath`, and `backgroundColor` hand off to WAAPI on HTML and SVG elements alike, which is why `useFollow` reaches render through `style={{ x }}` and `useReveal` derives a transform and an opacity rather than a box; a layout-triggering property animated here generates on rAF and reflows every frame, and the morph expressing it is `layout`/`layoutId`.
- Law: shared-element morphs are `layoutId` + `LayoutGroup` — grid↔detail and palette↔result morph across unrelated trees with no wrapper component; `layout` owns same-tree reflow animation, its value scoping what the morph reads — `"position"`/`"size"`/`"preserve-aspect"` by measurement kind, `"x"`/`"y"` by axis where only one dimension may move.
- Law: `MotionConfig` with `reducedMotion: "user"` is the subtree policy owner — it collapses per-row `matchMedia("(prefers-reduced-motion: reduce)")` reads on this plane into one provider, mirroring the `motion-reduce:` law of the class rows; `useReducedMotion()` reads the same signal where a value derivation branches.
- Law: velocity is a derived value — `useVelocity(value)` feeds momentum-aware decisions (overlay dismissal past a velocity floor) as a `MotionValue`, never a hand-differentiated sample pair.
- Boundary: raw gesture recognition stays `[4]`'s; drag physics on engine-animated elements are the engine's own (`CLASS_DIVISION`); the atom holds domain state — the engine interpolates presentation toward targets derived from it.
- Growth: a new spring temperament is one `springs` row; a new scroll-linked derivation is one `useTransform` fold over the same `useScroll` progress — never a second engine.

```typescript signature
import { useMotionValueEvent, useScroll, useSpring, useTransform } from "motion/react"
import type { MotionValue, SpringOptions } from "motion/react"
import { useEffect } from "react"

const _springs = {
  snap: { stiffness: 480, damping: 40, mass: 0.8 },
  glide: { stiffness: 240, damping: 32, mass: 1 },
  bounce: { stiffness: 320, damping: 18, mass: 1 },
} as const satisfies Record<string, SpringOptions>

const _useFollow = (target: number, kind: keyof typeof _springs = "glide"): MotionValue<number> => {
  const followed = useSpring(target, _springs[kind])
  useEffect(() => followed.set(target), [followed, target])
  return followed
}

const _useReveal = (target: RefObject<HTMLElement | null>): { readonly progress: MotionValue<number>; readonly lift: MotionValue<number>; readonly veil: MotionValue<number> } => {
  const { scrollYProgress } = useScroll({ target, offset: ["start end", "end start"] })
  return {
    progress: scrollYProgress,
    lift: useTransform(scrollYProgress, [0, 0.4, 1], [24, 0, -24]),
    veil: useTransform(scrollYProgress, [0, 0.25], [0, 1]),
  }
}

const _useSettle = (value: MotionValue<number>, commit: (settled: number) => void): void =>
  useMotionValueEvent(value, "animationComplete", () => commit(value.get()))

declare namespace Motion {
  type Spring = keyof typeof _springs
  type Shape = Types.Simplify<
    typeof _rows & {
      readonly kinds: typeof _kinds
      readonly holds: typeof _holds
      readonly springs: typeof _springs
      readonly useFollow: typeof _useFollow
      readonly useReveal: typeof _useReveal
      readonly useSettle: typeof _useSettle
    }
  >
}

const Motion: Motion.Shape = {
  ..._rows,
  kinds: _kinds,
  holds: _holds,
  springs: _springs,
  useFollow: _useFollow,
  useReveal: _useReveal,
  useSettle: _useSettle,
}
```

## [07]-[DOCUMENT_RAIL]

[DOCUMENT_RAIL]:
- Owner: `Transition` — one entrypoint owning the three-tier document ladder: `Transition.run(commit, options?)` gates on capability (`document.startViewTransition` present) and motion budget (`prefers-reduced-motion`), wraps the state commit in `flushSync` inside the transition callback so the DOM the browser snapshots is the post-commit DOM, and selects the tier from the options value — no `spring` row runs the native floor, a `spring` row upgrades to `animateView(update, { interrupt })`, the typed spring layer whose `"wait"`/`"immediate"` interruption policy the native API lacks; absent the API or under reduced motion the commit runs bare — the caller never branches.
- Packages: `react-dom` (`flushSync` — the synchronous commit the snapshot requires); `motion` (`animateView`, the `ViewTransitionBuilder` verbs `.add(a, b)`/`.crop()`/`.enter()`/`.exit()` for per-subject targeting); the platform View Transitions API (lib.dom); `react` canary (`ViewTransition`, `addTransitionType`); `effect` (`Effect.tryPromise` lifting `finished`).
- Entry: route changes, panel-set swaps, theme flips — any whole-surface state change whose old/new crossfade earns a document snapshot; per-element motion stays on `Motion` rows or the physical plane.
- Law: the commit inside `run` is synchronous by construction — an async commit leaves the snapshot pair torn; awaited work completes BEFORE `run` and the transition wraps only the final atom write.
- Law: the canary `<ViewTransition>` element tops the ladder — tree-driven per-element transitions (`name`/`enter`/`exit`/`update`/`share` props, shared-element morphs by repeated `name` + `share`), firing ONLY inside `startTransition`/`useDeferredValue`/a Suspense reveal and sitting directly above the DOM node it names; `addTransitionType(type)` is called in the SAME `startTransition` and keys the per-type class arms, styling landing on `::view-transition-old/new(.class)`. One `/// <reference types="react/canary" />` at the entry types admits the canary types; one tier fires per surface — a `<ViewTransition>` boundary already animating a subtree never also sits under a `run` spring upgrade of the same commit.
- Law: named transition regions are CSS data — `view-transition-name` styles assign region identity in the stylesheet (through `cn` where dynamic), and `::view-transition-*` pseudo-element animation is authored beside the token stylesheet; this module never touches per-region JS.
- Law: reduced motion degrades to instant — the gate reads `matchMedia("(prefers-reduced-motion: reduce)")` at call time, mirroring `Motion`'s `motion-reduce:` and `MotionConfig` laws at the document tier.
- Law: `<Activity mode="hidden">` is the stable pre-render/hide row — a subtree keeps its state and defers its effects while hidden, and pre-renders a cold route before navigation; the mode value is the whole knob and rides an atom-derived string; hidden means paused — a viewer frame loop reads the activity state and parks while its viewport subtree is hidden, and the wake path re-arms the loop on `mode` flipping visible; `Activity` composes with `Suspense` so a hidden pre-render suspends and resolves in the background and the fallback never flashes for a pre-rendered route.
- Law: the degrade chain is total — canary `<ViewTransition>` (tree tier) → `animateView` (spring tier) → native `startViewTransition` (floor) → bare `flushSync` commit; every tier preserves the commit semantics, so callers are transition-agnostic by construction and no public tier probe exists — a caller branching on the tier re-opens the modality `Transition.run` already owns.
- Boundary: `flushSync` also serves `FocusScope` restoration (`system/primitive`); the atom write being committed is `system/atom` material; which routes pre-render is app routing policy; interrupted-transition policy at the spring tier is the `interrupt` row — a second `run` while one is live queues (`"wait"`) or preempts (`"immediate"`), and the native tier inherits the platform's own `skipTransition` semantics.

```typescript signature
import { Effect } from "effect"
import { animateView } from "motion/react"
import { flushSync } from "react-dom"

declare namespace Transition {
  type Options = {
    readonly force?: boolean
    readonly spring?: { readonly interrupt: "wait" | "immediate" }
  }
  type Shape = {
    readonly run: (commit: () => void, options?: Transition.Options) => Effect.Effect<void>
  }
}

const _eligible = (force: boolean): boolean =>
  typeof globalThis.document.startViewTransition === "function"
  && (force || !globalThis.matchMedia("(prefers-reduced-motion: reduce)").matches)

const Transition: Transition.Shape = {
  run: (commit, options) =>
    _eligible(options?.force ?? false)
      ? options?.spring === undefined
        ? Effect.tryPromise(() => globalThis.document.startViewTransition(() => flushSync(commit)).finished).pipe(Effect.ignore)
        : Effect.tryPromise(() => animateView(() => flushSync(commit), { interrupt: options.spring.interrupt }).then(() => undefined)).pipe(Effect.ignore)
      : Effect.sync(() => flushSync(commit)),
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Gesture, Motion, Transition }
```

## [08]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
