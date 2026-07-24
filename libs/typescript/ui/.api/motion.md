# [TS_UI_API_MOTION]

`motion` owns the continuous plane of `system/act`: physical springs, pointer-following motion values, scroll-linked transforms, layout and shared-element morphs, exit choreography, and imperative sequences. Cost laddering is a per-surface decision — the WAAPI mini, the `LazyMotion` + `m.*` mid tier, the full `motion.*` proxy — never a default import.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `motion`
- package: `motion` (MIT)
- module: dual ESM/CJS conditional `exports`; subpaths `.` `./mini` `./react` `./react-mini` `./react-m` `./react-client` `./debug`, each a re-export shim on one core (`framer-motion` is the vendored engine, never a manifest name beside `motion`)
- runtime: browser hybrid engine — rAF spring/keyframe generation, offloading eligible values to WAAPI or the compositor; `react`/`react-dom` are optional peers, so vanilla entries run React-free
- plane: `plane:runtime` (W4 `ui`), folder-local to `ui`
- rail: continuous motion — springs, scroll-linked values, layout morphs, presence, and the view-transition builder

## [02]-[ENTRY_SPLIT]

Every entry re-exports one core; the engine is cost-laddered by entry. `animateView` rides `.`/`./react` only, and `delay` flips units — SECONDS on vanilla, MILLISECONDS on React — so pin the entry before citing a timeout.

- [01]-[ROOT] `motion`: vanilla full, drives DOM/three/canvas React-free — `animate` `scroll` `inView` `press` `hover` `stagger` `frame` `transform` `mix` `wrap` `spring` `delay` `motionValue` `animateView`
- [02]-[MINI] `motion/mini`: ~2.3kB WAAPI featherweight — `animate` (single-target) `animateSequence`
- [03]-[REACT] `motion/react`: full hybrid — the `motion`/`m` proxies, `AnimatePresence`, `LayoutGroup`, `MotionConfig`, `LazyMotion` + feature bundles, the hook family, and the vanilla re-exports
- [04]-[REACT_MINI] `motion/react-mini`: WAAPI featherweight — `useAnimate` alone (scoped single-target)
- [05]-[REACT_M/CLIENT] `motion/react-m` · `motion/react-client`: the `m.*` lazy `LazyMotion` payload tags and the RSC client-boundary tags

## [03]-[MOTION_VALUES]

`MotionValue` is the render-free cell React never reconciles for: it subscribes, transforms, and writes the DOM outside reconciliation.

| [INDEX] | [SURFACE]                                                | [FAMILY]     | [CAPABILITY]                                             |
| :-----: | :------------------------------------------------------- | :----------- | :------------------------------------------------------- |
|  [01]   | `motionValue(init)` / `useMotionValue(initial)`          | value cell   | the render-free animated cell, bound via `style={{ x }}` |
|  [02]   | `useSpring(source, options?)`                            | spring bind  | spring-follows a `MotionValue`, number, or unit string   |
|  [03]   | `useTransform(input, inputRange, outputRange, options?)` | derive       | mapped range-to-range derived value                      |
|  [04]   | `useTransform(() => expr)`                               | derive       | computed derived value, the fold over source values      |
|  [05]   | `useScroll({ container?, target?, offset? })`            | scroll link  | `scrollX/Y` + `scrollX/YProgress` scroll-linked values   |
|  [06]   | `useVelocity(value)` / `useTime()`                       | derive/clock | velocity tracking, elapsed-time value                    |
|  [07]   | `useAnimationFrame(cb)`                                  | clock        | per-frame callback                                       |
|  [08]   | `useMotionValueEvent(value, event, cb)`                  | event seam   | `change`/`animationStart`/`animationComplete` events     |

[TRANSITION]: `Transition.type: "spring"|"tween"|"keyframes"|"inertia"|"decay"` `duration` `visualDuration` `stiffness` `damping` `mass` `bounce` `velocity` `restSpeed` `restDelta` `ease`; the vanilla `transform`/`mix`/`wrap` mappers and the `spring`/`frame` generators ride the `[02]-[ROOT]` entry.

## [04]-[COMPONENT_PLANE]

- `motion.*` / `m.*` proxy factories (`motion.div`, `motion.create(Component)`): `initial`/`animate`/`exit` targets or variant labels, `variants`, `transition`.
- Gestures: `whileHover` `whileTap` `whileDrag` `whileFocus` `whileInView`; `drag: boolean|"x"|"y"`, `dragConstraints`, `dragElastic`, `onPan(event, info)`; `useDragControls()` for handle-initiated drag.
- Layout: `layout: boolean|"position"|"size"|"preserve-aspect"`, `layoutId` morphs a shared element across unrelated trees, `layoutRoot`, `layoutScroll`; `LayoutGroup` scopes propagation.
- `AnimatePresence` (`mode: "sync"|"wait"|"popLayout"`, `initial`, `onExitComplete`, `propagate`) exit-animates unmounting children; `usePresence()`/`useIsPresent()` drive manual exit completion.
- `MotionConfig` (`transition`, `reducedMotion: "user"|"always"|"never"`, `nonce`) owns subtree policy; `useReducedMotion()` reads the `matchMedia` signal `system/act` gates on.
- `LazyMotion` + `domAnimation`/`domMax` bundles with `strict`: `m.*` tags render while features load async.
- Imperative: `animate(target, keyframes, options)` (element/value/object/sequence overloads), `useAnimate() -> [scope, animate]` (scoped selectors, auto-cleanup), vanilla `scroll`/`inView`, `press`/`hover` recognizers.

