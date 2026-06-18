# [API_CATALOGUE] tailwindcss-react-aria-components

`tailwindcss-react-aria-components` is a Tailwind CSS plugin that generates variant utilities targeting the `data-*` attributes that `react-aria-components` sets on its DOM elements. It produces two selector strategies: enum-attribute variants (e.g. `placement-left:`, `orientation-horizontal:`) matching `[data-rac][data-placement="left"]`, and boolean-attribute variants (e.g. `open:`, `disabled:`, `focus-visible:`) matching `[data-rac][data-open]`. An optional `prefix` option namespaces every generated variant to avoid collisions with native Tailwind variants.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `tailwindcss-react-aria-components`
- package: `tailwindcss-react-aria-components`
- assembly: —
- namespace: `tailwindcss-react-aria-components`
- asset: Tailwind CSS plugin
- rail: styling

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: plugin options
- rail: styling

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]  | [RAIL]                                 |
| :-----: | :------------------- | :------------- | :------------------------------------- |
|   [1]   | `{ prefix: string }` | options object | namespaces all generated variant names |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: plugin factory
- rail: styling

| [INDEX] | [SURFACE]                    | [ENTRY_FAMILY] | [RAIL]                                  |
| :-----: | :--------------------------- | :------------- | :-------------------------------------- |
|   [1]   | `plugin(options?)`           | plugin factory | returns `{ handler: () => void }`       |
|   [2]   | `plugin.__isOptionsFunction` | marker field   | `true` — marks as `withOptions` factory |

## [4]-[IMPLEMENTATION_LAW]

[PLUGIN_TOPOLOGY]:
- exported via `export = plugin` (CommonJS-compatible default export)
- declared as `plugin.withOptions`-style factory; Tailwind resolves `plugin(options)` or `plugin` without invocation
- without `prefix`: RAC element variants use `&:where([data-rac])[data-${attr}]`; native states use `&:where(:not([data-rac])):${nativeSelector}` to prevent cascade conflicts
- with `prefix`: every variant is renamed to `${prefix}${variantName}:` — e.g. `prefix: 'rac-'` gives `rac-open:`, `rac-disabled:`
- enum-attribute variants enumerate all known `data-*` values (e.g. `data-placement="left"`, `data-type="month"`, `data-layout="grid"`)
- boolean-attribute variants enumerate all known boolean flags (`data-open`, `data-disabled`, `data-dragging`, `data-focus-visible`, etc.)
- short-name aliases: `data-selection-mode` → `selection`, `data-resizable-direction` → `resizable`
- hover variants are wrapped in `@media (hover: hover)` to match pointer-device behaviour
- `data-placeholder-shown` applies alongside `:placeholder-shown` pseudo-class for `placeholder` variant

[LOCAL_ADMISSION]:
- add to `plugins` array in `tailwind.config.js` / `tailwind.config.ts` as `require('tailwindcss-react-aria-components')` or `plugin()`
- for scoped usage, pass `{ prefix: 'rac-' }` to isolate variants from Tailwind core and third-party plugin variants
- every variant class pairs with a selector that checks `[data-rac]`; utility classes only activate on `react-aria-components` elements
- enum variants cascade before boolean variants due to author-order registration; this is load-bearing for specificity

[RAIL_LAW]:
- package: `tailwindcss-react-aria-components`
- owns: Tailwind variant generation for `react-aria-components` `data-*` interaction state attributes
- accept: optional `prefix` string to namespace generated variants
- reject: direct CSS selector authoring against `data-rac` / `data-*` attributes, applying these variants to non-RAC elements
