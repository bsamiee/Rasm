# [TS_UI_API_TYPES_REACT]

`@types/react` is the declaration-only (`.d.ts`) type surface for the `react` runtime — the foundation every `ui` sibling catalog composes against and the reason `tsc`/`tsgo` is the real gate for the whole folder (there is no runtime output to test). Its whole surface is a small set of PARAMETERIZED type families instanced many ways, never a flat member list: `SyntheticEvent<T, E>` is one event contract instanced per input device, `HTMLAttributes<T>` is one attribute base extended per element, `ComponentProps<T>` is one polymorphic prop extractor discriminating tag-vs-component, and `Ref<T>`/`RefObject<T>`/`RefCallback<T>` is one ref algebra — a new element, event, or widget is a row in the owning family, never a new mechanism. It pairs with the `react` runtime (`.api/react.md`) the way `react-stately` pairs with `react-aria`: one package split across behavior and types. React 19 is the baseline — `ref` is a plain prop (`forwardRef` soft-deprecated), ref callbacks return a cleanup, `use`/`useActionState`/`useOptimistic`/`useFormStatus` land the async-and-form primitives, and react-compiler (`./compiler-runtime`) compiles memoization so `useMemo`/`useCallback` are never hand-written in a `view` row.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@types/react`
- package: `@types/react`
- license: `MIT` (DefinitelyTyped)
- deps: `csstype` (`CSSProperties extends CSS.Properties<string | number>`)
- catalog-verdict: KEEP
- asset: declaration-only — NO runtime, NO ABI; `tsc`/`tsgo` typechecks against it and it emits nothing. The `react` runtime (`.api/react.md`) is the paired package; these are its types
- marker: react-compiler is enabled folder-wide, so `./compiler-runtime` (intentionally export-empty, autocomplete-hidden) is a real dependency; `global.d.ts` declares the ambient `JSX` namespace used by every `.tsx`
- exports: `.` (the full surface), `./jsx-runtime` + `./jsx-dev-runtime` (the automatic-runtime `jsx`/`jsxs`/`Fragment` the compiler emits), `./canary` (`ViewTransition`/`Activity`/`addTransitionType`), `./experimental` (`unstable_SuspenseList`/`unstable_startGestureTransition`), `./compiler-runtime`
- react-peer: consumed AS the `react` module's types; version-locked to the runtime major (19) — `@types/react-dom` (`.api/types-react-dom.md`) declares `@types/react` as its peer

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the element + component value vocabulary
- rail: view/primitive
- The renderable-value algebra: `ReactNode` is what a component may return, `ReactElement`/`ReactPortal` the element values, and the component-type union (`ComponentType`, `FC`, `ElementType`, `ExoticComponent` family) the callable shapes. Every `view` row's return type and every child slot types against this; a `Schema`-decoded value becomes renderable only by passing through `ReactNode`.

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CONSUMER_BOUNDARY] |
|:-----: |:----------------------------------------------------------------------- |:---------------- |:----------------------------------------------------------------------- |
| [01] | `ReactNode` / `ReactElement<P, T>` / `ReactPortal` / `Key` | renderable value | `view` — the return/`children` type of every row; `ReactPortal` is `react-dom` `createPortal`'s result |
| [02] | `FunctionComponent<P>` (`FC<P>`) / `ComponentType<P>` / `ComponentClass<P>` | component shape | `view/primitive` — a row is an `FC<P>`; `ComponentType` is the `memo`/`lazy`/HOC input union |
| [03] | `ElementType<P>` / `JSXElementConstructor<P>` | polymorphic tag | `view/compose` — the `as`/polymorphic-element pattern (`@radix-ui/react-slot`) types its element against `ElementType` |
| [04] | `ExoticComponent<P>` / `NamedExoticComponent` / `ForwardRefExoticComponent` / `MemoExoticComponent<T>` / `LazyExoticComponent<T>` / `ProviderExoticComponent` | exotic family | the result types of `Fragment`/`Suspense`/`forwardRef`/`memo`/`lazy`/`createContext` — one family, one instance per factory |
| [05] | `Context<T>` / `Provider<T>` / `Consumer<T>` / `ContextType<C>` / `ProviderProps<T>` | context carrier | `atom/binding` `RegistryContext`, `intl/format` `I18nProvider` context — `ContextType` extracts the value |
| [06] | `PropsWithChildren<P>` / `Attributes` / `ClassAttributes<T>` / `RefAttributes<T>` | prop mixin | shared prop shapes; `RefAttributes<T>` carries the `ref` prop, `PropsWithChildren` the `children` slot |

[PUBLIC_TYPE_SCOPE]: the props + ref + CSS algebra — one polymorphic extractor family
- rail: view/compose
- `ComponentProps<T>` is ONE extractor discriminating `T` on `keyof JSX.IntrinsicElements | JSXElementConstructor`; the `WithRef`/`WithoutRef`/`ComponentRef` variants project the same input. A `view` row NEVER hand-authors a prop interface for a wrapped element — it lifts the target's props with this family. `Ref<T>` is the whole ref algebra: React 19 accepts a `RefObject` directly and a `RefCallback` may return a cleanup.

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CONSUMER_BOUNDARY] |
|:-----: |:----------------------------------------------------------------------------------- |:----------------- |:---------------------------------------------------------------- |
| [01] | `ComponentProps<T>` / `ComponentPropsWithRef<T>` / `ComponentPropsWithoutRef<T>` / `ComponentRef<T>` | prop extractor | `view/compose` — lift a wrapped element/component's props + ref-target; supersedes a hand-written interface |
| [02] | `Ref<T>` = `RefCallback<T> \| RefObject<T \| null> \| null` / `RefObject<T>` (`{ current: T }`) | ref algebra | `act/gesture` + `view` — React 19 `ref` value; `RefObject.current` is mutable, `MutableRefObject` is the retired alias |
| [03] | `RefCallback<T>` = `(instance: T \| null) => void \| (() => void)` | ref callback | `view` — React 19 ref callbacks return a cleanup; `react-aria` `useObjectRef`/`mergeRefs` reconcile these |
| [04] | `ForwardedRef<T>` / `PropsWithoutRef<P>` / `PropsWithRef<P>` | ref plumbing | the `forwardRef` render-fn ref param + prop ref-stripping; retired where `ref` is now a plain prop |
| [05] | `CSSProperties` (`extends CSS.Properties<string \| number>`) | style value | `token/theme` + `token/scale` — the typed `style` prop; custom `--*` props via the string index; typed by `csstype` |
| [06] | `ElementRef<T>` (deprecated → `ComponentRef<T>`) / `LegacyRef<T>` (deprecated) | retired ref | never author new — `ComponentRef<T>` is the ref-target extractor; string refs are gone |

[PUBLIC_TYPE_SCOPE]: the hook-contract + concurrent-primitive types
- rail: atom/binding
- The type shapes the hook signatures thread: state dispatch, reducer/action arity, effect callbacks, and the React 19 concurrent primitives (`Usable` for `use`, the transition function shapes, the action-state arity). `atom/binding` and `view` rows type their callbacks against these rather than re-declaring closures.

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CONSUMER_BOUNDARY] |
|:-----: |:----------------------------------------------------------------------- |:---------------- |:------------------------------------------------------------------- |
| [01] | `Dispatch<A>` / `SetStateAction<S>` / `DispatchWithoutAction` | state setter | `view` — the `useState`/`useReducer` setter; `@effect-atom` write hooks mirror this shape |
| [02] | `Reducer<S, A>` / `ReducerWithoutAction<S>` / `ActionDispatch<Arg>` / `AnyActionArg` | reducer/action | `atom/derive` — `useReducer`/`useActionState` arity; `AnyActionArg = [] \| [any]` is the action-fn tail |
| [03] | `EffectCallback` / `DependencyList` / `Destructor` | effect shape | `view` — `useEffect`/`useLayoutEffect`/`useInsertionEffect` bodies; `Destructor` is the cleanup return |
| [04] | `Usable<T>` = `ReactPromise<T> \| Context<T>` / `ReactPromise<T>` | `use` input | `view` — `use(usable)` unwraps a promise or context in render; the promise arm suspends |
| [05] | `TransitionFunction` / `TransitionStartFunction` | transition scope | `act/transition` — `startTransition`/`useTransition` scope fn; an async fn defers to a non-blocking transition |

[PUBLIC_TYPE_SCOPE]: the synthetic-event family — one contract, instanced per device
- rail: act/gesture
- `SyntheticEvent<T, E>` is ONE cross-browser event contract parameterized by target element `T` and native event `E`; the roster below is SEED DATA for that one type — a new event kind is a row, never a new mechanism. `EventHandler<E>` and its per-event aliases are the matching handler contract. `act/gesture` prefers `react-aria`'s normalized `PressEvent`/`HoverEvent` over raw synthetic events, but the prop-level handler types are these.

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CONSUMER_BOUNDARY] |
|:-----: |:--------------------------------------------------------------------------------- |:---------------- |:---------------------------------------------------------------- |
| [01] | `SyntheticEvent<T, E>` / `BaseSyntheticEvent<E, C, T>` | event base | the root all event rows extend; carries `currentTarget`/`nativeEvent`/`preventDefault` |
| [02] | `MouseEvent<T, E>` / `PointerEvent<T>` / `DragEvent<T>` / `WheelEvent<T>` / `TouchEvent<T>` | pointer event | `act/gesture` — pointer/touch rows; `PointerEvent`/`DragEvent`/`WheelEvent` extend `MouseEvent` |
| [03] | `KeyboardEvent<T>` (`ModifierKey`) / `FocusEvent<Target, RelatedTarget>` | keyboard/focus | `act/gesture` — key + focus rows; `getModifierState(key: ModifierKey)` on `KeyboardEvent` |
| [04] | `ChangeEvent<C, T>` / `FormEvent<T>` / `InputEvent<T>` / `SubmitEvent<T>` / `InvalidEvent<T>` | form event | `view/compose` `FormBinding` — the input/submit handler payloads a `Schema` decode validates |
| [05] | `ClipboardEvent<T>` / `CompositionEvent<T>` / `AnimationEvent<T>` / `TransitionEvent<T>` / `ToggleEvent<T>` / `UIEvent<T>` | ancillary event | `act/transition` — `TransitionEvent`/`AnimationEvent` fire the CSS-motion end; `ToggleEvent` the `<dialog>`/popover state |
| [06] | `EventHandler<E>` + `MouseEventHandler<T>` / `KeyboardEventHandler<T>` / `ChangeEventHandler<C,T>` / … | handler alias | the `on*` prop types; one bivariant `EventHandler<E>`, one alias per event row |

[PUBLIC_TYPE_SCOPE]: the DOM-attribute family + JSX intrinsics + canary surface
- rail: view/primitive
- `HTMLAttributes<T>` is ONE attribute base (over `AriaAttributes` + `DOMAttributes<T>`) extended per element into `*HTMLAttributes<T>`; `JSX.IntrinsicElements` maps each tag to `DetailedHTMLProps<*HTMLAttributes<E>, E>`. A `react-aria` hook returns a bundle that spreads onto one of these. The canary/overlay rows are the gated upgrade surface `act/transition` owns.

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CONSUMER_BOUNDARY] |
|:-----: |:--------------------------------------------------------------------------------- |:---------------- |:---------------------------------------------------------------- |
| [01] | `DOMAttributes<T>` / `HTMLAttributes<T>` / `AllHTMLAttributes<T>` / `SVGAttributes<T>` | attribute base | `view/primitive` — the spread target for a `react-aria` `DOMAttributes` bundle; `AllHTMLAttributes` is the union |
| [02] | `AriaAttributes` / `AriaRole` / `HTMLInputTypeAttribute` / `HTMLAttributeReferrerPolicy` | aria + enum vocab | `view` — the `aria-*` prop set `react-aria` populates; `AriaRole` the `role` union |
| [03] | `*HTMLAttributes<T>` (`ButtonHTMLAttributes`/`InputHTMLAttributes`/`AnchorHTMLAttributes`/`DialogHTMLAttributes`/…) | per-element attrs | one interface per element extending `HTMLAttributes<T>` — SEED DATA; a new element is a row |
| [04] | `JSX.IntrinsicElements` / `JSX.Element` / `JSX.ElementType` / `JSX.LibraryManagedAttributes` | JSX namespace | every `.tsx` — the tag→props map the compiler resolves; `global.d.ts` makes it ambient |
| [05] | `DetailedHTMLProps<E, T>` / `HTMLProps<T>` / `SVGProps<T>` | detailed props | the `IntrinsicElements` cell shape (element attrs + `ClassAttributes<T>` ref/key) |
| [06] | `ViewTransitionProps` / `ViewTransitionClass` / `ActivityProps` / `FragmentInstance` / `SuspenseListProps` (canary/overlay) | gated upgrade | `act/transition` — the `<ViewTransition>`/`<Activity>` prop types; import from `./canary`/`./experimental` only |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the typed hook set — the React 19 primitives
- rail: view/primitive
- `@types/react` types the `react` runtime's hooks; the roster is the React 19 primitive set grouped by concern. `view` rows call these directly (react-compiler compiles the memoization), and `@effect-atom` hooks (`useAtomValue`, `.api/effect-atom-atom-react.md`) are built ON `useSyncExternalStore`. A hook is a call into the render-tracked fiber; there is no polymorphic collapse across primitives, but the FORM shapes (`Dispatch`, `Usable`, `EffectCallback`) are shared.

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY] |
|:-----: |:------------------------------------------------------------------------------------------ |:--------------- |:--------------------------------------------------------------- |
| [01] | `useState<S>` / `useReducer<S, A>` / `useRef<T>` / `useContext<T>` | state + context | `view` — but the ONE store binding is `@effect-atom`; local `useState` only for ephemeral view state |
| [02] | `useEffect` / `useLayoutEffect` / `useInsertionEffect` / `useImperativeHandle` / `useEffectEvent` | effect | `view` — `useEffectEvent<T>(cb)` (main entry, not `./experimental`) extracts a non-reactive event from an effect's deps |
| [03] | `useMemo<T>` / `useCallback<T>` / `useDebugValue<T>` | memo | react-compiler OWNS these — never hand-write in a row; present for library/interop code only |
| [04] | `useTransition` / `useDeferredValue<T>` / `startTransition` / `useId` / `useSyncExternalStore` | concurrent | `act/transition` defers work; `useSyncExternalStore` is the external-store subscription `@effect-atom` builds on |
| [05] | `use<T>(usable: Usable<T>)` | unwrap | `view` — unwrap a promise (suspends) or context in render/loops; the `Result`-atom async seam |
| [06] | `useActionState<State, Payload>` / `useOptimistic<State, Action>` | form action | `view/compose` `FormBinding` — server-action pending/error state + optimistic UI; pairs `react-dom` `useFormStatus` |
| [07] | `unstable_useCacheRefresh` / `addTransitionType` / `unstable_startGestureTransition` (canary/overlay) | gated | `act/transition` — behind a capability flag; import from `./canary`/`./experimental` |

[ENTRYPOINT_SCOPE]: the typed factories + exotic components
- rail: view/compose
- The element factories and the `ExoticComponent`-typed built-ins. React 19 makes `ref` a plain prop, so `forwardRef` is soft-deprecated — a new row takes `ref` in props, not through `forwardRef`. JSX (automatic runtime `./jsx-runtime`) is the normal element-construction path; `createElement` is the escape hatch.

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY] |
|:-----: |:------------------------------------------------------------------------------------------ |:--------------- |:--------------------------------------------------------------- |
| [01] | `createElement` / `cloneElement` / `isValidElement<P>` / `Children` / `createRef<T>` | element factory | escape hatch — JSX (`./jsx-runtime` `jsx`/`jsxs`) is the normal path; `Children` maps opaque `children` |
| [02] | `createContext<T>` → `Context<T>` | context factory | `atom/binding` `RegistryContext`, `intl` `I18nProvider` — the one context per cross-tree value |
| [03] | `forwardRef<T, P>` (soft-deprecated) / `memo<P>` / `lazy<T>` | component factory | React 19: `ref` is a prop, so author rows without `forwardRef`; `memo` redundant under react-compiler; `lazy` for code-split |
| [04] | `Fragment` / `Suspense` / `StrictMode` / `Profiler` (`ProfilerOnRenderCallback`) | built-in exotic | `view` — `Suspense` boundary pairs `useAtomSuspense`; `Profiler` feeds `viewer/probe` render timing |
| [05] | `ViewTransition` (`ExoticComponent<ViewTransitionProps>`) / `Activity` (`ExoticComponent<ActivityProps>`) | gated exotic | `act/transition` — the `<ViewTransition>`/`<Activity>` upgrade rows; `./canary`, capability-gated |
| [06] | `act<T>(cb)` / `version` | test/identity | `dev`-plane specs drive updates through `act`; `@effect/vitest` rows assert on flushed state |

## [04]-[IMPLEMENTATION_LAW]

[TYPE_TOPOLOGY]:
- Parameterized families, not flat lists: `SyntheticEvent<T, E>` is one event contract instanced per device, `HTMLAttributes<T>` one attribute base extended per element, `ComponentProps<T>` one extractor discriminating tag-vs-component, `Ref<T>`/`RefObject<T>`/`RefCallback<T>` one ref algebra, and the `ExoticComponent` set one family typing every built-in/factory result. Reach for the family member; never re-declare a parallel prop interface, event shape, or ref type a family already owns.
- `ComponentProps<T>` is the anti-duplication law: a row wrapping a `<button>` or a `cmdk`/`radix` component lifts `ComponentPropsWithRef<typeof Target>` and extends it — it never hand-authors the prop set. `PropsWithChildren<P>` adds the `children` slot; `VariantProps` (`.api/class-variance-authority.md`) contributes the styling axes; the three intersect into the row's prop type.
- The event/attribute vocabulary is the DOM contract, not the interaction API: a `view` row's `on*` props type against the `EventHandler` aliases, but `act/gesture` binds behavior through `react-aria`'s normalized `PressEvent`/`HoverEvent` (`.api/react-aria.md`), never raw synthetic-event listeners — the synthetic types are the spread-bundle target, the aria hooks the source.

[REACT_19_LAW]:
- `ref` is a plain prop: function components receive `ref` in props, so a row is authored without `forwardRef` (soft-deprecated, still typed for interop). `RefCallback<T>` returns `void | (() => void)` — a ref callback may return a cleanup, so `react-aria` `useObjectRef` reconciliation and imperative attach/detach are cleanup-based. `RefObject<T>.current` is mutable; `MutableRefObject`/`ElementRef`/`LegacyRef` are retired aliases and string refs are gone.
- Async and forms are primitives: `use(usable)` unwraps a promise (suspending) or context in render and in loops/conditionals; `useActionState`/`useOptimistic` model server-action pending-error-optimistic state; `useFormStatus` (`react-dom`) reads ambient form-submission state. A `FormBinding` row folds these with a `Schema` decode — never a manual `isSubmitting`/`error` boolean pair.
- react-compiler owns memoization: `./compiler-runtime` is the enabled runtime, so `useMemo`/`useCallback`/`memo` are compiled, not hand-written in a `ui` row; they remain in the type surface only for library and interop code. `useEffectEvent` extracts the non-reactive slice of an effect so a handler reads latest values without widening the dependency list.

[STACKS_WITH]:
- `react` (`.api/react.md`): the paired runtime — these types type that package's exports. Import named ESM symbols (`import { useId, type ReactNode } from "react"`); the `React.*` namespace is available but the named form is canonical. One package, split behavior/types like `react-aria`/`react-stately`.
- `@types/react-dom` (`.api/types-react-dom.md`): declares `@types/react` as its peer and shares `ReactNode`/`ReactElement`/`Ref`/`Container`; `createPortal`'s result is a `ReactPortal`, `flushSync` forces the commit `act/transition` and `react-aria` `FocusScope` depend on. The DOM renderer types over these element types.
- `react-aria` / `react-stately` (`.api/react-aria.md`): every `use<Widget>` hook returns a record of `DOMAttributes`/`HTMLAttributes` bundles a `view` row spreads onto a JSX element; the hooks consume `Ref<T>` and emit the `aria-*`/`role`/event props typed here. `react-aria` is the behavior over this type vocabulary; the types are the DOM contract it targets.
- `@effect-atom/atom-react` (`.api/effect-atom-atom-react.md`): the hooks build on `useSyncExternalStore` and integrate `Suspense`/`use`/`startTransition`; `useAtomValue`'s selector overload replaces the `useMemo`-over-selector idiom, so the memo hooks stay compiler-owned. The atom is the store, these types are the render projection.
- `class-variance-authority` (`.api/class-variance-authority.md`): `VariantProps<typeof cva>` lifts the variant axes into a component's prop type, intersected with `ComponentProps<T>` and `PropsWithChildren` — the three-way prop composition that is a `view/compose` row's typed surface.
- `effect` `Schema` (`libs/typescript/.api/effect.md`): a `Schema.standardSchemaV1` decoder validates form input and its `ParseError` projects into the `react-aria` `ValidationResult`; `useActionState`'s payload and `ChangeEvent` target type against the decoded value, so one `Schema` owns wire decode and live field validity. Renderable `Schema` output crosses into the tree only as `ReactNode`.
- `cmdk` / `@radix-ui/*` / `@floating-ui/react` / `@tanstack/react-*` (sibling `.api/*.md`): every third-party React component is typed through `ComponentProps`/`ComponentPropsWithRef`/`ExoticComponent` and returns `ReactNode` — a `view/compose` row wraps them by lifting their props, never re-declaring them.

[LOCAL_ADMISSION]:
- Import named ESM symbols and `import type` the type-only ones; type a wrapped element/component with `ComponentPropsWithRef<T>` + `PropsWithChildren` + `VariantProps`, never a parallel hand-written prop interface.
- Author rows with `ref` as a plain prop; reach for `forwardRef`/`memo` only in interop code — react-compiler owns memoization, so no `useMemo`/`useCallback` in a row.
- Type `on*` props against the `EventHandler` aliases but source interaction behavior from `react-aria` normalized events; never bind raw synthetic-event listeners for gesture/press/hover.
- Fold async and forms through `use`/`useActionState`/`useOptimistic`/`useFormStatus` + a `Schema` decode; never thread manual loading/error booleans. Gate `ViewTransition`/`Activity`/`addTransitionType` behind a capability flag from `./canary`.

[RAIL_LAW]:
- Package: `@types/react`
- Owns: the React 19 type vocabulary — the element/component value algebra (`ReactNode`/`ReactElement`/`FC`/`ComponentType`/`ExoticComponent`), the `ComponentProps`/`Ref`/`CSSProperties` prop-and-ref extractor family, the hook-contract shapes (`Dispatch`/`Usable`/`EffectCallback`/`TransitionFunction`), the `SyntheticEvent<T,E>` + `EventHandler` event family, the `HTMLAttributes<T>` + `JSX.IntrinsicElements` DOM-attribute family, and the typed hook/factory/exotic runtime surface
- Accept: named ESM imports paired with `react`, `ComponentPropsWithRef<T>` prop lifting, `ref` as a plain prop, react-compiler-owned memoization, `use`/`useActionState`/`useOptimistic` async-form primitives, `react-aria` normalized events over raw synthetic listeners, `./canary` gated upgrades
- Reject: hand-written prop interfaces where `ComponentProps` lifts them, `forwardRef`/`memo`/`useMemo`/`useCallback` in a row under react-compiler, string refs / `MutableRefObject` / `ElementRef` / `LegacyRef` (retired), raw synthetic-event listeners for gestures, manual loading/error booleans where the form primitives + `Schema` fold them, ungated canary imports on the stable path
