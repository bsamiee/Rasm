# [API_CATALOGUE] @radix-ui/react-visually-hidden

`@radix-ui/react-visually-hidden` supplies the `VisuallyHidden` component and `VISUALLY_HIDDEN_STYLES` constant, which render a `<span>` that is invisible to sighted users but present in the accessibility tree. It is consumed by any ui-stack owner that needs screen-reader-only labels, descriptions, or announcements without impacting visual layout.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@radix-ui/react-visually-hidden`
- package: `@radix-ui/react-visually-hidden`
- module: `@radix-ui/react-visually-hidden`
- namespace: named exports from `dist/index.d.ts`
- asset: accessible visually-hidden span primitive
- rail: render

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: visually-hidden family
- rail: render

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]                                                                                                                                                                        | [RAIL]                                      |
| :-----: | :----------------------- | :----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :------------------------------------------ |
|  [01]   | `VisuallyHidden`         | `ForwardRefExoticComponent<VisuallyHiddenProps & RefAttributes<HTMLSpanElement>>`                                                                                                    | accessible hidden span                      |
|  [02]   | `VisuallyHiddenProps`    | interface extending `PrimitiveSpanProps`                                                                                                                                             | full HTML span prop surface                 |
|  [03]   | `VISUALLY_HIDDEN_STYLES` | `Readonly<{ position: "absolute"; border: 0; width: 1; height: 1; padding: 0; margin: -1; overflow: "hidden"; clip: "rect(0, 0, 0, 0)"; whiteSpace: "nowrap"; wordWrap: "normal" }>` | CSS rules for screen-reader-only visibility |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: visually-hidden construction
- rail: render

| [INDEX] | [SURFACE]                           | [ENTRY_FAMILY]  | [RAIL]                                               |
| :-----: | :---------------------------------- | :-------------- | :--------------------------------------------------- |
|  [01]   | `VisuallyHidden`                    | primary export  | renders `<span>` with screen-reader-only CSS applied |
|  [02]   | `Root` (alias for `VisuallyHidden`) | named re-export | Radix-style named default export                     |
|  [03]   | `VISUALLY_HIDDEN_STYLES`            | style constant  | readonly CSS object for manual application           |

## [04]-[IMPLEMENTATION_LAW]

[VISUALLY_HIDDEN_TOPOLOGY]:
- `VisuallyHiddenProps` extends `PrimitiveSpanProps` (`React.ComponentPropsWithoutRef<typeof Primitive.span>`); the full span prop surface including ARIA attributes is available
- `VISUALLY_HIDDEN_STYLES` uses absolute positioning with zero effective dimensions, `overflow: "hidden"`, `clip: "rect(0, 0, 0, 0)"`, `whiteSpace: "nowrap"`, and `wordWrap: "normal"` — the canonical pattern for screen-reader-only content
- ref is typed as `HTMLSpanElement`

[LOCAL_ADMISSION]:
- Use `VisuallyHidden` to wrap text labels, instructions, or announcements that must be accessible but not visually present.
- Use `VISUALLY_HIDDEN_STYLES` only when a `<span>` cannot be inserted (e.g., applying the same hiding rules to a non-Radix element).

[RAIL_LAW]:
- Package: `@radix-ui/react-visually-hidden`
- Owns: screen-reader-accessible visually hidden span
- Accept: children as the hidden content; standard span props
- Reject: CSS-only `sr-only` utility classes when the Radix primitive is available in the dependency tree
