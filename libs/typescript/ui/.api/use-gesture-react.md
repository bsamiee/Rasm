# [TS_UI_API_USE_GESTURE_REACT]

`@use-gesture/react` recognizes continuous pointer, wheel, touch, and keyboard input as typed gesture state — cumulative `offset`, per-gesture `movement`, `velocity`, `direction`, and swipe/tap classification — the analog interaction layer `act/gesture` composes and the `viewer` camera rows drive. It wraps `@use-gesture/core` as React hooks and re-exports the engine's config, state, and handler type algebra; `react-aria` owns discrete accessible interaction and `vaul` owns sheet-drag, neither routing through here.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@use-gesture/react`
- package: `@use-gesture/react` (MIT)
- module: ESM + CJS (`dist/use-gesture-react.esm.js` / `.cjs.js`), `.d.ts` under `dist/declarations/src`, `sideEffects: false`; single `.` entry, no subpaths
- runtime: React DOM in the browser; peer `react`, dep `@use-gesture/core` — the framework-agnostic engine this package wraps as hooks and re-exports (`/utils`, `/actions`, `/types`); binds pointer/wheel/touch/keyboard events on a DOM node or ref
- rail: interaction (`act/gesture`; the `viewer` camera-control rows `viewer/geo/project`, `viewer/scene/glb`)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: config family — the per-gesture option algebra re-exported from `@use-gesture/core/types`

`GenericOptions` (shared) layers into `GestureOptions<T>` (per-gesture base) and the specific `DragConfig`/`PinchConfig`/`CoordinatesConfig`; `target` switches bind→imperative, `from`/`bounds`/`transform` bind to model space, `Bounds`/`PinchBounds`/`DragBounds` shape the clamps, and `ModifierKey` names the modifier union.

| [INDEX] | [SYMBOL]                       | [CAPABILITY]                                                                            |
| :-----: | :----------------------------- | :-------------------------------------------------------------------------------------- |
|  [01]   | `GenericOptions` (shared)      | `target` `window` `eventOptions` `enabled` `transform`                                  |
|  [02]   | `GestureOptions<T>` (base)     | `from` `threshold` `preventDefault` `triggerAllEvents` `rubberband`                     |
|  [03]   | `CoordinatesConfig<Key>`       | `axis: 'x'\|'y'\|'lock'` `bounds` `axisThreshold`                                       |
|  [04]   | `DragConfig` (pointer)         | `pointer{buttons,touch,mouse,keys,capture,lock}` `bounds: DragBounds`                   |
|  [05]   | `DragConfig` (taps/swipe)      | `filterTaps` `tapsThreshold` `swipe{velocity,distance,duration}`                        |
|  [06]   | `DragConfig` (scroll/kbd)      | `preventScroll` `preventScrollAxis` `delay` `axisThreshold` `keyboardDisplacement`      |
|  [07]   | `PinchConfig`                  | `pointer.touch` `scaleBounds` `angleBounds` `axis: 'lock'` `modifierKey` `pinchOnWheel` |
|  [08]   | `MoveConfig` / `HoverConfig`   | `mouseOnly`                                                                             |
|  [09]   | `UserGestureConfig` (combined) | `{drag,wheel,scroll,move,pinch,hover}` sub-configs + `GenericOptions`                   |

[PUBLIC_TYPE_SCOPE]: state family — the typed payload every handler receives

`FullGestureState<Key>` unions `SharedGestureState` (cross-gesture flags) with the per-gesture state; `CommonGestureState` is the core, `offset`/`movement`/`velocity`/`memo` its load-bearing fields.

| [INDEX] | [SYMBOL]                         | [CAPABILITY]                                                                     |
| :-----: | :------------------------------- | :------------------------------------------------------------------------------- |
|  [01]   | `SharedGestureState` (active)    | `dragging` `wheeling` `moving` `hovering` `scrolling` `pinching`                 |
|  [02]   | `SharedGestureState` (pointer)   | `touches` `down` `pressed` `buttons` `locked`                                    |
|  [03]   | `SharedGestureState` (mods)      | `shiftKey` `altKey` `metaKey` `ctrlKey`                                          |
|  [04]   | `CommonGestureState` (motion)    | `movement` `offset` `delta` `distance` `velocity` `direction` `values` `initial` |
|  [05]   | `CommonGestureState` (cycle)     | `first` `last` `active` `intentional` `overflow` `memo` `args`                   |
|  [06]   | `CommonGestureState` (meta)      | `elapsedTime` `timeStamp` `event` `target`                                       |
|  [07]   | `CoordinatesState` / `DragState` | `+ axis` `xy` / `+ tap` `swipe` `canceled` `cancel()`                            |
|  [08]   | `PinchState`                     | `+ da: [distance, angle]` `origin` `turns` `axis: 'scale'\|'angle'`              |
|  [09]   | `FullGestureState<Key>`          | typed handler arg; `State`/`EventTypes` map each gesture to its DOM event union  |
|  [10]   | `Vector2` / `WebKitGestureEvent` | `Vector2 = [number, number]`; `IngKey` the active-gesture key                    |

