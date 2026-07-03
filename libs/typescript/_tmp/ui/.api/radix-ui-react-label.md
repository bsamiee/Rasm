# [API_CATALOGUE] @radix-ui/react-label

`@radix-ui/react-label` is the accessible field-label leaf: `Label` renders a native `<label>` over `@radix-ui/react-primitive` `Primitive.label`, forwards its ref to `HTMLLabelElement`, exposes the full native label prop surface (`htmlFor`, `form`, ARIA, event handlers) through `LabelProps`, inherits `asChild` from the primitive, and adds one behavior over a bare `<label>` — an `onMouseDown` guard that calls `preventDefault` on a multi-click (`event.detail > 1`) so double-clicking a label does not select its text. It is the DOM leaf the `inputs` `RoleBehavior` rows render (`interaction/role.md#ROLE_BEHAVIOR`) and the label a `FormBinding` field carries alongside the `react-aria-components` `Form` `validationBehavior: "aria"` error region (`interaction/form.md#FORM_BINDING`).

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@radix-ui/react-label`
- package / version: `@radix-ui/react-label` @ `2.1.11`
- license: `MIT`
- module: dual ESM `dist/index.mjs` + CJS `dist/index.js`; types `dist/index.d.ts`
- asset: `node_modules/@radix-ui/react-label/dist/index.d.ts` (TSDECL)
- exports: `Label`, `Root`, `type LabelProps` — the derivation basis `PrimitiveLabelProps` is internal, never exported
- dependency: `@radix-ui/react-primitive` `2.1.7` (supplies `Primitive.label`, the `asChild`-enabled native-`<label>` wrapper); peer `react` / `react-dom` / `@types/react` `19.x`
- rail: render

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: label component and props
- rail: render

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY]                                                              | [NOTE]                                                                                                    |
| :-----: | :----------- | :------------------------------------------------------------------------ | :------------------------------------------------------------------------------------------------------- |
|  [01]   | `Label`      | `ForwardRefExoticComponent<LabelProps & RefAttributes<HTMLLabelElement>>` | the label root; `Root` is the Radix-idiom re-export of the same value                                     |
|  [02]   | `LabelProps` | interface extending `PrimitiveLabelProps` (empty body)                     | the full native `<label>` prop surface; `PrimitiveLabelProps = React.ComponentPropsWithoutRef<typeof Primitive.label>` is internal — import `LabelProps`, never the primitive alias |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: label construction
- rail: render

| [INDEX] | [SURFACE]       | [ENTRY_FAMILY]  | [NOTE]                                                                                             |
| :-----: | :-------------- | :-------------- | :------------------------------------------------------------------------------------------------ |
|  [01]   | `Label`         | primary export  | renders `<label>`, forwards ref to `HTMLLabelElement`, guards double-click text selection          |
|  [02]   | `Root`          | named re-export | Radix-idiom alias of `Label` (distinct `const`, identical type)                                    |

## [04]-[IMPLEMENTATION_LAW]

[LABEL_TOPOLOGY]:
- `LabelProps` is an empty extension of `PrimitiveLabelProps`, so the exact native `<label>` prop set — `htmlFor`, `form`, `id`, ARIA attributes, event handlers — is available with no extra wrapping and the ref is `HTMLLabelElement`
- `Primitive.label` supplies `asChild`: pass `asChild` to render the label as a merged custom element through `@radix-ui/react-slot` (`radix-ui-react-slot.md`) rather than a literal `<label>`
- the sole behavior beyond a native `<label>` is the `onMouseDown` selection guard (`event.detail > 1 -> preventDefault`); everything else is the native element

[STACKING]:
- field association (canonical): `Label htmlFor={fieldId}` binds to the control's `id` so a click focuses the control and assistive tech reads the accessible name; the `id` is the same literal the `inputs` `RoleBehavior` field row (`interaction/role.md#ROLE_BEHAVIOR` — field/radio/select/slider) wires onto its control
- form validity: pair `Label` with the `interaction/form.md#FORM_BINDING` `FormBinding` — `Label` names the field, the `react-aria-components` `Form` `validationBehavior: "aria"` `FieldError` slot describes its validity, and the `errorMap` (`Schema.decodeUnknownEither` -> `ParseResult.ArrayFormatter` per-path message) resolves onto that field's `aria-describedby`; the `Label` `htmlFor` and the error path share the field name
- overlap discipline: `react-aria-components` ships its own `Label` bound inside `TextField`/`NumberField`/`Select`; use the radix `Label` as the styled leaf when the `inputs` row renders a bare native control (or a `cn`-styled label over a non-react-aria element), and use the react-aria-components `Label` when the field is a full react-aria-components field — one label per control, never both
- styling: a label's variant classes read the one `cn` = `twMerge(cx(...))` recipe (`class-variance-authority.md`, `tailwind-merge.md`) and the live CSS custom properties `theming/tokens.md#THEME_TOKENS` writes, so a required/invalid label state is a `data-*` variant, not JS
- compiler: under `babel-plugin-react-compiler` (`interaction/role.md`) a `Label` inside a field row carries no manual `useMemo`/`useCallback`

[LOCAL_ADMISSION]:
- always give `Label` an explicit `htmlFor` pointing at the control `id`; an implicit-wrap association (control nested inside the label) is the weaker form
- import only `LabelProps`; `PrimitiveLabelProps` is internal and not exported
- render exactly one label per control — the radix `Label` or the react-aria-components `Label`, never both

[RAIL_LAW]:
- package: `@radix-ui/react-label`
- owns: the accessible native `<label>` primitive with a double-click selection guard and `asChild` polymorphism
- accept: `Label`/`Root`, `htmlFor` for explicit control association, `asChild` to merge onto a custom element, the full native label prop surface via `LabelProps`
- reject: a bare `<label>` when this primitive is in the tree; a second label on the same control beside the react-aria-components field label; importing the internal `PrimitiveLabelProps`
