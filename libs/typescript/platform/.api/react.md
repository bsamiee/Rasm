# [API_CATALOGUE] react

`react` covers React 19 core: element creation and cloning, context, component utilities (`forwardRef`, `memo`, `lazy`), boundary components (`Suspense`, `StrictMode`, `Fragment`, `Activity`), all built-in hooks through React 19 (`use`, `useActionState`, `useOptimistic`, `cache`, `cacheSignal`), and the key composition utilities consumed by the platform packages.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `react`
- package: `react`
- module: `react` (CJS and ESM; types from `@types/react`)
- asset: runtime library
- rail: ui

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: core element and component types
- rail: ui

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY] | [RAIL]                                         |
| :-----: | :------------------------- | :------------ | :--------------------------------------------- |
|   [1]   | `ReactElement<P, T>`       | interface     | JSX element representation                     |
|   [2]   | `ReactNode`                | union type    | everything React renders                       |
|   [3]   | `ReactPortal`              | interface     | portal element                                 |
|   [4]   | `ComponentType<P>`         | union type    | `ComponentClass<P> \| FunctionComponent<P>`    |
|   [5]   | `FunctionComponent<P>`     | interface     | `(props: P) => ReactNode`                      |
|   [6]   | `ComponentClass<P>`        | interface     | class component constructor                    |
|   [7]   | `ExoticComponent<P>`       | interface     | `forwardRef`/`memo`/`createContext` result     |
|   [8]   | `PropsWithChildren<P>`     | type alias    | `P & { children?: ReactNode }`                 |
|   [9]   | `PropsWithoutRef<Props>`   | type alias    | omits `ref` from props                         |
|  [10]   | `ComponentProps<T>`        | utility type  | infers props from element type or string       |
|  [11]   | `ComponentRef<T>`          | utility type  | infers ref type from component                 |
|  [12]   | `ElementType<P, Tag>`      | union type    | all components and tags matching props         |
|  [13]   | `JSXElementConstructor<P>` | union type    | function or class component constructor        |
|  [14]   | `RefObject<T>`             | interface     | `{ current: T }`                               |
|  [15]   | `Ref<T>`                   | union type    | `RefCallback<T> \| RefObject<T\|null> \| null` |

[PUBLIC_TYPE_SCOPE]: hook and dispatch types
- rail: ui

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [RAIL]                           |
| :-----: | :------------------------ | :------------ | :------------------------------- |
|   [1]   | `SetStateAction<S>`       | union type    | `S \| ((prevState: S) => S)`     |
|   [2]   | `Dispatch<A>`             | function type | `(value: A) => void`             |
|   [3]   | `Reducer<S, A>`           | function type | `(prevState: S, action: A) => S` |
|   [4]   | `EffectCallback`          | function type | `() => void \| Destructor`       |
|   [5]   | `DependencyList`          | type alias    | `readonly unknown[]`             |
|   [6]   | `TransitionStartFunction` | interface     | wraps deferred state updates     |
|   [7]   | `Usable<T>`               | union type    | `ReactPromise<T> \| Context<T>`  |

[PUBLIC_TYPE_SCOPE]: boundary and context types
- rail: ui

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY] | [RAIL]                                          |
| :-----: | :-------------- | :------------ | :---------------------------------------------- |
|   [1]   | `Context<T>`    | interface     | provider/consumer context object                |
|   [2]   | `SuspenseProps` | interface     | `children`, `fallback`, `name`                  |
|   [3]   | `ActivityProps` | interface     | `mode: "hidden"\|"visible"`, `name`, `children` |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: element and component utilities
- rail: ui

```ts
// @types/react index.d.ts
function createElement<P>(
  type: FunctionComponent<P> | ComponentClass<P> | string,
  props?: Attributes & P | null,
  ...children: ReactNode[]
): ReactElement<P>

function cloneElement<P>(
  element: ReactElement<P>,
  props?: Partial<P> & Attributes,
  ...children: ReactNode[]
): ReactElement<P>

function createContext<T>(defaultValue: T): Context<T>

function isValidElement<P>(object: {} | null | undefined): object is ReactElement<P>

function forwardRef<T, P = {}>(
  render: ForwardRefRenderFunction<T, PropsWithoutRef<P>>,
): ForwardRefExoticComponent<PropsWithoutRef<P> & RefAttributes<T>>

function memo<P extends object>(
  Component: FunctionComponent<P>,
  propsAreEqual?: (prevProps: Readonly<P>, nextProps: Readonly<P>) => boolean,
): NamedExoticComponent<P>

function lazy<T extends ComponentType<any>>(
  load: () => Promise<{ default: T }>,
): LazyExoticComponent<T>
```

[ENTRYPOINT_SCOPE]: boundary and structural components
- rail: ui

