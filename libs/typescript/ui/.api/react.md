# [TS_UI_API_REACT]

`react` runs the whole `ui` folder as function components and hooks: `react-compiler` compiles memoization from the dataflow, `ref` rides as an ordinary prop, and `<Context>` is its own provider, so the folder authors no class component, `forwardRef`, or hand-written `useMemo`. `react` owns ephemeral interaction-local state alone — the `@effect-atom` binding owns domain state and reaches a component only through `useSyncExternalStore`, `@types/react` types the surface, and `react-dom` commits the tree.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `react`
- package: `react` (MIT)
- module: ESM/CJS dual via conditional `exports`; subpaths `.` (barrel), `./jsx-runtime`, `./jsx-dev-runtime`, `./compiler-runtime`; a `react-server` condition swaps the RSC-safe build (no client hooks) at the `.` and jsx subpaths
- runtime: isomorphic (browser/node/worker); zero dependencies; renderer-agnostic — `react` is the reconciler-facing runtime, a renderer package (`react-dom`) commits the tree
- asset: JS runtime shipping no own `.d.ts` — `@types/react` (`.api/types-react.md`) is the type surface and `tsc` the gate, so a runtime member and its type move as one wave; the stable barrel omits the canary members, which resolve only from the `react/canary` types subpath, admitted by one `/// <reference types="react/canary" />` at the entry types since a tsconfig `types` array cannot reach a conditional subpath
- compiler: `react-compiler` compiles memoization from the dataflow (`.api/babel-plugin-react-compiler.md` build pass, `.api/react-compiler-runtime.md` runtime companion); `__COMPILER_RUNTIME` is the internal handle the emitted code binds
- rail: the `ui` React spine — every `view`/`act`/`atom`/`intl` row is a function component or hook composed on this runtime

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the runtime-value type shapes the hooks return and consume — `@types/react` (`.api/types-react.md`) declares the full element/props/ref/child vocabulary (`ReactNode`, `ReactElement`, `ComponentProps`, `FC`, `PropsWithChildren`, `Ref`/`RefObject`, `Context<T>`), and the rows below carry the runtime-behavior types a hook signature threads, composing the entrypoint tables without re-teaching the element vocabulary.

| [INDEX] | [SYMBOL]                                                                          | [TYPE_FAMILY]            |
| :-----: | :-------------------------------------------------------------------------------- | :----------------------- |
|  [01]   | `Usable<T>` (`Promise<T>` \| `Context<T>`)                                        | suspendable read         |
|  [02]   | `Dispatch<A>` / `SetStateAction<S>` / `ActionDispatch<A>`                         | state dispatcher         |
|  [03]   | `TransitionStartFunction`                                                         | transition gate          |
|  [04]   | `RefObject<T>` / `Ref<T>` / `RefCallback<T>`                                      | ref cell                 |
|  [05]   | `Context<T>` / `Provider<T>`                                                      | context handle           |
|  [06]   | `ActivityProps` (`mode?: "visible" \| "hidden"`)                                  | activity mode            |
|  [07]   | `ViewTransitionProps` / `ViewTransitionInstance` / `ViewTransitionClass` (CANARY) | view-transition contract |
|  [08]   | `ReactNode` / `ReactElement` / `JSX.Element` (from `@types/react`)                | render tree              |