## [05]-[VIEW_TRANSITIONS_BUILDER]

`animateView(update, options?)` is the typed spring-physics layer over `document.startViewTransition`; the membership-gated React `<AnimateView>` is REJECTED.

[VIEW_TRANSITION_BUILDER]: `add(Element|string, Element|string?)` `crop(bool?)` `group(bool?)` `class(string)` `layout(opts?)` `enter(kf, opts?)` `exit(kf, opts?)` `new(kf, opts?)` `old(kf, opts?)` `updateTarget("enter"|"exit"|"layout"|"new"|"old", kf, opts?) -> void` `then(resolve, reject?) -> Promise<void>` — every method chains `this` except `updateTarget`.
[SURFACES]: `animateView(() => void|Promise<void>, AnimationOptions & { interrupt?: "wait"|"immediate" }?) -> ViewTransitionBuilder`

Interruption queues by default (`"wait"`); `"immediate"` preempts — the policy `startViewTransition` alone lacks.

## [06]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `MotionValue` is presentation state outside both React and the atom store: it lives on rAF and the engine reads/writes the DOM off-reconciliation, so mirroring it into React or atom state is the render-thrash defect.
- One surface, one owner: `tw-animate-css` class rows on RAC `entering:`/`exiting:` variants own discrete enter/exit, a surface graduates to `motion` only for physics, interruption, values, drag, or layout, and the two never animate one element together.

[STACKING]:
- `system/act` (within-lib): the act charter owns the discrete/continuous split, the `matchMedia` reduced-motion law `MotionConfig`/`useReducedMotion` mirror, and `useScroll`/`useTransform` as the one scroll-animation engine — raw `ScrollTimeline` and polyfills never enter.
- `react`(`.api/react.md`): three view-transition tiers fire one per surface — `Transition.run`'s native `startViewTransition` for capability-gated swaps, `animateView` for spring-timed per-subject swaps, the canary `<ViewTransition>` element for React-tree per-element transitions inside `startTransition`/`Suspense`.
- `effect-atom-atom-react`(`.api/effect-atom-atom-react.md`): domain state lives in the atom, the component derives an animation TARGET from the atom value, and `useMotionValueEvent` is the read seam when a threshold crossing must become atom state through `useAtomSet`.
- `use-gesture-react`(`.api/use-gesture-react.md`) + `vaul`(`.api/vaul.md`): `@use-gesture` owns raw recognition and vaul owns sheet drag; motion `drag`/`whileTap` bind only on engine-animated elements, and a use-gesture binding and motion `drag` on one node is the double-bind defect.
- `tw-animate-css`(`.api/tw-animate-css.md`): a RAC overlay animating through a `Motion` class row never also mounts `AnimatePresence` on the same element.

[LOCAL_ADMISSION]:
- Choose the entry cost tier per surface (mini → `LazyMotion` + `m` → full); vanilla entries drive non-React canvases, and the mini entries never carry `animateView`, components, or gestures.
- Derive animation targets from atom state with the engine interpolating; `MotionConfig` binds subtree transition and reduced-motion policy, `layoutId` binds shared-element morphs.
- Reach `animateView` where a document swap needs spring physics or interruption policy.

[RAIL_LAW]:
- Package: `motion`
- Owns: continuous motion — motion values and their derivation graph, spring/tween/inertia physics, `motion.*`/`m.*` elements, gesture-while states and engine-owned drag, layout/`layoutId` morphs, `AnimatePresence` exit choreography, imperative `animate`/`useAnimate` sequences, scroll/in-view linkage, and the `animateView` view-transition builder.
- Accept: per-surface cost laddering, atom-derived targets with engine interpolation, `MotionConfig` subtree policy, `layoutId` morphs, `animateView` for physics or interruption swaps, vanilla entries for non-React canvases.
- Reject: `framer-motion` as a manifest name beside `motion`, the paid `<AnimateView>`, a second scroll engine beside `useScroll`, `AnimatePresence` duplicating a `Motion` row on one surface, motion `drag` double-bound with a use-gesture handler or vaul sheet, `MotionValue`s mirrored into React/atom state, mini entries expected to carry `animateView`, components, or gestures.
