# [TS_UI_API_RADIX_UI_REACT_SLOT]

[PACKAGE_SURFACE]:
- package: `@radix-ui/react-slot` · license `MIT`
- module: dual — `dist/index.mjs` (ESM `module`/`import`) + `dist/index.js` (CJS `main`/`require`); `sideEffects: false`; one `.` barrel, no subpaths.
- asset: `dist/index.d.ts`, the consumer-bound declaration surface — the top-level `pnpm-workspace.yaml` catalog symlink, never the transitive store copies `cmdk`/`vaul` pull in.
- runtime: React render-time only — no DOM read, no effect, no async. Internalizes `@radix-ui/react-compose-refs` (the ref-merge primitive) as its single dependency.
- plane: `plane:runtime` (W4 `ui`); folder-local to `ui`, catalogued here.
- rail: `ui/view` composition — the `asChild` element-override primitive.
- role: `view/compose.md` composition/slot rows — the `asChild` merge behind styled-atom polymorphism.

`@radix-ui/react-slot` is the merge primitive behind `asChild`: a component renders `<Slot {...props}>{child}</Slot>`, and `Slot` clones the single child, composing props, refs, and handlers onto it — a styled atom becomes ANY element with zero wrapper DOM. `createSlot(ownerName)` mints a named `Slot` per owner; `Slottable` marks WHICH child receives the merge among siblings. `mergeProps` is the exported default reconciler; `SlotProvider` swaps it per subtree, a per-`Slot` prop per instance. A new polymorphic atom is a `createSlot(name)` row, never a hand-written `React.cloneElement`.

## [01]-[SLOT_CONTRACT]

One factory, its zero-config instance, a `Slottable` marker for sibling interleave, and a swappable reconciler form the surface. `createSlot`/`createSlottable` are the parameterized mechanism; `Slot`/`Slottable` are the default instances — never a hand-rolled clone per component. `mergeProps` is the exported default reconciler; `SlotProvider` swaps it for descendant slots.

| [INDEX] | [SYMBOL]                             | [KIND]    | [CAPABILITY_BOUNDARY]                                                             |
| :-----: | :----------------------------------- | :-------- | :-------------------------------------------------------------------------------- |
|  [01]   | `createSlot<Elem, Props>(ownerName)` | factory   | mints a named `Slot`; `ownerName` rides devtools + the single-child invariant     |
|  [02]   | `Slot` (= `Root`)                    | component | `createSlot('Slot')` — the default merge instance; `Root` is the same value       |
|  [03]   | `SlotProps<Elem, Props>`             | type      | `Props & { children?, mergeProps? }` — merged props + per-slot reconciler override |
|  [04]   | `createSlottable(ownerName)`         | factory   | mints a named `Slottable` marker (carries the `__radixId` brand)                  |
|  [05]   | `Slottable` (`SlottableComponent`)   | component | marks the child that receives the merge among static siblings                     |
|  [06]   | `mergeProps(SlotProps, ChildProps)`  | function  | the exported default reconciler — the props-merge algorithm §[02], reusable       |
|  [07]   | `MergePropsFunction`                 | type      | the reconciler contract a custom merge implements                                 |
|  [08]   | `SlotProvider` (= `Provider`)        | component | context provider setting a custom `mergeProps` for every descendant `Slot`        |

```ts signature
// createSlot IS the mechanism: one named Slot per owner component. Slot = createSlot('Slot'); Root aliases Slot.
declare function createSlot<Elem extends Element = HTMLElement, Props = React.HTMLAttributes<Elem>>(
  ownerName: string,
): React.ForwardRefExoticComponent<React.PropsWithoutRef<SlotProps<Elem, Props>> & React.RefAttributes<Elem>>
type SlotProps<Elem extends Element = HTMLElement, Props = React.HTMLAttributes<Elem>> = Props & { children?: React.ReactNode; mergeProps?: MergePropsFunction }
declare const Slot: React.ForwardRefExoticComponent<React.HTMLAttributes<HTMLElement> & { children?: React.ReactNode; mergeProps?: MergePropsFunction } & React.RefAttributes<HTMLElement>>

// Reconciliation is exported and swappable: mergeProps is the default; SlotProvider replaces it per subtree via context; a per-Slot mergeProps prop overrides one instance.
declare const mergeProps: <S extends AnyProps = AnyProps, C extends AnyProps = S, R extends AnyProps = S & C>(slotProps: S, childProps: C) => R
interface MergePropsFunction<S extends AnyProps = UnknownProps, C extends AnyProps = S, R extends AnyProps = S & C> { (slotProps: S, childProps: C): R }
declare const SlotProvider: React.FC<{ children: React.ReactNode; mergeProps: MergePropsFunction }>

// Slottable interleaves non-slotted siblings around the slotted child; the render-fn form is the general case, the plain form the shorthand.
type SlottableProps = { child: React.ReactNode; children: (slottable: React.ReactNode) => React.ReactNode } | { children: React.ReactNode }
interface SlottableComponent extends React.FC<SlottableProps> { __radixId: symbol }   // the brand Slot detects among children
declare function createSlottable(ownerName: string): SlottableComponent

export { type MergePropsFunction, SlotProvider as Provider, Slot as Root, Slot, type SlotProps, SlotProvider, Slottable, createSlot, createSlottable, mergeProps }
```

