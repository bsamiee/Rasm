# [TS_UI_API_RADIX_UI_REACT_SLOT]

[PACKAGE_SURFACE]:
- package: `@radix-ui/react-slot` · version `` · license `MIT`
- module: dual — `dist/index.mjs` (ESM `module`/`import`) + `dist/index.js` (CJS `main`/`require`); `sideEffects: false`; one `.` barrel, no subpaths.
- asset: `dist/index.d.ts` (-bound `catalog`, `restore: restored`). The pnpm store also carries transitive `catalog`/`catalog` copies pulled by `cmdk`/`vaul`; the consumer-bound asset is the `pnpm-workspace.yaml` catalog `catalog` (top-level symlink), never the recovery copies `assay` may decompile.
- runtime: React render-time only — no DOM read, no effect, no async. Internalizes `@radix-ui/react-compose-refs@catalog` (the ref-merge primitive) as its single dependency.
- marker: importing the module globally augments `react`'s `ReactElement` with `$$typeof?: symbol | string` (a `declare module 'react'` type-surface side-effect, not runtime).
- plane: `plane:runtime` (W4 `ui`); folder-local to `ui`, catalogued here.
- rail: `ui/view` composition — the `asChild` element-override primitive.
- role: `view/compose.md` composition/slot rows — the `asChild` merge behind styled-atom polymorphism.

`@radix-ui/react-slot` is the merge primitive behind the `asChild` pattern: a component renders `<Slot {...props}>{child}</Slot>` in place of a fixed DOM element, and `Slot` clones the single child, composing the component's props, refs, and event handlers onto it — so a styled atom becomes ANY element (a router `<Link>`, an `<a>`, a third-party button) while keeping its behavior and styling and emitting zero wrapper DOM. This is the polymorphic-element mechanism the `view/compose` styled-atom rows ride. `createSlot(ownerName)` is the parameterized factory — a named `Slot` per owner for devtools and invariant clarity — and `Slot` is its default instance; `Slottable` marks WHICH child receives the merge when a trigger renders siblings around the slotted node. The full surface is documented because the merge/compose-refs law is the boundary this folder owns: a new polymorphic atom is a `createSlot(name)` row, never a hand-written `React.cloneElement`.

## [01]-[SLOT_CONTRACT]

The surface is one factory plus its zero-config instance, and a `Slottable` marker for sibling interleave. `createSlot`/`createSlottable` are the parameterized mechanism; `Slot`/`Slottable` are the default instances — never a hand-rolled clone per component.

| [INDEX] | [SYMBOL]                             | [KIND]    | [CAPABILITY_BOUNDARY]                                                         |
| :-----: | :----------------------------------- | :-------- | :---------------------------------------------------------------------------- |
|  [01]   | `createSlot<Elem, Props>(ownerName)` | factory   | mints a named `Slot`; `ownerName` rides devtools + the single-child invariant |
|  [02]   | `Slot` (= `Root`)                    | component | `createSlot('Slot')` — the default merge instance; `Root` is the same value   |
|  [03]   | `SlotProps<Elem, Props>`             | type      | `Props & { children?: ReactNode }` — the props merged onto the child          |
|  [04]   | `createSlottable(ownerName)`         | factory   | mints a named `Slottable` marker (carries the `__radixId` brand)              |
|  [05]   | `Slottable` (`SlottableComponent`)   | component | marks the child that receives the merge among static siblings                 |

```ts contract
// The factory IS the mechanism: one named Slot per owner component. Slot = createSlot('Slot'); Root aliases Slot.
declare function createSlot<Elem extends Element = HTMLElement, Props = React.HTMLAttributes<Elem>>(
  ownerName: string,
): React.ForwardRefExoticComponent<React.PropsWithoutRef<SlotProps<Elem, Props>> & React.RefAttributes<Elem>>
type SlotProps<Elem extends Element = HTMLElement, Props = React.HTMLAttributes<Elem>> = Props & { children?: React.ReactNode }
declare const Slot: React.ForwardRefExoticComponent<React.HTMLAttributes<HTMLElement> & { children?: React.ReactNode } & React.RefAttributes<HTMLElement>>

// Slottable interleaves non-slotted siblings around the slotted child; the render-fn form is the general case, the plain form the shorthand.
type SlottableProps = { child: React.ReactNode; children: (slottable: React.ReactNode) => React.ReactNode } | { children: React.ReactNode }
interface SlottableComponent extends React.FC<SlottableProps> { __radixId: symbol }   // the brand Slot detects among children
declare function createSlottable(ownerName: string): SlottableComponent

export { Slot as Root, Slot, type SlotProps, Slottable, createSlot, createSlottable }
```

