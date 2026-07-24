# [TS_UI_API_RADIX_UI_REACT_SEPARATOR]

`@radix-ui/react-separator` owns the composition-plane divider primitive: one `forwardRef` over `Primitive.div` folding `(decorative, orientation)` into a single ARIA role projection and emitting `data-orientation` unconditionally as the token-plane styling hook. This row owns the role projection and the `asChild` merge inherited whole from `@radix-ui/react-slot`, and the token plane owns the pixel line. Core `ui` browser plane, never `scope:viewer`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@radix-ui/react-separator`
- package: `@radix-ui/react-separator` (MIT)
- module: single `.` barrel, `sideEffects:false`; one component under `Separator`/`Root` and its `SeparatorProps` type
- runtime: `runtime:browser`, core `ui` composition plane
- depends: `@radix-ui/react-primitive` → `@radix-ui/react-slot` (`.api/radix-ui-react-slot.md`) — the `asChild` merge source
- rail: `view/compose` divider primitive

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: divider component and its prop contract

| [INDEX] | [SYMBOL]                                   | [TYPE_FAMILY]       | [CAPABILITY]                                                        |
| :-----: | :----------------------------------------- | :------------------ | :------------------------------------------------------------------ |
|  [01]   | `Separator` / `Root`                       | primitive component | `view/compose` divider; ref forwards to `<div>`                     |
|  [02]   | `SeparatorProps`                           | prop contract       | native `<div>` attrs + `orientation` + `decorative` + `asChild`     |
|  [03]   | `orientation?: "horizontal" \| "vertical"` | closed axis         | drives `aria-orientation` (vertical) + always-on `data-orientation` |
|  [04]   | `decorative?: boolean`                     | a11y gate           | `true` → `role="none"` (off a11y tree); else `role="separator"`     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: render the divider under the `(decorative × orientation)` projection

| [INDEX] | [SURFACE]                                | [SHAPE]        | [CAPABILITY]                                       |
| :-----: | :--------------------------------------- | :------------- | :------------------------------------------------- |
|  [01]   | `<Separator orientation={o} />`          | semantic       | `role="separator"`; `aria-orientation` on vertical |
|  [02]   | `<Separator decorative />`               | presentational | `role="none"`; off a11y tree                       |
|  [03]   | `<Separator asChild>{child}</Separator>` | slot-merge     | render-as-child via `createSlot('Primitive.div')`  |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `Separator === Root` is one component under two names; a `HorizontalSeparator`/`VerticalSeparator` or decorative-vs-semantic split is the named defect — `orientation` and `decorative` are policy knobs of one primitive.
- `(decorative, orientation)` maps totally to `{ role, aria-orientation }`: `decorative` gates semantics-vs-presentation, `orientation` gates `aria-orientation` emitted only for vertical (ARIA defaults separators to horizontal); an out-of-range `orientation` coerces to horizontal via the internal `isValidOrientation` guard, and `data-orientation={orientation}` emits unconditionally as the styling hook.
- `asChild` is inherited: `Primitive.div` is `asChild ? createSlot('Primitive.div') : 'div'`, so the separator never re-implements slot merging, and `ORIENTATIONS`/`Orientation` stay module-internal.

[STACKING]:
- `class-variance-authority`+`clsx`+`tailwind-merge` (`.api/class-variance-authority.md`, `.api/clsx.md`, `.api/tailwind-merge.md`): the line variant keys off the emitted `data-orientation` — `[data-orientation=horizontal]` → `h-px w-full`, `[data-orientation=vertical]` → `w-px h-full` — so the primitive emits the role and `data-orientation` while the token plane emits the pixel.
- `@radix-ui/react-slot` (`.api/radix-ui-react-slot.md`): `asChild` routes through `createSlot('Primitive.div')`, inheriting the whole Slot merge algorithm that catalog owns; spread order `{ data-orientation, ...semanticProps, ...domProps }` lets caller `domProps` override the computed `role`/`aria-orientation`/`data-orientation`, then `Slot` merges the one element child over that result, letting an `asChild` child further override the role.
- `react-aria-components` (`.api/react-aria-components.md`): RAC ships its own `Separator` over `useSeparator`, reading `SeparatorContext`; inside an RAC grouped region (`Toolbar`/`Menu`/`Group`/`ListBox`) the RAC `Separator` owns the divider and a radix `Separator` there is the double-primitive defect. Radix `Separator` serves the non-aria styling plane — a standalone semantic divider, or `decorative` for a visual-only rule inside an already-grouped region.

[LOCAL_ADMISSION]:
- core `ui` composition plane only, `scope:viewer` never imports it; a new line style is a token-plane variant off `data-orientation` and a new render target is `asChild`, and code depends on the `orientation` prop, never the internal `ORIENTATIONS` tuple.

[RAIL_LAW]:
- Package: `@radix-ui/react-separator`
- Owns: the composition-plane divider primitive, the `(decorative × orientation) → role` a11y projection, and the always-emitted `data-orientation` token hook
- Accept: `Separator`/`Root` as the one divider, `orientation` as the axis knob, `decorative` as the a11y gate, `asChild` inheriting the Slot merge through `Primitive.div` → `createSlot('Primitive.div')`, a `data-orientation`-keyed token class for the line
- Reject: per-orientation or decorative-vs-semantic component variants, importing the internal `ORIENTATIONS` tuple, a radix `Separator` inside an RAC grouped region that owns the divider via `SeparatorContext`, separator-local re-implementation of slot merging or line styling
