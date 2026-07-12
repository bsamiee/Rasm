# [TS_UI_API_MOTION]

[PACKAGE_SURFACE]:
- package: `motion` · license `MIT`
- module: dual ESM/CJS via conditional `exports`; subpaths `.` (vanilla DOM, full hybrid), `./mini` (vanilla WAAPI-only), `./react` (full hybrid React), `./react-mini` (WAAPI-only React), `./react-m` (lazy `m.*` tag proxies), `./react-client` (RSC client boundary), `./debug` (`recordStats`). Every subpath is a re-export shim onto the same- — `motion` is the canonical name, `framer-motion` never appears in a manifest beside it.
- asset: `sideEffects: false`; deps `framer-motion` + `tslib`; peers `react`/`react-dom` 18||19 all OPTIONAL — the vanilla entries run with zero React.
- runtime: hybrid engine — spring/keyframe generation on rAF with WAAPI/compositor offload where the value allows; browser only.
- plane: `plane:runtime` (W4 `ui`); folder-local to `ui`.
- rail: `system/act` continuous motion — springs, scroll-linked values, layout morphs, presence, the view-transition builder.

`motion` owns the CONTINUOUS plane of `system/act`: physical springs, pointer-following values, scroll-linked transforms, layout/shared-element morphs, exit choreography, and imperative sequences. The DISCRETE plane stays where `system/act#MOTION_ROWS` put it — `tw-animate-css` class rows through the RAC `entering:`/`exiting:` variants own simple enter/exit, and a surface graduates to `motion` only when it needs physics, interruption, values, or layout — the two compose by strength per surface, never rival on one. The engine is cost-laddered by entry point: the mini entries are a ~2.3kB WAAPI `animate`, `LazyMotion` + `m.*` + a feature bundle is the mid tier, and the full `motion.*` proxy is the top tier — the ladder is a real decision row, not a default import.

## [01]-[ENTRY_SPLIT]

| [INDEX] | [ENTRY]                                  | [EXPORTS]                                                                                                                                                                                       | [TIER]                                                      |
| :-----: | :--------------------------------------- | :---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :---------------------------------------------------------- |
|  [01]   | `motion` (root)                          | vanilla `animate` (all overloads) `scroll` `inView` `press` `hover` `stagger` `frame` `transform` `mix` `wrap` `spring` `delay` (SECONDS) `motionValue` `animateView`                           | vanilla full — drives DOM/three/canvas without React        |
|  [02]   | `motion/mini`                            | `animate` (single-target WAAPI) + `animateSequence` only                                                                                                                                        | vanilla featherweight (~2.3kB WAAPI)                        |
|  [03]   | `motion/react`                           | everything in [02]-[04] below: `motion`/`m` proxies, `AnimatePresence`, `LayoutGroup`, `MotionConfig`, `LazyMotion`+bundles, the hook family, vanilla re-exports (`delay` in MILLISECONDS here) | React full hybrid                                           |
|  [04]   | `motion/react-mini`                      | `useAnimate` ONLY (WAAPI single-target scope animate)                                                                                                                                           | React featherweight — no components, no gestures, no layout |
|  [05]   | `motion/react-m` / `motion/react-client` | `m.*` lazy tags / RSC client-boundary tags                                                                                                                                                      | the `LazyMotion` payload split and the RSC seam             |

`animateView` rides entries [01] and [03], never the mini entries. The `delay` unit flip (seconds vanilla, milliseconds react) is a live boundary trap — pin the entry before citing a timeout.

## [02]-[MOTION_VALUES]

The `MotionValue` is the state cell React never re-renders for: it subscribes, transforms, and writes to the DOM outside reconciliation.