[PUBLIC_TYPE_SCOPE]: handler family — the handler signatures and bind result

| [INDEX] | [SYMBOL]                              | [CAPABILITY]                                                                          |
| :-----: | :------------------------------------ | :------------------------------------------------------------------------------------ |
|  [01]   | `Handler<Key, EventType>`             | `(state: FullGestureState<Key>) => any \| void`; return becomes next `state.memo`     |
|  [02]   | `GestureHandlers<T>` / `UserHandlers` | `onDrag`/`onDragStart`/`onDragEnd`/`onPinch`/…/`onHover`; `Start`/`End` fire on edges |
|  [03]   | `NativeHandlers`                      | native DOM passthrough; `AnyHandlerEventTypes`/`InternalHandlers` the engine maps     |
|  [04]   | `ReactDOMAttributes`                  | the prop object `bind()` returns, spread onto the JSX element                         |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: per-gesture hooks — one recognizer per interaction, each `useX(handler, config?)` returning a `bind` function spread on JSX, or `void` when `config.target` is set for imperative native binding

| [INDEX] | [SURFACE]                               | [SHAPE] | [CAPABILITY]                                                                  |
| :-----: | :-------------------------------------- | :------ | :---------------------------------------------------------------------------- |
|  [01]   | `useDrag(handler, UserDragConfig?)`     | drag    | pan/reorder/orbit; `filterTaps`/`swipe` classify, `bounds`/`rubberband` clamp |
|  [02]   | `usePinch(handler, UserPinchConfig?)`   | pinch   | zoom + rotate; `pinchOnWheel` folds ctrl+wheel, `origin` for zoom-to-cursor   |
|  [03]   | `useWheel(handler, UserWheelConfig?)`   | wheel   | wheel-zoom; a non-passive `target` blocks page scroll                         |
|  [04]   | `useScroll(handler, UserScrollConfig?)` | scroll  | scroll-driven parallax                                                        |
|  [05]   | `useMove(handler, UserMoveConfig?)`     | move    | cursor-follow; `mouseOnly` skips touch                                        |
|  [06]   | `useHover(handler, UserHoverConfig?)`   | hover   | hover-intent                                                                  |

[ENTRYPOINT_SCOPE]: combined hook and tree-shake factory

| [INDEX] | [SURFACE]                                         | [SHAPE]    | [CAPABILITY]                                                           |
| :-----: | :------------------------------------------------ | :--------- | :--------------------------------------------------------------------- |
|  [01]   | `useGesture(GestureHandlers, UserGestureConfig?)` | combined   | one bind for drag+pinch+wheel+move; shared + per-gesture sub-configs   |
|  [02]   | `createUseGesture(Action[])`                      | tree-shake | a `useGesture` with only the imported actions, dropping unused engines |

[ENTRYPOINT_SCOPE]: actions, engine registry, and math util — re-exported from `@use-gesture/core`

[TREE_SHAKE_UNITS]: `dragAction` `pinchAction` `wheelAction` `scrollAction` `moveAction` `hoverAction` — the engine registrations `createUseGesture` composes; import only the gestures a surface uses.

