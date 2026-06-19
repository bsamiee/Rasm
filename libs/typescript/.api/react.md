# [API_CATALOGUE] react

`react` supplies the core rendering primitives, component model, hooks, and concurrent features for all UI surfaces. It provides `ReactElement`, `ReactNode`, `FunctionComponent`, context, suspense, refs, and the full hook set consumed by every component owner on the `ui` stack.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `react`
- package: `react`
- module: `react` (typed by `@types/react`)
- namespace: `React` (UMD ambient name)
- asset: core component primitives, hooks, context, suspense, concurrent APIs
- rail: render

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: element and component family
- rail: render

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]         | [RAIL]                                      |
| :-----: | :----------------------------- | :-------------------- | :------------------------------------------ |
|   [1]   | `ReactElement<P, T>`           | element value         | JSX tree node                               |
|   [2]   | `ReactNode`                    | renderable union      | all renderable values                       |
|   [3]   | `ReactPortal`                  | portal element        | out-of-tree insertion                       |
|   [4]   | `FunctionComponent<P>`         | function component    | `(props: P) => ReactNode`                   |
|   [5]   | `FC<P>`                        | alias for FC          | shorthand for `FunctionComponent`           |
|   [6]   | `ComponentType<P>`             | component union       | `FC<P> \| ComponentClass<P>`                |
|   [7]   | `ComponentClass<P, S>`         | class component       | stateful class surface                      |
|   [8]   | `ForwardRefExoticComponent<P>` | exotic component      | forwardRef result shape                     |
|   [9]   | `ExoticComponent<P>`           | exotic component base | `$$typeof` branded component                |
|  [10]   | `NamedExoticComponent<P>`      | named exotic          | has `displayName`                           |
|  [11]   | `Provider<T>`                  | context provider      | `ProviderExoticComponent<ProviderProps<T>>` |
|  [12]   | `Consumer<T>`                  | context consumer      | render-prop consumer                        |

[PUBLIC_TYPE_SCOPE]: ref and identity family
- rail: render

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]   | [RAIL]                                           |
| :-----: | :----------------- | :-------------- | :----------------------------------------------- |
|   [1]   | `RefObject<T>`     | ref carrier     | `{ current: T }`                                 |
|   [2]   | `Ref<T>`           | ref union       | `RefCallback<T> \| RefObject<T \| null> \| null` |
|   [3]   | `RefCallback<T>`   | callback ref    | mutation callback form                           |
|   [4]   | `RefAttributes<T>` | ref prop        | `{ ref?: Ref<T> }`                               |
|   [5]   | `ForwardedRef<T>`  | forwarded ref   | accepted by `ForwardRefRenderFunction`           |
|   [6]   | `Key`              | reconciler key  | `string \| number \| bigint`                     |
|   [7]   | `Attributes`       | base attributes | `{ key?: Key \| null }`                          |

[PUBLIC_TYPE_SCOPE]: context family
- rail: render

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]  | [RAIL]                                  |
| :-----: | :----------------- | :------------- | :-------------------------------------- |
|   [1]   | `Context<T>`       | context object | extends `Provider<T>`; has `Consumer`   |
|   [2]   | `ContextType<C>`   | context value  | infers `T` from `Context<T>`            |
|   [3]   | `ProviderProps<T>` | provider props | `{ value: T; children?: ReactNode }`    |
|   [4]   | `ConsumerProps<T>` | consumer props | `{ children: (value: T) => ReactNode }` |

[PUBLIC_TYPE_SCOPE]: suspense and async family
- rail: render

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]     | [RAIL]                                          |
| :-----: | :------------------------- | :---------------- | :---------------------------------------------- |
|   [1]   | `SuspenseProps`            | suspense props    | `{ children?; fallback?; name? }`               |
|   [2]   | `ReactPromise<T>`          | promise union     | `untracked \| pending \| fulfilled \| rejected` |
|   [3]   | `PendingReactPromise<T>`   | pending promise   | `{ status: "pending" }`                         |
|   [4]   | `FulfilledReactPromise<T>` | fulfilled promise | `{ status: "fulfilled"; value: T }`             |
|   [5]   | `RejectedReactPromise<T>`  | rejected promise  | `{ status: "rejected"; reason: unknown }`       |
|   [6]   | `Usable<T>`                | usable union      | `ReactPromise<T> \| Context<T>`                 |

