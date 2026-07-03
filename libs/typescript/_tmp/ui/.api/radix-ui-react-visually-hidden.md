# [API_CATALOGUE] @radix-ui/react-visually-hidden

`@radix-ui/react-visually-hidden` renders a `<span>` invisible to sighted users but present in the accessibility tree, plus the raw `VISUALLY_HIDDEN_STYLES` object for the rare element that cannot be wrapped. `VisuallyHidden` is a `@radix-ui/react-primitive` `Primitive.span`, so it inherits the full span prop surface AND the Radix `asChild` slot merge — it composes onto a caller element through the same `@radix-ui/react-slot` substrate the `interaction/command.md` `cn`/`Slot` recipe owns. This is the standalone Radix a11y primitive admitted alongside `@radix-ui/react-label` and `@radix-ui/react-separator`; it is distinct from `react-aria`'s `VisuallyHidden`/`useVisuallyHidden` (which carry `isFocusable` for skip links) and `react-aria-components`' re-export.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@radix-ui/react-visually-hidden`
- package: `@radix-ui/react-visually-hidden`
- version: `1.2.7`
- license: `MIT`
- module: `@radix-ui/react-visually-hidden` (single entry, `dist/index.d.ts`)
- namespace: named exports `VisuallyHidden`, `Root`, `VISUALLY_HIDDEN_STYLES`, `VisuallyHiddenProps`
- asset: dual CJS/ESM (`dist/index.js` / `dist/index.mjs`), `sideEffects: false`, client React
- runtime: browser DOM (peer `react ^19`, `react-dom ^19`), builds on `@radix-ui/react-primitive`
- rail: render / screen-reader-only span

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: visually-hidden family
- rail: render

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]                                                                        | [RAIL]                                       |
| :-----: | :----------------------- | :---------------------------------------------------------------------------------- | :------------------------------------------- |
|  [01]   | `VisuallyHidden`         | `ForwardRefExoticComponent<VisuallyHiddenProps & RefAttributes<HTMLSpanElement>>`   | accessible hidden span                       |
|  [02]   | `VisuallyHiddenProps`    | `interface extends PrimitiveSpanProps` (empty body; inherits the full span surface) | full span props + `asChild?: boolean`        |
|  [03]   | `VISUALLY_HIDDEN_STYLES` | `Readonly<{ position:"absolute"; border:0; width:1; height:1; padding:0; margin:-1; overflow:"hidden"; clip:"rect(0, 0, 0, 0)"; whiteSpace:"nowrap"; wordWrap:"normal" }>` | raw CSS rules for the un-wrappable element    |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: visually-hidden construction
- rail: render

| [INDEX] | [SURFACE]                           | [ENTRY_FAMILY]  | [RAIL]                                                    |
| :-----: | :---------------------------------- | :-------------- | :------------------------------------------------------- |
|  [01]   | `VisuallyHidden`                    | primary export  | renders `<span>` (or the `asChild` child) with SR-only CSS |
|  [02]   | `Root`                              | named re-export | Radix-namespace alias for `VisuallyHidden`               |
|  [03]   | `VISUALLY_HIDDEN_STYLES`            | style constant  | readonly CSS object applied to a non-wrappable element   |

## [04]-[IMPLEMENTATION_LAW]

[VISUALLY_HIDDEN_TOPOLOGY]:
- `VisuallyHiddenProps extends PrimitiveSpanProps` (`React.ComponentPropsWithoutRef<typeof Primitive.span>`) — an empty interface body that inherits every native span attribute, every ARIA attribute, and the Radix `asChild?: boolean` slot flag from `@radix-ui/react-primitive`
- `asChild` merges the SR-only styling and props onto a single caller child through Radix `Slot` rather than emitting a wrapper `<span>`, the same merge primitive `command.md` composes for `ActionElement`
- `VISUALLY_HIDDEN_STYLES` uses absolute positioning with unitless `1`px effective dimensions (React coerces the numeric values to px), `margin: -1`, `overflow:"hidden"`, `clip:"rect(0, 0, 0, 0)"`, `whiteSpace:"nowrap"`, `wordWrap:"normal"` — the canonical clip-rect pattern; content stays in the a11y tree while occupying no visual space
- ref forwards to the rendered `HTMLSpanElement` (or the `asChild` element)

[LOCAL_ADMISSION]:
- wrap SR-only labels, instructions, or dialog titles in `VisuallyHidden`; pass `asChild` to fold the hiding onto an existing element instead of nesting a span
- apply `VISUALLY_HIDDEN_STYLES` inline only when no element can be inserted — a `::before` pseudo-target, a third-party node
- `Root` and `VisuallyHidden` are the same component; prefer the plain name outside a Radix-namespace import block

[STACKING]:
- sibling Radix primitives: shares the `@radix-ui/react-primitive` + `@radix-ui/react-slot` `asChild` substrate with `@radix-ui/react-label` and `@radix-ui/react-separator`, so the `role.md` headless roles that admit the Radix trio compose one consistent slot merge across label/separator/hidden
- sibling `react-aria` / `react-aria-components`: those ship their own `VisuallyHidden` — `react-aria`'s adds `isFocusable` (skip-link reveal on focus) and `elementType`, and `useVisuallyHidden` returns a `visuallyHiddenProps` bag for the hook path; use the Radix primitive when composing with other Radix `asChild` primitives, the `react-aria` surface inside the interaction spine or for a focusable skip link
- universal tier: no `effect` seam — this is a pure render primitive with no state, effect, or async surface; a static-`sr-only` Tailwind utility beside the admitted primitive is the deleted defect

[RAIL_LAW]:
- package: `@radix-ui/react-visually-hidden`
- owns: the screen-reader-accessible, visually-hidden span and its raw style object
- accept: children as hidden content, any span/ARIA prop, `asChild` for slot merge, a `ref` to the span
- reject: a CSS-only `sr-only` utility when the primitive is in the tree, a hand-rolled clip-rect style block beside `VISUALLY_HIDDEN_STYLES`, confusing this primitive with `react-aria`'s focusable variant
