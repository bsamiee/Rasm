# [API_CATALOGUE] tailwindcss-react-aria-components

`tailwindcss-react-aria-components` is a Tailwind CSS v4 plugin (`plugin.withOptions`) that generates interaction-state variant utilities from one data table, targeting the `data-*` attributes `react-aria-components` sets on its DOM elements. It is not a hand-written variant list: two seed tables — `attributes.enum` (attribute → value set) and `attributes.boolean` (flag list with optional variant aliases) — drive one generation loop that registers `addVariant(name, selector)` per row. With no `prefix`, native-overlapping variants collapse to a unified `:is()` selector so one variant name (`hover:`, `disabled:`) works on both RAC and non-RAC elements and `not-*` inverts cleanly; `prefix` namespaces every generated variant.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `tailwindcss-react-aria-components`
- package: `tailwindcss-react-aria-components`
- version: `2.2.0`
- license: `Apache-2.0`
- module: CommonJS `src/index.js` (`module.exports = plugin.withOptions(...)`); types `src/index.d.ts`
- runtime: peer `tailwindcss@^4.0.0` (a v4 plugin; built on `require('tailwindcss/plugin')`)
- admission: v4 `@plugin "tailwindcss-react-aria-components"` CSS directive (v4 does not auto-detect `tailwind.config.js`; a legacy config needs `@config`)
- rail: styling

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: plugin factory (the entire typed surface)
- rail: styling

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]  | [SHAPE]                                                     |
| :-----: | :--------------------------- | :------------- | :--------------------------------------------------------- |
|  [01]   | `plugin(options?)`           | plugin factory | `(options?: Partial<{ prefix: string }>) => { handler: () => void }` |
|  [02]   | `plugin.__isOptionsFunction` | marker field   | `true` — marks the `withOptions` factory for Tailwind      |

## [03]-[VARIANT_TABLE]

[VARIANT_TABLE_SCOPE]: the seed data driving generation
- rail: styling

The plugin iterates `attributes.enum` first (enum values → `${name}-${value}` variants, always RAC-only) then `attributes.boolean` (flags → boolean variants, native-merged where a native selector exists). Enum-before-boolean author order is load-bearing for specificity. Adding a state is one table row, never a new mechanism.

[ENUM_ATTRIBUTES]: variant `${shortName ?? attr}-${value}`, selector `[data-${attr}="${value}"]`

| [INDEX] | [ATTRIBUTE]            | [VARIANT_PREFIX] | [VALUES → VARIANTS]                                                       |
| :-----: | :--------------------- | :--------------- | :------------------------------------------------------------------------ |
|  [01]   | `placement`            | `placement`      | `placement-left`, `placement-right`, `placement-top`, `placement-bottom` |
|  [02]   | `type`                 | `type`           | `type-literal`, `type-year`, `type-month`, `type-day`                     |
|  [03]   | `layout`               | `layout`         | `layout-grid`, `layout-stack`                                            |
|  [04]   | `orientation`          | `orientation`    | `orientation-horizontal`, `orientation-vertical`                        |
|  [05]   | `selection-mode`       | `selection`      | `selection-single`, `selection-multiple`                                |
|  [06]   | `resizable-direction`  | `resizable`      | `resizable-right`, `resizable-left`, `resizable-both`                    |
|  [07]   | `sort-direction`       | `sort`           | `sort-ascending`, `sort-descending`                                     |

[BOOLEAN_ATTRIBUTES]: variant `[0]`, selector on `data-${attr[1] ?? attr}`

| [INDEX] | [GROUP]              | [VARIANTS → DATA ATTRIBUTE]                                                                                              |
| :-----: | :------------------- | :---------------------------------------------------------------------------------------------------------------------- |
|  [01]   | conditions (RAC-only) | `allows-removing`, `allows-sorting`, `allows-dragging`, `has-submenu`, `has-child-items` → `[data-<same>]`             |
|  [02]   | states (RAC-only)    | `entering`, `exiting`, `current`, `unavailable`, `outside-month`, `outside-visible-range`, `pending` → `[data-<same>]` |
|  [03]   | interactive (RAC-only) | `pressed`, `selected`, `selection-start`, `selection-end`, `dragging`, `drop-target`, `resizing` → `[data-<same>]`   |
|  [04]   | native-merged        | `indeterminate`, `required`, `invalid`, `empty`, `focus-visible`, `focus-within`, `disabled`, `active`, `open`, `expanded` → RAC `[data-<name>]` **and** the native `:<name>`/`[<name>]` |
|  [05]   | aliased + native     | `hover` → `[data-hovered]`+`:hover`; `focus` → `[data-focused]`+`:focus`; `read-only` → `[data-readonly]`+`:read-only`  |
|  [06]   | aliased + merged     | `placeholder-shown` → `:is([data-placeholder], :placeholder-shown)` (both apply, no `data-rac` guard)                   |

