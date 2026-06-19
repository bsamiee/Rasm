# [API_CATALOGUE] @radix-ui/react-separator

`@radix-ui/react-separator` supplies the `Separator` component, a styled `<div>` that renders a horizontal or vertical visual divider with correct ARIA role semantics. It is built on `@radix-ui/react-primitive` and consumed by ui-stack owners that need an accessible content or menu separator.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@radix-ui/react-separator`
- package: `@radix-ui/react-separator`
- module: `@radix-ui/react-separator`
- namespace: named exports from `dist/index.d.ts`
- asset: accessible separator primitive
- rail: render

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: separator family
- rail: render

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]                                                               | [RAIL]                       |
| :-----: | :--------------- | :-------------------------------------------------------------------------- | :--------------------------- |
|  [01]   | `Separator`      | `ForwardRefExoticComponent<SeparatorProps & RefAttributes<HTMLDivElement>>` | separator root               |
|  [02]   | `SeparatorProps` | interface extending `PrimitiveDivProps`                                     | full div prop surface + axis |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: separator construction
- rail: render

| [INDEX] | [SURFACE]                      | [ENTRY_FAMILY]  | [RAIL]                                            |
| :-----: | :----------------------------- | :-------------- | :------------------------------------------------ |
|  [01]   | `Separator`                    | primary export  | renders `<div>`, forwards ref to `HTMLDivElement` |
|  [02]   | `Root` (alias for `Separator`) | named re-export | Radix-style named default export                  |

## [04]-[IMPLEMENTATION_LAW]

[SEPARATOR_TOPOLOGY]:
- `SeparatorProps` adds `orientation?: "horizontal" | "vertical"` (default `"horizontal"`) and `decorative?: boolean` to the full `PrimitiveDivProps` surface
- when `decorative` is `true` ARIA attributes are removed so the element is excluded from the accessibility tree; when `false` the element carries `role="separator"` and `aria-orientation`
- rendered element is always a `<div>`; ref type is `HTMLDivElement`

[LOCAL_ADMISSION]:
- Pass `decorative` when the separator is purely visual with no semantic content boundary meaning.
- `orientation="vertical"` requires explicit height styling from the caller.

[RAIL_LAW]:
- Package: `@radix-ui/react-separator`
- Owns: accessible horizontal and vertical separator primitive
- Accept: `orientation` and `decorative` props for semantic control
- Reject: bare `<hr>` or `<div>` separators when this primitive is available
