# [TS_UI_API_USE_GESTURE_REACT]

`@use-gesture/react` is the continuous-gesture recognizer the `act/gesture` plane composes and the `viewer` camera rows drive: one hook family (`useDrag`/`usePinch`/`useWheel`/`useScroll`/`useMove`/`useHover`, and the combined `useGesture`) that turns raw pointer/wheel/touch/keyboard events into a rich, typed gesture state — cumulative `offset`, per-gesture `movement`, `velocity`, `direction`, `swipe`/`tap` classification, and modifier flags. It is the ANALOG interaction layer react-aria leaves out: react-aria owns discrete accessible press/hover/focus, this owns the pointer math of drag deltas, pinch scale+rotate, and wheel zoom. Each hook returns a `bind` function spread on JSX, OR binds imperatively to a `target` ref — the `target` mode with `eventOptions: { passive: false }` is the ONLY way to `preventDefault` a wheel/touch gesture, because React's synthetic wheel listeners are passive. The gesture state carries `memo` (a handler's return becomes the next event's `state.memo`, the per-gesture accumulator), `from` binds the offset origin to live model state, `bounds`+`rubberband` clamp with elastic overflow, and `transform` maps screen coordinates into world/canvas space. It re-exports the framework-agnostic engine, config, and state types from `@use-gesture/core`, and `createUseGesture([…actions])` tree-shakes a hook down to only the gestures used. It feeds camera control on the `three`/`deck.gl`/`maplibre` canvas and binds through `@effect-atom` state; `vaul` owns its own sheet-drag and never routes through this package.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@use-gesture/react`
- package: `@use-gesture/react` (MIT, © Poimandres)
- module format: ESM + CJS (`dist/use-gesture-react.esm.js` / `.cjs.js`), `.d.ts` under `dist/declarations/src`, `sideEffects: false`; single `.` entry, no subpaths
- runtime target: React DOM (browser); binds pointer/wheel/touch/keyboard events on a DOM node or ref
- peer: `react@>= catalog` (hooks); dep `@use-gesture/core@catalog` — the framework-agnostic engine this package wraps in hooks and re-exports (`@use-gesture/core/utils`, `/actions`, `/types`)
- asset: the continuous-pointer gesture recognizer (drag/pinch/wheel/scroll/move/hover) — the analog interaction layer above raw DOM events, below react-aria's discrete accessible model
- rail: interaction (`act/gesture`; the `viewer` camera-control rows `viewer/geo/project`, `viewer/scene/glb`)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: config family — the per-gesture option algebra (re-exported from `@use-gesture/core/types`)
- rail: interaction
- config layers `GenericOptions` (shared) → `GestureOptions<T>` (per-gesture base) → the specific `DragConfig`/`PinchConfig`/`CoordinatesConfig`. `target` switches bind→imperative; `from`/`bounds`/`transform` are the model-space binding seams.

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CONSUMER_BOUNDARY] |
|:-----: |:----------------------------------------------------------------------------------------------- |:------------- |:---------------------------------------------------------------- |
| [01] | `GenericOptions` (`target`, `window`, `eventOptions`, `enabled`, `transform`) | shared base | `act/gesture` — `target` switches bind→imperative, `eventOptions` sets passive/capture, `transform` maps screen→world `Vector2` |
| [02] | `GestureOptions<T>` (`from`, `threshold`, `preventDefault`, `triggerAllEvents`, `rubberband`) | per-gesture base | `from` sets the offset origin (bind to model state), `rubberband` elastic overflow past `bounds`, `threshold` intent gate |
| [03] | `CoordinatesConfig<Key>` (`axis: 'x'\|'y'\|'lock'`, `bounds`, `axisThreshold`) | xy gestures | drag/wheel/scroll/move; `bounds` clamps `offset`, `axis: 'lock'` locks the dominant axis after threshold |
| [04] | `DragConfig` (`filterTaps`, `tapsThreshold`, `bounds: DragBounds`, `pointer{buttons,touch,mouse,keys,capture,lock}`, `swipe{velocity,distance,duration}`, `preventScroll`, `preventScrollAxis`, `delay`, `axisThreshold`, `keyboardDisplacement`) | drag config | the richest gesture; `pointer.lock` for pointer-lock orbit, `swipe` release classification, `bounds` accepts a ref/`HTMLElement`, `keyboardDisplacement` for arrow-key drag |
| [05] | `PinchConfig` (`pointer.touch`, `scaleBounds`, `angleBounds`, `axis: 'lock'`, `modifierKey`, `pinchOnWheel`) | pinch config | scale + rotate; `pinchOnWheel` folds ctrl+wheel into a pinch, `modifierKey` picks the wheel-scale trigger, `axis: 'lock'` scales OR rotates |
| [06] | `MoveConfig` / `HoverConfig` (`mouseOnly`) / `UserGestureConfig` (`{drag,wheel,scroll,move,pinch,hover}` + `GenericOptions`) / `Bounds` / `PinchBounds` / `DragBounds` / `ModifierKey` | move/hover + combined + geometry | `mouseOnly` skips touch; `UserGestureConfig` is the `useGesture` object with per-gesture sub-configs under shared options |

