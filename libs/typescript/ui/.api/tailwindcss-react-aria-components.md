# [TS_UI_API_TAILWINDCSS_REACT_ARIA_COMPONENTS]

`tailwindcss-react-aria-components` registers one Tailwind variant per react-aria-components interaction state, so RAC render-prop `className`/`style` functions collapse into static utility strings that flow through the folder's `cn` merge rail. It runs build-time only as an `@plugin` carrying a single `prefix` knob — zero runtime, never imported by a component.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `tailwindcss-react-aria-components`
- package: `tailwindcss-react-aria-components` (Apache-2.0)
- module: CommonJS Tailwind plugin (`module.exports = plugin.withOptions(...)`); `.d.ts` types it `plugin(options?: { prefix?: string }) => { handler }` with `__isOptionsFunction`
- runtime: build-time PostCSS/Tailwind plugin run during CSS compilation; zero runtime, never imported by a component, entirely outside the Effect boundary; `tailwindcss` peer registered CSS-first through `@plugin`
- rail: the `token`/`view` styling plane — turns RAC render-prop state into the class vocabulary that composes through `cn`/`twMerge`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the closed variant vocabulary — one variant per RAC render-prop boolean flag, one `name-value` variant per render-prop enum, each selecting the `data-*` attribute RAC emits; `selection-mode`/`resizable-direction`/`sort-direction` register under the short `selection`/`resizable`/`sort` names.

| [INDEX] | [KIND]            | [SHAPE] | [VARIANTS]                                                                               |
| :-----: | :---------------- | :------ | :--------------------------------------------------------------------------------------- |
|  [01]   | interactive       | boolean | `hover` `focus` `focus-visible` `focus-within` `pressed` `active` `disabled`             |
|  [02]   | selection         | boolean | `selected` `selection-start` `selection-end` `indeterminate`                             |
|  [03]   | overlay/motion    | boolean | `open` `expanded` `entering` `exiting`                                                   |
|  [04]   | field/value       | boolean | `required` `invalid` `unavailable` `read-only` `placeholder-shown` `pending` `empty`     |
|  [05]   | drag/resize       | boolean | `dragging` `drop-target` `resizing` `allows-dragging` `allows-removing` `allows-sorting` |
|  [06]   | structural        | boolean | `has-submenu` `has-child-items` `current` `outside-month` `outside-visible-range`        |
|  [07]   | overlay side      | enum    | `placement-{left,right,top,bottom}`                                                      |
|  [08]   | axis              | enum    | `orientation-{horizontal,vertical}`                                                      |
|  [09]   | selection mode    | enum    | `selection-{single,multiple}`                                                            |
|  [10]   | sort direction    | enum    | `sort-{ascending,descending}`                                                            |
|  [11]   | resize dir        | enum    | `resizable-{right,left,both}`                                                            |
|  [12]   | date segment      | enum    | `type-{literal,year,month,day}`                                                          |
|  [13]   | collection layout | enum    | `layout-{grid,stack}`                                                                    |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: registration and variant use — the whole surface is one `@plugin` directive in the `token` entry CSS, never a JS import.

| [INDEX] | [SURFACE]                                                 | [SHAPE]   | [CAPABILITY]                                               |
| :-----: | :-------------------------------------------------------- | :-------- | :--------------------------------------------------------- |
|  [01]   | `@plugin "tailwindcss-react-aria-components"`             | directive | registers every variant; `{ prefix: rac }` namespaces them |
|  [02]   | `selected:` `pressed:` `placement-bottom:` in `className` | variant   | scopes trailing utilities; stacks with `hover:`/`md:`      |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One `addVariant` fold over a `(data-attribute → selector)` table mints every variant — `<name>:` → `&[data-<attr>]` for a boolean, `<name>-<value>:` → `&[data-<attr>="<value>"]` for an enum — so `className="selected:… pressed:…"` reads state declaratively where a render-prop `className={({isSelected}) => …}` function stood; a new RAC state is a new upstream row, never a hand-written selector.
- Unprefixed, a variant overlapping a native CSS state emits one `&:is(:where([data-rac])[data-hovered], :where(:not([data-rac])):hover)` selector so `hover:`/`focus:`/`disabled:` cover RAC and native elements alike; `hover` wraps `@media (hover: hover)`, `placeholder-shown` merges `[data-placeholder], :placeholder-shown`, and `:where()` holds specificity at (0,1,0) so `group-*`/`peer-*`/`not-*` stay composable.

[STACKING]:
- `react-aria-components` (`.api/react-aria-components.md`): every render-prop boolean (`isHovered`/`isSelected`/`isInvalid`) and enum (`placement`/`orientation`/`selectionMode`) has a matching variant, so a styled row expresses interaction state in classes instead of a render-prop `className`/`style` function.
- `tailwindcss` (`.api/tailwindcss.md`): registered through the `@plugin` directive; the variants compose with Tailwind's own (`selected:hover:…`, `md:pressed:…`) and with `group-*`/`peer-*` over RAC's group/peer data-attributes.
- `tailwind-merge` (`.api/tailwind-merge.md`): the variants are modifiers `twMerge` preserves as conflict scopes, flowing through the one `cn = twMerge(clsx(...))` rail; an order-sensitive interaction registers `orderSensitiveModifiers`.
- `tw-animate-css` (`.api/tw-animate-css.md`): `entering:`/`exiting:` drive RAC transition phases — pairing them with `animate-in`/`animate-out` declares overlay motion in classes off RAC's `data-entering`/`data-exiting`.
- `class-variance-authority` (`.api/class-variance-authority.md`): a `cva` slot's class string carries these state variants (`selected:` inside a variant), binding RAC state declaratively while `cva` selects the design variant.
- `view/primitive` + `view/compose`: the folder styles every RAC state through these variants, so the `Schema`→aria `FormBinding` surfaces `invalid:`/`required:` in classes and no styled row hand-writes a state branch.

[LOCAL_ADMISSION]:
- Register once via `@plugin` in the `token` entry CSS; a component never imports the plugin or lists it in a JS `plugins: []` array, and a `prefix` is chosen only when an unprefixed name genuinely collides.
- Style RAC state with variants (`selected:`/`pressed:`/`invalid:`/`placement-bottom:`) and pair `entering:`/`exiting:` with `tw-animate-css` for overlay transitions, never a render-prop `className`/`style` function or a hand-written enter/exit keyframe machine where a variant exists.

[RAIL_LAW]:
- Package: `tailwindcss-react-aria-components`
- Owns: the build-time mapping of react-aria-components render-prop state to Tailwind variants — boolean state variants, `name-value` enum variants, the `@plugin` registration with its `prefix` option, and the unprefixed native-collapse selector emission
- Accept: CSS-first `@plugin` registration, declarative variant styling of RAC state, `entering:`/`exiting:` + tw-animate transitions, composition through the `cn`/`twMerge` rail and with `cva`/`group-*`/`peer-*`
- Reject: importing the plugin from a component, a JS `plugins: []` registration, render-prop `className`/`style` functions where a variant exists, hand-written enter/exit keyframe machines, a needless `prefix` that breaks native-collapse
