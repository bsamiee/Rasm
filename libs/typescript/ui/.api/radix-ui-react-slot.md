# [TS_UI_API_RADIX_UI_REACT_SLOT]

`@radix-ui/react-slot` mints the `asChild` merge primitive: `Slot` clones its single child and composes props, refs, and event handlers onto it, so a styled atom renders as any host element with zero wrapper DOM. `createSlot(ownerName)` mints each polymorphic atom as one row, foreclosing a hand-rolled `React.cloneElement`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@radix-ui/react-slot`
- package: `@radix-ui/react-slot` (MIT)
- module: ESM `dist/index.mjs` + CJS `dist/index.js`; `sideEffects: false`; one `.` barrel, no subpaths
- runtime: React render-time only — no DOM read, effect, or async; internalizes `@radix-ui/react-compose-refs` as its one dependency
- rail: `view/compose` — the `asChild` element-override primitive

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the two exported types a slot consumer annotates against — the merged-prop shape and the reconciler contract.

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [CAPABILITY]                                                                           |
| :-----: | :----------------------- | :------------ | :------------------------------------------------------------------------------------- |
|  [01]   | `SlotProps<Elem, Props>` | type alias    | `Props & { children?, mergeProps? }` — host props plus a per-slot reconciler override  |
|  [02]   | `MergePropsFunction`     | interface     | `(slotProps, childProps) -> ReturnProps` — the contract a custom reconciler implements |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the factory, its default instances, the sibling-interleave marker, and the swappable reconciler.

| [INDEX] | [SURFACE]                                 | [SHAPE]   | [CAPABILITY]                                                                    |
| :-----: | :---------------------------------------- | :-------- | :------------------------------------------------------------------------------ |
|  [01]   | `createSlot(ownerName) -> Slot`           | factory   | mints a named `Slot`; `ownerName` rides devtools and the single-child invariant |
|  [02]   | `Slot` / `Root`                           | component | the default merge instance; `Slot === Root`                                     |
|  [03]   | `createSlottable(ownerName) -> Slottable` | factory   | mints a named marker carrying the `__radixId: symbol` brand                     |
|  [04]   | `Slottable`                               | component | marks which child receives the merge among static siblings                      |
|  [05]   | `mergeProps(slotProps, childProps)`       | function  | the exported default reconciler, reusable standalone                            |
|  [06]   | `SlotProvider` / `Provider`               | component | sets a custom `mergeProps` for every descendant `Slot`                          |

- `Slottable`: `{children}` marks the slotted child directly; `{child, children: (slottable) => node}` drives the render-fn wrapper mode.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `Slot` clones exactly one React-element child and reconciles overlapping props through the default `mergeProps`; a text node, fragment, or multiple elements trips the `ownerName`-named single-child invariant.
- event handlers (`on*`) compose — the child's own fires first, then the component's, both run, and the child handler's return survives.
- `style` shallow-merges child-wins; `className` concatenates component then child; every other prop is child-wins (`{ ...slotProps, ...childProps }`); `ref` composes through the internalized `@radix-ui/react-compose-refs`.
- `Slottable` interleaves static siblings: `<Icon/><Slottable>{children}</Slottable><Chevron/>` lands the merge on the marked child while icon and chevron render as ordinary siblings, so `asChild` survives a decorated trigger.
- `SlotProvider` over a subtree or the per-`Slot` `mergeProps` prop over one instance swaps the whole reconciler algorithm, never an inline patch of a single prop's merge.

[STACKING]:
- `class-variance-authority`/`clsx`/`tailwind-merge` (`.api/class-variance-authority.md`, `.api/clsx.md`, `.api/tailwind-merge.md`): `<Slot className={cn(...)}>` lands the `cva`-folded variant class on the caller's element under `asChild`, so `<Button asChild><a/></Button>` styles the anchor with no wrapper node.
- `@radix-ui/react-label`/`@radix-ui/react-separator` (`.api/radix-ui-react-label.md`, `.api/radix-ui-react-separator.md`): both render `@radix-ui/react-primitive`, which is `Slot` behind an `asChild` flag, so each inherits this exact merge under its own owner name rather than re-cloning.
- `react-aria-components` (`.api/react-aria-components.md`): the aria spine owns element override through each component's `render` prop; `Slot.asChild` owns override for the non-aria `cva` atoms off the react-aria state machine — one override per node, an RAC `render` and a `Slot` never stacked on one element.
- `@effect-atom/atom-react` (`.api/effect-atom-atom-react.md`): `useAtomValue` resolves the child element's props (`href`, `isDisabled`) and `Slot` relays them onto the host element, the state binding staying the one fold while `Slot` only forwards the resolved props.
- within-lib `view/compose`: a polymorphic atom names its slot with `createSlot(name)`, forwards its `cva` class through `Slot className`, and lands each new atom as a row on the compose spine.

[LOCAL_ADMISSION]:
- folder-local to the `ui` composition plane; render-time only, no runtime side-effect.
- Bound asset is the workspace-catalog declaration surface; transitive store copies stay ignored.

[RAIL_LAW]:
- Package: `@radix-ui/react-slot`
- Owns: the `asChild` element-override merge — clone one child, compose props, refs, and event handlers — via `createSlot`, the `Slottable` sibling-interleave marker, and the default `mergeProps` reconciler with the `SlotProvider` context that swaps it for descendant slots.
- Accept: `<Slot {...props}>{singleChild}</Slot>` for polymorphic atoms; `createSlot(ownerName)` to name a slot; `Slottable` to interleave siblings around the slotted child; `cva`/`clsx`/`twMerge` classes forwarded through `Slot className`; a custom `mergeProps` via `SlotProvider` or the per-`Slot` prop.
- Reject: `React.cloneElement` or manual prop-merge where `Slot` owns reconciliation; `Slot` on a react-aria component whose `render` owns override; more than one element child or a bare text/fragment under a `Slot`; a second `asChild` over an element already overridden by RAC `render`; re-implementing the default merge inline where `mergeProps`/`SlotProvider` swaps it.