- [01]-[SUSPENDABLE_READ]: `use(usable)` argument — the value `Suspense` unwraps; the Effect→promise boundary in `view` feeds it.
- [02]-[STATE_DISPATCHER]: `useState`/`useReducer`/`useActionState` setter half — ephemeral-state mutation, never domain mutation.
- [03]-[TRANSITION_GATE]: `useTransition`'s returned `startTransition` — wraps a state update non-urgent for `view` route/filter churn.
- [04]-[REF_CELL]: `useRef`/`useImperativeHandle` results; a `Ref` passes as an ordinary prop, so `forwardRef` is never authored.
- [05]-[CONTEXT_HANDLE]: `createContext` result; rendered directly as `<Ctx value>` (the value provider) — `.Provider` is the retired spelling.
- [06]-[ACTIVITY_MODE]: `<Activity>` subtree pre-render/hide with preserved state — the stable `act/transition` pre-render/hide row.
- [07]-[VIEW_TRANSITION_CONTRACT]: props `name`/`default`/`enter`/`exit`/`update`/`share` (a class string or a per-transition-type map) + `onEnter`/`onExit`/`onUpdate`/`onShare(instance, types)`; declared only in the `react/canary` types subpath, never the default barrel.
- [08]-[RENDER_TREE]: every component return; the headless `view` rows and `intl` message folds produce `ReactNode`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: local state, reducers, and the action surface — `react` owns *ephemeral interaction-local* state, the value a widget needs this frame; domain/app state is the `@effect-atom` binding (`ONE_FOLD_ONE_BINDING`), so `useState`/`useReducer` never mirror a domain fold, and the action hooks (`useActionState`/`useOptimistic`) are the pure-React form boundary the folder mostly supersedes with `Schema`→aria `FormBinding` + the atom.

| [INDEX] | [SURFACE]                                                                        | [ENTRY_FAMILY] |
| :-----: | :------------------------------------------------------------------------------- | :------------- |
|  [01]   | `useState(initial)` / `useReducer(reducer, initial, init?)`                      | local state    |
|  [02]   | `useOptimistic(state, updateFn)` / `useActionState(action, initial, permalink?)` | action state   |
|  [03]   | `useSyncExternalStore(subscribe, getSnapshot, getServerSnapshot?)`               | external bind  |
|  [04]   | `useContext(Context)` / `createContext(default)`                                 | context read   |

