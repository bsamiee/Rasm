# [API_CATALOGUE] @radix-ui/react-slot

`@radix-ui/react-slot` is the `asChild` polymorphism owner: `createSlot<Elem, Props>(ownerName)` is the one parameterized factory that mints a `ForwardRefExoticComponent` merging its own props and forwarded ref onto a single child element, and `Slot` is its `HTMLElement`/`HTMLAttributes` default instance. `Slottable`/`createSlottable` mark which child inside a compound render tree receives the merge, branded by a `__radixId: symbol` the runtime locates without name matching. This is the primitive every `asChild`-polymorphic surface in `ui` composes — one `cva`+`twMerge` recipe carried onto any element shape through `Slot`, never a `React.cloneElement` hand-roll (`interaction/command.md#COMMAND_SURFACE`, `interaction/role.md#ROLE_BEHAVIOR`).

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@radix-ui/react-slot`
- package / version: `@radix-ui/react-slot` @ `1.3.0`
- license: `MIT`
- module: dual ESM `dist/index.mjs` + CJS `dist/index.js`; types `dist/index.d.ts`
- asset: `node_modules/@radix-ui/react-slot/dist/index.d.ts` (TSDECL)
- exports: `Slot`, `Slot as Root`, `type SlotProps`, `Slottable`, `createSlot`, `createSlottable`
- dependency: `@radix-ui/react-compose-refs` (the ref-merge primitive `Slot` folds owner + child refs through); peer `react` / `react-dom` / `@types/react` `19.x`
- augmentation: `declare module 'react' { interface ReactElement { $$typeof?: symbol | string } }` — widens `ReactElement` so `Slot` can inspect a child element's `$$typeof` brand during merge
- rail: render

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: slot factory and its default instance
- rail: render

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]        | [NOTE]                                                                                                          |
| :-----: | :------------------------- | :------------------- | :------------------------------------------------------------------------------------------------------------- |
|  [01]   | `SlotProps<Elem, Props>`   | generic props alias  | `Props & { children?: React.ReactNode }`; `Elem extends Element = HTMLElement`, `Props = React.HTMLAttributes<Elem>` — the merge surface is parameterized on element + prop shape |
|  [02]   | `Slot`                     | forward-ref value    | `ForwardRefExoticComponent<React.HTMLAttributes<HTMLElement> & { children?: ReactNode } & RefAttributes<HTMLElement>>` — the `createSlot` `HTMLElement` default instance |

[PUBLIC_TYPE_SCOPE]: slottable merge-target marker
- rail: render

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]        | [NOTE]                                                                                                          |
| :-----: | :------------------------- | :------------------- | :------------------------------------------------------------------------------------------------------------- |
|  [01]   | `SlottableComponent`       | branded interface    | `React.FC<SlottableProps> & { __radixId: symbol }` — the brand the merge algorithm scans for, immune to display-name mangling |
|  [02]   | `SlottableProps`           | union                | `SlottableRenderFnProps \| SlottableChildrenProps` — the plain-children form and the render-prop form of the marker |
|  [03]   | `SlottableChildrenProps`   | plain-children shape | `{ children: React.ReactNode }`                                                                                |
|  [04]   | `SlottableRenderFnProps`   | render-prop shape    | `{ child: React.ReactNode; children: (slottable: React.ReactNode) => React.ReactNode }` — projects the located slot child through a render function |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: mint / instance — the parameterized factory and its bound instances
- rail: render

| [INDEX] | [SURFACE]                            | [ENTRY_FAMILY]    | [NOTE]                                                                                          |
| :-----: | :----------------------------------- | :---------------- | :--------------------------------------------------------------------------------------------- |
|  [01]   | `createSlot<Elem, Props>(ownerName)` | factory           | `(ownerName: string) => ForwardRefExoticComponent<PropsWithoutRef<SlotProps<Elem, Props>> & RefAttributes<Elem>>` — the one owner of every element/prop merge shape; `ownerName` tags dev-mode diagnostics |
|  [02]   | `Slot` / `Root`                      | default instance  | `createSlot` bound to `HTMLElement` + `HTMLAttributes`; `Root` is the Radix-idiom re-export of the same value |
|  [03]   | `createSlottable(ownerName)`         | marker factory    | `(ownerName: string) => SlottableComponent` — mints a named, `__radixId`-branded merge-target marker |
|  [04]   | `Slottable`                          | default marker    | the `createSlottable` default instance                                                          |

## [04]-[IMPLEMENTATION_LAW]

[SLOT_TOPOLOGY]:
- `Slot` accepts exactly one React child; it merges its own props onto that child (event handlers compose — the owner's and the child's both fire), merges `style`/`className`, and forwards the owner ref onto the child through `@radix-ui/react-compose-refs`
- `createSlot<Elem, Props>` is the parameterization axis: pass a non-`HTMLElement` `Elem` or a narrower `Props` when the merged child is an SVG/custom element or the owner exposes a bounded prop surface; `Slot` is the `HTMLElement` instance, never a separate implementation
- `Slottable` designates the merge target inside a compound tree: an owner rendering an icon and a label around a single `asChild` element wraps the mergeable child in `Slottable` so `Slot` merges onto it and leaves the siblings intact; `__radixId` locates it structurally, so the marker survives minification and re-export
- `SlottableRenderFnProps` projects the located child through `children: (slottable) => …`, letting a compound owner reorder or wrap the merged child without losing the merge

[STACKING]:
- asChild variant element (canonical): `const Comp = asChild ? Slot : "div"` then render `Comp` with `className: cn(recipe({ …variants }))` — `Slot` carries the one `cva`+`tailwind-merge` `cn` recipe (`class-variance-authority.md`, `tailwind-merge.md`) onto whatever element the caller passes as the child, so a single `VariantProps`-typed component owns every element shape with no `cloneElement` and no per-element prop-forwarding boilerplate (`interaction/command.md#COMMAND_SURFACE`, the `ActionItem` owner)
- compound merge: wrap the mergeable child in `Slottable` when the `asChild` element is not the outermost node (icon + `Slottable(children)` under one `Slot`), so the variant recipe and ref land on the intended child and the icon stays a static sibling
- role vocabulary leaf: `Slot` is the DOM-merge leaf the headless `actions` `RoleBehavior` rows render through (`interaction/role.md#ROLE_BEHAVIOR` — button/toggle/toolbar), spreading the row's `aria` map and focus ref onto the caller's element; the behavior is owned by the role, the element polymorphism by `Slot`
- ref rail: `createSlot` returns a `ForwardRefExoticComponent`, so an owner's `React.forwardRef` and the child's own ref merge through `@radix-ui/react-compose-refs` into one node — the same ref the `overlay/floating.md` anchor and the focus model read
- compiler: under `babel-plugin-react-compiler` (`interaction/role.md`) the `Comp`/`cn(...)` selection needs no `useMemo`; the compiler memoizes the element identity

[LOCAL_ADMISSION]:
- use `Slot` directly when one `HTMLElement` merge shape covers the callers; reach for `createSlot<Elem, Props>` only when the element type is non-HTML or the prop surface must narrow in the type signature
- `Slottable` is required only in compound components where the merged child is not the outermost element; a single-child owner needs `Slot` alone
- extend the one `asChild`-polymorphic owner with a `cva` variant row before minting a second slot component for the same surface

[RAIL_LAW]:
- package: `@radix-ui/react-slot`
- owns: `asChild` polymorphic element composition — prop merge, event composition, and ref forwarding onto one child
- accept: `createSlot` as the factory, `Slot`/`Root` as the HTML instance, `Slottable`/`createSlottable` as the compound merge-target marker, a single valid-element child
- reject: `React.cloneElement`-based or hand-rolled prop-merge `asChild`; a per-owner bespoke slot when `createSlot<Elem, Props>` parameterizes the shape; name-matching a merge child instead of the `__radixId` brand