## [02]-[MERGE_SEMANTICS]

`Slot` clones its single child and reconciles overlapping props under a fixed rule set; refs compose through the internalized `@radix-ui/react-compose-refs`. This is the behavior a consuming atom depends on, not an implementation detail to rediscover.

- Event handlers (`on*`) compose — both fire, the child's own handler first then the component's; neither is dropped, and the child handler's return value is preserved.
- `style` merges shallowly, child keys win on conflict; `className` concatenates (component then child).
- Every other prop: the component supplies the default, the child's own prop overrides it (`{ ...slotProps, ...childProps }`).
- `ref` composes via `composeRefs` — the forwarded ref and the child's ref both receive the node.
- Exactly ONE React-element child is required (or one `Slottable`-marked child among static siblings); a text node, fragment, or multiple elements is the misuse the invariant rejects (the `ownerName` names it).
- `Slottable` lets a trigger render `<Icon/><Slottable>{children}</Slottable><Chevron/>` — the merge lands on the `Slottable` child while icon/chevron render as ordinary siblings, so `asChild` survives decorated triggers.

## [03]-[INTEGRATION]

[STACK: `Slot` + `class-variance-authority` + `clsx` + `tailwind-merge` (`.api/class-variance-authority.md`, `.api/clsx.md`, `.api/tailwind-merge.md`)] — the styled-atom polymorphism: an atom computes its class via `cva(base, variants)({...})` folded through `clsx`/`twMerge`, then renders `<Slot className={cn}>` when `asChild` is set, so the variant-styled class lands on the caller's element (`<Button asChild><a/></Button>`) with no wrapper node. `createSlot(name)` names the atom's slot for its single-child invariant.

[BOUNDARY: `Slot.asChild` vs `react-aria-components` `render` (`.api/react-aria-components.md`)] — the aria spine owns element override through each component's `render?: (domProps, state) => ReactElement` DOM-override prop; `Slot` owns `asChild` for the NON-aria `token`/`view` atoms (cva-styled primitives off the react-aria state machine). One element override per node: an RAC component uses `render`, a plain atom uses `Slot` — never both stacked on one element.

[STACK: `Slot` + `@radix-ui/react-label` / `@radix-ui/react-separator` (`.api/radix-ui-react-label.md`, `.api/radix-ui-react-separator.md`)] — the sibling radix composition primitives are built on `@radix-ui/react-primitive`, which is `Slot` under an `asChild` flag; a radix `Label`/`Separator` inherits this exact merge, so the composition-plane primitives share one polymorphism mechanism rather than each re-cloning.

[STACK: `Slot` + `@effect-atom` (`.api/effect-atom-atom-react.md`)] — the merged child is an atom-driven element: `useAtomValue` resolves the element props (`href`, `isDisabled`) the `Slot` forwards; the state binding stays the one fold (`ONE_FOLD_ONE_BINDING`), and `Slot` only relays the resolved props onto the host element.

## [04]-[RAIL_LAW]

- Owns: the `asChild` element-override merge — clone one child, compose props/refs/event-handlers onto it — plus the `createSlot(name)` factory and the `Slottable` sibling-interleave marker.
- Accept: `<Slot {...props}>{singleChild}</Slot>` for polymorphic atoms; `createSlot(ownerName)` to name an atom's slot; `Slottable` to interleave static siblings around the slotted child; `cva`/`clsx`/`twMerge` classes forwarded through `Slot className`.
- Reject: a hand-written `React.cloneElement`/manual prop-merge where `Slot` owns the reconciliation; `Slot` on a react-aria component (its `render` prop owns element override); more than one element child (or a bare text/fragment) under a Slot; a second `asChild` layer over an element already overridden by RAC `render`.
- Boundary: render-time only, no runtime side-effect; the module type-augments `react`'s `ReactElement` on import. Handlers chain (both fire), `style`/`className` merge, other props child-wins; refs compose via the internalized `@radix-ui/react-compose-refs`. The bound asset is the catalog-owned declaration surface; transitive store copies are ignored.