[PUBLIC_TYPE_SCOPE]: state family — the typed payload every handler receives
- rail: interaction
- `FullGestureState<Key>` is the full handler arg: `SharedGestureState` (cross-gesture flags) ∪ the per-gesture state. `CommonGestureState` is the core — `offset`/`movement`/`velocity`/`memo` are the load-bearing fields.

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CONSUMER_BOUNDARY] |
|:-----: |:----------------------------------------------------------------------------------------------- |:------------- |:---------------------------------------------------------------- |
| [01] | `SharedGestureState` (`dragging`/`wheeling`/`moving`/`hovering`/`scrolling`/`pinching`, `touches`, `down`, `pressed`, `buttons`, `shiftKey`/`altKey`/`metaKey`/`ctrlKey`, `locked`) | cross-gesture flags | the active-per-gesture booleans + live modifier state on every handler; branch behavior on `shiftKey`/`down` |
| [02] | `CommonGestureState` (`movement`, `offset`, `delta`, `distance`, `velocity`, `direction`, `values`, `initial`, `first`, `last`, `active`, `intentional`, `overflow`, `memo`, `args`, `elapsedTime`, `timeStamp`, `event`, `target`) | core payload | THE handler state — `offset` cumulative across gestures, `movement` this-gesture, `velocity`+`direction` for momentum, `first`/`last` lifecycle, `memo` the accumulator |
| [03] | `CoordinatesState` (`+ axis`, `xy`) / `DragState` (`+ tap`, `swipe`, `canceled`, `cancel()`) | xy + drag state | `cancel()` aborts mid-gesture, `swipe: Vector2`/`tap` classify the release |
| [04] | `PinchState` (`+ da: [distance, angle]`, `origin`, `turns`, `axis: 'scale'\|'angle'`, `canceled`, `cancel()`) | pinch state | `da` the distance/angle raw values, `origin` the pinch center for zoom-to-cursor, `turns` full rotations |
| [05] | `FullGestureState<Key>` / `State` / `EventTypes` / `IngKey` / `Vector2` / `WebKitGestureEvent` | state index | `FullGestureState<'drag'>` is the typed handler arg; `EventTypes` maps each gesture to its DOM event union; `Vector2 = [number, number]` |

