# [API_CATALOGUE] @effect-atom/atom-react

Dependency catalogue for `@effect-atom/atom-react` (0.5.0) — the React binding of the
framework-agnostic atom engine, consumed by the `platform` SPA shell over the `ui` binding spine.
It owns every React store subscription on the browser surface. Grounded from installed `node_modules` `.d.ts` declarations;
exact spellings. Tier `browser`: the React hooks/components plus `Atom.windowFocusSignal`/DOM focus couple it to the browser bundle,
unlike its `neutral`-tier core.

The entry `index.d.ts` is a thin React facade. It re-exports the seven framework-neutral
`@effect-atom/atom` (0.5.3) core namespaces verbatim — `Atom`, `Registry`, `Result`, `AtomRef`,
`AtomHttpApi`, `AtomRpc`, `Hydration` — whose full member surface is transcribed once on
`effect-atom.md` and is NOT duplicated here, imported as
`import { Atom, Result, Registry } from "@effect-atom/atom-react"`. This page transcribes the
binding's OWN surface: the React modules `Hooks`, `RegistryContext`, `ScopedAtom`, and
`ReactHydration`. Cross-package owners (`Atom.*`, `Result.*`, `AtomRef.*`, `Registry.*`,
`Hydration.*`, `Exit`, `React`) resolve against `effect-atom.md` and `ui-stack.md`.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect-atom/atom-react`
- package: `@effect-atom/atom-react`
- entry: `@effect-atom/atom-react` (barrel) — re-export namespaces + flat-exported hooks/context
- peers: `@effect-atom/atom` (the re-exported core, page `effect-atom.md`); transitive `effect`, `@effect/platform`, `@effect/rpc`, `@effect/experimental`; `react` peer for the React modules
- asset: React subscription hooks (`Hooks`), per-tree registry context/provider (`RegistryContext`), scoped per-component atom factory (`ScopedAtom`), SSR hydration boundary component (`ReactHydration`)
- tier: `browser`

[ENTRY_REEXPORTS]: the barrel hoists the core namespaces and flat-exports its own hook/context members

```ts
export * as Atom from "@effect-atom/atom/Atom"            // → effect-atom.md §Atom
export * as Registry from "@effect-atom/atom/Registry"    // → effect-atom.md §Registry
export * as Result from "@effect-atom/atom/Result"        // → effect-atom.md §Result
export * as AtomRef from "@effect-atom/atom/AtomRef"      // → effect-atom.md §AtomRef
export * as AtomHttpApi from "@effect-atom/atom/AtomHttpApi"  // → effect-atom.md §AtomHttpApi
export * as AtomRpc from "@effect-atom/atom/AtomRpc"      // → effect-atom.md §AtomRpc
export * as Hydration from "@effect-atom/atom/Hydration"  // → effect-atom.md §Hydration
export * as ScopedAtom from "./ScopedAtom.js"            // namespace — §4
export * from "./Hooks.js"            // flat — §2 (useAtom, useAtomValue, … top-level)
export * from "./RegistryContext.js"  // flat — §3 (RegistryContext, RegistryProvider, scheduleTask)
// NOTE: index.d.ts does NOT re-export ReactHydration — §5 is reachable ONLY via the subpath
//       import { HydrationBoundary } from "@effect-atom/atom-react/ReactHydration"
```

`package.json` `exports` exposes five entry points: the `.` barrel above plus four first-class
subpaths `./Hooks`, `./RegistryContext`, `./ScopedAtom`, `./ReactHydration` (each a `types`/`import`/
`default` triple). `Hooks`/`RegistryContext`/`ScopedAtom` are ALSO hoisted onto the barrel;
`ReactHydration` is subpath-only.

## [2]-[HOOKS]

`Hooks.d.ts` — flat-exported subscription hooks. `Mode` discriminates the writer return when the
atom value is a `Result` (`"value"` setter, `"promise"` resolving to `Result.Result.Success<R>`,
`"promiseExit"` resolving to `Exit.Exit<Success, Failure>`). `useAtomValue` accepts an optional
projection; `useAtomSuspense` throws to React Suspense until the `Result` resolves.

```ts
const useAtomInitialValues: (initialValues: Iterable<readonly [Atom.Atom<any>, any]>) => void

const useAtomValue: {
  <A>(atom: Atom.Atom<A>): A
  <A, B>(atom: Atom.Atom<A>, f: (_: A) => B): B
}
const useAtomMount:   <A>(atom: Atom.Atom<A>) => void
const useAtomRefresh: <A>(atom: Atom.Atom<A>) => () => void

