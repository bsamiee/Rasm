# [TS_UI_API_RADIX_UI_REACT_VISUALLY_HIDDEN]

`@radix-ui/react-visually-hidden` owns the SR-only clip primitive: `VisuallyHidden` renders a `Primitive.span` whose clip styles keep the node in the accessibility tree while hiding it from sight, and `VISUALLY_HIDDEN_STYLES` exports that same frozen rule for any element without the component.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the clip component, its native prop contract, and the reusable style constant

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [CAPABILITY]                                                                    |
| :-----: | :-------------------------- | :------------ | :------------------------------------------------------------------------------ |
|  [01]   | `VisuallyHidden` (= `Root`) | component     | forwardRef `<span>` carrying the clip styles; `asChild` clips a passed child    |
|  [02]   | `VisuallyHiddenProps`       | interface     | `ComponentPropsWithoutRef<typeof Primitive.span>` — every span prop + `asChild` |
|  [03]   | `VISUALLY_HIDDEN_STYLES`    | const         | frozen clip-style object, applied directly to any element for SR-only           |

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: clip a child SR-only, clip an existing element, or apply the raw style constant

| [INDEX] | [SURFACE]                                          | [SHAPE]    | [CAPABILITY]                                                    |
| :-----: | :------------------------------------------------- | :--------- | :-------------------------------------------------------------- |
|  [01]   | `<VisuallyHidden>{label}</VisuallyHidden>`         | component  | clip content out of sight while holding it in the a11y tree     |
|  [02]   | `<VisuallyHidden asChild>{child}</VisuallyHidden>` | slot-merge | clip a passed element via `createSlot('Primitive.span')`        |
|  [03]   | `VISUALLY_HIDDEN_STYLES`                           | const      | apply the frozen clip rule to any element without the component |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `VisuallyHidden === Root`: one component under two names; the clip holds the node in the accessibility tree while removing it from sight, where `display:none`/`visibility:hidden` drops it from assistive tech.
- `asChild` is inherited: `Primitive.span` is `asChild ? createSlot('Primitive.span') : 'span'`, so the clip merges onto a passed child rather than nesting a second span, never a component-local re-clone.
- `VISUALLY_HIDDEN_STYLES` is that same frozen rule as a plain object; a bespoke SR-only element applies it directly where the component cannot wrap.

[STACKING]:
- `react-aria-components`(`.api/react-aria-components.md`) / `react-aria`(`.api/react-aria.md`): RAC re-exports `VisuallyHidden` from `react-aria/VisuallyHidden`; `react-aria` `useVisuallyHidden({isFocusable})` adds the reveal-on-focus mode for skip-links — the aria spine owns SR-only for every node it renders.
- `@react-aria/live-announcer`(`.api/react-aria-live-announcer.md`): its vanilla-DOM region copies these clip styles; status routes through `announce()`, and a hand-authored region applies `VISUALLY_HIDDEN_STYLES` to stay SR-only.
- `class-variance-authority`(`.api/class-variance-authority.md`) / `clsx`(`.api/clsx.md`) / `lucide-react`(`.api/lucide-react.md`): `VISUALLY_HIDDEN_STYLES` seeds a cva base or an `sr-only` utility; the icon-button atom pairs a `lucide-react` glyph with a `<VisuallyHidden>` label so the control carries an accessible name with no visible text.
- `@radix-ui/react-slot`(`.api/radix-ui-react-slot.md`): the inherited `asChild` IS the `Slot` merge through `@radix-ui/react-primitive`, clipping an existing `<label>`/`<span>` onto that child rather than nesting a second span.

[LOCAL_ADMISSION]:
- core `ui`/`view` plane only; RAC's own `VisuallyHidden` owns any aria-spine node, and a radix hidden-span there is the double-primitive defect.
- one clip owner per node: the component where it can wrap, `VISUALLY_HIDDEN_STYLES` where it cannot.
