# [API_CATALOGUE] @radix-ui/react-separator

`@radix-ui/react-separator` is the accessible divider leaf: `Separator` renders a `<div>` over `@radix-ui/react-primitive` `Primitive.div`, forwards its ref to `HTMLDivElement`, and adds two behavior columns to the full native div prop surface — `orientation` (the `ORIENTATIONS` `["horizontal", "vertical"]` bounded vocabulary, default `horizontal`) drives `aria-orientation`, and `decorative` toggles between the semantic `role="separator"` form and the accessibility-tree-excluded form. It is the DOM divider the `navigation`/`collections` `RoleBehavior` rows render — menu/toolbar dividers and list-section boundaries (`interaction/role.md#ROLE_BEHAVIOR`).

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@radix-ui/react-separator`
- package / version: `@radix-ui/react-separator` @ `1.1.11`
- license: `MIT`
- module: dual ESM `dist/index.mjs` + CJS `dist/index.js`; types `dist/index.d.ts`
- asset: `node_modules/@radix-ui/react-separator/dist/index.d.ts` (TSDECL)
- exports: `Separator`, `Root`, `type SeparatorProps` — the internals `ORIENTATIONS`, `Orientation`, `PrimitiveDivProps` are not exported
- dependency: `@radix-ui/react-primitive` `2.1.7` (supplies `Primitive.div`, the `asChild`-enabled native-`<div>` wrapper); peer `react` / `react-dom` / `@types/react` `19.x`
- rail: render

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: separator component and props
- rail: render

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]                                                               | [NOTE]                                                                                                    |
| :-----: | :--------------- | :------------------------------------------------------------------------- | :------------------------------------------------------------------------------------------------------- |
|  [01]   | `Separator`      | `ForwardRefExoticComponent<SeparatorProps & RefAttributes<HTMLDivElement>>` | the divider root; `Root` is the Radix-idiom re-export of the same value                                   |
|  [02]   | `SeparatorProps` | interface extending `PrimitiveDivProps`                                     | the full native `<div>` prop surface plus `orientation?: "horizontal" \| "vertical"` (default `horizontal`) and `decorative?: boolean`; the `Orientation` axis is the internal `(typeof ORIENTATIONS)[number]` vocabulary, `PrimitiveDivProps = React.ComponentPropsWithoutRef<typeof Primitive.div>` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: separator construction
- rail: render

| [INDEX] | [SURFACE]       | [ENTRY_FAMILY]  | [NOTE]                                                                                             |
| :-----: | :-------------- | :-------------- | :------------------------------------------------------------------------------------------------ |
|  [01]   | `Separator`     | primary export  | renders `<div>`, forwards ref to `HTMLDivElement`; `orientation`/`decorative` drive the ARIA form   |
|  [02]   | `Root`          | named re-export | Radix-idiom alias of `Separator` (distinct `const`, identical type)                                |

## [04]-[IMPLEMENTATION_LAW]

[SEPARATOR_TOPOLOGY]:
- `decorative: false` (default) renders `role="separator"` and, for `orientation="vertical"`, `aria-orientation="vertical"` (horizontal is the implicit role default, so no attribute); `decorative: true` strips the role and orientation so the element leaves the accessibility tree — a purely visual rule
- the rendered element is always a `<div>`; `orientation="vertical"` has no intrinsic height, so the caller supplies the cross-axis size through `className`
- `Primitive.div` supplies `asChild`: render the divider as a merged custom element through `@radix-ui/react-slot` (`radix-ui-react-slot.md`) when the boundary node is not a literal `<div>`

[STACKING]:
- role dividers (canonical): `Separator` is the divider leaf the `navigation` role (menu/toolbar/tabs section breaks) and the `collections` role (list/grid section boundaries) render (`interaction/role.md#ROLE_BEHAVIOR`); `decorative: false` when the divider marks a real semantic content boundary, `decorative: true` when it is a visual rule between already-labeled regions
- styling: the visual rule (thickness, color, inset, `orientation`-driven axis) reads the one `cn` = `twMerge(cx(...))` recipe (`class-variance-authority.md`, `tailwind-merge.md`) and the live CSS custom properties `theming/tokens.md#THEME_TOKENS` writes — a `cva` factory keyed on `orientation` owns the horizontal/vertical class shape, never two hand-spelled class strings
- radix-primitive family: `Separator` shares the `@radix-ui/react-primitive` shape with `radix-ui-react-label.md` `Label` and `radix-ui-react-visually-hidden.md` `VisuallyHidden` — a forward-ref over `ComponentPropsWithoutRef<typeof Primitive.X>`, dual-exported as `Root`, `asChild`-capable; treat the trio as instances of one primitive pattern, not three bespoke components
- compiler: under `babel-plugin-react-compiler` (`interaction/role.md`) a `Separator` inside a role row carries no manual memoization

[LOCAL_ADMISSION]:
- pass `decorative` when the separator is a purely visual rule with no semantic boundary meaning; omit it when the divider marks a real content boundary
- supply explicit cross-axis size for `orientation="vertical"`; the `<div>` has none
- key the horizontal/vertical class shape off one `cva` `orientation` axis rather than duplicating class strings per direction

[RAIL_LAW]:
- package: `@radix-ui/react-separator`
- owns: the accessible horizontal/vertical divider primitive with `role="separator"`/`aria-orientation` semantics and a decorative opt-out
- accept: `Separator`/`Root`, `orientation` and `decorative` for semantic control, `asChild` to merge onto a custom element, the full native div prop surface
- reject: a bare `<hr>` or `<div>` divider when this primitive is in the tree; two hand-spelled class strings where one `cva` `orientation` axis owns both; a semantic `role="separator"` on a purely decorative rule (set `decorative`)