[PUBLIC_TYPE_SCOPE]: handler family — the handler signatures and bind result
- rail: interaction

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CONSUMER_BOUNDARY] |
|:-----: |:----------------------------------------------------------------------------------------------- |:------------- |:---------------------------------------------------------------- |
| [01] | `Handler<Key, EventType>` = `(state: FullGestureState<Key>) => any \| void` | handler sig | a returned value becomes the next event's `state.memo`; `void` for no accumulation |
| [02] | `GestureHandlers<T>` / `UserHandlers` (`onDrag`/`onDragStart`/`onDragEnd`/`onPinch`/…/`onHover`) / `NativeHandlers` / `AnyHandlerEventTypes` / `InternalHandlers` | handler maps | the `useGesture` handler object; `Start`/`End` variants fire on gesture edges; native DOM handlers pass through |
| [03] | `ReactDOMAttributes` | bind result | the prop object the returned `bind` function produces — spread onto the JSX element |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: per-gesture hooks — one recognizer per interaction
- rail: interaction
- each is `useX(handler, config?)` returning a `bind` function (spread on JSX) OR `void` when `config.target` is set (imperative native binding).

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY] |
|:-----: |:---------------------------------------------------------------------------------------------- |:------------- |:---------------------------------------------------------- |
| [01] | `useDrag(handler, config?: UserDragConfig)` | drag | pan / reorder / free-drag / pointer-lock orbit; `filterTaps`+`swipe` classify, `bounds`+`rubberband` clamp |
| [02] | `usePinch(handler, config?: UserPinchConfig)` | pinch | zoom + rotate; `pinchOnWheel` folds ctrl+wheel, `origin` gives zoom-to-cursor, `scaleBounds`/`angleBounds` clamp |
| [03] | `useWheel(handler, config?: UserWheelConfig)` / `useScroll(handler, config?: UserScrollConfig)` | wheel / scroll | wheel-zoom, scroll-driven parallax; `target`+non-passive to block page scroll |
| [04] | `useMove(handler, config?: UserMoveConfig)` / `useHover(handler, config?: UserHoverConfig)` | move / hover | cursor-follow, hover-intent; `mouseOnly` skips touch |

[ENTRYPOINT_SCOPE]: combined hook + tree-shake factory
- rail: interaction

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY] |
|:-----: |:---------------------------------------------------------------------------------------------- |:------------- |:---------------------------------------------------------- |
| [01] | `useGesture(handlers: GestureHandlers, config?: UserGestureConfig)` | combined | `viewer` — ONE bind for drag+pinch+wheel+move on the canvas; shared options + per-gesture sub-configs; the canonical camera hook |
| [02] | `createUseGesture(actions: Action[])` | tree-shake | build a `useGesture` variant with ONLY the imported actions (`createUseGesture([dragAction, pinchAction])`) — drops the unused engines from the bundle |

[ENTRYPOINT_SCOPE]: actions + engine registry + math util (re-exported from `@use-gesture/core`)
- rail: interaction

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY] |
|:-----: |:---------------------------------------------------------------------------------------------- |:------------- |:---------------------------------------------------------- |
| [01] | `dragAction` / `pinchAction` / `wheelAction` / `scrollAction` / `moveAction` / `hoverAction` | tree-shake units | the engine registrations `createUseGesture` composes — import only the gestures a surface uses |
| [02] | `registerAction(action)` / `EngineMap` / `ConfigResolverMap` | engine registry | register a custom action; the gesture-key→engine and →resolver maps |
| [03] | `rubberbandIfOutOfBounds(position, min, max, constant?)` | math util | the elastic-overflow function behind `rubberband`; reuse for a custom clamp on the same easing |

## [04]-[IMPLEMENTATION_LAW]

[GESTURE_TOPOLOGY]:
- Two binding modes. No `target` → the hook returns a `bind` function spread on JSX (`<div {...bind()} />`), attaching through React's synthetic (passive) event system. Set `config.target` to a ref/DOM node → the hook returns `void` and binds natively with `config.eventOptions`. The `target` mode with `eventOptions: { passive: false }` is the ONLY way to `preventDefault` a wheel/touch gesture — React's synthetic wheel/touch listeners are passive, so `preventDefault` through JSX props is a silent no-op. Canvas zoom/pan that must block page scroll binds to the canvas ref with `{ target: ref, eventOptions: { passive: false }, preventDefault: true }` (or `preventScroll` on drag).
- `memo` is the per-gesture accumulator: a handler's return value becomes `state.memo` on the next event of the same gesture. Capture the origin on `first` (`if (first) memo = [cam.x, cam.y]`), apply the `movement` delta thereafter, `return memo` — this replaces an external mutable ref for start state and stays correct under React's render model.
- `from` sets where `offset` starts so a new gesture continues from the model's live value — `from: () => [viewState.longitude, viewState.latitude]` binds the drag offset to the current camera, so consecutive drags accumulate. `bounds` clamps `offset` (a ref/`HTMLElement` for drag, a box for coordinates), and `rubberband` gives elastic overflow past the bound with `overflow` reported per axis.
- `transform` maps the raw screen `Vector2` into world/canvas space before the handler sees `movement`/`offset` — the seam that keeps the handler in domain coordinates. `velocity` + `direction` + `swipe` classify the release for momentum/fling; `axis: 'lock'` locks the dominant axis after `threshold`; `intentional`+`triggerAllEvents` separate below-threshold noise from a committed gesture.