| [INDEX] | [SURFACE]                                                | [SHAPE]   | [CAPABILITY]                                                   |
| :-----: | :------------------------------------------------------- | :-------- | :------------------------------------------------------------- |
|  [01]   | `registerAction(action)`                                 | registry  | registers a custom action into the engine map                  |
|  [02]   | `EngineMap` / `ConfigResolverMap`                        | registry  | the gesture-key→engine and gesture-key→resolver maps           |
|  [03]   | `rubberbandIfOutOfBounds(position, min, max, constant?)` | math util | elastic-overflow behind `rubberband`; reuse for a custom clamp |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Two binding modes resolve on `config.target`: absent, the hook returns a `bind` function spread on JSX (`<div {...bind()} />`) through React's passive synthetic events; set to a ref/DOM node, the hook returns `void` and binds natively under `config.eventOptions`.
- `eventOptions: { passive: false }` on a `target` binding is the only path to `preventDefault` a wheel/touch gesture — React's synthetic wheel/touch listeners are passive, so `preventDefault` through JSX props is a silent no-op; canvas zoom/pan that must block page scroll binds the canvas ref with `{ target: ref, eventOptions: { passive: false }, preventDefault: true }` (or `preventScroll` on drag).
- `memo` accumulates per gesture: a handler's return becomes `state.memo` on the next event of the same gesture; capture the origin on `first`, apply the `movement` delta thereafter, and `return memo`, replacing an external mutable ref for start state under React's render model.
- `from` sets where `offset` starts so a new gesture continues from the model's live value (`from: () => [viewState.longitude, viewState.latitude]`), consecutive drags accumulating; `bounds` clamps `offset` (a ref/`HTMLElement` for drag, a box for coordinates) and `rubberband` gives elastic overflow past the bound, reported per axis in `overflow`.
- `transform` maps the raw screen `Vector2` into world/canvas space before the handler sees `movement`/`offset`, keeping the handler in domain coordinates; `velocity`+`direction`+`swipe` classify the release for momentum, `axis: 'lock'` locks the dominant axis after `threshold`, `intentional`+`triggerAllEvents` separate below-threshold noise from a committed gesture, and `cancel()` aborts a gesture mid-stream.

[STACKING]:
- `react-aria` (`act/gesture` sibling, `libs/typescript/ui/.api/react-aria.md`): react-aria owns discrete accessible interaction — press, hover-intent, focus, keyboard, ARIA — this package owns continuous analog gestures — drag deltas, pinch scale/rotate, wheel zoom, swipe momentum; they compose on one element by binding both, each owning its class, never a discrete press routed through `useDrag` nor accessible focus through a raw pointer handler.
- `three` / `@deck.gl/core` / `maplibre-gl` (`viewer` rows, `libs/typescript/ui/.api/three.md`): `useGesture` is the camera-control input for the spatial canvas — drag→orbit/pan, pinch/wheel→zoom — feeding the `three` camera or the deck.gl/maplibre `viewState`, bound to the canvas ref with non-passive options so it owns the wheel; the renderers ship no gesture layer here.
- `@effect-atom/atom-react` (`atom/binding`, `libs/typescript/ui/.api/effect-atom-atom-react.md`): camera/offset state is an atom — `from` reads `useAtomValue(cameraAtom)` for the origin and the handler dispatches through `useAtomSet(cameraAtom)`, so the view state is undoable and URL-syncable like any other atom, never a local `useState` the gesture mutates.
- `react` (universal spine, `libs/typescript/ui/.api/react.md`): `useRef` supplies the `target` node, and a high-frequency gesture driving layout wraps the atom write in `startTransition` so the pointer stream stays responsive while the non-urgent camera commit deprioritizes.
- `vaul` (`view/compose` sibling, `libs/typescript/ui/.api/vaul.md`): vaul owns its own drag-to-dismiss internally and never routes through this package; `@use-gesture` is the canvas/camera and free-drag recognizer, never double-bound on a vaul surface.

[LOCAL_ADMISSION]:
- One `useGesture` (or a specific `useX`) per interactive surface; a new gesture is a handler key or sub-config, never a second hook stacked on the same element.
- `target` with `eventOptions: { passive: false }` binds any gesture that must `preventDefault` — canvas wheel/zoom, drag that blocks scroll — since the JSX-spread bind cannot preventDefault a passive listener.
- `memo` carries per-gesture start state, `from` binds the offset origin, `bounds`+`rubberband` clamp, and `transform` maps to domain space — never an external mutable ref for accumulation.
- `createUseGesture([…actions])` tree-shakes when only a subset of gestures is used; the full `useGesture` pulls every engine.
- Continuous analog gestures bind here; discrete accessible interaction is react-aria and sheet-drag is vaul.

[RAIL_LAW]:
- Package: `@use-gesture/react`
- Owns: the continuous-pointer recognizers (`useDrag`/`usePinch`/`useWheel`/`useScroll`/`useMove`/`useHover`), the combined `useGesture`, the `createUseGesture` tree-shake factory, the action registry (`dragAction`…`hoverAction`, `registerAction`), and the gesture state/config/handler type algebra re-exported from `@use-gesture/core`
- Accept: the bind-function or `target`-ref binding mode, `memo` accumulation, `from`/`bounds`/`rubberband`/`transform` model-space binding, `eventOptions: { passive: false }` for preventable gestures, atom-backed offset/camera state, react-aria as the discrete-interaction peer
- Reject: a discrete press/focus routed through a pointer gesture, a JSX-spread bind where `preventDefault` is needed, an external mutable ref where `memo` fits, a second gesture hook on one element, a gesture on a vaul sheet, and the full `useGesture` where `createUseGesture` tree-shakes
