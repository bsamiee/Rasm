# [TS_UI_API_RADIX_UI_REACT_SLOT]

`@radix-ui/react-slot` mints the `asChild` merge primitive: `Slot` clones its single child and composes props, refs, and event handlers onto it, so a styled atom renders as any host element with zero wrapper DOM. `createSlot(ownerName)` mints each polymorphic atom as one row, foreclosing a hand-rolled `React.cloneElement`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@radix-ui/react-slot`
- package: `@radix-ui/react-slot` (MIT)
- module: ESM `dist/index.mjs` + CJS `dist/index.js`; `sideEffects: false`; one `.` barrel, no subpaths
- runtime: React render-time only — no DOM read, effect, or async; internalizes `@radix-ui/react-compose-refs` as its one dependency
- rail: `system/primitive` — the `asChild` element-override primitive

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the one exported type a slot consumer annotates against — the merged-prop shape the host element receives.

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [CAPABILITY]                                                       |
| :-----: | :----------------------- | :------------ | :----------------------------------------------------------------- |
|  [01]   | `SlotProps<Elem, Props>` | type alias    | `Props & { children? }` — the host props one element child absorbs |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the two factories and the two default instances they seed — one merge component and one sibling-interleave marker.

| [INDEX] | [SURFACE]                                 | [SHAPE]   | [CAPABILITY]                                                                    |
| :-----: | :---------------------------------------- | :-------- | :------------------------------------------------------------------------------ |
|  [01]   | `createSlot(ownerName) -> Slot`           | factory   | mints a named `Slot`; `ownerName` rides devtools and the single-child invariant |
|  [02]   | `Slot` / `Root`                           | component | the default merge instance; `Slot === Root`                                     |
|  [03]   | `createSlottable(ownerName) -> Slottable` | factory   | mints a named marker carrying the `__radixId: symbol` brand                     |
|  [04]   | `Slottable`                               | component | marks which child receives the merge among static siblings                      |

- `Slottable`: `{children}` marks the slotted child directly; `{child, children: (slottable) => node}` drives the render-fn wrapper mode.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `Slot` clones exactly one React-element child and reconciles overlapping props through one fixed algorithm the package keeps private; a text node, fragment, or multiple elements trips the `ownerName`-named single-child invariant.
- event handlers (`on*`) compose — the child's own fires first, then the component's, both run, and the child handler's return survives.
- `style` shallow-merges child-wins; `className` concatenates component then child; every other prop is child-wins (`{ ...slotProps, ...childProps }`); `ref` composes through the internalized `@radix-ui/react-compose-refs`.
- `Slottable` interleaves static siblings: `<Icon/><Slottable>{children}</Slottable><Chevron/>` lands the merge on the marked child while icon and chevron render as ordinary siblings, so `asChild` survives a decorated trigger.
- `Slot` fixes its reconciliation and exposes no swap, so a prop needing a merge the algorithm does not perform resolves to its final value before it reaches the slot — the caller composes, the slot relays.

[STACKING]:
- `class-variance-authority`/`clsx`/`tailwind-merge` (`.api/class-variance-authority.md`, `.api/clsx.md`, `.api/tailwind-merge.md`): `<Slot className={cn(...)}>` lands the `cva`-folded variant class on the caller's element under `asChild`, so `<Button asChild><a/></Button>` styles the anchor with no wrapper node.
- `react-aria-components` (`.api/react-aria-components.md`): the aria spine owns element override through each component's `render` prop; `Slot.asChild` owns override for the non-aria `cva` atoms off the react-aria state machine — one override per node, an RAC `render` and a `Slot` never stacked on one element.
- `@effect-atom/atom-react` (`.api/effect-atom-atom-react.md`): `useAtomValue` resolves the child element's props (`href`, `isDisabled`) and `Slot` relays them onto the host element, the state binding staying the one fold while `Slot` only forwards the resolved props.
- within-lib `system/primitive`: a polymorphic atom names its slot with `createSlot(name)`, forwards its `cva` class through `Slot className`, and lands each new atom as a row on the primitive spine.

[LOCAL_ADMISSION]:
- folder-local to the `ui` composition plane; render-time only, no runtime side-effect.
- Bound asset is the workspace-catalog declaration surface; transitive store copies stay ignored.

[RAIL_LAW]:
- Package: `@radix-ui/react-slot`
- Owns: the `asChild` element-override merge — clone one child, compose props, refs, and event handlers under one fixed reconciliation — via `createSlot`, and the `Slottable` sibling-interleave marker.
- Accept: `<Slot {...props}>{singleChild}</Slot>` for polymorphic atoms; `createSlot(ownerName)` to name a slot; `Slottable` to interleave siblings around the slotted child; `cva`/`clsx`/`twMerge` classes forwarded through `Slot className`; a caller-resolved prop value where the fixed merge does not compose it.
- Reject: `React.cloneElement` or manual prop-merge where `Slot` owns reconciliation; `Slot` on a react-aria component whose `render` owns override; more than one element child or a bare text/fragment under a `Slot`; a second `asChild` over an element already overridden by RAC `render`.