[PUBLIC_TYPE_SCOPE]: hooks supporting types
- rail: render

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]      | [RAIL]                                      |
| :-----: | :------------------------ | :----------------- | :------------------------------------------ |
|   [1]   | `Dispatch<A>`             | dispatch fn type   | `(value: A) => void`                        |
|   [2]   | `SetStateAction<S>`       | state setter union | `S \| ((prevState: S) => S)`                |
|   [3]   | `DependencyList`          | dep array          | `readonly unknown[]`                        |
|   [4]   | `EffectCallback`          | effect return      | `() => void \| Destructor`                  |
|   [5]   | `TransitionStartFunction` | transition fn      | `(callback: TransitionFunction) => void`    |
|   [6]   | `TransitionFunction`      | transition cb      | `() => VoidOrUndefinedOnly \| Promise<...>` |

[PUBLIC_TYPE_SCOPE]: component intrinsics
- rail: render

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY] | [RAIL]                               |
| :-----: | :-------------- | :------------ | :----------------------------------- |
|   [1]   | `Fragment`      | exotic        | groups children without DOM wrapper  |
|   [2]   | `StrictMode`    | exotic        | development double-invoke guard      |
|   [3]   | `Suspense`      | exotic        | async boundary with `fallback`       |
|   [4]   | `Profiler`      | exotic        | render timing boundary               |
|   [5]   | `ProfilerProps` | props type    | `{ id: string; onRender: callback }` |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: element construction
- rail: render

| [INDEX] | [SURFACE]                                   | [ENTRY_FAMILY]    | [RAIL]                             |
| :-----: | :------------------------------------------ | :---------------- | :--------------------------------- |
|   [1]   | `createElement(type, props, ...children)`   | element factory   | primary JSX-compiled entrypoint    |
|   [2]   | `cloneElement(element, props, ...children)` | element clone     | override props on existing element |
|   [3]   | `isValidElement(object)`                    | type guard        | `object is ReactElement<P>`        |
|   [4]   | `createContext<T>(defaultValue)`            | context factory   | returns `Context<T>`               |
|   [5]   | `forwardRef<T, P>(render)`                  | ref forwarder     | `ForwardRefExoticComponent<...>`   |
|   [6]   | `memo<P>(Component, propsAreEqual?)`        | memo wrapper      | `NamedExoticComponent<P>`          |
|   [7]   | `lazy<T>(factory)`                          | code-split loader | throws to Suspense until resolved  |
|   [8]   | `createRef<T>()`                            | imperative ref    | `RefObject<T \| null>`             |

[ENTRYPOINT_SCOPE]: hooks
- rail: render