| [INDEX] | [SURFACE]                                                                                                   | [FAMILY]     | [CAPABILITY]                                                                              |
| :-----: | :---------------------------------------------------------------------------------------------------------- | :----------- | :---------------------------------------------------------------------------------------- |
|  [01]   | `motionValue(init)` / `useMotionValue(initial)`                                                             | value cell   | the render-free animated cell; bound via `style={{ x }}`                                  |
|  [02]   | `useSpring(source, options?)`                                                                               | spring bind  | spring-follows a `MotionValue`, number, or unit string                                    |
|  [03]   | `useTransform(input, inputRange, outputRange, options?)` / `useTransform(() => expr)`                       | derive       | mapped or computed derived value — the fold over source values                            |
|  [04]   | `useScroll({ container?, target?, offset? })` → `{ scrollX, scrollY, scrollXProgress, scrollYProgress }`    | scroll link  | scroll-linked values — the JS `ScrollTimeline` the folder rejects as a second engine      |
|  [05]   | `useVelocity(value)` / `useTime()` / `useAnimationFrame(cb)`                                                | derive/clock | velocity tracking, elapsed-time value, per-frame callback                                 |
|  [06]   | `useMotionValueEvent(value, event, cb)`                                                                     | event seam   | `change`/`animationStart`/`animationComplete` without an effect                           |
|  [07]   | `transform` / `mix` / `wrap` / `spring(options)` / `stagger(duration?, {startDelay, from, ease})` / `frame` | math/batch   | range mapping, mixers, spring keyframe generator, stagger `delay` option, the rAF batcher |

```ts contract
// Transition vocabulary: type discriminant + spring physics. visualDuration overrides duration; stiffness/damping/mass override bounce/duration when set.
type Transition = { type?: "spring" | "tween" | "keyframes" | "inertia" | "decay"; duration?: number; visualDuration?: number
  stiffness?: number; damping?: number; mass?: number; bounce?: number; velocity?: number; restSpeed?: number; restDelta?: number
  ease?: "linear" | "easeIn" | "easeOut" | "easeInOut" | "circIn" | "circOut" | "circInOut" | "backIn" | "backOut" | "backInOut" | "anticipate" | [number, number, number, number] | ((t: number) => number) }
```

## [03]-[COMPONENT_PLANE]

- `motion.*` / `m.*` — the proxy component factories (`motion.div`, `motion.create(Component)`): `initial`/`animate`/`exit` targets or variant labels, `variants`, `transition`.
- Gesture props (verified 12.x): `whileHover` `whileTap` `whileDrag` `whileFocus` `whileInView`; `drag: boolean | "x" | "y"`, `dragConstraints` (box or ref), `dragElastic`, `onPan(event, info)`; `useDragControls()` for handle-initiated drag.
- Layout props: `layout: boolean | "position" | "size" | "preserve-aspect"`, `layoutId` (shared-element morph across unrelated trees — no wrapper component exists or is needed), `layoutRoot`, `layoutScroll`; `LayoutGroup` scopes layout-change propagation.
- `AnimatePresence` (`mode: "sync" | "wait" | "popLayout"`, `initial`, `onExitComplete`, `propagate`) — exit animation for unmounting children; `usePresence()`/`useIsPresent()` for manual exit completion.
- `MotionConfig` (`transition`, `reducedMotion: "user" | "always" | "never"`, `nonce`) — the subtree policy owner; `useReducedMotion()` reads the same signal `system/act` gates on via `matchMedia`.
- `LazyMotion` + `domAnimation`/`domMax` feature bundles with `strict` — the mid-tier: `m.*` tags render, features load async.
- Imperative: `animate(target, keyframes, options)` (element/value/object/sequence overloads), `useAnimate()` → `[scope, animate]` (scoped selectors, auto-cleanup), vanilla `scroll(onScrollOrAnimation, options)`, `inView(target, onStart, options)`, `press`/`hover` recognizers.

## [04]-[VIEW_TRANSITIONS_BUILDER]

`animateView(update, options?)` is the typed spring-physics layer over `document.startViewTransition` — open-source in core, promoted out of Motion+; the React `<AnimateView>` component remains membership-gated and is REJECTED.

