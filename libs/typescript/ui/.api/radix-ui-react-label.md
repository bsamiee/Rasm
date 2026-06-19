# [API_CATALOGUE] @radix-ui/react-label

`@radix-ui/react-label` supplies the `Label` component, a thin accessible wrapper over the native `<label>` element built on `@radix-ui/react-primitive`. It forwards a ref to the underlying `HTMLLabelElement` and exposes the full HTML label prop surface via `LabelProps`, consumed by every form-field owner in the ui stack that needs a correctly associated accessible label.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@radix-ui/react-label`
- package: `@radix-ui/react-label`
- module: `@radix-ui/react-label`
- namespace: named exports from `dist/index.d.ts`
- asset: accessible label primitive
- rail: render

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: label family
- rail: render

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY]                                                             | [RAIL]                       |
| :-----: | :----------- | :------------------------------------------------------------------------ | :--------------------------- |
|  [01]   | `Label`      | `ForwardRefExoticComponent<LabelProps & RefAttributes<HTMLLabelElement>>` | accessible label root        |
|  [02]   | `LabelProps` | interface extending `PrimitiveLabelProps`                                 | full HTML label prop surface |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: label construction
- rail: render

| [INDEX] | [SURFACE]                  | [ENTRY_FAMILY]  | [RAIL]                                                |
| :-----: | :------------------------- | :-------------- | :---------------------------------------------------- |
|  [01]   | `Label`                    | primary export  | renders `<label>`, forwards ref to `HTMLLabelElement` |
|  [02]   | `Root` (alias for `Label`) | named re-export | Radix-style named default export                      |

## [04]-[IMPLEMENTATION_LAW]

[LABEL_TOPOLOGY]:
- `LabelProps` extends `PrimitiveLabelProps`, which is `React.ComponentPropsWithoutRef<typeof Primitive.label>`
- the full native label prop surface (`htmlFor`, `form`, ARIA attributes, event handlers) is available without any additional wrapping
- ref is typed as `HTMLLabelElement`

[LOCAL_ADMISSION]:
- Use `Label` with `htmlFor` pointing to the controlled input's `id` for correct accessibility association.
- `LabelProps` is the only type to import; do not reference `PrimitiveLabelProps` directly outside this package.

[RAIL_LAW]:
- Package: `@radix-ui/react-label`
- Owns: accessible HTML label primitive
- Accept: `htmlFor` for explicit input association
- Reject: plain `<label>` elements when the Radix primitive is available in the dependency tree
