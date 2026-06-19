# [API_CATALOGUE] @use-gesture/react

`@use-gesture/react` supplies React hooks for binding pointer, touch, wheel, scroll, move, and hover gestures to any DOM element or React component. Each gesture hook returns a prop-binder function (or is void when a `target` ref is configured), and the library exposes full typed state objects per gesture key for drag, pinch, wheel, scroll, move, and hover interactions consumed by canvas, panel, and interactive-widget owners in the ui stack.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@use-gesture/react`
- package: `@use-gesture/react`
- module: `@use-gesture/react`
- namespace: named exports; re-exports all `@use-gesture/core` types and actions
- asset: React gesture hooks and core gesture types
- rail: interaction

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: gesture state family
- rail: interaction

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]      | [RAIL]                                                                                                                               |
| :-----: | :-------------------- | :----------------- | :----------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `SharedGestureState`  | shared state       | activity flags (`dragging`, `pinching`, etc.), input state, lock                                                                     |
|  [02]   | `CommonGestureState`  | base gesture state | event, target, movement, distance, delta, offset, velocity, direction, first, last, active, memo                                     |
|  [03]   | `CoordinatesState`    | coordinate state   | extends `CommonGestureState`; `axis: 'x' \| 'y' \| undefined`; `xy: Vector2`                                                         |
|  [04]   | `DragState`           | drag state         | extends `CoordinatesState`; `canceled`, `tap`, `swipe: Vector2`, `cancel()`                                                          |
|  [05]   | `PinchState`          | pinch state        | extends `CommonGestureState`; `da: Vector2`, `axis: 'scale' \| 'angle' \| undefined`, `origin: Vector2`, `turns: number`, `cancel()` |
|  [06]   | `FullGestureState<K>` | keyed state union  | combines `SharedGestureState` with the specific state for `GestureKey` `K`                                                           |
|  [07]   | `State`               | full state bag     | `shared: SharedGestureState` + optional per-gesture typed states                                                                     |

[PUBLIC_TYPE_SCOPE]: gesture key and event type family
- rail: interaction

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]      | [RAIL]                                                                            |
| :-----: | :--------------------- | :----------------- | :-------------------------------------------------------------------------------- |
|  [01]   | `GestureKey`           | key union          | `'drag' \| 'wheel' \| 'scroll' \| 'move' \| 'hover' \| 'pinch'`                   |
|  [02]   | `IngKey`               | active-state union | `'dragging' \| 'wheeling' \| 'moving' \| 'hovering' \| 'scrolling' \| 'pinching'` |
|  [03]   | `EventTypes`           | event map          | maps each `GestureKey` to its DOM event class                                     |
|  [04]   | `AnyHandlerEventTypes` | custom event map   | partial gesture-key to any-event mapping for custom event types                   |

[PUBLIC_TYPE_SCOPE]: handler and config family
- rail: interaction

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]       | [RAIL]                                                                                                   |
| :-----: | :------------------- | :------------------ | :------------------------------------------------------------------------------------------------------- |
|  [01]   | `Handler<Key, Evt>`  | gesture handler     | `(state: FullGestureState<Key>) => any \| void`                                                          |
|  [02]   | `UserHandlers<T>`    | handler map         | all `onDrag`, `onDragStart`, `onDragEnd`, `onPinch`, `onWheel`, `onMove`, `onScroll`, `onHover` variants |
|  [03]   | `GestureHandlers<T>` | partial handler map | `Partial<NativeHandlers<T> & UserHandlers<T>>`                                                           |
|  [04]   | `UserDragConfig`     | drag config         | `GenericOptions` + bounds, axis, swipe, filterTaps, pointer options                                      |
|  [05]   | `UserPinchConfig`    | pinch config        | `GenericOptions` + pinch bounds, pointer, modifierKey                                                    |
|  [06]   | `UserWheelConfig`    | wheel config        | `GenericOptions` + axis, bounds                                                                          |
|  [07]   | `UserScrollConfig`   | scroll config       | `GenericOptions` + axis, bounds                                                                          |
|  [08]   | `UserMoveConfig`     | move config         | `GenericOptions` + `mouseOnly?`                                                                          |
|  [09]   | `UserHoverConfig`    | hover config        | `GenericOptions` + `mouseOnly?`                                                                          |
|  [10]   | `UserGestureConfig`  | composite config    | all per-gesture config keys under one object                                                             |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: individual gesture hooks
- rail: interaction

| [INDEX] | [SURFACE]                                  | [ENTRY_FAMILY] | [RAIL]                                          |
| :-----: | :----------------------------------------- | :------------- | :---------------------------------------------- |
|  [01]   | `useDrag<Evt, Config>(handler, config?)`   | drag hook      | binds pointer drag; returns prop-binder or void |
|  [02]   | `usePinch<Evt, Config>(handler, config?)`  | pinch hook     | binds two-pointer pinch/zoom gesture            |
|  [03]   | `useWheel<Evt, Config>(handler, config?)`  | wheel hook     | binds wheel events                              |
|  [04]   | `useScroll<Evt, Config>(handler, config?)` | scroll hook    | binds element scroll events                     |
|  [05]   | `useMove<Evt, Config>(handler, config?)`   | move hook      | binds pointermove / mousemove                   |
|  [06]   | `useHover<Evt, Config>(handler, config?)`  | hover hook     | binds pointerenter / pointerleave               |

[ENTRYPOINT_SCOPE]: composite and factory hooks
- rail: interaction

| [INDEX] | [SURFACE]                                             | [ENTRY_FAMILY] | [RAIL]                                                                |
| :-----: | :---------------------------------------------------- | :------------- | :-------------------------------------------------------------------- |
|  [01]   | `useGesture<HandlerTypes, Config>(handlers, config?)` | composite hook | binds multiple gestures in one call                                   |
|  [02]   | `createUseGesture(...actions)`                        | hook factory   | creates a typed `useGesture`-like hook from a specific set of actions |

## [04]-[IMPLEMENTATION_LAW]

[GESTURE_TOPOLOGY]:
- when `config.target` is a ref object, all hooks return `void` and bind events directly to the target element; otherwise they return a prop-binder `(...args) => ReactDOMAttributes` that must be spread onto the JSX element
- `Handler<Key, Evt>` receives `FullGestureState<Key>` which merges `SharedGestureState` (cross-gesture flags) with the per-gesture state; `first` is `true` on the first event of a gesture, `last` on the final event
- `DragState.cancel()` and `PinchState.cancel()` abort the gesture in progress; the handler receives a final call with `canceled: true`
- `useGesture` accepts `GestureHandlers` which is `Partial<UserHandlers & NativeHandlers>`, allowing any subset of gesture handlers

[LOCAL_ADMISSION]:
- Prefer `config.target` with a React ref for elements that are not under direct JSX control (canvas, external DOM nodes).
- Spread the returned prop-binder onto the element whose events should be captured when `target` is omitted.
- Use `createUseGesture` with explicit action imports to tree-shake unused gesture engines from the bundle.

[RAIL_LAW]:
- Package: `@use-gesture/react`
- Owns: React gesture binding for drag, pinch, wheel, scroll, move, and hover
- Accept: handler callbacks typed against `FullGestureState<Key>` or per-gesture state types
- Reject: raw `onPointerDown`/`onMouseMove` event handlers when a gesture hook covers the same interaction