[STACKS_WITH]:
- `react-aria` (`act/gesture` sibling, `libs/typescript/ui/.api/react-aria.md`): the interaction-class division. react-aria owns DISCRETE accessible interactions — press, hover-intent, focus, keyboard, ARIA — this package owns CONTINUOUS analog gestures — drag deltas, pinch scale/rotate, wheel zoom, swipe momentum. They compose on one element (a draggable that is also keyboard-operable) by binding both, each owning its class; never route a discrete press through `useDrag` or accessible focus through a raw pointer handler.
- `three` / `@deck.gl/core` / `maplibre-gl` (`viewer` rows, `libs/typescript/ui/.api/three.md`): `useGesture` is the camera-control input for the spatial canvas — drag→orbit/pan, pinch/wheel→zoom — feeding the `three` camera or the deck.gl/maplibre `viewState`. It binds to the canvas ref with non-passive options so it owns the wheel; the renderers ship no gesture layer here.
- `@effect-atom/atom-react` (`atom/binding`, `libs/typescript/ui/.api/effect-atom-atom-react.md`): camera/offset state is an atom — `from` reads `useAtomValue(cameraAtom)` for the origin, the handler dispatches through `useAtomSet(cameraAtom)`, so the view state is undoable/URL-syncable like any other atom, never a local `useState` the gesture mutates.
- `react` (universal spine, `libs/typescript/ui/.api/react.md`): `useRef` supplies the `target` node; a high-frequency gesture that drives layout wraps the atom write in `startTransition` so the pointer stream stays responsive while the non-urgent camera commit deprioritizes.
- `vaul` (`view/compose` sibling, `libs/typescript/ui/.api/vaul.md`): the seam — vaul owns its OWN drag-to-dismiss internally and does NOT use this package. A sheet's drag is vaul's; `@use-gesture` is the canvas/camera and free-drag recognizer. Never double-bind a drag on a vaul surface.

[LOCAL_ADMISSION]:
- One `useGesture` (or a specific `useX`) per interactive surface; a new gesture is a handler key or sub-config, never a second hook stacked on the same element.
- Use `target` + `eventOptions: { passive: false }` for any gesture that must `preventDefault` (canvas wheel/zoom, drag that blocks scroll); the JSX-spread bind cannot preventDefault a passive listener.
- Carry per-gesture start state in `memo`, bind the offset origin with `from`, clamp with `bounds`+`rubberband`, and map to domain space with `transform` — never an external mutable ref for accumulation.
- Tree-shake with `createUseGesture([…actions])` when only a subset of gestures is used; the full `useGesture` pulls every engine.
- Continuous analog gestures only; discrete accessible interactions are react-aria, sheet-drag is vaul.

[RAIL_LAW]:
- Package: `@use-gesture/react`
- Owns: the continuous-pointer recognizers (`useDrag`/`usePinch`/`useWheel`/`useScroll`/`useMove`/`useHover`), the combined `useGesture`, the `createUseGesture` tree-shake factory, the action registry (`dragAction`…`hoverAction`, `registerAction`), and the gesture state/config/handler type algebra re-exported from `@use-gesture/core`
- Accept: the bind-function or `target`-ref binding mode, `memo` accumulation, `from`/`bounds`/`rubberband`/`transform` model-space binding, `eventOptions: { passive: false }` for preventable gestures, atom-backed offset/camera state, react-aria as the discrete-interaction peer
- Reject: a discrete press/focus routed through a pointer gesture (react-aria owns it), a JSX-spread bind where `preventDefault` is needed (use `target`), an external mutable ref where `memo` fits, a second gesture hook on one element, a gesture on a vaul sheet (vaul owns its drag), and the full `useGesture` where `createUseGesture` does tree-shake
