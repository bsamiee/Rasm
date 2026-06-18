# [API_CATALOGUE] @radix-ui/react-slot

`@radix-ui/react-slot` supplies the `Slot` component and `createSlot` factory that merge their own props and ref onto a single child element, and the `Slottable` marker that designates which child within a compound component receives the slot merge. These primitives are consumed by every Radix UI component that forwards its render to an asChild pattern and by any ui-stack owner that needs polymorphic element composition.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@radix-ui/react-slot`
- package: `@radix-ui/react-slot`
- module: `@radix-ui/react-slot`
- namespace: named exports from `dist/index.d.ts`
- asset: asChild slot primitives
- rail: render

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: slot family
- rail: render

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]                                                       | [RAIL]                                             |
| :-----: | :----------------------- | :------------------------------------------------------------------ | :------------------------------------------------- |
|   [1]   | `Slot`                   | `ForwardRefExoticComponent<SlotProps & RefAttributes<HTMLElement>>` | asChild merge root                                 |
|   [2]   | `SlotProps<Elem, Props>` | props type                                                          | `Props & { children?: ReactNode }`                 |
|   [3]   | `Slottable`              | `SlottableComponent`                                                | marks child as slot merge target                   |
|   [4]   | `SlottableProps`         | union type                                                          | `SlottableRenderFnProps \| SlottableChildrenProps` |
|   [5]   | `SlottableChildrenProps` | plain children shape                                                | `{ children: ReactNode }`                          |
|   [6]   | `SlottableRenderFnProps` | render-fn shape                                                     | `{ child: ReactNode; children: (s) => ReactNode }` |
|   [7]   | `SlottableComponent`     | interface                                                           | `FC<SlottableProps> & { __radixId: symbol }`       |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: slot construction
- rail: render

| [INDEX] | [SURFACE]                            | [ENTRY_FAMILY]    | [RAIL]                                           |
| :-----: | :----------------------------------- | :---------------- | :----------------------------------------------- |
|   [1]   | `Slot`                               | default slot      | merges props+ref onto one HTML child element     |
|   [2]   | `Root` (alias for `Slot`)            | named re-export   | Radix-style named default export                 |
|   [3]   | `createSlot<Elem, Props>(ownerName)` | slot factory      | creates a typed `ForwardRefExoticComponent` slot |
|   [4]   | `Slottable`                          | marker component  | designates the merge-target child                |
|   [5]   | `createSlottable(ownerName)`         | slottable factory | creates a named `SlottableComponent`             |

## [4]-[IMPLEMENTATION_LAW]

[SLOT_TOPOLOGY]:
- `Slot` accepts exactly one React child; that child receives all `Slot` props merged onto it
- `createSlot` binds an `ownerName` for dev-mode diagnostics and returns the same `ForwardRefExoticComponent` shape as the default `Slot`
- `Slottable` and `createSlottable` designate one child within compound-component render trees as the asChild merge target
- `SlottableComponent` brands the component with a `__radixId: symbol` so the runtime can locate the slot target without name matching

[LOCAL_ADMISSION]:
- Use `Slot` directly when one universal element shape covers all asChild callers.
- Use `createSlot<Elem, Props>` when the consuming owner has a non-HTML element type or a narrower prop surface that must be reflected in the type signature.
- `Slottable` is required only in compound components where the child that receives the merge is not the outermost element.

[RAIL_LAW]:
- Package: `@radix-ui/react-slot`
- Owns: asChild polymorphic element composition
- Accept: a single React child that is a valid element
- Reject: hand-rolled prop-merge or cloneElement-based asChild patterns
