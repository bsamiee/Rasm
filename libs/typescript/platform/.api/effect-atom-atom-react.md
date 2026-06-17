# [TYPESCRIPT_API_EFFECT_ATOM_REACT]

Dependency catalogue for `@effect-atom/atom-react` (0.5.0) — the React binding of the
framework-agnostic atom engine. It owns ALL React store subscriptions for the TS view/UI surface
(the `api-ui-stack.md` page defers state to it). Grounded from installed `node_modules` `.d.ts`
declarations via `uv run python -m tools.assay api query --key @effect-atom/atom-react`; exact
spellings. **Tier**: `browser` (Nx `@nx/enforce-module-boundaries` — React hooks/components +
`Atom.windowFocusSignal`/DOM focus couple it to the browser bundle, unlike its `neutral`-tier core).

The entry `index.d.ts` is a thin React facade. It re-exports the seven framework-neutral
`@effect-atom/atom` (0.5.3) core namespaces verbatim — `Atom`, `Registry`, `Result`, `AtomRef`,
`AtomHttpApi`, `AtomRpc`, `Hydration` — whose full member surface is transcribed once on
`api-effect-atom.md` and is NOT duplicated here; consume those as
`import { Atom, Result, Registry } from "@effect-atom/atom-react"`. This page transcribes the
binding's OWN surface: the React modules `Hooks`, `RegistryContext`, `ScopedAtom`, and
`ReactHydration`. Cross-package owners (`Atom.*`, `Result.*`, `AtomRef.*`, `Registry.*`,
`Hydration.*`, `Exit`, `React`) resolve against `api-effect-atom.md` and `api-ui-stack.md`.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect-atom/atom-react`
- package: `@effect-atom/atom-react`
- entry: `@effect-atom/atom-react` (barrel) — re-export namespaces + flat-exported hooks/context
- peers: `@effect-atom/atom` (the re-exported core, page `api-effect-atom.md`); transitive `effect`, `@effect/platform`, `@effect/rpc`, `@effect/experimental`; `react` peer for the React modules
- asset: React subscription hooks (`Hooks`), per-tree registry context/provider (`RegistryContext`), scoped per-component atom factory (`ScopedAtom`), SSR hydration boundary component (`ReactHydration`)
- tier: `browser`

[ENTRY_REEXPORTS]: the barrel hoists the core namespaces and flat-exports its own hook/context members

```ts
export * as Atom from "@effect-atom/atom/Atom"            // → api-effect-atom.md §Atom
export * as Registry from "@effect-atom/atom/Registry"    // → api-effect-atom.md §Registry
export * as Result from "@effect-atom/atom/Result"        // → api-effect-atom.md §Result
export * as AtomRef from "@effect-atom/atom/AtomRef"      // → api-effect-atom.md §AtomRef
export * as AtomHttpApi from "@effect-atom/atom/AtomHttpApi"  // → api-effect-atom.md §AtomHttpApi
export * as AtomRpc from "@effect-atom/atom/AtomRpc"      // → api-effect-atom.md §AtomRpc
export * as Hydration from "@effect-atom/atom/Hydration"  // → api-effect-atom.md §Hydration
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

`RegistryContext.d.ts` — flat-exported. One `Registry` (page `api-effect-atom.md` §Registry) per
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
`api-effect-atom.md` §Hydration) produced server-side by `Hydration.dehydrate` and rehydrates the
client `Registry` before first paint.

```ts
interface HydrationBoundaryProps {
  state?: Iterable<Hydration.DehydratedAtom>
  children?: React.ReactNode
}
const HydrationBoundary: React.FC<HydrationBoundaryProps>
```

## [6]-[GAPS]

- No gaps in the binding's own React surface: every member of `Hooks.d.ts`, `RegistryContext.d.ts`,
  `ScopedAtom.d.ts`, and `ReactHydration.d.ts` is transcribed above with exact spellings.
- `ReactHydration` (§5: `HydrationBoundary`, `HydrationBoundaryProps`) is NOT hoisted onto the `.`
  barrel — `index.d.ts` re-exports only the 7 core namespaces, `ScopedAtom`, `Hooks`, and
  `RegistryContext`. `HydrationBoundary` resolves ONLY through the first-class subpath export
  `@effect-atom/atom-react/ReactHydration`. The other three React modules are subpath-exported AND
  barrel-hoisted.
- The seven re-exported core namespaces (`Atom`/`Registry`/`Result`/`AtomRef`/`AtomHttpApi`/
  `AtomRpc`/`Hydration`) are intentionally NOT re-transcribed here; their full member surface lives
  on `api-effect-atom.md` (the `@effect-atom/atom` 0.5.3 peer). The `index.d.ts` `export * as`
  re-export aliases are recorded in `[ENTRY_REEXPORTS]`.
- The `assay api query --full` reflector emits a flat 26-name scope index across the 5 dts files
  (the 7 core namespace aliases + `ScopedAtom` + 12 `Hooks` symbols + `scheduleTask`/
  `RegistryContext`/`RegistryProvider` + `TypeId`/`make` + `HydrationBoundary`/
  `HydrationBoundaryProps`); it does not emit member bodies for the `export * as` re-exports. Every
  hook/context/component signature above was transcribed directly from the installed `dist/dts` of
  `@effect-atom/atom-react` 0.5.0. No member was inferred.