```ts
const Fragment: ExoticComponent<FragmentProps>
const StrictMode: ExoticComponent<{ children?: ReactNode | undefined }>
const Suspense: ExoticComponent<SuspenseProps>
// React 19.2.0
const Activity: ExoticComponent<ActivityProps>
```

[ENTRYPOINT_SCOPE]: built-in hooks
- rail: ui

```ts
function useState<S>(initialState: S | (() => S)): [S, Dispatch<SetStateAction<S>>]
function useState<S = undefined>(): [S | undefined, Dispatch<SetStateAction<S | undefined>>]

function useReducer<S, A extends AnyActionArg>(
  reducer: Reducer<S, ...A>,
  initialState: S,
): [S, ActionDispatch<A>]
function useReducer<S, I, A extends AnyActionArg>(
  reducer: Reducer<S, ...A>,
  initialArg: I,
  init: (i: I) => S,
): [S, ActionDispatch<A>]

function useEffect(effect: EffectCallback, deps?: DependencyList): void
function useLayoutEffect(effect: EffectCallback, deps?: DependencyList): void
function useInsertionEffect(effect: EffectCallback, deps?: DependencyList): void

function useContext<T>(context: Context<T>): T

function useRef<T>(initialValue: T): RefObject<T>
function useRef<T>(initialValue: T | null): RefObject<T | null>
function useRef<T>(initialValue: T | undefined): RefObject<T | undefined>

function useCallback<T extends Function>(callback: T, deps: DependencyList): T
function useMemo<T>(factory: () => T, deps: DependencyList): T
function useImperativeHandle<T, R extends T>(ref: Ref<T> | undefined, init: () => R, deps?: DependencyList): void
function useDebugValue<T>(value: T, format?: (value: T) => any): void
function useId(): string

function useTransition(): [boolean, TransitionStartFunction]
function useDeferredValue<T>(value: T, initialValue?: T): T
function useSyncExternalStore<Snapshot>(
  subscribe: (onStoreChange: () => void) => () => void,
  getSnapshot: () => Snapshot,
  getServerSnapshot?: () => Snapshot,
): Snapshot
```

[ENTRYPOINT_SCOPE]: React 19 additions
- rail: ui

```ts
// React 19
function use<T>(usable: Usable<T>): T

function useOptimistic<State>(
  passthrough: State,
): [State, (action: State | ((pendingState: State) => State)) => void]
function useOptimistic<State, Action>(
  passthrough: State,
  reducer: (state: State, action: Action) => State,
): [State, (action: Action) => void]

function useActionState<State>(
  action: (state: Awaited<State>) => State | Promise<State>,
  initialState: Awaited<State>,
  permalink?: string,
): [state: Awaited<State>, dispatch: () => void, isPending: boolean]
function useActionState<State, Payload>(
  action: (state: Awaited<State>, payload: Payload) => State | Promise<State>,
  initialState: Awaited<State>,
  permalink?: string,
): [state: Awaited<State>, dispatch: (payload: Payload) => void, isPending: boolean]

// React 19.2.0
function cache<CachedFunction extends Function>(fn: CachedFunction): CachedFunction
function cacheSignal(): null | CacheSignal
function captureOwnerStack(): string | null  // development builds only

function startTransition(scope: TransitionFunction): void
function act(callback: () => VoidOrUndefinedOnly): void
function act<T>(callback: () => T | Promise<T>): Promise<T>
```

## [4]-[IMPLEMENTATION_LAW]

[UI_TOPOLOGY]:
- `ReactNode` is the renderable union: `ReactElement | string | number | bigint | Iterable<ReactNode> | ReactPortal | boolean | null | undefined | Promise<AwaitedReactNode>`
- `Suspense` accepts a `fallback` prop rendered while children are loading; `name` labels the boundary in DevTools
- `Activity` hides or reveals a subtree without unmounting it; `mode: "hidden"` preserves state while hiding from the viewport
- `lazy` defers component code loading; the loaded module must export `default` as a React component
- `forwardRef` exposes a DOM or component ref through a function component; the render function receives `props` and `ref`
- `memo` skips re-render when `propsAreEqual` returns `true`; default is shallow prop comparison

[LOCAL_ADMISSION]:
- `use(promise)` suspends the component until the promise resolves; use inside `Suspense` boundaries.
- `useActionState` manages form action state with a pending indicator; replaces `useState` + `useTransition` for form submission.
- `useOptimistic` applies an optimistic update immediately, rolling back when the async action settles.
- `cache` memoizes a function per React render tree; `cacheSignal` signals cache invalidation.
- `useSyncExternalStore` is the canonical hook for subscribing to external stores; include `getServerSnapshot` for SSR hydration safety.

[RAIL_LAW]:
- Package: `react`
- Owns: component lifecycle, hooks, element creation, context, and boundary primitives
- Accept: typed props via `ComponentProps<T>`, ref forwarding via `forwardRef`, lazy splits via `lazy`
- Reject: class components for new code; string refs (removed in React 19)