## [04]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: plugin registration
- rail: styling

| [INDEX] | [SURFACE]                          | [ENTRY_FAMILY] | [BEHAVIOR]                                                              |
| :-----: | :--------------------------------- | :------------- | :---------------------------------------------------------------------- |
|  [01]   | `plugin()` / `plugin({ prefix })`  | plugin factory | `withOptions` factory; Tailwind resolves it invoked or bare            |
|  [02]   | `prefix: "rac"`                    | option         | the plugin appends `-`, yielding `rac-open:`, `rac-selection-single:` — pass `"rac"`, never `"rac-"` |

## [05]-[IMPLEMENTATION_LAW]

[PLUGIN_TOPOLOGY]:
- `module.exports = plugin.withOptions(options => ({ addVariant }) => { ... })`; `prefix = options?.prefix ? `${options.prefix}-` : ""` — the trailing `-` is appended by the plugin.
- enum variants (no native counterpart) resolve to `&[data-${attr}="${value}"]` with no `data-rac` guard, since only RAC sets these attributes.
- with **no** `prefix`, a native-overlapping variant collapses both branches into one selector so `not-*` works and specificity stays `(0,1,0)` via `:where()`: `&:is(:where([data-rac])[data-open], :where(:not([data-rac]))[open])` — RAC elements match the `data-*` branch, native elements the pseudo/attribute branch.
- `hover` additionally wraps in `@media (hover: hover)` (emitted as CSS-in-JS, not `@media { & }`) so touch devices don't get sticky hover and `group-hover:`/`peer-hover:`/`not-hover:` stay composable.
- with a `prefix`, native merging is disabled — every variant is `&[data-${attr}]` (or the enum equivalent) namespaced under the prefix, so prefixed variants never touch non-RAC elements.
- RAC-only boolean variants (`pressed`, `dragging`, `entering`, …) always resolve to `&[data-${attr}]`.

[STACKING]:
- `react-aria-components`: every RAC component places `data-rac` plus its interaction `data-*` attributes on the root DOM element; these variants are the styling counterpart to that attribute contract, so a `ButtonRenderProps`/`ListBoxItemRenderProps` state is stylable as a Tailwind variant instead of a render-prop `className` function.
- `tailwind-merge` + `class-variance-authority`: a generated variant is an ordinary class modifier (`selected:bg-accent`, `disabled:opacity-50`), so `cva` composes it into a variant row and the `cn = twMerge(cx(...))` recipe (`interaction/command.md`) conflict-resolves the base class inside it — the variant names need no `orderSensitiveModifiers` entry unless a specificity conflict demands one.
- `theming/tokens.md`: this plugin owns RAC `data-*` interaction variants; `tw-animate-css` owns the Radix-style `data-[state=open|closed]` enter/exit layer — the two are distinct substrates registered at the same v4 stylesheet root, and the RAC `entering`/`exiting` variants are the RAC-native analog of the `data-[state]` animation triggers.

[LOCAL_ADMISSION]:
- load via the v4 `@plugin "tailwindcss-react-aria-components"` directive at the CSS entry (after `@import "tailwindcss"`); pass `{ prefix: "rac" }` through the `withOptions` factory (a v4 `@plugin` options block, or a `@config`-referenced JS config `plugins: [plugin({ prefix: "rac" })]`) to isolate variants from Tailwind core and other plugin variants.
- every generated variant activates only on the RAC `data-*` attribute (or, unprefixed, its native fallback); apply them to RAC-emitted elements, not arbitrary markup.
- author-order registration (enum before boolean) is load-bearing for cascade specificity — do not reorder.

[RAIL_LAW]:
- package: `tailwindcss-react-aria-components`
- owns: Tailwind v4 variant generation for the `react-aria-components` `data-*` interaction-state attribute contract
- accept: optional `{ prefix: string }` (bare name, no trailing dash) to namespace generated variants
- reject: direct CSS selector authoring against `data-rac`/`data-*`; applying variants to non-RAC elements; a v3 `tailwind.config.js plugins`-array admission (v4 uses `@plugin`); passing `prefix: "rac-"` (double-dashes the variant)
