# [UI_GESTURE]

`act/gesture.ts` owns the interaction-class division and the continuous-gesture owner: `react-aria` owns every DISCRETE accessible interaction — press, hover-intent, focus, keyboard, cross-input-normalized events, focus scoping — and `@use-gesture/react` owns every CONTINUOUS analog gesture — drag deltas, pinch scale/rotate, wheel zoom, swipe momentum. The two compose on one element with each owning its class; a discrete press routed through a pointer handler, a raw DOM listener where a hook exists, or a second gesture hook stacked on one element is the named defect. The module's owner, `Gesture`, is the parameterized recognizer factory the `viewer` camera rows drive: tree-shaken engines, atom-bound offset origin, world-space transform, and the non-passive binding law built in.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]         | [OWNS]                                                                            |
| :-----: | :---------------- | :--------------------------------------------------------------------------------- |
|   [1]   | `CLASS_DIVISION`  | the discrete/continuous ownership law and the composition rules on shared elements  |
|   [2]   | `DISCRETE_ROWS`   | the react-aria interaction/focus hook composition every `view` row binds through    |
|   [3]   | `CONTINUOUS_OWNER`| `Gesture` — the tree-shaken camera/free-drag recognizer factory over `@use-gesture` |

## [2]-[CLASS_DIVISION]

- Law: react-aria owns discrete — `usePress`/`useHover`/`useLongPress`/`useKeyboard`/`useMove` emit `PressEvent`/`HoverEvent`/`MoveEvent` normalized across mouse, touch, pen, keyboard, and virtual cursors; `act` never binds a raw `onClick`/`onPointerDown` where a hook covers the interaction, and `on*` prop TYPES stay the `@types/react` `EventHandler` aliases while behavior sources from the hooks.
- Law: `@use-gesture` owns continuous — cumulative `offset`, per-gesture `movement`, `velocity`/`direction`/`swipe` classification; a drag delta computed from raw pointer events or a wheel-zoom hand-rolled from `onWheel` restates the recognizer.
- Law: third-party drag surfaces keep their own physics — `vaul` sheets drag through vaul, RAC collections drag through `useDragAndDrop`; layering `useDrag` over either is the double-bind defect.
- Law: composition on one element is `mergeProps` — a draggable that is also keyboard-operable spreads the react-aria bundle and the gesture `bind()` through one `mergeProps` fold; handler chains, ids, and aria attributes merge, and declaration-order spreading is rejected.
- Boundary: which widget owns which state is `view/primitive`'s spine; sheet drag is `vaul`'s (`view/compose`); the camera atoms a gesture writes are `viewer/geo/project`'s.

## [3]-[DISCRETE_ROWS]

- Law: focus is scoped, never managed by hand — `FocusScope` traps/restores for overlays, `useFocusRing` styles keyboard-only focus (its `isFocusVisible` reaches CSS as the `focus-visible:` variant), `useFocusWithin` tracks containment, and `useFocusManager` walks programmatically; a `tabindex` ladder or a `document.activeElement` read in a row marks a missing hook.
- Law: hover carries intent — `useHover` suppresses touch-emulated hover and pairs with `@floating-ui`'s `safePolygon` only at the overlay seam (`view/compose`); `useInteractOutside` owns outside-press dismissal where a full overlay stack is not mounted.
- Law: every bundle spreads through `mergeProps`, refs reconcile through `useObjectRef`/`mergeRefs`, and `useId` supplies SSR-stable identity — three mechanisms, no local variants.