const useAtomSet: <R, W, Mode extends "value" | "promise" | "promiseExit" = never>(atom: Atom.Writable<R, W>, options?: {
  readonly mode?: ([R] extends [Result.Result<any, any>] ? Mode : "value") | undefined
}) => "promise" extends Mode ? ((value: W) => Promise<Result.Result.Success<R>>)
   : "promiseExit" extends Mode ? ((value: W) => Promise<Exit.Exit<Result.Result.Success<R>, Result.Result.Failure<R>>>)
   : ((value: W | ((value: R) => W)) => void)

const useAtom: <R, W, const Mode extends "value" | "promise" | "promiseExit" = never>(atom: Atom.Writable<R, W>, options?: {
  readonly mode?: ([R] extends [Result.Result<any, any>] ? Mode : "value") | undefined
}) => readonly [value: R, write: "promise" extends Mode ? ((value: W) => Promise<Result.Result.Success<R>>)
   : "promiseExit" extends Mode ? ((value: W) => Promise<Exit.Exit<Result.Result.Success<R>, Result.Result.Failure<R>>>)
   : ((value: W | ((value: R) => W)) => void)]

const useAtomSuspense: <A, E, const IncludeFailure extends boolean = false>(atom: Atom.Atom<Result.Result<A, E>>, options?: {
  readonly suspendOnWaiting?: boolean | undefined
  readonly includeFailure?: IncludeFailure | undefined
}) => Result.Success<A, E> | (IncludeFailure extends true ? Result.Failure<A, E> : never)

const useAtomSubscribe: <A>(atom: Atom.Atom<A>, f: (_: A) => void, options?: { readonly immediate?: boolean }) => void

const useAtomRef:          <A>(ref: AtomRef.ReadonlyRef<A>) => A
const useAtomRefProp:      <A, K extends keyof A>(ref: AtomRef.AtomRef<A>, prop: K) => AtomRef.AtomRef<A[K]>
const useAtomRefPropValue: <A, K extends keyof A>(ref: AtomRef.AtomRef<A>, prop: K) => A[K]
```

## [3]-[REGISTRY_CONTEXT]

`RegistryContext.d.ts` — flat-exported. One `Registry` (page `effect-atom.md` §Registry) per
React subtree. `RegistryProvider` constructs and provides it; `scheduleTask` is the default React
task scheduler the provider uses unless overridden.

```ts
function scheduleTask(f: () => void): void
const RegistryContext: React.Context<Registry.Registry>
const RegistryProvider: (options: {
  readonly children?: React.ReactNode | undefined
  readonly initialValues?: Iterable<readonly [Atom.Atom<any>, any]> | undefined
  readonly scheduleTask?: ((f: () => void) => void) | undefined
  readonly timeoutResolution?: number | undefined
  readonly defaultIdleTTL?: number | undefined
}) => React.FunctionComponentElement<React.ProviderProps<Registry.Registry>>
```

## [4]-[SCOPED_ATOM]

`ScopedAtom.d.ts` — exported as the `ScopedAtom` namespace. A per-component-tree scoped atom: the
factory builds an atom (optionally from a provided `Input`), `use()` reads it under the nearest
`Provider`, and `Context` is the backing React context. With `Input = never` the `Provider` takes
no `value`; otherwise it requires one.

```ts
type TypeId = "~@effect-atom/atom-react/ScopedAtom"
const TypeId: TypeId

interface ScopedAtom<A extends Atom.Atom<any>, Input = never> {
  readonly [TypeId]: TypeId
  use(): A
  Provider: Input extends never
    ? React.FC<{ readonly children?: React.ReactNode | undefined }>
    : React.FC<{ readonly children?: React.ReactNode | undefined; readonly value: Input }>
  Context: React.Context<A>
}

const make: <A extends Atom.Atom<any>, Input = never>(f: (() => A) | ((input: Input) => A)) => ScopedAtom<A, Input>
```

## [5]-[REACT_HYDRATION]

`ReactHydration.d.ts` — the SSR boundary component. Consumes `Hydration.DehydratedAtom` (page
`effect-atom.md` §Hydration) produced server-side by `Hydration.dehydrate` and rehydrates the
client `Registry` before first paint.

```ts
interface HydrationBoundaryProps {
  state?: Iterable<Hydration.DehydratedAtom>
  children?: React.ReactNode
}
const HydrationBoundary: React.FC<HydrationBoundaryProps>
```