## [02]-[MERGE_SEMANTICS]

`Slot` clones its single child and reconciles overlapping props through the exported default `mergeProps` algorithm; refs compose through the internalized `@radix-ui/react-compose-refs`. This is the behavior a consuming atom depends on, not an implementation detail to rediscover.

- Event handlers (`on*`) compose — both fire, the child's own handler first then the component's; neither is dropped, and the child handler's return value is preserved.
- `style` merges shallowly, child keys win on conflict; `className` concatenates (component then child).
- Every other prop: the component supplies the default, the child's own prop overrides it (`{ ...slotProps, ...childProps }`).
- `ref` composes via `composeRefs` — the forwarded ref and the child's ref both receive the node.
- Exactly ONE React-element child is required (or one `Slottable`-marked child among static siblings); a text node, fragment, or multiple elements is the misuse the invariant rejects (the `ownerName` names it).
- `Slottable` lets a trigger render `<Icon/><Slottable>{children}</Slottable><Chevron/>` — the merge lands on the `Slottable` child while icon/chevron render as ordinary siblings, so `asChild` survives decorated triggers.
- Rules above are the exported default `mergeProps(slotProps, childProps)`; a `SlotProvider` above the tree replaces them for every descendant `Slot`, and a per-`Slot` `mergeProps` prop overrides one instance — a custom reconciler swaps the whole algorithm, never patches one prop's merge inline.

## [03]-[INTEGRATION]

[STACK: `Slot` + `class-variance-authority` + `clsx` + `tailwind-merge` (`.api/class-variance-authority.md`, `.api/clsx.md`, `.api/tailwind-merge.md`)] — the styled-atom polymorphism: an atom computes its class via `cva(base, variants)({...})` folded through `clsx`/`twMerge`, then renders `<Slot className={cn}>` when `asChild` is set, so the variant-styled class lands on the caller's element (`<Button asChild><a/></Button>`) with no wrapper node. `createSlot(name)` names the atom's slot for its single-child invariant.

[BOUNDARY: `Slot.asChild` vs `react-aria-components` `render` (`.api/react-aria-components.md`)] — the aria spine owns element override through each component's `render?: (domProps, state) => ReactElement` DOM-override prop; `Slot` owns `asChild` for the NON-aria `token`/`view` atoms (cva-styled primitives off the react-aria state machine). One element override per node: an RAC component uses `render`, a plain atom uses `Slot` — never both stacked on one element.

[STACK: `Slot` + `@radix-ui/react-label` / `@radix-ui/react-separator` (`.api/radix-ui-react-label.md`, `.api/radix-ui-react-separator.md`)] — the sibling radix composition primitives are built on `@radix-ui/react-primitive`, which is `Slot` under an `asChild` flag; a radix `Label`/`Separator` inherits this exact merge, so the composition-plane primitives share one polymorphism mechanism rather than each re-cloning.

[STACK: `Slot` + `@effect-atom` (`.api/effect-atom-atom-react.md`)] — the merged child is an atom-driven element: `useAtomValue` resolves the element props (`href`, `isDisabled`) the `Slot` forwards; the state binding stays the one fold (`ONE_FOLD_ONE_BINDING`), and `Slot` only relays the resolved props onto the host element.

## [04]-[RAIL_LAW]

- Owns: the `asChild` element-override merge — clone one child, compose props/refs/event-handlers onto it — the `createSlot(name)` factory, the `Slottable` sibling-interleave marker, and the exported default `mergeProps` reconciler with the `SlotProvider` context that swaps it for descendant slots.
- Accept: `<Slot {...props}>{singleChild}</Slot>` for polymorphic atoms; `createSlot(ownerName)` to name an atom's slot; `Slottable` to interleave static siblings around the slotted child; `cva`/`clsx`/`twMerge` classes forwarded through `Slot className`; a custom `mergeProps` via `SlotProvider` (or the per-`Slot` `mergeProps` prop) where a subtree needs a different reconciliation.
- Reject: a hand-written `React.cloneElement`/manual prop-merge where `Slot` owns the reconciliation; `Slot` on a react-aria component (its `render` prop owns element override); more than one element child (or a bare text/fragment) under a Slot; a second `asChild` layer over an element already overridden by RAC `render`; re-implementing the default merge inline where `mergeProps`/`SlotProvider` swaps it.
- Boundary: render-time only, no runtime side-effect. Handlers chain (both fire), `style`/`className` merge, other props child-wins; refs compose via the internalized `@radix-ui/react-compose-refs` — this is the exported default `mergeProps`, swappable via `SlotProvider`. Bound asset is the catalog-owned declaration surface; transitive store copies stay ignored.