- [01]-[LOCAL_STATE]: ephemeral view state (an input's uncommitted text, a hovered index); never a domain fold — that is `atom`.
- [02]-[ACTION_STATE]: React's optimistic/pending form state; the folder routes mutation through `atom` + `FormBinding`, so this stays a bare-React boundary only.
- [03]-[EXTERNAL_BIND]: `useSyncExternalStore` binds an external store — `@effect-atom/atom-react` builds on it, so `atom` reaches React here and nowhere else.
- [04]-[CONTEXT_READ]: `<Context value>` sets ambient capability a descendant reads — `I18nProvider` locale in `intl/format`, a theme handle in `token/theme`.

[ENTRYPOINT_SCOPE]: the concurrent-async surface — `use` unwraps a promise/context under `Suspense`, `useTransition`/`useDeferredValue` keep interaction responsive during heavy `view` churn, and `lazy` code-splits the `scope:viewer` heavy tier so non-spatial apps never pay for it.

| [INDEX] | [SURFACE]                                                                     | [ENTRY_FAMILY] |
| :-----: | :---------------------------------------------------------------------------- | :------------- |
|  [01]   | `use(usable)` — `Promise<T>` \| `Context<T>`                                  | suspend read   |
|  [02]   | `Suspense` (`fallback`) / `lazy(() => import(...))`                           | boundary       |
|  [03]   | `useTransition()` → `[isPending, startTransition]` / `startTransition(scope)` | transition     |
|  [04]   | `useDeferredValue(value, initial?)`                                           | deferral       |

- [01]-[SUSPEND_READ]: reads an atom-suspended value or a context conditionally; a resolved `Effect` promise crosses here at a `view` boundary.
- [02]-[BOUNDARY]: `viewer/scene/glb` and the geo tier are `lazy` behind `Suspense` — the `scope:viewer` split; skeletons are the recovery.
- [03]-[TRANSITION]: non-urgent updates for `view/compose` table/filter/route churn; `isPending` drives a pending affordance.
- [04]-[DEFERRAL]: `useDeferredValue` lags a fast-changing value (search box → virtualized `view` list) so typing stays responsive.

[ENTRYPOINT_SCOPE]: effects, refs, and identity — `react-compiler` owns memoization, so `useMemo`/`useCallback`/`memo` are compiler-emitted, not hand-authored; `useEffectEvent` extracts the non-reactive part of an effect so the dependency set stays honest.

| [INDEX] | [SURFACE]                                                                               | [ENTRY_FAMILY]  |
| :-----: | :-------------------------------------------------------------------------------------- | :-------------- |
|  [01]   | `useEffect(fn, deps?)` / `useLayoutEffect(fn, deps?)` / `useInsertionEffect(fn, deps?)` | effect seam     |
|  [02]   | `useEffectEvent(fn)`                                                                    | effect event    |
|  [03]   | `useRef(initial)` / `useImperativeHandle(ref, create, deps?)`                           | ref cell        |
|  [04]   | `useMemo(fn, deps)` / `useCallback(fn, deps)` / `memo(Component, areEqual?)`            | memo (compiled) |
|  [05]   | `useId()` / `useDebugValue(value, format?)`                                             | identity/debug  |

- [01]-[EFFECT_SEAM]: subscription lifetime, DOM measurement (`floating-ui` anchor read), style insertion; cleanup is the returned fn.
- [02]-[EFFECT_EVENT]: `useEffectEvent` extracts the non-reactive slice of an effect (latest handler without re-subscribing), keeping `useEffect` deps minimal under the compiler.
- [03]-[REF_CELL]: mutable handles (a virtualizer scroll node, an imperative `view` focus method); `ref` arrives as a plain prop.
- [04]-[MEMO_COMPILED]: compiler-emitted from the dataflow — authored by hand only at a proven FFI/identity boundary the compiler cannot see.
- [05]-[IDENTITY_DEBUG]: SSR-stable ids for a11y wiring; `useDebugValue` labels a custom hook in devtools.

[ENTRYPOINT_SCOPE]: elements, roots-of-tree components, and gated concurrency — element construction is the JSX runtime's job (`./jsx-runtime`), the imperative `createElement`/`cloneElement`/`Children` forms escape hatches; `Activity` ships stable (the pre-render/hide row), `ViewTransition`/`addTransitionType` are CANARY-CHANNEL rows — real bare-name exports of the pinned canary runtime, its only additions over stable — composing with the `act/transition` native rail and the motion `animateView` builder per the tier law (one tier fires per surface).

| [INDEX] | [SURFACE]                                                                                            | [ENTRY_FAMILY]     |
| :-----: | :--------------------------------------------------------------------------------------------------- | :----------------- |
|  [01]   | `Fragment` / `StrictMode` / `Profiler(id, onRender)`                                                 | tree wrapper       |
|  [02]   | `Activity` (`mode`) — `<Activity mode="hidden">`                                                     | pre-render/hide    |
|  [03]   | `ViewTransition` (CANARY) — `name`/`default`/`enter`/`exit`/`update`/`share` props + `on*` callbacks | element transition |
|  [04]   | `addTransitionType(type)` (CANARY)                                                                   | transition tag     |
|  [05]   | `Suspense` list boundaries + `use` compose the streaming/reveal order                                | reveal order       |
|  [06]   | `createElement` / `cloneElement` / `isValidElement` / `Children.{map,toArray,only}`                  | element (escape)   |
|  [07]   | `createContext` / `forwardRef` / `createRef` / `Component` / `PureComponent`                         | retired            |
|  [08]   | `cache(fn)` / `cacheSignal()`                                                                        | server memo        |
|  [09]   | `act(scope)` / `captureOwnerStack()` / `unstable_useCacheRefresh()`                                  | test/dev           |

- [01]-[TREE_WRAPPER]: grouping without a node, dev double-invoke, render-cost measurement around a `view` subtree.
- [02]-[PRE_RENDER_HIDE]: hide a subtree keeping state + defer its effects, or pre-render a hidden route — stable.
- [03]-[ELEMENT_TRANSITION]: animates ONLY inside a transition (`startTransition`/`useDeferredValue`/a Suspense recovery→content reveal — sync updates opt out) and only as the first element above a DOM node; `share` + a repeated `name` drives shared-element morphs; styling lands on `::view-transition-old/new(.class)`.
- [04]-[TRANSITION_TAG]: called inside the SAME `startTransition` as the update it annotates; the string keys the per-type class maps on `enter`/`exit`/`update`/`share` (unmatched falls to `default`, `"none"` disables).
- [05]-[REVEAL_ORDER]: `view/compose` staged reveal; ordering is Suspense-boundary placement, not an API knob.
- [06]-[ELEMENT_ESCAPE]: slot/`asChild` internals (`@radix-ui/react-slot`), prop-injecting a passed element; JSX is the authored path.
- [07]-[RETIRED]: present but never authored — ref-as-prop, `<Context>` provider, and function components supersede the class/forwardRef forms.
- [08]-[SERVER_MEMO]: RSC-only request-scoped memoization; inert in the client SPA build — a boundary row, not a client capability.
- [09]-[TEST_DEV]: kit-driven spec flushing (`@effect/vitest`), dev-build-only owner-stack capture, cache invalidation — not shipped-component surface.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- function components + hooks are the whole authoring surface: a component is a function returning `ReactNode`, capability is added by composing hooks, and no class hierarchy is in play — `Component`/`PureComponent`/`forwardRef`/`createRef` exist for interop but the folder authors none; `ref` rides as an ordinary prop, `<Context>` is its own provider, and `use(promise|context)` reads conditionally under `Suspense`.
- memoization is compiled, not written: `react-compiler` derives the memo boundaries from the dataflow and emits them against `react-compiler-runtime`, so `useMemo`/`useCallback`/`memo` are compiler-owned and a hand-authored one guards only a proven identity/FFI boundary the compiler cannot see; `useEffect` dependency sets stay honest because `useEffectEvent` extracts the non-reactive slice rather than lying with an under-specified array.
- state is layered by lifetime: `react` owns *ephemeral interaction-local* state (`useState`/`useReducer` — the uncommitted, this-frame value), domain/app state is the `@effect-atom` fold, and `useSyncExternalStore` is their only seam — `@effect-atom/atom-react` is built on it, so an atom's value reaches a component through that subscription and nowhere else and a component never holds a second copy of domain state.
- async is Suspense + transitions, not manual flags: `use`/`Suspense`/`lazy` express loading structurally (fallbacks, code-split boundaries) and `useTransition`/`useDeferredValue` keep interaction responsive while a heavy `view` update settles; the `scope:viewer` heavy tier is `lazy`-loaded behind a `Suspense` boundary so the non-spatial majority never downloads three/deck.gl.
- `react` admits the canary dist-tag: its only runtime additions over stable are `ViewTransition` and `addTransitionType`, and `Activity` (pre-render/hide with preserved state) ships stable.
- `<ViewTransition>` fires only from a transition-wrapped update (`startTransition`/`useDeferredValue`/a Suspense reveal, a synchronous `setState` opting out) and sits directly above the DOM node it names; `addTransitionType` tags the update inside the same `startTransition`, selecting the per-type class arm on `enter`/`exit`/`update`/`share`.
- `react/experimental` stays rejected — `useSwipeTransition`/`unstable_SuspenseList`/`unstable_startGestureTransition` live only in those typings, absent from the pinned canary runtime; `cache`/`cacheSignal` are RSC-only and inert in the client build.

[STACKING]:
- `@effect-atom/atom-react` (`.api/effect-atom-atom-react.md`, `.api/effect-atom-atom.md`): `useAtomValue`/`useAtom` build on `useSyncExternalStore`, subscribing a component to an atom, Suspense-integrating an async atom into `use`/`Suspense` through `useAtomSuspense`, and surfacing the write half as a `useAtomSet` dispatcher — the one state binding, so `useState` never duplicates an atom-owned value and the `Effect` rail (retry/span/decode) stays behind the atom, never in a component body.
- `react-aria` / `react-stately` / `react-aria-components` (`.api/react-aria.md`, `.api/react-stately.md`, `.api/react-aria-components.md`): every `use<Widget>` hook runs on this runtime, consumes `useId` for a11y id stability, and returns `DOMAttributes` bundles a `view` row spreads through `mergeProps`; a `view` primitive composes a react-aria hook with `token`/`tailwind` classes and the compiler memoizes it.
- `react-dom` (`.api/react-dom.md`): `react-dom` commits the tree `react` describes, owning `createRoot`/`flushSync`/`createPortal` and the form-status/DOM hooks; the one `createRoot(...).render` edge lives at app boot in `browser`, and the renderer paints the `Suspense` fallbacks while `use` suspends.
- `react-error-boundary` (`.api/react-error-boundary.md`): a render throw — including an Effect fault surfaced through the atom as `Cause.squash(cause)` — lands in an `ErrorBoundary` paired with the `Suspense` recovery, so every `view` subtree carries a typed loading-plus-failure envelope.
- `babel-plugin-react-compiler` / `react-compiler-runtime` (`.api/babel-plugin-react-compiler.md`, `.api/react-compiler-runtime.md`): the build pass rewrites each component to memoized form against the runtime companion; rules-of-hooks and no-conditional-hooks are the contract it depends on, so the folder writes hooks straight and the pass optimizes.
- `view`/`act`/`atom`/`intl` (within-lib): every row is a function component or hook on this runtime — `atom/binding` owns domain state through `useSyncExternalStore`, `act/transition` the native/canary transition tiers, and `intl/format` the `I18nProvider` context.

[LOCAL_ADMISSION]:
- Author function components + hooks only; never a class component, `forwardRef`, or `createRef` — pass `ref` as a prop and compose hooks for behavior.
- Route domain/app state through `@effect-atom`; use `useState`/`useReducer` only for ephemeral interaction-local state, and bind any external store through `useSyncExternalStore`, never a hand-rolled subscribe-in-`useEffect`+`useState` pair.
- Let `react-compiler` emit memoization; never hand-write `useMemo`/`useCallback`/`memo` except at a proven identity/FFI boundary the compiler cannot see, and keep `useEffect` deps honest with `useEffectEvent` instead of an under-specified array.
- Express async with `use`/`Suspense`/`lazy` + `useTransition`/`useDeferredValue`; never a manual `isLoading` boolean ladder, and never run an `Effect` inside a component body — the atom binding owns the rail and the one `runFork` edge is at boot.
- Fire `<ViewTransition>` only from a transition-wrapped update with `addTransitionType` selecting the arm, keep the native `startViewTransition` rail (and motion `animateView`) for document-level swaps per the `act/transition` tier law, and admit the canary types with one `react/canary` reference; never author `react/experimental` surfaces, and never ship `cache`/`cacheSignal` as client capability.

[RAIL_LAW]:
- Package: `react`
- Owns: the React function-component runtime — the hook surface (`useState`/`useReducer`/`useContext`/`useRef`/`useEffect`/`useLayoutEffect`/`useInsertionEffect`/`useEffectEvent`/`useId`/`useDebugValue`/`useImperativeHandle`), the external-store bind (`useSyncExternalStore`), the concurrent-async surface (`use`/`Suspense`/`lazy`/`useTransition`/`useDeferredValue`/`startTransition`), the action hooks (`useActionState`/`useOptimistic`), context (`createContext`/`useContext` + `<Context>` provider), the element/tree APIs (`createElement`/`cloneElement`/`Children`/`Fragment`/`StrictMode`/`Profiler`), the stable `Activity`, the canary `ViewTransition`/`addTransitionType`, the RSC `cache`/`cacheSignal`, and the test/compiler handles (`act`/`__COMPILER_RUNTIME`)
- Accept: function components, compiler-emitted memoization, `atom`-owned domain state bound via `useSyncExternalStore`, Suspense/transition async, ref-as-prop, `<Context>` provider, `lazy` code-split of the `scope:viewer` tier, transition-gated `<ViewTransition>` composed with the `act/transition` tiers under the `react/canary` types reference
- Reject: class components / `forwardRef` / `createRef` authoring, hand-written `useMemo`/`useCallback` outside a proven boundary, `useState` mirroring a domain fold, manual subscribe-in-effect stores, `isLoading` flag ladders, an `Effect` run inside a component body, `<ViewTransition>` fired outside a transition or shipped without its canary types reference, any `react/experimental` surface, and treating `cache`/`cacheSignal` as client capability
