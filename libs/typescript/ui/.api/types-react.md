# [TS_UI_API_TYPES_REACT]

`@types/react` is the declaration-only `.d.ts` surface for the `react` runtime — no runtime output ships, so `tsc` gates the whole `ui` folder here. Its surface is a small set of parameterized type families instanced many ways: `SyntheticEvent<T, E>` one event contract per input device, `HTMLAttributes<T>` one attribute base per element, `ComponentProps<T>` one prop extractor discriminating tag-vs-component, and `Ref<T>`/`RefObject<T>`/`RefCallback<T>` one ref algebra — a new element, event, or widget is a row in the owning family, never a new mechanism.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@types/react`
- package: `@types/react` (MIT)
- module: declaration-only `.d.ts`; subpaths `.` (full surface), `./jsx-runtime` + `./jsx-dev-runtime` (automatic-runtime `jsx`/`jsxs`/`Fragment` the compiler emits), `./canary` (`ViewTransition`/`addTransitionType`), `./experimental` (`unstable_SuspenseList`/`unstable_startGestureTransition`), `./compiler-runtime`
- asset: no runtime, no ABI — `tsc` typechecks against it and it emits nothing; consumed as the `react` module's types (`.api/react.md`), the paired runtime
- depends: `csstype` (`CSSProperties extends CSS.Properties<string | number>`)
- marker: react-compiler enabled folder-wide, so `./compiler-runtime` (export-empty, autocomplete-hidden) is a real dependency; `global.d.ts` declares the ambient `JSX` namespace every `.tsx` uses
- rail: the React type vocabulary — `tsc` gates the whole `ui` folder against it, no runtime output to test

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the element + component value vocabulary — `ReactNode` is what a component may return, `ReactElement`/`ReactPortal` the element values, and the component-type union (`ComponentType`, `FC`, `ElementType`, `ExoticComponent` family) the callable shapes; every `view` row's return type and every child slot types against this, and a `Schema`-decoded value becomes renderable only by passing through `ReactNode`.

| [INDEX] | [SYMBOL]                                                                             | [TYPE_FAMILY]    |
| :-----: | :----------------------------------------------------------------------------------- | :--------------- |
|  [01]   | `ReactNode` / `ReactElement<P, T>` / `ReactPortal` / `Key`                           | renderable value |
|  [02]   | `FunctionComponent<P>` (`FC<P>`) / `ComponentType<P>` / `ComponentClass<P>`          | component shape  |
|  [03]   | `ElementType<P>` / `JSXElementConstructor<P>`                                        | polymorphic tag  |
|  [04]   | `ExoticComponent<P>` / `NamedExoticComponent` / `ForwardRefExoticComponent`          | exotic family    |
|  [05]   | `MemoExoticComponent<T>` / `LazyExoticComponent<T>` / `ProviderExoticComponent`      | exotic family    |
|  [06]   | `Context<T>` / `Provider<T>` / `Consumer<T>` / `ContextType<C>` / `ProviderProps<T>` | context carrier  |
|  [07]   | `PropsWithChildren<P>` / `Attributes` / `ClassAttributes<T>` / `RefAttributes<T>`    | prop mixin       |

[CONSUMER_BOUNDARY]:
- [01]: `view` return/`children` type of every row; `ReactPortal` = the `react-dom` `createPortal` result
- [02]: `FC<P>` types a `view` row; `ComponentType` is the `memo`/`lazy`/HOC input union
- [03]: `ElementType` types the `as`/polymorphic-element pattern (`@radix-ui/react-slot`)
- [04]: result types of `Fragment`/`Suspense`/`forwardRef`
- [05]: result types of `memo`/`lazy`/`createContext` — one family, one instance per factory
- [06]: `atom/binding` `RegistryContext`, `intl/format` `I18nProvider`; `ContextType` extracts the value
- [07]: `RefAttributes<T>` carries the `ref` prop, `PropsWithChildren` the `children` slot

[PUBLIC_TYPE_SCOPE]: the props + ref + CSS algebra, one polymorphic extractor family — `ComponentProps<T>` is ONE extractor discriminating `T` on `keyof JSX.IntrinsicElements | JSXElementConstructor`, the `WithRef`/`WithoutRef`/`ComponentRef` variants projecting the same input, so a `view` row lifts a wrapped element's props with this family rather than hand-authoring a prop interface; `Ref<T>` is the whole ref algebra — React accepts a `RefObject` directly and a `RefCallback` may return a cleanup.

| [INDEX] | [SYMBOL]                                                                                             | [TYPE_FAMILY]  |
| :-----: | :--------------------------------------------------------------------------------------------------- | :------------- |
|  [01]   | `ComponentProps<T>` / `ComponentPropsWithRef<T>` / `ComponentPropsWithoutRef<T>` / `ComponentRef<T>` | prop extractor |
|  [02]   | `Ref<T>` = `RefCallback<T> \| RefObject<T \| null> \| null` / `RefObject<T>` (`{ current: T }`)      | ref algebra    |
|  [03]   | `RefCallback<T>` = `(instance: T \| null) => void \| (() => void)`                                   | ref callback   |
|  [04]   | `ForwardedRef<T>` / `PropsWithoutRef<P>` / `PropsWithRef<P>`                                         | ref plumbing   |
|  [05]   | `CSSProperties` (`extends CSS.Properties<string \| number>`)                                         | style value    |
|  [06]   | `ElementRef<T>` (deprecated → `ComponentRef<T>`) / `LegacyRef<T>` (deprecated)                       | retired ref    |

[CONSUMER_BOUNDARY]:
- [01]: `view/compose` — lift a wrapped element/component's props + ref-target; supersedes a hand-written interface
- [02]: `act/gesture` + `view` — React `ref` value; `RefObject.current` mutable, `MutableRefObject` the retired alias
- [03]: `view` — React ref callbacks return a cleanup; `react-aria` `useObjectRef`/`mergeRefs` reconcile these
- [04]: `forwardRef` render-fn ref param + prop ref-stripping; retired where `ref` is a plain prop
- [05]: `token/theme` + `token/scale` — the typed `style` prop; custom `--*` props via the string index; typed by `csstype`
- [06]: never author new — `ComponentRef<T>` is the ref-target extractor; string refs are gone

[PUBLIC_TYPE_SCOPE]: the hook-contract + concurrent-primitive types — state dispatch, reducer/action arity, effect callbacks, and the concurrent primitives (`Usable` for `use`, the transition function shapes, the action-state arity); `atom/binding` and `view` rows type their callbacks against these rather than re-declaring closures.

| [INDEX] | [SYMBOL]                                                                             | [TYPE_FAMILY]    |
| :-----: | :----------------------------------------------------------------------------------- | :--------------- |
|  [01]   | `Dispatch<A>` / `SetStateAction<S>` / `DispatchWithoutAction`                        | state setter     |
|  [02]   | `Reducer<S, A>` / `ReducerWithoutAction<S>` / `ActionDispatch<Arg>` / `AnyActionArg` | reducer/action   |
|  [03]   | `EffectCallback` / `DependencyList` / `Destructor`                                   | effect shape     |
|  [04]   | `Usable<T>` = `ReactPromise<T> \| Context<T>` / `ReactPromise<T>`                    | `use` input      |
|  [05]   | `TransitionFunction` / `TransitionStartFunction`                                     | transition scope |

[CONSUMER_BOUNDARY]:
- [01]: `view` — the `useState`/`useReducer` setter; `@effect-atom` write hooks mirror this shape
- [02]: `atom/derive` — `useReducer`/`useActionState` arity; `AnyActionArg = [] \| [any]` is the action-fn tail
- [03]: `view` — `useEffect`/`useLayoutEffect`/`useInsertionEffect` bodies; `Destructor` is the cleanup return
- [04]: `view` — `use(usable)` unwraps a promise or context in render; the promise arm suspends
- [05]: `act/transition` — `startTransition`/`useTransition` scope fn; an async fn defers to a non-blocking transition

[PUBLIC_TYPE_SCOPE]: the synthetic-event family, one contract instanced per device — `SyntheticEvent<T, E>` is ONE cross-browser event contract parameterized by target element `T` and native event `E`, the roster below its seed data (a new event kind is a row, never a new mechanism), and `EventHandler<E>` with its per-event aliases the matching handler contract; `act/gesture` binds behavior through `react-aria`'s normalized `PressEvent`/`HoverEvent`, and these prop-level handler types are the spread target.

| [INDEX] | [SYMBOL]                                                                                               | [TYPE_FAMILY]   |
| :-----: | :----------------------------------------------------------------------------------------------------- | :-------------- |
|  [01]   | `SyntheticEvent<T, E>` / `BaseSyntheticEvent<E, C, T>`                                                 | event base      |
|  [02]   | `MouseEvent<T, E>` / `PointerEvent<T>` / `DragEvent<T>` / `WheelEvent<T>` / `TouchEvent<T>`            | pointer event   |
|  [03]   | `KeyboardEvent<T>` (`ModifierKey`) / `FocusEvent<Target, RelatedTarget>`                               | keyboard/focus  |
|  [04]   | `ChangeEvent<C, T>` / `FormEvent<T>` / `InputEvent<T>` / `SubmitEvent<T>` / `InvalidEvent<T>`          | form event      |
|  [05]   | `ClipboardEvent<T>` / `CompositionEvent<T>` / `AnimationEvent<T>`                                      | ancillary event |
|  [06]   | `TransitionEvent<T>` / `ToggleEvent<T>` / `UIEvent<T>`                                                 | ancillary event |
|  [07]   | `EventHandler<E>` + `MouseEventHandler<T>` / `KeyboardEventHandler<T>` / `ChangeEventHandler<C,T>` / … | handler alias   |

[CONSUMER_BOUNDARY]:
- [01]: `SyntheticEvent<T, E>` is the root all event rows extend, carrying `currentTarget`/`nativeEvent`/`preventDefault`
- [02]: `act/gesture` — pointer/touch rows; `PointerEvent`/`DragEvent`/`WheelEvent` extend `MouseEvent`
- [03]: `act/gesture` — key + focus rows; `getModifierState(key: ModifierKey)` on `KeyboardEvent`
- [04]: `view/compose` `FormBinding` — the input/submit handler payloads a `Schema` decode validates
- [05]: `act/transition` — `AnimationEvent` fires the CSS-motion end
- [06]: `TransitionEvent` fires the CSS-motion end; `ToggleEvent` the `<dialog>`/popover state
- [07]: `EventHandler<E>` types the `on*` props — one bivariant handler, one alias per event row

[PUBLIC_TYPE_SCOPE]: the DOM-attribute family + JSX intrinsics + canary surface — `HTMLAttributes<T>` is ONE attribute base (over `AriaAttributes` + `DOMAttributes<T>`) extended per element into `*HTMLAttributes<T>`, and `JSX.IntrinsicElements` maps each tag to `DetailedHTMLProps<*HTMLAttributes<E>, E>`; a `react-aria` hook returns a bundle spreading onto one of these, and the canary/overlay rows are the gated upgrade surface `act/transition` owns.

| [INDEX] | [SYMBOL]                                                                                     | [TYPE_FAMILY]     |
| :-----: | :------------------------------------------------------------------------------------------- | :---------------- |
|  [01]   | `DOMAttributes<T>` / `HTMLAttributes<T>` / `AllHTMLAttributes<T>` / `SVGAttributes<T>`       | attribute base    |
|  [02]   | `AriaAttributes` / `AriaRole` / `HTMLInputTypeAttribute` / `HTMLAttributeReferrerPolicy`     | aria + enum vocab |
|  [03]   | `*HTMLAttributes<T>` (`ButtonHTMLAttributes`/`InputHTMLAttributes`/`AnchorHTMLAttributes`/…) | per-element attrs |
|  [04]   | `JSX.IntrinsicElements` / `JSX.Element` / `JSX.ElementType` / `JSX.LibraryManagedAttributes` | JSX namespace     |
|  [05]   | `DetailedHTMLProps<E, T>` / `HTMLProps<T>` / `SVGProps<T>`                                   | detailed props    |
|  [06]   | `ViewTransitionProps` / `ViewTransitionClass` / `ActivityProps`                              | transition props  |
|  [07]   | `FragmentInstance` / `SuspenseListProps` (canary/overlay)                                    | gated upgrade     |

[CONSUMER_BOUNDARY]:
- [01]: `view/primitive` — the spread target for a `react-aria` `DOMAttributes` bundle; `AllHTMLAttributes` the union
- [02]: `view` — the `aria-*` prop set `react-aria` populates; `AriaRole` the `role` union
- [03]: one interface per element extending `HTMLAttributes<T>` — SEED DATA; a new element is a row
- [04]: every `.tsx` — the tag→props map the compiler resolves; `global.d.ts` makes it ambient
- [05]: `DetailedHTMLProps` is the `IntrinsicElements` cell shape (element attrs + `ClassAttributes<T>` ref/key)
- [06]: `act/transition` — `<ViewTransition>` props import from `./canary`; `<Activity>`/`ActivityProps` ship on the main surface
- [07]: `act/transition` — the `SuspenseList` shapes; import from `./experimental` only

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the typed hook set, the React primitive roster grouped by concern — `view` rows call these directly (react-compiler compiles the memoization) and `@effect-atom` hooks (`useAtomValue`, `.api/effect-atom-atom-react.md`) build ON `useSyncExternalStore`; a hook is a call into the render-tracked fiber, and the FORM shapes (`Dispatch`, `Usable`, `EffectCallback`) are shared across primitives even where the calls do not collapse.

| [INDEX] | [SURFACE]                                                                                             | [ENTRY_FAMILY]  |
| :-----: | :---------------------------------------------------------------------------------------------------- | :-------------- |
|  [01]   | `useState<S>` / `useReducer<S, A>` / `useRef<T>` / `useContext<T>`                                    | state + context |
|  [02]   | `useEffect` / `useLayoutEffect` / `useInsertionEffect` / `useImperativeHandle` / `useEffectEvent`     | effect          |
|  [03]   | `useMemo<T>` / `useCallback<T>` / `useDebugValue<T>`                                                  | memo            |
|  [04]   | `useTransition` / `useDeferredValue<T>` / `startTransition` / `useId` / `useSyncExternalStore`        | concurrent      |
|  [05]   | `use<T>(usable: Usable<T>)`                                                                           | unwrap          |
|  [06]   | `useActionState<State, Payload>` / `useOptimistic<State, Action>`                                     | form action     |
|  [07]   | `unstable_useCacheRefresh` / `addTransitionType` / `unstable_startGestureTransition` (canary/overlay) | gated           |

[CONSUMER_BOUNDARY]:
- [01]: `view` — but the ONE store binding is `@effect-atom`; local `useState` only for ephemeral view state
- [02]: `view` — `useEffectEvent<T>(cb)` (main entry, not `./experimental`) extracts a non-reactive event from a deps list
- [03]: react-compiler OWNS these — never hand-write in a row; present for library/interop code only
- [04]: `act/transition` defers work; `useSyncExternalStore` is the external-store subscription `@effect-atom` builds on
- [05]: `view` — unwrap a promise (suspends) or context in render/loops; the `Result`-atom async seam
- [06]: `view/compose` `FormBinding` — server-action pending/error + optimistic UI; pairs `react-dom` `useFormStatus`
- [07]: `act/transition` — behind a capability flag; import from `./canary`/`./experimental`

[ENTRYPOINT_SCOPE]: the typed factories + exotic components — React makes `ref` a plain prop, so `forwardRef` is soft-deprecated and a new row takes `ref` in props; JSX (automatic runtime `./jsx-runtime`) is the normal element-construction path and `createElement` the escape hatch.

| [INDEX] | [SURFACE]                                                                                                 | [ENTRY_FAMILY]    |
| :-----: | :-------------------------------------------------------------------------------------------------------- | :---------------- |
|  [01]   | `createElement` / `cloneElement` / `isValidElement<P>` / `Children` / `createRef<T>`                      | element factory   |
|  [02]   | `createContext<T>` → `Context<T>`                                                                         | context factory   |
|  [03]   | `forwardRef<T, P>` (soft-deprecated) / `memo<P>` / `lazy<T>`                                              | component factory |
|  [04]   | `Fragment` / `Suspense` / `StrictMode` / `Profiler` (`ProfilerOnRenderCallback`)                          | built-in exotic   |
|  [05]   | `ViewTransition` (`ExoticComponent<ViewTransitionProps>`) / `Activity` (`ExoticComponent<ActivityProps>`) | transition exotic |
|  [06]   | `act<T>(cb)` / `version`                                                                                  | test/identity     |

[CONSUMER_BOUNDARY]:
- [01]: escape hatch — JSX (`./jsx-runtime` `jsx`/`jsxs`) is the normal path; `Children` maps opaque `children`
- [02]: `atom/binding` `RegistryContext`, `intl` `I18nProvider` — the one context per cross-tree value
- [03]: React: `ref` is a prop, so author rows without `forwardRef`; `memo` redundant under react-compiler; `lazy` code-splits
- [04]: `view` — `Suspense` boundary pairs `useAtomSuspense`; `Profiler` feeds `viewer/probe` render timing
- [05]: `act/transition` — `<ViewTransition>` from `./canary` (capability-gated); `<Activity>` ships stable
- [06]: `dev`-plane specs drive updates through `act`; `@effect/vitest` rows assert on flushed state

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Parameterized families, not flat lists: `SyntheticEvent<T, E>` is one event contract instanced per device, `HTMLAttributes<T>` one attribute base extended per element, `ComponentProps<T>` one extractor discriminating tag-vs-component, `Ref<T>`/`RefObject<T>`/`RefCallback<T>` one ref algebra, and the `ExoticComponent` set one family typing every built-in/factory result. Reach for the family member; never re-declare a parallel prop interface, event shape, or ref type a family already owns.
- `ComponentProps<T>` is the anti-duplication law: a row wrapping a `<button>` or a `cmdk`/`radix` component lifts `ComponentPropsWithRef<typeof Target>` and extends it — it never hand-authors the prop set. `PropsWithChildren<P>` adds the `children` slot; `VariantProps` (`.api/class-variance-authority.md`) contributes the styling axes; the three intersect into the row's prop type.
- `EventHandler` aliases type a `view` row's `on*` props as the DOM contract, not the interaction API — `act/gesture` binds behavior through `react-aria`'s normalized `PressEvent`/`HoverEvent` (`.api/react-aria.md`), never raw synthetic-event listeners; the synthetic types are the spread-bundle target, the aria hooks the source.
- `ref` is a plain prop: function components receive `ref` in props, so a row is authored without `forwardRef` (soft-deprecated, still typed for interop); `RefCallback<T>` returns `void | (() => void)`, so a ref callback may return a cleanup and `react-aria` `useObjectRef` reconciliation and imperative attach/detach are cleanup-based. `RefObject<T>.current` is mutable; `MutableRefObject`/`ElementRef`/`LegacyRef` are retired aliases and string refs are gone.
- async and forms are primitives: `use(usable)` unwraps a promise (suspending) or context in render and in loops/conditionals, `useActionState`/`useOptimistic` model server-action pending-error-optimistic state, and `useFormStatus` (`react-dom`) reads ambient form-submission state; a `FormBinding` row folds these with a `Schema` decode, never a manual `isSubmitting`/`error` boolean pair.
- react-compiler owns memoization: `./compiler-runtime` is the enabled runtime, so `useMemo`/`useCallback`/`memo` are compiled, not hand-written in a `ui` row, and they remain in the type surface only for library and interop code. `useEffectEvent` extracts the non-reactive slice of an effect so a handler reads latest values without widening the dependency list.

[STACKING]:
- `react` (`.api/react.md`): the paired runtime — these types type that package's exports. Import named ESM symbols (`import { useId, type ReactNode } from "react"`); the `React.*` namespace is available but the named form is canonical. One package, split behavior/types like `react-aria`/`react-stately`.
- `@types/react-dom` (`.api/types-react-dom.md`): declares `@types/react` as its peer and shares `ReactNode`/`ReactElement`/`Ref`/`Container`; `createPortal`'s result is a `ReactPortal`, and `flushSync` forces the commit `act/transition` and `react-aria` `FocusScope` depend on; the DOM renderer types over these element types.
- `react-aria` / `react-stately` (`.api/react-aria.md`, `.api/react-stately.md`): every `use<Widget>` hook returns a record of `DOMAttributes`/`HTMLAttributes` bundles a `view` row spreads onto a JSX element, consuming `Ref<T>` and emitting the `aria-*`/`role`/event props typed here; `react-aria` is the behavior over this type vocabulary, the types the DOM contract it targets.
- `@effect-atom/atom-react` (`.api/effect-atom-atom-react.md`): the hooks build on `useSyncExternalStore` and integrate `Suspense`/`use`/`startTransition`, and `useAtomValue`'s selector overload replaces the `useMemo`-over-selector idiom so the memo hooks stay compiler-owned; the atom is the store, these types the render projection.
- `class-variance-authority` (`.api/class-variance-authority.md`): `VariantProps<typeof cva>` lifts the variant axes into a component's prop type, intersected with `ComponentProps<T>` and `PropsWithChildren` — the three-way prop composition that is a `view/compose` row's typed surface.
- `effect` `Schema` (`libs/typescript/.api/effect.md`): a `Schema.standardSchemaV1` decoder validates form input and its `ParseError` projects into the `react-aria` `ValidationResult`; `useActionState`'s payload and `ChangeEvent` target type against the decoded value, so one `Schema` owns wire decode and live field validity. Renderable `Schema` output crosses into the tree only as `ReactNode`.
- `cmdk` / `@radix-ui/*` / `@floating-ui/react` / `@tanstack/react-*` (sibling `.api/*.md`): every third-party React component is typed through `ComponentProps`/`ComponentPropsWithRef`/`ExoticComponent` and returns `ReactNode` — a `view/compose` row wraps them by lifting their props, never re-declaring them.
- `view`/`act`/`token` (within-lib): every row types its props with `ComponentPropsWithRef` + `PropsWithChildren` + `VariantProps`, its `on*` handlers with the `EventHandler` aliases, and its `style` with `CSSProperties`.

[LOCAL_ADMISSION]:
- Import named ESM symbols and `import type` the type-only ones; type a wrapped element/component with `ComponentPropsWithRef<T>` + `PropsWithChildren` + `VariantProps`, never a parallel hand-written prop interface.
- Author rows with `ref` as a plain prop; reach for `forwardRef`/`memo` only in interop code — react-compiler owns memoization, so no `useMemo`/`useCallback` in a row.
- Type `on*` props against the `EventHandler` aliases but source interaction behavior from `react-aria` normalized events; never bind raw synthetic-event listeners for gesture/press/hover.
- Fold async and forms through `use`/`useActionState`/`useOptimistic`/`useFormStatus` + a `Schema` decode; never thread manual loading/error booleans. Gate `ViewTransition`/`addTransitionType` behind the `./canary` reference; `Activity` ships stable.

[RAIL_LAW]:
- Package: `@types/react`
- Owns: the React type vocabulary — the element/component value algebra (`ReactNode`/`ReactElement`/`FC`/`ComponentType`/`ExoticComponent`), the `ComponentProps`/`Ref`/`CSSProperties` prop-and-ref extractor family, the hook-contract shapes (`Dispatch`/`Usable`/`EffectCallback`/`TransitionFunction`), the `SyntheticEvent<T,E>` + `EventHandler` event family, the `HTMLAttributes<T>` + `JSX.IntrinsicElements` DOM-attribute family, and the typed hook/factory/exotic runtime surface
- Accept: named ESM imports paired with `react`, `ComponentPropsWithRef<T>` prop lifting, `ref` as a plain prop, react-compiler-owned memoization, `use`/`useActionState`/`useOptimistic` async-form primitives, `react-aria` normalized events over raw synthetic listeners, `./canary` gated upgrades
- Reject: hand-written prop interfaces where `ComponentProps` lifts them, `forwardRef`/`memo`/`useMemo`/`useCallback` in a row under react-compiler, string refs / `MutableRefObject` / `ElementRef` / `LegacyRef` (retired), raw synthetic-event listeners for gestures, manual loading/error booleans where the form primitives + `Schema` fold them, ungated canary imports on the stable path