```typescript
import { mergeProps, useFocusRing, useHover, useKeyboard, usePress } from "react-aria"
import type { DOMAttributes } from "@react-types/shared"

declare namespace Discrete {
  type Options = {
    readonly onPress?: (kind: "keyboard" | "mouse" | "pen" | "touch" | "virtual") => void
    readonly onHoverChange?: (hovering: boolean) => void
    readonly onKey?: (key: string) => void
    readonly disabled?: boolean
  }
  type Bundle = { readonly props: DOMAttributes; readonly focusVisible: boolean; readonly pressed: boolean; readonly hovered: boolean }
}

const _discrete = (options: Discrete.Options): Discrete.Bundle => {
  const press = usePress({ isDisabled: options.disabled, onPress: (event) => options.onPress?.(event.pointerType) })
  const hover = useHover({ isDisabled: options.disabled, onHoverChange: options.onHoverChange })
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

- Owner: `Gesture` — the continuous recognizer factory: `Gesture.useCanvas(options)` composes ONE tree-shaken `useGesture` variant (`createUseGesture([dragAction, pinchAction, wheelAction])` — only the engines the viewer uses bundle) bound imperatively to the canvas ref with `eventOptions: { passive: false }` and `preventDefault: true`, the ONLY binding that can block page scroll on wheel/touch; drag pans, pinch zooms-and-rotates around `origin`, wheel zooms with `pinchOnWheel` folding ctrl+wheel into the pinch engine. The `use` prefix is load-bearing — the member composes hooks, so rules-of-hooks and the compiler's inference both key on it.
- Packages: `@use-gesture/react` (`createUseGesture`, `dragAction`/`pinchAction`/`wheelAction`, the config/state algebra), `@effect-atom/atom-react` (the camera atom the handlers write).
- Entry: one `Gesture.useCanvas` per interactive surface — a new gesture on that surface is a handler key or sub-config on the same call, never a second hook on the element.
- Law: start state rides `memo`, origin rides `from` — the handler captures the origin on `first`, applies `movement` against it, and returns the memo; `from: () => read(camera)` binds the offset origin to the live atom so consecutive gestures accumulate; an external mutable ref for gesture accumulation is the named defect.
- Law: the handler stays in domain coordinates — `transform` maps the raw screen `Vector2` into world/canvas space before the handler sees `movement`/`offset`; `bounds` + `rubberband` clamp with elastic overflow; `axis: "lock"` locks the dominant axis past `threshold`.
- Law: high-frequency writes commit non-urgently — the camera atom write wraps in `startTransition` so the pointer stream stays responsive while the non-urgent camera commit deprioritizes; the write itself is `useAtomSet(camera)` with `"value"` mode.
- Boundary: the camera state shape and its per-backend adapters are `viewer/geo/project`'s; `Gesture.useCanvas` only recognizes and writes intents.
- Growth: a new recognizer class (a two-finger rotate row, a keyboard-displacement drag) is one handler key plus its sub-config; a new surface is one `Gesture.useCanvas` call.

```typescript
import { createUseGesture, dragAction, pinchAction, wheelAction } from "@use-gesture/react"
import type { Vector2 } from "@use-gesture/react"
import { startTransition } from "react"
import type { RefObject } from "react"

const _useCanvasGesture = createUseGesture([dragAction, pinchAction, wheelAction])

declare namespace Gesture {
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
  readonly useCanvas: (options: Gesture.CanvasOptions) => void
} = {
  useCanvas: (options) => {
    _useCanvasGesture(
      {
        onDrag: ({ offset: [x, y] }) =>
          startTransition(() => options.write({ ...options.read(), center: [x, y] })),
        onPinch: ({ offset: [scale, angle] }) =>
          startTransition(() => options.write({ ...options.read(), zoom: scale, bearing: angle })),
        onWheel: ({ offset: [, y] }) =>
          startTransition(() => options.write({ ...options.read(), zoom: options.read().zoom - y / 500 })),
      },
      {
        target: options.target,
        eventOptions: { passive: false },
        transform: options.transform,
        drag: { from: () => options.read().center, preventDefault: true, filterTaps: true },
        pinch: {
          from: () => [options.read().zoom, options.read().bearing],
          scaleBounds: options.zoomBounds ?? { min: 0.1, max: 64 },
          pinchOnWheel: true,
        },
        wheel: { preventDefault: true },
      },
    )
  },
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Gesture }
```
