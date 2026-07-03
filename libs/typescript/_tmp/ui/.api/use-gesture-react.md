# [API_CATALOGUE] @use-gesture/react

`@use-gesture/react` binds pointer, touch, wheel, scroll, move, and hover gestures to any DOM element or React component. Each hook returns a prop-binder `(...args) => ReactDOMAttributes` (or `void` when `config.target` is a ref), and every gesture exposes a full typed `FullGestureState<Key>` carrying `movement`/`offset`/`delta`/`velocity`/`direction`/`overflow` vectors and cross-gesture modifier flags. Its advanced surface is the config algebra — `transform` (screen→canvas-space mapping), `from`/`bounds`/`rubberband` (offset seeding + elastic clamp), the full `pointer`/`swipe`/`preventScroll`/`threshold` controls — and the `createUseGesture(...actions)` tree-shake seam over the `dragAction`/`pinchAction`/… engine set. In the ui stack the recognizer never mutates state directly: it folds `FullGestureState` into an effect-tier `Data.TaggedEnum` under `Match.tagsExhaustive` that drives the `AtomBinding` (`interaction/gesture.md#GESTURE_ALGEBRA`).

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@use-gesture/react`
- package / version: `@use-gesture/react` @ `10.3.1`
- license: `MIT`
- module: ESM `dist/use-gesture-react.esm.js` + CJS `dist/use-gesture-react.cjs.js`; `sideEffects: false` (tree-shakeable); re-exports `@use-gesture/core` `/utils`, `/actions`, `/types`
- peer: `react >= 16.8.0` (hooks); a hook returns a React `ReactDOMAttributes` prop-binder keyed by `DOMHandlers`
- dependency: `@use-gesture/core` @ `10.3.1` — the framework-agnostic `Controller`/`Engine`/`parser`/config-resolver; the react package is the React binding over it
- rail: interaction

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: gesture state family (`@use-gesture/core/types`, re-exported)
- rail: interaction

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [NOTE] |
| :-----: | :------- | :------------ | :----- |
|  [01]   | `SharedGestureState`  | cross-gesture flags | `dragging`/`wheeling`/`moving`/`hovering`/`scrolling`/`pinching`, `touches`, `pressed`, `down`, `locked`, `buttons`, `shiftKey`/`altKey`/`metaKey`/`ctrlKey` |
|  [02]   | `CommonGestureState`  | base per-gesture state | `event`, `target`, `currentTarget`, `intentional`, `movement`, `offset`, `lastOffset`, `delta`, `distance`, `velocity`, `values`, `initial`, `direction`, `overflow`, `first`, `last`, `active`, `startTime`/`timeStamp`/`elapsedTime`/`timeDelta`, `type`, `memo`, `args` (underscore-prefixed `_*` fields are engine internals) |
|  [03]   | `CoordinatesState`    | coordinate state | `CommonGestureState` + `axis: 'x' \| 'y' \| undefined`, `xy: Vector2` (alias of `values`) |
|  [04]   | `DragState`           | drag state | `CoordinatesState` + `canceled`, `cancel()`, `tap`, `swipe: Vector2` |
|  [05]   | `PinchState`          | pinch state | `CommonGestureState` + `da: Vector2` (distance/angle), `axis: 'scale' \| 'angle' \| undefined`, `origin: Vector2`, `turns: number`, `canceled`, `cancel()` |
|  [06]   | `FullGestureState<K>` | keyed state union | `SharedGestureState & NonUndefined<State[K]>` — the exact object a `Handler<K>` receives |
|  [07]   | `State`               | full state bag | `{ shared: SharedGestureState }` + optional per-gesture `drag`/`wheel`/`scroll`/`move`/`hover`/`pinch` typed states |
|  [08]   | `EventTypes`          | event map | each `GestureKey` → its DOM event(s): `drag: Pointer\|Touch\|Mouse\|KeyboardEvent`, `wheel: WheelEvent`, `pinch: Pointer\|Touch\|Wheel\|WebKitGestureEvent`, … |

[PUBLIC_TYPE_SCOPE]: gesture-key + handler family
- rail: interaction

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [NOTE] |
| :-----: | :------- | :------------ | :----- |
|  [01]   | `GestureKey`           | key union | `'drag' \| 'wheel' \| 'scroll' \| 'move' \| 'hover' \| 'pinch'` (`Exclude<keyof State, 'shared'>`) |
|  [02]   | `CoordinatesKey`       | key union | `Exclude<GestureKey, 'pinch'>` — the coordinate gestures |
|  [03]   | `IngKey`               | active-flag union | `'dragging' \| 'wheeling' \| 'moving' \| 'hovering' \| 'scrolling' \| 'pinching'` |
|  [04]   | `Handler<Key, Evt>`    | gesture handler | `(state: FullGestureState<Key> & { event: Evt }) => any \| void`; a returned value becomes next call's `memo` |
|  [05]   | `UserHandlers<T>`      | handler map | `onDrag`/`onDragStart`/`onDragEnd`, `onPinch`/`Start`/`End`, `onWheel`/`Start`/`End`, `onMove`/`Start`/`End`, `onScroll`/`Start`/`End`, `onHover` (hover has NO Start/End) |
|  [06]   | `NativeHandlers<T>`    | DOM handler map | native `onClick`/`onPointerDown`/… receiving `SharedGestureState`; merged into `useGesture` |
|  [07]   | `GestureHandlers<T>`   | partial handler map | `Partial<NativeHandlers<T> & UserHandlers<T>>` — any subset |
|  [08]   | `AnyHandlerEventTypes` | custom event map | partial gesture-key → any-event map for custom event typing |
|  [09]   | `ReactDOMAttributes` / `DOMHandlers` | binder shape | the prop object a bind-fn spreads onto JSX; `DOMHandlers` is the full DOM event-handler key set |
|  [10]   | `Vector2` / `Target` / `PointerType` / `WebKitGestureEvent` | primitives | `[number, number]`; `EventTarget \| { current }`; `'mouse'\|'touch'\|'pen'`; Safari gesture event |

[PUBLIC_TYPE_SCOPE]: config family (per-gesture options fold into `UserGestureConfig`)
- rail: interaction

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [NOTE] |
| :-----: | :------- | :------------ | :----- |
|  [01]   | `GenericOptions`     | shared config | `target`, `window`, `eventOptions: AddEventListenerOptions` (passive/capture), `enabled`, `transform: (Vector2)=>Vector2` (screen→custom space) |
|  [02]   | `GestureOptions<T>`  | recognizer config | `GenericOptions` + `from` (seed offset), `threshold`, `preventDefault`, `triggerAllEvents`, `rubberband` (elasticity, default `0.15`), `transform` |
|  [03]   | `CoordinatesConfig`  | coordinate config | `GestureOptions` + `axis: 'x' \| 'y' \| 'lock'`, `bounds: Bounds \| (state)=>Bounds`, `axisThreshold` |
|  [04]   | `UserDragConfig`     | drag config | `filterTaps`, `tapsThreshold`, `bounds: DragBounds` (rect/HTMLElement/ref), `pointer: { buttons, touch, mouse, keys, capture, lock }`, `swipe: { velocity, distance, duration }`, `preventScroll`, `preventScrollAxis`, `delay`, `axisThreshold: Partial<Record<PointerType, number>>`, `keyboardDisplacement` |
|  [05]   | `UserPinchConfig`    | pinch config | `pointer: { touch }`, `scaleBounds`, `angleBounds`, `axis: 'lock'`, `modifierKey: ModifierKey \| ModifierKey[]`, `pinchOnWheel` |
|  [06]   | `UserWheelConfig` / `UserScrollConfig` | axis config | `CoordinatesConfig` for wheel / element scroll |
|  [07]   | `UserMoveConfig` / `UserHoverConfig`   | pointer config | `CoordinatesConfig`/base + `mouseOnly` (hover also `enabled`) |
|  [08]   | `UserGestureConfig`  | composite config | `{ drag?, wheel?, scroll?, move?, pinch?, hover? }` under one object for `useGesture` |
|  [09]   | `Bounds` / `PinchBounds` / `DragBounds` / `ModifierKey` | bound primitives | `{top,bottom,left,right}`; `{min,max}`; `Bounds \| HTMLElement \| { current }`; `'ctrlKey'\|'altKey'\|'metaKey'\|null` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: individual gesture hooks — `useX<EventType, Config>(handler, config?)`; returns `void` when `config.target` is set, else `(...args) => ReactDOMAttributes`
- rail: interaction

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [NOTE] |
| :-----: | :-------- | :------------- | :----- |
|  [01]   | `useDrag(handler, config?: UserDragConfig)`   | drag hook   | pointer drag; state `movement`/`offset`/`swipe`/`tap`/`cancel()` |
|  [02]   | `usePinch(handler, config?: UserPinchConfig)` | pinch hook  | two-pointer + wheel pinch; state `da`/`origin`/`turns`/`cancel()` |
|  [03]   | `useWheel(handler, config?: UserWheelConfig)` | wheel hook  | wheel `delta`/`offset` on an axis |
|  [04]   | `useScroll(handler, config?: UserScrollConfig)` | scroll hook | element scroll position |
|  [05]   | `useMove(handler, config?: UserMoveConfig)`   | move hook   | `pointermove`/`mousemove` (`mouseOnly`) |
|  [06]   | `useHover(handler, config?: UserHoverConfig)` | hover hook  | `pointerenter`/`pointerleave`; `onHover` only (no Start/End) |

[ENTRYPOINT_SCOPE]: composite + factory + engine actions (the tree-shake seam)
- rail: interaction

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [NOTE] |
| :-----: | :-------- | :------------- | :----- |
|  [01]   | `useGesture<HandlerTypes, Config>(handlers: GestureHandlers, config?: UserGestureConfig)` | composite hook | binds any subset of gestures + native handlers in one call; returns the merged binder or `void` |
|  [02]   | `createUseGesture(actions: Action[])` | hook factory | builds a `useGesture`-shaped hook from an explicit action set so unused engines tree-shake |
|  [03]   | `dragAction` / `pinchAction` / `wheelAction` / `scrollAction` / `moveAction` / `hoverAction` | engine actions | the `Action` values passed to `createUseGesture`; each carries one gesture engine + config resolver |
|  [04]   | `registerAction(action)` / `EngineMap` / `ConfigResolverMap` | registry | register an action; the `GestureKey → EngineClass` / `GestureKey → ResolverMap` maps the recognizer reads |
|  [05]   | `rubberbandIfOutOfBounds(position, min, max, constant?)` | util | the elastic-overflow math (from `@use-gesture/core/utils`) for custom bound clamping |

## [04]-[IMPLEMENTATION_LAW]

[GESTURE_TOPOLOGY]:
- return discrimination: with `config.target` (a ref/EventTarget) the hook binds events directly and returns `void`; without it the hook returns a prop-binder `(...args) => ReactDOMAttributes` spread onto the JSX element (the `...args` become `state.args`).
- state vectors are distinct: `values` (raw), `movement` (displacement this gesture), `offset` (cumulative across gestures — seed with `from`), `delta` (since last event), `velocity`, `direction` (`-1`/`0`/`1` per axis), `overflow` (bounds overflow per axis); `intentional` flips true once `threshold`/`axisThreshold` is passed; `first`/`last` bracket the active gesture.
- `transform: (Vector2)=>Vector2` maps screen coordinates into custom space (canvas/world) so `movement`/`offset` arrive already in the target space; `from` seeds `offset`; `bounds` (rect/HTMLElement/ref) clamps `offset` and `rubberband` gives elastic overflow past the bound.
- drag control: `pointer.{buttons,touch,mouse,keys,capture,lock}` selects the input (buttons combo, touch/mouse mode, keyboard drag, `setPointerCapture`, pointer-lock); `swipe.{velocity,distance,duration}` classifies a flick into `swipe: Vector2`; `preventScroll`/`preventScrollAxis`/`delay`/`keyboardDisplacement`/`filterTaps`/`tapsThreshold` tune scroll interplay, delayed start, arrow-key displacement, and tap suppression.
- pinch: `da` carries `[distance, angle]`, `origin` the pivot, `turns` full rotations; `scaleBounds`/`angleBounds` clamp, `axis: 'lock'` locks scale-vs-rotate, `modifierKey`/`pinchOnWheel` map wheel-with-modifier to pinch.
- `cancel()` on `DragState`/`PinchState` aborts in progress; the handler gets a final call with `canceled: true`.
- `eventOptions: { passive: false }` is required whenever a handler must `preventDefault` (wheel/touch); `enabled: false` silences all handlers.
- `createUseGesture([dragAction, pinchAction])` admits only the engines a surface uses; `sideEffects: false` lets the bundler drop the rest — the full `useGesture` pulls every engine.

[STACKING]:
- effect-tier fold: the recognizer feeds an algebra, never a raw mutation — fold `FullGestureState` into a `Data.TaggedEnum` (`CameraGesture`) under `Match.tagsExhaustive`, the folded result driving `AtomBinding` state (`interaction/gesture.md#GESTURE_ALGEBRA`); a state mutation inside a handler is the named defect there.
- canvas/viewport camera: bind `useGesture` to the canvas `target` ref with `eventOptions: { passive: false }`; read `shiftKey` off the state to branch orbit/pan, `pinch.offset[0]` for dolly, `wheel.delta[1]` for zoom, and use `transform` to map deltas into world space with `from`+`bounds`+`rubberband` seeding/clamping the camera offset — the one algebra the `render/glb.md#GLB_VIEWPORT` `ViewportCamera` `RoleBehavior` composes, never a per-surface pointer handler.
- vs vaul: sheet-drag is `vaul`'s own internal recognizer (`vaul.md`); this hook owns canvas/camera and any non-sheet draggable — sibling rows under the gesture concern, not composed.
- tree-shake admission: `createUseGesture([dragAction, pinchAction])` at the module owner keeps the bundle to the used engines; import gesture state types from here (re-exported from `@use-gesture/core/types`) rather than reaching into `@use-gesture/core`.

[LOCAL_ADMISSION]:
- Prefer `config.target` (a React ref) for canvas/external nodes; spread the returned binder when `target` is omitted.
- Read `offset`/`velocity`/`overflow`/`direction`/`swipe` off the typed state; seed with `from`, clamp with `bounds`+`rubberband`, map with `transform` — never recompute deltas from raw events.
- `eventOptions: { passive: false }` when the handler calls `preventDefault` (wheel/touch); otherwise leave passive for scroll performance.
- Use `createUseGesture` with explicit actions to tree-shake unused engines.

[RAIL_LAW]:
- package: `@use-gesture/react`
- owns: React gesture binding for drag/pinch/wheel/scroll/move/hover, the typed `FullGestureState`/config/action surface, and the `createUseGesture` tree-shake seam
- accept: handler callbacks typed against `FullGestureState<Key>`, the full config (`transform`/`from`/`bounds`/`rubberband`/`pointer`/`swipe`/`threshold`/`axis`), `createUseGesture` action sets, `rubberbandIfOutOfBounds`
- reject: raw `onPointerDown`/`onWheel` beside a gesture hook covering the same interaction; a state mutation inside a handler beside the `Match` fold; a parallel pointer-handler type beside the shared recognizer