```ts contract
declare function animateView(update: () => void | Promise<void>, options?: AnimationOptions & { interrupt?: "wait" | "immediate" }): ViewTransitionBuilder
// Builder targets resolve selectors/elements and manage view-transition-name automatically; .add(a, b) pairs a shared-element morph.
interface ViewTransitionBuilder { add(subject: Element | string, newSubject?: Element | string): this
  crop(): this; group(): this; class(): this; layout(): this; enter(): this; exit(): this; new(): this; old(): this; updateTarget(): this; then(onResolve: () => void): Promise<void> }
```

Interruption defaults to queued (`"wait"`); `"immediate"` preempts — the interruption handling `startViewTransition` alone lacks.

## [05]-[INTEGRATION]

[STACK: `system/act#MOTION_ROWS` — the discrete/continuous split] — `Motion` rows (tw-animate classes on RAC `entering:`/`exiting:` variants) stay the owner for simple presence transitions inside RAC overlays; `motion` takes a surface when it needs springs, interruption, values, drag, or layout. One surface, one owner: a RAC overlay animating through `Motion.overlay` never also mounts `AnimatePresence` around the same element.

[STACK: `system/act#DOCUMENT_RAIL` + React `<ViewTransition>` (`.api/react.md`)] — three tiers compose by strength: `Transition.run`'s native `startViewTransition` rail owns capability-gated document swaps; `animateView` upgrades a swap that needs spring timing, per-subject targeting, or interruption policy; the canary `<ViewTransition>` element (react.md canary rows) owns React-tree-driven per-element transitions activated inside `startTransition`/`Suspense`. Per surface exactly one tier fires — never `animateView` wrapping a commit that a `<ViewTransition>` boundary already animates.

[STACK: the atom store (`.api/effect-atom-atom-react.md`)] — a `MotionValue` is presentation state OUTSIDE both React and the atom: domain state lives in the atom, the component derives an animation TARGET from the atom value, and the engine interpolates. A `MotionValue` mirrored into an atom (or `useState`) is the render-thrash defect; `useMotionValueEvent` is the read seam when a threshold crossing must become real state.

[BOUNDARY: gestures (`.api/use-gesture-react.md`, `.api/vaul.md`)] — `system/act#CONTINUOUS_OWNER` keeps `@use-gesture` as the raw gesture recognizer; `motion`'s `drag`/`whileTap` are admitted only on elements the engine itself animates, and a use-gesture binding plus motion `drag` on one node is the double-bind defect. Sheet drag stays vaul's own physics. `useReducedMotion`/`MotionConfig reducedMotion:"user"` mirror the `matchMedia` reduced-motion law every act row already obeys.

[BOUNDARY: scroll engines] — `useScroll`/`useTransform` is the ONE scroll-animation engine (four-browser parity); CSS `animation-timeline` rows are decorative progressive enhancement per the act charter, and raw `ScrollTimeline`/polyfills never enter.

## [06]-[RAIL_LAW]

- Owns: continuous motion — motion values and their derivation graph, spring/tween/inertia physics, `motion.*`/`m.*` animated elements, gesture-while states and engine-owned drag, layout/`layoutId` shared-element morphs, `AnimatePresence` exit choreography, imperative `animate`/`useAnimate` sequences, scroll/in-view linkage, and the `animateView` View Transitions builder.
- Accept: entry-point cost laddering (mini → `LazyMotion`+`m` → full) chosen per surface; targets derived from atom state with the engine interpolating; `MotionConfig` for subtree transition/reduced-motion policy; `layoutId` for shared-element morphs; `animateView` where a document swap needs physics or interruption; vanilla entries for non-React canvases.
- Reject: `framer-motion` as a manifest name beside `motion`; the paid `<AnimateView>`/Motion+ surface; a second scroll engine (`ScrollTimeline` polyfill) beside `useScroll`; `AnimatePresence` duplicating a `Motion` row on one surface; motion `drag` double-bound with a use-gesture handler or vaul sheet; `MotionValue`s mirrored into React/atom state; mini entries expected to carry `animateView`, components, or gestures.