| [INDEX] | [SURFACE]                                               | [ENTRY_FAMILY]    | [RAIL]                                   |
| :-----: | :------------------------------------------------------ | :---------------- | :--------------------------------------- |
|   [1]   | `useState<S>(initialState)`                             | state hook        | `[S, Dispatch<SetStateAction<S>>]`       |
|   [2]   | `useReducer<S, A>(reducer, initialState)`               | reducer hook      | `[S, ActionDispatch<A>]`                 |
|   [3]   | `useEffect(effect, deps?)`                              | side-effect hook  | passive post-paint effect                |
|   [4]   | `useLayoutEffect(effect, deps?)`                        | sync effect hook  | synchronous after DOM mutations          |
|   [5]   | `useInsertionEffect(effect, deps?)`                     | insertion hook    | before DOM mutations (CSS-in-JS)         |
|   [6]   | `useRef<T>(initialValue)`                               | ref hook          | mutable `RefObject<T>`                   |
|   [7]   | `useContext<T>(context)`                                | context hook      | reads nearest `Provider` value           |
|   [8]   | `useMemo<T>(factory, deps)`                             | memo hook         | recomputes only when deps change         |
|   [9]   | `useCallback<T>(callback, deps)`                        | callback hook     | stable identity when deps unchanged      |
|  [10]   | `useImperativeHandle<T, R>(ref, init, deps?)`           | imperative hook   | customizes forwarded ref value           |
|  [11]   | `useId()`                                               | id hook           | stable SSR-consistent unique id          |
|  [12]   | `useDebugValue<T>(value, format?)`                      | devtools hook     | labels custom hooks in DevTools          |
|  [13]   | `useTransition()`                                       | transition hook   | `[boolean, TransitionStartFunction]`     |
|  [14]   | `useDeferredValue<T>(value, initialValue?)`             | deferred hook     | deferred copy of a value                 |
|  [15]   | `useOptimistic<State, Action>(passthrough, reducer?)`   | optimistic hook   | temporary optimistic state               |
|  [16]   | `useSyncExternalStore<Snapshot>(sub, getSnapshot, ...)` | external store    | subscribe to non-React stores            |
|  [17]   | `useActionState<State, Payload>(action, init, ...)`     | action state hook | form action state + dispatch + isPending |
|  [18]   | `useEffectEvent<T extends Function>(callback)`          | event hook        | non-reactive stable event handler        |

[ENTRYPOINT_SCOPE]: concurrent and transition utilities
- rail: render

| [INDEX] | [SURFACE]                       | [ENTRY_FAMILY] | [RAIL]                               |
| :-----: | :------------------------------ | :------------- | :----------------------------------- |
|   [1]   | `startTransition(scope)`        | imperative     | defers state update without hook     |
|   [2]   | `use<T>(usable)`                | use hook       | reads `Context<T>` or `Promise<T>`   |
|   [3]   | `cache<F extends Function>(fn)` | cache factory  | per-render memoization cache         |
|   [4]   | `cacheSignal()`                 | cache signal   | `null \| CacheSignal`                |
|   [5]   | `act(callback)`                 | test utility   | flushes effects in test environments |

## [4]-[IMPLEMENTATION_LAW]

[RENDER_TOPOLOGY]:
- namespace: `React` (UMD) + named exports from `react`
- hooks require a function component or custom hook call site — no class bodies
- `useEffect` is passive (fires after paint); `useLayoutEffect` fires synchronously after DOM mutations
- `useInsertionEffect` fires before DOM mutations — CSS-in-JS injection only
- `use()` reads a `Context<T>` or a `ReactPromise<T>`; calling with an unresolved promise throws to the nearest `Suspense`
- `ReactPromise<T>` status transitions: untracked → pending → fulfilled | rejected
- `lazy()` and `use(promise)` both throw to `Suspense`; the fallback renders until resolution
- `forwardRef` wraps a render function taking `(props, ref)` and returns a `ForwardRefExoticComponent`
- `memo` shallow-compares props by default; pass a custom `propsAreEqual` predicate to override
- `startTransition` marks updates as low-priority; `useTransition` additionally exposes an `isPending` boolean

[LOCAL_ADMISSION]:
- Components are `FunctionComponent<P>` or `ForwardRefExoticComponent<P>`; class components are allowed but not the default target.
- Context values flow through `Provider` at composition; components read with `useContext`.
- Refs are `RefObject<T>` for imperative DOM access and `ForwardedRef<T>` on forwarded surfaces.
- `use()` is the canonical hook for reading both context and promises in render.

[RAIL_LAW]:
- package: `react` (types: `@types/react`)
- Owns: component model, hooks, context, concurrent APIs, JSX primitives
- Accept: function components, hooks, context providers, lazy, suspense, forwardRef, memo
- Reject: string-ref patterns, `createFactory` (removed in 19), mutable ref reads during render
